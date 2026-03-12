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
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.CheckBox chkLimitLoad;
        private System.Windows.Forms.Label lblLoadLimit;
        private System.Windows.Forms.NumericUpDown nudLoadLimit;
        private System.Windows.Forms.Label lblWarehouse;
        private System.Windows.Forms.ComboBox cmbWarehouse;

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
            this.btnHelp = new System.Windows.Forms.Button();
            this.chkLimitLoad = new System.Windows.Forms.CheckBox();
            this.lblLoadLimit = new System.Windows.Forms.Label();
            this.nudLoadLimit = new System.Windows.Forms.NumericUpDown();
            this.lblWarehouse = new System.Windows.Forms.Label();
            this.cmbWarehouse = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAssets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoadLimit)).BeginInit();
            this.pnlTagPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewAssets
            // 
            this.dataGridViewAssets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAssets.Location = new System.Drawing.Point(12, 75);
            this.dataGridViewAssets.Name = "dataGridViewAssets";
            this.dataGridViewAssets.Size = new System.Drawing.Size(500, 302);
            this.dataGridViewAssets.TabIndex = 6;
            // 
            // btnLoadCsv
            // 
            this.btnLoadCsv.Location = new System.Drawing.Point(12, 12);
            this.btnLoadCsv.Name = "btnLoadCsv";
            this.btnLoadCsv.Size = new System.Drawing.Size(90, 29);
            this.btnLoadCsv.TabIndex = 0;
            this.btnLoadCsv.Text = "Load CSV";
            this.btnLoadCsv.UseVisualStyleBackColor = true;
            this.btnLoadCsv.Click += new System.EventHandler(this.btnLoadCsv_Click);
            // 
            // btnDiagnostics
            // 
            this.btnDiagnostics.Location = new System.Drawing.Point(108, 12);
            this.btnDiagnostics.Name = "btnDiagnostics";
            this.btnDiagnostics.Size = new System.Drawing.Size(90, 29);
            this.btnDiagnostics.TabIndex = 1;
            this.btnDiagnostics.Text = "Diagnostics";
            this.btnDiagnostics.UseVisualStyleBackColor = true;
            this.btnDiagnostics.Click += new System.EventHandler(this.btnDiagnostics_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(204, 12);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(110, 29);
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "Help / Updates";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // chkLimitLoad
            // 
            this.chkLimitLoad.AutoSize = true;
            this.chkLimitLoad.Location = new System.Drawing.Point(325, 16);
            this.chkLimitLoad.Name = "chkLimitLoad";
            this.chkLimitLoad.Size = new System.Drawing.Size(79, 17);
            this.chkLimitLoad.TabIndex = 3;
            this.chkLimitLoad.Text = "Limit load:";
            this.chkLimitLoad.UseVisualStyleBackColor = true;
            // 
            // lblLoadLimit
            // 
            this.lblLoadLimit.AutoSize = true;
            this.lblLoadLimit.Location = new System.Drawing.Point(475, 16);
            this.lblLoadLimit.Name = "lblLoadLimit";
            this.lblLoadLimit.Size = new System.Drawing.Size(34, 13);
            this.lblLoadLimit.TabIndex = 10;
            this.lblLoadLimit.TabStop = false;
            this.lblLoadLimit.Text = "rows";
            // 
            // nudLoadLimit
            // 
            this.nudLoadLimit.Location = new System.Drawing.Point(410, 14);
            this.nudLoadLimit.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudLoadLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLoadLimit.Name = "nudLoadLimit";
            this.nudLoadLimit.Size = new System.Drawing.Size(60, 20);
            this.nudLoadLimit.TabIndex = 4;
            this.nudLoadLimit.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblPrinterStatus
            // 
            this.lblPrinterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrinterStatus.Location = new System.Drawing.Point(518, 16);
            this.lblPrinterStatus.Name = "lblPrinterStatus";
            this.lblPrinterStatus.Size = new System.Drawing.Size(270, 20);
            this.lblPrinterStatus.TabIndex = 5;
            this.lblPrinterStatus.TabStop = false;
            this.lblPrinterStatus.Text = "Printer: Checking...";
            this.lblPrinterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWarehouse
            // 
            this.lblWarehouse.AutoSize = true;
            this.lblWarehouse.Location = new System.Drawing.Point(12, 52);
            this.lblWarehouse.Name = "lblWarehouse";
            this.lblWarehouse.Size = new System.Drawing.Size(65, 13);
            this.lblWarehouse.TabIndex = 11;
            this.lblWarehouse.TabStop = false;
            this.lblWarehouse.Text = "Warehouse:";
            // 
            // cmbWarehouse
            // 
            this.cmbWarehouse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWarehouse.FormattingEnabled = true;
            this.cmbWarehouse.Location = new System.Drawing.Point(83, 49);
            this.cmbWarehouse.Name = "cmbWarehouse";
            this.cmbWarehouse.Size = new System.Drawing.Size(429, 21);
            this.cmbWarehouse.TabIndex = 5;
            this.cmbWarehouse.SelectedIndexChanged += new System.EventHandler(this.cmbWarehouse_SelectedIndexChanged);
            // 
            // pnlTagPreview
            // 
            this.pnlTagPreview.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTagPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTagPreview.Controls.Add(this.lblTagPreview);
            this.pnlTagPreview.Location = new System.Drawing.Point(518, 75);
            this.pnlTagPreview.Name = "pnlTagPreview";
            this.pnlTagPreview.Size = new System.Drawing.Size(270, 302);
            this.pnlTagPreview.TabIndex = 3;
            this.pnlTagPreview.TabStop = false;
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
            this.lblTagPreview.TabStop = false;
            this.lblTagPreview.Text = "┌────────────────────┐\r\n│ [Your Logo - tiny] │\r\n├────────────────────┤\r\n│ [Barcode/QR Code] │\r\n│   (High density)   │\r\n├────────────────────┤\r\n│  **ID: BCC-RAM-  │\r\n│     00004**        │\r\n└────────────────────┘";
            this.lblTagPreview.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(12, 383);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(376, 43);
            this.btnPrint.TabIndex = 7;
            this.btnPrint.Text = "Print Selected (or All)";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnPrintPreview
            // 
            this.btnPrintPreview.Location = new System.Drawing.Point(412, 383);
            this.btnPrintPreview.Name = "btnPrintPreview";
            this.btnPrintPreview.Size = new System.Drawing.Size(376, 43);
            this.btnPrintPreview.TabIndex = 8;
            this.btnPrintPreview.Text = "Print Preview";
            this.btnPrintPreview.UseVisualStyleBackColor = true;
            this.btnPrintPreview.Click += new System.EventHandler(this.btnPrintPreview_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.cmbWarehouse);
            this.Controls.Add(this.lblWarehouse);
            this.Controls.Add(this.lblLoadLimit);
            this.Controls.Add(this.nudLoadLimit);
            this.Controls.Add(this.chkLimitLoad);
            this.Controls.Add(this.btnHelp);
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
            ((System.ComponentModel.ISupportInitialize)(this.nudLoadLimit)).EndInit();
            this.pnlTagPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
