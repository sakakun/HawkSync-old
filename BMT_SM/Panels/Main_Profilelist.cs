using HawkSync_SM.classes;
using HawkSync_SM.classes.logs;
using HawkSync_SM.classes.Plugins.pl_VoteMaps;
using HawkSync_SM.classes.Plugins.pl_WelcomePlayer;
using HawkSync_SM.RCClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WatsonTcp;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_SM
{
    public partial class Main_Profilelist : Form
    {
        // Import of Dynamic Link Libraries
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        // Memory HEX CONSTANTS
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const int VK_ENTER = 0x0D;
        const int console = 0xC0;
        const int GlobalChat = 0x54;

        // Object: GameTypes
        Dictionary<int, GameType> bitmapsAndGameTypes = new Dictionary<int, GameType>();
        // Object: DataTable
        public DataTable table = new DataTable();
        // Object: MapList
        public Dictionary<int, Dictionary<int, MapList>> maPList = new Dictionary<int, Dictionary<int, MapList>>();
        // Object: Application Settings Database
        public SQLiteConnection gametype_db = new SQLiteConnection(ProgramConfig.DBConfig);

        // Object: Application State (Server Manager)       
        public AppState _state;

        // Object: Ticker (Used to Refresh Information x Seconds
        private Timer Ticker = new Timer()
        {
            Enabled = false,
            Interval = 100
        };

        // Logging
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Client Number (Unsure Purpose)
        int clineNo;

        // Disabled or Unused objects.
        // - public Thread QueueHandler { get; set; }

        // Server Manager - Game Profile List
        public Main_Profilelist(AppState state)
        {
            _state = state;
            InitializeComponent();

            // add autofill on launch for reattachments
            Ticker.Tick += (sender, e) =>
            {
                CheckHandlers();
                UpdateTick();
                checkPlayerHistory();
                plugin_WelcomePlayers();
                //update_master();
                update_novahq();
                update_novacc();
                cleanRCClients();
                CheckForExpiredBans();
                //CheckForUpdates();
                //CheckForExpiredRCClients();
            };
            gametype_db.Open();
            SetupAppState(gametype_db);
            GetGameTypes(gametype_db);
            beginRCListen();
            //SetupWebServer(gametype_db);
            gametype_db.Close();
        }

        private void CheckHandlers()
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.Handle == IntPtr.Zero)
                {
                    if (ProcessExist(instance.Value.PID.GetValueOrDefault()))
                    {
                        instance.Value.Handle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, instance.Value.PID.GetValueOrDefault());
                        if (instance.Value.Handle == IntPtr.Zero)
                        {
                            log.Error("Failed to attach to process " + instance.Value.PID.GetValueOrDefault());
                        }
                        else
                        {
                            log.Info("Attached to process " + instance.Value.PID.GetValueOrDefault());
                        }
                        if (!_state.ApplicationProcesses.ContainsKey(instance.Key))
                        {
                            _state.ApplicationProcesses.Add(instance.Key, Process.GetProcessById(instance.Value.PID.GetValueOrDefault()));
                        }
                    }
                }
            }
        }

        private void plugin_WelcomePlayers()
        {
            foreach (var item in _state.Instances)
            {
                if (!item.Value.Plugins.WelcomeMessage)
                {
                    return;
                }
                else
                {
                    if (item.Value.Status == InstanceStatus.OFFLINE || item.Value.Status == InstanceStatus.LOADINGMAP || item.Value.Status == InstanceStatus.SCORING)
                    {
                        return;
                    }
                    foreach (var wm in item.Value.WelcomeQueue)
                    {
                        if (DateTime.Compare(item.Value.WelcomeTimer, DateTime.Now) > 0)
                        {
                            return; // since the timer hasn't passed.
                        }
                        if (wm.Processed == false && item.Value.PlayerList.ContainsKey(wm.Slot)) // && DateTime.Compare(wm.RunTime, DateTime.Now) < 0
                        {
                            if (item.Value.PlayerList[wm.Slot].ping == 0)
                            {
                                continue;
                            }

                            int colorbuffer_written = 0;
                            byte[] colorcode = HexConverter.ToByteArray("6A 08".Replace(" ", ""));
                            //WriteProcessMemory((int)_state.Instances[item.Key].Handle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            MemoryProcessor.Write(_state.Instances[item.Key], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                            Thread.Sleep(100);
                            // open console
                            PostMessage(_state.ApplicationProcesses[item.Key].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                            PostMessage(_state.ApplicationProcesses[item.Key].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                            Thread.Sleep(100);
                            int bytesWritten = 0;
                            byte[] buffer;
                            if (wm.ReturningPlayer == true)
                            {
                                buffer = Encoding.Default.GetBytes($"{_state.Instances[item.Key].Plugins.WelcomeMessageSettings.ReturningPlayerMsg.Replace("$(PlayerName)", wm.playerName)}\0"); // '\0' marks the end of string
                            }
                            else
                            {
                                buffer = Encoding.Default.GetBytes($"{_state.Instances[item.Key].Plugins.WelcomeMessageSettings.NewPlayerMsg.Replace("$(PlayerName)", wm.playerName)}\0"); // '\0' marks the end of string
                            }
                            //WriteProcessMemory((int)_state.Instances[item.Key].Handle, 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                            MemoryProcessor.Write(_state.Instances[item.Key], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                            Thread.Sleep(100);
                            PostMessage(_state.ApplicationProcesses[item.Key].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                            PostMessage(_state.ApplicationProcesses[item.Key].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                            Thread.Sleep(100);
                            int revert_colorbuffer = 0;
                            byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                            //WriteProcessMemory((int)_state.Instances[item.Key].Handle, 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                            MemoryProcessor.Write(_state.Instances[item.Key], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);

                            // insert memory write here
                            /*_state.Instances[item.Key].WelcomeQueue.Remove(wm);*/
                            wm.Processed = true;
                            _state.Instances[item.Key].WelcomeTimer = DateTime.Now.AddSeconds(5); // prevent console spam
                        }
                    }
                }
                for (int i = 0; i < item.Value.WelcomeQueue.Count; i++)
                {
                    bool plFound = false;
                    foreach (var onlinePlayer in item.Value.PlayerList)
                    {
                        if (item.Value.WelcomeQueue[i].playerName == onlinePlayer.Value.name)
                        {
                            plFound = true;
                            break;
                        }
                    }
                    if (plFound != true)
                    {
                        _state.Instances[item.Key].WelcomeQueue.RemoveAt(i);
                    }
                }
            }
        }

        private void update_novacc()
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.ReportNovaCC == true && DateTime.Compare(_state.Instances[instance.Key].NextUpdateNovaCC, DateTime.Now) < 0 && instance.Value.Status != InstanceStatus.OFFLINE)
                {
                    List<NovaHQPlayerListClass> playerlistHQ = new List<NovaHQPlayerListClass>();
                    WebClient client = new WebClient
                    {
                        BaseAddress = "http://ext.novaworld.cc/"
                    };
                    client.Headers["User-Agent"] = "Babstats.net BMTv4";
                    //client.Headers["User-Agent"] = "NovaHQ Heartbeat DLL (1.0.9)";
                    client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    NameValueCollection vars = new NameValueCollection
                    {
                        { "Encoding", "windows-1252" },
                        { "PKey", "eYkJaPPR-3WNbgPN93,(ZwxBCnEW" },
                        { "PVer", "1.0.9" },
                        { "SKey", "SECRET_KEY" },
                        { "DataType", "0x100" },
                        { "GameID", "dfbhd" },
                        { "Name", _state.Instances[instance.Key].ServerName },
                        { "Port", _state.Instances[instance.Key].GamePort.ToString() },
                        { "CK", "0" },
                        { "Country", _state.Instances[instance.Key].CountryCode },
                        { "Type", "Dedicated" },
                        { "GameType", _state.Instances[instance.Key].GameTypeName },
                        { "CurrentPlayers", _state.Instances[instance.Key].PlayerList.Count.ToString() },
                        { "MaxPlayers", _state.Instances[instance.Key].MaxSlots.ToString() },
                        { "MissionName", _state.Instances[instance.Key].CurrentMap.MapName },
                        { "MissionFile", _state.Instances[instance.Key].CurrentMap.MapFile },
                        { "TimeRemaining", (_state.Instances[instance.Key].StartDelay + _state.Instances[instance.Key].TimeRemaining * 60).ToString() }
                    };
                    if (_state.Instances[instance.Key].Password != string.Empty)
                    {
                        vars.Add("Password", "Y");
                    }
                    else
                    {
                        vars.Add("Password", "");
                    }
                    vars.Add("Message", _state.Instances[instance.Key].MOTD);
                    if (_state.Instances[instance.Key].IsTeamSabre == true)
                    {
                        vars.Add("Mod", "TS:");
                    }
                    else
                    {
                        vars.Add("Mod", "");
                    }
                    if (_state.Instances[instance.Key].PlayerList.Count > 0)
                    {
                        foreach (var player in _state.Instances[instance.Key].PlayerList)
                        {
                            playerlistHQ.Add(new NovaHQPlayerListClass
                            {
                                Deaths = player.Value.deaths,
                                Kills = player.Value.kills,
                                NameBase64Encoded = player.Value.nameBase64,
                                PlayerName = player.Value.name,
                                TeamId = player.Value.team,
                                TeamText = player.Value.team.ToString(),
                                WeaponId = Convert.ToInt32(Enum.Parse(typeof(playerlist.WeaponStack), player.Value.selectedWeapon)),
                                WeaponText = player.Value.selectedWeapon
                            });
                        }
                        vars.Add("PlayerList", Crypt.Base64Encode(JsonConvert.SerializeObject(playerlistHQ)));
                    }
                    else
                    {
                        vars.Add("PlayerList", "eyIwIjogeyJOYW1lIjoiSG9zdCIsIk5hbWVCYXNlNjRFbmNvZGVkIjoiU0c5emRBPT0iLCJLaWxscyI6IjAiLCJEZWF0aHMiOiIwIiwiV2VhcG9uSWQiOiI1IiwiV2VhcG9uVGV4dCI6IkNBUi0xNSIsIlRlYW1JZCI6IjUiLCJUZWFtVGV4dCI6Ik5vbmUiIH19");
                    }
                    try
                    {
                        byte[] response = client.UploadValues("nwapi.php", vars);
                        string getResponse = Encoding.Default.GetString(response);
                    }
                    catch
                    {

                    }
                    _state.Instances[instance.Key].NextUpdateNovaCC = DateTime.Now.AddMinutes(1.0);
                }
                else
                {
                    continue;
                }
            }
        }

        private void update_novahq()
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.ReportNovaHQ == true && DateTime.Compare(_state.Instances[instance.Key].NextUpdateNovaHQ, DateTime.Now) < 0 && instance.Value.Status != InstanceStatus.OFFLINE)
                {
                    List<NovaHQPlayerListClass> playerlistHQ = new List<NovaHQPlayerListClass>();
                    WebClient client = new WebClient
                    {
                        BaseAddress = "http://nw.novahq.net/"
                    };
                    /*client.Headers["User-Agent"] = "Babstats v4 " + ProgramConfig.version;*/
                    client.Headers["User-Agent"] = "NovaHQ Heartbeat DLL (1.0.9)";
                    client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    NameValueCollection vars = new NameValueCollection
                    {
                        { "Encoding", "windows-1252" },
                        { "PKey", "eYkJaPPR-3WNbgPN93,(ZwxBCnEW" },
                        { "PVer", "1.0.9" },
                        { "SKey", "SECRET_KEY" },
                        { "DataType", "0x100" },
                        { "GameID", "dfbhd" },
                        { "Name", _state.Instances[instance.Key].ServerName },
                        { "Port", _state.Instances[instance.Key].GamePort.ToString() },
                        { "CK", "0" },
                        { "Country", _state.Instances[instance.Key].CountryCode },
                        { "Type", "Dedicated" },
                        { "GameType", _state.Instances[instance.Key].GameTypeName },
                        { "CurrentPlayers", _state.Instances[instance.Key].PlayerList.Count.ToString() },
                        { "MaxPlayers", _state.Instances[instance.Key].MaxSlots.ToString() },
                        { "MissionName", _state.Instances[instance.Key].CurrentMap.MapName },
                        { "MissionFile", _state.Instances[instance.Key].CurrentMap.MapFile },
                        { "TimeRemaining", (_state.Instances[instance.Key].StartDelay + _state.Instances[instance.Key].TimeRemaining * 60).ToString() }
                    };
                    if (_state.Instances[instance.Key].Password != string.Empty)
                    {
                        vars.Add("Password", "Y");
                    }
                    else
                    {
                        vars.Add("Password", "");
                    }
                    vars.Add("Message", _state.Instances[instance.Key].MOTD);
                    if (_state.Instances[instance.Key].IsTeamSabre == true)
                    {
                        vars.Add("Mod", "TS:");
                    }
                    else
                    {
                        vars.Add("Mod", "");
                    }
                    if (_state.Instances[instance.Key].PlayerList.Count > 0)
                    {
                        foreach (var player in _state.Instances[instance.Key].PlayerList)
                        {
                            playerlistHQ.Add(new NovaHQPlayerListClass
                            {
                                Deaths = player.Value.deaths,
                                Kills = player.Value.kills,
                                NameBase64Encoded = player.Value.nameBase64,
                                PlayerName = player.Value.name,
                                TeamId = player.Value.team,
                                TeamText = player.Value.team.ToString(),
                                WeaponId = Convert.ToInt32(Enum.Parse(typeof(playerlist.WeaponStack), player.Value.selectedWeapon)),
                                WeaponText = player.Value.selectedWeapon
                            });
                        }
                        vars.Add("PlayerList", Crypt.Base64Encode(JsonConvert.SerializeObject(playerlistHQ)));
                    }
                    else
                    {
                        vars.Add("PlayerList", "eyIwIjogeyJOYW1lIjoiSG9zdCIsIk5hbWVCYXNlNjRFbmNvZGVkIjoiU0c5emRBPT0iLCJLaWxscyI6IjAiLCJEZWF0aHMiOiIwIiwiV2VhcG9uSWQiOiI1IiwiV2VhcG9uVGV4dCI6IkNBUi0xNSIsIlRlYW1JZCI6IjUiLCJUZWFtVGV4dCI6Ik5vbmUiIH19");
                    }
                    try
                    {
                        byte[] response = client.UploadValues("server/heartbeat-dll", vars);
                        string getResponse = Encoding.Default.GetString(response);
                    }
                    catch
                    {

                    }
                    _state.Instances[instance.Key].NextUpdateNovaHQ = DateTime.Now.AddSeconds(30.0);
                }
                else
                {
                    continue;
                }
            }
        }

        private void CheckForExpiredRCClients()
        {
            if (DateTime.Compare(ProgramConfig.checkRCClientsDate, DateTime.Now) == -1)
            {
                List<string> removalList = new List<string>();
                foreach (var RCClient in _state.rcClients)
                {
                    if (DateTime.Compare(DateTime.Parse(RCClient.Value.expires), DateTime.Now) == -1)
                    {
                        removalList.Add(RCClient.Key);
                    }
                    else
                    {
                        continue;
                    }
                }
                foreach (string sessionid in removalList)
                {
                    _state.rcClients[sessionid].authenticated = false;
                    _state.rcClients[sessionid].active = false;
                    _state.rcClients.Remove(sessionid);
                }
                ProgramConfig.checkRCClientsDate = DateTime.Now.AddMinutes(ProgramConfig.checkRCClientsInterval);
            }
            else
            {
                return;
            }
        }

        private void checkPlayerHistory()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.PlayerList == null)
                {
                    continue;
                }
                foreach (var playerObj in instance.Value.PlayerList)
                {
                    bool found = false;
                    foreach (var playerHistory in _state.playerHistories)
                    {
                        if (playerHistory.playerName == playerObj.Value.name && playerHistory.playerIP == playerObj.Value.address)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `playerhistory` (`id`, `playername`, `ip`, `firstseen`) VALUES (NULL, @playername, @ip, @firstseen);", db);
                        cmd.Parameters.AddWithValue("@playername", playerObj.Value.name);
                        cmd.Parameters.AddWithValue("@ip", playerObj.Value.address);
                        cmd.Parameters.AddWithValue("@firstseen", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        _state.playerHistories.Add(new playerHistory
                        {
                            DatabaseId = (int)db.LastInsertRowId,
                            firstSeen = DateTime.Now,
                            playerIP = playerObj.Value.address,
                            playerName = playerObj.Value.name
                        });
                        // HOOK: WelcomePlayers
                        if (_state.Instances[instance.Key].Plugins.WelcomeMessage)
                        {
                            _state.Instances[instance.Key].WelcomeQueue.Add(new WelcomePlayer
                            {
                                playerName = playerObj.Value.name,
                                ReturningPlayer = false,
                                Processed = false,
                                RunTime = DateTime.Now.AddSeconds(30.0),
                                Slot = playerObj.Value.slot
                            });
                        }
                    }
                    else
                    {
                        // HOOK: WelcomePlayers
                        if (_state.Instances[instance.Key].Plugins.WelcomeMessage)
                        {
                            bool plFound = false;
                            int index = -1;
                            foreach (var item in _state.Instances[instance.Key].WelcomeQueue)
                            {
                                index++;
                                if (item.playerName == playerObj.Value.name)
                                {
                                    plFound = true;
                                    break;
                                }
                            }
                            if (plFound != true)
                            {
                                _state.Instances[instance.Key].WelcomeQueue.Add(new WelcomePlayer
                                {
                                    playerName = playerObj.Value.name,
                                    ReturningPlayer = true,
                                    Processed = false,
                                    RunTime = DateTime.Now.AddSeconds(30.0),
                                    Slot = playerObj.Value.slot
                                });
                            }
                        }
                    }
                }
            }

            db.Close();
            db.Dispose();
        }

        private void cleanRCClients()
        {
            List<string> deleteClients = new List<string>();
            // clean the dead clients and reopen ports after 30 minutes of inactivity, or if the client never authenicated.
            foreach (var clientObj in _state.rcClients)
            {
                int dateCompare = DateTime.Compare(DateTime.Parse(clientObj.Value.expires), clientObj.Value.lastCMDTime);
                if (dateCompare < 0)
                {
                    deleteClients.Add(clientObj.Key);
                }
            }
            foreach (var client in deleteClients)
            {
                _state.rcClients[client].active = false;
                _state.rcClients.Remove(client);
            }
        }

        private void SetupAppState(SQLiteConnection db)
        {
            try
            {
                string sql = @"SELECT i.*, pi.pid, ic.* FROM instances i 
                        LEFT JOIN instances_pid pi ON i.id = pi.profile_id 
                        LEFT JOIN instances_config ic ON i.id = ic.profile_id;";
                SQLiteCommand command = new SQLiteCommand(sql, db);
                SQLiteDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    bool TeamSabre = false;
                    if (result.GetInt32(result.GetOrdinal("game_type")) == 0)
                    {
                        if (File.Exists(result.GetString(result.GetOrdinal("gamepath")) + "\\EXP1.pff"))
                        {
                            TeamSabre = true;
                        }
                    }

                    string WebstatsURL = "";
                    bool EnableWebStats;
                    switch (result.GetInt32(result.GetOrdinal("stats")))
                    {
                        case 1:
                            WebstatsURL = result.GetString(result.GetOrdinal("stats_url"));
                            EnableWebStats = true;
                            break;
                        case 2:
                            WebstatsURL = result.GetString(result.GetOrdinal("stats_url"));
                            EnableWebStats = true;
                            break;
                        default:
                            WebstatsURL = "";
                            EnableWebStats = false;
                            break;
                    }
                    //var jsonWeaponRestrictions = JsonConvert.DeserializeObject<WeaponsClass>(result.GetString(result.GetOrdinal("weaponrestrictions")));
                    Dictionary<int, MapList> mapList = new Dictionary<int, MapList>();
                    if (result.GetString(result.GetOrdinal("mapcycle")) != "[]")
                    {
                        mapList = JsonConvert.DeserializeObject<Dictionary<int, MapList>>(result.GetString(result.GetOrdinal("mapcycle")));
                    }
                    Dictionary<int, MapList> availableMaps = new Dictionary<int, MapList>();
                    if (result.GetString(result.GetOrdinal("availablemaps")) != "[]")
                    {
                        availableMaps = JsonConvert.DeserializeObject<Dictionary<int, MapList>>(result.GetString(result.GetOrdinal("availablemaps")));
                    }
                    int testHQ = result.GetInt32(result.GetOrdinal("novahq_master"));
                    int testCC = result.GetInt32(result.GetOrdinal("novacc_master"));
                    PluginsClass plugins;
                    if (result.GetString(result.GetOrdinal("plugins")) == "[]")
                    {
                        plugins = new PluginsClass
                        {
                            WelcomeMessage = false,
                            VoteMaps = false,
                            WelcomeMessageSettings = new wp_PluginSettings
                            {
                                NewPlayerMsg = "Welcome $(PlayerName)!",
                                ReturningPlayerMsg = "Welcome back $(PlayerName)!"
                            },
                            VoteMapSettings = new vm_PluginSettings
                            {
                                CoolDown = 1,
                                CoolDownType = vm_internal.CoolDownTypes.NUM_OF_MINS,
                                MinPlayers = 3
                            }
                        };
                    }
                    else
                    {
                        plugins = JsonConvert.DeserializeObject<PluginsClass>(result.GetString(result.GetOrdinal("plugins")));
                    }

                    Instance instance = new Instance()
                    {
                        Id = result.GetInt32(result.GetOrdinal("id")),
                        GamePath = result.GetString(result.GetOrdinal("gamepath")),
                        GameType = result.GetInt32(result.GetOrdinal("game_type")),
                        GameName = result.GetString(result.GetOrdinal("name")),
                        HostName = result.GetString(result.GetOrdinal("host_name")),
                        CountryCode = result.GetString(result.GetOrdinal("country_code")),
                        ServerName = result.GetString(result.GetOrdinal("server_name")),
                        Password = result.GetString(result.GetOrdinal("server_password")),
                        MOTD = result.GetString(result.GetOrdinal("motd")),
                        Dedicated = result.GetBoolean(result.GetOrdinal("run_dedicated")),
                        SessionType = result.GetInt32(result.GetOrdinal("session_type")),
                        MaxSlots = result.GetInt32(result.GetOrdinal("max_slots")),
                        GameScore = result.GetInt32(result.GetOrdinal("game_score")),
                        FBScore = result.GetInt32(result.GetOrdinal("fbscore")),
                        KOTHScore = result.GetInt32(result.GetOrdinal("kothscore")),
                        ZoneTimer = result.GetInt32(result.GetOrdinal("zone_timer")),
                        WindowedMode = result.GetBoolean(result.GetOrdinal("windowed_mode")),
                        LoopMaps = result.GetInt32(result.GetOrdinal("loop_maps")),
                        RespawnTime = result.GetInt32(result.GetOrdinal("respawn_time")),
                        RequireNovaLogin = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("require_novalogic"))),
                        AllowCustomSkins = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("allow_custom_skins"))),
                        MaxKills = result.GetInt32(result.GetOrdinal("max_kills")),
                        BindAddress = result.GetString(result.GetOrdinal("bind_address")),
                        GamePort = result.GetInt32(result.GetOrdinal("port")),
                        TimeLimit = result.GetInt32(result.GetOrdinal("time_limit")),
                        StartDelay = result.GetInt32(result.GetOrdinal("start_delay")),
                        MinPing = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("enable_min_ping"))),
                        MinPingValue = result.GetInt32(result.GetOrdinal("min_ping")),
                        MaxPing = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("enable_max_ping"))),
                        MaxPingValue = result.GetInt32(result.GetOrdinal("max_ping")),
                        OneShotKills = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("oneshotkills"))),
                        FatBullets = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("fatbullets"))),
                        DestroyBuildings = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("destroybuildings"))),
                        FriendlyFire = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("friendly_fire"))),
                        FriendlyFireWarning = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("friendly_fire_warning"))),
                        FriendlyTags = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("friendly_tags"))),
                        FriendlyFireKills = result.GetInt32(result.GetOrdinal("friendly_fire_kills")),
                        PSPTakeOverTime = result.GetInt32(result.GetOrdinal("psptakeover")),
                        AllowAutoRange = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("allow_auto_range"))),
                        AutoBalance = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("auto_balance"))),
                        BluePassword = result.GetString(result.GetOrdinal("blue_team_password")),
                        RedPassword = result.GetString(result.GetOrdinal("red_team_password")),
                        FlagReturnTime = result.GetInt32(result.GetOrdinal("flagreturntime")),
                        MaxTeamLives = result.GetInt32(result.GetOrdinal("max_team_lives")),
                        ShowTeamClays = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("show_team_clays"))),
                        ShowTracers = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("show_tracers"))),
                        RoleRestrictions = GetRoleRestrictions(result.GetString(result.GetOrdinal("rolerestrictions"))),
                        WeaponRestrictions = GetWeaponRestrictions(result.GetString(result.GetOrdinal("weaponrestrictions"))),
                        LastUpdateTime = DateTime.Now,
                        NextUpdateTime = DateTime.Now.AddSeconds(2.0),
                        nextWebStatsStatusUpdate = DateTime.Now.AddSeconds(2.0),
                        EnableWebStats = EnableWebStats,
                        enableVPNCheck = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("enableVPNCheck"))),
                        WebStatsSoftware = result.GetInt32(result.GetOrdinal("stats")),
                        WebstatsURL = WebstatsURL,
                        availableMaps = availableMaps,
                        MapList = mapList,
                        mapListCount = mapList.Count,
                        WebstatsIdVerified = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("stats_verified"))),
                        ReportMaster = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("misc_babstats_master"))),
                        Status = InstanceStatus.OFFLINE,
                        anti_stat_padding = result.GetInt32(result.GetOrdinal("anti_stat_padding")),
                        anti_stat_padding_min_minutes = result.GetInt32(result.GetOrdinal("anti_stat_padding_min_minutes")),
                        anti_stat_padding_min_players = result.GetInt32(result.GetOrdinal("anti_stat_padding_min_players")),
                        CrashRecovery = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("misc_crashrecovery"))),
                        misc_show_ranks = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("misc_show_ranks"))),
                        misc_left_leaning = result.GetInt32(result.GetOrdinal("misc_left_leaning")),
                        WebStatsId = result.GetInt32(result.GetOrdinal("stats_server_id")),
                        PlayerList = new Dictionary<int, playerlist>(),
                        previousMapList = new Dictionary<int, MapList>(),
                        IPWhiteList = new Dictionary<string, string>(),
                        ScoreBoardDelay = result.GetInt32(result.GetOrdinal("scoreboard_override")),
                        ReportNovaHQ = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("novahq_master"))),
                        ReportNovaCC = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("novacc_master"))),
                        Plugins = plugins,
                        VoteMapStandBy = true,
                        IsTeamSabre = TeamSabre,
                    };
                    var collectPlayerStats = new CollectedPlayerStatsPlayers()
                    {
                        Player = new Dictionary<string, CollectedPlayerStats>()
                    };

                    /*ChatLogHandler = new Timer()
                    {
                        Enabled = true
                    };
                    ChatLogHandler.Tick += (sender, e) => {
                        for (int i = 0; i < _state.Instances.Count; i++)
                        {
                            if (_state.Instances[i].Status != InstanceStatus.OFFLINE && _state.Instances[i].Status != InstanceStatus.LOADINGMAP)
                            {
                                GetChatLogs(i);
                            }
                        }
                    };*/
                    int instanceId = _state.Instances.Count;
                    _state.ChatHandlerTimer.Add(_state.Instances.Count, new Timer
                    {
                        Enabled = true,
                        Interval = 1
                    });
                    _state.ChatHandlerTimer[_state.Instances.Count].Tick += (sender, e) =>
                    {
                        if (_state.Instances[instanceId].Status != InstanceStatus.OFFLINE && _state.Instances[instanceId].Status != InstanceStatus.LOADINGMAP)
                        {
                            GetChatLogs(instanceId);
                        }
                    };

                    /*QueueHandler = new Thread(() =>
                    {
                        while (true)
                        {
                            for (int i = 0; i < _state.ConsoleQueue.Count; i++)
                            {
                                ProcessConsoleCommands(i);
                            }
                        }
                    });*/

                    if (!result.IsDBNull(result.GetOrdinal("pid")))
                    {
                        instance.PID = result.GetInt32(result.GetOrdinal("pid"));
                        _state.eventLog.WriteEntry("Found PID: " + instance.PID.GetValueOrDefault().ToString(), EventLogEntryType.Information);
                    }

                    if (ProcessExist(instance.PID.GetValueOrDefault()))
                    {
                        _state.eventLog.WriteEntry("Attempting to attach... ID: " + instance.Id, EventLogEntryType.Information);
                        if (!_state.ApplicationProcesses.ContainsKey(_state.Instances.Count))
                        {
                            // to do... INCLUDE CUSTOM MODS
                            if (Process.GetProcessById(instance.PID.GetValueOrDefault()).ProcessName == "dfbhd")
                            {
                                _state.ApplicationProcesses.Add(_state.Instances.Count, Process.GetProcessById(instance.PID.GetValueOrDefault()));
                            }
                        }
                        try
                        {
                            instance.Handle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, instance.PID.GetValueOrDefault());
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        if (instance.Handle == null || instance.Handle == IntPtr.Zero)
                        {
                            _state.eventLog.WriteEntry("Unable to attach to process...", EventLogEntryType.Error, 0, 0);
                            throw new Exception("Unable to attach to process...");
                        }
                    }
                    else
                    {
                        _state.eventLog.WriteEntry("Could not find PID: " + instance.PID.GetValueOrDefault().ToString(), EventLogEntryType.Warning);
                    }

                    _state.eventLog.WriteEntry("Attachment successful: " + instance.Id, EventLogEntryType.Information);
                    _state.eventLog.WriteEntry("Adding instance: " + instance.Id, EventLogEntryType.Information);

                    // init AppState
                    _state.ConsoleQueue.Add(_state.Instances.Count, new ConsoleQueue
                    {
                        queue = new List<Queue>(),
                        nextCmd = DateTime.Now.AddSeconds(10)
                    });
                    _state.ConsoleQueue[_state.Instances.Count].queue.Add(new Queue
                    {
                        text = "BMTv4 is starting...",
                        Type = ConsoleQueueType.MESSAGE,
                        color = ChatColor.NORMAL
                    });
                    _state.Instances.Add(_state.Instances.Count, instance);
                    // this should start a timer to automatically check for new messages in a different thread than the main thread.
                    _state.PlayerStats.Add(_state.PlayerStats.Count, collectPlayerStats);
                    // start the chat handler in it's OWN THREAD PER SERVER.
                    _state.ChatHandlerTimer[instanceId].Start();
                }
                command.Dispose();
                result.Close();
            }
            catch (Exception e)
            {
                _state.eventLog.WriteEntry("BMTv4 TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                log.Debug(e);
            }
        }

        private void ProcessConsoleCommands(int i)
        {
            if (_state.ConsoleQueue[i].queue.Count > 0)
            {
                if (DateTime.Compare(DateTime.Now, _state.ConsoleQueue[i].nextCmd) == -1)
                {
                    for (int xi = 0; xi < _state.ConsoleQueue[xi].queue.Count; xi++)
                    {
                        if (_state.ConsoleQueue[i].queue[xi].Type == ConsoleQueueType.CMD_SCOREMAP)
                        {
                            // open console
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYDOWN, console, 0);
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYUP, console, 0);
                            Thread.Sleep(50);
                            int bytesWritten3 = 0;
                            byte[] buffer3 = Encoding.Default.GetBytes("resetgames\0"); // '\0' marks the end of string
                            MemoryProcessor.Write(_state.Instances[i], 0x00879A14, buffer3, buffer3.Length, ref bytesWritten3);
                            Thread.Sleep(50);
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                        }
                        else if (_state.ConsoleQueue[i].queue[xi].Type == ConsoleQueueType.CMD_KICKPLAYER)
                        {
                            // open console
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYDOWN, console, 0);
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYUP, console, 0);
                            Thread.Sleep(50);
                            int bytesWritten3 = 0;
                            byte[] buffer3 = Encoding.Default.GetBytes("punt \0"); // '\0' marks the end of string
                            MemoryProcessor.Write(_state.Instances[i], 0x00879A14, buffer3, buffer3.Length, ref bytesWritten3);
                            Thread.Sleep(50);
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                            PostMessage(_state.ApplicationProcesses[i].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                        }
                        else if (_state.ConsoleQueue[i].queue[xi].Type == ConsoleQueueType.MESSAGE)
                        {

                        }
                        else if (_state.ConsoleQueue[i].queue[xi].Type == ConsoleQueueType.PLAYER_WARNING)
                        {

                        }
                    }
                }
            }
            _state.ConsoleQueue[i].nextCmd = DateTime.Now.AddSeconds(10);
        }

        private SystemInfoClass GatherSystemInfo()
        {
            SystemInfoClass systemInfo = new SystemInfoClass();
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject MOS_info in MOS.Get())
            {
                systemInfo.OSName = (string)MOS_info["Caption"];
                systemInfo.OSVersion = (string)MOS_info["Version"];
                systemInfo.OSBuild = Environment.OSVersion.Version.Build.ToString();
                systemInfo.VirtualMemory = Convert.ToString((Convert.ToInt32(MOS_info["TotalVirtualMemorySize"])) / 1024 / 1024) + "GB";
            }
            ManagementObjectSearcher CPU = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            systemInfo.CPUs = new Dictionary<int, string>();
            foreach (ManagementObject CPU_info in CPU.Get())
            {
                systemInfo.CPUs.Add(systemInfo.CPUs.Count, (string)CPU_info["Name"]);
            }
            long memkb;
            GetPhysicallyInstalledSystemMemory(out memkb);
            systemInfo.Memory = Convert.ToString((memkb / 1024 / 1024) + "GB");

            return systemInfo;
        }

        private void CheckForUpdates()
        {
            // Remove Updates
            return;
        }

        private string GetMACAddress()
        {
            List<string> macAddresses = new List<string>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    macAddresses.Add(nic.GetPhysicalAddress().ToString());
                    break;
                }
            }
            string jsonMacAddresses = JsonConvert.SerializeObject(macAddresses);
            return jsonMacAddresses;
        }

        private void CheckForExpiredBans()
        {
            foreach (var inst in _state.Instances)
            {
                Instance instance = inst.Value;
                int InstanceID = instance.Id;
                try
                {
                    foreach (var ban in instance.BanList)
                    {
                        // remove if it's expired, even if the player isn't on the server...
                        if (ban.expires != "-1" && ban.expires != null)
                        {
                            if (DateTime.Compare(DateTime.Parse(ban.expires), DateTime.Now) < 0)
                            {
                                _state.Instances[inst.Key].BanList.Remove(ban);
                                // remove from SQLiteDB
                                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                                db.Open();
                                SQLiteCommand cmdRemove = new SQLiteCommand("DELETE FROM `playerbans` WHERE `id` = @banid AND `profileid` = @profileid;", db);
                                cmdRemove.Parameters.AddWithValue("@banid", ban.id);
                                cmdRemove.Parameters.AddWithValue("@profileid", InstanceID);
                                cmdRemove.ExecuteNonQuery();
                                cmdRemove.Dispose();
                                db.Close();
                                db.Dispose();
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }
            }
            // reset checkExpiredBansChecker
            ProgramConfig.checkExpiredBans = DateTime.Now.AddMinutes(5);
        }

        public List<ipqualityClass> load_ipqualityCache(int profileid, SQLiteConnection db)
        {
            List<ipqualityClass> loadCache = new List<ipqualityClass>();
            SQLiteCommand query = new SQLiteCommand("SELECT * FROM `ipqualitycache` WHERE `profile_id` = @profileid;", db);
            query.Parameters.AddWithValue("@profileid", profileid);
            SQLiteDataReader reader = query.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    loadCache.Add(new ipqualityClass
                    {
                        address = reader.GetString(reader.GetOrdinal("address")),
                        fraud_score = reader.GetInt32(reader.GetOrdinal("fraud_score")),
                        country_code = reader.GetString(reader.GetOrdinal("country_code")),
                        region = reader.GetString(reader.GetOrdinal("region")),
                        city = reader.GetString(reader.GetOrdinal("city")),
                        ISP = reader.GetString(reader.GetOrdinal("ISP")),
                        is_crawler = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("is_crawler")))),
                        mobile = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("mobile")))),
                        host = reader.GetString(reader.GetOrdinal("host")),
                        proxy = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("proxy")))),
                        vpn = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("vpn")))),
                        tor = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("tor")))),
                        active_vpn = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("active_vpn")))),
                        active_tor = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("active_tor")))),
                        recent_abuse = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("recent_abuse")))),
                        bot_status = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("bot_status")))),
                        request_id = reader.GetString(reader.GetOrdinal("request_id")),
                        latitude = reader.GetString(reader.GetOrdinal("lat")),
                        longitude = reader.GetString(reader.GetOrdinal("long")),
                        NextCheck = DateTime.Parse(reader.GetString(reader.GetOrdinal("NextCheck")))
                    });
                }
                reader.Close();
                query.Dispose();
                return loadCache;
            }
            else
            {
                return new List<ipqualityClass>();
            }
        }

        public Dictionary<int, VPNWhiteListClass> load_vpnwhitelist(int ArrayID, int InstanceID, SQLiteConnection db)
        {
            Dictionary<int, VPNWhiteListClass> WhiteList = new Dictionary<int, VPNWhiteListClass>();
            SQLiteCommand checkVPNQuery = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = @key;", db);
            checkVPNQuery.Parameters.AddWithValue("@key", "check_for_vpn");
            int checkVPN = Convert.ToInt32(checkVPNQuery.ExecuteScalar());
            ProgramConfig.EnableVPNCheck = Convert.ToBoolean(checkVPN);
            if (checkVPN == 1)
            {
                checkVPNQuery.Parameters.AddWithValue("@key", "ip_quality_score_apikey");
                string apikey = checkVPNQuery.ExecuteScalar().ToString();
                ProgramConfig.Enable_VPNWhiteList = true;
                ProgramConfig.ip_quality_score_apikey = apikey;
                SQLiteCommand query = new SQLiteCommand("SELECT `description`, `address` FROM `vpnwhitelist` WHERE `profile_id` = @profileid;", db);
                query.Parameters.AddWithValue("@profileid", InstanceID);
                SQLiteDataReader list = query.ExecuteReader();
                bool hasRows = list.HasRows;
                if (hasRows == true)
                {
                    int Index = 0;
                    while (list.Read())
                    {
                        string description = list.GetString(list.GetOrdinal("description"));
                        IPAddress address = IPAddress.Parse(list.GetString(list.GetOrdinal("address")));
                        WhiteList.Add(Index, new VPNWhiteListClass
                        {
                            Description = description,
                            IPAddress = address.ToString()
                        });
                        Index++;
                    }
                }
                list.Close();
                query.Dispose();
                checkVPNQuery.Dispose();
                return WhiteList;
            }
            else
            {
                return new Dictionary<int, VPNWhiteListClass>();
            }
        }

        private static string GetCommandLine(Process process)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }
        }
        private void beginRCListen()
        {
            try
            {
                WatsonTcpServer server = _state.server;
                server.Events.MessageReceived += Events_MessageReceived;
                server.Events.ClientConnected += Events_ClientConnected;
                server.Events.ServerStarted += Events_ServerStarted;
                server.Events.ServerStopped += Events_ServerStopped;
                server.Events.StreamReceived += Events_StreamReceived;
                server.Events.ExceptionEncountered += Events_ExceptionEncountered;
                server.Callbacks.SyncRequestReceived = RunRCCmd;
                if (ProgramConfig.EnableRC == true)
                {
                    server.Start();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to bind to port: " + ProgramConfig.RCPort.ToString(), "ERROR");
                log.Debug(e);
            }
        }

        private SyncResponse RunRCCmd(SyncRequest request)
        {
            RCListener client = new RCListener(_state);
            Dictionary<object, object> metadata = new Dictionary<object, object>();

            byte[] bytes = Compression.Compress(client.BMTRemoteFunctions(clineNo, ref metadata, null, request));

            return new SyncResponse(request, bytes);
        }

        private void Events_ExceptionEncountered(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine(e);
        }

        private void Events_StreamReceived(object sender, StreamReceivedEventArgs e)
        {
            Console.WriteLine("Testing");
        }

        private void Events_ServerStopped(object sender, EventArgs e)
        {
            Console.WriteLine(">> Remote Client Listener Stopped.");
        }

        private void Events_ServerStarted(object sender, EventArgs e)
        {
            Console.WriteLine(">> Remote Client Listener Started.");
        }

        private void Events_ClientConnected(object sender, WatsonTcp.ConnectionEventArgs e)
        {
            clineNo++;
        }

        private void Events_MessageReceived(object sender, MessageReceivedEventArgs msg)
        {
            Dictionary<object, object> metadata = new Dictionary<object, object>();
            RCListener client = new RCListener(_state);
            client.BMTRemoteFunctions(clineNo, ref metadata, msg, null);
        }

        public void SetHostnames(Instance _instance)
        {
            bool processExists = ProcessExist(_instance.PID.GetValueOrDefault());
            if (processExists == true)
            {
                int buffer = 0;
                byte[] PointerAddr = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;
                ReadProcessMemory((int)_instance.Handle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                int buffer2 = 0;
                byte[] Hostname = Encoding.Default.GetBytes(_instance.HostName + "\0");
                var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                MemoryProcessor.Write(_instance, (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
            }
        }

        private void GetGameTypes(SQLiteConnection conn)
        {
            var sql = "SELECT * FROM gametypes;";

            SQLiteCommand defaultmaps_cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader defaultmaps_reader = defaultmaps_cmd.ExecuteReader();
            while (defaultmaps_reader.Read())
            {
                bitmapsAndGameTypes.Add(defaultmaps_reader.GetInt32(defaultmaps_reader.GetOrdinal("bitmap")),
                  new GameType
                  {
                      DatabaseId = defaultmaps_reader.GetInt32(defaultmaps_reader.GetOrdinal("id")),
                      Name = defaultmaps_reader.GetString(defaultmaps_reader.GetOrdinal("name")),
                      ShortName = defaultmaps_reader.GetString(defaultmaps_reader.GetOrdinal("shortname")),
                      Bitmap = defaultmaps_reader.GetInt32(defaultmaps_reader.GetOrdinal("bitmap"))
                  }
                );
            }
            defaultmaps_cmd.Dispose();
            defaultmaps_reader.Close();
        }

        public void ProcessAutoMessage(int id)
        {
            if (_state.Instances[id].AutoMessages.enable_msg == true)
            {
                if (_state.Instances[id].AutoMessages.messages.Count != 0)
                {
                    if (DateTime.Compare(_state.Instances[id].AutoMessages.NextMessage, DateTime.Now) < 0)
                    {
                        if (_state.Instances[id].AutoMessages.MsgNumber.Equals(_state.Instances[id].AutoMessages.messages.Count))
                        {
                            _state.Instances[id].AutoMessages.MsgNumber = 0;
                        }

                        // don't do anything if no one is on the server
                        if (_state.Instances[id].PlayerList.Count == 0)
                        {
                            _state.Instances[id].AutoMessages.NextMessage = DateTime.Now.AddMinutes(_state.Instances[id].AutoMessages.interval);
                            return;
                        }


                        int colorbuffer_written = 0;
                        byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[id], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                        Thread.Sleep(100);
                        // open console
                        PostMessage(_state.ApplicationProcesses[id].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                        PostMessage(_state.ApplicationProcesses[id].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                        Thread.Sleep(100);
                        int bytesWritten = 0;

                        string msg = _state.Instances[id].AutoMessages.messages[_state.Instances[id].AutoMessages.MsgNumber];


                        if (msg.Contains("$(NextMap)"))
                        {
                            int mapIndex = 0;
                            if (_state.Instances[id].mapIndex != _state.Instances[id].MapList.Count)
                            {
                                mapIndex = _state.Instances[id].mapIndex + 1;
                            }
                            msg = msg.Replace("$(NextMap)", "Next Map: " + _state.Instances[id].MapList[mapIndex].MapName);
                        }
                        else if (msg.Contains("$(HighestEXP)"))
                        {
                            string playerName = string.Empty;
                            int playerExp = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.exp > playerExp)
                                {
                                    playerName = player.Value.name;
                                    playerExp = player.Value.exp;
                                }
                            }
                            msg = msg.Replace("$(HighestEXP)", "Highest Exp: " + playerName + ": " + playerExp.ToString() + " EXP");
                        }
                        else if (msg.Contains("$(LowestEXP)"))
                        {
                            string playerName = string.Empty;
                            int playerExp = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.exp < playerExp)
                                {
                                    playerName = player.Value.name;
                                    playerExp = player.Value.exp;
                                }
                            }
                            msg.Replace("$(LowestEXP)", "Lowest Exp: " + playerName + ": " + playerExp.ToString() + " EXP");
                        }
                        else if (msg.Contains("$(MostKills)"))
                        {
                            string playerName = string.Empty;
                            int playerKills = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.kills > playerKills)
                                {
                                    playerName = player.Value.name;
                                    playerKills = player.Value.kills;
                                }
                            }
                            msg.Replace("$(MostKills)", "Most Kills: " + playerName + ": " + playerKills);
                        }
                        else if (msg.Contains("$(MostDeaths)"))
                        {
                            string playerName = string.Empty;
                            int playerDeaths = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.deaths > playerDeaths)
                                {
                                    playerName = player.Value.name;
                                    playerDeaths = player.Value.deaths;
                                }
                            }
                            msg.Replace("$(MostDeaths)", "Most Deaths: " + playerName + ": " + playerDeaths);
                        }
                        else if (msg.Contains("$(BestKDR)"))
                        {
                            string playerName = string.Empty;
                            decimal playerKDR = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if ((decimal)(player.Value.kills / player.Value.deaths) > playerKDR)
                                {
                                    playerName = player.Value.name;
                                    playerKDR = (decimal)(player.Value.kills / player.Value.deaths);
                                }
                            }
                            msg.Replace("$(BestKDR)", "Best KDR: " + playerName + ": " + playerKDR);
                        }
                        else if (msg.Contains("$(MostRevives)"))
                        {
                            string playerName = string.Empty;
                            int playerRevives = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.playerrevives > playerRevives)
                                {
                                    playerName = player.Value.name;
                                    playerRevives = player.Value.playerrevives;
                                }
                            }
                            msg.Replace("$(MostRevives)", "Most Player Revives: " + playerName + ": " + playerRevives);
                        }
                        else if (msg.Contains("$(MostHeadshots)"))
                        {
                            string playerName = string.Empty;
                            int playerHeadshots = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.headshots > playerHeadshots)
                                {
                                    playerName = player.Value.name;
                                    playerHeadshots = player.Value.headshots;
                                }
                            }
                            msg.Replace("$(MostHeadshots)", "Most Headshots: " + playerName + ": " + playerHeadshots);
                        }
                        else if (msg.Contains("$(MostSuicides)"))
                        {
                            string playerName = string.Empty;
                            int playerSuicides = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.suicides > playerSuicides)
                                {
                                    playerName = player.Value.name;
                                    playerSuicides = player.Value.suicides;
                                }
                            }
                            msg.Replace("$(MostSuicides)", "Most Suicides: " + playerName + ": " + playerSuicides);
                        }
                        else if (msg.Contains("$(MostTKs)"))
                        {
                            string playerName = string.Empty;
                            int playerTKs = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.teamkills > playerTKs)
                                {
                                    playerName = player.Value.name;
                                    playerTKs = player.Value.teamkills;
                                }
                            }
                            msg.Replace("$(MostTKs)", "Most Team Kills: " + playerName + ": " + playerTKs);
                        }

                        if (msg == "" || msg == string.Empty)
                        {
                            return; // do nothing since there is no message.
                        }

                        byte[] buffer = Encoding.Default.GetBytes($"{msg}\0"); // '\0' marks the end of string
                        MemoryProcessor.Write(_state.Instances[id], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                        Thread.Sleep(100);
                        PostMessage(_state.ApplicationProcesses[id].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                        PostMessage(_state.ApplicationProcesses[id].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                        Thread.Sleep(100);
                        int revert_colorbuffer = 0;
                        byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[id], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);

                        // insert memory write here
                        _state.Instances[id].AutoMessages.NextMessage = DateTime.Now.AddMinutes(_state.Instances[id].AutoMessages.interval);
                        _state.Instances[id].AutoMessages.MsgNumber++;
                    }
                }
                else
                {
                    return; // return if there are no messages present
                }
            }
            else
            {
                return; // return if the AutoMessages are disabled
            }
        }
        public void GetResultsTable()
        {
            BindingSource bindingSource = new BindingSource
            {
                DataSource = table
            };
            for (int i = 0; i < _state.Instances.Count; i++)
            {
                string img;
                string statusIMG;
                int findInstanceIndex = bindingSource.Find("ID", _state.Instances[i].Id);
                if (findInstanceIndex == -1)
                {
                    DataRow dr = table.NewRow();
                    statusIMG = "notactive.gif";
                    if (_state.Instances[i].GameType == 0 && _state.Instances[i].IsTeamSabre == true)
                    {
                        img = "bhdts.gif";
                    }
                    else if (_state.Instances[i].GameType == 0 && _state.Instances[i].IsTeamSabre == false)
                    {
                        img = "bhd.gif";
                    }
                    else
                    {
                        img = "bhd.gif";
                    }
                    dr["ID"] = _state.Instances[i].Id;
                    dr["Game Name"] = _state.Instances[i].GameName;
                    dr["Mod"] = imageToByteArray(img);
                    dr["Server Status"] = imageToByteArray(statusIMG);
                    table.Rows.Add(dr);
                }
            }
            bindingSource.Dispose();
        }

        private PlayerRoles GetRoleRestrictions(string v)
        {
            if (v == "[]")
            {
                PlayerRoles playerRoles = new PlayerRoles()
                {
                    CQB = true,
                    Gunner = true,
                    Medic = true,
                    Sniper = true
                };
                return playerRoles;
            }
            else
            {
                var playerRolesJson = JsonConvert.DeserializeObject<PlayerRoles>(v);
                PlayerRoles playerRoles = playerRolesJson;
                return playerRoles;
            }
        }

        public WeaponsClass GetWeaponRestrictions(string json)
        {
            if (json == "[]")
            {
                WeaponsClass SetupWeaponRestrictions = new WeaponsClass()
                {
                    WPN_AN_M8_SMOKE = true,
                    WPN_AT4 = true,
                    WPN_BARRETT = true,
                    WPN_CAR15 = true,
                    WPN_CAR15_203 = true,
                    WPN_CLAYMORE = true,
                    WPN_COLT45 = true,
                    WPN_G3 = true,
                    WPN_G36 = true,
                    WPN_M16_BURST = true,
                    WPN_M16_BURST_203 = true,
                    WPN_M21 = true,
                    WPN_M24 = true,
                    WPN_M240 = true,
                    WPN_M60 = true,
                    WPN_M67_FRAG = true,
                    WPN_M9BERETTA = true,
                    WPN_MCRT_300_TACTICAL = true,
                    WPN_MP5 = true,
                    WPN_PSG1 = true,
                    WPN_RADIO_DETONATOR = true,
                    WPN_REMMINGTONSG = true,
                    WPN_SATCHEL_CHARGE = true,
                    WPN_SAW = true,
                    WPN_XM84_STUN = true
                };
                return SetupWeaponRestrictions;
            }
            else
            {
                var DBWeaponRestrictions = JsonConvert.DeserializeObject<WeaponsClass>(json);
                WeaponsClass weaponRestrictions = DBWeaponRestrictions;
                return weaponRestrictions;
            }
        }

        public static DateTime ConvertUnixTime(int unixtime)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixtime).ToLocalTime();
            return dateTime;
        }
        public void update_master()
        {
            foreach (var item in _state.Instances)
            {
                if (item.Value.ReportMaster == true && item.Value.Status != InstanceStatus.OFFLINE)
                {
                    var PID = item.Value.PID.GetValueOrDefault();
                    if (ProcessExist(PID))
                    {
                        if (!string.IsNullOrEmpty(item.Value.ServerName) || !string.IsNullOrEmpty(ProgramConfig.ipaddress))
                        {
                            string bindaddr = "";
                            if (item.Value.BindAddress == "0.0.0.0" || item.Value.BindAddress == "")
                            {
                                bindaddr = ProgramConfig.ipaddress;
                            }
                            else
                            {
                                bindaddr = item.Value.BindAddress;
                            }
                            DateTime currentTime = DateTime.Now;
                            DateTime UpdateTime = ConvertUnixTime(item.Value.MasterUnixNextUpdate);
                            if (DateTime.Compare(UpdateTime, currentTime) < 0)
                            {
                                NameValueCollection serverinfo = new NameValueCollection
                                {
                                    { "game", item.Value.GameType.ToString() },
                                    { "server_name", item.Value.ServerName },
                                    { "country_code", item.Value.CountryCode },
                                    { "ip", bindaddr },
                                    { "port", item.Value.GamePort.ToString() },
                                    { "dedicated", Convert.ToInt32(item.Value.Dedicated).ToString() },
                                    { "MOTD", item.Value.MOTD },
                                    { "timeleft", item.Value.TimeRemaining.ToString() },
                                    { "current_players", item.Value.NumPlayers.ToString() },
                                    { "total_players", item.Value.MaxSlots.ToString() },
                                    { "status", item.Value.Status.ToString() },
                                    { "map", item.Value.Map },
                                    { "playerlist", JsonConvert.SerializeObject(item.Value.PlayerList) },
                                    { "maptype", item.Value.GameTypeName },
                                    { "isteamsabre", Convert.ToInt32(item.Value.IsTeamSabre).ToString() }
                                };

                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
                                ServicePointManager.Expect100Continue = true;
                                WebClient master = new WebClient()
                                {
                                    BaseAddress = "https://master.babstats.net"
                                };
                                master.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                                master.Headers.Add("User-Agent", "Babstats Program");
                                byte[] result = new byte[1024];
                                try
                                {
                                    result = master.UploadValues("update.php", serverinfo);
                                    master_server list = JsonConvert.DeserializeObject<master_server>(Encoding.ASCII.GetString(result));
                                    if (list.Status == "Banned")
                                    {
                                        _state.Instances[item.Key].ReportMaster = false;
                                        continue;
                                    }

                                    item.Value.MasterUnixNextUpdate = list.UnixNextUpdate;
                                    master.Dispose();
                                }
                                catch (WebException ex)
                                {
                                    _state.eventLog.WriteEntry("Error updating master server: " + ex.Message, EventLogEntryType.Error);
                                    item.Value.MasterUnixNextUpdate = (int)DateTimeOffset.Now.AddMinutes(1.0).ToUnixTimeSeconds();
                                    return;
                                }
                                catch (Exception e)
                                {
                                    _state.eventLog.WriteEntry(Encoding.Default.GetString(result), EventLogEntryType.Warning);
                                    _state.eventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
                                    //ProgramDebug.SendReport(e, false);
                                    // ignore if we can't connect to the master server...
                                    master.Dispose();
                                    item.Value.MasterUnixNextUpdate = (int)DateTimeOffset.Now.AddMinutes(1.0).ToUnixTimeSeconds();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public byte[] imageToByteArray(string imageIn)
        {
            if (_state.imageCache.ContainsKey(imageIn))
            {
                return _state.imageCache[imageIn];
            }
            else
            {
                return new byte[1];
            }
        }

        private void Main_Profilelist_Load(object sender, EventArgs e)
        {
            table.Columns.Add("ID".ToString());
            table.Columns.Add("Game Name".ToString());
            table.Columns.Add("Mod".ToString(), typeof(byte[]));
            table.Columns.Add("Slots".ToString());
            table.Columns.Add("Map".ToString());
            table.Columns.Add("Game Type".ToString());
            table.Columns.Add("Time Remaining".ToString());
            table.Columns.Add("Web Stats Status".ToString());
            table.Columns.Add("Server Status".ToString(), typeof(byte[]));
            ProgramConfig.NovaStatusCheck = DateTime.Now;
            GetResultsTable();
            list_serverProfiles.DataSource = table;
            SetupTable();
            gametype_db.Open();
            SQLiteCommand warnlevelquery = new SQLiteCommand("SELECT `warnlevel` FROM `instances_config` WHERE `profile_id` = @profileid;", gametype_db);
            foreach (var item in _state.Instances)
            {
                warnlevelquery.Parameters.AddWithValue("@profileid", item.Value.Id);
                SQLiteDataReader warnLevelRead = warnlevelquery.ExecuteReader();
                warnLevelRead.Read();
                _state.Instances[item.Key].BanList = Load_BanList(item.Value.Id);
                _state.Instances[item.Key].VPNWhiteList = load_vpnwhitelist(item.Key, item.Value.Id, gametype_db);
                _state.Instances[item.Key].GodModeList = new List<int>();
                _state.Instances[item.Key].ChangeTeamList = new List<ChangeTeamClass>();
                _state.Instances[item.Key].CustomWarnings = load_customwarnings(item.Value.Id, gametype_db);
                _state.Instances[item.Key].WarningQueue = new List<WarnPlayerClass>();
                _state.Instances[item.Key].DisarmPlayers = new List<int>();
                _state.Instances[item.Key].WeaponRestrictions = load_weaponRestrictions(item.Value.Id, gametype_db);
                _state.Instances[item.Key].AutoMessages = load_autoMessages(item.Value.Id, gametype_db);
                _state.Instances[item.Key].previousTeams = new List<PreviousTeams>();
                _state.Instances[item.Key].savedmaprotations = load_savedRotations(item.Value.Id, gametype_db);
                _state.Instances[item.Key].CurrentMap = new MapList();
                _state.Instances[item.Key].WelcomeQueue = new List<WelcomePlayer>();
                _state.Instances[item.Key].VoteMapsTally = new List<VoteMapsTally>();
                _state.Instances[item.Key].VoteMapTimer = new Timer
                {
                    Enabled = false
                };
                _state.IPQualityCache.Add(item.Key, new ipqualityscore
                {
                    WarnLevel = warnLevelRead.GetInt32(warnLevelRead.GetOrdinal("warnlevel")),
                    IPInformation = load_ipqualityCache(item.Value.Id, gametype_db)
                });
                _state.ChatLogs[item.Key] = new ChatLogs();
                warnLevelRead.Close();
                warnLevelRead.Dispose();
                SetHostnames(item.Value);
            }
            warnlevelquery.Dispose();
            _state.Users = GetUsersFrmDB(gametype_db);
            _state.yearlystats = GetStatsFrmDB(gametype_db);
            _state.adminChatMsgs = GetAdminChatMsgs(gametype_db);
            _state.SystemInfo = GatherSystemInfo();
            _state.autoRes = SetupAutoResClass(gametype_db);
            _state.playerHistories = GetPlayerHistories(gametype_db);
            _state.adminNotes = GetAdminNotes(gametype_db);
            _state.RCLogs = GetRCLogs(gametype_db);

            GlobalAppState.AppState = _state;
            gametype_db.Close();
            Ticker.Enabled = true;
            Ticker.Start();
            if (_state.Instances.Count == 0)
            {
                btn_start.Enabled = false;
                UpdateButtonsOffline();
            }
            else
            {
                btn_start.Enabled = true;
            }
        }

        private List<RCLogs> GetRCLogs(SQLiteConnection db)
        {
            List<RCLogs> logs = new List<RCLogs>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `rclogs` ORDER BY `id` ASC;", db);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new RCLogs
                {
                    Action = reader.GetString(reader.GetOrdinal("action")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))),
                    SessionID = reader.GetString(reader.GetOrdinal("sessionid")),
                    Username = reader.GetString(reader.GetOrdinal("username"))
                });
            }
            reader.Close();
            reader.Dispose();
            cmd.Dispose();
            return logs;
        }

        private List<savedmaprotations> load_savedRotations(int id, SQLiteConnection db)
        {
            List<savedmaprotations> savedMapRotations = new List<savedmaprotations>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `instances_map_rotations` WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@profileid", id);
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                List<MapList> mapCycle;
                if (read.GetString(read.GetOrdinal("mapcycle")) != "[]")
                {
                    mapCycle = JsonConvert.DeserializeObject<List<MapList>>(read.GetString(read.GetOrdinal("mapcycle")));
                }
                else
                {
                    mapCycle = new List<MapList>();
                }

                savedMapRotations.Add(new savedmaprotations
                {
                    RotationID = read.GetInt32(read.GetOrdinal("rotation_id")),
                    Description = read.GetString(read.GetOrdinal("description")),
                    mapcycle = mapCycle
                });
            }

            return savedMapRotations;
        }

        private List<adminnotes> GetAdminNotes(SQLiteConnection db)
        {
            List<adminnotes> adminnotesList = new List<adminnotes>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `adminnotes`;", db);
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                adminnotesList.Add(new adminnotes
                {
                    userid = read.GetInt32(read.GetOrdinal("userid")),
                    name = read.GetString(read.GetOrdinal("name")),
                    msg = read.GetString(read.GetOrdinal("msg"))
                });
            }
            read.Close();
            read.Dispose();
            cmd.Dispose();

            return adminnotesList;
        }

        private List<playerHistory> GetPlayerHistories(SQLiteConnection db)
        {
            List<playerHistory> playerHistories = new List<playerHistory>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `playerhistory`;", db);
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                playerHistories.Add(new playerHistory
                {
                    DatabaseId = read.GetInt32(read.GetOrdinal("id")),
                    firstSeen = DateTime.Parse(read.GetString(read.GetOrdinal("firstseen"))),
                    playerIP = read.GetString(read.GetOrdinal("ip")),
                    playerName = read.GetString(read.GetOrdinal("playername"))
                });
            }
            return playerHistories;
        }

        private autoRes SetupAutoResClass(SQLiteConnection db)
        {
            Dictionary<string, GameType> gameTypes = new Dictionary<string, GameType>();
            SQLiteCommand gameTypesCmd = new SQLiteCommand("SELECT * FROM `gametypes`;", db);
            SQLiteDataReader gameTypesRead = gameTypesCmd.ExecuteReader();
            while (gameTypesRead.Read())
            {
                gameTypes.Add(gameTypesRead.GetString(gameTypesRead.GetOrdinal("shortname")), new GameType
                {
                    Bitmap = gameTypesRead.GetInt32(gameTypesRead.GetOrdinal("bitmap")),
                    DatabaseId = gameTypesRead.GetInt32(gameTypesRead.GetOrdinal("id")),
                    Name = gameTypesRead.GetString(gameTypesRead.GetOrdinal("name")),
                    ShortName = gameTypesRead.GetString(gameTypesRead.GetOrdinal("shortname"))
                });
            }
            gameTypesRead.Close();
            gameTypesRead.Dispose();
            gameTypesCmd.Dispose();
            autoRes autoRes = new autoRes
            {
                gameTypes = gameTypes
            };
            return autoRes;
        }

        private auto_messages load_autoMessages(int id, SQLiteConnection db)
        {
            auto_messages AutoMessages = new auto_messages();
            SQLiteCommand cmd = new SQLiteCommand("SELECT `enable_msg`, `auto_msg_interval`, `auto_messages` FROM `instances_config` WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@profileid", id);
            SQLiteDataReader read = cmd.ExecuteReader();
            read.Read();
            AutoMessages.enable_msg = Convert.ToBoolean(read.GetInt32(read.GetOrdinal("enable_msg")));
            AutoMessages.NextMessage = DateTime.Now.AddMinutes(1.0);
            AutoMessages.interval = read.GetInt32(read.GetOrdinal("auto_msg_interval"));
            AutoMessages.MsgNumber = 0;
            if (read.GetString(read.GetOrdinal("auto_messages")) == "[]")
            {
                AutoMessages.messages = new List<string>();
            }
            else
            {
                AutoMessages.messages = JsonConvert.DeserializeObject<List<string>>(read.GetString(read.GetOrdinal("auto_messages")));
            }
            return AutoMessages;
        }

        private WeaponsClass load_weaponRestrictions(int id, SQLiteConnection db)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT `weaponrestrictions` FROM `instances_config` WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@profileid", id);
            SQLiteDataReader read = cmd.ExecuteReader();
            read.Read();
            WeaponsClass weapons;
            if (read.GetString(read.GetOrdinal("weaponrestrictions")) == "[]")
            {
                weapons = new WeaponsClass();
            }
            else
            {
                weapons = JsonConvert.DeserializeObject<WeaponsClass>(read.GetString(read.GetOrdinal("weaponrestrictions")));
            }
            read.Close();
            cmd.Dispose();
            return weapons;
        }

        private List<string> load_customwarnings(int instanceid, SQLiteConnection gametype_db)
        {
            List<string> customWarnings = new List<string>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `customwarnings` WHERE `instanceid` = @instanceid;", gametype_db);
            cmd.Parameters.AddWithValue("@instanceid", instanceid);
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                customWarnings.Add(read.GetString(read.GetOrdinal("message")));
            }
            read.Close();
            cmd.Dispose();
            return customWarnings;
        }

        private List<AdminChatMsgs> GetAdminChatMsgs(SQLiteConnection gametype_db)
        {
            List<AdminChatMsgs> log = new List<AdminChatMsgs>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT `adminchatlog`.`id`, `adminchatlog`.`userid`, `users`.`username`, `adminchatlog`.`msg`, `adminchatlog`.`datesent` FROM `adminchatlog` INNER JOIN `users` ON `adminchatlog`.`userid` = `users`.`id`;", gametype_db);
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                log.Add(new AdminChatMsgs
                {
                    MsgID = read.GetInt32(read.GetOrdinal("id")),
                    UserID = read.GetInt32(read.GetOrdinal("userid")),
                    Username = read.GetString(read.GetOrdinal("username")),
                    Msg = read.GetString(read.GetOrdinal("msg")),
                    DateSent = DateTime.Parse(read.GetString(read.GetOrdinal("datesent")))
                });
            }
            read.Close();
            cmd.Dispose();
            return log;
        }

        private Dictionary<int, monthlystats> GetStatsFrmDB(SQLiteConnection gametype_db)
        {
            Dictionary<int, monthlystats> stats = new Dictionary<int, monthlystats>();

            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `monthlystats` WHERE `year` = @year;", gametype_db);
            cmd.Parameters.AddWithValue("@year", DateTime.Now.Year);

            // current year
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                if (!stats.ContainsKey(read.GetInt32(read.GetOrdinal("year"))))
                {
                    stats.Add(DateTime.Now.Year, new monthlystats
                    {
                        monthstat = new Dictionary<int, daystat>()
                        {
                            {
                                read.GetInt32(read.GetOrdinal("month")),
                                new daystat
                                {
                                    daystats = new List<day>()
                                    {
                                        new day
                                        {
                                            Day = read.GetInt32(read.GetOrdinal("day")),
                                            Count = read.GetInt32(read.GetOrdinal("count"))
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
                else
                {
                    if (!stats[read.GetInt32(read.GetOrdinal("year"))].monthstat.ContainsKey(read.GetInt32(read.GetOrdinal("month"))))
                    {
                        stats[read.GetInt32(read.GetOrdinal("year"))].monthstat = new Dictionary<int, daystat>
                        {
                            {
                                read.GetInt32(read.GetOrdinal("month")),
                                new daystat{
                                    daystats = new List<day>()
                                    {
                                        new day
                                        {
                                            Day = read.GetInt32(read.GetOrdinal("day")),
                                            Count = read.GetInt32(read.GetOrdinal("count"))
                                        }
                                    }
                                }
                            }
                        };
                    }
                    else
                    {
                        stats[read.GetInt32(read.GetOrdinal("year"))].monthstat[read.GetInt32(read.GetOrdinal("month"))].daystats.Add(new day
                        {
                            Day = read.GetInt32(read.GetOrdinal("day")),
                            Count = read.GetInt32(read.GetOrdinal("count"))
                        });
                    }
                }
            }
            read.Close();

            // previous year
            /*cmd.Parameters.AddWithValue("@year", DateTime.Now.AddYears(-1).Year);
            SQLiteDataReader pRead = cmd.ExecuteReader();
            while (pRead.Read())
            {
                stats.Add(pRead.GetInt32(pRead.GetOrdinal("year")), new monthlystats
                {
                    monthstat = new Dictionary<int, daystats>()
                    {
                        {
                            read.GetInt32(pRead.GetOrdinal("month")), new daystats {
                                day = pRead.GetInt32(read.GetOrdinal("day")),
                                count = pRead.GetInt32(read.GetOrdinal("count"))
                            }
                        }
                    }
                });
            }
            pRead.Close();*/

            cmd.Dispose();

            return stats;
        }

        private Dictionary<string, UserCodes> GetUsersFrmDB(SQLiteConnection gametype_db)
        {
            Dictionary<string, UserCodes> usersDB = new Dictionary<string, UserCodes>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `users`;", gametype_db);
            SQLiteDataReader read = cmd.ExecuteReader();
            Permissions userPermissions;
            while (read.Read())
            {
                if (read.GetString(read.GetOrdinal("permissions")) == "[]")
                {
                    userPermissions = new Permissions();
                }
                else
                {
                    userPermissions = JsonConvert.DeserializeObject<Permissions>(read.GetString(read.GetOrdinal("permissions")));
                }
                usersDB.Add(read.GetString(read.GetOrdinal("username")), new UserCodes
                {
                    UserID = read.GetInt32(read.GetOrdinal("id")),
                    Password = read.GetString(read.GetOrdinal("password")),
                    SuperAdmin = Convert.ToBoolean(read.GetInt32(read.GetOrdinal("superadmin"))),
                    SubAdmin = read.GetInt32(read.GetOrdinal("subadmin")),
                    Permissions = userPermissions
                });
            }
            read.Close();
            read.Dispose();
            cmd.Dispose();
            return usersDB;
        }

        private List<playerbans> Load_BanList(int profileid)
        {
            List<playerbans> Currentbans = new List<playerbans>();
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand query = new SQLiteCommand("SELECT * FROM `playerbans` WHERE `profileid` = @profileid;", db);
            query.Parameters.AddWithValue("@profileid", profileid);
            SQLiteDataReader reader = query.ExecuteReader();
            if (reader.HasRows)
            {
                int i = 0;
                while (reader.Read())
                {
                    string bannedIP = "";
                    if (reader.GetString(reader.GetOrdinal("ipaddress")) == "-1")
                    {
                        bannedIP = "None";
                    }
                    else
                    {
                        bannedIP = reader.GetString(reader.GetOrdinal("ipaddress"));
                    }
                    Currentbans.Add(new playerbans
                    {
                        id = reader.GetInt32(reader.GetOrdinal("id")),
                        player = reader.GetString(reader.GetOrdinal("player")),
                        ipaddress = bannedIP,
                        lastseen = DateTime.Parse(reader.GetString(reader.GetOrdinal("lastseen"))),
                        reason = reader.GetString(reader.GetOrdinal("reason")),
                        addedDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("dateadded"))),
                        expires = reader.GetString(reader.GetOrdinal("expires")),
                        retry = DateTime.Now,
                        bannedBy = reader.GetString(reader.GetOrdinal("bannedby")),
                        onlykick = false,
                        newBan = false,
                        VPNBan = false
                    });
                    i++;
                }
            }
            else
            {
                Currentbans = new List<playerbans>();
            }
            reader.Close();
            query.Dispose();
            db.Close();
            // set checkExpiredBans date
            ProgramConfig.checkExpiredBans = DateTime.Now;

            return Currentbans;
        }

        private void btnOptions_click(object sender, EventArgs e)
        {
            Options op = new Options(_state);
            op.ShowDialog();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Environment.ExitCode = 0;
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            // capture last index.
            string img;
            string statusIMG;
            int lastIndex = _state.Instances.Count;
            Create_Profile frm = new Create_Profile(_state);
            frm.ShowDialog();
            if (Create_Profile.ProfileCreated == true)
            {
                DataRow dr = table.NewRow();
                statusIMG = "notactive.gif";
                if (_state.Instances[lastIndex].GameType == 0 && _state.Instances[lastIndex].IsTeamSabre == true)
                {
                    img = "bhdts.gif";
                }
                else if (_state.Instances[lastIndex].GameType == 0 && _state.Instances[lastIndex].IsTeamSabre == false)
                {
                    img = "bhd.gif";
                }
                else if (_state.Instances[lastIndex].GameType == 1)
                {
                    img = "jo.gif";
                }
                else
                {
                    img = "bhd.gif";
                }
                dr["ID"] = _state.Instances[lastIndex].Id;
                dr["Game Name"] = _state.Instances[lastIndex].GameName;
                dr["Mod"] = imageToByteArray(img);
                dr["Server Status"] = imageToByteArray(statusIMG);
                table.Rows.Add(dr);
                MessageBox.Show("Profile Added Successfully.", "Success", MessageBoxButtons.OK);
                if (_state.Instances.Count > 0) { btn_start.Enabled = true; }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

            // Are there any Profiles to start?
            if ((list_serverProfiles.Rows.Count) == 0)
            {
                MessageBox.Show("There are no profiles to delete.");
                return;
            }

            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("Are you sure you want to delete this profile?", "Delete Profile", buttons);
            if (result == DialogResult.Yes)
            {
                int id = list_serverProfiles.CurrentRow.Index;
                DataRow instance = table.Rows[id];
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();

                SQLiteCommand chatLog_del = new SQLiteCommand("DELETE FROM `chatlog` WHERE `profile_id` = @profileid;", db);
                chatLog_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                chatLog_del.ExecuteNonQuery();
                chatLog_del.Dispose();

                SQLiteCommand customWarnings_del = new SQLiteCommand("DELETE FROM `customwarnings` WHERE `instanceid` = @profileid;", db);
                customWarnings_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                customWarnings_del.ExecuteNonQuery();
                customWarnings_del.Dispose();

                SQLiteCommand command = new SQLiteCommand("DELETE FROM `instances` WHERE id = @profileid;", db);
                command.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                command.ExecuteNonQuery();
                command.Dispose();

                SQLiteCommand config = new SQLiteCommand("DELETE FROM `instances_config` WHERE profile_id = @profileid;", db);
                config.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                config.ExecuteNonQuery();
                config.Dispose();

                SQLiteCommand mapRotations_del = new SQLiteCommand("DELETE FROM `instances_map_rotations` WHERE `profile_id` = @profileid;", db);
                mapRotations_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                mapRotations_del.ExecuteNonQuery();
                mapRotations_del.Dispose();

                SQLiteCommand pid_del = new SQLiteCommand("DELETE from `instances_pid` WHERE profile_id = @profileid;", db);
                pid_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                pid_del.ExecuteNonQuery();
                pid_del.Dispose();

                SQLiteCommand IPCache_del = new SQLiteCommand("DELETE FROM `ipqualitycache` WHERE `profile_id` = @profileid;", db);
                IPCache_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                IPCache_del.ExecuteNonQuery();
                IPCache_del.Dispose();

                SQLiteCommand playerBans_del = new SQLiteCommand("DELETE FROM `playerbans` WHERE `profileid` = @profileid;", db);
                playerBans_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                playerBans_del.ExecuteNonQuery();
                playerBans_del.Dispose();

                SQLiteCommand whiteList_del = new SQLiteCommand("DELETE FROM `vpnwhitelist` WHERE `profile_id` = @profileid;", db);
                whiteList_del.Parameters.AddWithValue("@profileid", _state.Instances[id].Id);
                whiteList_del.ExecuteNonQuery();
                whiteList_del.Dispose();

                db.Close();
                db.Dispose();
                _state.Instances.Remove(id);
                _state.ChatLogs.Remove(id);
                _state.PlayerStats.Remove(id);
                _state.IPQualityCache.Remove(id);
                table.Rows.Remove(instance);
                if (list_serverProfiles.Rows.Count > 0)
                {
                    list_serverProfiles.Rows[0].Selected = true;
                }
            }
            else if (result == DialogResult.No) return;
        }
        public void SetupTable()
        {
            //set autosize mode
            list_serverProfiles.Columns["ID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["ID"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Game Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Game Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Mod"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Mod"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Slots"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Slots"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Map"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Map"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Game Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Game Type"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Time Remaining"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Time Remaining"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Web Stats Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Web Stats Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Server Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Server Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //set all rows to middlecenter
            list_serverProfiles.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            // prevent header selection

        }
        private void btnEdit_Click(object sender, EventArgs e)
        {

            // Are there any Profiles to edit?
            if ((list_serverProfiles.Rows.Count) == 0)
            {
                MessageBox.Show("You must create a profile first!");
                return;
            }

            int id = list_serverProfiles.CurrentRow.Index;
            string img;
            string statusIMG;
            Edit_Profile frm1 = new Edit_Profile(_state, id);
            frm1.ShowDialog();
            bool updated = Edit_Profile.profileUpdated;
            if (updated == true)
            {
                statusIMG = "notactive.gif";
                if (_state.Instances[id].GameType == 0 && _state.Instances[id].IsTeamSabre == true)
                {
                    img = "bhdts.gif";
                }
                else if (_state.Instances[id].GameType == 0 && _state.Instances[id].IsTeamSabre == false)
                {
                    img = "bhd.gif";
                }
                else if (_state.Instances[id].GameType == 1)
                {
                    img = "jo.gif";
                }
                else
                {
                    img = "bhd.gif";
                }
                DataRow editRow = table.Rows[id];
                editRow["Game Name"] = _state.Instances[id].GameName;
                editRow["Mod"] = imageToByteArray(img);
                editRow["Server Status"] = imageToByteArray(statusIMG);
                MessageBox.Show("Profile edited successfully!", "Success");
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Are there any Profiles to start?
            if ((list_serverProfiles.Rows.Count) == 0)
            {
                MessageBox.Show("You must create a profile first!");
                return;
            }

            Instance instance = _state.Instances[list_serverProfiles.SelectedRows[0].Index];
            if (instance.Status != InstanceStatus.OFFLINE)
            {
                // Run stop server stuff
                if (!instance.PID.HasValue)
                {
                    return;
                }

                var p = Process.GetProcessById((int)instance.PID);
                p.Kill();
                p.Dispose();
                UpdateButtonsOffline();
                return;
            }

            int InstanceID = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);

            Start_Game start_Game = new Start_Game(InstanceID, _state);
            start_Game.ShowDialog();
        }
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "Error";
        }

        private void CheckMapRotation()
        {
            if (list_serverProfiles.Rows.Count > 0)
            {
                btn_rotationManager.Enabled = true;
            }
            else
            {
                btn_rotationManager.Enabled = false;
            }
        }

        private void UpdateButtonsOnline()
        {
            btn_start.Text = "Stop Game";
            btn_edit.Enabled = false;
            btn_delete.Enabled = false;
            btn_serverManager.Enabled = true;
            CheckMapRotation();
        }

        private void UpdateButtonsOffline()
        {
            btn_start.Text = "Start Game";
            btn_edit.Enabled = true;
            btn_delete.Enabled = true;
            btn_serverManager.Enabled = false;
            CheckMapRotation();
        }
        public int GetTimeLeft(int InstanceID)
        {
            if (_state.Instances[InstanceID].Status != InstanceStatus.STARTDELAY || _state.Instances[InstanceID].Status != InstanceStatus.ONLINE)
            {
                var baseAddr = 0x400000;
                byte[] Ptr = new byte[4];
                int ReadPtr = 0;
                ReadProcessMemory((int)_state.Instances[InstanceID].Handle, (int)baseAddr + 0x00061098, Ptr, Ptr.Length, ref ReadPtr);
                int MapTimeAddr = BitConverter.ToInt32(Ptr, 0);
                Stopwatch stopwatchProcessingTime = new Stopwatch();
                stopwatchProcessingTime.Start();
                // DO NOT FUCKING TOUCH -- READ CURRENT MAP TIME
                byte[] MapTimeMs = new byte[4];
                int MapTimeRead = 0;
                ReadProcessMemory((int)_state.Instances[InstanceID].Handle, MapTimeAddr, MapTimeMs, MapTimeMs.Length, ref MapTimeRead);
                int MapTime = BitConverter.ToInt32(MapTimeMs, 0); // get amount of seconds or minutes... >.<
                int MapTimeInSeconds = MapTime / 60;
                // DO NOT FUCKING TOUCH -- READ CURRENT MAP TIME
                DateTime MapStartTime = DateTime.Now - TimeSpan.FromSeconds(MapTimeInSeconds);
                DateTime MapEndTime = MapStartTime + TimeSpan.FromMinutes(_state.Instances[InstanceID].StartDelay + _state.Instances[InstanceID].TimeLimit);
                byte[] TimeOffset = new byte[4];
                int TimeOffsetRead = 0;
                ReadProcessMemory((int)_state.Instances[InstanceID].Handle, MapTimeAddr, TimeOffset, TimeOffset.Length, ref TimeOffsetRead);
                int intTimeOffset = BitConverter.ToInt32(TimeOffset, 0);
                TimeSpan TimeRemaining = MapEndTime - (DateTime.Now + TimeSpan.FromMilliseconds(stopwatchProcessingTime.ElapsedMilliseconds) - TimeSpan.FromMilliseconds(intTimeOffset));
                stopwatchProcessingTime.Stop();
                if (TimeRemaining.Minutes <= 0)
                {
                    return 0;
                }
                return TimeRemaining.Minutes;
            }
            else if (_state.Instances[InstanceID].Status == InstanceStatus.LOADINGMAP)
            {
                return (_state.Instances[InstanceID].StartDelay + _state.Instances[InstanceID].TimeLimit);
            }
            else
            {
                return 0;
            }
        }
        public InternalPlayerStats GetPlayerStats(int instanceid, int reqslot)
        {
            // start   : base + 0x000362B0
            // add     : READ start + 0x40
            // addition: 0x000ADB40

            var baseaddr = 0x400000;
            var startList = baseaddr + 0x005ED600;

            byte[] startaddr = new byte[4];
            int startaddr_read = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)startList, startaddr, startaddr.Length, ref startaddr_read);

            var firstplayer = BitConverter.ToInt32(startaddr, 0) + 0x28;

            byte[] scanbeginaddr = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)firstplayer, scanbeginaddr, scanbeginaddr.Length, ref startaddr_read);
            int beginaddr = BitConverter.ToInt32(scanbeginaddr, 0);

            if (reqslot != 1)
            {
                for (int i = 1; i < reqslot; i++)
                {
                    beginaddr += 0xAF33C;
                }
            }

            byte[] read_name = new byte[15];
            int bytesread = 0;
            byte[] read_slot = new byte[4];

            // get player slot used for primary index
            /*ReadProcessMemory((int)processHandle, (int)beginaddr + 0xC, read_slot, read_slot.Length, ref bytesread);
            int slot = BitConverter.ToInt32(read_slot, 0);*/

            // get player name
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x1C, read_name, read_name.Length, ref bytesread);
            var PlayerName = Encoding.Default.GetString(read_name).Replace("\0", "");

            int fault = 0;
            if (PlayerName == "")
            {
                beginaddr += 0xAF33C;
                fault++;
            }

            if (fault == 3)
            {
                // sanity check for 3 to prevent unlimited loops
                log.Debug("Something went wrong here. We can't find any player names.");
                return new InternalPlayerStats();
            }

            byte[] read_ping = new byte[4];
            // get player ping to server
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADB40, read_ping, read_ping.Length, ref bytesread);

            byte[] read_totalshotsfired = new byte[4];
            // total shots fired
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAB4, read_totalshotsfired, read_totalshotsfired.Length, ref bytesread);
            int TotalShotsFired = BitConverter.ToInt32(read_totalshotsfired, 0);

            byte[] read_totalkills = new byte[4];
            // kills
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADA94, read_totalkills, read_totalkills.Length, ref bytesread);
            int TotalKills = BitConverter.ToInt32(read_totalkills, 0);

            byte[] read_totaldeaths = new byte[4];
            // deaths
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADA98, read_totaldeaths, read_totaldeaths.Length, ref bytesread);
            int TotalDeaths = BitConverter.ToInt32(read_totaldeaths, 0);

            byte[] read_totalsuicdes = new byte[4];
            // suicides // 0x00036A77C
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADA8C, read_totalsuicdes, read_totalsuicdes.Length, ref bytesread);
            int TotalSuicides = BitConverter.ToInt32(read_totalsuicdes, 0);

            byte[] read_totalheadshots = new byte[4];
            // headshots // 0x000ADAD0
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAD0, read_totalheadshots, read_totalheadshots.Length, ref bytesread);
            int TotalHeadShots = BitConverter.ToInt32(read_totalheadshots, 0);

            byte[] read_teamkills = new byte[4];
            // teamkills // 0x000ADA90
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADA90, read_teamkills, read_teamkills.Length, ref bytesread);
            int TotalTeamKills = BitConverter.ToInt32(read_teamkills, 0);

            byte[] read_knifekills = new byte[4];
            // knifekills // 0x000ADAD4
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAD4, read_knifekills, read_knifekills.Length, ref bytesread);
            int TotalKnifeKills = BitConverter.ToInt32(read_knifekills, 0);

            byte[] read_exppoints = new byte[4];
            // exp points // 0x000ADAF4
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAF4, read_exppoints, read_exppoints.Length, ref bytesread);
            int TotalExpPoints = BitConverter.ToInt32(read_exppoints, 0);

            byte[] read_revives = new byte[4];
            // revives // 0x000ADABC
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADABC, read_revives, read_revives.Length, ref bytesread);
            int TotalRevives = BitConverter.ToInt32(read_revives, 0);

            byte[] read_pspattempt = new byte[4];
            // pspattempts // 0x000ADAC0
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAC0, read_pspattempt, read_pspattempt.Length, ref bytesread);
            int TotalPSPAttempts = BitConverter.ToInt32(read_pspattempt, 0);

            byte[] read_psptakeover = new byte[4];
            // psptakeover // 0x000ADAC4
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAC4, read_psptakeover, read_psptakeover.Length, ref bytesread);
            int TotalPSPTakeover = BitConverter.ToInt32(read_psptakeover, 0);

            byte[] read_doublekills = new byte[4];
            // double kills // 0x000ADACC
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADACC, read_doublekills, read_doublekills.Length, ref bytesread);
            int TotalDoubleKills = BitConverter.ToInt32(read_doublekills, 0);

            byte[] read_revivedplayer = new byte[4];
            // medsaves / player revives // 0x000ADACC
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADACC, read_revivedplayer, read_revivedplayer.Length, ref bytesread);
            int TotalRevivedPlayer = BitConverter.ToInt32(read_revivedplayer, 0);

            byte[] read_FBCaptures = new byte[4];
            // FlagBall Captures // 0x000ADAA8
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAA8, read_FBCaptures, read_FBCaptures.Length, ref bytesread);
            int TotalFBCaptures = BitConverter.ToInt32(read_FBCaptures, 0);

            byte[] read_FBCarrierKills = new byte[4];
            // FlagBall Carrier Kills // 0x000ADAC8
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAC8, read_FBCarrierKills, read_FBCarrierKills.Length, ref bytesread);
            int TotalFBCarrierKills = BitConverter.ToInt32(read_FBCarrierKills, 0);

            byte[] read_FBCarrierDeaths = new byte[4];
            // FlagBall Carrier Kills // 0x000ADAD4
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAD4, read_FBCarrierDeaths, read_FBCarrierDeaths.Length, ref bytesread);
            int TotalFBCarrierDeaths = BitConverter.ToInt32(read_FBCarrierDeaths, 0);

            byte[] read_ZoneTime = new byte[4];
            // Zone Time // 0x000ADAA4
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAA4, read_ZoneTime, read_ZoneTime.Length, ref bytesread);
            int TotalZoneTime = BitConverter.ToInt32(read_ZoneTime, 0);

            byte[] read_ZoneKills = new byte[4];
            // Zone Kills // 0x000ADADC
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADADC, read_ZoneKills, read_ZoneKills.Length, ref bytesread);
            int TotalZoneKills = BitConverter.ToInt32(read_ZoneKills, 0);

            byte[] read_ZoneDefendKills = new byte[4];
            // Zone Defend Kills // 0x000ADA94
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADA94, read_ZoneDefendKills, read_ZoneDefendKills.Length, ref bytesread);
            int TotalZoneDefendKills = BitConverter.ToInt32(read_ZoneDefendKills, 0);

            byte[] read_ADTargetsDestroyed = new byte[4];
            // AD Targets Destroyed // 0x000ADAB0
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAB0, read_ADTargetsDestroyed, read_ADTargetsDestroyed.Length, ref bytesread);
            int TotalTargetsDestroyed = BitConverter.ToInt32(read_ADTargetsDestroyed, 0);

            byte[] read_CTFFlagSaves = new byte[4];
            // CTF Flag Saves // 0x000ADAAC
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADAAC, read_CTFFlagSaves, read_CTFFlagSaves.Length, ref bytesread);
            int TotalFlagSaves = BitConverter.ToInt32(read_CTFFlagSaves, 0);

            // specific player data
            byte[] read_playerObjectLocation = new byte[4];
            int read_selectedWeaponLocationReadBytes = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x5E7C, read_playerObjectLocation, read_playerObjectLocation.Length, ref read_selectedWeaponLocationReadBytes);
            int read_playerObject = BitConverter.ToInt32(read_playerObjectLocation, 0);

            int read_selectedWeaponReadBytes = 0;
            byte[] read_selectedWeapon = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)read_playerObject + 0x178, read_selectedWeapon, read_selectedWeapon.Length, ref read_selectedWeaponReadBytes);
            int SelectedWeapon = BitConverter.ToInt32(read_selectedWeapon, 0);

            int read_selectedCharacterClassReadBytes = 0;
            byte[] read_selectedCharacterClass = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)read_playerObject + 0x244, read_selectedCharacterClass, read_selectedCharacterClass.Length, ref read_selectedCharacterClassReadBytes);
            int SelectedCharacterClass = BitConverter.ToInt32(read_selectedCharacterClass, 0);

            // weapons
            byte[] read_weapons = new byte[250];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)beginaddr + 0x000ADB70, read_weapons, read_weapons.Length, ref bytesread);
            var MemoryWeapons = Encoding.Default.GetString(read_weapons).Replace("\0", "|");
            string[] weapons = MemoryWeapons.Split('|');
            List<string> WeaponList = new List<string>();

            int failureCount = 0;
            foreach (var item in weapons)
            {
                if (item != "" && failureCount != 3)
                {
                    WeaponList.Add(item);
                }
                else
                {
                    if (failureCount == 3)
                    {
                        break;
                    }
                    else
                    {
                        failureCount++;
                    }
                }
            }

            return new InternalPlayerStats
            {
                PlayerName = PlayerName,
                ping = BitConverter.ToInt32(read_ping, 0),
                CharacterClass = SelectedCharacterClass,
                SelectedWeapon = SelectedWeapon,
                PlayerWeapons = WeaponList,
                TotalShotsFired = TotalShotsFired,
                kills = TotalKills,
                deaths = TotalDeaths,
                suicides = TotalSuicides,
                headshots = TotalHeadShots,
                teamkills = TotalTeamKills,
                knifekills = TotalKnifeKills,
                exp = TotalExpPoints,
                revives = TotalRevives,
                pspattempts = TotalPSPAttempts,
                psptakeover = TotalPSPTakeover,
                doublekills = TotalDoubleKills,
                playerrevives = TotalRevivedPlayer,
                FBCaptures = TotalFBCaptures,
                FBCarrierKills = TotalFBCarrierKills,
                FBCarrierDeaths = TotalFBCarrierDeaths,
                ZoneTime = TotalZoneTime,
                ZoneKills = TotalZoneKills,
                ZoneDefendKills = TotalZoneDefendKills,
                ADTargetsDestroyed = TotalTargetsDestroyed,
                FlagSaves = TotalFlagSaves,
            };
        }

        public int GetCurrentPlayers(int instanceid)
        {
            // memory polling
            int bytesRead = 0;
            byte[] buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, 0x0065DCBC, buffer, buffer.Length, ref bytesRead);
            int CurrentPlayers = BitConverter.ToInt32(buffer, 0);
            return CurrentPlayers;
        }
        public int GetCurrentGameType(int instanceid)
        {
            // memory polling
            int bytesRead = 0;
            byte[] buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, 0x009F21A4, buffer, buffer.Length, ref bytesRead);
            int GameType = BitConverter.ToInt32(buffer, 0);

            return GameType;
        }
        public string GetCurrentMission(int instanceid)
        {
            // memory polling
            int bytesRead = 0;
            byte[] buffer = new byte[26];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, 0x0071569C, buffer, buffer.Length, ref bytesRead);
            string MissionName = Encoding.Default.GetString(buffer);

            return MissionName.Replace("\0", "");
        }
        private void Main_Profilelist_Close(object sender, FormClosingEventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("Are you sure you want to quit?", "Quit Babstats", buttons);
            if (result == DialogResult.Yes)
            {
                e.Cancel = false;
                Ticker.Stop(); // since the program is exiting stop updating everything...
                Ticker.Dispose(); // since the program is exiting stop updating everything...
                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        public static int rowId;

        // Somewhere else I would call the timer and tell it to call the whatever method every second

        public void SetStatusImage(int key)
        {
            DataRow row = table.Rows[key];

            var path = "";
            switch (_state.Instances[key].Status)
            {
                case InstanceStatus.ONLINE:
                    path = "hosting.gif";
                    break;
                case InstanceStatus.STARTDELAY:
                    path = "hosting.gif";
                    break;
                case InstanceStatus.LOADINGMAP:
                    path = "loading.gif";
                    break;
                case InstanceStatus.SCORING:
                    path = "scoring.gif";
                    break;
                default:
                    path = "notactive.gif";
                    break;
            }
            row["Server Status"] = new byte[0];
            row["Server Status"] = imageToByteArray(path);
        }

        public string GetPlayerIpAddress(string playername, int instanceid)
        {
            var baseAddr = 0x400000;
            var playerIpAddressPointer = baseAddr + 0x00ACE248; //secondary list contains memory addresses to PlayerIPs
            int playerIpAddressPointerBuffer = 0;
            byte[] PointerAddr_2 = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)playerIpAddressPointer, PointerAddr_2, PointerAddr_2.Length, ref playerIpAddressPointerBuffer);


            int IPList = BitConverter.ToInt32(PointerAddr_2, 0) + 0xBC; // playername and start of list

            int failureCounter = 0;
            while (true)
            {
                if (failureCounter > _state.Instances[instanceid].MaxSlots)
                {
                    return null;
                }

                // Check if the username equals what is passed in
                byte[] playername_bytes = new byte[15];
                int playername_buffer = 0;
                ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)IPList, playername_bytes, playername_bytes.Length, ref playername_buffer);
                var currentPlayerName = Encoding.Default.GetString(playername_bytes).Replace("\0", "");

                if (currentPlayerName != playername)
                {
                    IPList += 0xBC;
                    failureCounter += 1;
                    continue;
                }
                else if (currentPlayerName == playername)
                {
                    failureCounter = 0;
                    break;
                }
            }
            // Read the IP address and return it
            byte[] playerIPBytesPtr = new byte[4];
            int playerIPBufferPtr = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)IPList + 0xA4, playerIPBytesPtr, playerIPBytesPtr.Length, ref playerIPBufferPtr);

            int PlayerIPLocation = BitConverter.ToInt32(playerIPBytesPtr, 0) + 4;
            byte[] playerIPAddressBytes = new byte[4];
            int playerIPAddressBuffer = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)PlayerIPLocation, playerIPAddressBytes, playerIPAddressBytes.Length, ref playerIPAddressBuffer);
            IPAddress playerIp = new IPAddress(playerIPAddressBytes);
            return playerIp.ToString();
        }

        public Dictionary<int, playerlist> CurrentPlayerList(int instanceid)
        {
            Dictionary<int, playerlist> DicList = new Dictionary<int, playerlist>();
            int NumPlayers = GetCurrentPlayers(instanceid);
            //Dictionary<int, string> IPList = GetIPList(pid, NumPlayers);
            // memory polling
            if (NumPlayers > 0)
            {
                int buffer = 0;
                byte[] PointerAddr9 = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                // read the playerlist memory address from the game...
                int slot = 1;
                ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                byte[] playerListStartingLocationByteArray = new byte[4];
                int playerListStartingLocationBuffer = 0;
                ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);

                int failureCount = 0;

                for (int i = 0; i < NumPlayers; i++)
                {
                    if (failureCount == _state.Instances[instanceid].MaxSlots)
                    {
                        break;
                    }

                    // Slot number
                    byte[] slotNumberValue = new byte[4];
                    int slotNumberBuffer = 0;
                    int slotNumberLocation = (int)playerlistStartingLocation + 0xC;
                    ReadProcessMemory((int)_state.Instances[instanceid].Handle, slotNumberLocation, slotNumberValue, slotNumberValue.Length, ref slotNumberBuffer);
                    int playerSlot = BitConverter.ToInt32(slotNumberValue, 0);

                    // Fetching player name
                    byte[] playerNameBytes = new byte[15];
                    int playerNameBuffer = 0;
                    int playerNameLocation = (int)playerlistStartingLocation + 0x1C;
                    ReadProcessMemory((int)_state.Instances[instanceid].Handle, playerNameLocation, playerNameBytes, playerNameBytes.Length, ref playerNameBuffer);
                    string formattedPlayerName = Encoding.Default.GetString(playerNameBytes).Replace("\0", "");

                    if (string.IsNullOrEmpty(formattedPlayerName) || string.IsNullOrWhiteSpace(formattedPlayerName))
                    {
                        playerlistStartingLocation += 0xAF33C;
                        i--;
                        failureCount++;
                        continue;
                    }

                    // Fetching player team
                    byte[] playerTeamBytes = new byte[4];
                    int playerTeamBuffer = 0;
                    int playerTeamLocation = (int)playerlistStartingLocation + 0x90;
                    ReadProcessMemory((int)_state.Instances[instanceid].Handle, playerTeamLocation, playerTeamBytes, playerTeamBytes.Length, ref playerTeamBuffer);
                    int playerTeam = BitConverter.ToInt32(playerTeamBytes, 0);

                    string playerIP = GetPlayerIpAddress(formattedPlayerName, instanceid).ToString();
                    InternalPlayerStats PlayerStats = GetPlayerStats(instanceid, playerSlot);

                    playerlist.CharacterClass PlayerCharacterClass = (playerlist.CharacterClass)PlayerStats.CharacterClass;
                    playerlist.WeaponStack PlayerSelectedWeapon = (playerlist.WeaponStack)PlayerStats.SelectedWeapon;

                    // Build player weapon information
                    Dictionary<int, List<playerlist.WeaponStack>> PlayerWeapons = new Dictionary<int, List<playerlist.WeaponStack>>();

                    if (string.IsNullOrEmpty(formattedPlayerName) || string.IsNullOrWhiteSpace(formattedPlayerName))
                    {
                        if (DicList.Count >= NumPlayers)
                        {
                            break;
                        }
                        else
                        {
                            slot++;
                            playerlistStartingLocation += 0xAF33C;
                            continue;
                        }
                    }
                    else
                    {
                        try
                        {
                            DicList.Add(playerSlot, new playerlist
                            {
                                slot = playerSlot,
                                name = formattedPlayerName,
                                nameBase64 = Crypt.Base64Encode(formattedPlayerName),
                                team = playerTeam,
                                address = playerIP,
                                ping = PlayerStats.ping,
                                PlayerClass = PlayerCharacterClass.ToString(),
                                selectedWeapon = PlayerSelectedWeapon.ToString(),
                                weapons = PlayerStats.PlayerWeapons,
                                totalshots = PlayerStats.TotalShotsFired,
                                kills = PlayerStats.kills,
                                deaths = PlayerStats.deaths,
                                suicides = PlayerStats.suicides,
                                headshots = PlayerStats.headshots,
                                teamkills = PlayerStats.teamkills,
                                knifekills = PlayerStats.knifekills,
                                exp = PlayerStats.exp,
                                revives = PlayerStats.revives,
                                playerrevives = PlayerStats.playerrevives,
                                pspattempts = PlayerStats.pspattempts,
                                psptakeover = PlayerStats.psptakeover,
                                doublekills = PlayerStats.doublekills,
                                flagcaptures = PlayerStats.FBCaptures,
                                flagcarrierkills = PlayerStats.FBCarrierKills,
                                flagcarrierdeaths = PlayerStats.FBCarrierDeaths,
                                zonetime = PlayerStats.ZoneTime,
                                zonekills = PlayerStats.ZoneKills,
                                zonedefendkills = PlayerStats.ZoneDefendKills,
                                ADTargetsDestroyed = PlayerStats.ADTargetsDestroyed,
                                FlagSaves = PlayerStats.FlagSaves
                            });

                            playerlistStartingLocation += 0xAF33C;

                            if (slot == NumPlayers)
                            {
                                break; // we've reached the end of the list
                            }
                            slot++;
                            continue;
                        }
                        catch (Exception e)
                        {
                            _state.eventLog.WriteEntry("Detected an error!\n\n" + "Player Name: " + playerSlot + "\n\n" + formattedPlayerName + "\n\n" + e.ToString(), EventLogEntryType.Error);
                        }

                    }
                }
            }
            return DicList;
        }

        public bool ProcessExist(int id)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.Id == id)
                {
                    return true;
                }
            }
            return false;

            /*if (!Process.GetProcesses().Any(x => x.Id == id))
            {
                return false;
            }
            try
            {
                Process process = Process.GetProcessById(id);
                if (process.ProcessName != "dfbhd")
                {
                    process.Dispose();
                    return false;
                }
                process.Dispose();
                return true;
            }
            catch
            {
                return false;
            }*/
        }
        public string IPQualityCheck(string ipaddress)
        {
            string URL = $"https://ipqualityscore.com/api/json/ip/{ProgramConfig.ip_quality_score_apikey}/{ipaddress}?strictness=0&allow_public_access_points=true&fast=true&lighter_penalties=true&mobile=true";
            WebRequest request = WebRequest.Create(URL);
            request.Timeout = 7200;
            WebResponse reply = request.GetResponse();
            StreamReader replyTxt = new StreamReader(reply.GetResponseStream());
            string Response = replyTxt.ReadToEnd();
            reply.Close();
            reply.Dispose();
            replyTxt.Dispose();
            return Response;
        }
        public void UpdateTick()
        {
            try
            {
                foreach (var item in _state.Instances)
                {
                    Instance instance = item.Value;
                    int rowId = item.Key;

                    if (table.Rows.Count == 0)
                    {
                        return;
                    }

                    DataRow row = table.Rows[rowId];
                    DateTime currentTime = DateTime.Now;
                    if (DateTime.Compare(instance.NextUpdateTime, currentTime) < 0)
                    {
                        if (instance.PID != null)
                        {
                            try
                            {
                                var PID = instance.PID.GetValueOrDefault();
                                if (ProcessExist(instance.PID.GetValueOrDefault()))
                                {
                                    // set process name incase the PID changes...
                                    SetAppID(rowId);
                                    SetWindowText(_state.ApplicationProcesses[rowId].MainWindowHandle, $"{instance.GameName}");

                                    int timeRemainingInGame = GetTimeLeft(rowId);
                                    var map = GetCurrentMission(rowId);
                                    var currentPlayers = GetCurrentPlayers(rowId);
                                    var currentGameType = "";
                                    foreach (var gameTypeList in bitmapsAndGameTypes)
                                    {
                                        var gametype = gameTypeList.Value;
                                        if (GetCurrentGameType(rowId).Equals(gametype.DatabaseId))
                                        {
                                            currentGameType = gametype.ShortName;
                                            break;
                                        }
                                    }

                                    instance.Status = CheckStatus(rowId);
                                    UpdateGlobalGameType(rowId);
                                    // prevents loading garbage data while switching maps,
                                    // should also prevent ghosts from showing on the playerlist.
                                    if (instance.Status != InstanceStatus.LOADINGMAP)
                                    {
                                        try
                                        {
                                            instance.PlayerList = CurrentPlayerList(rowId);
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                        CheckBans(rowId, PID);
                                    }

                                    if (instance.EnableWebStats == true && instance.Status == InstanceStatus.ONLINE && instance.collectPlayerStats == true)
                                    {
                                        // collect player stats for submission
                                        CollectPlayerStats(rowId);
                                    }

                                    // important for clearing stupid map cycle shit
                                    instance.mapCounter = UpdateMapCycleCounter(rowId);
                                    //UpdateMapCycleGarbage(rowId);

                                    // check for VPNs
                                    if (ProgramConfig.Enable_VPNWhiteList == true)
                                    {
                                        Check4VPN(rowId);
                                    }

                                    // get chatLogs...
                                    //GetChatLogs(rowId, PID);


                                    if (instance.Status != InstanceStatus.LOADINGMAP && instance.Status != InstanceStatus.SCORING)
                                    {
                                        ProcessPlayerWarnings(rowId);
                                    }

                                    if (instance.Status == InstanceStatus.LOADINGMAP)
                                    {
                                        if (instance.IsRunningPostGameProcesses == false)
                                        {
                                            instance.IsRunningPostGameProcesses = true;
                                            if (ProgramConfig.Debug)
                                            {
                                                log.Info("Instance " + rowId + " is running the Loading Process Handler.");
                                            }
                                            LoadingProcessHandler handler = new LoadingProcessHandler(_state, instance.DataTableColumnId, _state.ChatLogs[rowId], _state.PlayerStats[item.Key]);
                                            handler.Run();

                                            _state.ChatLogs[rowId].Messages.Clear();
                                            _state.ChatLogs[rowId].CurrentIndex = 0;
                                        }
                                        instance.collectPlayerStats = true;
                                        instance.IsRunningScoringGameProcesses = false;
                                        CheckForCrashedGame(rowId);
                                    }

                                    if (instance.Status == InstanceStatus.SCORING)
                                    {
                                        if (instance.IsRunningScoringGameProcesses == false)
                                        {
                                            if (ProgramConfig.Debug)
                                            {
                                                log.Info("Instance " + rowId + " is running the Scoring Process Handler.");
                                            }
                                            ScoringProcessHandler scoringHandler = new ScoringProcessHandler(_state, rowId, _state.ChatLogs[rowId], _state.PlayerStats[item.Key]);
                                            scoringHandler.Run();
                                            instance.IsRunningScoringGameProcesses = true;
                                        }
                                    }

                                    if (instance.Status == InstanceStatus.LOADINGMAP || instance.Status == InstanceStatus.SCORING)
                                    {
                                        ChangePlayersTeam(rowId); // change player slot's team
                                    }

                                    if (instance.Status == InstanceStatus.STARTDELAY || instance.Status == InstanceStatus.ONLINE)
                                    {
                                        instance.gameCrashCounter = 0;
                                        ResetGodMode(rowId);
                                        ProcessDisarmedPlayers(rowId);
                                        ProcessAutoMessage(rowId);
                                        ChangeGameScore(rowId);
                                        GetCurrentMapIndex(rowId);
                                        if (instance.IsRunningPostGameProcesses == true)
                                        {
                                            instance.IsRunningPostGameProcesses = false;
                                        }
                                        if (instance.IsRunningScoringGameProcesses == true)
                                        {
                                            instance.IsRunningScoringGameProcesses = false;
                                        }
                                    }

                                    if (instance.Status != InstanceStatus.OFFLINE)
                                    {
                                        instance.CurrentMap = GetCurrentMap(rowId, currentGameType); // always get the current map so long as the instance is not offline.
                                    }

                                    switch (timeRemainingInGame)
                                    {
                                        case 0:
                                            row["Time Remaining"] = "< 1 Minute";
                                            instance.TimeFlag = true;
                                            break;
                                        case 1:
                                            row["Time Remaining"] = timeRemainingInGame + " Minute";
                                            instance.TimeFlag = false;
                                            break;
                                        default:
                                            row["Time Remaining"] = timeRemainingInGame + " Minutes";
                                            instance.TimeFlag = false;
                                            break;
                                    }
                                    row["Map"] = map;
                                    row["Slots"] = currentPlayers + "/" + instance.MaxSlots;
                                    row["Game Type"] = currentGameType;
                                    instance.gameMapType = GetCurrentGameType(rowId);
                                    instance.NextUpdateTime = currentTime.AddMilliseconds(100);
                                    instance.LastUpdateTime = currentTime;
                                    instance.Map = map;
                                    instance.TimeRemaining = timeRemainingInGame;
                                    instance.NumPlayers = currentPlayers;
                                    instance.GameTypeName = currentGameType;

                                    SetStatusImage(rowId);

                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        UpdateButtonsOnline();
                                    }
                                }
                                else
                                {
                                    row["Time Remaining"] = "";
                                    row["Map"] = "";
                                    row["Slots"] = "";
                                    row["Game Type"] = "";
                                    instance.Status = InstanceStatus.OFFLINE;
                                    instance.TimeFlag = false;
                                    SetStatusImage(rowId);
                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        UpdateButtonsOffline();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (ProgramConfig.Debug)
                                {
                                    _state.eventLog.WriteEntry("BMTv4 TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                                    log.Error("An error occurred: " + e);
                                }
                                if (ProcessExist((int)instance.PID) == false)
                                {
                                    row["Time Remaining"] = "";
                                    row["Map"] = "";
                                    row["Slots"] = "";
                                    row["Game Type"] = "";
                                    instance.Status = InstanceStatus.OFFLINE;
                                    instance.TimeFlag = false;
                                    SetStatusImage(rowId);
                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        UpdateButtonsOffline();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _state.eventLog.WriteEntry("BMTv4 TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                log.Debug(e);
                return;
            }
        }

        private MapList GetCurrentMap(int rowId, string currentGameType)
        {
            MapList map = new MapList();
            int currentIndex = 0;
            if (_state.Instances[rowId].mapIndex == _state.Instances[rowId].MapList.Count)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = _state.Instances[rowId].mapIndex;
            }
            if (!_state.Instances[rowId].MapList.ContainsKey(currentIndex))
            {
                if (!_state.Instances[rowId].previousMapList.ContainsKey(currentIndex))
                {
                    throw new Exception("Something went wrong while trying to retrieve maplists. #41");
                }
                else
                {
                    map.MapName = _state.Instances[rowId].previousMapList[currentIndex].MapName;
                    map.MapFile = _state.Instances[rowId].previousMapList[currentIndex].MapFile;
                }
            }
            else
            {
                map.MapName = _state.Instances[rowId].MapList[currentIndex].MapName;
                map.MapFile = _state.Instances[rowId].MapList[currentIndex].MapFile;
            }
            return map;
        }

        private void GetCurrentMapIndex(int ArrayID)
        {
            int appEntry = 0x400000;
            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)_state.Instances[ArrayID].Handle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int MapCycleIndex = BitConverter.ToInt32(ServerMapCyclePtr, 0) + 0xC;
            byte[] mapIndexBytes = new byte[4];
            int mapIndexRead = 0;
            ReadProcessMemory((int)_state.Instances[ArrayID].Handle, MapCycleIndex, mapIndexBytes, mapIndexBytes.Length, ref mapIndexRead);
            _state.Instances[ArrayID].mapIndex = BitConverter.ToInt32(mapIndexBytes, 0);
            return;
        }

        private void CheckForCrashedGame(int ArrayID)
        {
            if (_state.Instances[ArrayID].CrashRecovery == true)
            {
                if (DateTime.Compare(_state.Instances[ArrayID].gameCrashCheck, DateTime.Now) < 0)
                {
                    if (_state.Instances[ArrayID].gameCrashCounter < 3)
                    {
                        _state.Instances[ArrayID].gameCrashCounter++;
                        _state.Instances[ArrayID].gameCrashCheck = DateTime.Now.AddSeconds(10.0);
                        return;
                    }
                    else if (_state.Instances[ArrayID].gameCrashCounter == 3)
                    {
                        Process[] processes = Process.GetProcesses();

                        foreach (Process prs in processes)
                        {
                            if (prs.Id == _state.Instances[ArrayID].PID.GetValueOrDefault())
                            {
                                prs.Kill();
                                break;
                            }
                        }

                        string file_name = "";
                        SQLiteConnection _connection = new SQLiteConnection(ProgramConfig.DBConfig);
                        _connection.Open();
                        SQLiteCommand command = new SQLiteCommand("select `game_type` from instances WHERE id = @id;", _connection);
                        command.Parameters.AddWithValue("@id", _state.Instances[ArrayID].Id);
                        switch (Convert.ToInt32(command.ExecuteScalar()))
                        {
                            case 0:
                                file_name = "dfbhd.exe";
                                break;
                            case 1:
                                file_name = "jops.exe";
                                break;
                        }
                        command.Dispose();
                        Process process;
                        if (_state.Instances[ArrayID].BindAddress == "0.0.0.0")
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(_state.Instances[ArrayID].GamePath, file_name),
                                WorkingDirectory = _state.Instances[ArrayID].GamePath,
                                Arguments = "/w /LOADBAR /NOSYSDUMP /serveonly /autorestart",
                                WindowStyle = ProcessWindowStyle.Minimized
                            };
                            process = Process.Start(startInfo);
                        }
                        else
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(Environment.CurrentDirectory, "bind.exe"),
                                WorkingDirectory = _state.Instances[ArrayID].GamePath,
                                Arguments = $"{_state.Instances[ArrayID].BindAddress} \"{Path.Combine(_state.Instances[ArrayID].GamePath, file_name)}\" /w /LOADBAR /NOSYSDUMP /serveonly /autorestart",
                                WindowStyle = ProcessWindowStyle.Minimized
                            };
                            process = Process.Start(startInfo);
                            process.WaitForExit();
                        }



                        //
                        //- /w /LOADBAR /NOSYSDUMP /serveonly /autorestart
                        List<int> currentPIDs = new List<int>();
                        foreach (var instance in _state.Instances)
                        {
                            if (instance.Value.PID != 0)
                            {
                                currentPIDs.Add(instance.Value.PID.GetValueOrDefault());
                            }
                        }
                        Process[] bhdprocesses = Process.GetProcessesByName("dfbhd");
                        foreach (var activeProcess in bhdprocesses)
                        {
                            if (!currentPIDs.Contains(activeProcess.Id) && activeProcess.StartTime > DateTime.Now.AddMinutes(-1))
                            {
                                activeProcess.MaxWorkingSet = new IntPtr(0x7fffffff);
                                _state.Instances[ArrayID].PID = activeProcess.Id;
                                _state.ApplicationProcesses[ArrayID] = activeProcess;
                            }
                        }

                        SetWindowText(_state.ApplicationProcesses[ArrayID].Handle, _state.Instances[ArrayID].GameName);
                        var baseAddr = 0x400000;
                        IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _state.Instances[ArrayID].PID.GetValueOrDefault());
                        _state.Instances[ArrayID].Handle = processHandle;

                        SQLiteCommand updatePid = new SQLiteCommand("UPDATE `instances_pid` SET `pid` = @pid WHERE `profile_id` = @profileid;", _connection);
                        updatePid.Parameters.AddWithValue("@pid", _state.Instances[ArrayID].PID.GetValueOrDefault());
                        updatePid.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                        updatePid.ExecuteNonQuery();
                        updatePid.Dispose();


                        if (_state.Instances[ArrayID].HostName != "Host")
                        {
                            int buffer = 0;
                            byte[] PointerAddr = new byte[4];
                            var Pointer = baseAddr + 0x005ED600;
                            ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                            int buffer2 = 0;
                            byte[] Hostname = Encoding.Default.GetBytes(_state.Instances[ArrayID].HostName + "\0");
                            var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                            MemoryProcessor.Write(_state.Instances[ArrayID], (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
                        }

                        // wait for server to be online
                        int counter = 0;
                        while (true)
                        {
                            if (_state.Instances[ArrayID].Status == InstanceStatus.ONLINE || _state.Instances[ArrayID].Status == InstanceStatus.STARTDELAY || counter == 10)
                            {
                                break;
                            }
                            else
                            {
                                counter++;
                                Thread.Sleep(1000);
                            }
                        }

                        ServerManagerUpdateMemory serverManagerUpdateMemory = new ServerManagerUpdateMemory();
                        serverManagerUpdateMemory.UpdateAllowCustomSkins(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateDestroyBuildings(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateFatBullets(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateFlagReturnTime(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateMaxPing(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateMaxPingValue(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateMaxTeamLives(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateMinPing(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateMinPingValue(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateOneShotKills(_state, ArrayID);
                        serverManagerUpdateMemory.UpdatePSPTakeOverTime(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateRequireNovaLogin(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateRespawnTime(_state, ArrayID);
                        serverManagerUpdateMemory.UpdateWeaponRestrictions(_state, ArrayID);

                        _state.Instances[ArrayID].gameCrashCounter = 0;
                        _state.Instances[ArrayID].gameCrashCheck = DateTime.Now.AddSeconds(10.0);
                        _connection.Close();
                        _connection.Dispose();

                        _state.eventLog.WriteEntry("BMTv4 has detected a server crash!\n\nProfile Name: " + _state.Instances[ArrayID].GameName + "\nMap File: " + _state.Instances[ArrayID].MapList[_state.Instances[ArrayID].mapIndex + 1].MapFile + "\nMap Title: " + _state.Instances[ArrayID].MapList[_state.Instances[ArrayID].mapIndex + 1].MapName + "\n\nThe server has been automatically restarted.", EventLogEntryType.Warning);
                    }
                }
            }
        }
        private void ChangeGameScore(int ArrayID)
        {
            int nextGameScore = 0;
            var baseAddr = 0x400000;
            var startingPtr1 = 0;
            var startingPtr2 = 0;
            switch (_state.Instances[ArrayID].nextMapGameType)
            {
                // KOTH/TKOTH
                case 3:
                case 4:
                    startingPtr1 = baseAddr + 0x5F21B8;
                    startingPtr2 = baseAddr + 0x6344B4;
                    nextGameScore = _state.Instances[ArrayID].KOTHScore;
                    break;
                // flag ball
                case 8:
                    startingPtr1 = baseAddr + 0x5F21AC;
                    startingPtr2 = baseAddr + 0x6034B8;
                    nextGameScore = _state.Instances[ArrayID].FBScore;
                    break;
                // all other game types...
                default:
                    startingPtr1 = baseAddr + 0x5F21AC;
                    startingPtr2 = baseAddr + 0x6034B8;
                    nextGameScore = _state.Instances[ArrayID].GameScore;
                    break;
            }
            byte[] nextGameScoreBytes = BitConverter.GetBytes(nextGameScore);
            int nextGameScoreWritten1 = 0;
            int nextGameScoreWritten2 = 0;
            MemoryProcessor.Write(_state.Instances[ArrayID], startingPtr1, nextGameScoreBytes, nextGameScoreBytes.Length, ref nextGameScoreWritten1);
            MemoryProcessor.Write(_state.Instances[ArrayID], startingPtr2, nextGameScoreBytes, nextGameScoreBytes.Length, ref nextGameScoreWritten2);
        }

        private void UpdateMapCycleGarbage(int InstanceID)
        {

        }

        private int UpdateMapCycleCounter(int InstanceID)
        {
            byte[] currentMapCycleCountBytes = new byte[4];
            int currentMapCycleCountRead = 0;

            int baseAddr = 0x400000;
            int mapCycleCounterPtr = baseAddr + 0x5ED644;

            ReadProcessMemory((int)_state.Instances[InstanceID].Handle, mapCycleCounterPtr, currentMapCycleCountBytes, currentMapCycleCountBytes.Length, ref currentMapCycleCountRead);

            int currentMapCycleCount = BitConverter.ToInt32(currentMapCycleCountBytes, 0);

            return currentMapCycleCount;
        }

        private void ProcessDisarmedPlayers(int rowId)
        {
            for (int i = 0; i < _state.Instances[rowId].DisarmPlayers.Count; i++)
            {
                if (!_state.Instances[rowId].PlayerList.ContainsKey(_state.Instances[rowId].DisarmPlayers[i]))
                {
                    _state.Instances[rowId].DisarmPlayers.Remove(_state.Instances[rowId].DisarmPlayers[i]);
                    continue;
                }
                else
                {
                    int buffer = 0;
                    byte[] PointerAddr9 = new byte[4];
                    var baseAddr = 0x400000;
                    var Pointer = baseAddr + 0x005ED600;
                    ReadProcessMemory((int)_state.Instances[rowId].Handle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                    var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;

                    byte[] playerListStartingLocationByteArray = new byte[4];
                    int playerListStartingLocationBuffer = 0;
                    ReadProcessMemory((int)_state.Instances[rowId].Handle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                    int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                    for (int slot = 1; slot < _state.Instances[rowId].DisarmPlayers[i]; slot++)
                    {
                        playerlistStartingLocation += 0xAF33C;
                    }
                    byte[] disablePlayerWeapon = BitConverter.GetBytes(0);
                    int disablePlayerWeaponWrite = 0;
                    MemoryProcessor.Write(_state.Instances[rowId], playerlistStartingLocation + 0xADE08, disablePlayerWeapon, disablePlayerWeapon.Length, ref disablePlayerWeaponWrite);
                }
            }
        }

        private void SetAppID(int rowId)
        {
            if (_state.Instances[rowId].RequireNovaLogin == true)
            {
                return; // since we are requiring nova login, just return.
            }
            byte[] CurrentAppIDBytes = new byte[4];
            int currentAppIDRead = 0;
            ReadProcessMemory((int)_state.Instances[rowId].Handle, 0x009DDA44, CurrentAppIDBytes, CurrentAppIDBytes.Length, ref currentAppIDRead);
            int CurrentAppID = BitConverter.ToInt32(CurrentAppIDBytes, 0);

            if (CurrentAppID != 0)
            {
                byte[] WriteAppIDBytes = BitConverter.GetBytes((int)0);
                int WriteAppIDWritten = 0;
                MemoryProcessor.Write(_state.Instances[rowId], 0x009DDA44, WriteAppIDBytes, WriteAppIDBytes.Length, ref WriteAppIDWritten);
            }
        }

        private void ProcessPlayerWarnings(int instanceid)
        {
            if (_state.Instances[instanceid].WarningQueue.Count == 0)
            {
                return;
            }
            else
            {
                for (int i = 0; i < _state.Instances[instanceid].WarningQueue.Count; i++)
                {
                    var item = _state.Instances[instanceid].WarningQueue[i];

                    // change color to white
                    int colorbuffer_written = 0;
                    byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                    MemoryProcessor.Write(_state.Instances[instanceid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                    Thread.Sleep(100);

                    // post kick message
                    PostMessage(_state.ApplicationProcesses[instanceid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                    PostMessage(_state.ApplicationProcesses[instanceid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                    Thread.Sleep(100);
                    int bytesWritten = 0;
                    byte[] buffer;
                    string playername = _state.Instances[instanceid].PlayerList[item.slot].name;
                    buffer = Encoding.Default.GetBytes($"WARNING!!! {playername} - {item.warningMsg}\0"); // '\0' marks the end of string
                    MemoryProcessor.Write(_state.Instances[instanceid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                    Thread.Sleep(100);
                    PostMessage(_state.ApplicationProcesses[instanceid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                    PostMessage(_state.ApplicationProcesses[instanceid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                    // change color to normal
                    Thread.Sleep(100);
                    int revert_colorbuffer = 0;
                    byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                    MemoryProcessor.Write(_state.Instances[instanceid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                    _state.Instances[instanceid].WarningQueue.RemoveAt(i);
                }
            }
        }

        private void ChangePlayersTeam(int rowId)
        {
            if (_state.Instances[rowId].ChangeTeamList.Count == 0)
            {
                return;
            }
            else
            {
                int buffer = 0;
                byte[] PointerAddr9 = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                // read the playerlist memory address from the game...
                ReadProcessMemory((int)_state.Instances[rowId].Handle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                byte[] playerListStartingLocationByteArray = new byte[4];
                int playerListStartingLocationBuffer = 0;
                ReadProcessMemory((int)_state.Instances[rowId].Handle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);

                for (int ii = 0; ii < _state.Instances[rowId].ChangeTeamList.Count; ii++)
                {
                    int playerLocationOffset = 0;
                    for (int i = 1; i < _state.Instances[rowId].ChangeTeamList[ii].slotNum; i++)
                    {
                        playerLocationOffset += 0xAF33C;
                    }
                    int playerLocation = playerlistStartingLocation + playerLocationOffset;
                    int playerTeamLocation = playerLocation + 0x90;
                    byte[] teamBytes = BitConverter.GetBytes((int)_state.Instances[rowId].ChangeTeamList[ii].Team);
                    int bytesWritten = 0;
                    MemoryProcessor.Write(_state.Instances[rowId], playerTeamLocation, teamBytes, teamBytes.Length, ref bytesWritten);
                    _state.Instances[rowId].ChangeTeamList.RemoveAt(ii);
                }
            }
        }

        public void ResetGodMode(int InstanceID)
        {
            if (_state.Instances[InstanceID].GodModeList.Count == 0)
            {
                return;
            }
            for (int iSlot = 0; iSlot < _state.Instances[InstanceID].GodModeList.Count; iSlot++)
            {
                int slot = _state.Instances[InstanceID].GodModeList[iSlot];
                if (!_state.Instances[InstanceID].PlayerList.ContainsKey(slot))
                {
                    _state.Instances[InstanceID].GodModeList.Remove(slot);
                }
                else
                {
                    int buffer = 0;
                    byte[] PointerAddr9 = new byte[4];
                    var baseAddr = 0x400000;
                    var Pointer = baseAddr + 0x005ED600;

                    // read the playerlist memory address from the game...
                    ReadProcessMemory((int)_state.Instances[InstanceID].Handle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                    var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                    byte[] playerListStartingLocationByteArray = new byte[4];
                    int playerListStartingLocationBuffer = 0;
                    ReadProcessMemory((int)_state.Instances[InstanceID].Handle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                    int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                    for (int i = 1; i < slot; i++)
                    {
                        playerlistStartingLocation += 0xAF33C;
                    }
                    byte[] playerObjectLocationBytes = new byte[4];
                    int playerObjectLocationRead = 0;
                    ReadProcessMemory((int)_state.Instances[InstanceID].Handle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                    int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                    byte[] setPlayerHealth = BitConverter.GetBytes(9999); //set god mode health
                    int setPlayerHealthWrite = 0;

                    byte[] setDamageBy = BitConverter.GetBytes(0);
                    int setDamageByWrite = 0;

                    MemoryProcessor.Write(_state.Instances[InstanceID], playerObjectLocation + 0x138, setDamageBy, setDamageBy.Length, ref setDamageByWrite);
                    MemoryProcessor.Write(_state.Instances[InstanceID], playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                }
            }
        }

        public void GetChatLogs(int profileid)
        {
            //0x80
            if (_state.Instances[profileid].Status == InstanceStatus.OFFLINE || _state.Instances[profileid].Status == InstanceStatus.LOADINGMAP)
            {
                return;
            }
            var baseAddr = 0x400000;

            var starterPtr = baseAddr + 0x00062D10;
            byte[] ChatLogPtr = new byte[4];
            int ChatLogPtrRead = 0;
            ReadProcessMemory((int)_state.Instances[profileid].Handle, (int)starterPtr, ChatLogPtr, ChatLogPtr.Length, ref ChatLogPtrRead);

            // get last message sent...
            int ChatLogAddr = BitConverter.ToInt32(ChatLogPtr, 0);
            byte[] Message = new byte[74];
            int MessageRead = 0;
            ReadProcessMemory((int)_state.Instances[profileid].Handle, ChatLogAddr, Message, Message.Length, ref MessageRead);
            string LastMessage = Encoding.Default.GetString(Message).Replace("\0", "");

            int msgTypeAddr = ChatLogAddr + 0x78;
            byte[] msgType = new byte[4];
            int msgTypeRead = 0;
            ReadProcessMemory((int)_state.Instances[profileid].Handle, msgTypeAddr, msgType, msgType.Length, ref msgTypeRead);
            string msgTypeBytes = BitConverter.ToString(msgType).Replace("-", "");

            // Check for valid next chat message
            if (LastMessage == "")
            {
                return;
            }
            var chatLog = _state.ChatLogs[profileid];
            var nextIndex = chatLog.CurrentIndex + 1;
            string PlayerName = string.Empty;
            string PlayerMessage = string.Empty;
            int msgStart = LastMessage.IndexOf(':');

            foreach (var pop in LastMessage)
            {
                if (pop == ':')
                {
                    break;
                }
                else
                {
                    PlayerName += pop;
                }
            }

            for (int i = msgStart + 3; i < LastMessage.Length; i++)
            {
                PlayerMessage += LastMessage[i];
            }

            if (chatLog.Messages.Count != 0)
            {
                string LastPlayerName = chatLog.Messages[chatLog.Messages.Count - 1].PlayerName;
                string LastPlayerMsg = chatLog.Messages[chatLog.Messages.Count - 1].msg;
                if (LastPlayerName == PlayerName && LastPlayerMsg == PlayerMessage)
                {
                    return; // since we haven't gotten any new messages, return.
                }
            }

            string msgTypeString;
            switch (msgTypeBytes)
            {
                case "00FFFFFF":
                    //host
                    msgTypeString = "Server";
                    byte[] countDownKiller = BitConverter.GetBytes(0);
                    int countDownKillerWrite = 0;
                    MemoryProcessor.Write(_state.Instances[profileid], ChatLogAddr + 0x7C, countDownKiller, countDownKiller.Length, ref countDownKillerWrite);
                    break;
                case "FFC0A0FF":
                    //global
                    msgTypeString = "Global";
                    break;
                case "00FF00FF":
                    //teamchat
                    msgTypeString = "Team";
                    break;
                default:
                    msgTypeString = "Unknown";
                    break;
            }


            if (msgTypeString == "Team")
            {
                string team = "";
                foreach (var item in _state.Instances[profileid].PlayerList)
                {
                    if (item.Value.name == PlayerName)
                    {
                        team = _state.Instances[profileid].PlayerList[item.Key].team.ToString();
                        break;
                    }
                }
                _state.ChatLogs[profileid].Messages.Add(new PlayerChatLog
                {
                    PlayerName = PlayerName,
                    msg = PlayerMessage,
                    msgType = msgTypeString,
                    team = team,
                    dateSent = DateTime.Now
                });
                pl_VoteSkipMap(profileid, PlayerMessage, msgTypeString, PlayerName);
                chatLog.CurrentIndex = nextIndex;
            }
            else if (msgTypeString == "Server")
            {
                _state.ChatLogs[profileid].Messages.Add(new PlayerChatLog
                {
                    PlayerName = _state.Instances[profileid].HostName,
                    msg = PlayerMessage,
                    msgType = msgTypeString,
                    team = "Global",
                    dateSent = DateTime.Now
                });
                chatLog.CurrentIndex = nextIndex;

            }
            else
            {
                _state.ChatLogs[profileid].Messages.Add(new PlayerChatLog
                {
                    PlayerName = PlayerName,
                    msg = PlayerMessage,
                    msgType = msgTypeString,
                    team = "Global",
                    dateSent = DateTime.Now
                });
                // HOOK: Skip Map
                pl_VoteSkipMap(profileid, PlayerMessage, msgTypeString, PlayerName);
                chatLog.CurrentIndex = nextIndex;
            }
        }

        private void pl_VoteSkipMap(int profileid, string PlayerMessage, string msgTypeString, string PlayerName)
        {
            var baseAddr = 0x400000;
            var starterPtr = baseAddr + 0x00062D10;
            byte[] ChatLogPtr = new byte[4];
            int ChatLogPtrRead = 0;
            ReadProcessMemory((int)_state.Instances[profileid].Handle, (int)starterPtr, ChatLogPtr, ChatLogPtr.Length, ref ChatLogPtrRead);

            // get last message sent...
            int ChatLogAddr = BitConverter.ToInt32(ChatLogPtr, 0);
            if (_state.Instances[profileid].Plugins.VoteMaps == true)
            {
                if (_state.Instances[profileid].PlayerList.Count < _state.Instances[profileid].Plugins.VoteMapSettings.MinPlayers)
                {
                    /*if (_state.Instances[profileid].VoteMapStandBy == true)
                    {
                        return;
                    }*/

                    if (PlayerMessage == "!skip")
                    {
                        int colorbuffer_written = 0;
                        byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                        Thread.Sleep(100);
                        // open console
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                        Thread.Sleep(100);
                        int bytesWritten = 0;
                        byte[] buffer = Encoding.Default.GetBytes($"Not enough players to cast a vote! Min Required: {_state.Instances[profileid].Plugins.VoteMapSettings.MinPlayers}\0"); // '\0' marks the end of string
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                        Thread.Sleep(100);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                        Thread.Sleep(100);
                        int revert_colorbuffer = 0;
                        byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                        _state.Instances[profileid].VoteMapStandBy = true;
                    }
                    return;
                }
                else if (_state.Instances[profileid].Status == InstanceStatus.SCORING)
                {
                    if (_state.Instances[profileid].VoteMapStandBy == true)
                    {
                        return;
                    }
                    int colorbuffer_written = 0;
                    byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                    MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                    Thread.Sleep(100);
                    // open console
                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                    Thread.Sleep(100);
                    int bytesWritten = 0;
                    byte[] buffer = Encoding.Default.GetBytes($"Cannot cast vote at this time!\0"); // '\0' marks the end of string
                    MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                    Thread.Sleep(100);
                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                    Thread.Sleep(100);
                    int revert_colorbuffer = 0;
                    byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                    MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                    _state.Instances[profileid].VoteMapStandBy = true;
                    return;
                }
                else
                {
                    _state.Instances[profileid].VoteMapStandBy = false;
                }
                // HOOK: VoteMap
                if (PlayerMessage == "!skip" && msgTypeString != "Server" && PlayerName != _state.Instances[profileid].HostName)
                {
                    bool found = false;
                    foreach (var item in _state.Instances[profileid].VoteMapsTally)
                    {
                        if (item.PlayerName == PlayerName)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false && _state.Instances[profileid].VoteMapsTally.Count == 0)
                    {

                        int colorbuffer_written = 0;
                        byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                        Thread.Sleep(100);

                        // open console
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                        Thread.Sleep(100);
                        int bytesWritten = 0;
                        byte[] buffer = Encoding.Default.GetBytes($"{PlayerName} has initiated a SKIP MAP vote!\0"); // '\0' marks the end of string
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                        Thread.Sleep(100);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                        Thread.Sleep(3000);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                        Thread.Sleep(100);
                        int bytesWritten1 = 0;
                        byte[] buffer1 = Encoding.Default.GetBytes($"Cast your vote by typing !skip\0"); // '\0' marks the end of string
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer1, buffer1.Length, ref bytesWritten1);
                        Thread.Sleep(100);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                        var starterPtr1 = baseAddr + 0x00062D10;
                        byte[] ChatLogPtr1 = new byte[4];
                        int ChatLogPtrRead1 = 0;
                        ReadProcessMemory((int)_state.Instances[profileid].Handle, (int)starterPtr, ChatLogPtr, ChatLogPtr.Length, ref ChatLogPtrRead1);

                        int ChatLogAddr1 = BitConverter.ToInt32(ChatLogPtr, 0);
                        byte[] countDownKiller = BitConverter.GetBytes(0);
                        int countDownKillerWrite = 0;
                        MemoryProcessor.Write(_state.Instances[profileid], ChatLogAddr1 + 0x7C, countDownKiller, countDownKiller.Length, ref countDownKillerWrite);

                        _state.Instances[profileid].VoteMapsTally.Add(new VoteMapsTally
                        {
                            Slot = 0,
                            PlayerName = PlayerName,
                            Vote = VoteMapsTally.VoteStatus.VOTE_YES
                        });
                        _state.Instances[profileid].VoteMapTimer = new Timer
                        {
                            Enabled = true,
                            Interval = 1
                        };
                        int timer = 3000; // 2 minutes
                        _state.Instances[profileid].VoteMapTimer.Tick += (s, e) =>
                        {
                            if (timer != 0)
                            {
                                timer--;
                                return;
                            }
                            if (_state.Instances[profileid].VoteMapsTally.Count > (_state.Instances[profileid].PlayerList.Count / 2))
                            {
                                int colorbuffer_written2 = 0;
                                byte[] colorcode2 = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode2, colorcode2.Length, ref colorbuffer_written2);
                                Thread.Sleep(100);
                                // open console
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                Thread.Sleep(100);
                                int bytesWritten2 = 0;
                                byte[] buffer2 = Encoding.Default.GetBytes($"Vote Success! - Skipping Map...\0"); // '\0' marks the end of string
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer2, buffer2.Length, ref bytesWritten2);
                                Thread.Sleep(100);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                                Thread.Sleep(100);
                                int revert_colorbuffer = 0;
                                byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                                Thread.Sleep(3000);


                                // open console
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, console, 0);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, console, 0);
                                Thread.Sleep(50);
                                int bytesWritten3 = 0;
                                byte[] buffer3 = Encoding.Default.GetBytes("resetgames\0"); // '\0' marks the end of string
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer3, buffer3.Length, ref bytesWritten3);
                                Thread.Sleep(50);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                _state.Instances[profileid].VoteMapsTally.Clear();
                                _state.Instances[profileid].VoteMapStandBy = true;
                                _state.Instances[profileid].VoteMapTimer.Stop();
                            }
                            else
                            {
                                // change color
                                int colorbuffer_written2 = 0;
                                byte[] colorcode2 = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode2, colorcode2.Length, ref colorbuffer_written2);
                                Thread.Sleep(100);


                                // do not skip map
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                Thread.Sleep(100);
                                int bytesWritten_nope = 0;
                                byte[] buffer_nope = Encoding.Default.GetBytes($"Not enough votes to skip map.\0"); // '\0' marks the end of string
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer_nope, buffer_nope.Length, ref bytesWritten_nope);
                                Thread.Sleep(100);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                Thread.Sleep(100);

                                int revert_colorbuffer = 0;
                                byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                                _state.Instances[profileid].VoteMapStandBy = true;
                                _state.Instances[profileid].VoteMapsTally.Clear();
                                _state.Instances[profileid].VoteMapTimer.Stop();
                            }
                        };
                        _state.Instances[profileid].VoteMapTimer.Start();
                    }
                    else if (found == false && _state.Instances[profileid].VoteMapsTally.Count > 0)
                    {
                        int colorbuffer_written = 0;
                        byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                        Thread.Sleep(100);
                        // open console
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                        Thread.Sleep(100);
                        int bytesWritten = 0;
                        byte[] buffer = Encoding.Default.GetBytes($"{PlayerName} - vote casted!\0"); // '\0' marks the end of string
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                        Thread.Sleep(100);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                        Thread.Sleep(100);
                        int revert_colorbuffer = 0;
                        byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                        _state.Instances[profileid].VoteMapsTally.Add(new VoteMapsTally
                        {
                            Slot = 0,
                            PlayerName = PlayerName,
                            Vote = VoteMapsTally.VoteStatus.VOTE_YES
                        });
                    }
                }
            }
        }

        public void CollectPlayerStats(int InstanceID)
        {
            foreach (var item in _state.Instances[InstanceID].PlayerList)
            {
                var key = item.Key;
                var val = item.Value;

                if (!_state.PlayerStats[InstanceID].Player.ContainsKey(val.name))
                {
                    // add each weapon they currently have
                    Dictionary<string, InternalWeaponStats> weaponStats = new Dictionary<string, InternalWeaponStats>();
                    foreach (var weapon in _state.Instances[InstanceID].PlayerList[key].weapons)
                    {
                        weaponStats.Add(weapon, new InternalWeaponStats
                        {

                        });
                    }
                    // create new player object
                    _state.PlayerStats[InstanceID].Player.Add(val.name, new CollectedPlayerStats
                    {
                        kills = val.kills,
                        deaths = val.deaths,
                        zonetime = val.zonetime,
                        zonekills = val.zonekills,
                        zonedefendkills = val.zonedefendkills,
                        playerrevives = val.playerrevives,
                        team = val.team.ToString(),
                        flagcaptures = val.flagcaptures,
                        suicides = val.suicides,
                        teamkills = val.teamkills,
                        headshots = val.headshots,
                        knifekills = val.knifekills,
                        revives = val.revives,
                        pspattempts = val.pspattempts,
                        psptakeover = val.psptakeover,
                        doublekills = val.doublekills,
                        flagcarrierkills = val.flagcarrierkills,
                        flagcarrierdeaths = val.flagcarrierdeaths,
                        exp = val.exp,
                        ADTargetsDestroyed = val.ADTargetsDestroyed,
                        FlagSaves = val.FlagSaves,
                        totalshots = val.totalshots,
                        PlayerClass = val.PlayerClass,
                        weaponStats = weaponStats
                    });
                }
                else
                {
                    Dictionary<string, InternalWeaponStats> weaponStats = _state.PlayerStats[InstanceID].Player[val.name].weaponStats;

                    if (_state.PlayerStats[InstanceID].Player[val.name].weaponStats.ContainsKey(val.selectedWeapon))
                    {
                        _state.PlayerStats[InstanceID].Player[val.name].weaponStats[val.selectedWeapon].kills += (val.kills - (val.kills - _state.PlayerStats[InstanceID].Player[val.name].weaponStats[val.selectedWeapon].kills));
                        _state.PlayerStats[InstanceID].Player[val.name].weaponStats[val.selectedWeapon].shotsfired += (val.totalshots - (val.totalshots - _state.PlayerStats[InstanceID].Player[val.name].weaponStats[val.selectedWeapon].shotsfired));
                    }
                    else
                    {
                        _state.PlayerStats[InstanceID].Player[val.name].weaponStats.Add(val.selectedWeapon, new InternalWeaponStats());
                    }

                    // update existing player object...
                    _state.PlayerStats[InstanceID].Player[val.name].ADTargetsDestroyed += (val.ADTargetsDestroyed - _state.PlayerStats[InstanceID].Player[val.name].ADTargetsDestroyed);
                    _state.PlayerStats[InstanceID].Player[val.name].kills += (val.kills - _state.PlayerStats[InstanceID].Player[val.name].kills);
                    _state.PlayerStats[InstanceID].Player[val.name].deaths += (val.deaths - _state.PlayerStats[InstanceID].Player[val.name].deaths);
                    _state.PlayerStats[InstanceID].Player[val.name].zonetime += (val.zonetime - _state.PlayerStats[InstanceID].Player[val.name].zonetime);
                    _state.PlayerStats[InstanceID].Player[val.name].zonekills += (val.zonekills - _state.PlayerStats[InstanceID].Player[val.name].zonekills);
                    _state.PlayerStats[InstanceID].Player[val.name].zonedefendkills += (val.zonedefendkills - _state.PlayerStats[InstanceID].Player[val.name].zonedefendkills);
                    _state.PlayerStats[InstanceID].Player[val.name].playerrevives += (val.playerrevives - _state.PlayerStats[InstanceID].Player[val.name].playerrevives);
                    _state.PlayerStats[InstanceID].Player[val.name].team = val.team.ToString();
                    _state.PlayerStats[InstanceID].Player[val.name].flagcaptures += (val.flagcaptures - _state.PlayerStats[InstanceID].Player[val.name].flagcaptures);
                    _state.PlayerStats[InstanceID].Player[val.name].suicides += (val.suicides - _state.PlayerStats[InstanceID].Player[val.name].suicides);
                    _state.PlayerStats[InstanceID].Player[val.name].teamkills += (val.teamkills - _state.PlayerStats[InstanceID].Player[val.name].teamkills);
                    _state.PlayerStats[InstanceID].Player[val.name].headshots += (val.headshots - _state.PlayerStats[InstanceID].Player[val.name].headshots);
                    _state.PlayerStats[InstanceID].Player[val.name].knifekills += (val.knifekills - _state.PlayerStats[InstanceID].Player[val.name].knifekills);
                    _state.PlayerStats[InstanceID].Player[val.name].revives += (val.revives - _state.PlayerStats[InstanceID].Player[val.name].revives);
                    _state.PlayerStats[InstanceID].Player[val.name].pspattempts += (val.pspattempts - _state.PlayerStats[InstanceID].Player[val.name].pspattempts);
                    _state.PlayerStats[InstanceID].Player[val.name].psptakeover += (val.psptakeover - _state.PlayerStats[InstanceID].Player[val.name].psptakeover);
                    _state.PlayerStats[InstanceID].Player[val.name].doublekills += (val.doublekills - _state.PlayerStats[InstanceID].Player[val.name].doublekills);
                    _state.PlayerStats[InstanceID].Player[val.name].flagcarrierkills += (val.flagcarrierkills - _state.PlayerStats[InstanceID].Player[val.name].flagcarrierkills);
                    _state.PlayerStats[InstanceID].Player[val.name].flagcarrierdeaths += (val.flagcarrierdeaths - _state.PlayerStats[InstanceID].Player[val.name].flagcarrierdeaths);
                    _state.PlayerStats[InstanceID].Player[val.name].exp += (val.exp - _state.PlayerStats[InstanceID].Player[val.name].exp);
                    _state.PlayerStats[InstanceID].Player[val.name].FlagSaves += (val.FlagSaves - _state.PlayerStats[InstanceID].Player[val.name].FlagSaves);
                    _state.PlayerStats[InstanceID].Player[val.name].totalshots += (val.totalshots - _state.PlayerStats[InstanceID].Player[val.name].totalshots);
                    _state.PlayerStats[InstanceID].Player[val.name].PlayerClass = val.PlayerClass;

                    // _state.PlayerStats[InstanceID].Player[val.name].weaponStats = weaponStats;
                }

                // HOOK: detect disconnected player & remove vote
                bool found = false;
                int i = -1;
                foreach (var vote in _state.Instances[InstanceID].VoteMapsTally)
                {
                    i++;
                    if (vote.PlayerName == item.Value.name)
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    _state.Instances[InstanceID].VoteMapsTally.RemoveAt(i);
                }
            }
        }

        public static int tid;

        public void UpdateGlobalGameType(int instanceid)
        {
            // this function is responsible for adjusting the Pinger Queries to the current game type
            var baseAddr = 0x400000;
            var startingPtr = baseAddr + 0xACE0E8; // pinger query
            byte[] read_pingergametype = new byte[4];
            int read_pingergametypeBytesRead = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)startingPtr, read_pingergametype, read_pingergametype.Length, ref read_pingergametypeBytesRead);
            int PingerGameType = BitConverter.ToInt32(read_pingergametype, 0);

            // get set gametype
            var CurrentGameTypeAddr = baseAddr + 0x5F21A4;
            byte[] read_currentgametype = new byte[4];
            int read_currentgametypeBytesRead = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)CurrentGameTypeAddr, read_currentgametype, read_currentgametype.Length, ref read_currentgametypeBytesRead);
            int CurrentGameType = BitConverter.ToInt32(read_currentgametype, 0);

            // to prevent locking of this address simply look at each address before writing to the address...
            if (PingerGameType != CurrentGameType)
            {
                int UpdatePingerQuery = 0;
                MemoryProcessor.Write(_state.Instances[instanceid], (int)startingPtr, read_currentgametype, read_currentgametype.Length, ref UpdatePingerQuery);
                return;
            }
            else
            {
                // no update required... Exit the function.
                return;
            }
        }

        public InstanceStatus CheckStatus(int instanceid)
        {
            var baseAddr = 0x400000;
            var startingPointer = baseAddr + 0x00098334;
            byte[] startingPointerBuffer = new byte[4];
            int startingPointerReadBytes = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)startingPointer, startingPointerBuffer, startingPointerBuffer.Length, ref startingPointerReadBytes);


            int statusLocationPointer = BitConverter.ToInt32(startingPointerBuffer, 0);
            byte[] statusLocation = new byte[4];
            int statusLocationReadBytes = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].Handle, (int)statusLocationPointer, statusLocation, statusLocation.Length, ref statusLocationReadBytes);
            int instanceStatus = BitConverter.ToInt32(statusLocation, 0);

            return (InstanceStatus)instanceStatus;
        }

        public void Check4VPN(int ArrayID)
        {
            if (_state.Instances[ArrayID].enableVPNCheck == false)
            {
                return; // do not check for VPNs if it's disabled on the instance
            }
            foreach (var playerlistdata in _state.Instances[ArrayID].PlayerList)
            {
                try
                {
                    if (_state.IPQualityCache[ArrayID].IPInformation.Count == 0)
                    {
                        string jsonData = IPQualityCheck(playerlistdata.Value.address);
                        var results = JsonConvert.DeserializeObject<ipqualityClass>(jsonData);
                        results.address = playerlistdata.Value.address;
                        results.NextCheck = DateTime.Now.AddDays(25);
                        _state.IPQualityCache[ArrayID].IPInformation.Add(results);
                        SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                        conn.Open();
                        SQLiteCommand insertDBCache = new SQLiteCommand("INSERT INTO `ipqualitycache` (`id`, `profile_id`, `address`, `fraud_score`, `country_code`, `region`, `city`, `ISP`, `is_crawler`, `mobile`, `host`, `proxy`, `vpn`, `tor`, `active_vpn`, `active_tor`, `recent_abuse`, `bot_status`, `request_id`, `lat`, `long`, `NextCheck`) VALUES (NULL, @profileid, @address, @fraud_score, @country_code, @region, @city, @ISP, @is_crawler, @mobile, @host, @proxy, @vpn, @tor, @active_vpn, @active_tor, @recent_abuse, @bot_status, @request_id, @lat, @long, @NextCheck);", conn);
                        insertDBCache.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                        insertDBCache.Parameters.AddWithValue("@address", playerlistdata.Value.address);
                        insertDBCache.Parameters.AddWithValue("@fraud_score", results.fraud_score);
                        insertDBCache.Parameters.AddWithValue("@country_code", results.country_code);
                        insertDBCache.Parameters.AddWithValue("@region", results.region);
                        insertDBCache.Parameters.AddWithValue("@city", results.city);
                        insertDBCache.Parameters.AddWithValue("@ISP", results.ISP);
                        insertDBCache.Parameters.AddWithValue("@is_crawler", results.is_crawler);
                        insertDBCache.Parameters.AddWithValue("@mobile", results.mobile);
                        insertDBCache.Parameters.AddWithValue("@host", results.host);
                        insertDBCache.Parameters.AddWithValue("@proxy", results.proxy);
                        insertDBCache.Parameters.AddWithValue("@vpn", results.vpn);
                        insertDBCache.Parameters.AddWithValue("@tor", results.tor);
                        insertDBCache.Parameters.AddWithValue("@active_vpn", results.active_vpn);
                        insertDBCache.Parameters.AddWithValue("@active_tor", results.active_tor);
                        insertDBCache.Parameters.AddWithValue("@recent_abuse", results.recent_abuse);
                        insertDBCache.Parameters.AddWithValue("@bot_status", results.bot_status);
                        insertDBCache.Parameters.AddWithValue("@lat", results.latitude);
                        insertDBCache.Parameters.AddWithValue("@long", results.longitude);
                        insertDBCache.Parameters.AddWithValue("@request_id", results.request_id);
                        insertDBCache.Parameters.AddWithValue("@NextCheck", results.NextCheck);
                        insertDBCache.ExecuteNonQuery();
                        insertDBCache.Dispose();
                        conn.Close();
                    }
                    else
                    {
                        int Index = 0;
                        string addressCache = "";
                        for (int i = 0; i < _state.IPQualityCache[ArrayID].IPInformation.Count; i++)
                        {
                            string address = _state.IPQualityCache[ArrayID].IPInformation[i].address;
                            if (address == playerlistdata.Value.address)
                            {
                                Index = i;
                                addressCache = address;
                                break;
                            }
                        }
                        if (addressCache == "")
                        {
                            string jsonData = IPQualityCheck(playerlistdata.Value.address);
                            var results = JsonConvert.DeserializeObject<ipqualityClass>(jsonData);
                            results.address = playerlistdata.Value.address;
                            results.NextCheck = DateTime.Now.AddDays(25);
                            _state.IPQualityCache[ArrayID].IPInformation.Add(results);
                            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                            conn.Open();
                            SQLiteCommand insertDBCache = new SQLiteCommand("INSERT INTO `ipqualitycache` (`id`, `profile_id`, `address`, `fraud_score`, `country_code`, `region`, `city`, `ISP`, `is_crawler`, `mobile`, `host`, `proxy`, `vpn`, `tor`, `active_vpn`, `active_tor`, `recent_abuse`, `bot_status`, `lat`, `long`, `request_id`, `NextCheck`) VALUES (NULL, @profileid, @address, @fraud_score, @country_code, @region, @city, @ISP, @is_crawler, @mobile, @host, @proxy, @vpn, @tor, @active_vpn, @active_tor, @recent_abuse, @bot_status, @lat, @long, @request_id, @NextCheck);", conn);
                            insertDBCache.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                            insertDBCache.Parameters.AddWithValue("@address", playerlistdata.Value.address);
                            insertDBCache.Parameters.AddWithValue("@fraud_score", results.fraud_score);
                            insertDBCache.Parameters.AddWithValue("@country_code", results.country_code);
                            insertDBCache.Parameters.AddWithValue("@region", results.region);
                            insertDBCache.Parameters.AddWithValue("@city", results.city);
                            insertDBCache.Parameters.AddWithValue("@ISP", results.ISP);
                            insertDBCache.Parameters.AddWithValue("@is_crawler", results.is_crawler);
                            insertDBCache.Parameters.AddWithValue("@mobile", results.mobile);
                            insertDBCache.Parameters.AddWithValue("@host", results.host);
                            insertDBCache.Parameters.AddWithValue("@proxy", results.proxy);
                            insertDBCache.Parameters.AddWithValue("@vpn", results.vpn);
                            insertDBCache.Parameters.AddWithValue("@tor", results.tor);
                            insertDBCache.Parameters.AddWithValue("@active_vpn", results.active_vpn);
                            insertDBCache.Parameters.AddWithValue("@active_tor", results.active_tor);
                            insertDBCache.Parameters.AddWithValue("@recent_abuse", results.recent_abuse);
                            insertDBCache.Parameters.AddWithValue("@bot_status", results.bot_status);
                            insertDBCache.Parameters.AddWithValue("@lat", results.latitude);
                            insertDBCache.Parameters.AddWithValue("@long", results.longitude);
                            insertDBCache.Parameters.AddWithValue("@request_id", results.request_id);
                            insertDBCache.Parameters.AddWithValue("@NextCheck", results.NextCheck);
                            insertDBCache.ExecuteNonQuery();
                            insertDBCache.Dispose();
                            conn.Close();
                            return;
                        }
                        if (DateTime.Compare(_state.IPQualityCache[ArrayID].IPInformation[Index].NextCheck, DateTime.Now) < 0)
                        {
                            string jsonData = IPQualityCheck(playerlistdata.Value.address);
                            var results = JsonConvert.DeserializeObject<ipqualityClass>(jsonData);
                            results.address = playerlistdata.Value.address;
                            results.NextCheck = DateTime.Now.AddDays(25);
                            _state.IPQualityCache[ArrayID].IPInformation[Index] = results;
                            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                            conn.Open();
                            SQLiteCommand checkDB = new SQLiteCommand("SELECT * FROM `ipqualitycache` WHERE `profile_id` = @profileid AND `address` = @address; ", conn);
                            checkDB.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                            checkDB.Parameters.AddWithValue("@address", playerlistdata.Value.address);
                            SQLiteDataReader checkDBReader = checkDB.ExecuteReader();
                            checkDB.Dispose();
                            if (checkDBReader.HasRows)
                            {
                                checkDBReader.Read();
                                SQLiteCommand updateDBCache = new SQLiteCommand("UPDATE `ipqualitycache` SET `address` = @address, `fraud_score` = @fraud_score, `country_code` = @country_code, `region` = @region, `city` = @city, `ISP` = @ISP, `is_crawler` = @is_crawler, `mobile` = @mobile, `host` = @host, `proxy` = @proxy, `vpn` = @vpn, `tor` = @tor, `active_vpn` = @active_vpn, `active_tor` = @active_tor, `recent_abuse` = @recent_abuse, `bot_status` = @bot_status, `request_id` = @request_id, `lat` = @lat, `long` = @long, `NextCheck` = @NextCheck WHERE `id` = @id AND `profile_id` = @profileid;", conn);
                                updateDBCache.Parameters.AddWithValue("@address", playerlistdata.Value.address);
                                updateDBCache.Parameters.AddWithValue("@fraud_score", results.fraud_score);
                                updateDBCache.Parameters.AddWithValue("@country_code", results.country_code);
                                updateDBCache.Parameters.AddWithValue("@region", results.region);
                                updateDBCache.Parameters.AddWithValue("@city", results.city);
                                updateDBCache.Parameters.AddWithValue("@ISP", results.ISP);
                                updateDBCache.Parameters.AddWithValue("@is_crawler", results.is_crawler);
                                updateDBCache.Parameters.AddWithValue("@mobile", results.mobile);
                                updateDBCache.Parameters.AddWithValue("@host", results.host);
                                updateDBCache.Parameters.AddWithValue("@proxy", results.proxy);
                                updateDBCache.Parameters.AddWithValue("@vpn", results.vpn);
                                updateDBCache.Parameters.AddWithValue("@tor", results.tor);
                                updateDBCache.Parameters.AddWithValue("@active_vpn", results.active_vpn);
                                updateDBCache.Parameters.AddWithValue("@active_tor", results.active_tor);
                                updateDBCache.Parameters.AddWithValue("@recent_abuse", results.recent_abuse);
                                updateDBCache.Parameters.AddWithValue("@bot_status", results.bot_status);
                                updateDBCache.Parameters.AddWithValue("@request_id", results.request_id);
                                updateDBCache.Parameters.AddWithValue("@lat", results.latitude);
                                updateDBCache.Parameters.AddWithValue("@long", results.longitude);
                                updateDBCache.Parameters.AddWithValue("@NextCheck", results.NextCheck);
                                updateDBCache.Parameters.AddWithValue("@id", checkDBReader.GetInt32(checkDBReader.GetOrdinal("id")));
                                updateDBCache.Parameters.AddWithValue("@profileid", checkDBReader.GetInt32(checkDBReader.GetOrdinal("profile_id")));
                                checkDBReader.Close();
                                checkDBReader.Dispose();
                                updateDBCache.ExecuteNonQuery();
                                updateDBCache.Dispose();
                            }
                            if (!checkDBReader.IsClosed)
                                checkDBReader.Close();
                            conn.Close();
                        }
                        int fraud_score = _state.IPQualityCache[ArrayID].IPInformation[Index].fraud_score;
                        bool FoundOnWhitelist = false;
                        for (int i = 0; i < _state.Instances[ArrayID].VPNWhiteList.Count; i++)
                        {
                            if (_state.Instances[ArrayID].VPNWhiteList[i].IPAddress.ToString() == playerlistdata.Value.address)
                            {
                                FoundOnWhitelist = true;
                                break;
                            }
                        }
                        if (FoundOnWhitelist == false)
                        {
                            if (fraud_score >= _state.IPQualityCache[ArrayID].WarnLevel && (_state.IPQualityCache[ArrayID].IPInformation[Index].active_vpn == true))
                            {
                                bool isBanned = false;
                                foreach (var ban in _state.Instances[ArrayID].BanList)
                                {
                                    if (ban.ipaddress == addressCache)
                                    {
                                        isBanned = true;
                                        break;
                                    }
                                }
                                if (isBanned == false)
                                {
                                    _state.Instances[ArrayID].BanList.Add(new playerbans
                                    {
                                        ipaddress = playerlistdata.Value.address,
                                        player = playerlistdata.Value.name,
                                        reason = "[BMT] Failed to Pass IP Checks",
                                        retry = DateTime.Now,
                                        newBan = true,
                                        onlykick = false,
                                        expires = "-1",
                                        VPNBan = true,
                                        addedDate = DateTime.Now,
                                        bannedBy = "BMT Automated Systems",
                                        lastseen = DateTime.Now
                                    });
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    _state.eventLog.WriteEntry("BMTv4 TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                    return;
                }
            }
        }
        private void CheckBans(int profileid, int pid)
        {
            Dictionary<int, playerlist> currentPlayers = _state.Instances[profileid].PlayerList;
            List<playerbans> currentBans = _state.Instances[profileid].BanList;
            int numPlayers = _state.Instances[profileid].NumPlayers;

            if (numPlayers > 0)
            {
                for (int itemIndex = 0; itemIndex < currentBans.Count; itemIndex++)
                {
                    int index = 1;
                    foreach (var player in _state.Instances[profileid].PlayerList)
                    {
                        try
                        {
                            if (currentBans.Count == itemIndex)
                            {
                                break;
                            }
                            string playername = player.Value.name;
                            string playerIP = player.Value.address;
                            int slot = player.Value.slot;
                            string bannedname = currentBans[itemIndex].player;
                            string bannedIP = currentBans[itemIndex].ipaddress;
                            string reason = currentBans[itemIndex].reason;
                            bool VPNBan = currentBans[itemIndex].VPNBan;
                            bool newBan = currentBans[itemIndex].newBan;
                            DateTime NextTry = currentBans[itemIndex].retry;
                            string expires = currentBans[itemIndex].expires;
                            bool onlyKick = currentBans[itemIndex].onlykick;
                            int banid = currentBans[itemIndex].id;

                            if (onlyKick == true)
                            {
                                if ((playername == bannedname || playerIP == bannedIP) && DateTime.Compare(NextTry, DateTime.Now) < 0)
                                {
                                    // change color to white
                                    int colorbuffer_written = 0;
                                    byte[] colorcode = HexConverter.ToByteArray("6A 08".Replace(" ", ""));
                                    MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                                    Thread.Sleep(100);

                                    // post kick message
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                    Thread.Sleep(100);
                                    int bytesWritten = 0;
                                    byte[] buffer;
                                    buffer = Encoding.Default.GetBytes($"KICKING!!! {playername} - {reason}\0"); // '\0' marks the end of string
                                    MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                                    Thread.Sleep(100);
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                                    // change color to normal
                                    Thread.Sleep(100);
                                    int revert_colorbuffer = 0;
                                    byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                    MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);

                                    // open console
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, console, 0);
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, console, 0);
                                    Thread.Sleep(100);
                                    int bytesWritten_kick = 0;
                                    byte[] buffer_kick = Encoding.Default.GetBytes($"punt {slot}\0"); // '\0' marks the end of string
                                    MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer_kick, buffer_kick.Length, ref bytesWritten_kick);
                                    Thread.Sleep(100);
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                    PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                    index++;
                                    currentBans[itemIndex].retry = DateTime.Now.AddMinutes(1.0);
                                    currentBans[itemIndex].lastseen = DateTime.Now;
                                    Thread.Sleep(100);

                                    // remove entry as they're onlyKick not Banned.
                                    _state.Instances[profileid].BanList.RemoveAt(itemIndex);
                                    continue;
                                }
                                else
                                {
                                    index++;
                                    continue;
                                }
                            }
                            else
                            {
                                if (expires == "-1")
                                {
                                    if ((playername == bannedname || playerIP == bannedIP) && DateTime.Compare(NextTry, DateTime.Now) < 0)
                                    {
                                        bool FoundWhiteList = false;
                                        for (int i = 0; i < _state.Instances[profileid].VPNWhiteList.Count; i++)
                                        {
                                            if (bannedIP == _state.Instances[profileid].VPNWhiteList[i].IPAddress.ToString())
                                            {
                                                FoundWhiteList = true;
                                                break;
                                            }
                                        }
                                        if (VPNBan == true && FoundWhiteList == true)
                                        {
                                            _state.Instances[profileid].BanList.RemoveAt(itemIndex);
                                        }
                                        else
                                        {
                                            // change color to white
                                            int colorbuffer_written = 0;
                                            byte[] colorcode = HexConverter.ToByteArray("6A 08".Replace(" ", ""));
                                            MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                                            Thread.Sleep(100);

                                            // post kick message
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                            Thread.Sleep(100);
                                            int bytesWritten = 0;
                                            byte[] buffer;
                                            if (newBan == false)
                                            {
                                                buffer = Encoding.Default.GetBytes($"KICKING!!! {playername} - {reason}\0"); // '\0' marks the end of string
                                            }
                                            else
                                            {
                                                buffer = Encoding.Default.GetBytes($"BANNING!!! {playername} - {reason}\0"); // '\0' marks the end of string
                                            }
                                            MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                                            Thread.Sleep(100);
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                                            // change color to normal
                                            Thread.Sleep(100);
                                            int revert_colorbuffer = 0;
                                            byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                            MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);

                                            // open console
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, console, 0);
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, console, 0);
                                            Thread.Sleep(100);
                                            int bytesWritten_kick = 0;
                                            byte[] buffer_kick = Encoding.Default.GetBytes($"punt {slot}\0"); // '\0' marks the end of string
                                            MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer_kick, buffer_kick.Length, ref bytesWritten_kick);
                                            Thread.Sleep(100);
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                            PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                            index++;
                                            currentBans[itemIndex].retry = DateTime.Now.AddMinutes(1.0);
                                            currentBans[itemIndex].lastseen = DateTime.Now;
                                            Thread.Sleep(100);

                                            // reset newBan flag
                                            currentBans[itemIndex].newBan = false;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        index++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    // what to do when the ban expires
                                    if ((playername == bannedname || playerIP == bannedIP) && DateTime.Compare(NextTry, DateTime.Now) < 0)
                                    {
                                        // change color to white
                                        int colorbuffer_written = 0;
                                        byte[] colorcode = HexConverter.ToByteArray("6A 08".Replace(" ", ""));
                                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                                        Thread.Sleep(100);

                                        // post kick message
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                        Thread.Sleep(100);
                                        int bytesWritten = 0;
                                        byte[] buffer;
                                        if (newBan == false)
                                        {
                                            buffer = Encoding.Default.GetBytes($"BANNING!!! {playername} - {reason}\0"); // '\0' marks the end of string
                                        }
                                        else
                                        {
                                            buffer = Encoding.Default.GetBytes($"KICKING!!! {playername} - {reason}\0"); // '\0' marks the end of string
                                        }
                                        MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                                        Thread.Sleep(100);
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                                        // change color to normal
                                        Thread.Sleep(100);
                                        int revert_colorbuffer = 0;
                                        byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                        MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);

                                        // open console
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, console, 0);
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, console, 0);
                                        Thread.Sleep(100);
                                        int bytesWritten_kick = 0;
                                        byte[] buffer_kick = Encoding.Default.GetBytes($"punt {slot}\0"); // '\0' marks the end of string
                                        MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer_kick, buffer_kick.Length, ref bytesWritten_kick);
                                        Thread.Sleep(100);
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                        PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                        index++;
                                        currentBans[itemIndex].retry = DateTime.Now.AddMinutes(1.0);
                                        currentBans[itemIndex].lastseen = DateTime.Now;
                                        Thread.Sleep(100);

                                        // reset newBan flag
                                        currentBans[itemIndex].newBan = false;
                                        continue;
                                    }
                                    else
                                    {
                                        index++;
                                        continue;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            //Console.WriteLine(e);
                            break;
                        }
                    }
                }
            }
        }

        private void btnSM_click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);
            SM_ServerManager server_Manager = new SM_ServerManager(_state, id);
            server_Manager.ShowDialog();
        }

        private void serverProfiles_SelectionChanged(object sender, EventArgs e)
        {
            if (list_serverProfiles.SelectedRows.Count == 0)
            {
                return;
            }
            var row = list_serverProfiles.SelectedRows[0];
            var instance = _state.Instances[row.Index];

            if (instance.Status.Equals(InstanceStatus.ONLINE))
            {
                UpdateButtonsOnline();
            }
            else if (instance.Status.Equals(InstanceStatus.OFFLINE))
            {
                UpdateButtonsOffline();
            }
            else if (instance.Status.Equals(InstanceStatus.SCORING))
            {
                UpdateButtonsOnline();
            }
            else if (instance.Status.Equals(InstanceStatus.LOADINGMAP))
            {
                UpdateButtonsOnline();
            }
            else if (instance.Status.Equals(InstanceStatus.STARTDELAY))
            {
                UpdateButtonsOnline();
            }
        }

        private void btnUM_click(object sender, EventArgs e)
        {
            UserManager userManager = new UserManager(_state);
            userManager.ShowDialog();
        }

        private void btnRM_click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);
            SM_RotationManager rotationManager = new SM_RotationManager(_state, id);
            rotationManager.Show();
        }
    }
}
