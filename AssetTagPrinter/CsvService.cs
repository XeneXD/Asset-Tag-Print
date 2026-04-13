using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        private sealed class CsvColumnMap
        {
            public int Id { get; set; } = ID_IDX;
            public int Ref { get; set; } = REF_IDX;
            public int Label { get; set; } = LABEL_IDX;
            public int Barcode { get; set; } = BARCODE_IDX;
            public int Warehouse { get; set; } = WAREHOUSE_IDX;
            public int AcquisitionDate { get; set; } = ACQDATE_IDX;
        }

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
            var columnMap = BuildColumnMap(SplitCsvSimple(allLines[0]));

            // Skip header row and process each data row
            for (int rowIndex = 1; rowIndex < allLines.Length; rowIndex++)
            {
                var line = allLines[rowIndex];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var (asset, errors) = ValidateAndParseAsset(line, rowIndex, columnMap);
                
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

        private (Asset? Asset, List<string> Errors) ValidateAndParseAsset(string line, int rowIndex, CsvColumnMap columnMap)
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
            if (!TryGet(values, columnMap.Id, out var idText) || !int.TryParse(idText, out var id))
            {
                errors.Add($"Row {rowIndex}: Invalid or missing Id");
                return (null, errors);
            }

            // Validate Ref (required)
            if (!TryGet(values, columnMap.Ref, out var refValue) || string.IsNullOrEmpty(refValue))
            {
                errors.Add($"Row {rowIndex}: Ref is empty (required)");
            }

            // Validate Label (required)
            if (!TryGet(values, columnMap.Label, out var labelValue) || string.IsNullOrEmpty(labelValue))
            {
                errors.Add($"Row {rowIndex}: Label is empty (required)");
            }

            // Validate Barcode (required)
            if (!TryGet(values, columnMap.Barcode, out var barcodeValue) || string.IsNullOrEmpty(barcodeValue))
            {
                errors.Add($"Row {rowIndex}: Barcode is empty (required)");
            }

            // Validate AcquisitionDate if provided (optional, but must be valid format)
            var acqDateValue = GetOrEmpty(values, columnMap.AcquisitionDate);
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
                Warehouse = GetOrEmpty(values, columnMap.Warehouse),
                AcquisitionDate = acqDateValue
            };

            return (asset, errors);
        }

        private static bool IsValidDateFormat(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return true;
            }

            string value = dateString.Trim();

            // MM/YY or MM-YY
            if (Regex.IsMatch(value, @"^(0?[1-9]|1[0-2])[/\-]\d{2}$"))
            {
                return true;
            }

            // MM/YYYY or MM-YYYY
            if (Regex.IsMatch(value, @"^(0?[1-9]|1[0-2])[/\-]\d{4}$"))
            {
                return true;
            }

            // YYYY/MM or YYYY-MM
            if (Regex.IsMatch(value, @"^\d{4}[/\-](0?[1-9]|1[0-2])$"))
            {
                return true;
            }

            return System.DateTime.TryParse(value, out _);
        }

        private static CsvColumnMap BuildColumnMap(string[] headers)
        {
            var map = new CsvColumnMap();
            if (headers == null || headers.Length == 0)
            {
                return map;
            }

            var headerIndex = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++)
            {
                string key = NormalizeHeader(headers[i]);
                if (!string.IsNullOrWhiteSpace(key) && !headerIndex.ContainsKey(key))
                {
                    headerIndex[key] = i;
                }
            }

            map.Id = ResolveIndex(headerIndex, map.Id, "id", "assetid");
            map.Ref = ResolveIndex(headerIndex, map.Ref, "ref", "reference", "assetref");
            map.Label = ResolveIndex(headerIndex, map.Label, "label", "name", "assetlabel", "description");
            map.Barcode = ResolveIndex(headerIndex, map.Barcode, "barcode", "barcodeno", "qrcode", "qr", "assetcode");
            map.Warehouse = ResolveIndex(headerIndex, map.Warehouse, "warehouse", "wh", "location");
            map.AcquisitionDate = ResolveIndex(
                headerIndex,
                map.AcquisitionDate,
                "acquisitiondate",
                "acquisition",
                "acqdate",
                "acq",
                "acqdt",
                "aquisitiondate",
                "aquisition"
            );

            return map;
        }

        private static int ResolveIndex(Dictionary<string, int> headerIndex, int fallback, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (headerIndex.TryGetValue(key, out int index))
                {
                    return index;
                }
            }

            return fallback;
        }

        private static string NormalizeHeader(string? header)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                return string.Empty;
            }

            string value = header!;
            var chars = value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray();
            return new string(chars);
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
