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

OPOS and APD Driver Overview
Asset Tag Printer uses two types of printer drivers:

1) OPOS (Open POS) Drivers v1.14+
   - Primary interface for POS printer communication
   - Required for all supported printers
   - Handles USB and network connections
   - Provides standardized printing commands across printer brands
   - Download from your printer manufacturer's website

2) APD (Advanced Printer Driver) Documentation
   - Contains printer-specific capabilities and settings
   - Defines supported fonts, paper sizes, DPI, and code pages
   - Includes label positioning, margin, and alignment options
   - Specifies thermal vs inkjet printing modes (if supported)
   - Essential for optimizing print layout and formatting
   - Always consult your printer's APD when configuring styles
   - APD typically available on manufacturer support pages alongside OPOS driver

POS Printer Driver Installation
Asset Tag Printer uses OPOS (Open POS) v1.14+ and optimizes for popular POS receipt printers.

Supported & Preferred Printers
1) Epson TM Series (RECOMMENDED)
   - Preferred Model: TM-T88 (classic, widely used)
   - Modern Versions: TM-T90X, TM-T100, TM-M30 (currently popular)
   - Download TM Printer drivers and utilities from https://pos.epson.com/
   - Install TM-series OPOS driver v1.14 or higher
   - Download APD (Advanced Printer Driver documentation) for detailed capabilities
   - APD location: Usually included in driver package or available separately on support page
   - Check APD for: supported fonts, paper widths (58mm, 80mm, etc.), DPI settings, code pages

2) Star Micronics (RECOMMENDED)
   - Preferred Model: M244A (widely deployed)
   - Modern Versions: mPOP, SM-T11, SM-L300 (newer models, enhanced features)
   - Download StarPRNT or StarIO OPOS drivers from https://star-m.jp/
   - Install OPOS v1.14 or higher
   - Download APD (Advanced Printer Driver specification) for model-specific features
   - APD includes: font support, paper width options, thermal print quality settings, network config
   - Reference APD when setting up custom print styles

3) Other POS Receipt Printers
   - Zebra (Link-OS): https://www.zebra.com/
   - NCR: https://www.ncr.com/
   - Others: Check your printer manufacturer's support site
   - Ensure OPOS v1.14+ drivers are available for your model
   - Always download the APD (Advanced Printer Driver documentation) alongside the driver
   - APD is critical for button sizes, layout, and font rendering

Installation Steps
1) Download the OPOS driver AND APD documentation from your printer manufacturer
   - Example: Download ""TM Printer Utilities"" (includes OPOS and documentation)
   - Save the APD documentation for reference during setup
2) Run the OPOS driver installer and follow manufacturer instructions
3) Install vendor-provided USB or network drivers for your printer
4) Verify printer appears in Device Manager
5) Add printer via Settings > Devices > Add a printer
6) Consult the APD documentation to verify:
   - Supported paper widths and sizes
   - Available fonts and code pages
   - DPI and resolution capabilities
   - Special settings (thermal/dark mode, cutting, etc.)
7) Test printing from Notepad to confirm driver installation and basic functionality
8) Use Print Style Editor to configure label layout based on APD specs

APD-Specific Configuration
When configuring the Print Style Editor:
- Consult APD for exact paper width (58mm, 80mm, or custom)
- Verify fonts listed in APD match those used in your labels
- Check APD DPI setting to ensure proper resolution (typically 180 or 203 DPI for thermal)
- Reference APD for margin and positioning recommendations
- Some printers have APD-documented limits on text per line; check before designing labels
- APD often includes sample code or command sets; useful for troubleshooting
- Test with Print Preview (Ctrl+Shift+P) and verify exact rendering before bulk printing

Font & Text Styling
- Application supports TrueType fonts installed on your system
- Font availability depends on your Windows installation
- Some fonts may not render correctly on all POS printers due to hardware limitations
- Test your font choices with the Print Preview feature before bulk printing
- Refer to your printer's APD documentation for supported font formats
- Monospace fonts (Courier, Consolas) typically render best on thermal POS printers
- Check APD for pitch and point-size limitations on your specific printer model

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
Classic: TM-T88 (widely used, reliable)
Modern: TM-T90X, TM-T100, TM-M30 (newer versions, better features)
Download from: https://pos.epson.com/
Install ""TM Printer Utilities"" for configuration
Download the APD (Advanced Printer Driver) specification document
APD Reference checklist for TM series:
   - Default paper width: Check APD (typically 58mm or 80mm)
   - Supported fonts: Consult APD for internal font support
   - DPI setting: Verify in APD (usually 180 or 203 DPI)
   - Code pages: Check APD for character set support (ASCII, Extended ASCII, etc.)
   - Thermal print darkness: APD may have settings for print intensity
   - Cutting/tear-off options: Reference APD for hardware features
   - Max line width in characters: Important for layout design (APD specifies)
