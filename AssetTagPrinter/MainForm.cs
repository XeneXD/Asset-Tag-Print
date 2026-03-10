using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public partial class MainForm : Form
    {
        private PrinterService? _printerService;
        private CsvService _csvService;

        public MainForm()
        {
            InitializeComponent();
            _csvService = new CsvService();
            dataGridViewAssets.CellClick += DataGridViewAssets_CellClick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Auto-load default CSV when available so the grid is populated on startup.
            var defaultCsvPath = Path.Combine(AppContext.BaseDirectory, "data.csv");
            if (File.Exists(defaultCsvPath))
            {
                LoadAssetsIntoGrid(defaultCsvPath);
            }
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
                UpdatePreviewPanel(assets[0]);
            }

            return assets.Count;
        }

        private void UpdatePreviewPanel(Asset asset)
        {
            // Truncate barcode to fit, but keep full reference ID
            string barcode = TruncateText(asset.Barcode, 16);

            lblTagPreview.Text = $"┌────────────────────┐\n" +
                                 $"│ [Your Logo-tiny] │\n" +
                                 $"├────────────────────┤\n" +
                                 $"│ {barcode.PadRight(16)} │\n" +
                                 $"│  (High density)  │\n" +
                                 $"├────────────────────┤\n" +
                                 $"│ ID: {asset.Ref}    │\n" +
                                 $"└────────────────────┘";
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length > maxLength)
                return text.Substring(0, maxLength - 2) + "..";

            return text;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                _printerService ??= new PrinterService();
                _printerService.Open();
                foreach (DataGridViewRow row in dataGridViewAssets.Rows)
                {
                    if (row.DataBoundItem is Asset asset)
                    {
                        _printerService.PrintAssetTag(asset);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _printerService?.Close();
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
    }
}
