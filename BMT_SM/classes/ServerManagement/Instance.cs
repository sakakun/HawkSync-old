using HawkSync_SM;
using HawkSync_SM.classes;
using HawkSync_SM.classes.StatManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_SM
{
    public class Instance
    {
        // Instance Specific
        public int      instanceID              { get; set; }
        public int?     instanceAttachedPID     { get; set; }
        public IntPtr   instanceProcessHandle   { get; set; }
        public InstanceStatus instanceStatus    { get; set; } = InstanceStatus.OFFLINE;
        public int      instanceCrashCounter    { get; set; }                                   /* Counter for the number of Crashes */
        public DateTime instanceCrashCheckTime  { get; set; }                                   /* Next Timestamp of crash check */
        public bool     instanceCrashRecovery   { get; set; }                                   /* Enable/Disable crash recovery */

        // Instance Tick Flow
        public bool     instancePostProcRun     { get; set; }
        public bool     instanceScoreProcRun    { get; set; }
        public DateTime instanceLastUpdateTime  { get; set; } = DateTime.Now;
        public DateTime instanceNextUpdateTime  { get; set; } = DateTime.Now.AddSeconds(2.0);
        

        // Profile Server Information
        public int      profileServerType       { get; set; }
        public string   profileServerPath       { get; set; }
        public string   profileBindIP           { get; set; }
        public int      profileBindPort         { get; set; }
        public int      profileGameMod          { get; set; }
        public string   profileName             { get; set; }

        // Network & Firewall

        // Game Settings
        public string   gameServerName          { get; set; } = "Untitled Server";
        public string   gameMOTD                { get; set; } = "Welcome to the server!";
        public string   gameCountryCode         { get; set; } = "US";
        public string   gameHostName            { get; set; } = "HostName";
        public bool     gameDedicated           { get; set; } = true;
        public bool     gameWindowedMode        { get; set; } = true;
        public string   gamePasswordLobby       { get; set; }
        public string   gamePasswordBlue        { get; set; }
        public string   gamePasswordRed         { get; set; }
        public int      gameSessionType         { get; set; } = 0;                              /* Session Type (Internet/LAN) - Currently not working. */
        public int      gameMaxSlots            { get; set; } = 50;                             /* Max Players 50 */
        public int      gameLoopMaps            { get; set; } = 1;                              /* 0 = Play One Map, 1 = Loop Maps, 2 = Cycle Maps (Starting Server requires a T/F bool) */
        public bool     gameRequireNova         { get; set; } = false;
        public bool     gameCustomSkins         { get; set; } = false;
        public int      gameScoreKills          { get; set; } = 20;                             /* Game Score needed for T/DM Matches to Win */
        public int      gameScoreFlags          { get; set; } = 10;                             /* Game Score needed for CTF & FB to Win */
        public int      gameScoreZoneTime       { get; set; } = 10;                             /* Game Score needed for T/KOTH to Win */
        public int      gameFriendlyFireKills   { get; set; } = 10;                             /* Game Friendly Fire Kills allow before punt. */
        public int      gameTimeLimit           { get; set; } = 22;                             /* Time limit per game, minutes */
        public int      gameStartDelay          { get; set; } = 2;                              /* Game start delay (minutes) */
        public int      gameRespawnTime         { get; set; } = 20;                             /* Respawn Time in Minutes */
        public int      gameScoreBoardDelay     { get; set; } = 20;                             /* Score Board Delay in Seconds */
        public int      gamePSPTOTimer          { get; set; } = 20;                             /* PSP Take Over Timer in Seconds */
        public int      gameFlagReturnTime      { get; set; } = 4;                              /* Flag retrun time in minutes */
        public int      gameMaxTeamLives        { get; set; } = 20;                             /* Max Team Lives, don't know what this is really used for... */
        // Game Settings: Misc Settings
        public bool     gameOneShotKills        { get; set; } = false;
        public bool     gameFatBullets          { get; set; } = false;
        public bool     gameDestroyBuildings    { get; set; } = false;
        public int      gameAllowLeftLeaning    { get; set; } = 0;
        // Game Settings: Ping Settings
        public bool     gameMinPing             { get; set; } = false;
        public bool     gameMaxPing             { get; set; } = false;
        public int      gameMinPingValue        { get; set; } = 0;
        public int      gameMaxPingValue        { get; set; } = 0;
        // Game Settings: Game Options
        public bool     gameOptionAutoBalance   { get; set; } = true;
        public bool     gameOptionFF            { get; set; } = false;
        public bool     gameOptionFFWarn        { get; set; } = false;
        public bool     gameOptionFriendlyTags  { get; set; } = true;
        public bool     gameOptionShowTracers   { get; set; } = false;
        public bool     gameShowTeamClays       { get; set; } = true;
        public bool     gameOptionAutoRange     { get; set; } = false;

        // HeartBeat Reporting
        public bool     ReportNovaHQ            { get; set; }
        public DateTime NextUpdateNovaHQ        { get; set; }
        public bool     ReportNovaCC            { get; set; }
        public DateTime NextUpdateNovaCC        { get; set; }

        // WebStats Variables
        public int      WebStatsStatus          { get; set; }
        public bool     WebStatsEnabled         { get; set; } = false;
        public int      WebStatsSoftware        { get; set; } = 0;
        public string   WebstatsURL             { get; set; }
        public bool     WebstatsVerified        { get; set; }
        public string   WebStatsProfileID       { get; set; }
        // WebStats Anti Stat Padding (ASP) 
        public int      WebStatsASPEnabled      { get; set; } = 0;
        public int      WebStatsASPMinMinutes   { get; set; } = 5;
        public int      WebStatsASPMinPlayers   { get; set; } = 2;
        // WebStats Timers
        public BabstatsTimer        WebStatsBabstatsTimer       { get; set; } = new BabstatsTimer();
        // WebStats Rank Announcements
        public bool     WebStatsAnnouncements   { get; set; } = false;

        // VPN Settings
        public bool     vpnCheckEnabled         { get; set; } = false;

        // DataTable Storage Objects
        public ob_AutoMessages                          AutoMessages        { get; set; } = new ob_AutoMessages();                      /* Storage Object for AutoMessages */
        public List<ob_PlayerChatLog>                   ChatLog             { get; set; } = new List<ob_PlayerChatLog>();               /* Storage Object for Chat Logs */
        public Dictionary<int, MapList>                 MapListPrevious     { get; set; } = new Dictionary<int, MapList>();
        public Dictionary<int, MapList>                 MapListCurrent      { get; set; } = new Dictionary<int, MapList>();
        public Dictionary<int, MapList>                 MapListAvailable    { get; set; } = new Dictionary<int, MapList>();
        public List<savedmaprotations>                  MapListRotationDB   { get; set; } = new List<savedmaprotations>();
        public Dictionary<int, ob_playerList>           PlayerList          { get; set; } = new Dictionary<int, ob_playerList>();
        public List<ob_playerBanList>                   PlayerListBans      { get; set; } = new List<ob_playerBanList>();
        public List<int>                                PlayerListDisarm    { get; set; } = new List<int>();
        public List<int>                                PlayerListGodMod    { get; set; } = new List<int>();
        public Dictionary<string, PlayerStats>          PlayerStats         { get; set; } = new Dictionary<string, PlayerStats>();
        public Dictionary<string, PlayerWeaponStats>    PlayerWeaponStats   { get; set; } = new Dictionary<string, PlayerWeaponStats>();
        public Dictionary<int, ob_ipWhitelist>          IPWhiteList         { get; set; } = new Dictionary<int, ob_ipWhitelist>();
        public List<ob_playerChangeTeamList>            TeamListChange      { get; set; } = new List<ob_playerChangeTeamList>();
        public List<ob_playerPreviousTeam>              TeamListPrevious    { get; set; } = new List<ob_playerPreviousTeam>();
        public PlayerRoles                              RoleRestrictions    { get; set; } = new PlayerRoles();
        public WeaponsClass                             WeaponRestrictions  { get; set; } = new WeaponsClass();
        public List<string>                             CustomWarnings      { get; set; } = new List<string>();
        public List<ob_ServerMessageQueue>              ServerMessagesQueue { get; set; } = new List<ob_ServerMessageQueue>();
        public FirewallManagement                       Firewall            { get; set; } = new FirewallManagement();

        // Dynamic Storage Variables
        public bool     infoTeamSabre           { get; set; } = false;                          /* Should remove this */
        public int      infoNumPlayers          { get; set; } = 0;                              /* Current number of players in the server */
        public MapList  infoCurrentMap          { get; set; }                                   /* Current Map Details */
        public string   infoCurrentMapName      { get; set; }                                   /* Name of the current map playing */
        public int      infoCurrentMapIndex     { get; set; }                                   /* Index of currently playing map */
        public int      infoCounterMaps         { get; set; }                                   /* Number of current maps in playlist */
        public int      infoMapCycleIndex       { get; set; }                                   /* In-game Map Cycle Index */
        public int      infoMapTimeRemaining    { get; set; }                                   /* Current Remaining Time of Map Playing */
        public int      infoMapGameType         { get; set; }                                   /* Current Map Game Type (DM,TDM,etc) */
        public int      infoNextMapGameType     { get; set; }
        public bool     infoCollectPlayerStats  { get; set; }
        public bool     infoMarkerTime          { get; set; }                                   /* Mark when time is less than 1 minute */
        public Timer    infoTimerScoreboard     { get; set; }                                   /* Follow up on usuage */
        public scoreManagement      infoCurrentScores       { get; set; } = new scoreManagement();          

        // Objects to Change/Remove/Follow-up
        public string GameTypeName { get; set; }
        public PluginsClass Plugins { get; set; } = new PluginsClass();
        public List<WelcomePlayer> WelcomeQueue { get; set; }
        public DateTime WelcomeTimer { get; set; }
        public List<VoteMapsTally> VoteMapsTally { get; set; }
        public Timer VoteMapTimer { get; set; }
        public bool VoteMapStandBy { get; set; }

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
