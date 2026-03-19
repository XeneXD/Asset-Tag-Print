using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public partial class MainForm : Form
    {
        private const string BlankWarehouseOption = "(Blank Warehouse)";
        private const int ItemsPerPage = 12;
        
        private PrinterService? _printerService;
        private CsvService _csvService;
        private bool _isPrinting;
        private List<Asset> _loadedAssets = new List<Asset>();
        private List<Asset> _filteredAssets = new List<Asset>();
        private PrintStyleSettings _printStyleSettings = PrintStyleSettings.CreateDefault();
        private int _currentPage = 1;

        public MainForm()
        {
            InitializeComponent();
            _csvService = new CsvService();
            dataGridViewAssets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAssets.MultiSelect = true;
            dataGridViewAssets.ReadOnly = true;
            dataGridViewAssets.AllowUserToAddRows = false;
            dataGridViewAssets.AllowUserToDeleteRows = false;
            dataGridViewAssets.AllowUserToOrderColumns = false;
            dataGridViewAssets.CellClick += DataGridViewAssets_CellClick;
            btnPreviousPage.Click += btnPreviousPage_Click;
            btnNextPage.Click += btnNextPage_Click;
            cmbCategory.SelectedIndex = 0;
            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
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

            if (keyData == (Keys.Control | Keys.W))
            {
                if (cmbCategory != null && cmbCategory.CanFocus)
                {
                    cmbCategory.Focus();
                }
                return true;
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
                openFileDialog.InitialDirectory = AppContext.BaseDirectory;

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
            var assets = _csvService.ReadAssets(csvPath).ToList();
            _loadedAssets = assets;
            return ApplyFilters();
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

        private void DisplayCurrentPage()
        {
            // Calculate pagination
            int totalPages = (_filteredAssets.Count + ItemsPerPage - 1) / ItemsPerPage; // Ceiling division
            if (totalPages == 0) totalPages = 1;
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
            int totalPages = (_filteredAssets.Count + ItemsPerPage - 1) / ItemsPerPage;
            if (totalPages == 0) totalPages = 1;
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

            _isPrinting = true;
            btnPrint.Enabled = false;
            RefreshPrinterStatus();

            try
            {
                var assetsToPrint = GetAssetsToPrint();
                if (assetsToPrint.Count == 0)
                {
                    MessageBox.Show("No assets to print. Please load a CSV file first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _printerService?.Close();
                _isPrinting = false;
                RefreshPrinterStatus();
            }
        }

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedAssets = new System.Collections.Generic.List<Asset>();

                // Collect all assets from grid
                foreach (DataGridViewRow row in dataGridViewAssets.Rows)
                {
                    if (row.DataBoundItem is Asset asset)
                    {
                        selectedAssets.Add(asset);
                    }
                }

                if (selectedAssets.Count == 0)
                {
                    MessageBox.Show("No assets to preview. Please load a CSV file first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show print preview form
                PrintPreviewForm previewForm = new PrintPreviewForm(selectedAssets, _printStyleSettings);
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
