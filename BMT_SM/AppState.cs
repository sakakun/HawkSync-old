using HawkSync_SM.classes;
using HawkSync_SM.classes.logs;
using System.Collections.Generic;
using System.Diagnostics;
using WatsonTcp;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_SM
{
    public class AppState
    {
        public Dictionary<int, Instance> Instances { get; set; }
        public Dictionary<int, ipqualityscore> IPQualityCache { get; set; }
        public Dictionary<int, CollectedPlayerStatsPlayers> PlayerStats { get; set; }
        public Dictionary<int, ob_ChatLogs> ChatLogs { get; set; }
        public Dictionary<string, UserCodes> Users { get; set; }
        public Dictionary<int, monthlystats> yearlystats { get; set; }
        public List<ob_AdminChatMsgs> adminChatMsgs { get; set; }
        public SystemInfoClass SystemInfo { get; set; }
        public autoRes autoRes { get; set; }
        public Dictionary<string, RCListenerClass> rcClients { get; set; }
        public List<ob_playerHistory> playerHistories { get; set; }
        public List<adminnotes> adminNotes { get; set; }
        public List<RCLogs> RCLogs { get; set; }
        public Dictionary<string, byte[]> imageCache { get; set; }
        public WatsonTcpServer server { get; set; }
        public EventLog eventLog { get; set; }
        public Dictionary<int, Process> ApplicationProcesses { get; set; }
        public Dictionary<int, ConsoleQueue> ConsoleQueue { get; set; }
        public Dictionary<int, Timer> ChatHandlerTimer { get; set; }
        public Dictionary<int, ModsClass> Mods { get; set; }

        public AppState()
        {
            Instances = new Dictionary<int, Instance>();

            IPQualityCache = new Dictionary<int, ipqualityscore>();

            PlayerStats = new Dictionary<int, CollectedPlayerStatsPlayers>();

            ChatLogs = new Dictionary<int, ob_ChatLogs>();

            Users = new Dictionary<string, UserCodes>();

            yearlystats = new Dictionary<int, monthlystats>();

            adminChatMsgs = new List<ob_AdminChatMsgs>();

            SystemInfo = new SystemInfoClass();

            autoRes = new autoRes();

            rcClients = new Dictionary<string, RCListenerClass>();

            playerHistories = new List<ob_playerHistory>();

            adminNotes = new List<adminnotes>();

            RCLogs = new List<RCLogs>();

            server = new WatsonTcpServer("0.0.0.0", ProgramConfig.RCPort);

            ApplicationProcesses = new Dictionary<int, Process>();

            ConsoleQueue = new Dictionary<int, ConsoleQueue>();

            ChatHandlerTimer = new Dictionary<int, Timer>();

            eventLog = new EventLog("Application")
            {
                Source = "HawkSync_SM"
            };

            Mods = new Dictionary<int, ModsClass>();

        }
    }
}
