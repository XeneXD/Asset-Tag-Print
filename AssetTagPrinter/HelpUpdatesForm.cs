using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public class HelpUpdatesForm : Form
    {
        // Cached help text loaded once at startup
        private static string? _cachedHowToText;
        private static string? _cachedInstallationText;
        private static string? _cachedUpdateLogText;

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
            tabHowTo.Controls.Add(CreateReadOnlyTextBox(LoadTextFromFile("HowToUse.txt", ref _cachedHowToText) ?? string.Empty));

            var tabInstall = new TabPage("Installation & Setup");
            tabInstall.Controls.Add(CreateReadOnlyTextBox(LoadTextFromFile("Installation.txt", ref _cachedInstallationText) ?? string.Empty));

            var tabUpdates = new TabPage("Update log");
            tabUpdates.Controls.Add(CreateReadOnlyTextBox(LoadTextFromFile("UpdateLog.txt", ref _cachedUpdateLogText) ?? string.Empty));

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
                return Application.ProductVersion ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        /// <summary>
        /// Loads help text from external .txt files. Caches them in memory for subsequent calls.
        /// </summary>
        private static string LoadTextFromFile(string filename, ref string? cache)
        {
            // Return cached value if already loaded
            if (!string.IsNullOrEmpty(cache))
                return cache;

            try
            {
                // Try to load from Resources folder relative to executable
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(exePath, "Resources", filename);

                if (!File.Exists(filePath))
                {
                    // Try relative to current directory
                    filePath = Path.Combine("Resources", filename);
                }

                if (File.Exists(filePath))
                {
                    cache = File.ReadAllText(filePath);
                    return cache;
                }

                // Fallback message if file not found
                return $"Error: Could not load {filename}. Please ensure the Resources folder contains {filename}.";
            }
            catch (Exception ex)
            {
                return $"Error loading {filename}: {ex.Message}";
            }
        }
    }
}