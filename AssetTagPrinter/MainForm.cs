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
        private PrinterService? _printerService;
        private CsvService _csvService;
        private bool _isPrinting;
        private List<Asset> _loadedAssets = new List<Asset>();

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
            chkLimitLoad.CheckedChanged += FilterControlsChanged;
            nudLoadLimit.ValueChanged += FilterControlsChanged;
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
                if (cmbWarehouse != null && cmbWarehouse.CanFocus)
                {
                    cmbWarehouse.Focus();
                }
                return true;
            }

            if (keyData == (Keys.Control | Keys.L))
            {
                if (chkLimitLoad != null)
                {
                    chkLimitLoad.Checked = !chkLimitLoad.Checked;
                }

                if (nudLoadLimit != null && nudLoadLimit.CanFocus)
                {
                    nudLoadLimit.Focus();
                }

                return true;
            }

            if (keyData == Keys.Enter)
            {
                if (chkLimitLoad != null && chkLimitLoad.Focused)
                {
                    chkLimitLoad.Checked = !chkLimitLoad.Checked;
                    return true;
                }

                if (cmbWarehouse != null && (cmbWarehouse.Focused || cmbWarehouse.DroppedDown))
                {
                    if (cmbWarehouse.DroppedDown)
                    {
                        cmbWarehouse.DroppedDown = false;
                    }

                    ApplyWarehouseFilter();
                    return true;
                }

                if (nudLoadLimit != null && nudLoadLimit.Focused)
                {
                    ApplyWarehouseFilter();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshPrinterStatus();

            // Auto-load default CSV when available so the grid is populated on startup.
            var defaultCsvPath = Path.Combine(AppContext.BaseDirectory, "data.csv");
            if (File.Exists(defaultCsvPath))
            {
                LoadAssetsIntoGrid(defaultCsvPath);
            }
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
            PopulateWarehouseFilter(_loadedAssets);
            return ApplyWarehouseFilter();
        }

        private void PopulateWarehouseFilter(List<Asset> assets)
        {
            if (cmbWarehouse == null)
            {
                return;
            }

            string current = cmbWarehouse.SelectedItem as string ?? "All";
            bool hasBlankWarehouse = assets.Any(a => string.IsNullOrWhiteSpace(a.Warehouse));
            var warehouses = assets
                .Select(a => (a.Warehouse ?? string.Empty).Trim())
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .OrderBy(w => w)
                .ToList();

            cmbWarehouse.BeginUpdate();
            try
            {
                cmbWarehouse.Items.Clear();
                cmbWarehouse.Items.Add("All");
                if (hasBlankWarehouse)
                {
                    cmbWarehouse.Items.Add(BlankWarehouseOption);
                }

                foreach (var w in warehouses)
                {
                    cmbWarehouse.Items.Add(w);
                }

                int idx = cmbWarehouse.Items.IndexOf(current);
                cmbWarehouse.SelectedIndex = idx >= 0 ? idx : 0;
            }
            finally
            {
                cmbWarehouse.EndUpdate();
            }
        }

        private int ApplyWarehouseFilter()
        {
            if (cmbWarehouse == null)
            {
                return 0;
            }

            IEnumerable<Asset> view = _loadedAssets;

            string selected = cmbWarehouse.SelectedItem as string ?? "All";
            if (!string.Equals(selected, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(selected, BlankWarehouseOption, StringComparison.Ordinal))
                {
                    view = view.Where(a => string.IsNullOrWhiteSpace(a.Warehouse));
                }
                else
                {
                    view = view.Where(a => string.Equals((a.Warehouse ?? string.Empty).Trim(), selected, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (chkLimitLoad != null && nudLoadLimit != null && chkLimitLoad.Checked)
            {
                view = view.Take((int)nudLoadLimit.Value);
            }

            var list = view.ToList();
            dataGridViewAssets.DataSource = null;
            dataGridViewAssets.DataSource = new BindingSource { DataSource = list };

            if (list.Count > 0)
            {
                dataGridViewAssets.ClearSelection();
                dataGridViewAssets.Rows[0].Selected = true;
                UpdatePreviewPanel(list[0]);
            }
            else
            {
                lblTagPreview.Text = string.Empty;
            }

            return list.Count;
        }

        private void FilterControlsChanged(object? sender, EventArgs e)
        {
            if (_loadedAssets.Count == 0)
            {
                return;
            }

            ApplyWarehouseFilter();
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
                PrintPreviewForm previewForm = new PrintPreviewForm(selectedAssets);
                previewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating preview: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void cmbWarehouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyWarehouseFilter();
        }
    }
}
