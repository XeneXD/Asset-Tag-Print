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
        private PrinterService? _printerService;
        private CsvService _csvService;
        private bool _isPrinting;

        public MainForm()
        {
            InitializeComponent();
            _csvService = new CsvService();
            dataGridViewAssets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAssets.MultiSelect = false;
            dataGridViewAssets.CellClick += DataGridViewAssets_CellClick;
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

            // Reset binding to force DataGridView repaint when reloading another file.
            dataGridViewAssets.DataSource = null;
            dataGridViewAssets.DataSource = new BindingSource { DataSource = assets };

            if (assets.Count > 0)
            {
                dataGridViewAssets.ClearSelection();
                dataGridViewAssets.Rows[0].Selected = true;
                UpdatePreviewPanel(assets[0]);
            }

            return assets.Count;
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
                foreach (var asset in assetsToPrint)
                {
                    _printerService.PrintAssetTag(asset);
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
    }
}
