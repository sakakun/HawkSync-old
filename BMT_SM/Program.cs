using HawkSync_SM.classes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace HawkSync_SM
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.3", false);
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.2", false);
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures", false);

            // Firewall Check & Modification
            if (IsApplicationAllowed())
            {
                Console.WriteLine("Application is already allowed through the firewall.");
            }
            else
            {
                AddApplicationToFirewall(Process.GetCurrentProcess().MainModule.FileName);
            }

            // Process Database & Preload Configurations
            ProcessDatabase();
            // Load Prefered Language from DB
            LoadLanguages();
            // Load Remote Control Configurations
            GetRemoteControlConfig();

            // Application Version Statements
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            ProgramConfig.ApplicationVersion = fileVersionInfo.FileVersion;

            // Log Application Start
            log.Info("HawkSync Starting");

            // Trigger Non-Static Application
            Main_Load loader = new Main_Load();
        }

        static private bool IsApplicationAllowed()
        {
            // Specify the path of the Netsh executable
            string netshPath = Environment.ExpandEnvironmentVariables("%SystemRoot%\\System32\\netsh.exe");

            // Specify the command to list firewall rules for the program
            string command = $"advfirewall firewall show rule name=\"Babstats Server Manager\"";

            // Set up the process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = netshPath,
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            // Start the process
            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Check if the output contains the rule information
                return output.Contains("Rule Name:");
            }
        }
        static private void AddApplicationToFirewall(string applicationName)
        {
            // Specify the path of the Netsh executable
            string netshPath = Environment.ExpandEnvironmentVariables("%SystemRoot%\\System32\\netsh.exe");

            // Specify the command to add the application to the firewall
            string command = $"advfirewall firewall add rule name=\"Babstats Server Manager\" dir=in action=allow program=\"{applicationName}\" enable=yes";

            // Set up the process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = netshPath,
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            // Start the process
            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Application added to the firewall successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to add the application to the firewall.");
                }
            }
        }
        static private void ProcessDatabase()
        {

            // setup BMT Global Stuff
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Server Manager");
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Server Manager", "settings.sqlite");

            if (!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }

            // check for DB database
            bool fileExist = File.Exists(dbPath);
            //if (fileExist && Debugger.IsAttached) { File.Delete(dbPath); fileExist = false; }
            if (!fileExist) { File.WriteAllBytes(dbPath, HawkSync_SM.Properties.Resources.settings); }

            // Default Hard Coded Settings
            ProgramConfig.DBConfig = "Data Source=" + dbPath + ";Version=3;";
            ProgramConfig.ApplicationDebug = false;
            ProgramConfig.Encoder = Encoding.Default;

            // Public IP Address
            IPManagement.public_ip();

        }
        static private void LoadLanguages()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand langCmd = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'lang';", db);
            SQLiteDataReader langReader = langCmd.ExecuteReader();
            langReader.Read();
            ProgramConfig.Language = langReader.GetString(0);
            langReader.Close();
            db.Close();
        }
        static private void GetRemoteControlConfig()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand port_query = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'remote_client_port';", db);
            ProgramConfig.RCPort = Convert.ToInt32(port_query.ExecuteScalar());
            port_query.Dispose();

            SQLiteCommand rcQuery = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'remote_client';", db);
            ProgramConfig.RCEnabled = Convert.ToBoolean(Convert.ToInt32(rcQuery.ExecuteScalar()));
            rcQuery.Dispose();

            db.Close();
            db.Dispose();
        }

    }
}
