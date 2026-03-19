namespace AssetTagPrinter
{
    public class Asset
    {
        public int Id { get; set; }
        public string Ref { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public string AcquisitionDate { get; set; } = string.Empty;

        /// <summary>
        /// Formats the AcquisitionDate to display as "MMM YYYY" (e.g., "Jan 2024")
        /// </summary>
        public string AcquisitionDateDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AcquisitionDate))
                    return string.Empty;

                // Try to parse as a full date
                if (DateTime.TryParse(AcquisitionDate, out var date))
                {
                    return date.ToString("MMM yyyy");
                }

                // If already in "YYYY, MM" or similar format, try to extract and format it
                if (System.Text.RegularExpressions.Regex.IsMatch(AcquisitionDate, @"^\d{4}"))
                {
                    if (int.TryParse(AcquisitionDate.Substring(0, 4), out var year))
                    {
                        // Extract month if available (format like "2024, 05" or "2024-05")
                        var monthMatch = System.Text.RegularExpressions.Regex.Match(AcquisitionDate, @"[,\-/]\s*(\d{1,2})");
                        if (monthMatch.Success && int.TryParse(monthMatch.Groups[1].Value, out var month) && month > 0 && month <= 12)
                        {
                            try
                            {
                                var formattedDate = new DateTime(year, month, 1);
                                return formattedDate.ToString("MMM yyyy");
                            }
                            catch
                            {
                                return $"{year}";
                            }
                        }

                        return $"{year}";
                    }
                }

                return AcquisitionDate;
            }
        }
    }
}
