using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Management;
using Microsoft.PointOfService;

namespace AssetTagPrinter
{
    public class PrinterService
    {
        private const int FeedLinesBeforeBetweenTagCut = 0;
        private const int BetweenTagCutPercentage = 25;

        private PosPrinter? _printer;
        private PosExplorer? _posExplorer;
        private string? _windowsPrinterName;
        private bool _useWindowsPrinter;

        public PrintStyleSettings StyleSettings { get; set; } = PrintStyleSettings.CreateDefault();

        /// <summary>
        /// Finds the configured POS printer, then falls back to scanning available devices.
        /// Filters out simulators and prioritizes Epson/TM-T88/M244A models.
        /// </summary>
        private static DeviceInfo? FindPreferredPrinterDevice(PosExplorer explorer)
        {
            try
            {
                DeviceInfo configured = explorer.GetDevice(DeviceType.PosPrinter, "PosPrinter");
                if (configured != null && CanOpenPosDevice(explorer, configured))
                {
                    return configured;
                }
            }
            catch
            {
            }

            var devices = explorer.GetDevices(DeviceType.PosPrinter).Cast<DeviceInfo>().ToList();
            if (devices.Count == 0)
            {
                return null;
            }

            var nonSimulatorDevices = devices
                .Where(d =>
                {
                    var text = $"{d.ServiceObjectName} {d.LogicalNames?.FirstOrDefault()}".ToLowerInvariant();
                    return !text.Contains("simulator");
                })
                .ToList();

            if (nonSimulatorDevices.Count == 0)
            {
                return null;
            }

            var orderedCandidates = nonSimulatorDevices
                .OrderByDescending(d =>
                {
                    var text = $"{d.ServiceObjectName} {d.LogicalNames?.FirstOrDefault()}".ToLowerInvariant();
                    return text.Contains("epson") || text.Contains("tm-t88") || text.Contains("pos-80") || text.Contains("m244a");
                })
                .ToList();

            foreach (var candidate in orderedCandidates)
            {
                if (CanOpenPosDevice(explorer, candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Tests if a POS device can be opened and claimed successfully.
        /// </summary>
        private static bool CanOpenPosDevice(PosExplorer explorer, DeviceInfo device)
        {
            PosPrinter? probe = null;
            try
            {
                probe = (PosPrinter)explorer.CreateInstance(device);
                probe.Open();
                probe.Claim(500);
                probe.DeviceEnabled = true;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (probe != null)
                {
                    try
                    {
                        probe.DeviceEnabled = false;
                        probe.Release();
                        probe.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Scans Windows printer queues for Epson/TM-T88 compatible devices.
        /// </summary>
        private static string? FindPreferredWindowsPrinterName()
        {
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            if (printers.Count == 0)
            {
                return null;
            }

            var preferred = printers.FirstOrDefault(p =>
            {
                string text = p.ToLowerInvariant();
                return text.Contains("epson") || text.Contains("tm-t88") || text.Contains("pos-80") || text.Contains("m244a");
            });

            return preferred;
        }

        /// <summary>
        /// Tests if a Windows printer is actually available/connected by checking its status.
        /// </summary>
        private static bool IsWindowsPrinterAvailable(string printerName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT PrinterStatus, PrinterState FROM Win32_Printer WHERE Name LIKE '%{printerName}%'"))
                {
                    var results = searcher.Get().Cast<ManagementObject>().ToList();
                    if (results.Count == 0)
                    {
                        return false;
                    }

                    foreach (var printer in results)
                    {
                        // PrinterStatus: 1=Other, 2=Unknown, 3=Idle, 4=Printing, 5=WarmingUp, 10=Stopped
                        // We consider Idle (3) and Printing (4) as available, others as unavailable
                        object? statusObj = printer["PrinterStatus"];
                        if (statusObj == null)
                            continue;

                        if (uint.TryParse(statusObj.ToString(), out uint status))
                        {
                            // Status 3 = Idle (ready), Status 4 = Printing (busy but working)
                            if (status == 3 || status == 4)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tests printer availability via POS device or Windows printer queue.
        /// Returns status with device details when available.
        /// </summary>
        public static bool TryGetPrinterStatus(out string status)
        {
            status = "Not connected";
            PosPrinter? probePrinter = null;

            try
            {
                PosExplorer explorer = new PosExplorer();
                DeviceInfo? printerDevice = FindPreferredPrinterDevice(explorer);

                if (printerDevice != null)
                {
                    probePrinter = (PosPrinter)explorer.CreateInstance(printerDevice);
                    probePrinter.Open();
                    probePrinter.Claim(1000);
                    probePrinter.DeviceEnabled = true;

                    status = "Ready";
                    return true;
                }

                string? windowsPrinter = FindPreferredWindowsPrinterName();
                if (!string.IsNullOrWhiteSpace(windowsPrinter) && IsWindowsPrinterAvailable(windowsPrinter))
                {
                    status = $"Ready (Windows: {windowsPrinter})";
                    return true;
                }

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_PnPEntity WHERE Name LIKE '%EPSON%' OR Name LIKE '%TM-T88%' OR Name LIKE '%M244A%'") )
                    {
                        var deviceName = searcher.Get().Cast<ManagementObject>()
                            .Select(m => m["Name"]?.ToString())
                            .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));

                        if (!string.IsNullOrWhiteSpace(deviceName))
                        {
                            status = $"USB detected ({deviceName}) but not installed as POS/Windows printer";
                            return false;
                        }
                    }
                }
                catch
                {
                }

                status = "No supported printer found";
                return false;
            }
            catch (PosControlException ex)
            {
                status = $"Detected but unavailable ({ex.Message})";
                return false;
            }
            catch (Exception ex)
            {
                status = $"POS service error ({GetRootMessage(ex)})";
                return false;
            }
            finally
            {
                if (probePrinter != null)
                {
                    try
                    {
                        probePrinter.DeviceEnabled = false;
                        probePrinter.Release();
                        probePrinter.Close();
                    }
                    catch
                    {
                    }
                }
            }
            }

        /// <summary>
        /// Generates comprehensive diagnostics for POS/Windows printer configuration.
        /// Lists available devices, explains configuration issues, and provides setup guidance.
        /// </summary>
        public static string GetPrinterDiagnosticsReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("POS Device Diagnostics");
            sb.AppendLine("----------------------");
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pdPath = System.IO.Path.Combine(programData, "Microsoft", "Point Of Service", "Configuration", "Configuration.xml");
            sb.AppendLine($"POS config file: {(System.IO.File.Exists(pdPath) ? "Found" : "Missing")} ({pdPath})");
            string posConfigPath = pdPath;
            sb.AppendLine();

            try
            {
                PosExplorer explorer = new PosExplorer();
                var devices = explorer.GetDevices(DeviceType.PosPrinter).Cast<DeviceInfo>().ToList();

                if (devices.Count == 0)
                {
                    sb.AppendLine("POS printers: none");
                }
                else
                {
                    sb.AppendLine($"POS printers found: {devices.Count}");
                    int shown = 0;
                    foreach (var d in devices)
                    {
                        if (shown >= 25)
                        {
                            sb.AppendLine("- ... (more not shown)");
                            break;
                        }

                        string logical = d.LogicalNames != null ? string.Join(", ", d.LogicalNames) : "(none)";
                        bool openable = CanOpenPosDevice(explorer, d);
                        sb.AppendLine($"- {d.ServiceObjectName} | Logical: {logical} | Openable: {(openable ? "Yes" : "No")}");
                        shown++;
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"POS enumeration error: {GetRootMessage(ex)}");
            }

            sb.AppendLine();
            sb.AppendLine("Windows Printer Queues");
            sb.AppendLine("----------------------");

            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            if (printers.Count == 0)
            {
                sb.AppendLine("Windows printers: none");
            }
            else
            {
                foreach (var p in printers)
                {
                    sb.AppendLine($"- {p}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("USB/PNP Printer Devices (Device Manager)");
            sb.AppendLine("----------------------------------------");
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_PnPEntity WHERE Name LIKE '%EPSON%' OR Name LIKE '%TM-T88%' OR Name LIKE '%M244A%'") )
                {
                    var results = searcher.Get().Cast<ManagementObject>().ToList();
                    if (results.Count == 0)
                    {
                        sb.AppendLine("No matching Epson/TM-T88/M244A PnP devices found.");
                    }
                    else
                    {
                        foreach (var item in results)
                        {
                            sb.AppendLine($"- {item["Name"]}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"PnP enumeration error: {GetRootMessage(ex)}");
            }

            if (!sb.ToString().Contains("TM-T88V") && !sb.ToString().Contains("EPSON") && printers.Count == 0)
            {
                sb.AppendLine();
                sb.AppendLine("Hint: Device Manager detection alone is not enough. Install Epson Advanced Printer Driver (APD) or OPOS ADK so the device appears as a Windows printer queue or POS logical device.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Extracts the innermost exception message for clearer error reporting.
        /// </summary>
        private static string GetRootMessage(Exception ex)
        {
            Exception current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }

        /// <summary>
        /// Initializes printer service with POS Explorer, favoring Windows printer (APD) for font rendering.
        /// Falls back to POS logical device if available.
        /// </summary>
        public PrinterService()
        {
            try
            {
                _posExplorer = new PosExplorer();
                _windowsPrinterName = FindPreferredWindowsPrinterName();

                if (!string.IsNullOrWhiteSpace(_windowsPrinterName))
                {
                    _useWindowsPrinter = true;
                    return;
                }

                DeviceInfo? printerDevice = FindPreferredPrinterDevice(_posExplorer);
                if (printerDevice != null)
                {
                    _printer = (PosPrinter)_posExplorer.CreateInstance(printerDevice);
                    return;
                }

                throw new Exception("No supported printer found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to initialize POS printer: {GetRootMessage(ex)}", ex);
            }
        }

        /// <summary>
        /// Opens POS printer device and claims it for exclusive use. No-op if using Windows printer.
        /// </summary>
        public void Open()
        {
            if (_useWindowsPrinter || ShouldUseWindowsStyledRendering())
            {
                return;
            }

            if (_printer == null)
            {
                throw new Exception("No POS printer initialized.");
            }

            _printer.Open();
            _printer.Claim(1000);
            _printer.DeviceEnabled = true;

            // Process print commands synchronously to avoid delayed queued output.
            _printer.AsyncMode = false;
        }

        public void PrintAssetTag(Asset asset)
        {
            if (_useWindowsPrinter || ShouldUseWindowsStyledRendering())
            {
                PrintAssetTagWithWindowsPrinter(asset);
                return;
            }

            if (_printer == null)
            {
                throw new Exception("No POS printer initialized.");
            }

            string nl = "\r\n";
            string barcodeValue = asset.Barcode ?? string.Empty;
            int receiptWidth = GetReceiptTextWidth();
            var receiptLines = TagLayoutFormatter.BuildPosReceiptLines(asset, receiptWidth);

            // Try to render the header/details/ID region as a bitmap so we can control fonts
            try
            {
                int bitmapWidth = GetPreferredBitmapWidthPixels();
                using (var bmp = RenderReceiptBitmap(asset, StyleSettings ?? PrintStyleSettings.CreateDefault(), bitmapWidth, barcodeValue))
                {
                    try
                    {
                        _printer.PrintMemoryBitmap(PrinterStation.Receipt, bmp, bitmapWidth, PosPrinter.PrinterBitmapCenter);
                        // Ensure a small feed so following barcode prints in the expected area
                        try { _printer.PrintNormal(PrinterStation.Receipt, "\r\n"); } catch { }
                    }
                    catch (NotImplementedException)
                    {
                        // Fallback to printing as text if PrintMemoryBitmap isn't supported
                        _printer.PrintNormal(PrinterStation.Receipt, string.Join(nl, receiptLines.Take(4)) + nl);
                    }
                }
            }
            catch
            {
                // On any failure, fall back to normal text printing for the header region
                try { _printer.PrintNormal(PrinterStation.Receipt, string.Join(nl, receiptLines.Take(4)) + nl); } catch { }
            }

            // Print actual barcode (use native barcode if available)
            if (!string.IsNullOrWhiteSpace(barcodeValue))
            {
                try
                {
                    if (_printer.CapRecBarCode)
                    {
                        _printer.PrintBarCode(
                            PrinterStation.Receipt,
                            barcodeValue,
                            BarCodeSymbology.Code128,
                            90,
                            2,
                            PosPrinter.PrinterBarCodeCenter,
                            BarCodeTextPosition.None);
                    }
                    else
                    {
                        _printer.PrintNormal(PrinterStation.Receipt, $"(Barcode unavailable){nl}");
                    }
                }
                catch (NotImplementedException)
                {
                    _printer.PrintNormal(PrinterStation.Receipt, $"(Barcode not implemented by service object){nl}");
                }
                catch (FormatException)
                {
                    _printer.PrintNormal(PrinterStation.Receipt, $"(Invalid barcode format){nl}");
                }
                catch (PosControlException)
                {
                    _printer.PrintNormal(PrinterStation.Receipt, $"(Barcode not supported){nl}");
                }
                catch (Exception)
                {
                    _printer.PrintNormal(PrinterStation.Receipt, $"(Barcode unavailable){nl}");
                }
            }
            else
            {
                _printer.PrintNormal(PrinterStation.Receipt, $"(No barcode){nl}");
            }

            // Print the remaining lines after the barcode as normal text
            _printer.PrintNormal(PrinterStation.Receipt, string.Join(nl, receiptLines.Skip(4)) + nl + nl);

            Console.WriteLine($"Printed asset tag for: {asset.Label}");
        }

        private int GetReceiptTextWidth()
        {
            if (_printer == null)
            {
                return TagLayoutFormatter.ReceiptWidth;
            }

            try
            {
                int deviceWidth = _printer.RecLineChars;
                if (deviceWidth > 0)
                {
                    // Keep width in a practical range for common 58/80mm receipt printers.
                    return Math.Max(TagLayoutFormatter.ReceiptWidth, Math.Min(deviceWidth, 64));
                }
            }
            catch
            {
            }

            return TagLayoutFormatter.ReceiptWidth;
        }

        private int GetPreferredBitmapWidthPixels()
        {
            try
            {
                if (_printer != null)
                {
                    int chars = _printer.RecLineChars;
                    if (chars >= 48) return 576; // wide (80mm / high-density)
                    if (chars >= 36) return 512; // medium
                    if (chars > 0) return 384; // narrow (58mm)
                }
            }
            catch
            {
            }

            return 384;
        }

        private Bitmap RenderReceiptBitmap(Asset asset, PrintStyleSettings settings, int bitmapWidth, string? barcodeValue)
        {
            using (var measureBmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(measureBmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // If the user hasn't customized styles, pick sensible presets based on receipt width
                bool usePresets = IsDefaultStyle(settings);
                float scale = Math.Max(1f, bitmapWidth / 384f);

                string headerFamily = usePresets ? "Arial" : settings.Header.FontFamily;
                float headerSize = usePresets ? Math.Max(10f, 12f * scale) : settings.Header.Size;
                FontStyle headerStyle = usePresets ? FontStyle.Bold : settings.Header.Style;

                string secondaryFamily = usePresets ? "Arial" : settings.Secondary.FontFamily;
                float secondarySize = usePresets ? Math.Max(8f, 9f * scale) : settings.Secondary.Size;
                FontStyle secondaryStyle = usePresets ? FontStyle.Regular : settings.Secondary.Style;

                string bodyFamily = usePresets ? "Arial" : settings.Body.FontFamily;
                float bodySize = usePresets ? Math.Max(8f, 8f * scale) : settings.Body.Size;
                FontStyle bodyStyle = usePresets ? FontStyle.Regular : settings.Body.Style;

                using (Font header = new Font(headerFamily, headerSize, headerStyle))
                using (Font secondary = new Font(secondaryFamily, secondarySize, secondaryStyle))
                using (Font body = new Font(bodyFamily, bodySize, bodyStyle))
                {
                    string headerText = "Yoshii Software Solution Philippines";
                    string addressText = "602-B Metrobank Plaza Bldg., Osmena Blvd Cebu City";
                    string contactText = "(032) 254-0302";
                    string refText = $"ID: {asset.Ref}";
                    string labelText = string.IsNullOrWhiteSpace(asset.Label) ? string.Empty : asset.Label;

                    var blocks = new System.Collections.Generic.List<string> { headerText, addressText, contactText, string.Empty };
                    blocks.Add(string.Empty);
                    blocks.Add(refText);
                    if (!string.IsNullOrWhiteSpace(labelText)) blocks.Add(labelText);

                    float contentWidth = Math.Max(120f, bitmapWidth - 16f);
                    // Minimize top padding to reduce extra whitespace at the top of printed bitmap
                    float totalHeight = 0f;

                    for (int i = 0; i < blocks.Count; i++)
                    {
                        string block = blocks[i];
                        Font baseFont = (i == 0) ? header : (i == 1 || i == 2 ? secondary : body);
                        float fittedSize = GetBestFitSize(g, block, baseFont, contentWidth, 6f);
                        using (Font fitted = new Font(baseFont.FontFamily, fittedSize, baseFont.Style))
                        {
                            var size = g.MeasureString(block, fitted, (int)contentWidth);
                            totalHeight += size.Height + settings.ExtraLineSpacing;
                        }
                    }

                    int bmpHeight = Math.Max(48, (int)Math.Ceiling(totalHeight) + 2);
                    var bmp = new Bitmap(bitmapWidth, bmpHeight);
                    using (var gfx = Graphics.FromImage(bmp))
                    {
                        gfx.Clear(Color.White);
                        gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        // Start drawing at the very top of the bitmap to avoid added whitespace
                        float y = 0f;
                        var sf = new StringFormat(StringFormat.GenericDefault)
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near
                        };

                        for (int i = 0; i < blocks.Count; i++)
                        {
                            string block = blocks[i];
                            Font baseFont = (i == 0) ? header : (i == 1 || i == 2 ? secondary : body);
                            float fittedSize = GetBestFitSize(gfx, block, baseFont, contentWidth, 6f);
                            using (Font fitted = new Font(baseFont.FontFamily, fittedSize, baseFont.Style))
                            {
                                var measured = gfx.MeasureString(block, fitted, (int)contentWidth);
                                var rect = new RectangleF(8f, y, contentWidth, measured.Height);
                                gfx.DrawString(block, fitted, Brushes.Black, rect, sf);
                                y += measured.Height + settings.ExtraLineSpacing;
                            }
                        }
                    }

                    return bmp;
                }
            }
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

        public void CutBetweenTags()
        {
            if (_useWindowsPrinter || ShouldUseWindowsStyledRendering() || _printer == null)
            {
                return;
            }

            try
            {
                // Most thermal printers cut at a fixed position below the print head.
                // Feed extra blank lines so the cut lands below the current label.
                for (int i = 0; i < FeedLinesBeforeBetweenTagCut; i++)
                {
                    _printer.PrintNormal(PrinterStation.Receipt, "\r\n");
                }

                _printer.CutPaper(BetweenTagCutPercentage);
            }
            catch
            {
                // Ignore cut errors so printing can continue.
            }
        }

        private void PrintAssetTagWithWindowsPrinter(Asset asset)
        {
            if (string.IsNullOrWhiteSpace(_windowsPrinterName))
            {
                throw new Exception("No Windows printer selected.");
            }

            using (PrintDocument document = new PrintDocument())
            {
                document.PrinterSettings.PrinterName = _windowsPrinterName;
                if (!document.PrinterSettings.IsValid)
                {
                    throw new Exception($"Windows printer is not available: {_windowsPrinterName}");
                }

                // Apply orientation setting from style settings
                var settings = StyleSettings?.Clone() ?? PrintStyleSettings.CreateDefault();
                document.DefaultPageSettings.Landscape = (settings.Orientation == PrintOrientation.Landscape);

                document.PrintController = new StandardPrintController();
                document.PrintPage += (sender, e) =>
                {
                    var settings = StyleSettings?.Clone() ?? PrintStyleSettings.CreateDefault();
                    string barcodeValue = asset.Barcode ?? string.Empty;
                    
                    // Adjust font sizes based on orientation and auto-scale setting
                    Font header = settings.Header.CreateFont();
                    Font secondary = settings.Secondary.CreateFont();
                    Font body = settings.Body.CreateFont();
                    
                    if (settings.AutoScaleFonts)
                    {
                        float scale = settings.Orientation == PrintOrientation.Landscape ? 1.15f : 1.0f;
                        header = new Font(settings.Header.FontFamily, settings.Header.Size * scale, settings.Header.Style);
                        secondary = new Font(settings.Secondary.FontFamily, settings.Secondary.Size * scale, settings.Secondary.Style);
                        body = new Font(settings.Body.FontFamily, settings.Body.Size * scale, settings.Body.Style);
                    }
                    
                    using (header)
                    using (secondary)
                    using (body)
                    {
                        float y = settings.TopMargin;
                        var lines = TagLayoutFormatter.BuildPosReceiptLines(asset);
                        
                        // Get actual printable page width - this changes based on orientation
                        float pageWidth = e.MarginBounds.Width;
                        float pageHeight = e.MarginBounds.Height;
                        float contentWidth = Math.Max(120f, pageWidth - (settings.LeftMargin + settings.RightMargin));
                        
                        // Log orientation info for debugging
                        System.Diagnostics.Debug.WriteLine($"[Print] Orientation: {settings.Orientation}, IsLandscape: {e.PageSettings.Landscape}, PageWidth: {pageWidth}, ContentWidth: {contentWidth}");

                        if (lines.Count > 0)
                        {
                            e.Graphics.DrawString(lines[0], body, Brushes.Black, settings.LeftMargin, y);
                            y += body.GetHeight(e.Graphics) + settings.ExtraLineSpacing;
                        }

                        // Draw logo instead of company header text
                        y = DrawLogo(e.Graphics, settings, settings.LeftMargin, contentWidth, y);

                        y += 4;
                        int barcodeWidth = (int)Math.Min(260f, Math.Max(160f, contentWidth - 10f));
                        // Reduce QR size to save paper (about 50% of the computed width)
                        const int minQrSize = 48;
                        int qrSize = Math.Max(minQrSize, (int)(barcodeWidth * 0.5f));
                        const float dateGap = 8f;
                        const float minDatePanelWidth = 72f;
                        int maxQrForSidePanel = (int)Math.Floor(Math.Max((float)minQrSize, contentWidth - minDatePanelWidth - dateGap));
                        qrSize = Math.Max(minQrSize, Math.Min(qrSize, maxQrForSidePanel));
                        string acqDateValue = GetAcquisitionDateValue(asset);
                        using (Bitmap? barcode = BarcodeRenderer.CreateQrBitmap(barcodeValue, qrSize))
                        {
                            if (barcode != null)
                            {
                                float contentLeft = settings.LeftMargin;
                                float contentRight = contentLeft + contentWidth;
                                float x = contentLeft;
                                int drawY = Math.Max(0, (int)(y - 2f));
                                float datePanelX = x + barcode.Width + dateGap;
                                float datePanelWidth = Math.Max(0f, contentRight - datePanelX);

                                // Draw at native bitmap size to avoid scaling artifacts that hurt scanning.
                                e.Graphics.DrawImageUnscaled(barcode, (int)x, drawY);
                                float qrBottom = drawY + barcode.Height;
                                float dateBottom = qrBottom;

                                string dateValueText = string.IsNullOrWhiteSpace(acqDateValue) ? "-" : acqDateValue;
                                const string acqLabelText = "Acq Date:";
                                using Font acqLabelBaseFont = new Font(secondary.FontFamily, Math.Max(8f, secondary.Size), FontStyle.Bold);
                                using Font acqValueBaseFont = new Font(body.FontFamily, Math.Max(10f, body.Size * 1.25f), FontStyle.Bold);
                                float acqLabelSize = GetBestFitSize(e.Graphics, acqLabelText, acqLabelBaseFont, Math.Max(24f, datePanelWidth), 6f);
                                float acqValueSize = GetBestFitSize(e.Graphics, dateValueText, acqValueBaseFont, Math.Max(24f, datePanelWidth), 7f);
                                using Font acqLabelFont = new Font(acqLabelBaseFont.FontFamily, acqLabelSize, acqLabelBaseFont.Style);
                                using Font acqValueFont = new Font(acqValueBaseFont.FontFamily, acqValueSize, acqValueBaseFont.Style);

                                float labelHeight = acqLabelFont.GetHeight(e.Graphics);
                                float valueHeight = acqValueFont.GetHeight(e.Graphics);
                                float blockHeight = labelHeight + valueHeight + 1f;
                                float dateY = drawY + Math.Max(0f, (barcode.Height - blockHeight) / 2f);

                                using var acqFormat = new StringFormat(StringFormat.GenericDefault)
                                {
                                    Alignment = StringAlignment.Near,
                                    LineAlignment = StringAlignment.Near,
                                    FormatFlags = StringFormatFlags.NoWrap,
                                    Trimming = StringTrimming.None
                                };

                                e.Graphics.FillRectangle(Brushes.White, datePanelX, dateY, datePanelWidth, blockHeight + 2f);
                                var previousTextHint = e.Graphics.TextRenderingHint;
                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                                e.Graphics.DrawString(acqLabelText, acqLabelFont, Brushes.Black, datePanelX, dateY, acqFormat);
                                float valueY = dateY + labelHeight + 1f;
                                e.Graphics.DrawString(dateValueText, acqValueFont, Brushes.Black, datePanelX, valueY, acqFormat);
                                e.Graphics.TextRenderingHint = previousTextHint;
                                dateBottom = valueY + valueHeight;

                                y = Math.Max(qrBottom, dateBottom) + settings.ExtraLineSpacing;
                            }
                            else
                            {
                                e.Graphics.DrawString("(Barcode unavailable)", secondary, Brushes.Black, settings.LeftMargin, y);
                                y += secondary.GetHeight(e.Graphics) + settings.ExtraLineSpacing + 4;
                            }
                        }

                        for (int i = 4; i < lines.Count; i++)
                        {
                            string line = lines[i];
                            if (line.IndexOf("Acq. Date:", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                continue;
                            }

                            Font lineFont = GetLineFont(i, header, secondary, body);

                            e.Graphics.DrawString(line, lineFont, Brushes.Black, settings.LeftMargin, y);
                            y += lineFont.GetHeight(e.Graphics) + settings.ExtraLineSpacing;
                        }

                        e.HasMorePages = false;
                    }
                };

                document.Print();
            }
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

        private static float DrawLogo(Graphics g, PrintStyleSettings settings, float left, float width, float y)
        {
            try
            {
                Image? logoImage = null;

                // First try embedded resource (Resources.resx)
                try
                {
                    var res = AssetTagPrinter.Properties.Resources.OneLineWithBackground111;
                    if (res != null)
                    {
                        logoImage = new Bitmap(res);
                    }
                }
                catch
                {
                    // ignore resource load errors and fall back to file
                }

                // Fall back to Logo folder files if embedded resource not available
                if (logoImage == null)
                {
                    string exePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    string logoPath = System.IO.Path.Combine(exePath, "Logo", "one_line with background 111.png");

                    if (!System.IO.File.Exists(logoPath))
                    {
                        logoPath = System.IO.Path.Combine("Logo", "one_line with background 111.png");
                    }

                    if (System.IO.File.Exists(logoPath))
                    {
                        logoImage = Image.FromFile(logoPath);
                    }
                }

                if (logoImage != null)
                {
                    using (logoImage)
                    {
                        float maxLogoWidth = width * (settings.LogoMaxWidthPercent / 100f);
                        float scale = logoImage.Width > maxLogoWidth ? maxLogoWidth / logoImage.Width : 1f;
                        int scaledWidth = (int)(logoImage.Width * scale);
                        int scaledHeight = (int)(logoImage.Height * scale);

                        float logoX = left + Math.Max(0f, (width - scaledWidth) / 2f);
                        g.DrawImage(logoImage, logoX, y, scaledWidth, scaledHeight);
                        y += scaledHeight + 5;
                    }
                }
                else
                {
                    // Do not reserve large space when logo is missing for printed output.
                    // Leave `y` unchanged so content starts near the configured TopMargin.
                    // This avoids unnecessary whitespace on receipts when no logo is available.
                }
            }
            catch (Exception ex)
            {
                g.DrawString($"[Logo error: {ex.Message}]", new Font("Arial", 7), Brushes.Red, left, y);
                y += 15;
            }

            return y;
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

        private static string GetAcquisitionDateValue(Asset asset)
        {
            string source = !string.IsNullOrWhiteSpace(asset.AcquisitionDate)
                ? asset.AcquisitionDate.Trim()
                : (asset.AcquisitionDateDisplay ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var monthYearShort = System.Text.RegularExpressions.Regex.Match(source, @"^(?<month>\d{1,2})\s*[/\-]\s*(?<year>\d{2})$");
            if (monthYearShort.Success
                && int.TryParse(monthYearShort.Groups["month"].Value, out var m0)
                && int.TryParse(monthYearShort.Groups["year"].Value, out var y0)
                && m0 >= 1
                && m0 <= 12)
            {
                return $"{m0:D2}/{y0:D2}";
            }

            if (DateTime.TryParse(source, out var parsedDate))
            {
                return parsedDate.ToString("MM/yy");
            }

            var yearMonth = System.Text.RegularExpressions.Regex.Match(source, @"^(?<year>\d{4})\s*[,/\-]\s*(?<month>\d{1,2})$");
            if (yearMonth.Success
                && int.TryParse(yearMonth.Groups["year"].Value, out var y1)
                && int.TryParse(yearMonth.Groups["month"].Value, out var m1)
                && m1 >= 1
                && m1 <= 12)
            {
                return $"{m1:D2}/{(y1 % 100):D2}";
            }

            var monthYear = System.Text.RegularExpressions.Regex.Match(source, @"^(?<month>\d{1,2})\s*[/\-]\s*(?<year>\d{2,4})$");
            if (monthYear.Success
                && int.TryParse(monthYear.Groups["month"].Value, out var m2)
                && int.TryParse(monthYear.Groups["year"].Value, out var y2)
                && m2 >= 1
                && m2 <= 12)
            {
                int twoDigitYear = y2 % 100;
                return $"{m2:D2}/{twoDigitYear:D2}";
            }

            return source;
        }

        public void Close()
        {
            if (_useWindowsPrinter || ShouldUseWindowsStyledRendering())
            {
                return;
            }

            if (_printer != null)
            {
                try
                {
                    _printer.DeviceEnabled = false;
                    _printer.Release();
                    _printer.Close();
                }
                catch (PosControlException)
                {
                    // Ignore exceptions on close
                }
            }
        }

        private bool ShouldUseWindowsStyledRendering()
        {
            if (string.IsNullOrWhiteSpace(_windowsPrinterName))
            {
                return false;
            }

            return !IsDefaultStyle(StyleSettings);
        }

        private static bool IsDefaultStyle(PrintStyleSettings? settings)
        {
            if (settings == null)
            {
                return true;
            }

            var defaults = PrintStyleSettings.CreateDefault();
            return IsSameSection(settings.Header, defaults.Header)
                && IsSameSection(settings.Secondary, defaults.Secondary)
                && IsSameSection(settings.Body, defaults.Body)
                && NearlyEqual(settings.LeftMargin, defaults.LeftMargin)
                && NearlyEqual(settings.TopMargin, defaults.TopMargin)
                && NearlyEqual(settings.ExtraLineSpacing, defaults.ExtraLineSpacing);
        }

        private static bool IsSameSection(TextSectionStyle? a, TextSectionStyle? b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return string.Equals(a.FontFamily, b.FontFamily, StringComparison.OrdinalIgnoreCase)
                && a.Style == b.Style
                && NearlyEqual(a.Size, b.Size);
        }

        private static bool NearlyEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.01f;
        }
    }
}
