using HawkSync_SM.classes;
using System;
using System.Collections.Generic;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_SM
{
    public class Instance
    {
        public int Id { get; set; }
        public int DataTableColumnId { get; set; }
        public string GamePath { get; set; }
        public int GameType { get; set; }
        public int? PID { get; set; }
        public string GameName { get; set; }
        public string BindAddress { get; set; }
        public int GamePort { get; set; }
        public string ServerName { get; set; } = "Untitled Server";
        public bool Dedicated { get; set; } = true;
        public string CountryCode { get; set; } = "US";
        public string HostName { get; set; }
        public string Password { get; set; }
        public int SessionType { get; set; } = 0;
        public int MaxSlots { get; set; } = 50;
        public int GameScore { get; set; } = 20;
        public int FBScore { get; set; } = 10;
        public int KOTHScore { get; set; } = 10;
        public int ZoneTimer { get; set; } = 4;
        public bool WindowedMode { get; set; } = true;
        public int NumPlayers { get; set; } 
        public string Slots { get; set; }
        public string Map { get; set; }
        public string GameTypeName { get; set; }
        public string NextGameTypeName { get; set; }
        public int mapIndex { get; set; }
        public int TimeRemaining { get; set; }
        public int TimeLimit { get; set; } = 22;
        public int LoopMaps { get; set; } = 1;
        public int RespawnTime { get; set; } = 20;
        public bool RequireNovaLogin { get; set; } = false;
        public bool AllowCustomSkins { get; set; } = false;
        public bool AutoBalance { get; set; } = true;
        public string MOTD { get; set; } = "Welcome to the server!";
        public bool MinPing { get; set; } = false;
        public int MinPingValue { get; set; } = 0;
        public bool MaxPing { get; set; } = false;
        public int MaxPingValue { get; set; } = 0;
        public bool OneShotKills { get; set; } = false;
        public bool FatBullets { get; set; } = false;
        public bool DestroyBuildings { get; set; } = false;
        public string BluePassword { get; set; }
        public string RedPassword { get; set; }
        public bool FriendlyFire { get; set; } = false;
        public int FriendlyFireKills { get; set; } = 0;
        public bool FriendlyTags { get; set; } = true;
        public bool FriendlyFireWarning { get; set; } = false;
        public int ScoreBoardDelay { get; set; } = 20;
        public int PSPTakeOverTime { get; set; } = 20;
        public int FlagReturnTime { get; set; } = 4;
        public int MaxTeamLives { get; set; } = 20;
        public bool ShowTracers { get; set; } = false;
        public bool ShowTeamClays { get; set; } = true;
        public bool AllowAutoRange { get; set; } = false;
        public int StartDelay { get; set; } = 2;
        public int MaxKills { get; set; } = 20;
        public string WebStatsStatus { get; set; }
        public bool EnableWebStats { get; set; }
        public int WebStatsSoftware { get; set; }
        public int anti_stat_padding { get; set; }
        public int anti_stat_padding_min_minutes { get; set; } = 5;
        public int anti_stat_padding_min_players { get; set; } = 2;
        public bool misc_show_ranks { get; set; }
        public int misc_left_leaning { get; set; } = 0;
        public int mapCounter { get; set; }
        public bool collectPlayerStats { get; set; }
        /* this is a bool flag to prevent collection of stats
         * if BMT was started for the first time. This will allow for the correction of
         * inacturate stats.
        */
        public bool SubmitWebStats { get; set; } = false;
        public bool enableVPNCheck { get; set; } = false;
        public string WebstatsURL { get; set; }
        public string WebStatsId { get; set; }
        public bool WebstatsIdVerified { get; set; }
        public DateTime nextWebStatsStatusUpdate { get; set; }
        public ob_AutoMessages AutoMessages { get; set; } = new ob_AutoMessages();
        public DateTime NextUpdateWebStats { get; set; }
        public InstanceStatus Status { get; set; } = InstanceStatus.OFFLINE;
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        public DateTime NextUpdateTime { get; set; } = DateTime.Now.AddSeconds(2.0);
        public int MasterUnixNextUpdate { get; set; }
        public bool TimeFlag { get; set; }
        public bool FromDeathMatch { get; set; }
        public int mapListCount { get; set; }
        public int gameMapType { get; set; }
        public int gameCrashCounter { get; set; }
        public DateTime gameCrashCheck { get; set; }
        public bool CrashRecovery { get; set; }
        public Dictionary<int, MapList> previousMapList { get; set; } = new Dictionary<int, MapList>();
        public Dictionary<int, MapList> MapList { get; set; } = new Dictionary<int, MapList>();
        public List<ob_playerBanList> BanList { get; set; } = new List<ob_playerBanList>();
        public Dictionary<int, ob_playerList> PlayerList { get; set; } = new Dictionary<int, ob_playerList>();
        public Dictionary<int, ob_ipWhitelist> VPNWhiteList { get; set; } = new Dictionary<int, ob_ipWhitelist>();
        public Dictionary<string, string> IPWhiteList { get; set; } = new Dictionary<string, string>();
        public List<ob_playerChangeTeamList> ChangeTeamList { get; set; } = new List<ob_playerChangeTeamList>();
        public PlayerRoles RoleRestrictions { get; set; } = new PlayerRoles();
        public WeaponsClass WeaponRestrictions { get; set; } = new WeaponsClass();
        public List<int> GodModeList { get; set; } = new List<int>();
        public bool IsRunningPostGameProcesses { get; set; }
        public bool IsRunningScoringGameProcesses { get; set; }
        public List<string> CustomWarnings { get; set; } = new List<string>();
        public List<ob_WarnPlayerClass> WarningQueue { get; set; } = new List<ob_WarnPlayerClass>();
        public List<int> DisarmPlayers { get; set; } = new List<int>();
        public List<ob_playerPreviousTeam> previousTeams { get; set; } = new List<ob_playerPreviousTeam>();
        public Dictionary<int, MapList> availableMaps { get; set; } = new Dictionary<int, MapList>();
        public List<savedmaprotations> savedmaprotations { get; set; } = new List<savedmaprotations>();
        public IntPtr ProcessHandle { get; set; }
        public int nextMapGameType { get; set; }
        public Timer ScoreboardTimer { get; set; }
        public bool ReportNovaHQ { get; set; }
        public DateTime NextUpdateNovaHQ { get; set; }
        public bool ReportNovaCC { get; set; }
        public DateTime NextUpdateNovaCC { get; set; }
        public MapList CurrentMap { get; set; }
        public PluginsClass Plugins { get; set; } = new PluginsClass();
        public List<WelcomePlayer> WelcomeQueue { get; set; }
        public DateTime WelcomeTimer { get; set; }
        public List<VoteMapsTally> VoteMapsTally { get; set; }
        public Timer VoteMapTimer { get; set; }
        public bool VoteMapStandBy { get; set; }
        public bool BadMapStandBy { get; set; }
        public int previousSlot { get; set; }
        public bool IsTeamSabre { get; set; } = false;
        public List<int> modsDetected { get; set; }
        public int Mod { get; set; }
    }

    public enum InstanceStatus
    {
        OFFLINE = 0,
        LOADINGMAP = 1,
        STARTDELAY = 2,
        ONLINE = 3,
        SCORING = 4,
    }
}
