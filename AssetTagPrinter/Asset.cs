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
        /// Formats AcquisitionDate as "YYYY/MM" (e.g., "2024/04") supporting multiple input formats.
        /// Accepts: full dates, "YYYY, MM" format, "YYYY-MM" format, or just year.
        /// </summary>
        public string AcquisitionDateDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AcquisitionDate))
                    return string.Empty;

                if (DateTime.TryParse(AcquisitionDate, out var date))
                {
                    return date.ToString("yyyy/MM");
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(AcquisitionDate, @"^\d{4}"))
                {
                    if (int.TryParse(AcquisitionDate.Substring(0, 4), out var year))
                    {
                        var monthMatch = System.Text.RegularExpressions.Regex.Match(AcquisitionDate, @"[,\-/]\s*(\d{1,2})");
                        if (monthMatch.Success && int.TryParse(monthMatch.Groups[1].Value, out var month) && month > 0 && month <= 12)
                        {
                            try
                            {
                                var formattedDate = new DateTime(year, month, 1);
                                return formattedDate.ToString("yyyy/MM");
                            }
                            catch
                            {
                                return $"{year}/01";
                            }
                        }

                        return $"{year}/01";
                    }
                }

                return AcquisitionDate;
            }
        }
    }
}
