using System;
using System.Text;

namespace HawkSync_SM
{
    public static class ProgramConfig
    {
        public static string DBConfig { get; set; }
        public static string version { get; set; }
        public static bool Enable_VPNWhiteList { get; set; }
        public static int RCPort { get; set; }
        public static bool EnableVPNCheck { get; set; }
        public static bool EnableRC { get; set; }
        public static string ip_quality_score_apikey { get; set; }
        public static string ipaddress { get; set; }
        public static DateTime checkExpiredBans { get; set; }
        public static DateTime nextCheckForUpdates { get; set; }
        public static IDisposable WebAPI { get; set; }
        public static int web_api_port = 7001;
        public static int WebServerPID { get; set; }
        public static int WebServerPort { get; set; }
        public static bool EnableWebServer { get; set; }
        public static bool Debug { get; set; }
        public static DateTime checkRCClientsDate { get; set; }
        public static int checkRCClientsInterval { get; set; }
        public static int checkFTPClientsInterval { get; set; }
        public static DateTime checkFTPClients { get; set; }
        public static Encoding Encoder { get; set; }
        public static DateTime NovaStatusCheck { get; set; }
        public static int RCFTPPort { get; set; }
        public static int RCFTPPasvMin { get; set; }
        public static int RCFTPPasvMax { get; set; }
        public static Version RCVersion = Version.Parse("1.0.0");
        public static bool showedExpired { get; set; }
        public static string Language { get; set; }
    }
}
