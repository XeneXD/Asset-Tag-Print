using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public partial class PrintPreviewForm : Form
    {
        private readonly List<Asset> _assets;
        private readonly PrintStyleSettings _styleSettings;
        private int _currentIndex = 0;

        public PrintPreviewForm(List<Asset> assets, PrintStyleSettings? styleSettings = null)
        {
            InitializeComponent();
            _assets = assets;
            _styleSettings = (styleSettings ?? PrintStyleSettings.CreateDefault()).Clone();
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
                Bitmap previewBitmap = new Bitmap(280, 400);
                using (Graphics g = Graphics.FromImage(previewBitmap))
                {
                    g.Clear(Color.White);
                    g.DrawRectangle(Pens.Black, 0, 0, 279, 399);

                    Brush blackBrush = Brushes.Black;
                    var lines = TagLayoutFormatter.BuildPosReceiptLines(asset);
                    using Font headerFont = _styleSettings.Header.CreateFont();
                    using Font secondaryFont = _styleSettings.Secondary.CreateFont();
                    using Font bodyFont = _styleSettings.Body.CreateFont();

                    float yPos = _styleSettings.TopMargin;
                    float contentWidth = Math.Max(120f, previewBitmap.Width - (_styleSettings.LeftMargin * 2));

                    if (lines.Count > 0)
                    {
                        g.DrawString(lines[0], bodyFont, blackBrush, _styleSettings.LeftMargin, yPos);
                        yPos += bodyFont.GetHeight(g) + _styleSettings.ExtraLineSpacing;
                    }

                    yPos = DrawCenteredLine(g, "Yoshii Software Solution Philippines", headerFont, _styleSettings.LeftMargin, contentWidth, yPos, 9f, _styleSettings.ExtraLineSpacing);
                    yPos = DrawCenteredLine(g, "602-B Metrobank Plaza Bldg., Osmena Blvd Cebu City", secondaryFont, _styleSettings.LeftMargin, contentWidth, yPos, 7f, _styleSettings.ExtraLineSpacing);
                    yPos = DrawCenteredLine(g, "(032) 254-0302", secondaryFont, _styleSettings.LeftMargin, contentWidth, yPos, 7f, _styleSettings.ExtraLineSpacing);

                    yPos += 4;

                    int barcodeWidth = (int)Math.Min(260f, Math.Max(160f, contentWidth - 10f));
                    using (Bitmap? barcode = BarcodeRenderer.CreateCode128Bitmap(asset.Barcode, barcodeWidth, 65))
                    {
                        if (barcode != null)
                        {
                            float barcodeX = (previewBitmap.Width - barcode.Width) / 2f;
                            g.DrawImage(barcode, barcodeX, yPos, barcode.Width, barcode.Height);
                            yPos += barcode.Height + 2;
                            // Draw barcode value text below the barcode, centered
                            float textWidth = g.MeasureString(asset.Barcode, bodyFont).Width;
                            float textX = _styleSettings.LeftMargin + Math.Max(0f, (contentWidth - textWidth) / 2f);
                            g.DrawString(asset.Barcode, bodyFont, blackBrush, textX, yPos);
                            yPos += bodyFont.GetHeight(g) + _styleSettings.ExtraLineSpacing;
                        }
                        else
                        {
                            g.DrawString("(Barcode unavailable)", secondaryFont, blackBrush, _styleSettings.LeftMargin, yPos);
                            yPos += secondaryFont.GetHeight(g) + _styleSettings.ExtraLineSpacing + 4;
                        }
                    }

                    for (int i = 4; i < lines.Count; i++)
                    {
                        Font lineFont = GetLineFont(i, headerFont, secondaryFont, bodyFont);
                        g.DrawString(lines[i], lineFont, blackBrush, _styleSettings.LeftMargin, yPos);
                        yPos += lineFont.GetHeight(g) + _styleSettings.ExtraLineSpacing;
                    }

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

        private static Font GetLineFont(int lineIndex, Font headerFont, Font secondaryFont, Font bodyFont)
        {
            if (lineIndex == 1)
            {
                return headerFont;
            }

            if (lineIndex == 2 || lineIndex == 3)
            {
                return secondaryFont;
            }

            return bodyFont;
        }

        private static float GetBestFitSize(Graphics g, string text, Font baseFont, float maxWidth, float minSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return baseFont.Size;
            }

            float size = baseFont.Size;
            while (size > minSize)
            {
                using (Font probe = new Font(baseFont.FontFamily, size, baseFont.Style))
                {
                    var measured = g.MeasureString(text, probe, int.MaxValue);
                    if (measured.Width <= maxWidth)
                    {
                        break;
                    }
                }

                size -= 0.5f;
            }

            return Math.Max(minSize, size);
        }

        private static float DrawCenteredLine(Graphics g, string text, Font font, float left, float width, float y, float minSize, float extraSpacing)
        {
            if (string.IsNullOrWhiteSpace(text))
                return y;

            float fittedSize = GetBestFitSize(g, text, font, width, minSize);
            using (Font fitted = new Font(font.FontFamily, fittedSize, font.Style))
            {
                float textWidth = g.MeasureString(text, fitted).Width;
                float x = left + Math.Max(0f, (width - textWidth) / 2f);
                g.DrawString(text, fitted, Brushes.Black, x, y);
                y += fitted.GetHeight(g) + extraSpacing;
            }

            return y;
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
