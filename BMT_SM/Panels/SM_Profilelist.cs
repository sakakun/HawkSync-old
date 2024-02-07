using HawkSync_SM.classes;
using HawkSync_SM.classes.ChatManagement;
using HawkSync_SM.classes.logs;
using HawkSync_SM.classes.Plugins.pl_VoteMaps;
using HawkSync_SM.classes.Plugins.pl_WelcomePlayer;
using HawkSync_SM.RCClasses;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
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
    public partial class SM_Profilelist : Form
    {
        // Nothing controling the game should be in this window.  Should be in the ServerProcessHandler.cs
        // Goal to move all game control to ServerProcessHandler.cs and write the RC API to point to those methods.

        // Logging
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        // Number of RC Clients
        int numClientsRC;

        // Object: GameTypes
        Dictionary<int, GameType> gameTypes = new Dictionary<int, GameType>();
        public DataTable table_profileList = new DataTable();
        public Dictionary<int, Dictionary<int, MapList>> mapList = new Dictionary<int, Dictionary<int, MapList>>();
        public SQLiteConnection hawkSyncDB = new SQLiteConnection(ProgramConfig.DBConfig);

        // Object: Application State (Server Manager)       
        public AppState _state;
        public ServerManagement serverManagement = new ServerManagement();
        public IPManagement ipManagement = new IPManagement();
        public ChatManagement chatManagement = new ChatManagement();

        // Object: Ticker (Used to Refresh Information x Seconds
        private Timer Ticker = new Timer()
        {
            Enabled = false,
            Interval = 100
        };

        // Server Manager - Game Profile List (Main Process)
        public SM_Profilelist(AppState state)
        {
            // Launch GUI
            _state = state;
            InitializeComponent();                                                                // Load GUI (Desinger Elements)

            // Ticker Checks
            Ticker.Tick += (sender, e) =>
            {
                InstanceSeverCheck();                                                             // Instance Status Check
                InstanceTicker();                                                                 // Instance Ticker
                (new PlayerManagement()).checkPlayerHistory(ref _state);                          // Process Player History
                (new PlayerManagement()).checkExpiredBans(ref _state);                            // Check for Expired Bans
                (new HeartBeatMonitor()).ProcessHeartBeats(ref _state);                           // HeartBeat Monitor Hook
                RC_CleanClientConnections();                                                      // Clean RC Connections               
            };

            // Server Manager Specific Events
            hawkSyncDB.Open();
            SetupAppState(hawkSyncDB);
            get_gameTypes(hawkSyncDB);
            RC_BeginListening();
            hawkSyncDB.Close();
        }


        // Server Manager - Process Existing Functions

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
                        _state.Instances[ArrayID].ProcessHandle = processHandle;

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

                        // wait for ServerRC to be online
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

                        ServerManagement serverManagerUpdateMemory = new ServerManagement();
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

                        _state.eventLog.WriteEntry("BMTv4 has detected a ServerRC crash!\n\nProfile Name: " + _state.Instances[ArrayID].GameName + "\nMap File: " + _state.Instances[ArrayID].MapList[_state.Instances[ArrayID].mapIndex + 1].MapFile + "\nMap Title: " + _state.Instances[ArrayID].MapList[_state.Instances[ArrayID].mapIndex + 1].MapName + "\n\nThe ServerRC has been automatically restarted.", EventLogEntryType.Warning);
                    }
                }
            }
        }
       
        // Server Manager Functions //
        /* 
         * InstanceSeverCheck
         * - Check Server Instances for Active Servers (Recover from Server Manager Crash)
         * - The following function checks for the last process ID and attempts to re-attach to the process.
         */
        private void InstanceSeverCheck()
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.ProcessHandle == IntPtr.Zero)
                {
                    if (serverManagement.ProcessExist(instance.Value.PID.GetValueOrDefault()))
                    {
                        instance.Value.ProcessHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, instance.Value.PID.GetValueOrDefault());
                        if (instance.Value.ProcessHandle == IntPtr.Zero)
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
        /* 
         * InstanceTicker
         * - Check each instance and update the instance information from the game.
         * - Trigger any events that are required for the instance (Bans/Chat Events/ETC).
         */
        private void InstanceTicker()
        {
            try
            {
                foreach (var item in _state.Instances)
                {
                    Instance instance = item.Value;
                    int rowId = item.Key;

                    if (table_profileList.Rows.Count == 0)
                    {
                        return;
                    }

                    DataRow row = table_profileList.Rows[rowId];
                    DateTime currentTime = DateTime.Now;
                    if (DateTime.Compare(instance.NextUpdateTime, currentTime) < 0)
                    {
                        if (instance.PID != null)
                        {
                            try
                            {
                                var PID = instance.PID.GetValueOrDefault();
                                if (serverManagement.ProcessExist(instance.PID.GetValueOrDefault()))
                                {
                                    // set process name incase the PID changes...
                                    serverManagement.SetNovaID(ref _state, rowId);
                                    SetWindowText(_state.ApplicationProcesses[rowId].MainWindowHandle, $"{instance.GameName}");

                                    int timeRemainingInGame = serverManagement.GetTimeLeft(ref _state, rowId);
                                    var map = serverManagement.GetCurrentMission(ref _state, rowId);
                                    var currentPlayers = serverManagement.GetCurrentPlayers(ref _state,rowId);
                                    //var currentPlayers = GetCurrentPlayers(rowId);
                                    var currentGameType = "";
                                    foreach (var gameTypeList in gameTypes)
                                    {
                                        var gametype = gameTypeList.Value;
                                        if (serverManagement.GetCurrentGameType(ref _state,rowId).Equals(gametype.DatabaseId))
                                        {
                                            currentGameType = gametype.ShortName;
                                            break;
                                        }
                                    }

                                    instance.Status = serverManagement.CheckStatus(ref _state, rowId);
                                    serverManagement.UpdateGlobalGameType(ref _state, rowId);
                                    // prevents loading garbage data while switching maps,
                                    // should also prevent ghosts from showing on the playerlist.
                                    if (instance.Status != InstanceStatus.LOADINGMAP)
                                    {
                                        try
                                        {
                                            instance.PlayerList = serverManagement.CurrentPlayerList(ref _state, rowId);
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                        ipManagement.CheckBans(ref _state, rowId, PID);
                                    }

                                    if (instance.EnableWebStats == true && instance.Status == InstanceStatus.ONLINE && instance.collectPlayerStats == true)
                                    {
                                        // collect player stats for submission
                                        event_collectPlayerStats(rowId);
                                    }

                                    // important for clearing stupid map cycle shit
                                    instance.mapCounter = serverManagement.UpdateMapCycleCounter(ref _state, rowId);
                                    //UpdateMapCycleGarbage(rowId);

                                    // check for VPNs
                                    if (ProgramConfig.Enable_VPNWhiteList == true)
                                    {
                                        ipManagement.Check4VPN(ref _state, rowId);
                                    }

                                    // get chatLogs...
                                    //event_getChatLogs(rowId, PID);


                                    if (instance.Status != InstanceStatus.LOADINGMAP && instance.Status != InstanceStatus.SCORING)
                                    {
                                        event_processPlayerWarnings(rowId);
                                    }

                                    if (instance.Status == InstanceStatus.LOADINGMAP)
                                    {
                                        if (instance.IsRunningPostGameProcesses == false)
                                        {
                                            instance.IsRunningPostGameProcesses = true;
                                            if (ProgramConfig.ApplicationDebug)
                                            {
                                                log.Info("Instance " + rowId + " is running the Loading Process Handler.");
                                            }
                                            PostGameProcess postGameProcess = new PostGameProcess(_state, instance.DataTableColumnId, _state.ChatLogs[rowId], _state.PlayerStats[item.Key]);
                                            postGameProcess.Run();

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
                                            if (ProgramConfig.ApplicationDebug)
                                            {
                                                log.Info("Instance " + rowId + " is running the Scoring Process Handler.");
                                            }
                                            ScoringGameProcess postGameProcess = new ScoringGameProcess(_state, rowId, _state.ChatLogs[rowId], _state.PlayerStats[item.Key]);
                                            postGameProcess.Run();
                                            instance.IsRunningScoringGameProcesses = true;
                                        }
                                    }

                                    if (instance.Status == InstanceStatus.LOADINGMAP || instance.Status == InstanceStatus.SCORING)
                                    {
                                        serverManagement.ChangePlayerTeam(ref _state, rowId); // change player slot's team
                                    }

                                    if (instance.Status == InstanceStatus.STARTDELAY || instance.Status == InstanceStatus.ONLINE)
                                    {
                                        instance.gameCrashCounter = 0;
                                        serverManagement.ResetGodMode(ref _state, rowId);
                                        serverManagement.ProcessDisarmedPlayers(ref _state, rowId);
                                        chatManagement.ProcessAutoMessages(ref _state, rowId);
                                        serverManagement.ChangeGameScore(ref _state, rowId);
                                        serverManagement.GetCurrentMapIndex(ref _state, rowId);
                                        //GetCurrentMapIndex(rowId);
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
                                        instance.CurrentMap = get_currentMap(rowId, currentGameType); // always get the current map so long as the instance is not offline.
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
                                    instance.gameMapType = serverManagement.GetCurrentGameType(ref _state, rowId);
                                    instance.NextUpdateTime = currentTime.AddMilliseconds(100);
                                    instance.LastUpdateTime = currentTime;
                                    instance.Map = map;
                                    instance.TimeRemaining = timeRemainingInGame;
                                    instance.NumPlayers = currentPlayers;
                                    instance.GameTypeName = currentGameType;

                                    event_setStatusImage(rowId);

                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        event_ServerOnline();
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
                                    event_setStatusImage(rowId);
                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        event_serverOffline();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (ProgramConfig.ApplicationDebug)
                                {
                                    _state.eventLog.WriteEntry("BMTv4 TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                                    log.Error("An error occurred: " + e);
                                }
                                if (serverManagement.ProcessExist((int)instance.PID) == false)
                                {
                                    row["Time Remaining"] = "";
                                    row["Map"] = "";
                                    row["Slots"] = "";
                                    row["Game Type"] = "";
                                    instance.Status = InstanceStatus.OFFLINE;
                                    instance.TimeFlag = false;
                                    event_setStatusImage(rowId);
                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        event_serverOffline();
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
        /*
         * SetupAppState
         * - From the SQLite Database, setup the application state.
         */
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
                        RoleRestrictions = onload_convertRoleRestrictions(result.GetString(result.GetOrdinal("rolerestrictions"))),
                        WeaponRestrictions = onload_convertWeaponRestrictions(result.GetString(result.GetOrdinal("weaponrestrictions"))),
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
                        PlayerList = new Dictionary<int, ob_playerList>(),
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
                            event_getChatLogs(instanceId);
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

                    if (serverManagement.ProcessExist(instance.PID.GetValueOrDefault()))
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
                            instance.ProcessHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, instance.PID.GetValueOrDefault());
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        if (instance.ProcessHandle == null || instance.ProcessHandle == IntPtr.Zero)
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
        private autoRes SetupAutoRestart(SQLiteConnection db)
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
        /*
         * get_gameTypes
         * - Grabs the game types from the database and stores them in a dictionary.
         */
        private void get_gameTypes(SQLiteConnection conn)
        {
            var sql = "SELECT * FROM gametypes;";

            SQLiteCommand gameTypes_query = new SQLiteCommand(sql, conn);
            SQLiteDataReader gameTypes_results = gameTypes_query.ExecuteReader();
            while (gameTypes_results.Read())
            {
                gameTypes.Add(gameTypes_results.GetInt32(gameTypes_results.GetOrdinal("bitmap")),
                  new GameType
                  {
                      DatabaseId = gameTypes_results.GetInt32(gameTypes_results.GetOrdinal("id")),
                      Name = gameTypes_results.GetString(gameTypes_results.GetOrdinal("name")),
                      ShortName = gameTypes_results.GetString(gameTypes_results.GetOrdinal("shortname")),
                      Bitmap = gameTypes_results.GetInt32(gameTypes_results.GetOrdinal("bitmap"))
                  }
                );
            }
            gameTypes_query.Dispose();
            gameTypes_results.Close();
        }
        public byte[] get_imageResource(string imageName)
        {
            if (_state.imageCache.ContainsKey(imageName))
            {
                return _state.imageCache[imageName];
            }
            else
            {
                return new byte[1];
            }
        }
        private MapList get_currentMap(int rowId, string currentGameType)
        {
            MapList map = new MapList();
            int currentIndex = _state.Instances[rowId].mapIndex;
            if (!_state.Instances[rowId].MapList.ContainsKey(currentIndex))
            {
                if (!_state.Instances[rowId].previousMapList.ContainsKey(currentIndex))
                {
                    throw new Exception("Something went wrong while trying to retrieve maplists. #41");
                }
                map.MapName = _state.Instances[rowId].previousMapList[currentIndex].MapName;
                map.MapFile = _state.Instances[rowId].previousMapList[currentIndex].MapFile;
            }
            else
            {
                map.MapName = _state.Instances[rowId].MapList[currentIndex].MapName;
                map.MapFile = _state.Instances[rowId].MapList[currentIndex].MapFile;
            }
            return map;
        }

        // Main_Profilelist Form Onload Events
        private void Main_Profilelist_Load(object sender, EventArgs e)
        {
            table_profileList.Columns.Add("ID".ToString());
            table_profileList.Columns.Add("Game Name".ToString());
            table_profileList.Columns.Add("Mod".ToString(), typeof(byte[]));
            table_profileList.Columns.Add("Slots".ToString());
            table_profileList.Columns.Add("Map".ToString());
            table_profileList.Columns.Add("Game Type".ToString());
            table_profileList.Columns.Add("Time Remaining".ToString());
            table_profileList.Columns.Add("Web Stats Status".ToString());
            table_profileList.Columns.Add("Server Status".ToString(), typeof(byte[]));
            ProgramConfig.NovaStatusCheck = DateTime.Now;
            onLoad_buildProfileList();
            list_serverProfiles.DataSource = table_profileList;
            onLoad_configProfileList();

            hawkSyncDB.Open();
            SQLiteCommand warnlevelquery = new SQLiteCommand("SELECT `warnlevel` FROM `instances_config` WHERE `profile_id` = @profileid;", hawkSyncDB);
            foreach (var item in _state.Instances)
            {
                warnlevelquery.Parameters.AddWithValue("@profileid", item.Value.Id);
                SQLiteDataReader warnLevelRead = warnlevelquery.ExecuteReader();
                warnLevelRead.Read();
                _state.Instances[item.Key].BanList = onload_getBanPlayerList(item.Value.Id);
                _state.Instances[item.Key].VPNWhiteList = ipManagement.cache_loadWhitelist(item.Key, item.Value.Id, hawkSyncDB);
                _state.Instances[item.Key].GodModeList = new List<int>();
                _state.Instances[item.Key].ChangeTeamList = new List<ob_playerChangeTeamList>();
                _state.Instances[item.Key].CustomWarnings = onload_getCustomWarnings(item.Value.Id, hawkSyncDB);
                _state.Instances[item.Key].WarningQueue = new List<ob_WarnPlayerClass>();
                _state.Instances[item.Key].DisarmPlayers = new List<int>();
                _state.Instances[item.Key].WeaponRestrictions = onload_getWeaponRestrictions(item.Value.Id, hawkSyncDB);
                _state.Instances[item.Key].AutoMessages = onload_getAutoMessages(item.Value.Id, hawkSyncDB);
                _state.Instances[item.Key].previousTeams = new List<ob_playerPreviousTeam>();
                _state.Instances[item.Key].savedmaprotations = onload_getSavedMapRotations(item.Value.Id, hawkSyncDB);
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
                    IPInformation = ipManagement.cache_loadIPQuality(item.Value.Id, hawkSyncDB)
                });
                _state.ChatLogs[item.Key] = new ob_ChatLogs();
                warnLevelRead.Close();
                warnLevelRead.Dispose();
                serverManagement.SetHostnames(item.Value);
            }
            warnlevelquery.Dispose();
            _state.Users = onload_getUsersFromDB(hawkSyncDB);
            _state.yearlystats = onload_getStatsFromDB(hawkSyncDB);
            _state.adminChatMsgs = onload_getAdminChatMsgs(hawkSyncDB);
            _state.SystemInfo = GatherSystemInfo();
            _state.autoRes = SetupAutoRestart(hawkSyncDB);
            _state.playerHistories = onload_getPlayerHistories(hawkSyncDB);
            _state.adminNotes = onload_getPlayerAdminNotes(hawkSyncDB);
            _state.RCLogs = get_RCActionLogs(hawkSyncDB);

            GlobalAppState.AppState = _state;
            hawkSyncDB.Close();
            Ticker.Enabled = true;
            Ticker.Start();
            if (_state.Instances.Count == 0)
            {
                btn_start.Enabled = false;
                event_serverOffline();
            }
            else
            {
                btn_start.Enabled = true;
            }
        }

        // Main_Profilelist Form OnClose Events
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

        // Main_Profile Events
        /*
         * Event: Open Rotation Manager
         */
        private void event_OpenRotationManager(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);
            SM_RotationManager rotationManager = new SM_RotationManager(_state, id);
            rotationManager.Show();
        }
        /*
         * Event: Open User Manager
         */
        private void event_openUserManager(object sender, EventArgs e)
        {
            UserManager userManager = new UserManager(_state);
            userManager.ShowDialog();
        }
        /*
         * Event: Open Server Manager
         */
        private void event_openServerManager(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);
            SM_ServerManager server_Manager = new SM_ServerManager(_state, id);
            server_Manager.ShowDialog();
        }
        /*
         * Event: Open Options Panel
         */
        private void event_openOptionsPanel(object sender, EventArgs e)
        {
            Options op = new Options(_state);
            op.ShowDialog();
        }
        /*
         * Event: Opens the Create Profile Form and adds the profile to the list on submission.
         */
        private void event_createProfile(object sender, EventArgs e)
        {
            // capture last index.
            string img;
            string statusIMG;
            int lastIndex = _state.Instances.Count;
            Create_Profile frm = new Create_Profile(_state);
            frm.ShowDialog();
            if (Create_Profile.ProfileCreated == true)
            {
                DataRow dr = table_profileList.NewRow();
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
                dr["Mod"] = get_imageResource(img);
                dr["Server Status"] = get_imageResource(statusIMG);
                table_profileList.Rows.Add(dr);
                MessageBox.Show("Profile Added Successfully.", "Success", MessageBoxButtons.OK);
                if (_state.Instances.Count > 0) { btn_start.Enabled = true; }
            }
        }
        /*
         * Event: Edits the Selected Profile.
         */
        private void event_editProfile(object sender, EventArgs e)
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
                DataRow editRow = table_profileList.Rows[id];
                editRow["Game Name"] = _state.Instances[id].GameName;
                editRow["Mod"] = get_imageResource(img);
                editRow["Server Status"] = get_imageResource(statusIMG);
                MessageBox.Show("Profile edited successfully!", "Success");
            }
        }
        /*
         * Event: Deletes the Selected Profile from the Database and the on screen list.
         */
        private void event_deleteProfile(object sender, EventArgs e)
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
                DataRow instance = table_profileList.Rows[id];
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
                table_profileList.Rows.Remove(instance);
                if (list_serverProfiles.Rows.Count > 0)
                {
                    list_serverProfiles.Rows[0].Selected = true;
                }
            }
            else if (result == DialogResult.No) return;
        }
        /*
         * Event: Tiggered by the close button on the main form.
         */
        private void event_closeProfileList(object sender, EventArgs e)
        {
            Environment.ExitCode = 0;
            this.Close();
        }
        /*
         * Event: Start Game Server
         */
        private void event_startGameServer(object sender, EventArgs e)
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
                // Run stop ServerRC stuff
                if (!instance.PID.HasValue)
                {
                    return;
                }

                var p = Process.GetProcessById((int)instance.PID);
                p.Kill();
                p.Dispose();
                event_serverOffline();
                return;
            }

            int InstanceID = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);

            Start_Game start_Game = new Start_Game(InstanceID, _state);
            start_Game.ShowDialog();
        }
        /*
         * Event: Server Online
         * Tiggers updates to GUI buttons based on the status of the selected server.
         */
        private void event_ServerOnline()
        {
            btn_start.Text = "Stop Game";
            btn_edit.Enabled = false;
            btn_delete.Enabled = false;
            btn_serverManager.Enabled = true;
            btn_rotationManager.Enabled = list_serverProfiles.Rows.Count > 0;
        }
        /*
         * Event: Server Offline
         * Tiggers updates to GUI buttons based on the status of the selected server.
         */
        private void event_serverOffline()
        {
            btn_start.Text = "Start Game";
            btn_edit.Enabled = true;
            btn_delete.Enabled = true;
            btn_serverManager.Enabled = false;
            btn_rotationManager.Enabled = list_serverProfiles.Rows.Count > 0;
        }
        /*
         * Event: Process Player Warning
         * Process Warning Queue
         */
        private void event_processPlayerWarnings(int instanceid)
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

                    string playername = _state.Instances[instanceid].PlayerList[item.slot].name;
                    (new ServerManagement()).SendChatMessage(ref _state, instanceid, ChatManagement.ChatChannels[2], $"WARNING!!! {playername} - {item.warningMsg}");
                    _state.Instances[instanceid].WarningQueue.RemoveAt(i);

                }
            }
        }
        private void event_getChatLogs(int profileid)
        {
            //0x80
            if (_state.Instances[profileid].Status == InstanceStatus.OFFLINE || _state.Instances[profileid].Status == InstanceStatus.LOADINGMAP)
            {
                return;
            }

            string[] chatMessage = serverManagement.GetLastChatMessage(ref _state, profileid);
            int ChatLogAddr;
            int.TryParse(chatMessage[0], out ChatLogAddr);
            string LastMessage = chatMessage[1];
            string msgTypeBytes = chatMessage[2];

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
                    serverManagement.CountDownKiller(ref _state, profileid, ChatLogAddr);
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
                _state.ChatLogs[profileid].Messages.Add(new ob_PlayerChatLog
                {
                    PlayerName = PlayerName,
                    msg = PlayerMessage,
                    msgType = msgTypeString,
                    team = team,
                    dateSent = DateTime.Now
                });
                //pl_VoteSkipMap(profileid, PlayerMessage, msgTypeString, PlayerName);
                chatLog.CurrentIndex = nextIndex;
            }
            else if (msgTypeString == "Server")
            {
                _state.ChatLogs[profileid].Messages.Add(new ob_PlayerChatLog
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
                _state.ChatLogs[profileid].Messages.Add(new ob_PlayerChatLog
                {
                    PlayerName = PlayerName,
                    msg = PlayerMessage,
                    msgType = msgTypeString,
                    team = "Global",
                    dateSent = DateTime.Now
                });
                // HOOK: Skip Map
                //pl_VoteSkipMap(profileid, PlayerMessage, msgTypeString, PlayerName);
                chatLog.CurrentIndex = nextIndex;
            }
        }
        /*
         * Event: Server Status Change
         * Changes the status images.
         */
        public void event_setStatusImage(int key)
        {
            DataRow row = table_profileList.Rows[key];

            var resourceName = "";
            switch (_state.Instances[key].Status)
            {
                case InstanceStatus.ONLINE:
                    resourceName = "hosting.gif";
                    break;
                case InstanceStatus.STARTDELAY:
                    resourceName = "hosting.gif";
                    break;
                case InstanceStatus.LOADINGMAP:
                    resourceName = "loading.gif";
                    break;
                case InstanceStatus.SCORING:
                    resourceName = "scoring.gif";
                    break;
                default:
                    resourceName = "notactive.gif";
                    break;
            }
            row["Server Status"] = new byte[0];
            row["Server Status"] = get_imageResource(resourceName);
        }
        public void event_collectPlayerStats(int InstanceID)
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

        // Main_Profile On Change Events
        private void serverProfiles_SelectionChanged(object sender, EventArgs e)
        {
            if (list_serverProfiles.SelectedRows.Count == 0)
            {
                return;
            }

            var row = list_serverProfiles.SelectedRows[0];
            var instance = _state.Instances[row.Index];

            if (instance.Status == InstanceStatus.ONLINE ||
                instance.Status == InstanceStatus.SCORING ||
                instance.Status == InstanceStatus.LOADINGMAP ||
                instance.Status == InstanceStatus.STARTDELAY)
            {
                event_ServerOnline();
            }
            else if (instance.Status == InstanceStatus.OFFLINE)
            {
                event_serverOffline();
            }
        }


        // Onload Functions
        /*
         * Onload: Build Profilelist Table
         */
        private void onLoad_buildProfileList()
        {
            BindingSource bindingSource = new BindingSource
            {
                DataSource = table_profileList
            };
            for (int i = 0; i < _state.Instances.Count; i++)
            {
                string img;
                string statusIMG;
                int findInstanceIndex = bindingSource.Find("ID", _state.Instances[i].Id);
                if (findInstanceIndex == -1)
                {
                    DataRow dr = table_profileList.NewRow();
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
                    dr["Mod"] = get_imageResource(img);
                    dr["Server Status"] = get_imageResource(statusIMG);
                    table_profileList.Rows.Add(dr);
                }
            }
            bindingSource.Dispose();
        }
        private void onLoad_configProfileList()
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
        private PlayerRoles onload_convertRoleRestrictions(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                return new PlayerRoles()
                {
                    CQB = true,
                    Gunner = true,
                    Medic = true,
                    Sniper = true
                };
            }
            else
            {
                return JsonConvert.DeserializeObject<PlayerRoles>(json);
            }
        }
        private WeaponsClass onload_convertWeaponRestrictions(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                return new WeaponsClass()
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
            }
            else
            {
                return JsonConvert.DeserializeObject<WeaponsClass>(json);
            }
        }
        private List<savedmaprotations> onload_getSavedMapRotations(int id, SQLiteConnection db)
        {
            List<savedmaprotations> savedMapRotations = new List<savedmaprotations>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `instances_map_rotations` WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@profileid", id);
            SQLiteDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                List<MapList> mapCycle = read.GetString(read.GetOrdinal("mapcycle")) != "[]"
                    ? JsonConvert.DeserializeObject<List<MapList>>(read.GetString(read.GetOrdinal("mapcycle")))
                    : new List<MapList>();

                savedMapRotations.Add(new savedmaprotations
                {
                    RotationID = read.GetInt32(read.GetOrdinal("rotation_id")),
                    Description = read.GetString(read.GetOrdinal("description")),
                    mapcycle = mapCycle
                });
            }

            return savedMapRotations;
        }
        private List<adminnotes> onload_getPlayerAdminNotes(SQLiteConnection db)
        {
            List<adminnotes> adminnotesList = new List<adminnotes>();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `adminnotes`;", db))
            {
                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    while (read.Read())
                    {
                        adminnotesList.Add(new adminnotes
                        {
                            userid = read.GetInt32(read.GetOrdinal("userid")),
                            name = read.GetString(read.GetOrdinal("name")),
                            msg = read.GetString(read.GetOrdinal("msg"))
                        });
                    }
                }
            }
            return adminnotesList;
        }
        private List<ob_playerHistory> onload_getPlayerHistories(SQLiteConnection db)
        {
            List<ob_playerHistory> playerHistories = new List<ob_playerHistory>();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `playerhistory`;", db))
            {
                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    while (read.Read())
                    {
                        playerHistories.Add(new ob_playerHistory
                        {
                            DatabaseId = read.GetInt32(read.GetOrdinal("id")),
                            firstSeen = DateTime.Parse(read.GetString(read.GetOrdinal("firstseen"))),
                            playerIP = read.GetString(read.GetOrdinal("ip")),
                            playerName = read.GetString(read.GetOrdinal("playername"))
                        });
                    }
                }
            }
            return playerHistories;
        }
        private ob_AutoMessages onload_getAutoMessages(int id, SQLiteConnection db)
        {
            ob_AutoMessages AutoMessages = new ob_AutoMessages();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT `enable_msg`, `auto_msg_interval`, `auto_messages` FROM `instances_config` WHERE `profile_id` = @profileid;", db))
            {
                cmd.Parameters.AddWithValue("@profileid", id);
                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    read.Read();
                    AutoMessages.enable_msg = Convert.ToBoolean(read["enable_msg"]);
                    AutoMessages.NextMessage = DateTime.Now.AddMinutes(1.0);
                    AutoMessages.interval = read.GetInt32(read.GetOrdinal("auto_msg_interval"));
                    AutoMessages.MsgNumber = 0;
                    string autoMessagesJson = read.GetString(read.GetOrdinal("auto_messages"));
                    AutoMessages.messages = string.IsNullOrEmpty(autoMessagesJson) || autoMessagesJson == "[]"
                        ? new List<string>()
                        : JsonConvert.DeserializeObject<List<string>>(autoMessagesJson);
                }
            }
            return AutoMessages;
        }
        private WeaponsClass onload_getWeaponRestrictions(int id, SQLiteConnection db)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT `weaponrestrictions` FROM `instances_config` WHERE `profile_id` = @profileid;", db))
            {
                cmd.Parameters.AddWithValue("@profileid", id);
                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    read.Read();
                    string weaponRestrictionsJson = read.GetString(read.GetOrdinal("weaponrestrictions"));
                    return string.IsNullOrEmpty(weaponRestrictionsJson) || weaponRestrictionsJson == "[]"
                        ? new WeaponsClass()
                        : JsonConvert.DeserializeObject<WeaponsClass>(weaponRestrictionsJson);
                }
            }
        }
        private List<string> onload_getCustomWarnings(int instanceid, SQLiteConnection gametype_db)
        {
            List<string> customWarnings = new List<string>();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT `message` FROM `customwarnings` WHERE `instanceid` = @instanceid;", gametype_db))
            {
                cmd.Parameters.AddWithValue("@instanceid", instanceid);
                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    while (read.Read())
                    {
                        customWarnings.Add(read.GetString(read.GetOrdinal("message")));
                    }
                }
            }
            return customWarnings;
        }
        private List<ob_AdminChatMsgs> onload_getAdminChatMsgs(SQLiteConnection gametype_db)
        {
            List<ob_AdminChatMsgs> log = new List<ob_AdminChatMsgs>();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT `adminchatlog`.`id`, `adminchatlog`.`userid`, `users`.`username`, `adminchatlog`.`msg`, `adminchatlog`.`datesent` FROM `adminchatlog` INNER JOIN `users` ON `adminchatlog`.`userid` = `users`.`id`;", gametype_db))
            {
                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    while (read.Read())
                    {
                        log.Add(new ob_AdminChatMsgs
                        {
                            MsgID = read.GetInt32(read.GetOrdinal("id")),
                            UserID = read.GetInt32(read.GetOrdinal("userid")),
                            Username = read.GetString(read.GetOrdinal("username")),
                            Msg = read.GetString(read.GetOrdinal("msg")),
                            DateSent = DateTime.Parse(read.GetString(read.GetOrdinal("datesent")))
                        });
                    }
                }
            }
            return log;
        }
        private Dictionary<string, UserCodes> onload_getUsersFromDB(SQLiteConnection gametype_db)
        {
            var usersDB = new Dictionary<string, UserCodes>();

            using (var cmd = new SQLiteCommand("SELECT * FROM `users`;", gametype_db))
            using (var read = cmd.ExecuteReader())
            {
                while (read.Read())
                {
                    var username = read.GetString(read.GetOrdinal("username"));
                    var permissionsJson = read.GetString(read.GetOrdinal("permissions"));
                    var permissions = string.IsNullOrEmpty(permissionsJson) || permissionsJson == "[]"
                        ? new Permissions()
                        : JsonConvert.DeserializeObject<Permissions>(permissionsJson);

                    usersDB.Add(username, new UserCodes
                    {
                        UserID = read.GetInt32(read.GetOrdinal("id")),
                        Password = read.GetString(read.GetOrdinal("password")),
                        SuperAdmin = Convert.ToBoolean(read.GetInt32(read.GetOrdinal("superadmin"))),
                        SubAdmin = read.GetInt32(read.GetOrdinal("subadmin")),
                        Permissions = permissions
                    });
                }
            }

            return usersDB;
        }
        private List<ob_playerBanList> onload_getBanPlayerList(int profileid)
        {
            var playerBanList = new List<ob_playerBanList>();

            using (var db = new SQLiteConnection(ProgramConfig.DBConfig))
            {
                db.Open();
                using (var query = new SQLiteCommand("SELECT * FROM `playerbans` WHERE `profileid` = @profileid;", db))
                {
                    query.Parameters.AddWithValue("@profileid", profileid);
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bannedIP = reader.GetString(reader.GetOrdinal("ipaddress")) == "-1" ? "None" : reader.GetString(reader.GetOrdinal("ipaddress"));
                            playerBanList.Add(new ob_playerBanList
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
                        }
                    }
                }
            }

            ProgramConfig.checkExpiredBans = DateTime.Now;

            return playerBanList;
        }


        // Functions to Remove
        private Dictionary<int, monthlystats> onload_getStatsFromDB(SQLiteConnection gametype_db)
        {
            Dictionary<int, monthlystats> stats = new Dictionary<int, monthlystats>();

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM `monthlystats` WHERE `year` = @year;", gametype_db))
            {
                cmd.Parameters.AddWithValue("@year", DateTime.Now.Year);

                using (SQLiteDataReader read = cmd.ExecuteReader())
                {
                    while (read.Read())
                    {
                        int year = read.GetInt32(read.GetOrdinal("year"));
                        int month = read.GetInt32(read.GetOrdinal("month"));
                        int day = read.GetInt32(read.GetOrdinal("day"));
                        int count = read.GetInt32(read.GetOrdinal("count"));

                        if (!stats.TryGetValue(year, out var yearlyStats))
                        {
                            yearlyStats = new monthlystats { monthstat = new Dictionary<int, daystat>() };
                            stats[year] = yearlyStats;
                        }

                        if (!yearlyStats.monthstat.TryGetValue(month, out var monthlyStats))
                        {
                            monthlyStats = new daystat { daystats = new List<day>() };
                            yearlyStats.monthstat[month] = monthlyStats;
                        }

                        monthlyStats.daystats.Add(new day { Day = day, Count = count });
                    }
                }
            }

            return stats;
        }


        // RC Control Functions //
        /*
         * RC_BeginListening
         * - Start Listening for Remote Control Commands
         */
        private void RC_BeginListening()
        {
            try
            {
                WatsonTcpServer ServerRC = _state.server;
                ServerRC.Events.MessageReceived += RCEvents_MessageReceived;
                ServerRC.Events.ClientConnected += RCEvents_ClientConnected;
                ServerRC.Events.ServerStarted += RCEvents_ServerStarted;
                ServerRC.Events.ServerStopped += RCEvents_ServerStopped;
                ServerRC.Events.StreamReceived += RCEvents_StreamReceived;
                ServerRC.Events.ExceptionEncountered += RCEvents_ExceptionEncountered;
                ServerRC.Callbacks.SyncRequestReceived = RC_RunCommand;
                if (ProgramConfig.RCEnabled == true)
                {
                    ServerRC.Start();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to bind to port: " + ProgramConfig.RCPort.ToString(), "ERROR");
                log.Debug(e);
            }
        }
        private SyncResponse RC_RunCommand(SyncRequest request)
        {
            RCListener client = new RCListener(_state);
            Dictionary<object, object> metadata = new Dictionary<object, object>();

            byte[] bytes = Compression.Compress(client.BMTRemoteFunctions(numClientsRC, ref metadata, null, request));

            return new SyncResponse(request, bytes);
        }
        private void RCEvents_ExceptionEncountered(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine(e);
        }
        private void RCEvents_StreamReceived(object sender, StreamReceivedEventArgs e)
        {
            Console.WriteLine("Testing");
        }
        private void RCEvents_ServerStopped(object sender, EventArgs e)
        {
            Console.WriteLine(">> Remote Client Listener Stopped.");
        }
        private void RCEvents_ServerStarted(object sender, EventArgs e)
        {
            Console.WriteLine(">> Remote Client Listener Started.");
        }
        private void RCEvents_ClientConnected(object sender, WatsonTcp.ConnectionEventArgs e)
        {
            numClientsRC++;
        }
        private void RCEvents_MessageReceived(object sender, MessageReceivedEventArgs msg)
        {
            Dictionary<object, object> metadata = new Dictionary<object, object>();
            RCListener client = new RCListener(_state);
            client.BMTRemoteFunctions(numClientsRC, ref metadata, msg, null);
        }
        private void RC_CleanClientConnections()
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
        /*
         * Function: get_RCActionLogs
         * This function retrieves the Remote Control Action Logs from the database. This should be move to RC Management as it should get called more than once.
         */
        private List<RCLogs> get_RCActionLogs(SQLiteConnection db)
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

        // Potentially Deprecated or Junk Functions //
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
    }
}
