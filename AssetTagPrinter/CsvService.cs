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

            var lines = ReadAllLinesWithEncodingFallback(filePath).Skip(1); // Skip header
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = line.Split(',');
                if (values.Length >= 3 && int.TryParse(values[0], out var id))
                {
                    yield return new Asset
                    {
                        Id = id,
                        Ref = values[1],
                        Label = values[2],
                        Barcode = values.Length >= 4 ? values[3] : string.Empty
                    };
                }
            }
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
