using System;
using System.Collections.Generic;

namespace HawkSync_SM
{
    public class RCListenerClass
    {
        public int RCPort { get; set; }
        public bool active { get; set; }
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public string SessionID { get; set; }
        public string action { get; set; }
        public string lastCMD { get; set; }
        public DateTime lastCMDTime { get; set; }
        public string expires { get; set; }
        public string AddBanExpires { get; set; }
        public bool authenticated { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int serverID { get; set; }
        public int clientNo { get; set; }
        public int slot { get; set; }
        public string mapDetails { get; set; }
        public int warnID { get; set; }
        public string warning { get; set; }
        public string newWarning { get; set; }
        public string banReason { get; set; }
        public string newMsg { get; set; }
        public int interval { get; set; }
        public string PlayerName { get; set; }
        public string PlayerIP { get; set; }
        public int WarnLevel { get; set; }
        public bool DisallowVPN { get; set; }
        public string Description { get; set; }
        public bool GodModeStatus { get; set; }
        public string ServerName { get; set; }
        public string CountryCode { get; set; }
        public int SessionType { get; set; }
        public int TimeLimit { get; set; }
        public int StartDelay { get; set; }
        public int LoopMaps { get; set; }
        public int MaxSlots { get; set; }
        public int MaxKills { get; set; }
        public int ZoneTimer { get; set; }
        public bool WindowedMode { get; set; }
        public bool Dedicated { get; set; }
        public bool AllowAutoRange { get; set; }
        public bool MinPing { get; set; }
        public int MinPingValue { get; set; }
        public bool MaxPing { get; set; }
        public int MaxPingValue { get; set; }
        public int RespawnTime { get; set; }
        public bool RequireNovaLogin { get; set; }
        public bool AllowCustomSkins { get; set; }
        public string MOTD { get; set; }
        public string BluePassword { get; set; }
        public string RedPassword { get; set; }
        public bool FriendlyFire { get; set; }
        public bool FriendlyTags { get; set; }
        public bool ShowTeamClays { get; set; }
        public bool AutoBalance { get; set; }
        public bool FriendlyFireWarning { get; set; }
        public bool ShowTracers { get; set; }
        public bool AutoRange { get; set; }
        public int FBScore { get; set; }
        public int KOTHScore { get; set; }
        public int GameScore { get; set; }
        public bool EnablePing { get; set; }
        public int PingValue { get; set; }
        public WeaponsClass weapons { get; set; }
        public int PSPTime { get; set; }
        public int FlagReturnTime { get; set; }
        public int MaxTeamLives { get; set; }
        public int FriendlyFireKills { get; set; }
        public int ScoreboardDelay { get; set; }
        public List<MapList> MapCycle { get; set; }
        public string StartList { get; set; }
        public bool StartLoopMaps { get; set; }
        public int RotationID { get; set; }
        public string Rotation { get; set; }
        public string description { get; set; }
        public Version Version { get; set; }
        public Dictionary<int, UserCodes> SubUsers { get; set; }
        public Permissions userPermissions { get; set; }
        public bool superadmin { get; set; }
        public int subadmin { get; set; }
        public string _username { get; private set; }
        public void SetSessionUsername(string username)
        {
            _username = username;
        }

        public enum StatusCodes
        {
            INVALIDCOMMAND = -2,
            WELCOME = -1,
            NOTCONNECTED = 0,
            INVALIDLOGIN = 1,
            READY = 2,
            INVALIDSESSION = 3,
            LOGOUTSUCCESS = 4,
            LOGINSUCCESS = 5,
            SUCCESS = 6,
            FAILURE = 7,
            NOTAUTHENTICATED = 8,
            INVALIDINSTANCE = 9,
            INVALIDPLAYERNAME = 10,
            INVALIDIPADDRESS = 11,
            INVALIDBANREASON = 12,
            UPDATEREQUIRED = 13,
            USERALREADYEXISTS = 14,
        }
    }
}
