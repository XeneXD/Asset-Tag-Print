namespace AssetTagPrinter
{
    public class Asset
    {
        public int Id { get; set; }
        public string Ref { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
    }
}
