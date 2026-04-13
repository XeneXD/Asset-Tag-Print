# Asset Tag Printer - Setup Guide


## Overview
Asset Tag Printer is a Windows application for printing barcode asset tags using POS receipt printers. 

**New in 1.6.2:**
- Acquisition date panel beside QR code now uses a two-line format ("Acq Date:" label above MM/YY), with improved font sizing and no truncation.
- QR code and date panel layout rebalanced for better readability and consistency between preview and print.
- Barcode number under QR removed for a cleaner look.
- Toggle between one-line and two-line acquisition date display (see Print Style Editor).

It requires two separate components to be installed:
1. **POS.NET Framework** - for Windows/application layer
2. **Printer Device Drivers** - for hardware communication

---

## Part 1: POS.NET Setup

### What is POS.NET?
POS.NET is the Microsoft Point of Service framework that provides standardized APIs for communication with POS peripherals (printers, scanners, displays, etc.). It's essential for Asset Tag Printer to communicate with your printer hardware.

### Prerequisites
- Windows 7 or later
- .NET Framework 4.8 or higher (or .NET 8 for newer versions)
- Administrator access (recommended for installation)

### Installation Steps

#### Step 1: Download POS.NET Framework
1. Visit the Microsoft Download Center or your vendor's support site
2. Search for "Microsoft Point of Service" or "OPOS 1.14"
3. Download the POS.NET framework installer
4. **Alternative:** Many OPOS driver packages include POS.NET; check your printer driver package first

#### Step 2: Install POS.NET
1. Run the downloaded installer
2. Follow the installation wizard (default settings are fine)
3. Accept the license agreement
4. Choose installation location (default is recommended)
5. Complete the installation
6. **Note:** May require system restart

#### Step 3: Verify Installation
1. Open Windows Settings > Apps > Apps & features
2. Search for "Point of Service" or "POS"
3. Confirm installation is listed
4. Restart your computer if prompted

