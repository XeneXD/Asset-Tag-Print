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

            _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}Your Logo - tiny\n");

            // Print barcode
            _printer.PrintBarcode(PrinterStation.Receipt, asset.Barcode, BarcodeSymbology.Code128, 100, _printer.RecLineWidth, PosPrinter.PrinterBarCodeLeft, BarcodeTextPosition.Below);

            _printer.PrintNormal(PrinterStation.Receipt, $"{normal}{center}{bold}**ID: {asset.Ref}**\n\n");

            _printer.PrintNormal(PrinterStation.Receipt, $"{cut}");

            Console.WriteLine($"Printed asset tag for: {asset.Label}");
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
