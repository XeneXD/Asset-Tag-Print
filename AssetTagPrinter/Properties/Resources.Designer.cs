
namespace AssetTagPrinter.Properties {
    using System;
    using System.Drawing;
    using System.Resources;
    using System.Globalization;

    [System.CodeDom.Compiler.GeneratedCode("ResXResource", "1.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCode()]
    internal static class Resources {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal static ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new ResourceManager("AssetTagPrinter.Properties.Resources", typeof(Resources).Assembly);
                }
                return resourceMan;
            }
        }

        internal static CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static Bitmap BlackAndWhite {
            get { return (Bitmap) ResourceManager.GetObject("BlackAndWhite", resourceCulture); }
        }

        internal static Bitmap OneLineWithBackground111 {
            get { return (Bitmap) ResourceManager.GetObject("OneLineWithBackground111", resourceCulture); }
        }

        internal static Bitmap OneLineWithBackground {
            get { return (Bitmap) ResourceManager.GetObject("OneLineWithBackground", resourceCulture); }
        }

        internal static string HowToUse {
            get { return ResourceManager.GetString("HowToUse", resourceCulture); }
        }

        internal static string Installation {
            get { return ResourceManager.GetString("Installation", resourceCulture); }
        }

        internal static string UpdateLog {
            get { return ResourceManager.GetString("UpdateLog", resourceCulture); }
        }
    }
}
