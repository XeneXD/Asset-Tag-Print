using System.Drawing;
using System.Drawing.Printing;

namespace AssetTagPrinter
{
    /// <summary>
    /// Page orientation for printed labels
    /// </summary>
    public enum PrintOrientation
    {
        Portrait = 0,
        Landscape = 1
    }

    public sealed class PrintStyleSettings
    {
        // Default fonts changed from Consolas (monospace) to Arial (proportional) for better readability
        public TextSectionStyle Header { get; set; } = new TextSectionStyle("Arial", 11f, FontStyle.Bold);
        public TextSectionStyle Secondary { get; set; } = new TextSectionStyle("Arial", 8f, FontStyle.Bold);
        public TextSectionStyle Body { get; set; } = new TextSectionStyle("Arial", 9f, FontStyle.Regular);
        
        public float LeftMargin { get; set; } = 35f;
        public float TopMargin { get; set; } = 15f;
        public float RightMargin { get; set; } = 12f;
        public float BottomMargin { get; set; } = 12f;
        public float ExtraLineSpacing { get; set; } = 1f;
        
        /// <summary>
        /// Maximum width for logo display as percentage (10-100%)
        /// </summary>
        public float LogoMaxWidthPercent { get; set; } = 80f;
        
        /// <summary>
        /// Page orientation - automatically adjusts layout proportions
        /// </summary>
        public PrintOrientation Orientation { get; set; } = PrintOrientation.Portrait;
        
        /// <summary>
        /// When true, automatically scales fonts based on page orientation and dimensions
        /// </summary>
        public bool AutoScaleFonts { get; set; } = true;

        public PrintStyleSettings Clone()
        {
            return new PrintStyleSettings
            {
                Header = Header.Clone(),
                Secondary = Secondary.Clone(),
                Body = Body.Clone(),
                LeftMargin = LeftMargin,
                TopMargin = TopMargin,
                RightMargin = RightMargin,
                BottomMargin = BottomMargin,
                ExtraLineSpacing = ExtraLineSpacing,
                LogoMaxWidthPercent = LogoMaxWidthPercent,
                Orientation = Orientation,
                AutoScaleFonts = AutoScaleFonts
            };
        }

        /// <summary>
        /// Gets the effective page width accounting for margins and orientation
        /// </summary>
        public float GetContentWidth(float pageWidth)
        {
            return pageWidth - (LeftMargin + RightMargin);
        }

        /// <summary>
        /// Gets the effective page height accounting for margins and orientation
        /// </summary>
        public float GetContentHeight(float pageHeight)
        {
            return pageHeight - (TopMargin + BottomMargin);
        }

        public static PrintStyleSettings CreateDefault()
        {
            return new PrintStyleSettings();
        }

        /// <summary>
        /// Creates optimized settings for landscape orientation
        /// </summary>
        public static PrintStyleSettings CreateLandscape()
        {
            var settings = new PrintStyleSettings
            {
                Orientation = PrintOrientation.Landscape,
                Header = new TextSectionStyle("Arial", 13f, FontStyle.Bold),
                Secondary = new TextSectionStyle("Arial", 10f, FontStyle.Regular),
                Body = new TextSectionStyle("Arial", 10f, FontStyle.Regular),
                ExtraLineSpacing = 4f
            };
            return settings;
        }

        /// <summary>
        /// Creates optimized settings for portrait orientation
        /// </summary>
        public static PrintStyleSettings CreatePortrait()
        {
            return CreateDefault(); // Portrait is the default
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

        /// <summary>
        /// Font family name (e.g., "Arial", "Segoe UI", "Calibri")
        /// </summary>
        public string FontFamily { get; set; }
        
        /// <summary>
        /// Font size in points
        /// </summary>
        public float Size { get; set; }
        
        /// <summary>
        /// Font style (Bold, Italic, Regular, etc.)
        /// </summary>
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
                // Fallback to Arial instead of Consolas for better proportional rendering
                try
                {
                    return new Font("Arial", Size, Style);
                }
                catch
                {
                    return new Font(SystemFonts.DefaultFont.FontFamily, Size, Style);
                }
            }
        }
    }
}