using HawkSync_SM.classes.logs;
using Newtonsoft.Json;
using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM.RCClasses
{
    public class RCFunctions
    {
        AppState _state;
        BanPlayerFunction cmdPlayer;
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const int VK_ENTER = 0x0D;
        const uint console = 0xC0;
        const uint GlobalChat = 0x54;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public RCFunctions(AppState state)
        {
            _state = state;
            cmdPlayer = new BanPlayerFunction();
        }
        public Dictionary<int, Instance> GetInstances(string sessionID)
        {
            Dictionary<int, Instance> RCInstancesConfig = new Dictionary<int, Instance>();
            foreach (var item in _state.Instances)
            {
                if (_state.Users[_state.rcClients[sessionID]._username].SuperAdmin == false && (!_state.Users[_state.rcClients[sessionID]._username].Permissions.InstancePermissions.ContainsKey(item.Key) || _state.Users[_state.rcClients[sessionID]._username].Permissions.InstancePermissions[item.Key].Access == false || _state.Users[_state.rcClients[sessionID]._username].Permissions.InstancePermissions[item.Key] == null))
                {
                    continue;
                }
                else
                {
                    RCInstancesConfig.Add(item.Key, new Instance
                    {
                        AllowAutoRange = item.Value.AllowAutoRange,
                        AllowCustomSkins = item.Value.AllowCustomSkins,
                        anti_stat_padding = item.Value.anti_stat_padding,
                        anti_stat_padding_min_minutes = item.Value.anti_stat_padding_min_minutes,
                        anti_stat_padding_min_players = item.Value.anti_stat_padding_min_players,
                        AutoBalance = item.Value.AutoBalance,
                        AutoMessages = item.Value.AutoMessages,
                        availableMaps = item.Value.availableMaps,
                        BanList = item.Value.BanList,
                        BindAddress = item.Value.BindAddress,
                        BluePassword = item.Value.BluePassword,
                        ChangeTeamList = item.Value.ChangeTeamList,
                        CountryCode = item.Value.CountryCode,
                        CustomWarnings = item.Value.CustomWarnings,
                        Dedicated = item.Value.Dedicated,
                        DestroyBuildings = item.Value.DestroyBuildings,
                        DisarmPlayers = item.Value.DisarmPlayers,
                        enableVPNCheck = item.Value.enableVPNCheck,
                        EnableWebStats = item.Value.EnableWebStats,
                        FatBullets = item.Value.FatBullets,
                        FBScore = item.Value.FBScore,
                        FlagReturnTime = item.Value.FlagReturnTime,
                        FriendlyFire = item.Value.FriendlyFire,
                        FriendlyFireKills = item.Value.FriendlyFireKills,
                        FriendlyFireWarning = item.Value.FriendlyFireWarning,
                        FriendlyTags = item.Value.FriendlyTags,
                        gameMapType = item.Value.gameMapType,
                        GameName = item.Value.GameName,
                        GameScore = item.Value.GameScore,
                        GameType = item.Value.GameType,
                        GameTypeName = item.Value.GameTypeName,
                        GodModeList = item.Value.GodModeList,
                        HostName = item.Value.HostName,
                        Id = item.Value.Id,
                        IPWhiteList = item.Value.IPWhiteList,
                        KOTHScore = item.Value.KOTHScore,
                        LoopMaps = item.Value.LoopMaps,
                        Map = item.Value.Map,
                        mapCounter = item.Value.mapCounter,
                        mapIndex = item.Value.mapIndex,
                        MapList = item.Value.MapList,
                        mapListCount = item.Value.mapListCount,
                        MaxKills = item.Value.MaxKills,
                        MaxPing = item.Value.MaxPing,
                        MaxPingValue = item.Value.MaxPingValue,
                        MaxSlots = item.Value.MaxSlots,
                        MaxTeamLives = item.Value.MaxTeamLives,
                        MinPing = item.Value.MinPing,
                        MinPingValue = item.Value.MinPingValue,
                        misc_left_leaning = item.Value.misc_left_leaning,
                        misc_show_ranks = item.Value.misc_show_ranks,
                        Mod = item.Value.Mod,
                        MOTD = item.Value.MOTD,
                        NumPlayers = item.Value.NumPlayers,
                        OneShotKills = item.Value.OneShotKills,
                        Password = item.Value.Password,
                        PlayerList = item.Value.PlayerList,
                        Plugins = item.Value.Plugins,
                        PSPTakeOverTime = item.Value.PSPTakeOverTime,
                        RedPassword = item.Value.RedPassword,
                        RequireNovaLogin = item.Value.RequireNovaLogin,
                        RespawnTime = item.Value.RespawnTime,
                        RoleRestrictions = item.Value.RoleRestrictions,
                        savedmaprotations = item.Value.savedmaprotations,
                        ServerName = item.Value.ServerName,
                        ScoreBoardDelay = item.Value.ScoreBoardDelay,
                        SessionType = item.Value.SessionType,
                        ShowTeamClays = item.Value.ShowTeamClays,
                        ShowTracers = item.Value.ShowTracers,
                        Slots = item.Value.Slots,
                        StartDelay = item.Value.StartDelay,
                        Status = item.Value.Status,
                        TimeLimit = item.Value.TimeLimit,
                        TimeRemaining = item.Value.TimeRemaining,
                        VPNWhiteList = item.Value.VPNWhiteList,
                        WeaponRestrictions = item.Value.WeaponRestrictions,
                        ZoneTimer = item.Value.ZoneTimer,
                    });
                }
            }
            return RCInstancesConfig;
        }
        public int GetVPNSettings(int InstanceID)
        {
            int InstanceIndex = -1;
            foreach (var item in _state.Instances)
            {
                if (item.Value.Id == InstanceID)
                {
                    InstanceIndex = item.Key;
                    break;
                }
                else
                {
                    continue;
                }
            }
            if (InstanceIndex == -1)
            {
                return -1;
            }
            else
            {
                return _state.IPQualityCache[InstanceIndex].WarnLevel;
            }
        }

        public RCListenerClass.StatusCodes DeleteWarning(int InstanceID, int messageid, string sessionid)
        {
            int InstanceIndex = -1;
            foreach (var item in _state.Instances)
            {
                if (item.Value.Id == InstanceID)
                {
                    InstanceIndex = item.Key;
                    break;
                }
                else
                {
                    continue;
                }
            }
            if (InstanceIndex != -1)
            {
                try
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `customwarnings` WHERE `instanceid` = @instanceid AND `message` = @message;", db);
                    cmd.Parameters.AddWithValue("@instanceid", _state.Instances[InstanceIndex].Id);
                    cmd.Parameters.AddWithValue("@message", _state.Instances[InstanceIndex].CustomWarnings[messageid]);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "DeleteWarning");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();

                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "DeleteWarning",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].CustomWarnings.RemoveAt(messageid);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                catch
                {
                    return RCListenerClass.StatusCodes.FAILURE;
                }
            }
            else
            {
                return RCListenerClass.StatusCodes.INVALIDINSTANCE;
            }
        }
        public RCListenerClass.StatusCodes CreateWarning(int InstanceID, string msg, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `customwarnings` (`id`, `instanceid`, `message`) VALUES (NULL, @instanceid, @warningMsg);", db);
                    cmd.Parameters.AddWithValue("@instanceid", InstanceID);
                    cmd.Parameters.AddWithValue("@warningMsg", msg);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "CreateWarning");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();

                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "CreateWarning",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].CustomWarnings.Add(msg);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes WarnPlayer(int InstanceID, string warning, int slotNum, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "WarnPlayer");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();

                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "WarnPlayer",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].WarningQueue.Add(new ob_WarnPlayerClass
                    {
                        slot = slotNum,
                        warningMsg = warning
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetCountryCodes(ref List<string> countryCodes)
        {
            try
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT `iso` FROM `country`;", db);
                SQLiteDataReader read = cmd.ExecuteReader();
                while (read.Read())
                {
                    countryCodes.Add(read.GetString(read.GetOrdinal("iso")));
                }
                cmd.Dispose();
                read.Close();
                read.Dispose();
                db.Close();
                db.Dispose();
                return RCListenerClass.StatusCodes.SUCCESS;
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }
        public RCListenerClass.StatusCodes KickPlayer(int InstanceID, int slotNum, string reason, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "KickPlayer");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "KickPlayer",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    cmdPlayer.BanPlayer(_state.Instances[InstanceIndex], slotNum, DateTime.Now.ToString(), reason, "", true);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes BanPlayer(int InstanceID, int slot, string banReason, string expires, string SessionID)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", SessionID);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[SessionID]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "BanPlayer");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[SessionID].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "BanPlayer",
                        Address = _state.rcClients[SessionID].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = SessionID,
                        Username = _state.rcClients[SessionID]._username
                    });
                    cmdPlayer.BanPlayer(_state.Instances[InstanceIndex], slot, expires, _state.rcClients[SessionID]._username, banReason, false);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes AddBan(int InstanceID, string playerName, string banReason, string playerIP, string expiresDate, string sessionID)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (InstanceIndex != -1)
                {
                    IPAddress ipaddress;

                    if (string.IsNullOrWhiteSpace(playerName) == true)
                    {
                        return RCListenerClass.StatusCodes.INVALIDPLAYERNAME;
                    }

                    if (IPAddress.TryParse(playerIP, out ipaddress) == false)
                    {
                        return RCListenerClass.StatusCodes.INVALIDIPADDRESS;
                    }

                    if (string.IsNullOrWhiteSpace(banReason) == true)
                    {
                        return RCListenerClass.StatusCodes.INVALIDBANREASON;
                    }

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand lastID_query =
                        new SQLiteCommand("SELECT `id` FROM `playerbans` ORDER BY `id` DESC LIMIT 1;", db);
                    int NextID = Convert.ToInt32(lastID_query.ExecuteScalar());
                    NextID++; // +1 for the NEXT ID
                    lastID_query.Dispose();

                    SQLiteCommand query = new SQLiteCommand(
                        "INSERT INTO `playerbans` (`id`, `profileid`, `player`, `ipaddress`, `dateadded`, `lastseen`, `reason`, `expires`, `bannedby`) VALUES (@newid, @profileid, @playername, @playerip, @dateadded, @date, @reason, @expires, @bannedby);",
                        db);
                    query.Parameters.AddWithValue("@newid", NextID);
                    query.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    query.Parameters.AddWithValue("@playername", playerName);
                    query.Parameters.AddWithValue("@playerip", ipaddress);
                    query.Parameters.AddWithValue("@date", DateTime.Now);
                    query.Parameters.AddWithValue("@dateadded", DateTime.Now);
                    query.Parameters.AddWithValue("@reason", banReason);
                    query.Parameters.AddWithValue("@expires", expiresDate);
                    query.Parameters.AddWithValue("@bannedby", _state.rcClients[sessionID]._username);
                    query.ExecuteNonQuery();
                    query.Dispose();

                    _state.Instances[InstanceIndex].BanList.Add(new ob_playerBanList
                    {
                        expires = expiresDate,
                        id = (int)db.LastInsertRowId,
                        ipaddress = ipaddress.ToString(),
                        lastseen = DateTime.Now,
                        newBan = true,
                        player = playerName,
                        reason = banReason,
                        retry = DateTime.Now,
                        bannedBy = _state.rcClients[sessionID]._username,
                        addedDate = DateTime.Now,
                        onlykick = false,
                        VPNBan = false
                    });

                    SQLiteCommand newEntryCmd =
                        new SQLiteCommand(
                            "INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);",
                            db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionID);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionID]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "AddBan");
                    newEntryCmd.Parameters.AddWithValue("@address",
                        _state.rcClients[sessionID].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "AddBan",
                        Address = _state.rcClients[sessionID].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionID,
                        Username = _state.rcClients[sessionID]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RCListenerClass.StatusCodes.FAILURE;
            }
            
        }

        public dynamic DisarmPlayer(int InstanceID, int slot, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "DisarmPlayer");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "DisarmPlayer",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].DisarmPlayers.Add(slot);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes KillPlayer(int InstanceID, int slotNum, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    Process process = Process.GetProcessById((int)_state.Instances[InstanceIndex].PID.GetValueOrDefault());
                    IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                    int buffer = 0;
                    byte[] PointerAddr9 = new byte[4];
                    var baseAddr = 0x400000;
                    var Pointer = baseAddr + 0x005ED600;

                    // read the playerlist memory address from the game...
                    ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                    var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                    byte[] playerListStartingLocationByteArray = new byte[4];
                    int playerListStartingLocationBuffer = 0;
                    ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                    int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                    for (int i = 1; i < slotNum; i++)
                    {
                        playerlistStartingLocation += 0xAF33C;
                    }
                    byte[] playerObjectLocationBytes = new byte[4];
                    int playerObjectLocationRead = 0;
                    ReadProcessMemory((int)processHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                    int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                    byte[] setPlayerHealth = BitConverter.GetBytes(0);
                    int setPlayerHealthWrite = 0;

                    WriteProcessMemory((int)processHandle, playerObjectLocation + 0x138, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                    WriteProcessMemory((int)processHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                    process.Dispose();
                    CloseHandle(processHandle);

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "KillPlayer");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "KillPlayer",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });

                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes DeleteAutoMsg(int InstanceID, int slot, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].AutoMessages.messages.Remove(_state.Instances[InstanceIndex].AutoMessages.messages[slot]);
                    SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                    conn.Open();
                    SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
                    automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].AutoMessages.messages));
                    automessages.ExecuteNonQuery();
                    conn.Close();
                    conn.Dispose();
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "DeleteAutoMsg");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "DeleteAutoMsg",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].AutoMessages.MsgNumber = 0;
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes EnableAutoMsg(int InstanceID, bool Enabled, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceID != -1)
                {
                    if (Enabled == true)
                    {
                        SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                        conn.Open();
                        SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `enable_msg` = 1 WHERE `profile_id` = @profileid;", conn);
                        automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                        automessages.ExecuteNonQuery();
                        automessages.Dispose();
                        conn.Close();
                        conn.Dispose();
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();

                        string action = Enabled ? "EnableAutoMsg" : "DisableAutoMsg";

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", action);
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = action,
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });
                        _state.Instances[InstanceIndex].AutoMessages.enable_msg = true;
                        _state.Instances[InstanceIndex].AutoMessages.MsgNumber = 0;
                    }
                    else
                    {
                        SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                        conn.Open();
                        SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `enable_msg` = 0 WHERE `profile_id` = @profileid;", conn);
                        automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                        automessages.ExecuteNonQuery();
                        automessages.Dispose();
                        conn.Close();
                        conn.Dispose();
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "DisableAutoMsg");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "DisableAutoMsg",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });
                        _state.Instances[InstanceIndex].AutoMessages.enable_msg = false;
                        _state.Instances[InstanceIndex].AutoMessages.MsgNumber = 0;
                    }
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes EnableGodMode(int InstanceID, int slotNum, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    if (_state.Instances[InstanceIndex].PlayerList.ContainsKey(slotNum))
                    {
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "EnableGodMode");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "EnableGodMode",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });
                        _state.Instances[InstanceIndex].GodModeList.Add(slotNum);
                        return RCListenerClass.StatusCodes.SUCCESS;
                    }
                    else
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateMapCycle(int InstanceID, List<MapList> mapCycle, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    serverManagerUpdateMemory.UpdateMapCycle(_state, InstanceIndex);
                    Thread.Sleep(1000);
                    _state.Instances[InstanceIndex].previousMapList = new Dictionary<int, MapList>();
                    foreach (var mapEntry in _state.Instances[InstanceIndex].MapList)
                    {
                        _state.Instances[InstanceIndex].previousMapList.Add(_state.Instances[InstanceIndex].previousMapList.Count, mapEntry.Value);
                    }

                    _state.Instances[InstanceIndex].MapList = new Dictionary<int, MapList>();
                    // convert dictionary to List<MapList>
                    List<MapList> mapCycleList = new List<MapList>();
                    foreach (var item in mapCycle)
                    {
                        mapCycleList.Add(item);
                    }

                    foreach (var map in mapCycleList)
                    {
                        _state.Instances[InstanceIndex].MapList.Add(_state.Instances[InstanceIndex].MapList.Count, map);
                    }
                    serverManagerUpdateMemory.UpdateMapCycle2(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `mapcycle` = @mapcycle WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@mapcycle", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].MapList));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();



                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateMapCycle");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateMapCycle",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes SendMsg(int InstanceID, int MsgLocation, string Msg, string sessionid)
        {
            /*  
                0. Global Blue
                2. Team Red
                3. Team Blue
            */
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    int colorbuffer_written = 0;
                    byte[] colorcode;
                    Process process = Process.GetProcessById(_state.Instances[InstanceIndex].PID.GetValueOrDefault());
                    IntPtr h = process.MainWindowHandle;
                    switch (MsgLocation)
                    {
                        case 1:
                            colorcode = HexConverter.ToByteArray("6A 04".Replace(" ", ""));
                            WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            break;
                        case 2:
                            colorcode = HexConverter.ToByteArray("6A 05".Replace(" ", ""));
                            WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            break;
                        default:
                            colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                            WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            break;
                    }
                    // post message
                    PostMessage(h, (ushort)WM_KEYDOWN, (ushort)GlobalChat, 0);
                    PostMessage(h, (ushort)WM_KEYUP, (ushort)GlobalChat, 0);
                    Thread.Sleep(50);
                    int bytesWritten = 0;
                    byte[] buffer;
                    buffer = Encoding.Default.GetBytes($"{Msg}\0"); // '\0' marks the end of string
                    WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                    Thread.Sleep(50);
                    PostMessage(h, (ushort)WM_KEYDOWN, (ushort)VK_ENTER, 0);
                    PostMessage(h, (ushort)WM_KEYUP, (ushort)VK_ENTER, 0);

                    // change color to normal
                    Thread.Sleep(50);
                    int revert_colorbuffer = 0;
                    byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                    WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                    process.Dispose();

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "SendMsg");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "SendMsg",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });

                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateRespawnTime(int InstanceID, int respawnTime, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].RespawnTime = respawnTime;
                    serverManagerUpdateMemory.UpdateRespawnTime(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `respawn_time` = @respawntime WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@respawntime", _state.Instances[InstanceIndex].RespawnTime);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();


                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateRespawnTime");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateRespawnTime",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateFlagReturnTime(int InstanceID, int flagReturnTime, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].FlagReturnTime = flagReturnTime;
                    serverManagerUpdateMemory.UpdateFlagReturnTime(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `flagreturntime` = @flagreturntime WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@flagreturntime", _state.Instances[InstanceIndex].FlagReturnTime);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateFlagReturnTime");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateFlagReturnTime",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateFriendlyFireKills(int InstanceID, int friendlyFireKills, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].FriendlyFireKills = friendlyFireKills;
                    serverManagerUpdateMemory.UpdateFriendlyFireKills(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `friendly_fire_kills` = @friendly_fire_kills WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@friendly_fire_kills", _state.Instances[InstanceIndex].FriendlyFireKills);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateFriendlyFireKills");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateFriendlyFireKills",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes StartInstance(int InstanceID, string server_name, string motd, string country_code, string server_password, int session_type, int max_slots, int start_delay, bool startLoopMaps, int FBScore, int game_score, int zone_timer, int respawn_time, int time_limit, bool require_novalogic, bool run_windowed, bool allow_custom_skins, bool dedicated, string blue_team_password, string red_team_password, bool friendly_fire, bool friendly_fire_warning, bool friendly_tags, bool auto_balance, bool show_tracers, bool show_team_clays, bool allow_auto_range, bool enable_min_ping, int min_ping, bool enable_max_ping, int max_ping, int game_mod, string startList, string sessionid)
        {
            ServerManagement serverManagerUpdateMemory = new ServerManagement();

            // DB Connection
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
            conn.Open();

            // Get Instance Index
            int InstanceIndex = -1;
            foreach (var item in _state.Instances)
            {
                if (item.Value.Id == InstanceID)
                {
                    InstanceIndex = item.Key;
                    break;
                }
                else
                {
                    continue;
                }
            }
        
            // Invalid Instance
            if (InstanceIndex == -1) {
                return RCListenerClass.StatusCodes.INVALIDINSTANCE;
            }
            
            //JsonConvert.SerializeObject(selectedMapList)
            List<MapList> mapList = new List<MapList>();
            mapList = JsonConvert.DeserializeObject<List<MapList>>(startList);

            // Map List
            _state.Instances[InstanceIndex].MapList = new Dictionary<int, MapList>();
            foreach (var map in mapList)
            {
                _state.Instances[InstanceIndex].MapList.Add(_state.Instances[InstanceIndex].MapList.Count, map);
            }

            // Starting Server
            Instance startInstance = new Instance();
            startInstance = _state.Instances[InstanceIndex];

            // Collect Form Data
            startInstance.ServerName = server_name;                             // Server Name
            startInstance.MOTD = motd;                                          // Server MOTD
            startInstance.CountryCode = country_code;                           // Country Code
            startInstance.Password = server_password;                           // Server Global Password
            startInstance.SessionType = session_type;                           // Session Type (0 = Internet, 1 = LAN) - Doesn't work and Form Changed.
            startInstance.MaxSlots = max_slots + 1;                             // Max Players
            startInstance.StartDelay = start_delay;                             // Start Delay
            startInstance.LoopMaps = startLoopMaps ? 1 : 0;                     // Loop Maps
            startInstance.FBScore = FBScore;                                    // Flag Score
            startInstance.GameScore = game_score;                               // Game Score
            startInstance.ZoneTimer = zone_timer;                               // Zone Timer
            startInstance.RespawnTime = respawn_time;                           // Respawn Time
            startInstance.TimeLimit = time_limit;                               // Time Limit
            startInstance.RequireNovaLogin = require_novalogic;                 // Require NovaLogic Login
            startInstance.Dedicated = dedicated;                                // Run Dedicated
            startInstance.WindowedMode = run_windowed;                          // Windowed Mode
            startInstance.AllowCustomSkins = allow_custom_skins;                // Allow Custom Skins
            startInstance.Mod = game_mod;                                       // Game Mod
            startInstance.BluePassword = blue_team_password;                    // Blue Team Password
            startInstance.RedPassword = red_team_password;                      // Red Team Password
            startInstance.FriendlyFire = friendly_fire;                         // Friendly Fire Flag
            startInstance.FriendlyFireWarning = friendly_fire_warning;          // Friendly Fire Warning
            startInstance.FriendlyTags = friendly_tags;                         // Friendly Tags
            startInstance.AutoBalance = auto_balance;                           // Auto Balance
            startInstance.ShowTracers = show_tracers;                           // Show Tracers
            startInstance.ShowTeamClays = show_team_clays;                      // Show Team Clays
            startInstance.AllowAutoRange = allow_auto_range;                    // Allow Auto Range
            startInstance.MinPing = enable_min_ping;                            // Enable Min Ping
            startInstance.MinPingValue = min_ping;                              // Min Ping Value
            startInstance.MaxPing = enable_max_ping;                            // Enable Max Ping
            startInstance.MaxPingValue = max_ping;                              // Max Ping Value            
            startInstance.LastUpdateTime = DateTime.Now;
            startInstance.NextUpdateTime = DateTime.Now.AddSeconds(5.0);

            // Generate AutoRes.bin
            if (!(serverManagerUpdateMemory.createAutoRes(startInstance, _state)))
            {
                MessageBox.Show("Failed to create AutoRes.bin", "Error");
                return RCListenerClass.StatusCodes.FAILURE;
            }

            // Start Game
            // Check for PID in Database, if not found create a record.
            SQLiteCommand checkPidQuery = new SQLiteCommand("SELECT COUNT(*) FROM `instances_pid` WHERE `profile_id` = @instanceId;", conn);
            checkPidQuery.Parameters.AddWithValue("@instanceId", _state.Instances[InstanceIndex].Id);
            int checkPid = Convert.ToInt32(checkPidQuery.ExecuteScalar());
            checkPidQuery.Dispose();

            if (checkPid == 0)
            {
                SQLiteCommand insert_cmd = new SQLiteCommand("INSERT INTO `instances_pid` (`profile_id`, `pid`) VALUES (@instanceid, 0)", conn);
                insert_cmd.Parameters.AddWithValue("@instanceid", _state.Instances[InstanceIndex].Id);
                insert_cmd.ExecuteNonQuery();
            }

            serverManagerUpdateMemory.startGame(startInstance, _state, InstanceIndex, conn);

            // Sucess Update Database & Return Instance Array
            SQLiteCommand update_query = new SQLiteCommand("UPDATE `instances_config` SET `server_name` = @servername, `motd` = @motd, `country_code` = @countrycode, `server_password` = @server_password,`session_type` = @sessiontype, `max_slots` = @max_slots, `start_delay` = @start_delay, `loop_maps` = @loop_maps, `game_score` = @game_score, `fbscore` = @flag_scores, `zone_timer` = @zone_timer, `respawn_time` = @respawn_time, `time_limit` = @time_limit, `require_novalogic` = @require_novalogic, `windowed_mode` = @run_windowed, `allow_custom_skins` = @allow_custom_skins, `run_dedicated` = @run_dedicated, `game_mod` = @game_mod, `mapcycle` = @selected_maps, `blue_team_password` = @blue_team_password, `red_team_password` = @red_team_password, `friendly_fire` = @friendly_fire, `friendly_fire_warning` = @friendly_fire_warning, `friendly_tags` = @friendly_tags, `auto_balance` = @auto_balance, `show_tracers` = @show_tracers, `show_team_clays` = @show_team_clays, `allow_auto_range` = @allow_auto_range, `enable_min_ping` = @enable_min_ping, `min_ping` = @min_ping, `enable_max_ping` = @enable_max_ping, `max_ping` = @max_ping, `availablemaps` = @availablemaps WHERE `profile_id` = @profile_id", conn);

            update_query.Parameters.AddWithValue("@servername", startInstance.ServerName);
            update_query.Parameters.AddWithValue("@motd", startInstance.MOTD);
            update_query.Parameters.AddWithValue("@countrycode", startInstance.CountryCode);
            update_query.Parameters.AddWithValue("@sessiontype", startInstance.SessionType);
            update_query.Parameters.AddWithValue("@server_password", startInstance.Password);
            update_query.Parameters.AddWithValue("@max_slots", startInstance.MaxSlots);
            update_query.Parameters.AddWithValue("@start_delay", startInstance.StartDelay);
            update_query.Parameters.AddWithValue("@loop_maps", startInstance.LoopMaps);
            update_query.Parameters.AddWithValue("@game_score", startInstance.GameScore);
            update_query.Parameters.AddWithValue("@flag_scores", startInstance.GameScore);
            update_query.Parameters.AddWithValue("@zone_timer", startInstance.ZoneTimer);
            update_query.Parameters.AddWithValue("@respawn_time", startInstance.RespawnTime);
            update_query.Parameters.AddWithValue("@time_limit", startInstance.TimeLimit);
            update_query.Parameters.AddWithValue("@require_novalogic", startInstance.RequireNovaLogin ? 1 : 0);
            update_query.Parameters.AddWithValue("@run_windowed", startInstance.WindowedMode ? 1 : 0);
            update_query.Parameters.AddWithValue("@allow_custom_skins", startInstance.AllowCustomSkins ? 1 : 0);
            update_query.Parameters.AddWithValue("@run_dedicated", startInstance.Dedicated ? 1 : 0);
            update_query.Parameters.AddWithValue("@game_mod", startInstance.Mod);
            update_query.Parameters.AddWithValue("@blue_team_password", startInstance.BluePassword);
            update_query.Parameters.AddWithValue("@red_team_password", startInstance.RedPassword);
            update_query.Parameters.AddWithValue("@friendly_fire", startInstance.FriendlyFire);
            update_query.Parameters.AddWithValue("@friendly_fire_warning", startInstance.FriendlyFireWarning ? 1 : 0);
            update_query.Parameters.AddWithValue("@friendly_tags", startInstance.FriendlyTags ? 1 : 0);
            update_query.Parameters.AddWithValue("@auto_balance", startInstance.AutoBalance ? 1 : 0);
            update_query.Parameters.AddWithValue("@show_tracers", startInstance.ShowTracers ? 1 : 0);
            update_query.Parameters.AddWithValue("@show_team_clays", startInstance.ShowTeamClays ? 1 : 0);
            update_query.Parameters.AddWithValue("@allow_auto_range", startInstance.AllowAutoRange ? 1 : 0);
            update_query.Parameters.AddWithValue("@enable_min_ping", startInstance.MinPing ? 1 : 0);
            update_query.Parameters.AddWithValue("@min_ping", startInstance.MinPingValue);
            update_query.Parameters.AddWithValue("@enable_max_ping", startInstance.MaxPing ? 1 : 0);
            update_query.Parameters.AddWithValue("@max_ping", startInstance.MaxPingValue);
            update_query.Parameters.AddWithValue("@profile_id", _state.Instances[InstanceIndex].Id);

            update_query.Parameters.AddWithValue("@availablemaps", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].availableMaps));
            update_query.Parameters.AddWithValue("@selected_maps", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].MapList));
            update_query.ExecuteNonQuery();
            update_query.Dispose();


            conn.Close();
            conn.Dispose();

            _state.RCLogs.Add(new RCLogs
            {
                Action = "UpdateFriendlyFireKills",
                Address = _state.rcClients[sessionid].RemoteAddress,
                Date = DateTime.Now,
                SessionID = sessionid,
                Username = _state.rcClients[sessionid]._username
            });

            return RCListenerClass.StatusCodes.SUCCESS;

        }

        public RCListenerClass.StatusCodes StopInstance(int InstanceID, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                _state.ApplicationProcesses[InstanceIndex].Kill();

                // Call Regardless
                _state.Instances[InstanceID].Firewall.DeleteFirewallRules(_state.Instances[InstanceID].GameName, "Allow");
                _state.Instances[InstanceID].Firewall.DeleteFirewallRules(_state.Instances[InstanceID].GameName, "Deny");

                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();

                SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                newEntryCmd.Parameters.AddWithValue("@action", "StopInstance");
                newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                newEntryCmd.ExecuteNonQuery();
                newEntryCmd.Dispose();
                db.Close();
                db.Dispose();
                _state.RCLogs.Add(new RCLogs
                {
                    Action = "StopInstance",
                    Address = _state.rcClients[sessionid].RemoteAddress,
                    Date = DateTime.Now,
                    SessionID = sessionid,
                    Username = _state.rcClients[sessionid]._username
                });
                return RCListenerClass.StatusCodes.SUCCESS;
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateScoreboardDelay(int InstanceID, int scoreboardDelay, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].ScoreBoardDelay = scoreboardDelay;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `scoreboard_override` = @scoreboard_override WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@scoreboard_override", _state.Instances[InstanceIndex].ScoreBoardDelay);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateScoreboardDelay");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateScoreboardDelay",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateMaxTeamLives(int InstanceID, int maxTeamLives, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].MaxTeamLives = maxTeamLives;
                    serverManagerUpdateMemory.UpdateMaxTeamLives(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `max_team_lives` = @MaxTeamLives WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@MaxTeamLives", _state.Instances[InstanceIndex].MaxTeamLives);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateMaxTeamLives");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateMaxTeamLives",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdatePSPTime(int InstanceID, int pSPTime, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].PSPTakeOverTime = pSPTime;
                    serverManagerUpdateMemory.UpdatePSPTakeOverTime(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `psptakeover` = @PSPTakeOverTime WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@PSPTakeOverTime", _state.Instances[InstanceIndex].PSPTakeOverTime);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdatePSPTime");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdatePSPTime",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateAllowCustomSkins(int InstanceID, bool allowCustomSkins, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].AllowCustomSkins = allowCustomSkins;
                    serverManagerUpdateMemory.UpdateAllowCustomSkins(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `allow_custom_skins` = @allow_custom_skins WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@allow_custom_skins", _state.Instances[InstanceIndex].AllowCustomSkins);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateAllowCustomSkins");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateAllowCustomSkins",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateBluePassword(int InstanceID, string bluePassword, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    int oldPw = _state.Instances[InstanceIndex].BluePassword.Length;
                    _state.Instances[InstanceIndex].BluePassword = bluePassword;
                    serverManagerUpdateMemory.UpdateBluePassword(_state, InstanceIndex, oldPw);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `blue_team_password` = @blue_team_password WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@blue_team_password", _state.Instances[InstanceIndex].BluePassword);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();


                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateBluePassword");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateBluePassword",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetPlayerIPInfo(int InstanceID, int slot, out ipqualityClass ipqualityInfo)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    for (int i = 0; i < _state.IPQualityCache[InstanceIndex].IPInformation.Count; i++)
                    {
                        if (_state.IPQualityCache[InstanceIndex].IPInformation[i].address == _state.Instances[InstanceIndex].PlayerList[slot].address)
                        {
                            ipqualityInfo = _state.IPQualityCache[InstanceIndex].IPInformation[i];
                            return RCListenerClass.StatusCodes.SUCCESS;
                        }
                    }
                    ipqualityInfo = null;
                    return RCListenerClass.StatusCodes.FAILURE;
                }
                else
                {
                    ipqualityInfo = null;
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                ipqualityInfo = null;
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetAdminNotes(int InstanceID, int slot, out List<adminnotes> notes)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    notes = new List<adminnotes>();
                    foreach (ob_playerHistory player in _state.playerHistories)
                    {
                        if ((player.playerName == _state.Instances[InstanceIndex].PlayerList[slot].name) || (player.playerIP == _state.Instances[InstanceIndex].PlayerList[slot].address))
                        {
                            foreach (var note in _state.adminNotes)
                            {
                                if (note.userid == player.DatabaseId)
                                {
                                    notes.Add(note);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    notes = null;
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                notes = null;
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes RemoveAdminNote(string playerName, string msg, string sessionid)
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `adminnotes` WHERE `name` = @playername AND `msg` = @msg;", db);
            cmd.Parameters.AddWithValue("@playername", playerName);
            cmd.Parameters.AddWithValue("@msg", msg);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
            newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
            newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
            newEntryCmd.Parameters.AddWithValue("@action", "RemoveAdminNote");
            newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
            newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
            newEntryCmd.ExecuteNonQuery();
            newEntryCmd.Dispose();
            db.Close();
            db.Dispose();
            _state.RCLogs.Add(new RCLogs
            {
                Action = "RemoveAdminNote",
                Address = _state.rcClients[sessionid].RemoteAddress,
                Date = DateTime.Now,
                SessionID = sessionid,
                Username = _state.rcClients[sessionid]._username
            });

            adminnotes note = (adminnotes)_state.adminNotes.Single(x => x.name == playerName && x.msg == msg);
            _state.adminNotes.Remove(note);
            return RCListenerClass.StatusCodes.SUCCESS;
        }

        public RCListenerClass.StatusCodes AddAdminNote(string playerIP, string playerName, string note, string sessionid)
        {
            try
            {
                int PlayerID = -1;
                bool found = false;
                foreach (var player in _state.playerHistories)
                {
                    if ((player.playerName == playerName) && (player.playerIP == playerIP))
                    {
                        PlayerID = player.DatabaseId;
                        found = true;
                        break;
                    }
                }
                if (found == false || PlayerID == -1)
                {
                    return RCListenerClass.StatusCodes.FAILURE;
                }
                if (!string.IsNullOrWhiteSpace(note))
                {
                    _state.adminNotes.Add(new adminnotes
                    {
                        userid = PlayerID,
                        name = playerName,
                        msg = note
                    });
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `adminnotes` (`userid`, `name`, `msg`) VALUES (@newid, @playername, @msg);", db);
                    cmd.Parameters.AddWithValue("@newid", PlayerID);
                    cmd.Parameters.AddWithValue("@playername", playerName);
                    cmd.Parameters.AddWithValue("@msg", note);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "AddAdminNote");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "AddAdminNote",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.FAILURE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetPlayerHistory(int InstanceID, int slot, out List<ob_playerHistory> history)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    history = new List<ob_playerHistory>();
                    foreach (ob_playerHistory player in _state.playerHistories)
                    {
                        if ((player.playerName == _state.Instances[InstanceIndex].PlayerList[slot].name) || (player.playerIP == _state.Instances[InstanceIndex].PlayerList[slot].address))
                        {
                            history.Add(player);
                        }
                    }
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    history = null;
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                history = null;
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateFBScore(int InstanceID, int fBScore, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].FBScore = fBScore;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `fbscore` = @fbscore WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@fbscore", _state.Instances[InstanceIndex].FBScore);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateFBScore");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateFBScore",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateGameScore(int InstanceID, int gameScore, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].GameScore = gameScore;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `game_score` = @game_score WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@game_score", _state.Instances[InstanceIndex].GameScore);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateGameScore");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateGameScore",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateKOTHScore(int InstanceID, int kOTHScore, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].KOTHScore = kOTHScore;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `kothscore` = @kothscore WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@kothscore", _state.Instances[InstanceIndex].KOTHScore);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateKOTHScore");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateKOTHScore",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateMaxPing(int InstanceID, bool enablePing, int pingValue, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].MaxPing = enablePing;
                    _state.Instances[InstanceIndex].MaxPingValue = pingValue;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `enable_max_ping` = @enableMaxPing, `max_ping` = @maxPingValue WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@enableMaxPing", Convert.ToInt32(_state.Instances[InstanceIndex].MaxPing));
                    updateDB.Parameters.AddWithValue("@maxPingValue", (int)_state.Instances[InstanceIndex].MaxPingValue);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateMaxPing");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateMaxPing",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateWeapons(int InstanceID, WeaponsClass weapons, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].WeaponRestrictions = weapons;
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    serverManagerUpdateMemory.UpdateWeaponRestrictions(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateWeapons");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `weaponrestrictions` = @weaponrestrictions WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@weaponrestrictions", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].WeaponRestrictions));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateWeapons",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateMinPing(int InstanceID, bool enablePing, int pingValue, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].MinPing = enablePing;
                    _state.Instances[InstanceIndex].MinPingValue = pingValue;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `enable_min_ping` = @enableMinPing, `min_ping` = @minPingValue WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@enableMinPing", Convert.ToInt32(_state.Instances[InstanceIndex].MinPing));
                    updateDB.Parameters.AddWithValue("@minPingValue", _state.Instances[InstanceIndex].MinPingValue);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();


                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateMinPing");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateMinPing",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateGamePlayOptions(int InstanceID, bool friendlyFire, bool friendlyTags, bool showTeamClays, bool autoBalance, bool friendlyFireWarning, bool showTracers, bool allowAutoRange, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].FriendlyFire = friendlyFire;
                    _state.Instances[InstanceIndex].FriendlyTags = friendlyTags;
                    _state.Instances[InstanceIndex].ShowTeamClays = showTeamClays;
                    _state.Instances[InstanceIndex].AutoBalance = autoBalance;
                    _state.Instances[InstanceIndex].FriendlyFireWarning = friendlyFireWarning;
                    _state.Instances[InstanceIndex].ShowTracers = showTracers;
                    _state.Instances[InstanceIndex].AllowAutoRange = allowAutoRange;
                    serverManagerUpdateMemory.GamePlayOptions(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `friendly_fire` = @friendlyfire, `friendly_tags` = @friendlytags, `show_team_clays` = @showTeamClays, `auto_balance` = @autoBalance, `friendly_fire_warning` = @friendlyFireWarning, `show_tracers` = @showTracers, `allow_auto_range` = @allowAutoRange WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@friendlyfire", Convert.ToInt32(_state.Instances[InstanceIndex].FriendlyFire));
                    updateDB.Parameters.AddWithValue("@friendlytags", Convert.ToInt32(_state.Instances[InstanceIndex].FriendlyTags));
                    updateDB.Parameters.AddWithValue("@showTeamClays", Convert.ToInt32(_state.Instances[InstanceIndex].ShowTeamClays));
                    updateDB.Parameters.AddWithValue("@autoBalance", Convert.ToInt32(_state.Instances[InstanceIndex].AutoBalance));
                    updateDB.Parameters.AddWithValue("@friendlyFireWarning", Convert.ToInt32(_state.Instances[InstanceIndex].FriendlyFireWarning));
                    updateDB.Parameters.AddWithValue("@showTracers", Convert.ToInt32(_state.Instances[InstanceIndex].ShowTracers));
                    updateDB.Parameters.AddWithValue("@allowAutoRange", Convert.ToInt32(_state.Instances[InstanceIndex].AllowAutoRange));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateGamePlayOptions");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateGamePlayOptions",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateRedPassword(int InstanceID, string redPassword, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    int oldPw = _state.Instances[InstanceIndex].RedPassword.Length;
                    _state.Instances[InstanceIndex].RedPassword = redPassword;
                    serverManagerUpdateMemory.UpdateRedPassword(_state, InstanceIndex, oldPw);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `red_team_password` = @redPassword WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@redPassword", _state.Instances[InstanceIndex].RedPassword);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateRedPassword");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateRedPassword",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateMOTD(int InstanceID, string MOTD, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].MOTD = MOTD;
                    serverManagerUpdateMemory.UpdateMOTD(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `motd` = @motd WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@motd", _state.Instances[InstanceIndex].MOTD);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateMOTD");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateMOTD",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateRequireNovaLogin(int InstanceID, bool requireNovaLogin, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].RequireNovaLogin = requireNovaLogin;
                    serverManagerUpdateMemory.UpdateRequireNovaLogin(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `require_nova` = @requireNova WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@requireNova", Convert.ToInt32(_state.Instances[InstanceIndex].RequireNovaLogin));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();


                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateRequireNovaLogin");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateRequireNovaLogin",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateLoopMaps(int InstanceID, int loopMaps, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].LoopMaps = loopMaps;
                    serverManagerUpdateMemory.UpdateLoopMaps(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `loop_maps` = @loopMaps WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@loopMaps", _state.Instances[InstanceIndex].LoopMaps);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateLoopMaps");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateLoopMaps",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateStartDelay(int InstanceID, int startDelay, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].StartDelay = startDelay;
                    serverManagerUpdateMemory.UpdateStartDelay(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `start_delay` = @startDelay WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@startDelay", _state.Instances[InstanceIndex].StartDelay);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateStartDelay");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateStartDelay",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateTimeLimit(int InstanceID, int timeLimit, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].TimeLimit = timeLimit;
                    serverManagerUpdateMemory.UpdateTimeLimit(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `time_limit` = @timeLimit WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@timeLimit", _state.Instances[InstanceIndex].TimeLimit);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateTimeLimit");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateTimeLimit",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateSessionType(int InstanceID, int sessionType, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].SessionType = sessionType;
                    serverManagerUpdateMemory.UpdateSessionType(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `session_type` = @sessionType WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@sessionType", _state.Instances[InstanceIndex].SessionType);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateSessionType");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateSessionType",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateServerPassword(int InstanceID, string password, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].Password = password;
                    serverManagerUpdateMemory.UpdateServerPassword(_state, InstanceIndex, password.Length);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `server_password` = @serverPassword WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@serverPassword", _state.Instances[InstanceIndex].Password);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateServerPassword");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateServerPassword",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateCountryCode(int InstanceID, string countryCode, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].CountryCode = countryCode;
                    serverManagerUpdateMemory.UpdateCountryCode(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `country_code` = @countryCode WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@countryCode", _state.Instances[InstanceIndex].CountryCode);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateCountryCode");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateCountryCode",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateServerName(int InstanceID, string serverName, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    _state.Instances[InstanceIndex].ServerName = serverName;
                    serverManagerUpdateMemory.UpdateServerName(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `server_name` = @serverName WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@serverName", _state.Instances[InstanceIndex].ServerName);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateServerName");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateServerName",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes DisableGodMode(int InstanceID, int slotNum, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    if (_state.Instances[InstanceIndex].GodModeList.Contains(slotNum))
                    {
                        _state.Instances[InstanceIndex].GodModeList.Remove(slotNum);
                        int buffer = 0;
                        byte[] PointerAddr9 = new byte[4];
                        var baseAddr = 0x400000;
                        var Pointer = baseAddr + 0x005ED600;

                        // read the playerlist memory address from the game...
                        ReadProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                        var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                        byte[] playerListStartingLocationByteArray = new byte[4];
                        int playerListStartingLocationBuffer = 0;
                        ReadProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                        int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                        for (int i = 1; i < slotNum; i++)
                        {
                            playerlistStartingLocation += 0xAF33C;
                        }
                        byte[] playerObjectLocationBytes = new byte[4];
                        int playerObjectLocationRead = 0;
                        ReadProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                        int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                        byte[] setPlayerHealth = BitConverter.GetBytes(100); //set god mode health
                        int setPlayerHealthWrite = 0;

                        byte[] setDamageBy = BitConverter.GetBytes(0);
                        int setDamageByWrite = 0;

                        WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, playerObjectLocation + 0x138, setDamageBy, setDamageBy.Length, ref setDamageByWrite);
                        WriteProcessMemory((int)_state.Instances[InstanceIndex].ProcessHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);

                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "DisableGodMode");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "DisableGodMode",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });

                        return RCListenerClass.StatusCodes.SUCCESS;
                    }
                    else
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes EnableVPNCheck(int InstanceID, string sessionid)
        {
            if (ProgramConfig.EnableVPNCheck)
            {
                return RCListenerClass.StatusCodes.SUCCESS;
            }
            return RCListenerClass.StatusCodes.FAILURE;
        }

        public RCListenerClass.StatusCodes AddVPN(int InstanceID, string description, string address, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceID != -1)
                {
                    if (IPAddress.TryParse(address, out IPAddress PlayerIP))
                    {
                        _state.Instances[InstanceIndex].VPNWhiteList.Add(_state.Instances[InstanceIndex].VPNWhiteList.Count, new ob_ipWhitelist
                        {
                            Description = description,
                            IPAddress = PlayerIP.ToString()
                        });
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();
                        SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `vpnwhitelist` (`profile_id`, `description`, `address`) VALUES (@profileid, @description, @PublicIP);", db);
                        cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@PublicIP", PlayerIP.ToString());
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "AddVPN");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "AddVPN",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });
                        return RCListenerClass.StatusCodes.SUCCESS;
                    }
                    else
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes DisallowVPN(int InstanceID, bool DisallowVPN, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `enableVPNCheck` = @enableVPNCheck WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@enableVPNCheck", Convert.ToInt32(_state.Instances[InstanceIndex].enableVPNCheck));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "DisallowVPN");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "DisallowVPN",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].enableVPNCheck = DisallowVPN;
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateWarnLevel(int InstanceID, int warnLevel, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `warnlevel` = @warnLevel WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@warnLevel", _state.IPQualityCache[InstanceIndex].WarnLevel);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    updateDB.ExecuteNonQuery();
                    updateDB.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "UpdateWarnLevel");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "UpdateWarnLevel",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.IPQualityCache[InstanceIndex].WarnLevel = warnLevel;
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes RemoveBan(int InstanceID, string playerName, string playerIP, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    int playerIndex = -1;
                    for (int i = 0; i < _state.Instances[InstanceIndex].BanList.Count; i++)
                    {
                        if (_state.Instances[InstanceIndex].BanList[i].ipaddress == playerIP && _state.Instances[InstanceIndex].BanList[i].player == playerName)
                        {
                            playerIndex = i;
                            break;
                        }
                    }
                    if (playerIndex == -1)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else
                    {
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();
                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `playerbans` WHERE `id` = @banid AND `profileid` = @profileid;", db);
                        cmd.Parameters.AddWithValue("@banid", _state.Instances[InstanceIndex].BanList[playerIndex].id);
                        cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "RemoveBan");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "RemoveBan",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });
                        _state.Instances[InstanceIndex].BanList.RemoveAt(playerIndex);
                        return RCListenerClass.StatusCodes.SUCCESS;
                    }
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes ChangeAutoMsgInterval(int InstanceID, int interval, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].AutoMessages.interval = interval;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_msg_interval` = @value WHERE `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@value", interval);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "ChangeAutoMsgInterval");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "ChangeAutoMsgInterval",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes AddAutoMsg(int InstanceID, string newMsg, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].AutoMessages.messages.Add(newMsg);
                    SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                    conn.Open();
                    SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
                    automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].AutoMessages.messages));
                    automessages.ExecuteNonQuery();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", conn);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "AddAutoMsg");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "AddAutoMsg",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    _state.Instances[InstanceIndex].AutoMessages.MsgNumber = 0;
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes SetNextMap(int InstanceID, int slot, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    ServerManagement serverManagerUpdateMemory = new ServerManagement();
                    serverManagerUpdateMemory.UpdateNextMap(_state, InstanceIndex, slot);
                    _state.Instances[InstanceIndex].mapListCount = _state.Instances[InstanceIndex].MapList.Count;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "SetNextMap");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "SetNextMap",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes ScoreMap(int InstanceID, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    if (_state.Instances[InstanceIndex].Status == InstanceStatus.LOADINGMAP)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].Status == InstanceStatus.OFFLINE)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].Status == InstanceStatus.SCORING)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].Status == InstanceStatus.STARTDELAY || _state.Instances[InstanceIndex].Status == InstanceStatus.ONLINE)
                    {
                        Process process = Process.GetProcessById(_state.Instances[InstanceIndex].PID.GetValueOrDefault());
                        IntPtr h = process.MainWindowHandle;
                        IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                        // open cmdConsole
                        PostMessage(h, (ushort)WM_KEYDOWN, (ushort)console, 0);
                        PostMessage(h, (ushort)WM_KEYUP, (ushort)console, 0);
                        System.Threading.Thread.Sleep(50);
                        int bytesWritten = 0;
                        byte[] buffer = Encoding.Default.GetBytes("resetgames\0"); // '\0' marks the end of string
                        WriteProcessMemory((int)processHandle, 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                        System.Threading.Thread.Sleep(50);
                        PostMessage(h, (ushort)WM_KEYDOWN, (ushort)VK_ENTER, 0);
                        PostMessage(h, (ushort)WM_KEYUP, (ushort)VK_ENTER, 0);
                        process.Dispose();
                        CloseHandle(processHandle);

                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "ScoreMap");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();
                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "ScoreMap",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });

                        return RCListenerClass.StatusCodes.SUCCESS;
                    }
                    else
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                }
                else
                {
                    return RCListenerClass.StatusCodes.FAILURE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes ChangeTeam(int InstanceID, int slotNum, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceID != -1)
                {
                    ob_playerList.Teams currentTeam = (ob_playerList.Teams)_state.Instances[InstanceIndex].PlayerList[slotNum].team;
                    ob_playerList.Teams switchTeam = (ob_playerList.Teams)ob_playerList.Teams.TEAM_SPEC;

                    if (currentTeam == ob_playerList.Teams.TEAM_BLUE)
                    {
                        switchTeam = ob_playerList.Teams.TEAM_RED;
                    }
                    else if (currentTeam == ob_playerList.Teams.TEAM_RED)
                    {
                        switchTeam = ob_playerList.Teams.TEAM_BLUE;
                    }

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "ChangeTeam");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "ChangeTeam",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });

                    _state.Instances[InstanceIndex].ChangeTeamList.Add(new ob_playerChangeTeamList
                    {
                        slotNum = slotNum,
                        Team = (int)switchTeam
                    });
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes RearmPlayer(int InstanceID, int slotNum, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (InstanceIndex != -1)
                {
                    _state.Instances[InstanceIndex].DisarmPlayers.Remove(slotNum);
                    Process process = Process.GetProcessById((int)_state.Instances[InstanceIndex].PID.GetValueOrDefault());
                    IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                    int buffer = 0;
                    byte[] PointerAddr9 = new byte[4];
                    var baseAddr = 0x400000;
                    var Pointer = baseAddr + 0x005ED600;
                    ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                    var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;

                    byte[] playerListStartingLocationByteArray = new byte[4];
                    int playerListStartingLocationBuffer = 0;
                    ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                    int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                    for (int slot = 1; slot < slotNum; slot++)
                    {
                        playerlistStartingLocation += 0xAF33C;
                    }
                    byte[] disablePlayerWeapon = BitConverter.GetBytes(1);
                    int disablePlayerWeaponWrite = 0;
                    WriteProcessMemory((int)processHandle, playerlistStartingLocation + 0xADE08, disablePlayerWeapon, disablePlayerWeapon.Length, ref disablePlayerWeaponWrite);
                    process.Dispose();
                    CloseHandle(processHandle);

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                    newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                    newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                    newEntryCmd.Parameters.AddWithValue("@action", "RearmPlayer");
                    newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                    newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                    newEntryCmd.ExecuteNonQuery();
                    newEntryCmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.RCLogs.Add(new RCLogs
                    {
                        Action = "RearmPlayer",
                        Address = _state.rcClients[sessionid].RemoteAddress,
                        Date = DateTime.Now,
                        SessionID = sessionid,
                        Username = _state.rcClients[sessionid]._username
                    });

                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }
        public RCListenerClass.StatusCodes ScanInstanceDirectory(int InstanceID, out Dictionary<int, MapList> avalMaps)
        {
            // setup out
            Dictionary<int, MapList> avalMapsDic = new Dictionary<int, MapList>();
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    List<CustomMap> customMaps = new List<CustomMap>();
                    DirectoryInfo d = new DirectoryInfo(_state.Instances[InstanceIndex].GamePath);
                    List<int> numbers = new List<int>() { 128, 64, 32, 16, 8, 4, 2, 1 };
                    List<string> badMapList = new List<string>();

                    foreach (var file in d.GetFiles("*.bms"))
                    {
                        using (FileStream fsSourceDDS = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        using (BinaryReader binaryReader = new BinaryReader(fsSourceDDS))
                        {
                            var map = new CustomMap();
                            string first_line = File.ReadLines(_state.Instances[InstanceIndex].GamePath + "\\" + file.Name, Encoding.Default).First().ToString();
                            first_line = first_line.Replace("", "|").Replace("\0\0\0", "|");
                            string[] first_line_arr = first_line.Split("|".ToCharArray());
                            var first_line_list = new List<string>();
                            foreach (string f in first_line_arr)
                            {
                                string tmp = f.Trim().Replace("\0", ((string)"").ToString());
                                if (string.IsNullOrEmpty(tmp))
                                {
                                    continue;
                                }
                                else
                                {
                                    first_line_list.Add(tmp);
                                }
                            }

                            try
                            {
                                map.MapName = first_line_list[1];
                            }
                            catch
                            {
                                badMapList.Add(file.Name);
                                continue;
                            }
                            map.FileName = file.Name;

                            fsSourceDDS.Seek(0x8A, SeekOrigin.Begin);
                            var attackdefend = binaryReader.ReadInt16();

                            if (attackdefend == 128)
                            {
                                // Set game type to be 0 (attack and defend)
                                map.gameTypeBits.Add(0);
                            }
                            else
                            {
                                fsSourceDDS.Seek(0x8B, SeekOrigin.Begin);
                                var maptype = binaryReader.ReadInt16();

                                //Console.WriteLine(file.Name);
                                // file.Name = "Backstabbers.bms"
                                _state.autoRes.sum_up(numbers, Convert.ToInt32(maptype), ref map);
                            }
                            customMaps.Add(map);
                        }
                    }

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT `default_maps`.`mission_name`, `default_maps`.`mission_file`, `gametypes`.`id` FROM `default_maps` INNER JOIN `gametypes` ON `default_maps`.`gametype` = `gametypes`.`shortname`;", db);
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<int> gametypes = new List<int>
                        {
                            reader.GetInt32(reader.GetOrdinal("id"))
                        };
                        avalMapsDic.Add(avalMapsDic.Count, new MapList
                        {
                            CustomMap = false,
                            MapFile = reader.GetString(reader.GetOrdinal("mission_file")),
                            MapName = reader.GetString(reader.GetOrdinal("mission_name")),
                            GameTypes = gametypes
                        });
                    }
                    reader.Close();
                    reader.Dispose();
                    cmd.Dispose();


                    foreach (var customMap in customMaps)
                    {
                        List<int> gametypes = new List<int>();
                        foreach (var customMapBits in customMap.gameTypeBits)
                        {
                            int gametypeId = -1;

                            // determine gametype index for each "bit" from the BMS files.
                            foreach (var gametype in _state.autoRes.gameTypes)
                            {
                                if (customMapBits == gametype.Value.Bitmap)
                                {
                                    gametypeId = gametype.Value.DatabaseId;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            // EOF

                            if (gametypeId == -1)
                            {
                                throw new Exception("FUCK. WE COULDN'T DETERMINE GAME TYPE FOR MAP: " + customMap.FileName);
                            }
                            else
                            {
                                gametypes.Add(gametypeId);
                            }
                        }
                        avalMapsDic.Add(avalMapsDic.Count, new MapList
                        {
                            CustomMap = true,
                            GameTypes = gametypes,
                            MapName = customMap.MapName,
                            MapFile = customMap.FileName
                        });
                    }
                    /*List<MapList> avalMapsList = new List<MapList>();
                    foreach (var map in avalMapsDic)
                    {
                        avalMapsList.Add(new MapList
                        {
                            CustomMap = map.Value.CustomMap,
                            GameTypes = map.Value.GameTypes,
                            MapFile = map.Value.MapFile,
                            MapName = map.Value.MapName
                        });
                    }*/
                    SQLiteCommand update_avalMapsQuery = new SQLiteCommand("UPDATE `instances_config` SET `availablemaps` = @availablemaps WHERE `profile_id` = @profileid;", db);
                    string test = JsonConvert.SerializeObject(avalMapsDic);
                    update_avalMapsQuery.Parameters.AddWithValue("@availablemaps", test);
                    update_avalMapsQuery.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    update_avalMapsQuery.ExecuteNonQuery();
                    update_avalMapsQuery.Dispose();

                    _state.Instances[InstanceIndex].availableMaps = new Dictionary<int, MapList>();
                    _state.Instances[InstanceIndex].availableMaps = avalMapsDic;

                    db.Close();
                    db.Dispose();
                    avalMaps = avalMapsDic;
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    avalMaps = null;
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                avalMaps = null;
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes DeleteRotation(int InstanceID, int RotationID)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == InstanceID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `instances_map_rotations` WHERE `rotation_id` = @id AND `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@id", RotationID);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    db.Close();
                    db.Dispose();
                    // find key from saved rotations
                    savedmaprotations selectedRotation = null;
                    foreach (var item in _state.Instances[InstanceIndex].savedmaprotations)
                    {
                        if (item.RotationID == RotationID)
                        {
                            selectedRotation = item;
                            break;
                        }
                    }
                    if (selectedRotation != null)
                    {
                        _state.Instances[InstanceIndex].savedmaprotations.Remove(selectedRotation);
                        return RCListenerClass.StatusCodes.SUCCESS;
                    }
                    else
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes CreateRotation(int serverID, string rotation, string rotationDescription)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == serverID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();

                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `instances_map_rotations` (`rotation_id`, `profile_id`, `description`, `mapcycle`) VALUES (NULL, @profileid, @rotationDescription, @rotation);", db);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    cmd.Parameters.AddWithValue("@rotationDescription", rotationDescription);
                    cmd.Parameters.AddWithValue("@rotation", rotation);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    _state.Instances[InstanceIndex].savedmaprotations.Add(new savedmaprotations
                    {
                        Description = rotationDescription,
                        mapcycle = JsonConvert.DeserializeObject<List<MapList>>(rotation),
                        RotationID = (int)db.LastInsertRowId
                    });

                    db.Close();
                    db.Dispose();
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes UpdateRotation(int serverID, string rotation, string description, int rotationId)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.Id == serverID)
                    {
                        InstanceIndex = item.Key;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (InstanceIndex != -1)
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_map_rotations` SET `description` = @description, `mapcycle` = @mapcycle WHERE `rotation_id` = @rotationid AND `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@mapcycle", rotation);
                    cmd.Parameters.AddWithValue("@rotationid", _state.Instances[InstanceIndex].savedmaprotations[rotationId].RotationID);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].Id);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.Instances[InstanceIndex].savedmaprotations[rotationId].Description = description;
                    _state.Instances[InstanceIndex].savedmaprotations[rotationId].mapcycle = JsonConvert.DeserializeObject<List<MapList>>(rotation);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
                else
                {
                    return RCListenerClass.StatusCodes.INVALIDINSTANCE;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetUsers(string sessionID, out Dictionary<string, UserCodes> subUsers)
        {
            subUsers = new Dictionary<string, UserCodes>();
            if (!_state.Users.ContainsKey(_state.rcClients[sessionID]._username))
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
            if (_state.Users[_state.rcClients[sessionID]._username].SuperAdmin == true)
            {
                foreach (var user in _state.Users)
                {
                    subUsers.Add(user.Key, user.Value);
                }
            }
            else
            {
                int primaryUserID = _state.Users[_state.rcClients[sessionID]._username].UserID;
                foreach (var user in _state.Users)
                {
                    if (user.Value.SubAdmin == primaryUserID || user.Value.UserID == primaryUserID)
                    {
                        subUsers.Add(user.Key, user.Value);
                    }
                }
            }
            return RCListenerClass.StatusCodes.SUCCESS;
        }

        public RCListenerClass.StatusCodes AddUser(string sessionID, string username, string password, bool superadmin, int subadmin, Permissions userPermissions)
        {
            try
            {
                if (_state.Users.ContainsKey(username))
                {
                    return RCListenerClass.StatusCodes.USERALREADYEXISTS;
                }

                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `users` (`id`, `username`, `password`, `permissions`, `superadmin`, `subadmin`) VALUES (NULL, @username, @password, @permissions, @superadmin, @subadmin);", db);

                /*int serverKey = -1;
                foreach (var item in userPermissions.InstancePermissions) {
                    foreach (var instance in _state.Instances)
                    {
                        if (instance.Value.Id == item.Key)
                        {
                            serverKey = instance.Key;
                            break;
                        }
                    }
                    InstancePermissions iP = item.Value;
                }*/

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", Crypt.CreateMD5(password));
                cmd.Parameters.AddWithValue("@permissions", JsonConvert.SerializeObject(userPermissions));
                cmd.Parameters.AddWithValue("@superadmin", Convert.ToInt32(superadmin));
                cmd.Parameters.AddWithValue("@subadmin", subadmin);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                _state.Users.Add(username, new UserCodes
                {
                    Password = Crypt.CreateMD5(password),
                    Permissions = userPermissions,
                    SubAdmin = subadmin,
                    SuperAdmin = superadmin,
                    UserID = (int)db.LastInsertRowId
                });

                db.Close();
                db.Dispose();
                return RCListenerClass.StatusCodes.SUCCESS;
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes DeleteUser(string sessionID, string username)
        {
            try
            {
                if (!_state.Users.ContainsKey(username))
                {
                    return RCListenerClass.StatusCodes.INVALIDPLAYERNAME;
                }
                else
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `users` WHERE `username` = @username;", db);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.Users.Remove(username);
                    return RCListenerClass.StatusCodes.SUCCESS;
                }
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetLogs(string sessionID, out List<RCLogs> logs)
        {
            logs = new List<RCLogs>();
            try
            {
                int userID = _state.Users[_state.rcClients[sessionID]._username].UserID;
                foreach (var item in _state.RCLogs)
                {
                    if (_state.rcClients[sessionID]._username == item.Username || _state.Users[_state.rcClients[sessionID]._username].SubAdmin == userID || _state.Users[_state.rcClients[sessionID]._username].SuperAdmin == true)
                    {
                        logs.Add(item);
                    }
                }
                return RCListenerClass.StatusCodes.SUCCESS;
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }

        public RCListenerClass.StatusCodes GetCurrentConnections(string sessionID, out List<RCLogs> activeConnections)
        {
            activeConnections = new List<RCLogs>();
            int userID = _state.Users[_state.rcClients[sessionID]._username].UserID;
            try
            {
                foreach (var RCClient in _state.rcClients)
                {
                    if (_state.server.IsClientConnected(RCClient.Value.RemoteAddress.ToString() + ":" + RCClient.Value.RemotePort))
                    {
                        if (_state.Users[_state.rcClients[sessionID]._username].SuperAdmin == true || _state.Users[RCClient.Value._username].SubAdmin == userID)
                        {
                            activeConnections.Add(new RCLogs
                            {
                                Address = RCClient.Value.RemoteAddress,
                                Username = RCClient.Value._username,
                                SessionID = RCClient.Value.SessionID
                            });
                        }
                    }
                }
                return RCListenerClass.StatusCodes.SUCCESS;
            }
            catch
            {
                return RCListenerClass.StatusCodes.FAILURE;
            }
        }
    }
}
