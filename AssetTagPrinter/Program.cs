using System;
 
using System.Windows.Forms;

namespace AssetTagPrinter
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Runtime no longer attempts to deploy Configuration.xml to ProgramData.
            // The app will rely on the system-wide Configuration.xml already present in ProgramData.

            Application.Run(new MainForm());
        }

        // Deployment is handled externally (installer/administrator); no runtime copy logic.
    }
}
