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
        private const int FeedLinesBeforeBetweenTagCut = 4;
        private const int BetweenTagCutPercentage = 25;

        private PosPrinter? _printer;
        private PosExplorer? _posExplorer;
        private string? _windowsPrinterName;
        private bool _useWindowsPrinter;

        public PrintStyleSettings StyleSettings { get; set; } = PrintStyleSettings.CreateDefault();

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
                if (!string.IsNullOrWhiteSpace(windowsPrinter))
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

        public static string GetPrinterDiagnosticsReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("POS Device Diagnostics");
            sb.AppendLine("----------------------");
            string posConfigPath = @"C:\ProgramData\Microsoft\Point Of Service\Configuration\Configuration.xml";
            sb.AppendLine($"POS config file: {(System.IO.File.Exists(posConfigPath) ? "Found" : "Missing")} ({posConfigPath})");
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

        private static string GetRootMessage(Exception ex)
        {
            Exception current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }

        public PrinterService()
        {
            try
            {
                _posExplorer = new PosExplorer();
                _windowsPrinterName = FindPreferredWindowsPrinterName();
                DeviceInfo? printerDevice = FindPreferredPrinterDevice(_posExplorer);

                if (printerDevice != null)
                {
                    _printer = (PosPrinter)_posExplorer.CreateInstance(printerDevice);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_windowsPrinterName))
                {
                    _useWindowsPrinter = true;
                    return;
                }

                throw new Exception("No supported printer found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to initialize POS printer: {GetRootMessage(ex)}", ex);
            }
        }

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
            string barcodeValue = (asset.Barcode ?? string.Empty).Trim();
            int receiptWidth = GetReceiptTextWidth();
            var receiptLines = TagLayoutFormatter.BuildPosReceiptLines(asset, receiptWidth);

            // Try to render the header/details/ID region as a bitmap so we can control fonts
            try
            {
                int bitmapWidth = GetPreferredBitmapWidthPixels();
                using (var bmp = RenderReceiptBitmap(asset, StyleSettings ?? PrintStyleSettings.CreateDefault(), bitmapWidth))
                {
                    try
                    {
                        _printer.PrintMemoryBitmap(PrinterStation.Receipt, bmp, bitmapWidth, PosPrinter.PrinterBitmapCenter);
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
                    _printer.PrintBarCode(PrinterStation.Receipt, barcodeValue, BarCodeSymbology.Code128, 80, 2, PosPrinter.PrinterBarCodeCenter, BarCodeTextPosition.Below);
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

        private Bitmap RenderReceiptBitmap(Asset asset, PrintStyleSettings settings, int bitmapWidth)
        {
            var lines = TagLayoutFormatter.BuildPosReceiptLines(asset, GetReceiptTextWidth());

            using (var measureBmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(measureBmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                using (Font header = settings.Header.CreateFont())
                using (Font secondary = settings.Secondary.CreateFont())
                using (Font body = settings.Body.CreateFont())
                {
                    float totalHeight = 4f; // small top padding
                    float[] lineHeights = new float[lines.Count];

                    for (int i = 0; i < lines.Count; i++)
                    {
                        Font f = GetLineFont(i, header, secondary, body);
                        var size = g.MeasureString(lines[i], f, bitmapWidth);
                        lineHeights[i] = size.Height;
                        totalHeight += size.Height + settings.ExtraLineSpacing;
                    }

                    int bmpHeight = Math.Max(32, (int)Math.Ceiling(totalHeight) + 4);
                    var bmp = new Bitmap(bitmapWidth, bmpHeight);
                    using (var gfx = Graphics.FromImage(bmp))
                    {
                        gfx.Clear(Color.White);
                        gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        float y = 2f;
                        for (int i = 0; i < lines.Count; i++)
                        {
                            string line = lines[i];
                            Font fRef = GetLineFont(i, header, secondary, body);
                            var sz = gfx.MeasureString(line, fRef, bitmapWidth);
                            float x = (bitmapWidth - sz.Width) / 2f;
                            gfx.DrawString(line, fRef, Brushes.Black, x, y);
                            y += sz.Height + settings.ExtraLineSpacing;
                        }
                    }

                    return bmp;
                }
            }
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

                document.PrintController = new StandardPrintController();
                document.PrintPage += (sender, e) =>
                {
                    var settings = StyleSettings?.Clone() ?? PrintStyleSettings.CreateDefault();
                    using (Font header = settings.Header.CreateFont())
                    using (Font secondary = settings.Secondary.CreateFont())
                    using (Font body = settings.Body.CreateFont())
                    {
                        float y = settings.TopMargin;
                        var lines = TagLayoutFormatter.BuildPosReceiptLines(asset);
                        for (int i = 0; i < lines.Count; i++)
                        {
                            string line = lines[i];
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
