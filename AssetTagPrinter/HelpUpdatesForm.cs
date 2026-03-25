using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public class HelpUpdatesForm : Form
    {
        public HelpUpdatesForm()
        {
            Text = "Help / Updates";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(680, 460);
            KeyPreview = true;

            var lblTitle = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 48,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 12, 0),
                Text = $"Asset Tag Printer — Version {GetAppVersion()}"
            };

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                TabIndex = 0
            };

            var tabHowTo = new TabPage("How to use");
            tabHowTo.Controls.Add(CreateReadOnlyTextBox(Properties.Resources.HowToUse ?? string.Empty));

            var tabInstall = new TabPage("Installation & Setup");
            tabInstall.Controls.Add(CreateReadOnlyTextBox(Properties.Resources.Installation ?? string.Empty));

            var tabUpdates = new TabPage("Update log");
            tabUpdates.Controls.Add(CreateReadOnlyTextBox(Properties.Resources.UpdateLog ?? string.Empty));

            tabs.TabPages.Add(tabHowTo);
            tabs.TabPages.Add(tabInstall);
            tabs.TabPages.Add(tabUpdates);

            var btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 42
            };
            btnClose.Click += (s, e) => Close();
            btnClose.TabIndex = 1;

            Controls.Add(tabs);
            Controls.Add(btnClose);
            Controls.Add(lblTitle);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static Control CreateReadOnlyTextBox(string text)
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = SystemColors.Window,
                Text = text
            };
        }

        private static string GetAppVersion()
        {
            try
            {
                // Matches what Windows displays for "Product Version" in most WinForms apps.
                var version = Application.ProductVersion ?? "unknown";
                // Strip out git commit hash (e.g., "1.1.0+dcd102230..." becomes "1.1.0")
                if (version.Contains("+"))
                {
                    version = version.Substring(0, version.IndexOf("+"));
                }
                return version;
            }
            catch
            {
                return "unknown";
            }
        }

        // Help texts now embedded; no file I/O required.
    }
}