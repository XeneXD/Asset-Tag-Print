using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetTagPrinter
{
    public class CsvService
    {
        /// <summary>
        /// Expected CSV format: Id,Ref,Label,Barcode,Warehouse,AcquisitionDate
        /// Required columns: Id (int), Ref, Label, Barcode
        /// Optional columns: Warehouse, AcquisitionDate
        /// </summary>
        private const int ID_IDX = 0;
        private const int REF_IDX = 1;
        private const int LABEL_IDX = 2;
        private const int BARCODE_IDX = 3;
        private const int WAREHOUSE_IDX = 4;
        private const int ACQDATE_IDX = 5;

        private const int MIN_REQUIRED_COLUMNS = 4;

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

            var assets = new List<Asset>();
            var allErrors = new List<string>();

            // Skip header row and process each data row
            for (int rowIndex = 1; rowIndex < allLines.Length; rowIndex++)
            {
                var line = allLines[rowIndex];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var (asset, errors) = ValidateAndParseAsset(line, rowIndex);
                
                if (errors.Count > 0)
                {
                    allErrors.AddRange(errors);
                }
                else if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            // If there are validation errors, throw them all at once
            if (allErrors.Count > 0)
            {
                var errorMessage = "CSV contains the following errors:\r\n" + 
                                   string.Join("\r\n", allErrors);
                throw new InvalidOperationException(errorMessage);
            }

            // Yield all valid assets
            foreach (var asset in assets)
            {
                yield return asset;
            }
        }

        private (Asset? Asset, List<string> Errors) ValidateAndParseAsset(string line, int rowIndex)
        {
            var errors = new List<string>();
            var values = SplitCsvSimple(line);

            // Validate row has minimum required columns
            if (values.Length < MIN_REQUIRED_COLUMNS)
            {
                errors.Add($"Row {rowIndex}: Insufficient columns (has {values.Length}, needs at least {MIN_REQUIRED_COLUMNS})");
                return (null, errors);
            }

            // Validate and parse required Id field
            if (!TryGet(values, ID_IDX, out var idText) || !int.TryParse(idText, out var id))
            {
                errors.Add($"Row {rowIndex}: Invalid or missing Id");
                return (null, errors);
            }

            // Validate Ref (required)
            if (!TryGet(values, REF_IDX, out var refValue) || string.IsNullOrEmpty(refValue))
            {
                errors.Add($"Row {rowIndex}: Ref is empty (required)");
            }

            // Validate Label (required)
            if (!TryGet(values, LABEL_IDX, out var labelValue) || string.IsNullOrEmpty(labelValue))
            {
                errors.Add($"Row {rowIndex}: Label is empty (required)");
            }

            // Validate Barcode (required)
            if (!TryGet(values, BARCODE_IDX, out var barcodeValue) || string.IsNullOrEmpty(barcodeValue))
            {
                errors.Add($"Row {rowIndex}: Barcode is empty (required)");
            }

            // Validate AcquisitionDate if provided (optional, but must be valid format)
            var acqDateValue = GetOrEmpty(values, ACQDATE_IDX);
            if (!string.IsNullOrEmpty(acqDateValue) && !IsValidDateFormat(acqDateValue))
            {
                errors.Add($"Row {rowIndex}: AcquisitionDate '{acqDateValue}' is not in a valid date format");
            }

            // If there are validation errors, return them
            if (errors.Count > 0)
            {
                return (null, errors);
            }

            // All validations passed, create and return asset
            var asset = new Asset
            {
                Id = id,
                Ref = refValue,
                Label = labelValue,
                Barcode = barcodeValue,
                Warehouse = GetOrEmpty(values, WAREHOUSE_IDX),
                AcquisitionDate = acqDateValue
            };

            return (asset, errors);
        }

        private static bool IsValidDateFormat(string dateString)
        {
            return System.DateTime.TryParse(dateString, out _);
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