- TM-series supports multiple paper widths (58mm, 80mm for most models)
- Refer to TM APD for baud rate and COM port settings if using serial connection
- Print Preview supports APD-based settings verification

Star Micronics (Classic & Modern)
Classic: M244A (proven, widely deployed)
Modern: mPOP, SM-T11, SM-L300 (newer models with enhanced features)
Download from: https://star-m.jp/
Install StarPRNT or StarIO drivers (latest version recommended)
Download the APD (Advanced Printer Driver specification) document
APD Reference checklist for Star printers:
   - Paper width options: Check APD (typically 58mm, 80mm)
   - Font rendering: StarPRNT APD lists supported font families
   - Resolution: Verify DPI in APD (200 DPI common for modern Star models)
   - Connectivity: APD specifies USB, serial, and Ethernet options
   - Print mode: Check APD for thermal or inkjet mode selection
   - Special features: APD documents logo storage, barcode capabilities
   - Command set: StarPRNT uses proprietary commands; APD details these
- Modern models support improved connectivity and performance (compare via APD)
- Reference APD for native barcode support and QR code rendering
- Consult APD for paper sensor and auto-cutter functionality

Zebra & Other Printers
Download from manufacturer site (e.g., https://www.zebra.com/)
Install appropriate OPOS driver (v1.14+)
Download and review the APD documentation
Key APD items for any POS printer:
   - Supported paper widths and label sizes
   - Native font list and TrueType font support
   - DPI and print resolution capabilities
   - Barcode symbologies supported (Code 39, Code 128, QR, etc.)
   - Thermal vs. inkjet printing modes
   - Paper sensor and cutting capabilities
   - Network and serial communication settings
- Always consult APD before configuring custom print styles
- APD is essential for troubleshooting layout and rendering issues

Troubleshooting
- If printer shows ""USB detected but not installed"": Install the OPOS driver from manufacturer
- If labels print but appear cut off: Check APD for maximum line width and adjust label width in Print Style settings
- If fonts don't render: Ensure font is installed on system; consult APD for supported fonts; verify printer supports that font family
- If printer not detected: Ensure OPOS v1.14+ drivers are installed, not just Windows drivers; check APD for connection settings
- If printed text is too small or too large: Check APD DPI setting and font point-size limits; adjust in Print Style Editor
- If barcode doesn't print: Consult printer APD for supported barcode symbologies; verify barcode format is supported
- If characters are garbled: Check APD code page settings; ensure character set matches printer's APD specification
- If labels misalign vertically: Review APD for margin and positioning recommendations; test with Print Preview
- Use Print Preview (Ctrl+Shift+P) to test before printing full batches and verify APD-based rendering

APD Documentation Resources
- Epson TM Series: Included with ""TM Printer Utilities"" or available on https://pos.epson.com/
- Star Micronics: Download with driver or check support page at https://star-m.jp/
- Zebra: Available on https://www.zebra.com/ under technical documents
- Other manufacturers: Check support/downloads section for ""Advanced Printer Driver"" specs

For Additional Support
- Epson TM Support: https://pos.epson.com/ (downloads, drivers, documentation, APD)
- Star Micronics Support: https://star-m.jp/ (drivers, APD, specifications)
- Check your printer's user manual and APD for specific configuration
- APD documentation is critical for resolving layout, font, and rendering issues
- Contact your printer manufacturer for hardware or driver issues (reference APD in support request)";
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

Pagination
- The asset list displays 12 items per page for better performance and usability.
- Use ""< Previous"" and ""Next >"" buttons to navigate through pages.
- Page indicator shows your current position (e.g., ""Page 1 of 5"").
- When you change filters, pagination resets to page 1.
- Pagination works well with ETD systems for batch processing large asset lists.
- Perfect for handling large CSV files with thousands of items.

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
- Pagination handles large datasets efficiently (1000+ items) and integrates seamlessly with ETD systems.
";
        }

        private static string GetUpdateLogText()
        {
            return
@"Update log

Latest Version
- Pagination system: Displays 12 items per page for large asset lists
- Page navigation with Previous/Next buttons and page indicator
- Pagination resets to page 1 when filters change
- Optimized for ETD (Electronic Tag Display) systems and batch processing
- Pagination works well with large CSV files (1000+ items)
- Removed load limit feature in favor of intuitive pagination
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
- Optional PaperCut integration hooks (silent, best-effort, no prompts)
- Additional ETD system integration options";
        }
    }
}

