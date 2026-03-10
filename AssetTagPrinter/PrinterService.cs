using System;
using Microsoft.PointOfService;

namespace AssetTagPrinter
{
    public class PrinterService
    {
        private PosPrinter _printer;
        private PosExplorer _posExplorer;

        public PrinterService()
        {
            _posExplorer = new PosExplorer();
            DeviceInfo printerDevice = _posExplorer.GetDevice(DeviceType.PosPrinter);
            if (printerDevice == null)
            {
                throw new Exception("No POS printer found.");
            }

            _printer = (PosPrinter)_posExplorer.CreateInstance(printerDevice);
        }

        public void Open()
        {
            _printer.Open();
            _printer.Claim(1000);
            _printer.DeviceEnabled = true;
        }

        public void PrintAssetTag(Asset asset)
        {
            string normal = "\x1b|N";
            string center = "\x1b|cA";
            string bold = "\x1b|bC";
            string cut = "\x1b|fP";

            // Header with logo
            _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}[Your Logo - tiny]\n");

            // Barcode as text (truncated to fit)
            string barcodeText = TruncateBarcodeForPrint(asset.Barcode, 20);
            _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}{barcodeText}\n");

            // High density note
            _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}(High density)\n");

            // Print actual barcode
            _printer.PrintBarCode(PrinterStation.Receipt, asset.Barcode, BarCodeSymbology.Code128, 80, _printer.RecLineWidth, PosPrinter.PrinterBarCodeCenter, BarCodeTextPosition.Below);

            // ID line (truncated to fit)
            string refText = TruncateBarcodeForPrint(asset.Ref, 18);
            _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}{bold}**ID: {refText}**\n");

            // Label line (if needed)
            if (!string.IsNullOrEmpty(asset.Label))
            {
                string labelText = TruncateBarcodeForPrint(asset.Label, 20);
                _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}{labelText}\n");
            }

            _printer.PrintNormal(PrinterStation.Receipt, $"\n{cut}");

            Console.WriteLine($"Printed asset tag for: {asset.Label}");
        }

        private string TruncateBarcodeForPrint(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length > maxLength)
                return text.Substring(0, maxLength - 2) + "..";

            return text;
        }

        public void Close()
        {
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
