using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AssetTagPrinter.Icons;

namespace AssetTagPrinter
{
    public partial class MainForm : Form
    {
        private const string BlankWarehouseOption = "(Blank Warehouse)";
        private const int ItemsPerPage = 15;
        private const string GRID_LOCK_TITLE_SUFFIX = " [GRID MODE]";

        private PrinterService? _printerService;
        private CsvService _csvService;
        private bool _isPrinting;
        private bool _isGridFocusLocked; // Lock navigation to data grid
        private Control? _previousFocusedControl; // Track control that was focused before entering grid mode
        private string _originalTitle = ""; // Store original form title
        private List<Asset> _loadedAssets = new List<Asset>();
        private List<Asset> _filteredAssets = new List<Asset>();
        private PrintStyleSettings _printStyleSettings = PrintStyleSettings.CreateDefault();
        private int _currentPage = 1;
        private Icon? _titleBarIcon;
        private Icon? _taskbarIcon;

        // Preview state
        private List<Asset> _previewAssets = new List<Asset>();
        private int _currentPreviewIndex = 0;

        // Timer for polling printer status in the background
        private System.Windows.Forms.Timer? _printerStatusTimer;

        // File index map in IconManager:
        // 0=16x16, 1=24x24, 2=32x32, 3=48x48, 4=256x256
        private const int TitleBarIconIndex = 3;
        private const int TaskbarIconIndex = 4;

