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
        private PosPrinter? _printer;
        private PosExplorer? _posExplorer;
        private string? _windowsPrinterName;
        private bool _useWindowsPrinter;

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
                DeviceInfo? printerDevice = FindPreferredPrinterDevice(_posExplorer);

                if (printerDevice != null)
                {
                    _printer = (PosPrinter)_posExplorer.CreateInstance(printerDevice);
                    return;
                }

                _windowsPrinterName = FindPreferredWindowsPrinterName();
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
            if (_useWindowsPrinter)
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
        }

        public void PrintAssetTag(Asset asset)
        {
            if (_useWindowsPrinter)
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
            var receiptLines = TagLayoutFormatter.BuildPosReceiptLines(asset);

            _printer.PrintNormal(PrinterStation.Receipt, string.Join(nl, receiptLines.Take(5)) + nl);

            // Print actual barcode
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

            _printer.PrintNormal(PrinterStation.Receipt, string.Join(nl, receiptLines.Skip(5)) + nl + nl);

            Console.WriteLine($"Printed asset tag for: {asset.Label}");
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
                    using (Font normal = new Font("Consolas", 9))
                    {
                        float y = 10;
                        float lineHeight = normal.GetHeight(e.Graphics) + 2;
                        foreach (string line in TagLayoutFormatter.BuildPosReceiptLines(asset))
                        {
                            e.Graphics.DrawString(line, normal, Brushes.Black, 10, y);
                            y += lineHeight;
                        }

                        e.HasMorePages = false;
                    }
                };

                document.Print();
            }
        }

        public void Close()
        {
            if (_useWindowsPrinter)
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
    }
}
