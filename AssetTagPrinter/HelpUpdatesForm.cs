using System;
using System.Drawing;
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
            tabHowTo.Controls.Add(CreateReadOnlyTextBox(GetHowToText()));

            var tabUpdates = new TabPage("Update log");
            tabUpdates.Controls.Add(CreateReadOnlyTextBox(GetUpdateLogText()));

            tabs.TabPages.Add(tabHowTo);
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

        private static string GetHowToText()
        {
            return
@"Quick start
1) Click ""Load CSV"" and select your file.
2) Click a row to see the tag preview on the right.
3) Optional: click ""Print Preview"" to review tags one-by-one.
4) Click ""Print Selected (or All)"" to print:
   - If a row is selected, it prints that row.
   - If nothing is selected, it prints all rows.

Keyboard shortcuts
- Ctrl+O: Load CSV
- Ctrl+P: Print selected (or all)
- Ctrl+Shift+P: Print preview
- Ctrl+D: Diagnostics
- F1: Help / Updates
- Ctrl+W: Focus Warehouse filter
- Ctrl+L: Toggle load limit + focus limit value
- Enter: M1-like action on focused filter controls (toggle Limit checkbox, apply Warehouse/Limit)
- Esc: Close this Help window

CSV format (current)
- The app expects a header row, then data rows.
- Header names are used when available (e.g. Ref., Label, Barcode, Default warehouse).
- Fallback for simple files without matching headers:
    0 = Id (must be a number)
    1 = Ref
    2 = Label
    3 = Barcode (optional)

Tips
- If the printer status says USB detected but not installed, install the POS/Windows driver so it appears as a printer queue or POS device.
- If labels look cut off, reduce label text length or adjust the printer's paper/width settings.
- Warehouse dropdown includes a ""(Blank Warehouse)"" option when empty warehouse values exist in the CSV.";
        }

        private static string GetUpdateLogText()
        {
            return
@"Update log
- CSV parsing now supports quoted fields (commas inside text no longer shift columns).
- Warehouse filter now runs before load-limit (limit no longer gate-keeps matching rows).
- Enter key now applies Warehouse/Limit filters for keyboard users.
- Enter key now toggles the Limit checkbox when focused (M1-like behavior).
- Warehouse dropdown now supports filtering rows where warehouse is blank.
- POS/Windows printing support and preview.
- Preview and printing share the same line layout builder (so they match).
- Multi-row selection enabled for printing (select multiple rows to print in order).
- Optional CSV load limit (limit to first N rows for large files).
- Warehouse filter (dropdown based on ""Default warehouse"" column when present).
- Help / Updates window with usage notes and update log.

Planned / upcoming
- Paper/label profiles (sticker sizes, margins, orientation)
- Header-based CSV column mapping (Ref vs Ref., etc.)
- Optional PaperCut integration hooks (silent, best-effort, no prompts)";
        }
    }
}

