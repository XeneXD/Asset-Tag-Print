using System;
using System.IO;
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

            // Ensure POS Configuration.xml is present in ProgramData for runtime POS components.
            try
            {
                EnsurePosConfigInProgramData();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Warning: Access denied when deploying Configuration.xml to ProgramData. Please run the installer or run this app as Administrator to install system-wide configuration.", "Configuration deploy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Non-fatal: show a warning but allow the app to continue. Installer should handle final placement.
                MessageBox.Show($"Warning: unable to ensure POS Configuration.xml in ProgramData. {ex.Message}", "Configuration deploy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Application.Run(new MainForm());
        }

        private static void EnsurePosConfigInProgramData()
        {
            string appLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.xml");
            if (!File.Exists(appLocal)) return; // nothing to do

            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pdFolder = Path.Combine(programData, "Microsoft", "Point Of Service", "Configuration");
            string dest = Path.Combine(pdFolder, "Configuration.xml");

            if (File.Exists(dest))
            {
                // If destination exists and is older than source, overwrite; otherwise leave as-is.
                try
                {
                    var srcInfo = new FileInfo(appLocal);
                    var dstInfo = new FileInfo(dest);
                    if (srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc)
                    {
                        Directory.CreateDirectory(pdFolder);
                        File.Copy(appLocal, dest, true);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    throw;
                }
            }
            else
            {
                Directory.CreateDirectory(pdFolder);
                File.Copy(appLocal, dest);
            }
        }
    }
}
