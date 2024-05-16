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
                    RCInstancesConfig.Add(item.Key, _state.Instances[item.Key]);
                }
            }
            return RCInstancesConfig;
        }
        public int GetVPNSettings(int InstanceID)
        {
            int InstanceIndex = -1;
            foreach (var item in _state.Instances)
            {
                if (item.Value.instanceID == InstanceID)
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
                if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `customwarnings` WHERE `instanceid` = @instanceid AND `message` = @message;", db);
                    cmd.Parameters.AddWithValue("@instanceid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `customwarnings` (`id`, `instanceid`, `message`) VALUES (NULL, @instanceid, @message);", db);
                    cmd.Parameters.AddWithValue("@instanceid", InstanceID);
                    cmd.Parameters.AddWithValue("@message", msg);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    _state.Instances[InstanceIndex].ServerMessagesQueue.Add(new ob_ServerMessageQueue { slot = slotNum, message = warning });
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
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    query.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                    query.Parameters.AddWithValue("@playername", playerName);
                    query.Parameters.AddWithValue("@playerip", ipaddress);
                    query.Parameters.AddWithValue("@date", DateTime.Now);
                    query.Parameters.AddWithValue("@dateadded", DateTime.Now);
                    query.Parameters.AddWithValue("@reason", banReason);
                    query.Parameters.AddWithValue("@expires", expiresDate);
                    query.Parameters.AddWithValue("@bannedby", _state.rcClients[sessionID]._username);
                    query.ExecuteNonQuery();
                    query.Dispose();

                    _state.Instances[InstanceIndex].PlayerListBans.Add(new ob_playerBanList
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    _state.Instances[InstanceIndex].PlayerListDisarm.Add(slot);
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
                    if (item.Value.instanceID == InstanceID)
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
                    Process process = Process.GetProcessById((int)_state.Instances[InstanceIndex].instanceAttachedPID.GetValueOrDefault());
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

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
                    conn.Open();
                    SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
                    automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                    automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].AutoMessages.messages));
                    automessages.ExecuteNonQuery();
                    conn.Close();
                    conn.Dispose();
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                        SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
                        conn.Open();
                        SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `enable_msg` = 1 WHERE `profile_id` = @profileid;", conn);
                        automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                        automessages.ExecuteNonQuery();
                        automessages.Dispose();
                        conn.Close();
                        conn.Dispose();
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                        SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
                        conn.Open();
                        SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `enable_msg` = 0 WHERE `profile_id` = @profileid;", conn);
                        automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                        automessages.ExecuteNonQuery();
                        automessages.Dispose();
                        conn.Close();
                        conn.Dispose();
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                        _state.Instances[InstanceIndex].PlayerListGodMod.Add(slotNum);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].MapListPrevious = new Dictionary<int, MapList>();
                    foreach (var mapEntry in _state.Instances[InstanceIndex].MapListCurrent)
                    {
                        _state.Instances[InstanceIndex].MapListPrevious.Add(_state.Instances[InstanceIndex].MapListPrevious.Count, mapEntry.Value);
                    }

                    _state.Instances[InstanceIndex].MapListCurrent = new Dictionary<int, MapList>();
                    // convert dictionary to List<MapListCurrent>
                    List<MapList> mapCycleList = new List<MapList>();
                    foreach (var item in mapCycle)
                    {
                        mapCycleList.Add(item);
                    }

                    foreach (var map in mapCycleList)
                    {
                        _state.Instances[InstanceIndex].MapListCurrent.Add(_state.Instances[InstanceIndex].MapListCurrent.Count, map);
                    }
                    serverManagerUpdateMemory.UpdateMapCycle2(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `mapcycle` = @mapcycle WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@mapcycle", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].MapListCurrent));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    Process process = Process.GetProcessById(_state.Instances[InstanceIndex].instanceAttachedPID.GetValueOrDefault());
                    IntPtr h = process.MainWindowHandle;
                    switch (MsgLocation)
                    {
                        case 1:
                            colorcode = HexConverter.ToByteArray("6A 04".Replace(" ", ""));
                            WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            break;
                        case 2:
                            colorcode = HexConverter.ToByteArray("6A 05".Replace(" ", ""));
                            WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            break;
                        default:
                            colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                            WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            break;
                    }
                    // post message
                    PostMessage(h, (ushort)WM_KEYDOWN, (ushort)GlobalChat, 0);
                    PostMessage(h, (ushort)WM_KEYUP, (ushort)GlobalChat, 0);
                    Thread.Sleep(50);
                    int bytesWritten = 0;
                    byte[] buffer;
                    buffer = Encoding.Default.GetBytes($"{Msg}\0"); // '\0' marks the end of string
                    WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                    Thread.Sleep(50);
                    PostMessage(h, (ushort)WM_KEYDOWN, (ushort)VK_ENTER, 0);
                    PostMessage(h, (ushort)WM_KEYUP, (ushort)VK_ENTER, 0);

                    // change color to normal
                    Thread.Sleep(50);
                    int revert_colorbuffer = 0;
                    byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                    WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                    process.Dispose();

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameRespawnTime = respawnTime;
                    serverManagerUpdateMemory.UpdateRespawnTime(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `respawn_time` = @respawntime WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@respawntime", _state.Instances[InstanceIndex].gameRespawnTime);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameFlagReturnTime = flagReturnTime;
                    serverManagerUpdateMemory.UpdateFlagReturnTime(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `flagreturntime` = @flagreturntime WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@flagreturntime", _state.Instances[InstanceIndex].gameFlagReturnTime);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameFriendlyFireKills = friendlyFireKills;
                    serverManagerUpdateMemory.UpdateFriendlyFireKills(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `friendly_fire_kills` = @friendly_fire_kills WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@friendly_fire_kills", _state.Instances[InstanceIndex].gameFriendlyFireKills);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
            conn.Open();

            // Get Instance Index
            int InstanceIndex = -1;
            foreach (var item in _state.Instances)
            {
                if (item.Value.instanceID == InstanceID)
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

            // infoCurrentMapName List
            _state.Instances[InstanceIndex].MapListCurrent = new Dictionary<int, MapList>();
            foreach (var map in mapList)
            {
                _state.Instances[InstanceIndex].MapListCurrent.Add(_state.Instances[InstanceIndex].MapListCurrent.Count, map);
            }

            // Starting Server
            Instance startInstance = new Instance();
            startInstance = _state.Instances[InstanceIndex];

            // Collect Form Data
            startInstance.gameServerName = server_name;                             // Server Name
            startInstance.gameMOTD = motd;                                          // Server gameMOTD
            startInstance.gameCountryCode = country_code;                           // Country Code
            startInstance.gamePasswordLobby = server_password;                           // Server Global gamePasswordLobby
            startInstance.gameSessionType = session_type;                           // Session Type (0 = Internet, 1 = LAN) - Doesn't work and Form Changed.
            startInstance.gameMaxSlots = max_slots + 1;                             // Max Players
            startInstance.gameStartDelay = start_delay;                             // Start Delay
            startInstance.gameLoopMaps = startLoopMaps ? 1 : 0;                     // Loop Maps
            startInstance.gameScoreFlags = FBScore;                                    // Flag Score
            startInstance.gameScoreKills = game_score;                               // Game Score
            startInstance.gameScoreZoneTime = zone_timer;                               // Zone Timer
            startInstance.gameRespawnTime = respawn_time;                           // Respawn Time
            startInstance.gameTimeLimit = time_limit;                               // Time Limit
            startInstance.gameRequireNova = require_novalogic;                 // Require NovaLogic Login
            startInstance.gameDedicated = dedicated;                                // Run gameDedicated
            startInstance.gameWindowedMode = run_windowed;                          // Windowed Mode
            startInstance.gameCustomSkins = allow_custom_skins;                // Allow Custom Skins
            startInstance.profileGameMod = game_mod;                                       // Game profileGameMod
            startInstance.gamePasswordBlue = blue_team_password;                    // Blue Team gamePasswordLobby
            startInstance.gamePasswordRed = red_team_password;                      // Red Team gamePasswordLobby
            startInstance.gameOptionFF = friendly_fire;                         // Friendly Fire Flag
            startInstance.gameOptionFFWarn = friendly_fire_warning;          // Friendly Fire Warning
            startInstance.gameOptionFriendlyTags = friendly_tags;                         // Friendly Tags
            startInstance.gameOptionAutoBalance = auto_balance;                           // Auto Balance
            startInstance.gameOptionShowTracers = show_tracers;                           // Show Tracers
            startInstance.gameShowTeamClays = show_team_clays;                      // Show Team Clays
            startInstance.gameOptionAutoRange = allow_auto_range;                    // Allow Auto Range
            startInstance.gameMinPing = enable_min_ping;                            // Enable Min Ping
            startInstance.gameMinPingValue = min_ping;                              // Min Ping Value
            startInstance.gameMaxPing = enable_max_ping;                            // Enable Max Ping
            startInstance.gameMaxPingValue = max_ping;                              // Max Ping Value            
            startInstance.instanceLastUpdateTime = DateTime.Now;
            startInstance.instanceNextUpdateTime = DateTime.Now.AddSeconds(5.0);

            // Generate AutoRes.bin
            if (!(serverManagerUpdateMemory.createAutoRes(startInstance, _state)))
            {
                MessageBox.Show("Failed to create AutoRes.bin", "Error");
                return RCListenerClass.StatusCodes.FAILURE;
            }

            // Start Game
            // Check for instanceAttachedPID in Database, if not found create a record.
            SQLiteCommand checkPidQuery = new SQLiteCommand("SELECT COUNT(*) FROM `instances_pid` WHERE `profile_id` = @instanceId;", conn);
            checkPidQuery.Parameters.AddWithValue("@instanceId", _state.Instances[InstanceIndex].instanceID);
            int checkPid = Convert.ToInt32(checkPidQuery.ExecuteScalar());
            checkPidQuery.Dispose();

            if (checkPid == 0)
            {
                SQLiteCommand insert_cmd = new SQLiteCommand("INSERT INTO `instances_pid` (`profile_id`, `pid`) VALUES (@instanceid, 0)", conn);
                insert_cmd.Parameters.AddWithValue("@instanceid", _state.Instances[InstanceIndex].instanceID);
                insert_cmd.ExecuteNonQuery();
            }

            serverManagerUpdateMemory.startGame(startInstance, _state, InstanceIndex, conn);

            // Sucess Update Database & Return Instance Array
            SQLiteCommand update_query = new SQLiteCommand("UPDATE `instances_config` SET `server_name` = @servername, `motd` = @motd, `country_code` = @countrycode, `server_password` = @server_password,`session_type` = @sessiontype, `max_slots` = @max_slots, `start_delay` = @start_delay, `loop_maps` = @loop_maps, `game_score` = @game_score, `fbscore` = @flag_scores, `zone_timer` = @zone_timer, `respawn_time` = @respawn_time, `time_limit` = @time_limit, `require_novalogic` = @require_novalogic, `windowed_mode` = @run_windowed, `allow_custom_skins` = @allow_custom_skins, `run_dedicated` = @run_dedicated, `game_mod` = @game_mod, `mapcycle` = @selected_maps, `blue_team_password` = @blue_team_password, `red_team_password` = @red_team_password, `friendly_fire` = @friendly_fire, `friendly_fire_warning` = @friendly_fire_warning, `friendly_tags` = @friendly_tags, `auto_balance` = @auto_balance, `show_tracers` = @show_tracers, `show_team_clays` = @show_team_clays, `allow_auto_range` = @allow_auto_range, `enable_min_ping` = @enable_min_ping, `min_ping` = @min_ping, `enable_max_ping` = @enable_max_ping, `max_ping` = @max_ping, `availablemaps` = @availablemaps WHERE `profile_id` = @profile_id", conn);

            update_query.Parameters.AddWithValue("@servername", startInstance.gameServerName);
            update_query.Parameters.AddWithValue("@motd", startInstance.gameMOTD);
            update_query.Parameters.AddWithValue("@countrycode", startInstance.gameCountryCode);
            update_query.Parameters.AddWithValue("@sessiontype", startInstance.gameSessionType);
            update_query.Parameters.AddWithValue("@server_password", startInstance.gamePasswordLobby);
            update_query.Parameters.AddWithValue("@max_slots", startInstance.gameMaxSlots);
            update_query.Parameters.AddWithValue("@start_delay", startInstance.gameStartDelay);
            update_query.Parameters.AddWithValue("@loop_maps", startInstance.gameLoopMaps);
            update_query.Parameters.AddWithValue("@game_score", startInstance.gameScoreKills);
            update_query.Parameters.AddWithValue("@flag_scores", startInstance.gameScoreKills);
            update_query.Parameters.AddWithValue("@zone_timer", startInstance.gameScoreZoneTime);
            update_query.Parameters.AddWithValue("@respawn_time", startInstance.gameRespawnTime);
            update_query.Parameters.AddWithValue("@time_limit", startInstance.gameTimeLimit);
            update_query.Parameters.AddWithValue("@require_novalogic", startInstance.gameRequireNova ? 1 : 0);
            update_query.Parameters.AddWithValue("@run_windowed", startInstance.gameWindowedMode ? 1 : 0);
            update_query.Parameters.AddWithValue("@allow_custom_skins", startInstance.gameCustomSkins ? 1 : 0);
            update_query.Parameters.AddWithValue("@run_dedicated", startInstance.gameDedicated ? 1 : 0);
            update_query.Parameters.AddWithValue("@game_mod", startInstance.profileGameMod);
            update_query.Parameters.AddWithValue("@blue_team_password", startInstance.gamePasswordBlue);
            update_query.Parameters.AddWithValue("@red_team_password", startInstance.gamePasswordRed);
            update_query.Parameters.AddWithValue("@friendly_fire", startInstance.gameOptionFF);
            update_query.Parameters.AddWithValue("@friendly_fire_warning", startInstance.gameOptionFFWarn ? 1 : 0);
            update_query.Parameters.AddWithValue("@friendly_tags", startInstance.gameOptionFriendlyTags ? 1 : 0);
            update_query.Parameters.AddWithValue("@auto_balance", startInstance.gameOptionAutoBalance ? 1 : 0);
            update_query.Parameters.AddWithValue("@show_tracers", startInstance.gameOptionShowTracers ? 1 : 0);
            update_query.Parameters.AddWithValue("@show_team_clays", startInstance.gameShowTeamClays ? 1 : 0);
            update_query.Parameters.AddWithValue("@allow_auto_range", startInstance.gameOptionAutoRange ? 1 : 0);
            update_query.Parameters.AddWithValue("@enable_min_ping", startInstance.gameMinPing ? 1 : 0);
            update_query.Parameters.AddWithValue("@min_ping", startInstance.gameMinPingValue);
            update_query.Parameters.AddWithValue("@enable_max_ping", startInstance.gameMaxPing ? 1 : 0);
            update_query.Parameters.AddWithValue("@max_ping", startInstance.gameMaxPingValue);
            update_query.Parameters.AddWithValue("@profile_id", _state.Instances[InstanceIndex].instanceID);

            update_query.Parameters.AddWithValue("@availablemaps", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].MapListAvailable));
            update_query.Parameters.AddWithValue("@selected_maps", JsonConvert.SerializeObject(_state.Instances[InstanceIndex].MapListCurrent));
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
                    if (item.Value.instanceID == InstanceID)
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
                _state.Instances[InstanceID].Firewall.DeleteFirewallRules(_state.Instances[InstanceID].profileName, "Allow");
                _state.Instances[InstanceID].Firewall.DeleteFirewallRules(_state.Instances[InstanceID].profileName, "Deny");

                SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameScoreBoardDelay = scoreboardDelay;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `scoreboard_override` = @scoreboard_override WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@scoreboard_override", _state.Instances[InstanceIndex].gameScoreBoardDelay);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameMaxTeamLives = maxTeamLives;
                    serverManagerUpdateMemory.UpdateMaxTeamLives(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `max_team_lives` = @gameMaxTeamLives WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@gameMaxTeamLives", _state.Instances[InstanceIndex].gameMaxTeamLives);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gamePSPTOTimer = pSPTime;
                    serverManagerUpdateMemory.UpdatePSPTakeOverTime(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `psptakeover` = @gamePSPTOTimer WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@gamePSPTOTimer", _state.Instances[InstanceIndex].gamePSPTOTimer);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameCustomSkins = allowCustomSkins;
                    serverManagerUpdateMemory.UpdateAllowCustomSkins(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `allow_custom_skins` = @allow_custom_skins WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@allow_custom_skins", _state.Instances[InstanceIndex].gameCustomSkins);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    int oldPw = _state.Instances[InstanceIndex].gamePasswordBlue.Length;
                    _state.Instances[InstanceIndex].gamePasswordBlue = bluePassword;
                    serverManagerUpdateMemory.UpdateBluePassword(_state, InstanceIndex, oldPw);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `blue_team_password` = @blue_team_password WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@blue_team_password", _state.Instances[InstanceIndex].gamePasswordBlue);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    if (item.Value.instanceID == InstanceID)
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
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameScoreFlags = fBScore;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `fbscore` = @fbscore WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@fbscore", _state.Instances[InstanceIndex].gameScoreFlags);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameScoreKills = gameScore;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `game_score` = @game_score WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@game_score", _state.Instances[InstanceIndex].gameScoreKills);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameScoreZoneTime = kOTHScore;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `zone_timer` = @zoneTimer WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@zoneTimer", _state.Instances[InstanceIndex].gameScoreZoneTime);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameMaxPing = enablePing;
                    _state.Instances[InstanceIndex].gameMaxPingValue = pingValue;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `enable_max_ping` = @enableMaxPing, `max_ping` = @maxPingValue WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@enableMaxPing", Convert.ToInt32(_state.Instances[InstanceIndex].gameMaxPing));
                    updateDB.Parameters.AddWithValue("@maxPingValue", (int)_state.Instances[InstanceIndex].gameMaxPingValue);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameMinPing = enablePing;
                    _state.Instances[InstanceIndex].gameMinPingValue = pingValue;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `enable_min_ping` = @enableMinPing, `min_ping` = @minPingValue WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@enableMinPing", Convert.ToInt32(_state.Instances[InstanceIndex].gameMinPing));
                    updateDB.Parameters.AddWithValue("@minPingValue", _state.Instances[InstanceIndex].gameMinPingValue);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameOptionFF = friendlyFire;
                    _state.Instances[InstanceIndex].gameOptionFriendlyTags = friendlyTags;
                    _state.Instances[InstanceIndex].gameShowTeamClays = showTeamClays;
                    _state.Instances[InstanceIndex].gameOptionAutoBalance = autoBalance;
                    _state.Instances[InstanceIndex].gameOptionFFWarn = friendlyFireWarning;
                    _state.Instances[InstanceIndex].gameOptionShowTracers = showTracers;
                    _state.Instances[InstanceIndex].gameOptionAutoRange = allowAutoRange;
                    serverManagerUpdateMemory.GamePlayOptions(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();


                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `friendly_fire` = @friendlyfire, `friendly_tags` = @friendlytags, `show_team_clays` = @showTeamClays, `auto_balance` = @autoBalance, `friendly_fire_warning` = @friendlyFireWarning, `show_tracers` = @showTracers, `allow_auto_range` = @allowAutoRange WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@friendlyfire", Convert.ToInt32(_state.Instances[InstanceIndex].gameOptionFF));
                    updateDB.Parameters.AddWithValue("@friendlytags", Convert.ToInt32(_state.Instances[InstanceIndex].gameOptionFriendlyTags));
                    updateDB.Parameters.AddWithValue("@showTeamClays", Convert.ToInt32(_state.Instances[InstanceIndex].gameShowTeamClays));
                    updateDB.Parameters.AddWithValue("@autoBalance", Convert.ToInt32(_state.Instances[InstanceIndex].gameOptionAutoBalance));
                    updateDB.Parameters.AddWithValue("@friendlyFireWarning", Convert.ToInt32(_state.Instances[InstanceIndex].gameOptionFFWarn));
                    updateDB.Parameters.AddWithValue("@showTracers", Convert.ToInt32(_state.Instances[InstanceIndex].gameOptionShowTracers));
                    updateDB.Parameters.AddWithValue("@allowAutoRange", Convert.ToInt32(_state.Instances[InstanceIndex].gameOptionAutoRange));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    int oldPw = _state.Instances[InstanceIndex].gamePasswordRed.Length;
                    _state.Instances[InstanceIndex].gamePasswordRed = redPassword;
                    serverManagerUpdateMemory.UpdateRedPassword(_state, InstanceIndex, oldPw);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `red_team_password` = @redPassword WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@redPassword", _state.Instances[InstanceIndex].gamePasswordRed);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameMOTD = MOTD;
                    serverManagerUpdateMemory.UpdateMOTD(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `motd` = @motd WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@motd", _state.Instances[InstanceIndex].gameMOTD);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameRequireNova = requireNovaLogin;
                    serverManagerUpdateMemory.UpdateRequireNovaLogin(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `require_nova` = @requireNova WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@requireNova", Convert.ToInt32(_state.Instances[InstanceIndex].gameRequireNova));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameLoopMaps = loopMaps;
                    serverManagerUpdateMemory.UpdateLoopMaps(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `loop_maps` = @loopMaps WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@loopMaps", _state.Instances[InstanceIndex].gameLoopMaps);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameStartDelay = startDelay;
                    serverManagerUpdateMemory.UpdateStartDelay(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `start_delay` = @startDelay WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@startDelay", _state.Instances[InstanceIndex].gameStartDelay);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameTimeLimit = timeLimit;
                    serverManagerUpdateMemory.UpdateTimeLimit(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `time_limit` = @timeLimit WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@timeLimit", _state.Instances[InstanceIndex].gameTimeLimit);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameSessionType = sessionType;
                    serverManagerUpdateMemory.UpdateSessionType(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `session_type` = @sessionType WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@sessionType", _state.Instances[InstanceIndex].gameSessionType);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gamePasswordLobby = password;
                    serverManagerUpdateMemory.UpdateServerPassword(_state, InstanceIndex, password.Length);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `server_password` = @serverPassword WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@serverPassword", _state.Instances[InstanceIndex].gamePasswordLobby);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameCountryCode = countryCode;
                    serverManagerUpdateMemory.UpdateCountryCode(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `country_code` = @countryCode WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@countryCode", _state.Instances[InstanceIndex].gameCountryCode);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].gameServerName = serverName;
                    serverManagerUpdateMemory.UpdateServerName(_state, InstanceIndex);
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `server_name` = @serverName WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@serverName", _state.Instances[InstanceIndex].gameServerName);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    if (_state.Instances[InstanceIndex].PlayerListGodMod.Contains(slotNum))
                    {
                        _state.Instances[InstanceIndex].PlayerListGodMod.Remove(slotNum);
                        int buffer = 0;
                        byte[] PointerAddr9 = new byte[4];
                        var baseAddr = 0x400000;
                        var Pointer = baseAddr + 0x005ED600;

                        // read the playerlist memory address from the game...
                        ReadProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                        var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                        byte[] playerListStartingLocationByteArray = new byte[4];
                        int playerListStartingLocationBuffer = 0;
                        ReadProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                        int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                        for (int i = 1; i < slotNum; i++)
                        {
                            playerlistStartingLocation += 0xAF33C;
                        }
                        byte[] playerObjectLocationBytes = new byte[4];
                        int playerObjectLocationRead = 0;
                        ReadProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                        int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                        byte[] setPlayerHealth = BitConverter.GetBytes(100); //set god mode health
                        int setPlayerHealthWrite = 0;

                        byte[] setDamageBy = BitConverter.GetBytes(0);
                        int setDamageByWrite = 0;

                        WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, playerObjectLocation + 0x138, setDamageBy, setDamageBy.Length, ref setDamageByWrite);
                        WriteProcessMemory((int)_state.Instances[InstanceIndex].instanceProcessHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);

                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                        _state.Instances[InstanceIndex].IPWhiteList.Add(_state.Instances[InstanceIndex].IPWhiteList.Count, new ob_ipWhitelist
                        {
                            Description = description,
                            IPAddress = PlayerIP.ToString()
                        });
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                        db.Open();
                        SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `vpnwhitelist` (`profile_id`, `description`, `address`) VALUES (@profileid, @description, @PublicIP);", db);
                        cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `vpnCheckEnabled` = @vpnCheckEnabled WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@vpnCheckEnabled", Convert.ToInt32(_state.Instances[InstanceIndex].vpnCheckEnabled));
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    _state.Instances[InstanceIndex].vpnCheckEnabled = DisallowVPN;
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand updateDB = new SQLiteCommand("UPDATE `instances_config` SET `warnlevel` = @warnLevel WHERE `profile_id` = @profileid;", db);
                    updateDB.Parameters.AddWithValue("@warnLevel", _state.IPQualityCache[InstanceIndex].WarnLevel);
                    updateDB.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    for (int i = 0; i < _state.Instances[InstanceIndex].PlayerListBans.Count; i++)
                    {
                        if (_state.Instances[InstanceIndex].PlayerListBans[i].ipaddress == playerIP && _state.Instances[InstanceIndex].PlayerListBans[i].player == playerName)
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
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                        db.Open();
                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `playerbans` WHERE `id` = @banid AND `profileid` = @profileid;", db);
                        cmd.Parameters.AddWithValue("@banid", _state.Instances[InstanceIndex].PlayerListBans[playerIndex].id);
                        cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                        _state.Instances[InstanceIndex].PlayerListBans.RemoveAt(playerIndex);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_msg_interval` = @value WHERE `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@value", interval);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
                    conn.Open();
                    SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
                    automessages.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].infoCounterMaps = _state.Instances[InstanceIndex].MapListCurrent.Count;
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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

        public RCListenerClass.StatusCodes SkipMap(int InstanceID, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.instanceID == InstanceID)
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
                    if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.LOADINGMAP)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.OFFLINE)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.SCORING)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.STARTDELAY || _state.Instances[InstanceIndex].instanceStatus == InstanceStatus.ONLINE)
                    {
                        Process process = Process.GetProcessById(_state.Instances[InstanceIndex].instanceAttachedPID.GetValueOrDefault());
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

                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                        db.Open();

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (NULL, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
                        newEntryCmd.Parameters.AddWithValue("@action", "SkipMap");
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

        public RCListenerClass.StatusCodes ScoreMap(int InstanceID, string sessionid)
        {
            try
            {
                int InstanceIndex = -1;
                foreach (var item in _state.Instances)
                {
                    if (item.Value.instanceID == InstanceID)
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
                    if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.LOADINGMAP)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.OFFLINE)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.SCORING)
                    {
                        return RCListenerClass.StatusCodes.FAILURE;
                    }
                    else if (_state.Instances[InstanceIndex].instanceStatus == InstanceStatus.STARTDELAY || _state.Instances[InstanceIndex].instanceStatus == InstanceStatus.ONLINE)
                    {
                        (new ServerManagement()).ScoreMap(ref _state, InstanceIndex);

                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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

                    _state.Instances[InstanceIndex].TeamListChange.Add(new ob_playerChangeTeamList
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
                    if (item.Value.instanceID == InstanceID)
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
                    _state.Instances[InstanceIndex].PlayerListDisarm.Remove(slotNum);
                    Process process = Process.GetProcessById((int)_state.Instances[InstanceIndex].instanceAttachedPID.GetValueOrDefault());
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

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    if (item.Value.instanceID == InstanceID)
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
                    DirectoryInfo d = new DirectoryInfo(_state.Instances[InstanceIndex].profileServerPath);
                    List<int> numbers = new List<int>() { 128, 64, 32, 16, 8, 4, 2, 1 };
                    List<string> badMapList = new List<string>();

                    foreach (var file in d.GetFiles("*.bms"))
                    {
                        using (FileStream fsSourceDDS = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        using (BinaryReader binaryReader = new BinaryReader(fsSourceDDS))
                        {
                            var map = new CustomMap();
                            string first_line = File.ReadLines(_state.Instances[InstanceIndex].profileServerPath + "\\" + file.Name, Encoding.Default).First().ToString();
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

                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    /*List<MapListCurrent> avalMapsList = new List<MapListCurrent>();
                    foreach (var map in avalMapsDic)
                    {
                        avalMapsList.Add(new MapListCurrent
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
                    update_avalMapsQuery.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                    update_avalMapsQuery.ExecuteNonQuery();
                    update_avalMapsQuery.Dispose();

                    _state.Instances[InstanceIndex].MapListAvailable = new Dictionary<int, MapList>();
                    _state.Instances[InstanceIndex].MapListAvailable = avalMapsDic;

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
                    if (item.Value.instanceID == InstanceID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `instances_map_rotations` WHERE `rotation_id` = @id AND `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@id", RotationID);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    db.Close();
                    db.Dispose();
                    // find key from saved rotations
                    savedmaprotations selectedRotation = null;
                    foreach (var item in _state.Instances[InstanceIndex].MapListRotationDB)
                    {
                        if (item.RotationID == RotationID)
                        {
                            selectedRotation = item;
                            break;
                        }
                    }
                    if (selectedRotation != null)
                    {
                        _state.Instances[InstanceIndex].MapListRotationDB.Remove(selectedRotation);
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
                    if (item.Value.instanceID == serverID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();

                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `instances_map_rotations` (`rotation_id`, `profile_id`, `description`, `mapcycle`) VALUES (NULL, @profileid, @rotationDescription, @rotation);", db);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                    cmd.Parameters.AddWithValue("@rotationDescription", rotationDescription);
                    cmd.Parameters.AddWithValue("@rotation", rotation);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    _state.Instances[InstanceIndex].MapListRotationDB.Add(new savedmaprotations
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
                    if (item.Value.instanceID == serverID)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                    db.Open();
                    SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_map_rotations` SET `description` = @description, `mapcycle` = @mapcycle WHERE `rotation_id` = @rotationid AND `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@mapcycle", rotation);
                    cmd.Parameters.AddWithValue("@rotationid", _state.Instances[InstanceIndex].MapListRotationDB[rotationId].RotationID);
                    cmd.Parameters.AddWithValue("@profileid", _state.Instances[InstanceIndex].instanceID);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    db.Close();
                    db.Dispose();
                    _state.Instances[InstanceIndex].MapListRotationDB[rotationId].Description = description;
                    _state.Instances[InstanceIndex].MapListRotationDB[rotationId].mapcycle = JsonConvert.DeserializeObject<List<MapList>>(rotation);
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

                SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `users` (`id`, `username`, `password`, `permissions`, `superadmin`, `subadmin`) VALUES (NULL, @username, @password, @permissions, @superadmin, @subadmin);", db);

                /*int serverKey = -1;
                foreach (var item in userPermissions.InstancePermissions) {
                    foreach (var instance in _state.Instances)
                    {
                        if (instance.Value.instanceID == item.Key)
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
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
                    #pragma warning disable CS0618
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
                    #pragma warning restore CS0618
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
