using System;
using System.IO;
using System.Windows.Forms;
using Salaros.Configuration;

namespace HawkSync_RC
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Babstats", "Remote Control");
                if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

                // check remote.ini
                var configFile = new ConfigParser();
                var RemoteINIFileExist = File.Exists(ProgramConfig.RemoteINI);
                if (!RemoteINIFileExist)
                {
                    configFile.SetValue("Main", "Ban", 0);
                    configFile.SetValue("Main", "BanReason", "\"\"");
                    configFile.SetValue("Main", "LastSelected", 0);
                    configFile.SetValue("Main", "InternalRefresh", 3);
                    File.WriteAllText(ProgramConfig.RemoteINI, configFile.ToString());
                }

                // check profile.cfg
                var RemoteProfilesExist = File.Exists(ProgramConfig.RemoteProfiles);
                var remoteProfilesConfig = new ConfigParser();
                if (!RemoteProfilesExist)
                    File.WriteAllText(ProgramConfig.RemoteProfiles, remoteProfilesConfig.ToString());

                Application.Run(new RC_RemoteLogin());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}