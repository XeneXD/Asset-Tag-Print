using System;
using System.Drawing;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

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

        public static Bitmap? CreateQrBitmap(string? value, int size)
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
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Width = Math.Max(40, size),
                        Height = Math.Max(40, size),
                        Margin = 1,
                        ErrorCorrection = ErrorCorrectionLevel.L
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