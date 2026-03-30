namespace AssetTagPrinter
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridViewAssets;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnLoadCsv;
        private System.Windows.Forms.Panel pnlTagPreview;
        private System.Windows.Forms.PictureBox picBoxTagPreview;
        private System.Windows.Forms.Label lblPreviewStatus;
        private System.Windows.Forms.Button btnPreviewPrevious;
        private System.Windows.Forms.Button btnPreviewNext;
        private System.Windows.Forms.Label lblPrinterStatus;
        private System.Windows.Forms.Button btnDiagnostics;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnPreviousPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Label lblPageInfo;
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
            this.pnlTagPreview = new System.Windows.Forms.Panel();
            this.picBoxTagPreview = new System.Windows.Forms.PictureBox();
            this.lblPreviewStatus = new System.Windows.Forms.Label();
            this.btnPreviewPrevious = new System.Windows.Forms.Button();
            this.btnPreviewNext = new System.Windows.Forms.Button();
            this.lblPrinterStatus = new System.Windows.Forms.Label();
            this.btnDiagnostics = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnPreviousPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblFilterValue = new System.Windows.Forms.Label();
            this.cmbFilterValue = new System.Windows.Forms.ComboBox();
            this.btnPrintStyle = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAssets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxTagPreview)).BeginInit();
            this.pnlTagPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewAssets
            // 
            this.dataGridViewAssets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAssets.Location = new System.Drawing.Point(12, 95);
            this.dataGridViewAssets.Name = "dataGridViewAssets";
            this.dataGridViewAssets.Size = new System.Drawing.Size(500, 385);
            this.dataGridViewAssets.TabStop = false;
            this.dataGridViewAssets.TabIndex = 999;
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
            this.btnPrintStyle.Visible = false;
            // 
            // btnPreviousPage
            // 
            this.btnPreviousPage.Location = new System.Drawing.Point(12, 500);
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.Size = new System.Drawing.Size(90, 23);
            this.btnPreviousPage.TabIndex = 6;
            this.btnPreviousPage.Text = "< Previous";
            this.btnPreviousPage.UseVisualStyleBackColor = true;
            this.btnPreviousPage.Click += new System.EventHandler(this.btnPreviousPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Location = new System.Drawing.Point(108, 500);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(90, 23);
            this.btnNextPage.TabIndex = 7;
            this.btnNextPage.Text = "Next >";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(204, 505);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(80, 13);
            this.lblPageInfo.TabIndex = 10;
            this.lblPageInfo.TabStop = false;
            this.lblPageInfo.Text = "Page 1 of 1";
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
            "Warehouse",
            "Acquisition Date"});
            this.cmbCategory.Location = new System.Drawing.Point(80, 49);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(220, 18);
            this.cmbCategory.DropDownWidth = 260;
            this.cmbCategory.TabIndex = 4;
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);
            // 
            // lblFilterValue
            // 
            this.lblFilterValue.AutoSize = true;
            this.lblFilterValue.Location = new System.Drawing.Point(310, 52);
            this.lblFilterValue.Name = "lblFilterValue";
            this.lblFilterValue.Size = new System.Drawing.Size(72, 13);
            this.lblFilterValue.TabIndex = 12;
            this.lblFilterValue.TabStop = false;
            this.lblFilterValue.Text = "Filter by:";
            this.lblFilterValue.Visible = false;
            // 
            // cmbFilterValue
            // 
            this.cmbFilterValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterValue.FormattingEnabled = true;
            this.cmbFilterValue.Location = new System.Drawing.Point(380, 49);
            this.cmbFilterValue.Name = "cmbFilterValue";
            this.cmbFilterValue.Size = new System.Drawing.Size(140, 18);
            this.cmbFilterValue.DropDownWidth = 220;
            this.cmbFilterValue.TabIndex = 5;
            this.cmbFilterValue.Visible = false;
            this.cmbFilterValue.SelectedIndexChanged += new System.EventHandler(this.cmbFilterValue_SelectedIndexChanged);
            // 
            // pnlTagPreview
            // 
            this.pnlTagPreview.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTagPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTagPreview.Controls.Add(this.picBoxTagPreview);
            this.pnlTagPreview.Controls.Add(this.lblPreviewStatus);
            this.pnlTagPreview.Controls.Add(this.btnPreviewNext);
            this.pnlTagPreview.Controls.Add(this.btnPreviewPrevious);
            this.pnlTagPreview.Location = new System.Drawing.Point(518, 95);
            this.pnlTagPreview.Name = "pnlTagPreview";
            this.pnlTagPreview.Size = new System.Drawing.Size(270, 385);
            this.pnlTagPreview.TabIndex = 3;
            this.pnlTagPreview.TabStop = false;
            // 
            // picBoxTagPreview
            // 
            this.picBoxTagPreview.BackColor = System.Drawing.Color.White;
            this.picBoxTagPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxTagPreview.Location = new System.Drawing.Point(8, 8);
            this.picBoxTagPreview.Name = "picBoxTagPreview";
            this.picBoxTagPreview.Size = new System.Drawing.Size(254, 300);
            this.picBoxTagPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBoxTagPreview.TabIndex = 0;
            this.picBoxTagPreview.TabStop = false;
            // 
            // lblPreviewStatus
            // 
            this.lblPreviewStatus.AutoSize = true;
            this.lblPreviewStatus.Location = new System.Drawing.Point(8, 315);
            this.lblPreviewStatus.Name = "lblPreviewStatus";
            this.lblPreviewStatus.Size = new System.Drawing.Size(71, 13);
            this.lblPreviewStatus.TabIndex = 1;
            this.lblPreviewStatus.TabStop = false;
            this.lblPreviewStatus.Text = "No selection";
            // 
            // btnPreviewPrevious
            // 
            this.btnPreviewPrevious.Enabled = false;
            this.btnPreviewPrevious.Location = new System.Drawing.Point(8, 335);
            this.btnPreviewPrevious.Name = "btnPreviewPrevious";
            this.btnPreviewPrevious.Size = new System.Drawing.Size(118, 25);
            this.btnPreviewPrevious.TabIndex = 2;
            this.btnPreviewPrevious.Text = "< Previous";
            this.btnPreviewPrevious.UseVisualStyleBackColor = true;
            this.btnPreviewPrevious.Click += new System.EventHandler(this.btnPreviewPrevious_Click);
            // 
            // btnPreviewNext
            // 
            this.btnPreviewNext.Enabled = false;
            this.btnPreviewNext.Location = new System.Drawing.Point(144, 335);
            this.btnPreviewNext.Name = "btnPreviewNext";
            this.btnPreviewNext.Size = new System.Drawing.Size(118, 25);
            this.btnPreviewNext.TabIndex = 3;
            this.btnPreviewNext.Text = "Next >";
            this.btnPreviewNext.UseVisualStyleBackColor = true;
            this.btnPreviewNext.Click += new System.EventHandler(this.btnPreviewNext_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(518, 490);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(270, 43);
            this.btnPrint.TabIndex = 8;
            this.btnPrint.Text = "Print Selected (or All)";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 560);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Controls.Add(this.cmbFilterValue);
            this.Controls.Add(this.lblFilterValue);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.btnPrintStyle);
            this.Controls.Add(this.lblPageInfo);
            this.Controls.Add(this.btnNextPage);
            this.Controls.Add(this.btnPreviousPage);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnDiagnostics);
            this.Controls.Add(this.lblPrinterStatus);
            this.Controls.Add(this.pnlTagPreview);
            this.Controls.Add(this.btnLoadCsv);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.dataGridViewAssets);
            this.Name = "MainForm";
            this.Text = "Asset Tag Printer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAssets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxTagPreview)).EndInit();
            this.pnlTagPreview.ResumeLayout(false);
            this.pnlTagPreview.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
