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

                    yPos = DrawCenteredLine(g, "Yoshii Software Solution Philippines", headerFont, _styleSettings.LeftMargin, contentWidth, yPos, 6f, _styleSettings.ExtraLineSpacing);
                    yPos = DrawCenteredLine(g, "602-B Metrobank Plaza Bldg., Osmena Blvd Cebu City", secondaryFont, _styleSettings.LeftMargin, contentWidth, yPos, 6f, _styleSettings.ExtraLineSpacing);
                    yPos = DrawCenteredLine(g, "(032) 254-0302", secondaryFont, _styleSettings.LeftMargin, contentWidth, yPos, 6f, _styleSettings.ExtraLineSpacing);

                    yPos += 4;

                    int barcodeWidth = Math.Max(160, previewBitmap.Width - 110);
                    using (Bitmap? barcode = BarcodeRenderer.CreateCode128Bitmap(asset.Barcode, barcodeWidth, 55))
                    {
                        if (barcode != null)
                        {
                            float barcodeX = (previewBitmap.Width - barcode.Width) / 2f;
                            g.DrawImage(barcode, barcodeX, yPos, barcode.Width, barcode.Height);
                            yPos += barcode.Height + _styleSettings.ExtraLineSpacing + 4;
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

        private static float DrawWrappedCenteredBlock(Graphics g, string text, Font font, float left, float width, float y, float extraSpacing)
        {
            foreach (var line in WrapText(g, text, font, width))
            {
                float lineWidth = g.MeasureString(line, font).Width;
                float x = left + Math.Max(0f, (width - lineWidth) / 2f);
                g.DrawString(line, font, Brushes.Black, x, y);
                y += font.GetHeight(g) + extraSpacing;
            }

            return y;
        }

        private static List<string> WrapText(Graphics g, string text, Font font, float maxWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                result.Add(string.Empty);
                return result;
            }

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string current = string.Empty;

            foreach (var word in words)
            {
                string candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
                if (g.MeasureString(candidate, font).Width <= maxWidth)
                {
                    current = candidate;
                    continue;
                }

                if (!string.IsNullOrEmpty(current))
                {
                    result.Add(current);
                }

                current = word;
                while (g.MeasureString(current, font).Width > maxWidth && current.Length > 1)
                {
                    int split = current.Length - 1;
                    while (split > 1 && g.MeasureString(current.Substring(0, split), font).Width > maxWidth)
                    {
                        split--;
                    }

                    result.Add(current.Substring(0, split));
                    current = current.Substring(split);
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                result.Add(current);
            }

            return result;
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
