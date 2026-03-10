using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public partial class PrintPreviewForm : Form
    {
        private List<Asset> _assets;
        private int _currentIndex = 0;

        public PrintPreviewForm(List<Asset> assets)
        {
            InitializeComponent();
            _assets = assets;
            ShowCurrentAsset();
        }

        private void ShowCurrentAsset()
        {
            if (_currentIndex < _assets.Count)
            {
                var asset = _assets[_currentIndex];
                UpdatePreview(asset);
                lblStatus!.Text = $"Preview {_currentIndex + 1} of {_assets.Count}";
                btnPrevious!.Enabled = _currentIndex > 0;
                btnNext!.Enabled = _currentIndex < _assets.Count - 1;
            }
        }

        private void UpdatePreview(Asset asset)
        {
            try
            {
                // Create a bitmap to draw the preview
                Bitmap previewBitmap = new Bitmap(280, 400);
                using (Graphics g = Graphics.FromImage(previewBitmap))
                {
                    g.Clear(Color.White);
                    g.DrawRectangle(Pens.Black, 0, 0, 279, 399);

                    // Font sizes
                    Font smallFont = new Font("Arial", 7);
                    Font mediumFont = new Font("Arial", 8, FontStyle.Bold);
                    Font largeFont = new Font("Arial", 9, FontStyle.Bold);
                    Brush blackBrush = Brushes.Black;

                    int yPos = 10;

                    // Logo placeholder
                    g.DrawString("[Your Logo - tiny]", smallFont, blackBrush, 20, yPos);
                    yPos += 20;

                    // Divider line
                    g.DrawLine(Pens.Black, 5, yPos, 275, yPos);
                    yPos += 10;

                    // Barcode number text
                    g.DrawString(asset.Barcode, mediumFont, blackBrush, 10, yPos);
                    yPos += 25;

                    g.DrawRectangle(Pens.Black, 20, yPos, 240, 60);
                    g.DrawString("Barcode generated at print time", smallFont, blackBrush, 55, yPos + 24);
                    yPos += 65;

                    // High density note
                    g.DrawString("(High density)", smallFont, blackBrush, 70, yPos);
                    yPos += 20;

                    // Divider line
                    g.DrawLine(Pens.Black, 5, yPos, 275, yPos);
                    yPos += 10;

                    // ID and Reference
                    g.DrawString($"**ID: {asset.Ref}**", largeFont, blackBrush, 10, yPos);
                    yPos += 25;

                    // Label
                    string label = asset.Label.Length > 30 ? asset.Label.Substring(0, 27) + "..." : asset.Label;
                    g.DrawString(label, mediumFont, blackBrush, 10, yPos);
                    yPos += 25;

                    // Bottom divider
                    g.DrawLine(Pens.Black, 5, yPos, 275, yPos);

                    // Draw cut line (dashed)
                    Pen dashedPen = new Pen(Color.Red) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                    g.DrawLine(dashedPen, 0, 390, 280, 390);
                    g.DrawString("CUT", new Font("Arial", 6), Brushes.Red, 260, 392);
                }

                lblPreview!.Image = previewBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating preview: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPrevious_Click(object? sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                ShowCurrentAsset();
            }
        }

        private void btnNext_Click(object? sender, EventArgs e)
        {
            if (_currentIndex < _assets.Count - 1)
            {
                _currentIndex++;
                ShowCurrentAsset();
            }
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void InitializeComponent()
        {
            this.lblPreview = new System.Windows.Forms.PictureBox();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.lblPreview)).BeginInit();
            this.SuspendLayout();

            // lblPreview (PictureBox)
            this.lblPreview.BackColor = System.Drawing.Color.LightGray;
            this.lblPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPreview.Location = new System.Drawing.Point(20, 20);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(300, 320);
            this.lblPreview.TabIndex = 0;
            this.lblPreview.TabStop = false;
            this.lblPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(20, 350);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(300, 20);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // btnPrevious
            this.btnPrevious.Location = new System.Drawing.Point(20, 380);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(90, 40);
            this.btnPrevious.TabIndex = 2;
            this.btnPrevious.Text = "< Previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);

            // btnNext
            this.btnNext.Location = new System.Drawing.Point(120, 380);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(90, 40);
            this.btnNext.TabIndex = 3;
            this.btnNext.Text = "Next >";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);

            // btnClose
            this.btnClose.Location = new System.Drawing.Point(230, 380);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 40);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // PrintPreviewForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 440);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblPreview);
            this.Name = "PrintPreviewForm";
            this.Text = "Print Preview - Paper Layout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.lblPreview)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.PictureBox? lblPreview;
        private System.Windows.Forms.Button? btnPrevious;
        private System.Windows.Forms.Button? btnNext;
        private System.Windows.Forms.Label? lblStatus;
        private System.Windows.Forms.Button? btnClose;
    }
}
