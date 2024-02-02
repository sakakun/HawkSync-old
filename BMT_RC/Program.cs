using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace HawkSync_RC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Remote Control");
                if (!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }

                // check remote.ini
                ConfigParser configFile = new ConfigParser();
				bool RemoteINIFileExist = File.Exists(ProgramConfig.RemoteINI);
				if (!RemoteINIFileExist)
				{
					configFile.SetValue("Main", "Ban", 0);
					configFile.SetValue("Main", "BanReason", "\"\"");
					configFile.SetValue("Main", "LastSelected", 0);
					configFile.SetValue("Main", "InternalRefresh", 3);
					File.WriteAllText(ProgramConfig.RemoteINI, configFile.ToString());
				}

				// check profile.cfg
				bool RemoteProfilesExist = File.Exists(ProgramConfig.RemoteProfiles);
				ConfigParser remoteProfilesConfig = new ConfigParser();
				if (!RemoteProfilesExist)
				{
					File.WriteAllText(ProgramConfig.RemoteProfiles, remoteProfilesConfig.ToString());
				}

                Application.Run(new Main_Login());
            }
            catch (Exception e)
            {

				return;
			}
		}
    }
}
