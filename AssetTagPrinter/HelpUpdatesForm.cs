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

            var tabInstall = new TabPage("Installation & Setup");
            tabInstall.Controls.Add(CreateReadOnlyTextBox(GetInstallationText()));

            var tabUpdates = new TabPage("Update log");
            tabUpdates.Controls.Add(CreateReadOnlyTextBox(GetUpdateLogText()));

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

        private static string GetInstallationText()
        {
            return
@"Installation & Setup Requirements

System Requirements
- Windows 7 or later
- .NET Framework 4.8 or higher (for .NET Framework builds)
- .NET 8 (for .NET 8 builds with Windows support)
- USB or network printer connection

POS Printer Driver Installation
Asset Tag Printer uses OPOS (Open POS) v1.14+ and optimizes for popular POS receipt printers.

Supported & Preferred Printers
1) Epson TM Series (RECOMMENDED)
   - Preferred Model: TM-T88 (classic, widely used)
   - Modern Versions: TM-T90X, TM-T100, TM-M30 (currently popular)
   - Download TM Printer drivers and utilities
   - Site: https://pos.epson.com/
   - Install TM-series OPOS driver v1.14 or higher

2) Star Micronics (RECOMMENDED)
   - Preferred Model: M244A (widely deployed)
   - Modern Versions: mPOP, SM-T11, SM-L300 (currently popular)
   - Download StarPRNT or StarIO OPOS drivers
   - Site: https://star-m.jp/
   - Install OPOS v1.14 or higher

3) Other POS Receipt Printers
   - Zebra (Link-OS): https://www.zebra.com/
   - NCR: https://www.ncr.com/
   - Others: Check your printer manufacturer's support site
   - Ensure OPOS v1.14+ drivers are available for your model

Installation Steps
1) Download the appropriate OPOS driver from your printer manufacturer
2) Run the driver installer and follow manufacturer instructions
3) Install vendor-provided USB or network drivers for your printer
4) Verify printer appears in Device Manager
5) Add printer via Settings > Devices > Add a printer
6) Test printing from Notepad to confirm driver installation

Font & Text Styling
- Application supports TrueType fonts installed on your system
- Font availability depends on your Windows installation
- Some fonts may not render correctly on all POS printers due to hardware limitations
- Test your font choices with the Print Preview feature before bulk printing
- Refer to your printer's documentation for supported font formats

Print Style & Layout Configuration
- Use the Print Style Editor (Ctrl+Shift+E) to configure:
  * Text alignment and positioning
  * Font families and sizes
  * Line spacing and margins
  * Label dimensions
- Save custom styles for reuse
- Printer-specific paper/label widths affect layout rendering
- Consult your printer's APD (Advanced Printer Driver) documentation for:
  * Supported paper sizes and widths
  * Label positioning options
  * DPI and resolution settings

Printer-Specific Setup

Epson TM Series (Classic & Modern)
- Classic: TM-T88 (widely used, reliable)
- Modern: TM-T90X, TM-T100, TM-M30 (newer versions, better features)
- Download from: https://pos.epson.com/
- Install ""TM Printer Utilities"" for configuration
- Configure paper width matching your label stock
- Check APD documentation for font support and code pages
- TM-series supports multiple paper widths (58mm, 80mm for most models)

Star Micronics (Classic & Modern)
- Classic: M244A (proven, widely deployed)
- Modern: mPOP, SM-T11, SM-L300 (newer models with enhanced features)
- Download from: https://star-m.jp/
- Install StarPRNT or StarIO drivers (latest version recommended)
- Configure paper width per your label requirement
- Check APD for supported fonts and DPI settings
- Modern models support improved connectivity and performance

Troubleshooting
- If printer shows ""USB detected but not installed"": Install the OPOS driver from manufacturer
- If labels print but appear cut off: Adjust label width in Print Style settings
- If fonts don't render: Ensure font is installed on system; verify printer supports that font family
- If printer not detected: Ensure OPOS v1.14+ drivers are installed, not just Windows drivers
- Use Print Preview (Ctrl+Shift+P) to test before printing full batches

For Additional Support
- Epson TM Support: https://pos.epson.com/ (downloads, drivers, documentation)
- Star Micronics Support: https://star-m.jp/ (drivers, APD, specifications)
- Check your printer's user manual and APD for specific configuration
- Contact your printer manufacturer for hardware or driver issues";
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

Filtering assets
- Use the ""Category"" dropdown to choose a filter type:
  * Select ""None"" for no filtering (shows all assets)
  * Select ""Warehouse"" to filter by warehouse location
- When you select a category, a second dropdown ""Filter by:"" appears
- Choose the specific value (warehouse name)
- The asset list automatically updates to show matching items
- This progressive filtering is cleaner and more intuitive than separate dropdowns

Keyboard shortcuts
- Ctrl+O: Load CSV
- Ctrl+P: Print selected (or all)
- Ctrl+Shift+P: Print preview
- Ctrl+D: Diagnostics
- F1: Help / Updates
- Ctrl+W: Focus Category filter
- Ctrl+L: Toggle load limit + focus limit value
- Enter: M1-like action on focused filter controls (toggle Limit checkbox, apply filters)
- Esc: Close this Help window

CSV format (current)
- The app expects a header row, then data rows.
- Header names are used when available (e.g. Ref., Label, Barcode, Default warehouse).
- Fallback for simple files without matching headers:
    0 = Id (must be a number)
    1 = Ref
    2 = Label
    3 = Barcode (optional)
    4 = Warehouse (optional)

Tips
- If the printer status says USB detected but not installed, install the POS/Windows driver so it appears as a printer queue or POS device.
- If labels look cut off, reduce label text length or adjust the printer's paper/width settings.
- Warehouse dropdown includes a ""(Blank Warehouse)"" option when empty warehouse values exist in the CSV.
";
        }

        private static string GetUpdateLogText()
        {
            return
@"Update log

Latest Version
- New ""Installation & Setup"" tab with comprehensive printer driver setup instructions
- Added support documentation for Epson TM series (TM-T88, TM-T90X, TM-T100, TM-M30)
- Added support documentation for Star Micronics (M244A, mPOP, SM-T11, SM-L300)
- Installation guide covers OPOS v1.14+ driver setup, APD configuration, and troubleshooting
- Added font styling, print layout, and brand-specific setup instructions
- Included links to official manufacturer driver download sites
- UI Refactoring: Replaced separate Warehouse dropdown with unified Category-based filtering
- New Category dropdown with options: None (all) or Warehouse
- Progressive filter: Second ""Filter by"" dropdown appears/updates based on selected category
- Cleaner, less redundant UI that's more intuitive for asset filtering

Previous Updates
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

