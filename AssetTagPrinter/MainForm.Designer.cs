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
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblFilterValue;
        private System.Windows.Forms.ComboBox cmbFilterValue;
        private System.Windows.Forms.Button btnPrintStyle;

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
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblFilterValue = new System.Windows.Forms.Label();
            this.cmbFilterValue = new System.Windows.Forms.ComboBox();
            this.btnPrintStyle = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAssets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLoadLimit)).BeginInit();
            this.pnlTagPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewAssets
            // 
            this.dataGridViewAssets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAssets.Location = new System.Drawing.Point(12, 95);
            this.dataGridViewAssets.Name = "dataGridViewAssets";
            this.dataGridViewAssets.Size = new System.Drawing.Size(500, 282);
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
            // btnPrintStyle
            // 
            this.btnPrintStyle.Location = new System.Drawing.Point(320, 12);
            this.btnPrintStyle.Name = "btnPrintStyle";
            this.btnPrintStyle.Size = new System.Drawing.Size(110, 29);
            this.btnPrintStyle.TabIndex = 3;
            this.btnPrintStyle.Text = "Print Style";
            this.btnPrintStyle.UseVisualStyleBackColor = true;
            this.btnPrintStyle.Click += new System.EventHandler(this.btnPrintStyle_Click);
            // 
            // chkLimitLoad
            // 
            this.chkLimitLoad.AutoSize = true;
            this.chkLimitLoad.Location = new System.Drawing.Point(436, 16);
            this.chkLimitLoad.Name = "chkLimitLoad";
            this.chkLimitLoad.Size = new System.Drawing.Size(79, 17);
            this.chkLimitLoad.TabIndex = 4;
            this.chkLimitLoad.Text = "Limit load:";
            this.chkLimitLoad.UseVisualStyleBackColor = true;
            // 
            // lblLoadLimit
            // 
            this.lblLoadLimit.AutoSize = true;
            this.lblLoadLimit.Location = new System.Drawing.Point(586, 16);
            this.lblLoadLimit.Name = "lblLoadLimit";
            this.lblLoadLimit.Size = new System.Drawing.Size(34, 13);
            this.lblLoadLimit.TabIndex = 10;
            this.lblLoadLimit.TabStop = false;
            this.lblLoadLimit.Text = "rows";
            // 
            // nudLoadLimit
            // 
            this.nudLoadLimit.Location = new System.Drawing.Point(521, 14);
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
            this.nudLoadLimit.TabIndex = 5;
            this.nudLoadLimit.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblPrinterStatus
            // 
            this.lblPrinterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrinterStatus.Location = new System.Drawing.Point(518, 45);
            this.lblPrinterStatus.Name = "lblPrinterStatus";
            this.lblPrinterStatus.Size = new System.Drawing.Size(270, 20);
            this.lblPrinterStatus.TabIndex = 5;
            this.lblPrinterStatus.TabStop = false;
            this.lblPrinterStatus.Text = "Printer: Checking...";
            this.lblPrinterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(12, 52);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 11;
            this.lblCategory.TabStop = false;
            this.lblCategory.Text = "Category:";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Items.AddRange(new object[] {
            "None",
            "Warehouse"});
            this.cmbCategory.Location = new System.Drawing.Point(70, 49);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(442, 21);
            this.cmbCategory.TabIndex = 6;
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);
            // 
            // lblFilterValue
            // 
            this.lblFilterValue.AutoSize = true;
            this.lblFilterValue.Location = new System.Drawing.Point(12, 70);
            this.lblFilterValue.Name = "lblFilterValue";
            this.lblFilterValue.Size = new System.Drawing.Size(52, 13);
            this.lblFilterValue.TabIndex = 12;
            this.lblFilterValue.TabStop = false;
            this.lblFilterValue.Text = "Filter by:";
            this.lblFilterValue.Visible = false;
            // 
            // cmbFilterValue
            // 
            this.cmbFilterValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterValue.FormattingEnabled = true;
            this.cmbFilterValue.Location = new System.Drawing.Point(70, 67);
            this.cmbFilterValue.Name = "cmbFilterValue";
            this.cmbFilterValue.Size = new System.Drawing.Size(442, 21);
            this.cmbFilterValue.TabIndex = 7;
            this.cmbFilterValue.Visible = false;
            this.cmbFilterValue.SelectedIndexChanged += new System.EventHandler(this.cmbFilterValue_SelectedIndexChanged);
            // 
            // pnlTagPreview
            // 
            this.pnlTagPreview.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTagPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTagPreview.Controls.Add(this.lblTagPreview);
            this.pnlTagPreview.Location = new System.Drawing.Point(518, 95);
            this.pnlTagPreview.Name = "pnlTagPreview";
            this.pnlTagPreview.Size = new System.Drawing.Size(270, 282);
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
            this.Controls.Add(this.cmbFilterValue);
            this.Controls.Add(this.lblFilterValue);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.btnPrintStyle);
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
