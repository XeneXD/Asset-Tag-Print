using System;
using System.Collections.Generic;

namespace AssetTagPrinter
{
    internal static class TagLayoutFormatter
    {
        public const int PreviewInnerWidth = 20;
        public const int ReceiptWidth = 32;

        public static string BuildMainPreviewText(Asset asset)
        {
            string barcode = Truncate(asset.Barcode, PreviewInnerWidth);
            string refText = $"ID: {asset.Ref}";
            string label = string.IsNullOrWhiteSpace(asset.Label) ? string.Empty : Truncate(asset.Label, PreviewInnerWidth);

            var lines = new List<string>
            {
                BoxTop(PreviewInnerWidth),
                BoxLine("[Company Name]", PreviewInnerWidth),
                BoxLine("[Company Address]", PreviewInnerWidth),
                BoxLine("[Company Contact #]", PreviewInnerWidth),
                BoxDivider(PreviewInnerWidth),
                BoxLine(barcode, PreviewInnerWidth),
                /*BoxLine("(High density)", PreviewInnerWidth),*/
                BoxDivider(PreviewInnerWidth),
                BoxLine(refText, PreviewInnerWidth)
            };

            if (!string.IsNullOrWhiteSpace(label))
            {
                lines.Add(BoxLine(label, PreviewInnerWidth));
            }

            lines.Add(BoxBottom(PreviewInnerWidth));
            return string.Join("\r\n", lines);
        }

        public static IReadOnlyList<string> BuildPosReceiptLines(Asset asset)
        {
            string barcode = Truncate(asset.Barcode, ReceiptWidth - 2);
            string refText = Truncate($"ID: {asset.Ref}", ReceiptWidth - 2);
            string label = Truncate(asset.Label, ReceiptWidth - 2);

            var lines = new List<string>
            {
                Divider('=', ReceiptWidth),
                Center("[Company Name", ReceiptWidth),
                Center("[Company Address]", ReceiptWidth),
                Center("[Company Contact #]", ReceiptWidth),
                Divider('-', ReceiptWidth),
                Pad(barcode, ReceiptWidth),
                /*Center("(High density)", ReceiptWidth),*/
                Divider('-', ReceiptWidth),
                Pad(refText, ReceiptWidth)
            };

            if (!string.IsNullOrWhiteSpace(label))
            {
                lines.Add(Pad(label, ReceiptWidth));
            }

            lines.Add(Divider('=', ReceiptWidth));
            return lines;
        }

        private static string Truncate(string? text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string value = text!.Trim();
            if (value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, Math.Max(0, maxLength - 2)) + "..";
        }

        private static string Pad(string text, int width)
        {
            if (text.Length >= width)
            {
                return text;
            }

            return text.PadRight(width);
        }

        private static string Center(string text, int width)
        {
            if (text.Length >= width)
            {
                return text;
            }

            int left = (width - text.Length) / 2;
            return new string(' ', left) + text;
        }

        private static string Divider(char c, int width)
        {
            return new string(c, width);
        }

        private static string BoxTop(int innerWidth)
        {
            return $"┌{new string('─', innerWidth)}┐";
        }

        private static string BoxBottom(int innerWidth)
        {
            return $"└{new string('─', innerWidth)}┘";
        }

        private static string BoxDivider(int innerWidth)
        {
            return $"├{new string('─', innerWidth)}┤";
        }

        private static string BoxLine(string text, int innerWidth)
        {
            string content = Truncate(text, innerWidth).PadRight(innerWidth);
            return $"│{content}│";
        }
    }
}
