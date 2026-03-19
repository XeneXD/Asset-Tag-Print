using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetTagPrinter
{
    public class CsvService
    {
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

            // Header-aware parsing to support different CSV exports (e.g. "Ref." and "Default warehouse").
            var headerValues = SplitCsvSimple(allLines[0]);
            var headerIndex = headerValues
                .Select((name, idx) => new { name = (name ?? string.Empty).Trim(), idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.name))
                .GroupBy(x => x.name, System.StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().idx, System.StringComparer.OrdinalIgnoreCase);

            int idIndex = GetIndex(headerIndex, "Id", "ID");
            int refIndex = GetIndex(headerIndex, "Ref", "Ref.", "Reference", "Asset Ref");
            int labelIndex = GetIndex(headerIndex, "Label", "Name", "Product", "Product name");
            int barcodeIndex = GetIndex(headerIndex, "Barcode", "Bar code", "BarCode");
            int warehouseIndex = GetIndex(headerIndex, "Default warehouse", "Warehouse", "Default Warehouse");
            int acquisitionDateIndex = GetIndex(headerIndex, "AcquisitionDate", "Acquisition Date", "Acq. Date");

            // Fallback for the simple 4-column CSV: Id,Ref,Label,Barcode
            if (idIndex < 0) idIndex = 0;
            if (refIndex < 0) refIndex = 1;
            if (labelIndex < 0) labelIndex = 2;
            if (barcodeIndex < 0) barcodeIndex = 3;

            foreach (var line in allLines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = SplitCsvSimple(line);
                if (values.Length <= System.Math.Max(2, idIndex))
                {
                    continue;
                }

                if (!TryGet(values, idIndex, out var idText) || !int.TryParse(idText, out var id))
                {
                    continue;
                }

                yield return new Asset
                {
                    Id = id,
                    Ref = GetOrEmpty(values, refIndex),
                    Label = GetOrEmpty(values, labelIndex),
                    Barcode = GetOrEmpty(values, barcodeIndex),
                    Warehouse = warehouseIndex >= 0 ? GetOrEmpty(values, warehouseIndex) : string.Empty,
                    AcquisitionDate = acquisitionDateIndex >= 0 ? GetOrEmpty(values, acquisitionDateIndex) : string.Empty
                };
            }
        }

        private static int GetIndex(Dictionary<string, int> headerIndex, params string[] names)
        {
            foreach (var name in names)
            {
                if (headerIndex.TryGetValue(name, out var idx))
                {
                    return idx;
                }
            }

            return -1;
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
