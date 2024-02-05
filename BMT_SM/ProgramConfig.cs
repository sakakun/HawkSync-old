using System;
using System.Text;

namespace HawkSync_SM
{
    public static class ProgramConfig
    {
        // Core Functionality
        public static string ApplicationVersion { get; set; }
        public static bool ApplicationDebug { get; set; }
        public static string DBConfig { get; set; }
        public static Encoding Encoder { get; set; }
        public static string Language { get; set; }

        // Remote Control Configurations
        public static Version RCVersion = Version.Parse("1.0.0");
        public static int RCPort { get; set; }
        public static bool RCEnabled { get; set; }
        
        // IP Management
        public static string PublicIP { get; set; }
        public static bool Enable_VPNWhiteList { get; set; }
        public static bool EnableVPNCheck { get; set; }       
        public static string ip_quality_score_apikey { get; set; }
        
        // Next Checkin Dates
        public static DateTime checkExpiredBans { get; set; }
        public static DateTime NovaStatusCheck { get; set; }

        // To Be Removed
        public static int RCFTPPort { get; set; }
        
    }
}
