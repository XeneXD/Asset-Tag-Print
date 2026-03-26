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
        private const int ItemsPerPage = 12;
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

            // Print and Preview require assets and selection
            btnPrint.Enabled = hasAssets && hasSelection && !_isPrinting;
            btnPrintPreview.Enabled = hasAssets && hasSelection;
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

            if (keyData == (Keys.Control | Keys.Shift | Keys.P))
            {
                btnPrintPreview?.PerformClick();
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
            if (e.RowIndex >= 0 && dataGridViewAssets.Rows[e.RowIndex].DataBoundItem is Asset asset)
            {
                UpdatePreviewPanel(asset);
            }
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
                UpdatePreviewPanel(pageList[0]);
            }
            else
            {
                lblTagPreview.Text = string.Empty;
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

        private void UpdatePreviewPanel(Asset asset)
        {
            lblTagPreview.Text = TagLayoutFormatter.BuildMainPreviewText(asset);
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

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            try
            {
                var assetsToPrint = GetAssetsToPrint();
                
                if (assetsToPrint.Count == 0)
                {
                    MessageBox.Show("Please select at least one asset to preview. Use Space in grid mode to select.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show print preview form
                PrintPreviewForm previewForm = new PrintPreviewForm(assetsToPrint, _printStyleSettings);
                previewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating preview: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrintStyle_Click(object sender, EventArgs e)
        {
            using (var styleEditor = new PrintStyleEditorForm(_printStyleSettings))
            {
                if (styleEditor.ShowDialog(this) == DialogResult.OK)
                {
                    _printStyleSettings = styleEditor.ResultSettings.Clone();
                    MessageBox.Show(
                        "Print style updated. Open Print Preview to verify before printing.",
                        "Print Style",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
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
