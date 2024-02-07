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
        public string ServerName { get; set; }
        public bool Dedicated { get; set; }
        public string CountryCode { get; set; }
        public string HostName { get; set; }
        public string Password { get; set; }
        public int SessionType { get; set; }
        public int MaxSlots { get; set; }
        public int GameScore { get; set; } // dm/tdm
        public int FBScore { get; set; }
        public int KOTHScore { get; set; }
        public int ZoneTimer { get; set; }
        public bool WindowedMode { get; set; }
        public int NumPlayers { get; set; }
        public string Slots { get; set; }
        public string Map { get; set; }
        public string GameTypeName { get; set; }
        public string NextGameTypeName { get; set; }
        public int mapIndex { get; set; }
        public int TimeRemaining { get; set; }
        public int TimeLimit { get; set; }
        public int LoopMaps { get; set; }
        public int RespawnTime { get; set; }
        public bool RequireNovaLogin { get; set; }
        public bool AllowCustomSkins { get; set; }
        public bool AutoBalance { get; set; }
        public string MOTD { get; set; }
        public bool MinPing { get; set; }
        public int MinPingValue { get; set; }
        public bool MaxPing { get; set; }
        public int MaxPingValue { get; set; }
        public bool OneShotKills { get; set; }
        public bool FatBullets { get; set; }
        public bool DestroyBuildings { get; set; }
        public string BluePassword { get; set; }
        public string RedPassword { get; set; }
        public bool FriendlyFire { get; set; }
        public int FriendlyFireKills { get; set; }
        public bool FriendlyTags { get; set; }
        public bool FriendlyFireWarning { get; set; }
        public int ScoreBoardDelay { get; set; }
        public int PSPTakeOverTime { get; set; }
        public int FlagReturnTime { get; set; }
        public int MaxTeamLives { get; set; }
        public bool ShowTracers { get; set; }
        public bool ShowTeamClays { get; set; }
        public bool AllowAutoRange { get; set; }
        public int StartDelay { get; set; }
        public int MaxKills { get; set; }
        public string WebStatsStatus { get; set; }
        public bool EnableWebStats { get; set; }
        public int WebStatsSoftware { get; set; }
        public int anti_stat_padding { get; set; }
        public int anti_stat_padding_min_minutes { get; set; }
        public int anti_stat_padding_min_players { get; set; }
        public bool misc_show_ranks { get; set; }
        public int misc_left_leaning { get; set; }
        public int mapCounter { get; set; }
        public bool collectPlayerStats { get; set; }
        /* this is a bool flag to prevent collection of stats
         * if BMT was started for the first time. This will allow for the correction of
         * inacturate stats.
        */
        public bool SubmitWebStats { get; set; }
        public bool enableVPNCheck { get; set; }
        public string WebstatsURL { get; set; }
        public int WebStatsId { get; set; }
        public bool WebstatsIdVerified { get; set; }
        public DateTime nextWebStatsStatusUpdate { get; set; }
        // this is used for the "Server Status" on BabWebStats
        public ob_AutoMessages AutoMessages { get; set; }
        public DateTime NextUpdateWebStats { get; set; }
        public InstanceStatus Status { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime NextUpdateTime { get; set; }
        public bool ReportMaster { get; set; }
        public int MasterUnixNextUpdate { get; set; }
        public bool TimeFlag { get; set; }
        public bool FromDeathMatch { get; set; }
        public int mapListCount { get; set; }
        public int gameMapType { get; set; }
        public int gameCrashCounter { get; set; }
        public DateTime gameCrashCheck { get; set; }
        public bool CrashRecovery { get; set; }
        public Dictionary<int, MapList> previousMapList { get; set; }
        public Dictionary<int, MapList> MapList { get; set; }
        public List<ob_playerBanList> BanList { get; set; }
        public Dictionary<int, ob_playerList> PlayerList { get; set; }
        public Dictionary<int, ob_ipWhitelist> VPNWhiteList { get; set; }
        public Dictionary<string, string> IPWhiteList { get; set; }
        public List<ob_playerChangeTeamList> ChangeTeamList { get; set; }
        public PlayerRoles RoleRestrictions { get; set; }
        public WeaponsClass WeaponRestrictions { get; set; }
        public List<int> GodModeList { get; set; }
        public bool IsRunningPostGameProcesses { get; set; }
        public bool IsRunningScoringGameProcesses { get; set; }
        public List<string> CustomWarnings { get; set; }
        public List<ob_WarnPlayerClass> WarningQueue { get; set; }
        public List<int> DisarmPlayers { get; set; }
        public List<ob_playerPreviousTeam> previousTeams { get; set; }
        public Dictionary<int, MapList> availableMaps { get; set; }
        public List<savedmaprotations> savedmaprotations { get; set; }
        public IntPtr ProcessHandle { get; set; }
        public int nextMapGameType { get; set; }
        public Timer ScoreboardTimer { get; set; }
        public bool ReportNovaHQ { get; set; }
        public DateTime NextUpdateNovaHQ { get; set; }
        public bool ReportNovaCC { get; set; }
        public DateTime NextUpdateNovaCC { get; set; }
        public MapList CurrentMap { get; set; }
        public PluginsClass Plugins { get; set; }
        public List<WelcomePlayer> WelcomeQueue { get; set; }
        public DateTime WelcomeTimer { get; set; }
        public List<VoteMapsTally> VoteMapsTally { get; set; }
        public Timer VoteMapTimer { get; set; }
        public bool VoteMapStandBy { get; set; }
        public bool BadMapStandBy { get; set; }
        public int previousSlot { get; set; }
        public bool IsTeamSabre { get; set; }
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
