using HawkSync_SM.classes.SupportClasses;
using System;
using System.IO;
using System.Text;

namespace HawkSync_SM
{
    public static class ProgramConfig
    {
        // Core Functionality
        public static string        ApplicationVersion          { get; set; }
        public static bool          ApplicationDebug            { get; set; } = false;
        public static Encoding      ApplicationEncoding         { get; set; } = Encoding.Default;
        public static AppState      ApplicationState            { get; set; }

        // Remote Control Configurations
        public static Version       RCVersion                   { get; set; } = Version.Parse("1.0.0");
        public static int           RCPort                      { get; set; }
        public static bool          RCEnabled                   { get; set; }

        // Application Paths
        public static string        Path_AppData                { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Server Manager");
        public static string        Path_DBFile                 { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Server Manager", "settings.sqlite");

        // Database
        public static string        dbConfig                    { get; set; } = "Data Source=" + Path_DBFile + ";Version=3;";
        public static bool          dbReady                     { get; set; } = DatabaseCheck();

        // IP/Firewall Management
        public static string        PublicIP { get; set; }
        public static bool          EnableVPNCheck { get; set; }
        public static bool          EnableWFB { get; set; }
        public static string        ip_quality_score_apikey { get; set; }


        // Next Checkin Dates
        public static DateTime      checkExpiredBans { get; set; }
        public static DateTime      NovaStatusCheck { get; set; }

        // Function DatabaseCheck
        // Description: Check if the database exists and is up to date
        static private bool DatabaseCheck()
        {
            if (dbReady) return true;

            if (!Directory.Exists(Path_AppData)) { Directory.CreateDirectory(Path_AppData); }

            // check for DB database
            bool fileExist = File.Exists(Path_DBFile);
            // debug mode
            // if (fileExist && Debugger.IsAttached) { File.Delete(dbPath); fileExist = false; }
            // database upgrade check
            if (!fileExist) { File.WriteAllBytes(Path_DBFile, HawkSync_SM.Properties.Resources.settings); }
            else
            {
                File.WriteAllBytes(Path_DBFile + "_check", HawkSync_SM.Properties.Resources.settings);
                SQLiteDatabaseUpdater dbUpdater = new SQLiteDatabaseUpdater(Path_DBFile, Path_DBFile + "_check");
                if (dbUpdater.RunUpdater())
                {
                    File.Delete(Path_DBFile + "_check");
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            // Public IP Address
            IPManagement.public_ip();

            return true;
            
        }

    }
}
