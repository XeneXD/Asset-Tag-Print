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

        /// <summary>
        /// Builds formatted preview text with asset details in a bordered box.
        /// </summary>
        public static string BuildMainPreviewText(Asset asset)
        {
            string barcode = Truncate(asset.Barcode, PreviewInnerWidth);
            string refText = $"{asset.Ref}";
            string acquisitionDate = FormatAcquisitionDate(asset.AcquisitionDate);
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
                BoxLine(refText, PreviewInnerWidth)
            };

            if (!string.IsNullOrWhiteSpace(label))
            {
                lines.Add(BoxLine(label, PreviewInnerWidth));
            }

            lines.Add(BoxLine(acquisitionDate, PreviewInnerWidth));
            lines.Add(BoxBottom(PreviewInnerWidth));
            return string.Join("\r\n", lines);
        }

        /// <summary>
        /// Builds POS receipt layout lines for thermal printer output.
        /// Supports custom receipt width with sensible minimum (24 chars).
        /// </summary>
        public static IReadOnlyList<string> BuildPosReceiptLines(Asset asset)
        {
            return BuildPosReceiptLines(asset, ReceiptWidth);
        }

        public static IReadOnlyList<string> BuildPosReceiptLines(Asset asset, int receiptWidth)
        {
            int width = Math.Max(24, receiptWidth);
            string refText = Truncate($"{asset.Ref}", width - 2);
            string label = Truncate(asset.Label, width - 2);
            string acquisitionDate = FormatAcquisitionDate(asset.AcquisitionDate);

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

            lines.Add(Center(acquisitionDate, width));
            lines.Add(Divider('=', width));
            return lines;
        }

        /// <summary>
        /// Truncates text to maxLength, appending ".." if needed.
        /// </summary>
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

        /// <summary>
        /// Formats acquisition date as "Acq. Date: YYYY/MM" with fallback handling for various input formats.
        /// </summary>
        private static string FormatAcquisitionDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return "Acq. Date: Not Recorded";
            }

            dateString = dateString!.Trim();

            if (DateTime.TryParse(dateString, out var date))
            {
                return $"Acq. Date: {date.Year}/{date.Month:D2}";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(dateString, @"^\d{4}/\d{1,2}$"))
            {
                return $"Acq. Date: {dateString}";
            }

            if (int.TryParse(dateString, out var year) && year >= 1900 && year <= 2100)
            {
                return $"Acq. Date: {year}/01";
            }

            return "Acq. Date: Not Recorded";
        }

        /// <summary>
        /// Pads text to width without truncation (text longer than width is returned as-is).
        /// </summary>
        private static string Pad(string text, int width)
        {
            if (text.Length >= width)
            {
                return text;
            }

            return text.PadRight(width);
        }

        /// <summary>
        /// Centers text within a given width.
        /// </summary>
        private static string Center(string text, int width)
        {
            if (text.Length >= width)
            {
                return text;
            }

            int left = (width - text.Length) / 2;
            return (new string(' ', left) + text).PadRight(width);
        }

        /// <summary>
        /// Creates a horizontal divider line with the specified character.
        /// </summary>
        private static string Divider(char c, int width)
        {
            return new string(c, width);
        }

        // Box drawing: ┌─┐ │ └─┘ ├─┤ (used for preview panel)
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
