using System.Collections.Generic;

namespace HawkSync_SM
{
    public class UserCodes
    {
        public int UserID { get; set; }
        public string Password { get; set; }
        public bool SuperAdmin { get; set; }
        public int SubAdmin { get; set; }
        public Permissions Permissions { get; set; }
    }
    public class Permissions
    {
        public bool WebAdmin { get; set; }
        public bool RemoteAdmin { get; set; }
        public MainAppPermissions MainAppPermissions { get; set; }
        public Dictionary<int, InstancePermissions> InstancePermissions { get; set; }
    }
    public class InstancePermissions
    {
        public bool Access { get; set; }
        public bool StartInstance { get; set; }
        public bool StopInstance { get; set; }
        public bool ModifyInstance { get; set; }
        public ServerManagerPermissions ServerManagerPermissions { get; set; }
        public RotationManagerPermissions RotationManagerPermissions { get; set; }
        public FileManagerPermissions FileManagerPermissions { get; set; }
    }
    public class MainAppPermissions
    {
        public UserManagerPermissions UserManagerPermissions { get; set; }
    }
    public class UserManagerPermissions
    {
        public bool Access { get; set; }
        public bool User_Add { get; set; }
        public bool User_Delete { get; set; }
        public bool User_Modify { get; set; }
    }
    public class ServerManagerPermissions
    {
        public bool Access { get; set; }
        public AutoMessagesPermissions AutoMessagesPermissions { get; set; }
        public PlayerManagerPermissions PlayerManagerPermissions { get; set; }
        public ServerSettingsPermissions ServerSettingsPermissions { get; set; }
        public MapManagerPermissions MapManagerPermissions { get; set; }
        public ChatManagerPermissions ChatManagerPermissions { get; set; }
    }
    public class AutoMessagesPermissions
    {
        public bool Access { get; set; }
        public bool AddMsg { get; set; }
        public bool DeleteMsg { get; set; }
        public bool ModifyInterval { get; set; }
    }
    public class PlayerManagerPermissions
    {
        public bool Access { get; set; }
        public bool ViewPlayersList { get; set; }
        public bool ActionKickPlayer { get; set; }
        public bool ActionTempBanPlayer { get; set; }
        public bool ActionBanPlayer { get; set; }
        public bool ActionWarnPlayer { get; set; }
        public bool BanListAddPlayer { get; set; }
        public bool BanListDeletePlayer { get; set; }
        public bool ChangeBanSettings { get; set; }
        public bool SlapListAddSlap { get; set; }
        public bool SlapListDeleteSlap { get; set; }
        public bool EnableGodMode { get; set; }
        public bool DisableGodMode { get; set; }
        public bool DisarmPlayer { get; set; }
        public bool RearmPlayer { get; set; }
        public bool ChangePlayerTeam { get; set; }
        public VPNSettings VPNSettings { get; set; }
    }
    public class VPNSettings
    {
        public bool Access { get; set; }
        public bool WhiteListAddPlayer { get; set; }
        public bool WhiteListDeletePlayer { get; set; }
        public bool ModifyWarnLevel { get; set; }
    }
    public class ServerSettingsPermissions
    {
        public bool Access { get; set; }
        public bool ModifyServerName { get; set; }
        public bool ModifySessionType { get; set; }
        public bool ModifyMaxSlots { get; set; }
        public bool ModifyCountryCode { get; set; }
        public bool ModifyPassword { get; set; }
        public bool ModifyTimeLimit { get; set; }
        public bool ModifyStartDelay { get; set; }
        public bool ModifyReplayMaps { get; set; }
        public bool ModifyRespawnTime { get; set; }
        public bool ModifyMOTD { get; set; }
        public bool ModifyRequireNovaLogin { get; set; }
        public bool ModifyCustomSkins { get; set; }
        public bool ModifyAutoBalance { get; set; }
        public bool ModifyEnableMinPing { get; set; }
        public bool ModifyMinPing { get; set; }
        public bool ModifyEnableMaxPing { get; set; }
        public bool ModifyMaxPing { get; set; }
        public bool ModifyOneShotKills { get; set; }
        public bool ModifyFatBullets { get; set; }
        public bool ModifyDestroyBuildings { get; set; }
        public bool ModifyBlueTeamPassword { get; set; }
        public bool ModifyRedTeamPassword { get; set; }
        public bool ModifyFriendlyFire { get; set; }
        public bool ModifyFriendlyFireKills { get; set; }
        public bool ModifyFriendlyTags { get; set; }
        public bool ModifyFriendlyWarning { get; set; }
        public bool ModifyScoreBoard { get; set; }
        public bool ModifyPSPTakeOver { get; set; }
        public bool ModifyFlagReturnTime { get; set; }
        public bool ModifyMaxTeamLives { get; set; }
        public bool ModifyShowTracers { get; set; }
        public bool ModifyShowTeamClays { get; set; }
        public bool ModifyAllowAutoRange { get; set; }
        public bool ModifyFlagBallScore { get; set; }
        public bool ModifyKOTHScore { get; set; }
        public bool ModifyDMScore { get; set; }
        public Restrictions Restrictions { get; set; }
    }

    public class Restrictions
    {
        public bool Access { get; set; }
        public bool ModifyRoleRestrictions { get; set; }
        public bool ModifyWeaponRestrictions { get; set; }
    }

    public class MapManagerPermissions
    {
        public bool Access { get; set; }
        public bool ModifyMapList { get; set; }
        public bool UpdateMapList { get; set; }
        public bool ScoreMap { get; set; }
        public bool ShuffleMaps { get; set; }
        public bool SetNextMap { get; set; }
        public bool SaveRotation { get; set; }
        public bool LoadRotation { get; set; }
    }
    public class ChatManagerPermissions
    {
        public bool Access { get; set; }
        public bool SendMsg { get; set; }
    }
    public class RotationManagerPermissions
    {
        public bool Access { get; set; }
        public bool CreateNewRotation { get; set; }
        public bool DeleteRotation { get; set; }
    }
    public class FileManagerPermissions
    {
        public bool Access { get; set; }
        public bool Download { get; set; }
        public bool Upload { get; set; }
    }
}
