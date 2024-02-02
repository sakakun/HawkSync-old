using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HawkSync_SM
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.3", false);
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.2", false);
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures", false);
            Application.SetCompatibleTextRenderingDefault(false);

            // Firewall

            // Check if the application is already allowed through the firewall
            if (IsApplicationAllowed())
            {
                Console.WriteLine("Application is already allowed through the firewall.");
            }
            else
            {
                // If not allowed, add the application to the firewall
                AddApplicationToFirewall(Process.GetCurrentProcess().MainModule.FileName);
            }

            // setup BMT Global Stuff
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Server Manager");
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Server Manager", "settings.sqlite");
            //ProgramConfig.DBConfig = "Data Source=" + configPath + ";Version=3;";

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            ProgramConfig.DBConfig = "Data Source=" + dbPath + ";Version=3;";
            ProgramConfig.nextCheckForUpdates = DateTime.Now.AddMinutes(10.0);
            ProgramConfig.checkRCClientsDate = DateTime.Now.AddMinutes(10.0);
            ProgramConfig.checkRCClientsInterval = 10; // 10 minutes between checks...
            ProgramConfig.Debug = false;
            ProgramConfig.Encoder = Encoding.Default;

            log.Info("...Starting BMT...");

            // check for DB database
            bool fileExist = File.Exists(dbPath);
            if (!fileExist)
            {
                File.WriteAllBytes(dbPath, HawkSync_SM.Properties.Resources.settings);
            }

            // Run Main Loader
            Main_Load loader = new Main_Load();
            loader.Show();

        }

        static bool IsApplicationAllowed()
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

        static void AddApplicationToFirewall(string applicationName)
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

    }
}
