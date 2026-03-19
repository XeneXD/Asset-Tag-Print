using System;
using System.Collections.Generic;

namespace AssetTagPrinter
{
    internal static class TagLayoutFormatter
    {
        public const int PreviewInnerWidth = 20;
        public const int ReceiptWidth = 32;
        private const string CompanyName = "Yoshii Software Solution Philippines";
        private const string CompanyAddress = "602-B Metrobank Plaza Bldg., Osmena Blvd Cebu City";
        private const string CompanyContact = "(032) 254-0302";

        public static string BuildMainPreviewText(Asset asset)
        {
            string barcode = Truncate(asset.Barcode, PreviewInnerWidth);
            string refText = $"ID: {asset.Ref}";
            string label = string.IsNullOrWhiteSpace(asset.Label) ? string.Empty : Truncate(asset.Label, PreviewInnerWidth);

            var lines = new List<string>
            {
                BoxTop(PreviewInnerWidth),
                BoxLine(CompanyName, PreviewInnerWidth),
                BoxLine(CompanyAddress, PreviewInnerWidth),
                BoxLine(CompanyContact, PreviewInnerWidth),
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
            return BuildPosReceiptLines(asset, ReceiptWidth);
        }

        public static IReadOnlyList<string> BuildPosReceiptLines(Asset asset, int receiptWidth)
        {
            int width = Math.Max(24, receiptWidth);
            string refText = Truncate($"ID: {asset.Ref}", width - 2);
            string label = Truncate(asset.Label, width - 2);

            var lines = new List<string>
            {
                Divider('=', width),
                Center(CompanyName, width),
                Center(CompanyAddress, width),
                Center(CompanyContact, width),
                Divider('-', width) + Divider('-', width - 7),
                Center(refText, width)
            };

            if (!string.IsNullOrWhiteSpace(label))
            {
                lines.Add(Center(label, width));
            }

            lines.Add(Divider('=', width));
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
            return (new string(' ', left) + text).PadRight(width);
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
