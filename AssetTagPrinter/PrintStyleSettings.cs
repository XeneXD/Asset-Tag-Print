using System.Drawing;

namespace AssetTagPrinter
{
    public sealed class PrintStyleSettings
    {
        public TextSectionStyle Header { get; set; } = new TextSectionStyle("Consolas", 10f, FontStyle.Bold);
        public TextSectionStyle Secondary { get; set; } = new TextSectionStyle("Consolas", 8f, FontStyle.Regular);
        public TextSectionStyle Body { get; set; } = new TextSectionStyle("Consolas", 9f, FontStyle.Regular);
        public float LeftMargin { get; set; } = 10f;
        public float TopMargin { get; set; } = 10f;
        public float ExtraLineSpacing { get; set; } = 2f;

        public PrintStyleSettings Clone()
        {
            return new PrintStyleSettings
            {
                Header = Header.Clone(),
                Secondary = Secondary.Clone(),
                Body = Body.Clone(),
                LeftMargin = LeftMargin,
                TopMargin = TopMargin,
                ExtraLineSpacing = ExtraLineSpacing
            };
        }

        public static PrintStyleSettings CreateDefault()
        {
            return new PrintStyleSettings();
        }
    }

    public sealed class TextSectionStyle
    {
        public TextSectionStyle(string fontFamily, float size, FontStyle style)
        {
            FontFamily = fontFamily;
            Size = size;
            Style = style;
        }

        public string FontFamily { get; set; }
        public float Size { get; set; }
        public FontStyle Style { get; set; }

        public TextSectionStyle Clone()
        {
            return new TextSectionStyle(FontFamily, Size, Style);
        }

        public Font CreateFont()
        {
            try
            {
                return new Font(FontFamily, Size, Style);
            }
            catch
            {
                return new Font("Consolas", Size, Style);
            }
        }
    }
}