        private const int WM_SETICON = 0x0080;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
        private const int GCL_HICON = -14;
        private const int GCL_HICONSM = -34;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetClassLong", SetLastError = true)]
        private static extern uint SetClassLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr", SetLastError = true)]
        private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public MainForm()
        {
            InitializeComponent();

            _titleBarIcon = IconManager.LoadTitleBarIcon(TitleBarIconIndex);
            _taskbarIcon = IconManager.LoadTaskbarIcon(TaskbarIconIndex);

            _csvService = new CsvService();
            _originalTitle = Text;

            // Configure data grid
            dataGridViewAssets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAssets.MultiSelect = true;
            dataGridViewAssets.ReadOnly = true;
            dataGridViewAssets.AllowUserToAddRows = false;
            dataGridViewAssets.AllowUserToDeleteRows = false;
            dataGridViewAssets.AllowUserToOrderColumns = false;

            // Attach event handlers
            dataGridViewAssets.CellClick += DataGridViewAssets_CellClick;
            dataGridViewAssets.PreviewKeyDown += DataGridView_PreviewKeyDown;
            dataGridViewAssets.KeyDown += DataGridView_KeyDown;
            dataGridViewAssets.SelectionChanged += DataGridView_SelectionChanged;

            // Pagination Button Click handlers were called along with the designer file's Click Handlers of the same name
            // Note: pagination button Click handlers are wired in the Designer file        

            // Initialize UI state
            cmbCategory.SelectedIndex = 0;
            KeyPreview = true;
            StartPosition = FormStartPosition.CenterScreen;
            UpdateButtonStates();

            // Start background printer status polling (non-blocking)
            _printerStatusTimer = new System.Windows.Forms.Timer();
            _printerStatusTimer.Interval = 5000; // poll every 5 seconds
            _printerStatusTimer.Tick += (s, e) =>
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    bool ready = PrinterService.TryGetPrinterStatus(out string status);
                    try
                    {
                        if (!IsDisposed)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                lblPrinterStatus.Text = $"Printer: {status}";
                                lblPrinterStatus.ForeColor = ready ? Color.DarkGreen : Color.DarkRed;
                            }));
                        }
                    }
                    catch
                    {
                        // ignore invoke errors during shutdown
                    }
                });
            };
            _printerStatusTimer.Start();

            this.FormClosing += MainForm_FormClosing;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyWindowIcons();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

        private void ApplyWindowIcons()
        {
            ShowIcon = true;

            if (_titleBarIcon != null)
            {
                Icon = (Icon)_titleBarIcon.Clone();
            }
            else if (_taskbarIcon != null)
            {
                Icon = (Icon)_taskbarIcon.Clone();
            }

            IntPtr smallHandle = _titleBarIcon != null ? _titleBarIcon.Handle : (Icon != null ? Icon.Handle : IntPtr.Zero);
            IntPtr bigHandle = _taskbarIcon != null ? _taskbarIcon.Handle : smallHandle;

            SendMessage(Handle, WM_SETICON, (IntPtr)ICON_SMALL, smallHandle);
            SendMessage(Handle, WM_SETICON, (IntPtr)ICON_BIG, bigHandle);

            // Set class icons as well so Windows shell picks up both title bar and taskbar icons.
            SetClassIcon(Handle, GCL_HICONSM, smallHandle);
            SetClassIcon(Handle, GCL_HICON, bigHandle);
        }

        private static void SetClassIcon(IntPtr hWnd, int index, IntPtr iconHandle)
        {
            if (IntPtr.Size == 8)
            {
                SetClassLongPtr64(hWnd, index, iconHandle);
            }
            else
            {
                SetClassLong32(hWnd, index, (uint)iconHandle.ToInt32());
            }
        }

        /// <summary>
        /// Toggle grid lock mode on/off with proper state management and visual feedback
        /// </summary>
        private void ToggleGridLockMode()
        {
            if (_isGridFocusLocked)
            {
                // Exit lock mode
                ExitGridLockMode();
            }
            else
            {
                // Enter lock mode
                EnterGridLockMode();
            }
        }

        /// <summary>
        /// Enter grid lock mode - focus grid and disable other navigation
        /// </summary>
        private void EnterGridLockMode()
        {
            if (dataGridViewAssets != null && dataGridViewAssets.CanFocus && dataGridViewAssets.Rows.Count > 0)
            {
                // Save the control that currently has focus so we can restore it later
                _previousFocusedControl = ActiveControl;

                _isGridFocusLocked = true;
                dataGridViewAssets.Focus();

                // Select first cell if no selection exists
                if (dataGridViewAssets.CurrentCell == null)
                {
                    dataGridViewAssets.CurrentCell = dataGridViewAssets.Rows[0].Cells[0];
                }

                // Visual feedback: update title to show grid mode is active
                Text = _originalTitle + GRID_LOCK_TITLE_SUFFIX;
            }
        }

        /// <summary>
        /// Exit grid lock mode - return focus to main window
        /// </summary>
        private void ExitGridLockMode()
        {
            _isGridFocusLocked = false;

            // Restore original title
            Text = _originalTitle;

            // Try to return to the previous control if it's enabled, otherwise focus a default control
            if (_previousFocusedControl != null && _previousFocusedControl.CanFocus && _previousFocusedControl.Enabled)
            {
                _previousFocusedControl.Focus();
            }
            else
            {
                // Fallback: focus Load CSV button
                if (btnLoadCsv != null && btnLoadCsv.CanFocus)
                {
                    btnLoadCsv.Focus();
                }
            }
        }



        /// <summary>
        /// Update button states based on asset data and selection status
        /// </summary>
        private void UpdateButtonStates()
        {
            bool hasAssets = dataGridViewAssets.Rows.Count > 0;
            bool hasSelection = dataGridViewAssets.SelectedRows.Count > 0;

            // Print requires assets and selection
            btnPrint.Enabled = hasAssets && hasSelection && !_isPrinting;
            btnPrintStyle.Enabled = hasAssets;

            // Pagination buttons
            btnPreviousPage.Enabled = hasAssets && _currentPage > 1;
            btnNextPage.Enabled = hasAssets && _currentPage < (int)Math.Ceiling((double)_loadedAssets.Count / ItemsPerPage);
        }

        /// <summary>
        /// Handle grid selection changes to update button states
        /// </summary>
        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
            RefreshPreviewForSelection();
        }

        private void DataGridView_KeyDown(object? sender, KeyEventArgs e)
        {
            // Exit grid lock mode when Ctrl+Tab is pressed
            if (e.Control && e.KeyCode == Keys.Tab && _isGridFocusLocked)
            {
                e.Handled = true;
                ExitGridLockMode();
            }

            // Handle multi-select shortcuts in grid lock mode
            if (!_isGridFocusLocked || dataGridViewAssets == null || dataGridViewAssets.Rows.Count == 0)
            {
                return;
            }

            // Ctrl+A: Select all rows
            if (e.Control && e.KeyCode == Keys.A)
            {
                dataGridViewAssets.SelectAll();
                // Ensure CurrentCell is set so Space works immediately after
                if (dataGridViewAssets.CurrentCell == null && dataGridViewAssets.Rows.Count > 0)
                {
                    dataGridViewAssets.CurrentCell = dataGridViewAssets.Rows[0].Cells[0];
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            // Space: Toggle selection of current row
            if (e.KeyCode == Keys.Space)
            {
                if (dataGridViewAssets.CurrentCell == null && dataGridViewAssets.Rows.Count > 0)
                {
                    dataGridViewAssets.CurrentCell = dataGridViewAssets.Rows[0].Cells[0];
                }

                if (dataGridViewAssets.CurrentCell != null)
                {
                    int rowIndex = dataGridViewAssets.CurrentCell.RowIndex;

                    // Store all currently selected rows
                    List<int> selectedIndices = new List<int>();
                    foreach (DataGridViewRow row in dataGridViewAssets.SelectedRows)
                    {
                        selectedIndices.Add(row.Index);
                    }

                    // Toggle the current row
                    if (selectedIndices.Contains(rowIndex))
                    {
                        selectedIndices.Remove(rowIndex);
                    }
                    else
                    {
                        selectedIndices.Add(rowIndex);
                    }

                    // Apply selections after the grid finishes processing
                    this.BeginInvoke(new Action(() =>
                    {
                        dataGridViewAssets.ClearSelection();
                        foreach (int idx in selectedIndices)
                        {
                            if (idx >= 0 && idx < dataGridViewAssets.Rows.Count)
                            {
                                dataGridViewAssets.Rows[idx].Selected = true;
                            }
                        }
                    }));

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            // Shift+A: Deselect all rows
            if (e.Shift && e.KeyCode == Keys.A)
            {
                dataGridViewAssets.ClearSelection();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            // Arrow keys: Navigate without auto-selecting (preserves selections)
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                // Store the current selection
                List<int> selectedRowIndices = new List<int>();
                foreach (DataGridViewRow row in dataGridViewAssets.SelectedRows)
                {
                    selectedRowIndices.Add(row.Index);
                }

                // Manually navigate without letting grid auto-select
                int currentRow = dataGridViewAssets.CurrentCell?.RowIndex ?? 0;
                int currentCol = dataGridViewAssets.CurrentCell?.ColumnIndex ?? 0;

                int newRow = currentRow;
                int newCol = currentCol;

                if (e.KeyCode == Keys.Up && currentRow > 0) newRow--;
                if (e.KeyCode == Keys.Down && currentRow < dataGridViewAssets.Rows.Count - 1) newRow++;
                if (e.KeyCode == Keys.Left && currentCol > 0) newCol--;
                if (e.KeyCode == Keys.Right && currentCol < dataGridViewAssets.Columns.Count - 1) newCol++;

                // Set new current cell (this navigates without auto-selecting)
                if (newRow >= 0 && newRow < dataGridViewAssets.Rows.Count &&
                    newCol >= 0 && newCol < dataGridViewAssets.Columns.Count)
                {
                    dataGridViewAssets.CurrentCell = dataGridViewAssets.Rows[newRow].Cells[newCol];
                }

                // Re-apply selections after navigation
                this.BeginInvoke(new Action(() =>
                {
                    dataGridViewAssets.ClearSelection();
                    foreach (int rowIndex in selectedRowIndices)
                    {
                        if (rowIndex >= 0 && rowIndex < dataGridViewAssets.Rows.Count)
                        {
                            dataGridViewAssets.Rows[rowIndex].Selected = true;
                        }
                    }
                }));

                e.Handled = true;
            }
        }

        private void DataGridView_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            // When in grid lock mode, mark Space and selection shortcuts as input keys to prevent default behavior
            if (_isGridFocusLocked)
            {
                // Space key for toggle selection
                if (e.KeyCode == Keys.Space)
                {
                    e.IsInputKey = true;
                }
                // Ctrl+A for select all
                if (e.Control && e.KeyCode == Keys.A)
                {
                    e.IsInputKey = true;
                }
                // Shift+A for deselect all
                if (e.Shift && e.KeyCode == Keys.A)
                {
                    e.IsInputKey = true;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Button shortcuts: Ctrl+O (Load), Ctrl+P (Print), Ctrl+Shift+P (Preview), Ctrl+D (Diagnostics), F1 (Help)
            if (keyData == (Keys.Control | Keys.O))
            {
                btnLoadCsv?.PerformClick();
                return true;
            }

            if (keyData == (Keys.Control | Keys.P))
            {
                btnPrint?.PerformClick();
                return true;
            }

            if (keyData == (Keys.Control | Keys.D))
            {
                btnDiagnostics?.PerformClick();
                return true;
            }

            if (keyData == Keys.F1)
            {
                btnHelp?.PerformClick();
                return true;
            }

            // Dropdown focus shortcuts: Ctrl+W (Category), Ctrl+E (Filter)
            if (keyData == (Keys.Control | Keys.W))
            {
                if (cmbCategory != null && cmbCategory.CanFocus)
                {
                    cmbCategory.Focus();
                    cmbCategory.DroppedDown = true;
                }
                return true;
            }

            if (keyData == (Keys.Control | Keys.E))
            {
                if (cmbFilterValue != null && cmbFilterValue.Visible && cmbFilterValue.CanFocus)
                {
                    cmbFilterValue.Focus();
                    cmbFilterValue.DroppedDown = true;
                }
                return true;
            }

            // Tab navigation: Handle grid lock and dropdown navigation
            if (keyData == Keys.Tab)
            {
                if (_isGridFocusLocked && dataGridViewAssets != null && dataGridViewAssets.Focused)
                {
                    return false;
                }

                if (!_isGridFocusLocked && cmbCategory != null && cmbCategory.Focused &&
                    cmbFilterValue != null && cmbFilterValue.Visible)
                {
                    cmbFilterValue.Focus();
                    cmbFilterValue.DroppedDown = true;
                    return true;
                }
            }

            if (keyData == (Keys.Shift | Keys.Tab))
            {
                if (_isGridFocusLocked && dataGridViewAssets != null && dataGridViewAssets.Focused)
                {
                    return false;
                }
            }

            // Space/Enter: Dropdown interaction
            if (keyData == Keys.Space)
            {
                if (cmbCategory != null && cmbCategory.Focused && !cmbCategory.DroppedDown)
                {
                    cmbCategory.DroppedDown = true;
                    return true;
                }
                if (cmbFilterValue != null && cmbFilterValue.Focused && !cmbFilterValue.DroppedDown)
                {
                    cmbFilterValue.DroppedDown = true;
                    return true;
                }
            }

            if (keyData == Keys.Enter)
            {
                if (cmbCategory != null && (cmbCategory.Focused || cmbCategory.DroppedDown))
                {
                    if (cmbCategory.DroppedDown)
                    {
                        cmbCategory.DroppedDown = false;
                    }

                    ApplyFilters();
                    return true;
                }
                if (cmbFilterValue != null && (cmbFilterValue.Focused || cmbFilterValue.DroppedDown))
                {
                    if (cmbFilterValue.DroppedDown)
                    {
                        cmbFilterValue.DroppedDown = false;
                    }

                    ApplyFilters();
                    return true;
                }
            }

            // Escape: Close dropdowns or exit grid lock mode
            if (keyData == Keys.Escape)
            {
                if (_isGridFocusLocked)
                {
                    ExitGridLockMode();
                    return true;
                }
                if (cmbCategory != null && cmbCategory.DroppedDown)
                {
                    cmbCategory.DroppedDown = false;
                    return true;
                }
                if (cmbFilterValue != null && cmbFilterValue.DroppedDown)
                {
                    cmbFilterValue.DroppedDown = false;
                    return true;
                }
            }

            // Grid lock toggle: Ctrl+Tab enters/exits grid focus isolation
            if (keyData == (Keys.Control | Keys.Tab))
            {
                ToggleGridLockMode();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshPrinterStatus();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                if (_printerStatusTimer != null)
                {
                    _printerStatusTimer.Stop();
                    _printerStatusTimer.Dispose();
                    _printerStatusTimer = null;
                }
            }
            catch { }
        }

        private void RefreshPrinterStatus()
        {
            bool ready = PrinterService.TryGetPrinterStatus(out string status);

            lblPrinterStatus.Text = $"Printer: {status}";
            lblPrinterStatus.ForeColor = ready ? Color.DarkGreen : Color.DarkRed;
            btnPrint.Enabled = !_isPrinting;
        }

        private void btnLoadCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var count = LoadAssetsIntoGrid(openFileDialog.FileName);
                        MessageBox.Show($"Successfully loaded {count} assets.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading assets: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DataGridViewAssets_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Preview is now updated automatically via SelectionChanged event
        }

        private int LoadAssetsIntoGrid(string csvPath)
        {
            // Validate file exists
            if (!File.Exists(csvPath))
            {
                throw new FileNotFoundException("CSV file not found.");
            }

            // Validate file is not empty
            var fileInfo = new FileInfo(csvPath);
            if (fileInfo.Length == 0)
            {
                throw new InvalidOperationException("CSV file is empty.");
            }

            var assets = _csvService.ReadAssets(csvPath).ToList();

            // Validate we loaded assets
            if (assets.Count == 0)
            {
                throw new InvalidOperationException("No valid assets found in CSV file.");
            }

            _loadedAssets = assets;
            int result = ApplyFilters();
            UpdateButtonStates(); // Update button states after loading
            return result;
        }

        private void PopulateFilterValueDropdown()
        {
            string selectedCategory = cmbCategory.SelectedItem as string ?? "None";

            if (selectedCategory == "None")
            {
                cmbFilterValue.Visible = false;
                lblFilterValue.Visible = false;
                return;
            }

            cmbFilterValue.Visible = true;
            lblFilterValue.Visible = true;

            string currentSelection = cmbFilterValue.SelectedItem as string ?? "All";
            cmbFilterValue.BeginUpdate();
            try
            {
                cmbFilterValue.Items.Clear();

                if (selectedCategory == "Warehouse")
                {
                    lblFilterValue.Text = "Warehouse:";
                    bool hasBlankWarehouse = _loadedAssets.Any(a => string.IsNullOrWhiteSpace(a.Warehouse));
                    var warehouses = _loadedAssets
                        .Select(a => (a.Warehouse ?? string.Empty).Trim())
                        .Where(w => !string.IsNullOrWhiteSpace(w))
                        .Distinct(System.StringComparer.OrdinalIgnoreCase)
                        .OrderBy(w => w)
                        .ToList();

                    cmbFilterValue.Items.Add("All");
                    if (hasBlankWarehouse)
                    {
                        cmbFilterValue.Items.Add(BlankWarehouseOption);
                    }
                    foreach (var w in warehouses)
                    {
                        cmbFilterValue.Items.Add(w);
                    }
                }
                else if (selectedCategory == "Acquisition Date")
                {
                    lblFilterValue.Text = "Year:";
                    var years = _loadedAssets
                        .Where(a => !string.IsNullOrWhiteSpace(a.AcquisitionDate))
                        .Select(a => ExtractYearFromAcquisitionDate(a.AcquisitionDate))
                        .Where(y => y > 0)
                        .Distinct()
                        .OrderByDescending(y => y)
                        .ToList();

                    cmbFilterValue.Items.Add("All");
                    foreach (var year in years)
                    {
                        cmbFilterValue.Items.Add(year.ToString());
                    }
                }

                int idx = cmbFilterValue.Items.IndexOf(currentSelection);
                cmbFilterValue.SelectedIndex = idx >= 0 ? idx : 0;
            }
            finally
            {
                cmbFilterValue.EndUpdate();
            }
        }

        private int ApplyFilters()
        {
            IEnumerable<Asset> view = _loadedAssets;

            // Apply category-based filter
            string selectedCategory = cmbCategory.SelectedItem as string ?? "None";
            string selectedFilter = cmbFilterValue.SelectedItem as string ?? "All";

            if (selectedCategory == "Warehouse" && !string.Equals(selectedFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(selectedFilter, BlankWarehouseOption, StringComparison.Ordinal))
                {
                    view = view.Where(a => string.IsNullOrWhiteSpace(a.Warehouse));
                }
                else
                {
                    view = view.Where(a => string.Equals((a.Warehouse ?? string.Empty).Trim(), selectedFilter, StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (selectedCategory == "Acquisition Date" && !string.Equals(selectedFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(selectedFilter, out var selectedYear))
                {
                    view = view.Where(a => ExtractYearFromAcquisitionDate(a.AcquisitionDate) == selectedYear);
                }
            }

            // Store the filtered results for pagination
            _filteredAssets = view.ToList();
            _currentPage = 1; // Reset to first page when filters change
            DisplayCurrentPage();

            return _filteredAssets.Count;
        }

        /// <summary>
        /// Calculate total number of pages based on filtered assets
        /// </summary>
        private int CalculateTotalPages()
        {
            int totalPages = (_filteredAssets.Count + ItemsPerPage - 1) / ItemsPerPage; // Ceiling division
            if (totalPages == 0) totalPages = 1;
            return totalPages;
        }

        private void DisplayCurrentPage()
        {
            // Calculate pagination
            int totalPages = CalculateTotalPages();
            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            int startIndex = (_currentPage - 1) * ItemsPerPage;
            int endIndex = Math.Min(startIndex + ItemsPerPage, _filteredAssets.Count);

            var pageList = _filteredAssets.Skip(startIndex).Take(ItemsPerPage).ToList();

            // Update grid
            dataGridViewAssets.DataSource = null;
            dataGridViewAssets.DataSource = new BindingSource { DataSource = pageList };

            // Configure grid columns to show AcquisitionDateDisplay instead of AcquisitionDate
            ConfigureGridColumns();

            // Update pagination display
            lblPageInfo.Text = $"Page {_currentPage} of {totalPages}";

            // Update navigation button states
            btnPreviousPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < totalPages;

            if (pageList.Count > 0)
            {
                dataGridViewAssets.ClearSelection();
                dataGridViewAssets.Rows[0].Selected = true;
                // Selection change will trigger RefreshPreviewForSelection
            }
            else
            {
                RefreshPreviewForSelection();
            }

            // Update print/preview button states
            UpdateButtonStates();
        }

        private void ConfigureGridColumns()
        {
            // Hide the raw AcquisitionDate column and ensure AcquisitionDateDisplay is visible
            foreach (DataGridViewColumn col in dataGridViewAssets.Columns)
            {
                if (col.DataPropertyName == "AcquisitionDate")
                {
                    col.Visible = false;
                }
                else if (col.DataPropertyName == "AcquisitionDateDisplay")
                {
                    col.Visible = true;
                    col.HeaderText = "Acq. Date";
                }
            }
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateFilterValueDropdown();
            ApplyFilters();
        }

        private void cmbFilterValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void btnPreviousPage_Click(object? sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DisplayCurrentPage();
            }
        }

        private void btnNextPage_Click(object? sender, EventArgs e)
        {
            int totalPages = CalculateTotalPages();
            if (_currentPage < totalPages)
            {
                _currentPage++;
                DisplayCurrentPage();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_isPrinting)
            {
                return;
            }

            var assetsToPrint = GetAssetsToPrint();
            if (assetsToPrint.Count == 0)
            {
                MessageBox.Show("Please select at least one asset to print. Use Space in grid mode to select.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isPrinting = true;
            UpdateButtonStates();
            RefreshPrinterStatus();

            try
            {
                _printerService ??= new PrinterService();
                _printerService.StyleSettings = _printStyleSettings.Clone();
                _printerService.Open();

                for (int i = 0; i < assetsToPrint.Count; i++)
                {
                    var asset = assetsToPrint[i];
                    _printerService.PrintAssetTag(asset);
                    _printerService.CutBetweenTags();
                    Thread.Sleep(200);
                }

                MessageBox.Show($"Successfully printed {assetsToPrint.Count} asset(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _printerService?.Close();
                _isPrinting = false;
                UpdateButtonStates();
                RefreshPrinterStatus();
            }
        }

        private List<Asset> GetAssetsToPrint()
        {
            var selectedAssets = dataGridViewAssets.SelectedRows
                .Cast<DataGridViewRow>()
                .OrderBy(r => r.Index)
                .Select(row => row.DataBoundItem as Asset)
                .Where(asset => asset != null)
                .Cast<Asset>()
                .ToList();

            if (selectedAssets.Count > 0)
            {
                return selectedAssets;
            }

            return dataGridViewAssets.Rows
                .Cast<DataGridViewRow>()
                .Select(row => row.DataBoundItem as Asset)
                .Where(asset => asset != null)
                .Cast<Asset>()
                .ToList();
        }

        private void btnDiagnostics_Click(object sender, EventArgs e)
        {
            string report = PrinterService.GetPrinterDiagnosticsReport();
            MessageBox.Show(report, "Printer Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            using (var help = new HelpUpdatesForm())
            {
                help.ShowDialog(this);
            }
        }

        // Preview functionality
        private void RefreshPreviewForSelection()
        {
            try
            {
                var selectedAssets = GetAssetsToPrint();

                if (selectedAssets.Count == 0)
                {
                    _previewAssets.Clear();
                    _currentPreviewIndex = 0;
                    picBoxTagPreview.Image = null;
                    lblPreviewStatus.Text = "No selection";
                    btnPreviewPrevious.Enabled = false;
                    btnPreviewNext.Enabled = false;
                    return;
                }

                _previewAssets = selectedAssets;
                _currentPreviewIndex = 0;
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating preview: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdatePreview()
        {
            if (_previewAssets.Count == 0)
            {
                picBoxTagPreview.Image = null;
                lblPreviewStatus.Text = "No selection";
                btnPreviewPrevious.Enabled = false;
                btnPreviewNext.Enabled = false;
                return;
            }

            // Clamp index to valid range
            if (_currentPreviewIndex >= _previewAssets.Count)
            {
                _currentPreviewIndex = _previewAssets.Count - 1;
            }
            if (_currentPreviewIndex < 0)
            {
                _currentPreviewIndex = 0;
            }

            var asset = _previewAssets[_currentPreviewIndex];

            try
            {
                Bitmap previewBitmap = new Bitmap(280, 400);
                using (Graphics g = Graphics.FromImage(previewBitmap))
                {
                    g.Clear(Color.White);
                    g.DrawRectangle(Pens.Black, 0, 0, 279, 399);

                    Brush blackBrush = Brushes.Black;
                    var lines = TagLayoutFormatter.BuildPosReceiptLines(asset);
                    using Font headerFont = _printStyleSettings.Header.CreateFont();
                    using Font secondaryFont = _printStyleSettings.Secondary.CreateFont();
                    using Font bodyFont = _printStyleSettings.Body.CreateFont();

                    float yPos = _printStyleSettings.TopMargin;
                    float contentWidth = Math.Max(120f, previewBitmap.Width - (_printStyleSettings.LeftMargin * 2));

                    // Draw logo at the top
                    yPos = DrawLogoInPreview(g, _printStyleSettings, _printStyleSettings.LeftMargin, contentWidth, yPos);

                    int barcodeWidth = (int)Math.Min(260f, Math.Max(160f, contentWidth - 10f));
                    // Scale QR down to save sticker space (about 50% of computed barcode width)
                    const int minQrSize = 48;
                    int qrSize = Math.Max(minQrSize, (int)(barcodeWidth * 0.5f));
                    const float dateGap = 8f;
                    const float minDatePanelWidth = 72f;
                    int maxQrForSidePanel = (int)Math.Floor(Math.Max((float)minQrSize, contentWidth - minDatePanelWidth - dateGap));
                    qrSize = Math.Max(minQrSize, Math.Min(qrSize, maxQrForSidePanel));
                    string acqDateValue = GetAcquisitionDateValue(asset);

                    using (Bitmap? barcode = BarcodeRenderer.CreateQrBitmap(asset.Barcode, qrSize))
                    {
                        if (barcode != null)
                        {
                            float contentLeft = _printStyleSettings.LeftMargin;
                            float contentRight = contentLeft + contentWidth;
                            float barcodeX = contentLeft;
                            float drawY = Math.Max(0f, yPos - 2f);
                            float datePanelX = barcodeX + barcode.Width + dateGap;
                            float datePanelWidth = Math.Max(0f, contentRight - datePanelX);

                            g.DrawImage(barcode, barcodeX, drawY, barcode.Width, barcode.Height);
                            float qrBottom = drawY + barcode.Height;
                            float dateBottom = qrBottom;

                            string dateValueText = string.IsNullOrWhiteSpace(acqDateValue) ? "-" : acqDateValue;
                            const string acqLabelText = "Acq Date:";
                            using Font acqLabelBaseFont = new Font(secondaryFont.FontFamily, Math.Max(8f, secondaryFont.Size), FontStyle.Bold);
                            using Font acqValueBaseFont = new Font(bodyFont.FontFamily, Math.Max(10f, bodyFont.Size * 1.25f), FontStyle.Bold);
                            float acqLabelSize = GetBestFitSize(g, acqLabelText, acqLabelBaseFont, Math.Max(24f, datePanelWidth), 6f);
                            float acqValueSize = GetBestFitSize(g, dateValueText, acqValueBaseFont, Math.Max(24f, datePanelWidth), 7f);
                            using Font acqLabelFont = new Font(acqLabelBaseFont.FontFamily, acqLabelSize, acqLabelBaseFont.Style);
                            using Font acqValueFont = new Font(acqValueBaseFont.FontFamily, acqValueSize, acqValueBaseFont.Style);

                            float labelHeight = acqLabelFont.GetHeight(g);
                            float valueHeight = acqValueFont.GetHeight(g);
                            float blockHeight = labelHeight + valueHeight + 1f;
                            float dateY = drawY + Math.Max(0f, (barcode.Height - blockHeight) / 2f);

                            using var acqFormat = new StringFormat(StringFormat.GenericDefault)
                            {
                                Alignment = StringAlignment.Near,
                                LineAlignment = StringAlignment.Near,
                                FormatFlags = StringFormatFlags.NoWrap,
                                Trimming = StringTrimming.None
                            };

                            g.FillRectangle(Brushes.White, datePanelX, dateY, datePanelWidth, blockHeight + 2f);
                            var previousTextHint = g.TextRenderingHint;
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                            g.DrawString(acqLabelText, acqLabelFont, blackBrush, datePanelX, dateY, acqFormat);
                            float valueY = dateY + labelHeight + 1f;
                            g.DrawString(dateValueText, acqValueFont, blackBrush, datePanelX, valueY, acqFormat);
                            g.TextRenderingHint = previousTextHint;
                            dateBottom = valueY + valueHeight;
                            yPos = Math.Max(qrBottom, dateBottom) + _printStyleSettings.ExtraLineSpacing;
                        }
                        else
                        {
                            g.DrawString("(Barcode unavailable)", secondaryFont, blackBrush, _printStyleSettings.LeftMargin, yPos);
                            yPos += secondaryFont.GetHeight(g) + _printStyleSettings.ExtraLineSpacing + 4;
                        }
                    }

                    // Create a center-aligned layout format constraint for text block strings
                    using var textCenterFormat = new StringFormat()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    };

                    // Calculate the exact horizontal midpoint of our printable area width boundary
                    float centerPointX = _printStyleSettings.LeftMargin + (contentWidth / 2f);

                    for (int i = 4; i < lines.Count; i++)
                    {
                        if (lines[i].IndexOf("Acq. Date:", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            continue;
                        }

                        // FIXED: Strip space padding characters so GDI+ calculates alignment based on visible text layout
                        string cleanLineText = lines[i].Trim();
                        if (string.IsNullOrEmpty(cleanLineText))
                        {
                            continue;
                        }

                        Font lineFont = GetLineFont(i, headerFont, secondaryFont, bodyFont);

                        // FIXED: Render raw line contents precisely from the center coordinates
                        g.DrawString(cleanLineText, lineFont, blackBrush, centerPointX, yPos, textCenterFormat);
                        yPos += lineFont.GetHeight(g) + _printStyleSettings.ExtraLineSpacing;
                    }

                    // Draw cut line (dashed)
                    Pen dashedPen = new Pen(Color.Red) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                    g.DrawLine(dashedPen, 0, 390, 280, 390);
                    g.DrawString("CUT", new Font("Arial", 6), Brushes.Red, 260, 392);
                }

                // Dispose old image if exists
                if (picBoxTagPreview.Image != null)
                {
                    picBoxTagPreview.Image.Dispose();
                }

                picBoxTagPreview.Image = previewBitmap;
                lblPreviewStatus.Text = $"Preview {_currentPreviewIndex + 1} of {_previewAssets.Count}";
                btnPreviewPrevious.Enabled = _currentPreviewIndex > 0;
                btnPreviewNext.Enabled = _currentPreviewIndex < _previewAssets.Count - 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating preview: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static Font GetLineFont(int lineIndex, Font headerFont, Font secondaryFont, Font bodyFont)
        {
            if (lineIndex == 1)
            {
                return headerFont;
            }

            if (lineIndex == 2 || lineIndex == 3)
            {
                return secondaryFont;
            }

            return bodyFont;
        }

        private static float GetBestFitSize(Graphics g, string text, Font baseFont, float maxWidth, float minSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return baseFont.Size;
            }

            float size = baseFont.Size;
            while (size > minSize)
            {
                using (Font probe = new Font(baseFont.FontFamily, size, baseFont.Style))
                {
                    var measured = g.MeasureString(text, probe, int.MaxValue);
                    if (measured.Width <= maxWidth)
                    {
                        break;
                    }
                }

                size -= 0.5f;
            }

            return Math.Max(minSize, size);
        }

        private static string GetAcquisitionDateValue(Asset asset)
        {
            string source = !string.IsNullOrWhiteSpace(asset.AcquisitionDate)
                ? asset.AcquisitionDate.Trim()
                : (asset.AcquisitionDateDisplay ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var monthYearShort = System.Text.RegularExpressions.Regex.Match(source, @"^(?<month>\d{1,2})\s*[/\-]\s*(?<year>\d{2})$");
            if (monthYearShort.Success
                && int.TryParse(monthYearShort.Groups["month"].Value, out var m0)
                && int.TryParse(monthYearShort.Groups["year"].Value, out var y0)
                && m0 >= 1
                && m0 <= 12)
            {
                return $"{m0:D2}/{y0:D2}";
            }

            if (DateTime.TryParse(source, out var parsedDate))
            {
                return parsedDate.ToString("MM/yy");
            }

            var yearMonth = System.Text.RegularExpressions.Regex.Match(source, @"^(?<year>\d{4})\s*[,/\-]\s*(?<month>\d{1,2})$");
            if (yearMonth.Success
                && int.TryParse(yearMonth.Groups["year"].Value, out var y1)
                && int.TryParse(yearMonth.Groups["month"].Value, out var m1)
                && m1 >= 1
                && m1 <= 12)
            {
                return $"{m1:D2}/{(y1 % 100):D2}";
            }

            var monthYear = System.Text.RegularExpressions.Regex.Match(source, @"^(?<month>\d{1,2})\s*[/\-]\s*(?<year>\d{2,4})$");
            if (monthYear.Success
                && int.TryParse(monthYear.Groups["month"].Value, out var m2)
                && int.TryParse(monthYear.Groups["year"].Value, out var y2)
                && m2 >= 1
                && m2 <= 12)
            {
                int twoDigitYear = y2 % 100;
                return $"{m2:D2}/{twoDigitYear:D2}";
            }

            return source;
        }

        private static float DrawLogoInPreview(Graphics g, PrintStyleSettings settings, float left, float width, float y)
        {
            try
            {
                Image? logoImage = null;

                // First try embedded resource (Resources.resx)
                try
                {
                    var res = AssetTagPrinter.Properties.Resources.OneLineWithBackground111;
                    if (res != null)
                    {
                        logoImage = new Bitmap(res);
                    }
                }
                catch
                {
                    // ignore resource load errors and fall back to file
                }

                // Fall back to Logo folder files if embedded resource not available
                if (logoImage == null)
                {
                    string exePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    string logoPath = System.IO.Path.Combine(exePath, "Logo", "one_line with background 111.png");

                    if (!System.IO.File.Exists(logoPath))
                    {
                        logoPath = System.IO.Path.Combine("Logo", "one_line with background 111.png");
                    }

                    if (System.IO.File.Exists(logoPath))
                    {
                        logoImage = Image.FromFile(logoPath);
                    }
                }

                if (logoImage != null)
                {
                    using (logoImage)
                    {
                        float maxLogoWidth = width * (settings.LogoMaxWidthPercent / 100f);
                        float scale = logoImage.Width > maxLogoWidth ? maxLogoWidth / logoImage.Width : 1f;
                        int scaledWidth = (int)(logoImage.Width * scale);
                        int scaledHeight = (int)(logoImage.Height * scale);

                        float logoX = left + Math.Max(0f, (width - scaledWidth) / 2f);
                        g.DrawImage(logoImage, logoX, y, scaledWidth, scaledHeight);
                        y += scaledHeight + 5;
                    }
                }
                else
                {
                    // Show a tiny placeholder in the preview but keep the vertical gap minimal
                    g.DrawString("[Logo not found]", new Font("Arial", 8), Brushes.Gray, left, y);
                    y += 4; // small gap for preview only
                }
            }
            catch (Exception ex)
            {
                g.DrawString($"[Logo error: {ex.Message}]", new Font("Arial", 7), Brushes.Red, left, y);
                y += 15;
            }

            return y;
        }

        private void btnPreviewPrevious_Click(object? sender, EventArgs e)
        {
            if (_currentPreviewIndex > 0)
            {
                _currentPreviewIndex--;
                UpdatePreview();
            }
        }

        private void btnPreviewNext_Click(object? sender, EventArgs e)
        {
            if (_currentPreviewIndex < _previewAssets.Count - 1)
            {
                _currentPreviewIndex++;
                UpdatePreview();
            }
        }

        private int ExtractYearFromAcquisitionDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return 0;
            }

            dateString = dateString!.Trim();

            // Try to parse as a full date
            if (DateTime.TryParse(dateString, out var date))
            {
                return date.Year;
            }

            // If already in "YYYY, MM" format
            if (System.Text.RegularExpressions.Regex.IsMatch(dateString, @"^\d{4}"))
            {
                if (int.TryParse(dateString.Substring(0, 4), out var year))
                {
                    return year;
                }
            }

            // If it's just a year
            if (int.TryParse(dateString, out var yearOnly) && yearOnly >= 1900 && yearOnly <= 2100)
            {
                return yearOnly;
            }

            return 0;
        }
    }
}
