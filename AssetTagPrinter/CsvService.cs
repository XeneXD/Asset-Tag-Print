using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetTagPrinter
{
    public class CsvService
    {
        /// <summary>
        /// CSV Format Specification - Column indices for the required format:
        /// Id, Ref, Label, Barcode, Warehouse (optional), AcquisitionDate (optional)
        /// 
        /// Example Header:
        /// Id,Ref,Label,Barcode,Warehouse,AcquisitionDate
        /// 
        /// Whoever generates the CSV must follow this exact format.
        /// </summary>
        private const int ID_IDX = 0;
        private const int REF_IDX = 1;
        private const int LABEL_IDX = 2;
        private const int BARCODE_IDX = 3;
        private const int WAREHOUSE_IDX = 4;
        private const int ACQDATE_IDX = 5;

        private const int MIN_REQUIRED_COLUMNS = 4; // Id, Ref, Label, Barcode are required

        // Define the expected format for clarity
        private static readonly string[] EXPECTED_HEADERS = new[] { "Id", "Ref", "Label", "Barcode", "Warehouse", "AcquisitionDate" };
        private static readonly string EXPECTED_FORMAT = "Id,Ref,Label,Barcode,Warehouse,AcquisitionDate";

        public IEnumerable<Asset> ReadAssets(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("CSV file not found.", filePath);
            }

            var allLines = ReadAllLinesWithEncodingFallback(filePath);
            if (allLines.Length == 0)
            {
                yield break;
            }

            // Validate header
            var headerValues = SplitCsvSimple(allLines[0]);
            ValidateAndReportCsvFormat(headerValues);

            // Parse data rows using the defined column indices
            foreach (var line in allLines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = SplitCsvSimple(line);

                // Skip rows with insufficient columns
                if (values.Length < MIN_REQUIRED_COLUMNS)
                {
                    continue;
                }

                // Parse required Id field
                if (!TryGet(values, ID_IDX, out var idText) || !int.TryParse(idText, out var id))
                {
                    continue;
                }

                yield return new Asset
                {
                    Id = id,
                    Ref = GetOrEmpty(values, REF_IDX),
                    Label = GetOrEmpty(values, LABEL_IDX),
                    Barcode = GetOrEmpty(values, BARCODE_IDX),
                    Warehouse = GetOrEmpty(values, WAREHOUSE_IDX),
                    AcquisitionDate = GetOrEmpty(values, ACQDATE_IDX)
                };
            }
        }

        private static void ValidateAndReportCsvFormat(string[] headerValues)
        {
            if (headerValues.Length < MIN_REQUIRED_COLUMNS)
            {
                throw new InvalidOperationException(
                    $"CSV header has too few columns. Need at least 4, got {headerValues.Length}.\r\n" +
                    $"Use: {EXPECTED_FORMAT}");
            }

            // Trim headers for comparison
            var trimmedHeaders = headerValues.Select(h => (h ?? string.Empty).Trim()).ToArray();

            // Check all required columns and collect errors
            var requiredHeaders = EXPECTED_HEADERS.Take(MIN_REQUIRED_COLUMNS).ToArray();
            var errors = new List<string>();

            for (int i = 0; i < MIN_REQUIRED_COLUMNS; i++)
            {
                if (!trimmedHeaders[i].Equals(requiredHeaders[i], System.StringComparison.Ordinal))
                {
                    errors.Add($"- Column {i + 1}: Change '{trimmedHeaders[i]}' to '{requiredHeaders[i]}'");
                }
            }

            // If there are errors, report them all
            if (errors.Count > 0)
            {
                var errorMessage = "Fix the column names in your CSV:\r\n" + string.Join("\r\n", errors) + 
                                   $"\r\n\r\nCorrect order: {EXPECTED_FORMAT}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static bool TryGet(string[] values, int index, out string text)
        {
            text = string.Empty;
            if (index < 0 || index >= values.Length)
            {
                return false;
            }

            text = (values[index] ?? string.Empty).Trim();
            return true;
        }

        private static string GetOrEmpty(string[] values, int index)
        {
            return TryGet(values, index, out var text) ? text : string.Empty;
        }

        // Keeps existing behavior (no quoted-field parsing) but centralizes it for easier upgrades later.
        private static string[] SplitCsvSimple(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return new[] { string.Empty };
            }

            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            values.Add(current.ToString());
            return values.ToArray();
        }

        private static string[] ReadAllLinesWithEncodingFallback(string filePath)
        {
            var utf8 = File.ReadAllText(filePath, new UTF8Encoding(false));
            if (!utf8.Contains('�'))
            {
                return utf8.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            }

            var shiftJis = Encoding.GetEncoding(932);
            var sjisText = File.ReadAllText(filePath, shiftJis);
            return sjisText.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        }
    }
}
