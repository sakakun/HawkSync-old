using HawkSync_SM.classes;
using HawkSync_SM.classes.ChatManagement;
using HawkSync_SM.classes.logs;
using HawkSync_SM.RCClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HawkSync_SM.classes.StatManagement;
using WatsonTcp;
using Timer = System.Windows.Forms.Timer;
using System.ComponentModel;

namespace HawkSync_SM
{
    public partial class SM_ProfileList : Form
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
        public statsBabstats statsBabstats = new statsBabstats();

        // Object: Ticker (Used to Refresh Information x Seconds
        private Timer Ticker = new Timer()
        {
            Enabled = false,
            Interval = 100
        };

        // Server Manager - Game Profile List (Main Process)
        public SM_ProfileList(AppState state)
        {
            try
            {
                // Launch GUI
                _state = state;
                InitializeComponent();                                                                // Load GUI (Desinger Elements)

                // Ticker Checks
                Ticker.Tick += (sender, e) =>
                {
                    InstanceSeverCheck();                                                             // Instance instanceStatus Check
                    InstanceTicker();                                                                 // Instance Ticker
                    (new PlayerManagement()).checkPlayerHistory(ref _state);                          // Process Player History
                    (new PlayerManagement()).checkExpiredBans(ref _state);                            // Check for Expired Bans
                    (new HeartBeatMonitor()).ProcessHeartBeats(ref _state);                           // HeartBeat Monitor Hook
                    RC_CleanClientConnections();                                                      // Clean RC Connections               
                };

                // Server Manager Specific Events
                hawkSyncDB.Open();
                processAppState(hawkSyncDB);
                get_gameTypes(hawkSyncDB);
                RC_BeginListening();
                hawkSyncDB.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            
        }


        // Server Manager - Process Existing Functions
        private void CheckForCrashedGame(int ArrayID)
        {
            if (_state.Instances[ArrayID].instanceCrashRecovery == true)
            {
                if (DateTime.Compare(_state.Instances[ArrayID].instanceCrashCheckTime, DateTime.Now) < 0)
                {
                    if (_state.Instances[ArrayID].instanceCrashCounter < 3)
                    {
                        _state.Instances[ArrayID].instanceCrashCounter++;
                        _state.Instances[ArrayID].instanceCrashCheckTime = DateTime.Now.AddSeconds(10.0);
                        return;
                    }
                    else if (_state.Instances[ArrayID].instanceCrashCounter == 3)
                    {
                        Process[] processes = Process.GetProcesses();

                        foreach (Process prs in processes)
                        {
                            if (prs.Id == _state.Instances[ArrayID].instanceAttachedPID.GetValueOrDefault())
                            {
                                prs.Kill();
                                break;
                            }
                        }

                        string file_name = "";
                        SQLiteConnection _connection = new SQLiteConnection(ProgramConfig.DBConfig);
                        _connection.Open();
                        SQLiteCommand command = new SQLiteCommand("select `game_type` from instances WHERE id = @id;", _connection);
                        command.Parameters.AddWithValue("@id", _state.Instances[ArrayID].instanceID);
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
                        if (_state.Instances[ArrayID].profileBindIP == "0.0.0.0")
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(_state.Instances[ArrayID].profileServerPath, file_name),
                                WorkingDirectory = _state.Instances[ArrayID].profileServerPath,
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
                                WorkingDirectory = _state.Instances[ArrayID].profileServerPath,
                                Arguments = $"{_state.Instances[ArrayID].profileBindIP} \"{Path.Combine(_state.Instances[ArrayID].profileServerPath, file_name)}\" /w /LOADBAR /NOSYSDUMP /serveonly /autorestart",
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
                            if (instance.Value.instanceAttachedPID != 0)
                            {
                                currentPIDs.Add(instance.Value.instanceAttachedPID.GetValueOrDefault());
                            }
                        }
                        Process[] bhdprocesses = Process.GetProcessesByName("dfbhd");
                        foreach (var activeProcess in bhdprocesses)
                        {
                            if (!currentPIDs.Contains(activeProcess.Id) && activeProcess.StartTime > DateTime.Now.AddMinutes(-1))
                            {
                                activeProcess.MaxWorkingSet = new IntPtr(0x7fffffff);
                                _state.Instances[ArrayID].instanceAttachedPID = activeProcess.Id;
                                _state.ApplicationProcesses[ArrayID] = activeProcess;
                            }
                        }

                        SetWindowText(_state.ApplicationProcesses[ArrayID].Handle, _state.Instances[ArrayID].profileName);
                        var baseAddr = 0x400000;
                        IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _state.Instances[ArrayID].instanceAttachedPID.GetValueOrDefault());
                        _state.Instances[ArrayID].instanceProcessHandle = processHandle;

                        SQLiteCommand updatePid = new SQLiteCommand("UPDATE `instances_pid` SET `pid` = @pid WHERE `profile_id` = @profileid;", _connection);
                        updatePid.Parameters.AddWithValue("@pid", _state.Instances[ArrayID].instanceAttachedPID.GetValueOrDefault());
                        updatePid.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].instanceID);
                        updatePid.ExecuteNonQuery();
                        updatePid.Dispose();


                        if (_state.Instances[ArrayID].gameHostName != "Host")
                        {
                            int buffer = 0;
                            byte[] PointerAddr = new byte[4];
                            var Pointer = baseAddr + 0x005ED600;
                            ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                            int buffer2 = 0;
                            byte[] Hostname = Encoding.Default.GetBytes(_state.Instances[ArrayID].gameHostName + "\0");
                            var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                            MemoryProcessor.Write(_state.Instances[ArrayID], (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
                        }

                        // wait for Server to be online
                        int counter = 0;
                        while (true)
                        {
                            if (_state.Instances[ArrayID].instanceStatus == InstanceStatus.ONLINE || _state.Instances[ArrayID].instanceStatus == InstanceStatus.STARTDELAY || counter == 10)
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

                        _state.Instances[ArrayID].instanceCrashCounter = 0;
                        _state.Instances[ArrayID].instanceCrashCheckTime = DateTime.Now.AddSeconds(10.0);
                        _connection.Close();
                        _connection.Dispose();

                        _state.eventLog.WriteEntry("HawkSync has detected a Server crash!\n\nProfile Name: " + _state.Instances[ArrayID].profileName + "\nMap File: " + _state.Instances[ArrayID].MapListCurrent[_state.Instances[ArrayID].infoCurrentMapIndex + 1].MapFile + "\nMap Title: " + _state.Instances[ArrayID].MapListCurrent[_state.Instances[ArrayID].infoCurrentMapIndex + 1].MapName + "\n\nThe Server has been automatically restarted.", EventLogEntryType.Warning);
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
                if (instance.Value.instanceProcessHandle == IntPtr.Zero)
                {
                    if (serverManagement.ProcessExist(instance.Value.instanceAttachedPID.GetValueOrDefault()))
                    {
                        instance.Value.instanceProcessHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, instance.Value.instanceAttachedPID.GetValueOrDefault());
                        if (instance.Value.instanceProcessHandle == IntPtr.Zero)
                        {
                            log.Error("Failed to attach to process " + instance.Value.instanceAttachedPID.GetValueOrDefault());
                        }
                        else
                        {
                            log.Info("Attached to process " + instance.Value.instanceAttachedPID.GetValueOrDefault());
                        }
                        if (!_state.ApplicationProcesses.ContainsKey(instance.Key))
                        {
                            _state.ApplicationProcesses.Add(instance.Key, Process.GetProcessById(instance.Value.instanceAttachedPID.GetValueOrDefault()));
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
            if (_state.Instances.Count == 0 || table_profileList.Rows.Count == 0)
            {
                // No Servers to Check or the table didn't get built. Something very wrong.
                event_NoServerProfiles();
                return;
            }

            try
            {
                foreach (var item in _state.Instances)
                {
                    Instance instance = item.Value;
                    int rowId = item.Key;

                    DataRow row = table_profileList.Rows[rowId];
                    DateTime currentTime = DateTime.Now;
                    if (DateTime.Compare(instance.instanceNextUpdateTime, currentTime) < 0)
                    {
                        if (instance.instanceAttachedPID != null)
                        {
                            try
                            {
                                var PID = instance.instanceAttachedPID.GetValueOrDefault();
                                if (serverManagement.ProcessExist(instance.instanceAttachedPID.GetValueOrDefault()))
                                {
                                    event_getChatLogs(ref _state, rowId);
                                    // set process name incase the instanceAttachedPID changes...
                                    serverManagement.SetNovaID(ref _state, rowId);
                                    //SetWindowText(_state.ApplicationProcesses[rowId].MainWindowHandle, $"{instance.profileName}");

                                    int timeRemainingInGame = serverManagement.GetTimeLeft(ref _state, rowId);
                                    var map = serverManagement.GetCurrentMission(ref _state, rowId);
                                    var currentPlayers = serverManagement.GetCurrentPlayers(ref _state, rowId);
                                    var currentGameType = "";
                                    foreach (var gameTypeList in gameTypes)
                                    {
                                        var gametype = gameTypeList.Value;
                                        if (serverManagement.GetCurrentGameType(ref _state, rowId).Equals(gametype.DatabaseId))
                                        {
                                            currentGameType = gametype.ShortName;
                                            break;
                                        }
                                    }

                                    instance.instanceStatus = serverManagement.CheckStatus(ref _state, rowId);
                                    serverManagement.UpdateGlobalGameType(ref _state, rowId);
                                    // prevents loading garbage data while switching maps,
                                    // should also prevent ghosts from showing on the playerlist.
                                    if (instance.instanceStatus != InstanceStatus.LOADINGMAP)
                                    {
                                        try
                                        {
                                            // Grabs Player List & Current Player Stats
                                            instance.PlayerList = serverManagement.CurrentPlayerList(ref _state, rowId);
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                        ipManagement.CheckBans(ref _state, rowId, PID);
                                    }

                                    if (instance.WebStatsEnabled == true && instance.instanceStatus == InstanceStatus.ONLINE)
                                    {

                                        (new PlayerStatsManager()).CollectPlayerStats(_state, rowId);

                                    }

                                    // important for clearing stupid map cycle shit
                                    instance.infoMapCycleIndex = serverManagement.UpdateMapCycleCounter(ref _state, rowId);

                                    // Update Firewall IP Records Here
                                    instance.Firewall.DenyTraffic(instance.profileName, instance.profileServerPath, instance.PlayerListBans);

                                    // check for VPNs
                                    if (ProgramConfig.EnableVPNCheck == true)
                                    {
                                        ipManagement.Check4VPN(ref _state, rowId);
                                    }

                                    if (instance.instanceStatus != InstanceStatus.LOADINGMAP && instance.instanceStatus != InstanceStatus.SCORING)
                                    {
                                        event_processServerMessages(rowId);
                                    }

                                    if (instance.instanceStatus == InstanceStatus.LOADINGMAP)
                                    {
                                        if (instance.instancePostProcRun == false)
                                        {
                                            instance.instancePostProcRun = true;
                                            if (ProgramConfig.ApplicationDebug)
                                            {
                                                log.Info("Instance " + rowId + " is running the Loading Process Handler.");
                                            }
                                            PostGameProcess postGameProcess = new PostGameProcess(_state, rowId, instance.ChatLog);
                                            postGameProcess.Run();
                                            instance.ChatLog.Clear();

                                            // Reset Babstats TimeStamps
                                            if (instance.WebStatsEnabled && instance.WebStatsSoftware == 2)
                                            {
                                                instance.WebStatsBabstatsTimer = new BabstatsTimer();
                                            }
                                        }
                                        instance.infoCollectPlayerStats = true;
                                        instance.instanceScoreProcRun = false;
                                        CheckForCrashedGame(rowId);
                                    }

                                    if (instance.instanceStatus == InstanceStatus.SCORING)
                                    {
                                        if (instance.instanceScoreProcRun == false)
                                        {
                                            instance.instanceScoreProcRun = true;
                                            if (ProgramConfig.ApplicationDebug)
                                            {
                                                log.Info("Instance " + rowId + " is running the Scoring Process Handler.");
                                            }
                                            ScoringGameProcess postGameProcess = new ScoringGameProcess(_state, rowId);
                                            postGameProcess.Run();
                                        }
                                    }

                                    if (instance.instanceStatus == InstanceStatus.LOADINGMAP || instance.instanceStatus == InstanceStatus.SCORING)
                                    {
                                        serverManagement.ChangePlayerTeam(ref _state, rowId);
                                    }

                                    if (instance.instanceStatus == InstanceStatus.STARTDELAY || instance.instanceStatus == InstanceStatus.ONLINE)
                                    {
                                        instance.instanceCrashCounter = 0;
                                        serverManagement.ResetGodMode(ref _state, rowId);
                                        serverManagement.ProcessDisarmedPlayers(ref _state, rowId);
                                        chatManagement.ProcessAutoMessages(ref _state, rowId);
                                        serverManagement.ChangeGameScore(ref _state, rowId);
                                        serverManagement.GetCurrentMapIndex(ref _state, rowId);
                                        if (instance.instancePostProcRun == true)
                                        {
                                            instance.instancePostProcRun = false;
                                        }
                                        if (instance.instanceScoreProcRun == true)
                                        {
                                            instance.instanceScoreProcRun = false;
                                        }
                                        
                                    }

                                    if (instance.instanceStatus != InstanceStatus.OFFLINE)
                                    {
                                        instance.infoCurrentMap = get_currentMap(rowId, currentGameType); // always get the current map so long as the instance is not offline.
                                    }

                                    // Babstats Tick Process
                                    if (instance.WebStatsEnabled && instance.WebStatsSoftware == 2)
                                    {
                                        BabstatsTimer babstatsTime = instance.WebStatsBabstatsTimer;
                                        // if DateTime.Now is 20 seconds greater than babstatsTime.updateTimeStamp
                                        if(babstatsTime.updateTimeStamp.AddSeconds(20) < DateTime.Now)
                                        {
                                            statsBabstats.sendBabstatsUpdateData(_state, rowId);
                                            babstatsTime.updateTimeStamp = DateTime.Now;
                                        }
                                        if(babstatsTime.reportTimeStamp.AddSeconds(60) < DateTime.Now)
                                        {
                                            statsBabstats.requestBabstatsReportData(_state, rowId);
                                            babstatsTime.reportTimeStamp = DateTime.Now;
                                        }

                                    }

                                    switch (timeRemainingInGame)
                                    {
                                        case 0:
                                            row["Time Remaining"] = "< 1 Minute";
                                            instance.infoMarkerTime = true;
                                            break;
                                        case 1:
                                            row["Time Remaining"] = timeRemainingInGame + " Minute";
                                            instance.infoMarkerTime = false;
                                            break;
                                        default:
                                            row["Time Remaining"] = timeRemainingInGame + " Minutes";
                                            instance.infoMarkerTime = false;
                                            break;
                                    }

                                    row["infoCurrentMapName"] = map;
                                    row["Slots"] = currentPlayers + "/" + instance.gameMaxSlots;
                                    row["Game Type"] = currentGameType;
                                    instance.infoMapGameType = serverManagement.GetCurrentGameType(ref _state, rowId);
                                    instance.instanceNextUpdateTime = currentTime.AddMilliseconds(100);
                                    instance.instanceLastUpdateTime = currentTime;
                                    instance.infoCurrentMapName = map;
                                    instance.infoMapTimeRemaining = timeRemainingInGame;
                                    instance.infoNumPlayers = currentPlayers;
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
                                    row["infoCurrentMapName"] = "";
                                    row["Slots"] = "";
                                    row["Game Type"] = "";
                                    instance.instanceStatus = InstanceStatus.OFFLINE;
                                    instance.infoMarkerTime = false;
                                    event_setStatusImage(rowId);
                                    if (list_serverProfiles.SelectedRows[0].Index == rowId)
                                    {
                                        event_serverOffline();
                                    }
                                }
                            }
                            catch (KeyNotFoundException ex)
                            {
                                // Handle the exception
                                Console.WriteLine("ArgumentNullException occurred:");
                                Console.WriteLine($"Parameter name: {ex.Message}");
                                Console.WriteLine($"Message: {ex.StackTrace}");
                                // You can also access other properties of the exception object if needed
                            }
                            catch (Exception e)
                            {
                                if (ProgramConfig.ApplicationDebug)
                                {
                                    _state.eventLog.WriteEntry("HawkSync TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                                    log.Error("An error occurred: " + e);
                                }
                                if (serverManagement.ProcessExist((int)instance.instanceAttachedPID) == false)
                                {
                                    row["Time Remaining"] = "";
                                    row["infoCurrentMapName"] = "";
                                    row["Slots"] = "";
                                    row["Game Type"] = "";
                                    instance.instanceStatus = InstanceStatus.OFFLINE;
                                    instance.infoMarkerTime = false;
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
                _state.eventLog.WriteEntry("HawkSync has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                log.Debug(e);
                return;
            }
        }
        /* 
         * ProcessAppState
         */
        private void processAppState(SQLiteConnection db)
        {
            try
            {
                // SQL query to fetch data from the database
                string sql = @"SELECT i.*, pi.pid, ic.* FROM instances i 
                            LEFT JOIN instances_pid pi ON i.id = pi.profile_id 
                            LEFT JOIN instances_config ic ON i.id = ic.profile_id;";

                // Execute the SQL command using SQLiteCommand
                using (SQLiteCommand command = new SQLiteCommand(sql, db))
                {
                    // Execute the query and retrieve data using SQLiteDataReader
                    using (SQLiteDataReader result = command.ExecuteReader())
                    {
                        // Loop through each row in the result set
                        while (result.Read())
                        {
                            // Check for TeamSabre
                            bool TeamSabre = result.GetInt32(result.GetOrdinal("game_type")) == 0 && File.Exists(result.GetString(result.GetOrdinal("gamepath")) + "\\EXP1.pff");

                            // Initialize WebstatsURL and WebStatsEnabled
                            string WebstatsURL = "";
                            bool EnableWebStats;

                            // Switch case for setting WebstatsURL and WebStatsEnabled
                            switch (result.GetInt32(result.GetOrdinal("stats")))
                            {
                                case 1:
                                case 2:
                                    WebstatsURL = result.GetString(result.GetOrdinal("stats_url"));
                                    EnableWebStats = true;
                                    break;
                                default:
                                    EnableWebStats = false;
                                    break;
                            }

                            // Deserialize mapList and MapListAvailable
                            Dictionary<int, MapList> mapList = result.GetString(result.GetOrdinal("mapcycle")) != "[]" ? JsonConvert.DeserializeObject<Dictionary<int, MapList>>(result.GetString(result.GetOrdinal("mapcycle"))) : new Dictionary<int, MapList>();
                            Dictionary<int, MapList> availableMaps = result.GetString(result.GetOrdinal("availablemaps")) != "[]" ? JsonConvert.DeserializeObject<Dictionary<int, MapList>>(result.GetString(result.GetOrdinal("availablemaps"))) : new Dictionary<int, MapList>();

                            // Deserialize plugins or create default PluginsClass
                            PluginsClass plugins = result.GetString(result.GetOrdinal("plugins")) == "[]" ? new PluginsClass { /* default values */ } : JsonConvert.DeserializeObject<PluginsClass>(result.GetString(result.GetOrdinal("plugins")));

                            // Create Instance object and assign properties
                            Instance instance = new Instance
                            {
                                // Assigning properties...
                                instanceID = result.GetInt32(result.GetOrdinal("id")),
                                ChatLog = new List<ob_PlayerChatLog>(),
                                profileServerPath = result.GetString(result.GetOrdinal("gamepath")),
                                profileServerType = result.GetInt32(result.GetOrdinal("game_type")),
                                profileName = result.GetString(result.GetOrdinal("name")),
                                gameHostName = result.GetString(result.GetOrdinal("host_name")),
                                gameCountryCode = result.GetString(result.GetOrdinal("country_code")),
                                gameServerName = result.GetString(result.GetOrdinal("server_name")),
                                gamePasswordLobby = result.GetString(result.GetOrdinal("server_password")),
                                gameMOTD = result.GetString(result.GetOrdinal("motd")),
                                gameDedicated = result.GetBoolean(result.GetOrdinal("run_dedicated")),
                                gameSessionType = result.GetInt32(result.GetOrdinal("session_type")),
                                gameMaxSlots = result.GetInt32(result.GetOrdinal("max_slots")),
                                gameScoreKills = result.GetInt32(result.GetOrdinal("game_score")),
                                gameScoreFlags = result.GetInt32(result.GetOrdinal("fbscore")),
                                gameScoreZoneTime = result.GetInt32(result.GetOrdinal("kothscore")),
                                gameWindowedMode = result.GetBoolean(result.GetOrdinal("windowed_mode")),
                                gameLoopMaps = result.GetInt32(result.GetOrdinal("loop_maps")),
                                gameRespawnTime = result.GetInt32(result.GetOrdinal("respawn_time")),
                                gameRequireNova = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("require_novalogic"))),
                                gameCustomSkins = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("allow_custom_skins"))),
                                profileBindIP = result.GetString(result.GetOrdinal("bind_address")),
                                profileBindPort = result.GetInt32(result.GetOrdinal("port")),
                                gameTimeLimit = result.GetInt32(result.GetOrdinal("time_limit")),
                                gameStartDelay = result.GetInt32(result.GetOrdinal("start_delay")),
                                gameMinPing = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("enable_min_ping"))),
                                gameMinPingValue = result.GetInt32(result.GetOrdinal("min_ping")),
                                gameMaxPing = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("enable_max_ping"))),
                                gameMaxPingValue = result.GetInt32(result.GetOrdinal("max_ping")),
                                gameOneShotKills = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("oneshotkills"))),
                                gameFatBullets = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("fatbullets"))),
                                gameDestroyBuildings = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("destroybuildings"))),
                                gameOptionFF = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("friendly_fire"))),
                                gameOptionFFWarn = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("friendly_fire_warning"))),
                                gameOptionFriendlyTags = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("friendly_tags"))),
                                gameFriendlyFireKills = result.GetInt32(result.GetOrdinal("friendly_fire_kills")),
                                gamePSPTOTimer = result.GetInt32(result.GetOrdinal("psptakeover")),
                                gameOptionAutoRange = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("allow_auto_range"))),
                                gameOptionAutoBalance = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("auto_balance"))),
                                gamePasswordBlue = result.GetString(result.GetOrdinal("blue_team_password")),
                                gamePasswordRed = result.GetString(result.GetOrdinal("red_team_password")),
                                gameFlagReturnTime = result.GetInt32(result.GetOrdinal("flagreturntime")),
                                gameMaxTeamLives = result.GetInt32(result.GetOrdinal("max_team_lives")),
                                gameShowTeamClays = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("show_team_clays"))),
                                gameOptionShowTracers = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("show_tracers"))),
                                RoleRestrictions = onload_convertRoleRestrictions(result.GetString(result.GetOrdinal("rolerestrictions"))),
                                WeaponRestrictions = onload_convertWeaponRestrictions(result.GetString(result.GetOrdinal("weaponrestrictions"))),
                                instanceLastUpdateTime = DateTime.Now,
                                instanceNextUpdateTime = DateTime.Now.AddSeconds(2.0),
                                WebStatsEnabled = EnableWebStats,
                                vpnCheckEnabled = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("enableVPNcheck"))),
                                WebStatsSoftware = result.GetInt32(result.GetOrdinal("stats")),
                                WebstatsURL = WebstatsURL,
                                MapListAvailable = availableMaps,
                                MapListCurrent = mapList,
                                infoCounterMaps = mapList.Count,
                                WebstatsVerified = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("stats_verified"))),
                                instanceStatus = InstanceStatus.OFFLINE,
                                WebStatsASPEnabled = result.GetInt32(result.GetOrdinal("anti_stat_padding")),
                                WebStatsASPMinMinutes = result.GetInt32(result.GetOrdinal("anti_stat_padding_min_minutes")),
                                WebStatsASPMinPlayers = result.GetInt32(result.GetOrdinal("anti_stat_padding_min_players")),
                                instanceCrashRecovery = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("misc_crashrecovery"))),
                                WebStatsAnnouncements = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("misc_show_ranks"))),
                                gameAllowLeftLeaning = result.GetInt32(result.GetOrdinal("misc_left_leaning")),
                                WebStatsProfileID = result.GetOrdinal("stats_server_id").ToString(),
                                PlayerList = new Dictionary<int, ob_playerList>(),
                                MapListPrevious = new Dictionary<int, MapList>(),
                                gameScoreBoardDelay = result.GetInt32(result.GetOrdinal("scoreboard_override")),
                                ReportNovaHQ = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("novahq_master"))),
                                ReportNovaCC = Convert.ToBoolean(result.GetInt32(result.GetOrdinal("novacc_master"))),
                                Plugins = plugins,
                                VoteMapStandBy = true,
                                infoTeamSabre = TeamSabre
                            };

                            // Initialize infoCollectPlayerStats
                            //var infoCollectPlayerStats = new CollectedPlayerStatsPlayers { Player = new Dictionary<string, CollectedPlayerStats>() };

                            // Get instanceId
                            int instanceId = _state.Instances.Count;

                            // Add Timer for chat handling
                            /*
                            _state.ChatHandlerTimer.Add(instanceId, new Timer { Enabled = true, Interval = 1 });
                            _state.ChatHandlerTimer[instanceId].Tick += (sender, e) =>
                            {
                                
                            };
                            */

                            // Set instanceAttachedPID if not null
                            if (!result.IsDBNull(result.GetOrdinal("pid")))
                            {
                                instance.instanceAttachedPID = result.GetInt32(result.GetOrdinal("pid"));
                                _state.eventLog.WriteEntry("Found instanceAttachedPID: " + instance.instanceAttachedPID.GetValueOrDefault(), EventLogEntryType.Information);
                            }

                            // Check if process exists and attach if so
                            if (serverManagement.ProcessExist(instance.instanceAttachedPID.GetValueOrDefault()))
                            {
                                // Attempt to attach to process
                                _state.eventLog.WriteEntry("Attempting to attach... ID: " + instance.instanceID, EventLogEntryType.Information);
                                if (!_state.ApplicationProcesses.ContainsKey(_state.Instances.Count))
                                {
                                    if (Process.GetProcessById(instance.instanceAttachedPID.GetValueOrDefault()).ProcessName == "dfbhd")
                                    {
                                        _state.ApplicationProcesses.Add(_state.Instances.Count, Process.GetProcessById(instance.instanceAttachedPID.GetValueOrDefault()));
                                    }
                                }
                                try
                                {
                                    instance.instanceProcessHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, instance.instanceAttachedPID.GetValueOrDefault());
                                }
                                catch (Exception e)
                                {
                                    throw e;
                                }
                                // Check if process handle is valid
                                if (instance.instanceProcessHandle == null || instance.instanceProcessHandle == IntPtr.Zero)
                                {
                                    _state.eventLog.WriteEntry("Unable to attach to process...", EventLogEntryType.Error, 0, 0);
                                    throw new Exception("Unable to attach to process...");
                                }
                            }
                            else
                            {
                                _state.eventLog.WriteEntry("Could not find instanceAttachedPID: " + instance.instanceAttachedPID.GetValueOrDefault(), EventLogEntryType.Warning);
                            }

                            _state.eventLog.WriteEntry("Attachment successful: " + instance.instanceID, EventLogEntryType.Information);
                            _state.eventLog.WriteEntry("Adding instance: " + instance.instanceID, EventLogEntryType.Information);

                            // Initialize ConsoleQueue and add starting rowObj
                            _state.ConsoleQueue.Add(_state.Instances.Count, new ConsoleQueue { queue = new List<Queue>(), nextCmd = DateTime.Now.AddSeconds(10) });
                            _state.ConsoleQueue[_state.Instances.Count].queue.Add(new Queue { text = "HawkSync is starting...", Type = ConsoleQueueType.MESSAGE, color = ChatColor.NORMAL });

                            // Add instance to AppState
                            _state.Instances.Add(_state.Instances.Count, instance);
                            //_state.PlayerStats.Add(_state.PlayerStats.Count, infoCollectPlayerStats);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Log error
                _state.eventLog.WriteEntry("HawkSync TV has detected an error!\n\n" + e, EventLogEntryType.Error);
                log.Debug(e);
                Console.WriteLine(e);
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
            int currentIndex = _state.Instances[rowId].infoCurrentMapIndex;
            if (!_state.Instances[rowId].MapListCurrent.ContainsKey(currentIndex))
            {
                if (!_state.Instances[rowId].MapListPrevious.ContainsKey(currentIndex))
                {
                    throw new Exception("Something went wrong while trying to retrieve maplists. #41");
                }
                map.MapName = _state.Instances[rowId].MapListPrevious[currentIndex].MapName;
                map.MapFile = _state.Instances[rowId].MapListPrevious[currentIndex].MapFile;
            }
            else
            {
                map.MapName = _state.Instances[rowId].MapListCurrent[currentIndex].MapName;
                map.MapFile = _state.Instances[rowId].MapListCurrent[currentIndex].MapFile;
            }
            return map;
        }

        // Main_Profilelist Form Onload Events ( Runs Once on Window Load )
        private void Main_Profilelist_Load(object sender, EventArgs e)
        {
            table_profileList.Columns.Add("ID".ToString());
            table_profileList.Columns.Add("Game Name".ToString());
            table_profileList.Columns.Add("profileGameMod".ToString(), typeof(byte[]));
            table_profileList.Columns.Add("Slots".ToString());
            table_profileList.Columns.Add("infoCurrentMapName".ToString());
            table_profileList.Columns.Add("Game Type".ToString());
            table_profileList.Columns.Add("Time Remaining".ToString());
            table_profileList.Columns.Add("Web Stats instanceStatus".ToString());
            table_profileList.Columns.Add("Server instanceStatus".ToString(), typeof(byte[]));
            ProgramConfig.NovaStatusCheck = DateTime.Now;
            onLoad_buildProfileList();
            list_serverProfiles.DataSource = table_profileList;
            onLoad_configProfileList();

            hawkSyncDB.Open();

            // Load Core IP Configurations
            ipManagement.initIPManagement_ProgramConfig(hawkSyncDB);

            SQLiteCommand warnlevelquery = new SQLiteCommand("SELECT `warnlevel` FROM `instances_config` WHERE `profile_id` = @profileid;", hawkSyncDB);
            foreach (var item in _state.Instances)
            {
                warnlevelquery.Parameters.AddWithValue("@profileid", item.Value.instanceID);
                SQLiteDataReader warnLevelRead = warnlevelquery.ExecuteReader();
                warnLevelRead.Read();
                _state.Instances[item.Key].PlayerListBans = onload_getBanPlayerList(item.Value.instanceID);
                _state.Instances[item.Key].IPWhiteList = ipManagement.cache_loadWhitelist(item.Key, item.Value.instanceID, hawkSyncDB);
                _state.Instances[item.Key].PlayerListGodMod = new List<int>();
                _state.Instances[item.Key].TeamListChange = new List<ob_playerChangeTeamList>();
                _state.Instances[item.Key].CustomWarnings = onload_getCustomWarnings(item.Value.instanceID, hawkSyncDB);
                _state.Instances[item.Key].ServerMessagesQueue = new List<ob_ServerMessageQueue>();
                _state.Instances[item.Key].PlayerListDisarm = new List<int>();
                _state.Instances[item.Key].WeaponRestrictions = onload_getWeaponRestrictions(item.Value.instanceID, hawkSyncDB);
                _state.Instances[item.Key].AutoMessages = onload_getAutoMessages(item.Value.instanceID, hawkSyncDB);
                _state.Instances[item.Key].TeamListPrevious = new List<ob_playerPreviousTeam>();
                _state.Instances[item.Key].MapListRotationDB = onload_getSavedMapRotations(item.Value.instanceID, hawkSyncDB);
                _state.Instances[item.Key].infoCurrentMap = new MapList();
                _state.Instances[item.Key].WelcomeQueue = new List<WelcomePlayer>();
                _state.Instances[item.Key].VoteMapsTally = new List<VoteMapsTally>();
                _state.Instances[item.Key].VoteMapTimer = new Timer
                {
                    Enabled = false
                };
                _state.IPQualityCache.Add(item.Key, new ipqualityscore
                {
                    WarnLevel = warnLevelRead.GetInt32(warnLevelRead.GetOrdinal("warnlevel")),
                    IPInformation = ipManagement.cache_loadIPQuality(item.Value.instanceID, hawkSyncDB)
                });
                warnLevelRead.Close();
                warnLevelRead.Dispose();
                serverManagement.SetHostnames(item.Value);
            }
            warnlevelquery.Dispose();

            _state.adminNotes = onload_getPlayerAdminNotes(hawkSyncDB);
            _state.playerHistories = onload_getPlayerHistories(hawkSyncDB);
            _state.RCLogs = get_RCActionLogs(hawkSyncDB);
            _state.Users = onload_getUsersFromDB(hawkSyncDB);

            _state.adminChatMsgs = onload_getAdminChatMsgs(hawkSyncDB);
            _state.SystemInfo = GatherSystemInfo();
            _state.autoRes = SetupAutoRestart(hawkSyncDB);

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
            SM_Options op = new SM_Options(_state);
            op.ShowDialog();
        }
        /*
         * Event: Opens the Create Profile Form and adds the profile to the list on submission.
         */
        private void event_createProfile(object sender, EventArgs e)
        {
            // capture last index.
            string img;
            int lastIndex = _state.Instances.Count;
            SM_ServerProfile createProfile = new SM_ServerProfile(_state, lastIndex);
            createProfile.ShowDialog();
            if (createProfile.profileUpdated == true)
            {
                // Create Instance
                createProfile.event_addProfile(ref _state, lastIndex);

                // Add Instance to the Profile Table
                DataRow dr = table_profileList.NewRow();
                if (_state.Instances[lastIndex].profileServerType == 1)
                {
                    img = "jo.gif";
                }
                else if (_state.Instances[lastIndex].profileServerType == 0)
                {
                    img = _state.Instances[lastIndex].infoTeamSabre ? "bhdts.gif" : "bhd.gif";
                }
                else
                {
                    img = "bhd.gif";
                }
                dr["ID"] = _state.Instances[lastIndex].instanceID;
                dr["Game Name"] = _state.Instances[lastIndex].profileName;
                dr["profileGameMod"] = get_imageResource(img);
                dr["Server instanceStatus"] = get_imageResource("notactive.gif");
                table_profileList.Rows.Add(dr);
                event_setStatusImage(lastIndex);

                createProfile.Close();
                MessageBox.Show("Profile Added Successfully.", "Success", MessageBoxButtons.OK);
                if (_state.Instances.Count > 0)
                {
                    list_serverProfiles.Rows[lastIndex].Selected = true;
                    btn_start.Enabled = true;
                }
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
            SM_ServerProfile editProfile = new SM_ServerProfile(_state, id);
            editProfile.ShowDialog();
            if (editProfile.profileUpdated == true)
            {

                editProfile.event_editProfile(ref _state, id);

                statusIMG = "notactive.gif";
                if (_state.Instances[id].profileServerType == 0 && _state.Instances[id].infoTeamSabre == true)
                {
                    img = "bhdts.gif";
                }
                else if (_state.Instances[id].profileServerType == 0 && _state.Instances[id].infoTeamSabre == false)
                {
                    img = "bhd.gif";
                }
                else if (_state.Instances[id].profileServerType == 1)
                {
                    img = "jo.gif";
                }
                else
                {
                    img = "bhd.gif";
                }
                DataRow editRow = table_profileList.Rows[id];
                editRow["Game Name"] = _state.Instances[id].profileName;
                editRow["profileGameMod"] = get_imageResource(img);
                editRow["Server instanceStatus"] = get_imageResource(statusIMG);

                editProfile.Close();
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
                try
                {
                    Ticker.Stop();
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    using (SQLiteCommand command = db.CreateCommand())
                    {
                        command.CommandText = @"DELETE FROM `chatlog` WHERE `profile_id` = @profileid;
                                DELETE FROM `customwarnings` WHERE `instanceid` = @profileid;
                                DELETE FROM `instances` WHERE id = @profileid;
                                DELETE FROM `instances_config` WHERE profile_id = @profileid;
                                DELETE FROM `instances_map_rotations` WHERE `profile_id` = @profileid;
                                DELETE FROM `instances_pid` WHERE profile_id = @profileid;
                                DELETE FROM `ipqualitycache` WHERE `profile_id` = @profileid;
                                DELETE FROM `playerbans` WHERE `profileid` = @profileid;
                                DELETE FROM `vpnwhitelist` WHERE `profile_id` = @profileid;";
                        command.Parameters.AddWithValue("@profileid", _state.Instances[id].instanceID);
                        Console.WriteLine(command.Parameters);
                        command.ExecuteNonQuery();

                        _state.Instances.Remove(id);
                        _state.IPQualityCache.Remove(id);
                        table_profileList.Rows.Remove(instance);

                        if (list_serverProfiles.Rows.Count > 0)
                        {
                            list_serverProfiles.Rows[0].Selected = true;
                        }
                    }
                    db.Close();
                    Ticker.Start();
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error");
                }

            }
            else if (result == DialogResult.No) return;

        }
        /*
         * Event: Tiggered by the close button on the main form.
         */
        private void event_closeProfileList(object sender, EventArgs e)
        {
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
            if (instance.instanceStatus != InstanceStatus.OFFLINE)
            {
                // Run stop Server stuff
                if (!instance.instanceAttachedPID.HasValue)
                {
                    return;
                }

                var p = Process.GetProcessById((int)instance.instanceAttachedPID);
                p.Kill();
                p.Dispose();
                event_serverOffline();

                // Call regardless.
                instance.Firewall.DeleteFirewallRules(instance.profileName, "Allow");
                instance.Firewall.DeleteFirewallRules(instance.profileName, "Deny");

                return;
            }

            int InstanceID = Convert.ToInt32(list_serverProfiles.CurrentCell.RowIndex);

            SM_StartGame StartGame = new SM_StartGame(InstanceID, _state);
            StartGame.ShowDialog();
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
        private void event_NoServerProfiles()
        {
            btn_start.Text = "Start Game";
            btn_start.Enabled = false;
            btn_edit.Enabled = false;
            btn_delete.Enabled = false;
            btn_serverManager.Enabled = false;
            btn_rotationManager.Enabled = false;
        }
        /*
         * Event: Process Player Warning
         * Process Warning Queue
         */
        private void event_processServerMessages(int instanceid)
        {
            var serverMessages = _state.Instances[instanceid].ServerMessagesQueue;
            var playerList = _state.Instances[instanceid].PlayerList;
            int channel = 0;
            string serverMessage = string.Empty;
            // Process Messages
            foreach (var rowObj in serverMessages)
            {
                channel = 0; serverMessage = rowObj.message;
                switch (rowObj.slot)
                {
                    case 90:
                        channel = 1; 
                        break;
                    case 91:
                        channel = 2;
                        break;
                    default:
                        serverMessage = $"WARNING!!! {playerList[rowObj.slot].name} - {rowObj.message}";
                        break;
                }              

                (new ServerManagement()).SendChatMessage(ref _state, instanceid, ChatManagement.ChatChannels[channel], serverMessage);
            }
            // Clear Messages
            serverMessages.Clear();
        }

        private void event_getChatLogs(ref AppState _state, int profileid)
        {
            
            try
            {
                //0x80
                if (_state.Instances[profileid].instanceStatus == InstanceStatus.OFFLINE || _state.Instances[profileid].instanceStatus == InstanceStatus.LOADINGMAP)
                {
                    return;
                }

                string[] chatMessage = serverManagement.GetLastChatMessage(ref _state, profileid);
                int ChatLogAddr;
                int.TryParse(chatMessage[0], out ChatLogAddr);
                string LastMessage = chatMessage[1];
                string msgTypeBytes = chatMessage[2];

                // Check for valid next chat rowObj
                if (LastMessage == "")
                {
                    return;
                }
                var chatLog = _state.Instances[profileid].ChatLog;
                var nextIndex = chatLog.Count;
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

                if (chatLog.Count != 0)
                {
                    string LastPlayerName = chatLog[chatLog.Count - 1].PlayerName;
                    string LastPlayerMsg = chatLog[chatLog.Count - 1].msg;
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

                int teamNum = 3;

                foreach (var item in _state.Instances[profileid].PlayerList)
                {
                    if (item.Value.name == PlayerName)
                    {
                        teamNum = _state.Instances[profileid].PlayerList[item.Key].slot;
                        break;
                    }

                }

                string[] teamNames = new string[] { "Self", "Blue", "Red", "Host" };

                _state.Instances[profileid].ChatLog.Add(new ob_PlayerChatLog
                {
                    PlayerName = PlayerName,
                    msg = PlayerMessage,
                    msgType = msgTypeString,
                    team = teamNames[teamNum],
                    dateSent = DateTime.Now
                });

            } catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
            }            

        }
        /*
         * Event: Server instanceStatus Change
         * Changes the status images.
         */
        public void event_setStatusImage(int key)
        {
            DataRow row = table_profileList.Rows[key];

            var resourceName = "";
            switch (_state.Instances[key].instanceStatus)
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
            row["Server instanceStatus"] = new byte[0];
            row["Server instanceStatus"] = get_imageResource(resourceName);

            table_profileList.Rows.CopyTo(row.ItemArray, key);
        }


        // Main_Profile On Change Events
        private void serverProfiles_SelectionChanged(object sender, EventArgs e)
        {
            int intCount = 0;
            while (intCount < _state.Instances.Count)
            {
                if (_state.Instances[intCount].instanceStatus == InstanceStatus.ONLINE ||
                    _state.Instances[intCount].instanceStatus == InstanceStatus.SCORING ||
                    _state.Instances[intCount].instanceStatus == InstanceStatus.LOADINGMAP ||
                    _state.Instances[intCount].instanceStatus == InstanceStatus.STARTDELAY)
                {
                    event_ServerOnline();
                }
                else if (_state.Instances[intCount].instanceStatus == InstanceStatus.OFFLINE)
                {
                    event_serverOffline();
                }
                intCount++;
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
                int findInstanceIndex = bindingSource.Find("ID", _state.Instances[i].instanceID);
                if (findInstanceIndex == -1)
                {
                    DataRow dr = table_profileList.NewRow();
                    statusIMG = "notactive.gif";
                    if (_state.Instances[i].profileServerType == 0 && _state.Instances[i].infoTeamSabre == true)
                    {
                        img = "bhdts.gif";
                    }
                    else if (_state.Instances[i].profileServerType == 0 && _state.Instances[i].infoTeamSabre == false)
                    {
                        img = "bhd.gif";
                    }
                    else
                    {
                        img = "bhd.gif";
                    }
                    Console.WriteLine($"{dr["Game Name"]}");
                    dr["ID"] = _state.Instances[i].instanceID;
                    dr["Game Name"] = _state.Instances[i].profileName;
                    dr["profileGameMod"] = get_imageResource(img);
                    dr["Server instanceStatus"] = get_imageResource(statusIMG);
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
            list_serverProfiles.Columns["profileGameMod"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["profileGameMod"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Slots"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Slots"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["infoCurrentMapName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["infoCurrentMapName"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Game Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Game Type"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Time Remaining"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Time Remaining"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Web Stats instanceStatus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Web Stats instanceStatus"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            list_serverProfiles.Columns["Server instanceStatus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            list_serverProfiles.Columns["Server instanceStatus"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
                        customWarnings.Add(read.GetString(read.GetOrdinal("rowObj")));
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

        // RC Control Functions //
        /*
         * RC_BeginListening
         * - Start Listening for Remote Control Commands
         */
        private void RC_BeginListening()
        {
            try
            {
                WatsonTcpServer Server = _state.server;
                Server.Events.MessageReceived += RCEvents_MessageReceived;
                Server.Events.ClientConnected += RCEvents_ClientConnected;
                Server.Events.ServerStarted += RCEvents_ServerStarted;
                Server.Events.ServerStopped += RCEvents_ServerStopped;
                Server.Events.StreamReceived += RCEvents_StreamReceived;
                Server.Events.ExceptionEncountered += RCEvents_ExceptionEncountered;
                Server.Callbacks.SyncRequestReceived = RC_RunCommand;
                if (ProgramConfig.RCEnabled == true)
                {
                    Server.Start();
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
