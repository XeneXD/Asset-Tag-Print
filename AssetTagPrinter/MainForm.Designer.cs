namespace AssetTagPrinter
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridViewAssets;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnLoadCsv;
        private System.Windows.Forms.Button btnPrintPreview;
        private System.Windows.Forms.Panel pnlTagPreview;
        private System.Windows.Forms.Label lblTagPreview;
        private System.Windows.Forms.Label lblPrinterStatus;
        private System.Windows.Forms.Button btnDiagnostics;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dataGridViewAssets = new System.Windows.Forms.DataGridView();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnLoadCsv = new System.Windows.Forms.Button();
            this.btnPrintPreview = new System.Windows.Forms.Button();
            this.pnlTagPreview = new System.Windows.Forms.Panel();
            this.lblTagPreview = new System.Windows.Forms.Label();
            this.lblPrinterStatus = new System.Windows.Forms.Label();
            this.btnDiagnostics = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAssets)).BeginInit();
            this.pnlTagPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewAssets
            // 
            this.dataGridViewAssets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAssets.Location = new System.Drawing.Point(12, 47);
            this.dataGridViewAssets.Name = "dataGridViewAssets";
            this.dataGridViewAssets.Size = new System.Drawing.Size(500, 330);
            this.dataGridViewAssets.TabIndex = 0;
            // 
            // btnLoadCsv
            // 
            this.btnLoadCsv.Location = new System.Drawing.Point(12, 12);
            this.btnLoadCsv.Name = "btnLoadCsv";
            this.btnLoadCsv.Size = new System.Drawing.Size(120, 29);
            this.btnLoadCsv.TabIndex = 2;
            this.btnLoadCsv.Text = "Load CSV";
            this.btnLoadCsv.UseVisualStyleBackColor = true;
            this.btnLoadCsv.Click += new System.EventHandler(this.btnLoadCsv_Click);
            // 
            // btnDiagnostics
            // 
            this.btnDiagnostics.Location = new System.Drawing.Point(138, 12);
            this.btnDiagnostics.Name = "btnDiagnostics";
            this.btnDiagnostics.Size = new System.Drawing.Size(120, 29);
            this.btnDiagnostics.TabIndex = 6;
            this.btnDiagnostics.Text = "Diagnostics";
            this.btnDiagnostics.UseVisualStyleBackColor = true;
            this.btnDiagnostics.Click += new System.EventHandler(this.btnDiagnostics_Click);
            // 
            // lblPrinterStatus
            // 
            this.lblPrinterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrinterStatus.Location = new System.Drawing.Point(138, 16);
            this.lblPrinterStatus.Name = "lblPrinterStatus";
            this.lblPrinterStatus.Size = new System.Drawing.Size(650, 20);
            this.lblPrinterStatus.TabIndex = 5;
            this.lblPrinterStatus.Text = "Printer: Checking...";
            this.lblPrinterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlTagPreview
            // 
            this.pnlTagPreview.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTagPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTagPreview.Controls.Add(this.lblTagPreview);
            this.pnlTagPreview.Location = new System.Drawing.Point(518, 47);
            this.pnlTagPreview.Name = "pnlTagPreview";
            this.pnlTagPreview.Size = new System.Drawing.Size(270, 330);
            this.pnlTagPreview.TabIndex = 3;
            // 
            // lblTagPreview
            // 
            this.lblTagPreview.BackColor = System.Drawing.Color.Black;
            this.lblTagPreview.Font = new System.Drawing.Font("Courier New", 8F);
            this.lblTagPreview.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblTagPreview.Location = new System.Drawing.Point(8, 8);
            this.lblTagPreview.Name = "lblTagPreview";
            this.lblTagPreview.Size = new System.Drawing.Size(254, 314);
            this.lblTagPreview.TabIndex = 0;
            this.lblTagPreview.Text = "┌────────────────────┐\r\n│ [Your Logo - tiny] │\r\n├────────────────────┤\r\n│ [Barcode/QR Code] │\r\n│   (High density)   │\r\n├────────────────────┤\r\n│  **ID: BCC-RAM-  │\r\n│     00004**        │\r\n└────────────────────┘";
            this.lblTagPreview.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(12, 383);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(376, 43);
            this.btnPrint.TabIndex = 1;
            this.btnPrint.Text = "Print Selected (or All)";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnPrintPreview
            // 
            this.btnPrintPreview.Location = new System.Drawing.Point(412, 383);
            this.btnPrintPreview.Name = "btnPrintPreview";
            this.btnPrintPreview.Size = new System.Drawing.Size(376, 43);
            this.btnPrintPreview.TabIndex = 4;
            this.btnPrintPreview.Text = "Print Preview";
            this.btnPrintPreview.UseVisualStyleBackColor = true;
            this.btnPrintPreview.Click += new System.EventHandler(this.btnPrintPreview_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDiagnostics);
            this.Controls.Add(this.lblPrinterStatus);
            this.Controls.Add(this.pnlTagPreview);
            this.Controls.Add(this.btnPrintPreview);
            this.Controls.Add(this.btnLoadCsv);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.dataGridViewAssets);
            this.Name = "MainForm";
            this.Text = "Asset Tag Printer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAssets)).EndInit();
            this.pnlTagPreview.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