#### Step 4: Check POS Configuration Directory
1. Open File Explorer
2. Navigate to: `C:\ProgramData\Microsoft\Point Of Service\Configuration\`
3. This directory should exist (created by POS.NET installer)
4. This is where printer configurations are stored

---

## Part 2: Printer Device Setup

### What You Need
- A POS receipt printer (Epson, Star Micronics, Zebra, etc.)
- OPOS drivers (v1.14 or higher) for your specific printer model
- APD (Advanced Printer Driver) documentation for your printer
- USB cable, network connectivity, or serial cable (depending on printer)

### Supported Printers

#### Epson TM Series (RECOMMENDED)
**Classic Model:**
- TM-T88 (widely used, time-proven)

**Modern Models:**
- TM-T90X (improved connectivity)
- TM-T100 (latest generation)
- TM-M30 (compact, portable)

**Download From:** https://pos.epson.com/

**Installation:**
1. Download "TM Printer Utilities" or "TM Series OPOS Driver" (v1.14+)
2. Download the APD (Advanced Printer Driver) documentation
3. Run the driver installer and follow on-screen instructions
4. Install any USB/network drivers for your specific connection type
5. Restart your computer

#### Star Micronics (RECOMMENDED)
**Classic Model:**
- M244A (proven, widely deployed)

**Modern Models:**
- mPOP (mobile, wireless)
- SM-T11 (compact)
- SM-L300 (advanced features)

**Download From:** https://star-m.jp/ (or regional distributor)

**Installation:**
1. Download StarPRNT driver or StarIO OPOS driver (v1.14+)
2. Download the APD (Advanced Printer Driver specification)
3. Run the installer and complete setup
4. Install connectivity drivers (USB/network as needed)
5. Restart your computer

#### Other POS Printers
- **Zebra:** https://www.zebra.com/ (Link-OS drivers)
- **NCR:** https://www.ncr.com/
- **Others:** Check your printer manufacturer's support website

**Requirement:** OPOS v1.14 or higher must be available for your printer model.

### Step-by-Step Device Installation

#### Step 1: Download Drivers
1. Find your printer manufacturer's support website
2. Locate your specific printer model
3. Download:
   - **OPOS Driver** (required) - v1.14 or higher
   - **APD Documentation** (required for setup)
   - **Firmware Updates** (optional, but recommended)
   - **Configuration Utilities** (optional, helpful)

#### Step 2: Install USB/Network Drivers (if needed)
1. If using USB: install the USB device driver first
2. If using network: configure network settings before OPOS driver
3. Follow manufacturer's instructions for connection type

#### Step 3: Install OPOS Driver
1. Run the OPOS driver installer
2. Follow the wizard
3. Complete installation and restart if prompted
4. The installer typically places configuration files in POS.NET directory

#### Step 4: Add Printer to Windows
1. Open **Settings > Devices > Printers & scanners**
2. Click **Add a printer or scanner**
3. Wait for Windows to detect your printer
4. If found, click **Add device**
5. If not found:
   - Ensure cable is connected or network is accessible
   - Try installing vendor's configuration utility first
   - Restart computer and retry

#### Step 5: Verify OPOS Configuration
1. Open File Explorer
2. Navigate to: `C:\ProgramData\Microsoft\Point Of Service\Configuration\`
3. Look for a file named `Configuration.xml` or similar
4. This file defines your printer to the OPOS system
5. If missing or empty, the OPOS driver installer may not have completed successfully

#### Step 6: Test Printing
1. Open Notepad
2. Type: "Test 123"
3. Print to your new POS printer
4. Verify output (text should be visible and clear)
5. If it fails, check:
   - Printer is powered on and connected
   - Paper is loaded
   - OPOS driver is properly installed
   - Run vendor's diagnostic utility

---

## Using APD (Advanced Printer Driver) Documentation

### What is APD?
The APD (Advanced Printer Driver) is manufacturer documentation that specifies your printer's capabilities. It includes critical information for proper setup in Asset Tag Printer.

### Key APD Information to Check

**Paper & Size:**
- Supported paper widths (typically 58mm or 80mm)
- Roll width and length specifications
- Maximum printable area dimensions

**Fonts & Text:**
- Supported internal fonts
- TrueType font compatibility
- Character pitch and point size limits
- Code pages (ASCII, Extended ASCII, unicode support)

**Resolution & Quality:**
- DPI setting (commonly 180 or 203 for thermal)
- Print darkness/density settings
- Thermal vs inkjet capabilities

**Hardware Features:**
- Barcode support (linear, 2D, QR)
- Logo/image storage capacity
- Auto-cutter functionality
- Drawer kick-out ports
- Network vs USB vs Serial capabilities

**Connectivity:**
- Baud rate settings (serial connection)
- IP address configuration (network connection)
- USB device identifiers

### Before Configuring in Asset Tag Printer
1. **Print Test:** Print sample from Notepad first
2. **Check APD:** Verify paper width, fonts, and DPI match your settings
3. **Read APD Examples:** Many APDs include command examples
4. **Note Limitations:** Some printers limit characters per line; APD specifies these
5. **Test Fonts:** Try your planned fonts in Print Preview before bulk printing

---

## Troubleshooting

### "Insufficient columns" Error
- Ensure CSV file has at least 4 columns: Id, Ref, Label, Barcode
- Header row matters less now, but data must be in correct order

### "POS Configuration not found" Warning
- POS.NET may not be installed
- Check: `C:\ProgramData\Microsoft\Point Of Service\Configuration\`
- If missing, reinstall POS.NET framework

### Printer Not Found in Windows
- Check physical connection (USB or network)
- Verify printer is powered on
- Reinstall OPOS driver
- Try vendor's diagnostic/configuration utility
- Restart computer

### Print Job Fails or Prints Garbage
- Verify paper is loaded and correct width
- Check APD for proper DPI settings
- Try printing from Notepad first (tests basic OPOS setup)
- Update OPOS driver to latest version
- Restart spooler service: Open Services, restart "Print Spooler"

### Font Not Rendering Correctly
- Check APD for supported fonts
- Monospace fonts (Courier, Consolas) typically work best
- Test in Print Preview before bulk printing
- Avoid fancy/decorative fonts for receipt printers

### Barcode Not Printing
- Verify barcode format in CSV (BARCODE field)
- Check APD for barcode support
- Ensure printer supports barcode mode
- Test barcode manually in Print Preview

---

## Next Steps

1. **Install POS.NET** (Part 1) if not already installed
2. **Install Printer Drivers** (Part 2) for your specific printer
3. **Verify Setup** - Test printing from Notepad
4. **Launch Asset Tag Printer** - Load a test CSV file
5. **Use Print Style Editor** - Configure label layout based on APD specs
6. **Test Print Preview** - Verify output before printing assets

---

## Questions or Issues?

- Check the **Installation.txt** file for detailed setup help
- Use **Ctrl+D** in Asset Tag Printer for printer diagnostics
- Press **F1** for in-app help and keyboard shortcuts
- Consult your printer's APD documentation for advanced settings

---

## Support Resources

- **Epson:** https://pos.epson.com/
- **Star Micronics:** https://star-m.jp/
- **Zebra:** https://www.zebra.com/
- **Microsoft POS.NET:** Microsoft Support website
- **Asset Tag Printer Diagnostics:** Use Ctrl+D to check your system configuration
