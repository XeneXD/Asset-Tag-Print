using System;
using System.Drawing;
using ZXing;
using ZXing.Common;

namespace AssetTagPrinter
{
    internal static class BarcodeRenderer
    {
        public static Bitmap? CreateCode128Bitmap(string? value, int width, int height)
        {
            string data = value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = Math.Max(120, width),
                        Height = Math.Max(40, height),
                        Margin = 2,
                        PureBarcode = true
                    }
                };

                return writer.Write(data);
            }
            catch
            {
                return null;
            }
        }
    }
}