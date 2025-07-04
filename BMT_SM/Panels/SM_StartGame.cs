﻿using HawkSync_SM.classes.MapManagement;
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

namespace HawkSync_SM
{
    public partial class SM_StartGame : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string windowClass, string windowName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public Dictionary<int, string> maplist = new Dictionary<int, string>();
        public Dictionary<int, MapList> availableMaps = new Dictionary<int, MapList>();
        public List<CustomMap> customMaps = new List<CustomMap>();
        List<MapList> selectedMapList = new List<MapList>();
        List<MapList> selectedGameTypeMapList = new List<MapList>();
        ServerManagement serverManagerUpdateMemory = new ServerManagement();

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

        public int ArrayID;
        public int max_start_maps = 128;
        AppState _state;

        public SM_StartGame(int InstanceID, AppState state)
        {
            _state = state;
            ArrayID = InstanceID;
            InitializeComponent();
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.Items.Count > 128)
            {
                listBox2.Items.Clear();
                MessageBox.Show("It appears you've added more maps then allowed.\n\nResetting MapCycle. Please try again.\n\nError: #55", "Uh Oh!");
                listBox2.Items.Clear();
                label_mapCount.Text = $"{listBox2.Items.Count} / {max_start_maps}";
                SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
                conn.Open();
                SQLiteCommand clearmaps = new SQLiteCommand($"UPDATE `instances_config` SET `mapcycle` = '[]' WHERE `profile_id` = @profileid;", conn);
                clearmaps.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].instanceID);
                clearmaps.ExecuteNonQuery();
                clearmaps.Dispose();
                conn.Close();
                conn.Dispose();
            }
            else if (listBox2.Items.Count == 128)
            {
                MessageBox.Show("Due to limitations set by NovaLogic,\nwhen starting a server you are only allowed to choose 128 maps.\nYou can add more maps, after the server has been started\nby using the Server Manager.", "infoCurrentMapName List Error");
            }
            else
            {
                string maptype;
                switch (comboBox10.SelectedIndex)
                {
                    case 1:
                        maptype = "|TDM| ";
                        break;
                    case 2:
                        maptype = "|CP| ";
                        break;
                    case 3:
                        maptype = "|TKOTH| ";
                        break;
                    case 4:
                        maptype = "|KOTH| ";
                        break;
                    case 5:
                        maptype = "|SD| ";
                        break;
                    case 6:
                        maptype = "|AD| ";
                        break;
                    case 7:
                        maptype = "|CTF| ";
                        break;
                    case 8:
                        maptype = "|FB| ";
                        break;
                    default:
                        maptype = "|DM| ";
                        break;
                }
                // build list with current gametype selected, and use the selected Index from CB10.
                string maptypeList = "";
                foreach (var gametype in _state.autoRes.gameTypes)
                {
                    if (comboBox10.SelectedIndex == gametype.Value.DatabaseId)
                    {
                        maptypeList = gametype.Key;
                        break;
                    }
                }
                selectedMapList.Add(new MapList
                {
                    MapFile = selectedGameTypeMapList[listBox1.SelectedIndex].MapFile,
                    MapName = selectedGameTypeMapList[listBox1.SelectedIndex].MapName,
                    CustomMap = selectedGameTypeMapList[listBox1.SelectedIndex].CustomMap,
                    GameType = maptypeList
                });
                listBox2.Items.Add(maptype + listBox1.SelectedItem);
                label_mapCount.Text = $"{listBox2.Items.Count} / {max_start_maps}";
            }
        }

        private void ListBox2_DoubleClick(object sender, EventArgs e)
        {
            selectedMapList.RemoveAt(listBox2.SelectedIndex);
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            label_mapCount.Text = $"{listBox2.Items.Count} / {max_start_maps}";
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            selectedGameTypeMapList.Clear();
            foreach (var mapItem in availableMaps)
            {
                if (mapItem.Value.GameTypes.Contains(comboBox10.SelectedIndex))
                {
                    foreach (var mapList in availableMaps)
                    {
                        if (mapList.Value.MapFile == mapItem.Value.MapFile && mapList.Value.MapName == mapItem.Value.MapName)
                        {
                            selectedGameTypeMapList.Add(new MapList
                            {
                                MapFile = mapItem.Value.MapFile,
                                MapName = mapItem.Value.MapName,
                                CustomMap = mapList.Value.CustomMap
                            });
                        }
                        else
                        {
                            continue;
                        }
                    }
                    listBox1.Items.Add(mapItem.Value.MapName + " <" + mapItem.Value.MapFile + ">");
                }
                else
                {
                    continue;
                }
            }
        }
        private void btn_CloseClick(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btn_startServerClick(object sender, EventArgs e)
        {
            // Condense Variable Collection, prefer to place it right into the instance array.
            // Generate AutoRes.bin
            // Start Server
            // Attach instanceAttachedPID to Instance
            // Update Database with new settings.
            // Pass Instance Array back to main state.           

            // Database Connection
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.dbConfig);
            conn.Open();

            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one map.", "Uh Oh!");
                return;
            }

            string badMapFileNames = string.Empty;
            bool badMapsDetected = false;
            foreach (var map in selectedMapList)
            {
                if (map.MapFile.Length > 30)
                {
                    badMapsDetected = true;
                    badMapFileNames += map.MapFile + "\n";
                }
            }
            if (badMapsDetected == true)
            {
                MessageBox.Show("A few bad map file names have been detected! Please rename the files BELOW 30 characters.\n\n" + badMapFileNames, "Error");
                return;
            }

            _state.Instances[ArrayID].MapListCurrent = new Dictionary<int, MapList>();
            foreach (var map in selectedMapList)
            {
                _state.Instances[ArrayID].MapListCurrent.Add(_state.Instances[ArrayID].MapListCurrent.Count, map);
            }

            // Temporary Instance
            Instance startInstance = new Instance();
            startInstance = _state.Instances[ArrayID];

            // Collect Form Data
            startInstance.gameServerName = textBox_serverName.Text;                             // Server Name
            startInstance.gameMOTD = textBox_MOTD.Text;                                         // Server gameMOTD
            startInstance.gameCountryCode = textBox_countryCode.Text;                           // Country Code
            startInstance.gamePasswordLobby = textBox_serverPassword.Text;                           // Server Global gamePasswordLobby
            startInstance.gameSessionType = comboBox_sessionType.SelectedIndex;                 // Session Type (0 = Internet, 1 = LAN) - Doesn't work and Form Changed.
            startInstance.gameMaxSlots = Convert.ToInt32(comboBox_maxPlayers.SelectedIndex + 1);// Max Players
            startInstance.gameStartDelay = Convert.ToInt32(comboBox_startDelay.SelectedIndex);  // Start Delay
            startInstance.gameLoopMaps = checkBox_loopMaps.Checked ? 1 : 0;                     // Loop Maps
            startInstance.gameScoreFlags = comboBox_flagsScored.SelectedIndex;                     // Flag Score
            startInstance.gameScoreKills = comboBox_gameScore.SelectedIndex;                     // Game Score
            startInstance.gameScoreZoneTime = comboBox_zoneTimer.SelectedIndex;                     // Zone Timer
            startInstance.gameRespawnTime = comboBox_respawnTime.SelectedIndex;                 // Respawn Time
            startInstance.gameTimeLimit = comboBox_timeLimit.SelectedIndex;                     // Time Limit
            startInstance.gameRequireNova = checkBox_reqNova.Checked;                      // Require NovaLogic Login
            startInstance.gameDedicated = checkBox_runDedicated.Checked;                        // Run gameDedicated
            startInstance.gameWindowedMode = checkBox_windowMode.Checked;                       // Windowed Mode
            startInstance.gameCustomSkins = checkBox_customSkin.Checked;                   // Allow Custom Skins
            startInstance.profileGameMod = comboBox_gameMod.SelectedIndex;                             // Game profileGameMod
            startInstance.gamePasswordBlue = textBox_passBlue.Text;                             // Blue Team gamePasswordLobby
            startInstance.gamePasswordRed = textBox_passRed.Text;                               // Red Team gamePasswordLobby
            startInstance.gameOptionFF = checkBox_ffire.Checked;                            // Friendly Fire Flag
            startInstance.gameOptionFFWarn = checkBox_ffireWarn.Checked;                 // Friendly Fire Warning
            startInstance.gameOptionFriendlyTags = checkBox_ftags.Checked;                            // Friendly Tags
            startInstance.gameOptionAutoBalance = checkBox_autoBal.Checked;                           // Auto Balance
            startInstance.gameOptionShowTracers = checkBox_showTrace.Checked;                         // Show Tracers
            startInstance.gameShowTeamClays = checkBox_showTeamClays.Checked;                   // Show Team Clays
            startInstance.gameOptionAutoRange = checkBox_autoRange.Checked;                      // Allow Auto Range
            startInstance.gameMinPing = checkBox_minPing.Checked;                               // Enable Min Ping
            startInstance.gameMinPingValue = Convert.ToInt32(textBox_minPing.Text);             // Min Ping Value
            startInstance.gameMaxPing = checkBox_maxPing.Checked;                               // Enable Max Ping
            startInstance.gameMaxPingValue = Convert.ToInt32(textBox_maxPing.Text);             // Max Ping Value            
            startInstance.instanceLastUpdateTime = DateTime.Now;
            startInstance.instanceNextUpdateTime = DateTime.Now.AddSeconds(5.0);
            
            // Generate AutoRes.bin
            if(!(serverManagerUpdateMemory.createAutoRes(startInstance, _state)))
            {
                MessageBox.Show("Failed to create AutoRes.bin", "Error");
                return;
            }

            // Start Game
            // Check for instanceAttachedPID in Database, if not found create a record.
            SQLiteCommand checkPidQuery = new SQLiteCommand("SELECT COUNT(*) FROM `instances_pid` WHERE `profile_id` = @instanceId;", conn);
            checkPidQuery.Parameters.AddWithValue("@instanceId", _state.Instances[ArrayID].instanceID);
            int checkPid = Convert.ToInt32(checkPidQuery.ExecuteScalar());
            checkPidQuery.Dispose();

            if (checkPid == 0)
            {
                SQLiteCommand insert_cmd = new SQLiteCommand("INSERT INTO `instances_pid` (`profile_id`, `pid`) VALUES (@instanceid, 0)", conn);
                insert_cmd.Parameters.AddWithValue("@instanceid", _state.Instances[ArrayID].instanceID);
                insert_cmd.ExecuteNonQuery();
            }

            serverManagerUpdateMemory.startGame(startInstance, _state, ArrayID, conn);

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
            update_query.Parameters.AddWithValue("@flag_scores", startInstance.gameScoreFlags);
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
            update_query.Parameters.AddWithValue("@profile_id", _state.Instances[ArrayID].instanceID);
            update_query.Parameters.AddWithValue("@availablemaps", JsonConvert.SerializeObject(_state.Instances[ArrayID].MapListAvailable));
            update_query.Parameters.AddWithValue("@selected_maps", JsonConvert.SerializeObject(_state.Instances[ArrayID].MapListCurrent));
            update_query.ExecuteNonQuery();
            update_query.Dispose();


            conn.Close();
            conn.Dispose();
            
            this.Close();
           
        }

        private void checkBoxMinP_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_minPing.Checked)
            {
                textBox_minPing.Enabled = true;
            }
            else
            {
                textBox_minPing.Enabled = false;
            }
        }

        private void checkBoxMaxPing_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_maxPing.Checked)
            {
                textBox_maxPing.Enabled = true;
            }
            else
            {
                textBox_maxPing.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            selectedMapList.Clear();
            label_mapCount.Text = listBox2.Items.Count.ToString() + " / 128";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            List<MapList> newMapList = _state.autoRes.ShuffleSelectedMapList(selectedMapList);
            selectedMapList = newMapList;
            foreach (var item in newMapList)
            {
                listBox2.Items.Add("|" + item.GameType + "| " + item.MapName + " " + "<" + item.MapFile + ">");
            }
        }

        private void Start_Game_Load(object sender, EventArgs e)
        {

            (new AvailMaps()).checkAvailableMaps(_state, ArrayID);

            availableMaps = _state.Instances[ArrayID].MapListAvailable;

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
            db.Open();

            // fill slot
            int slotnum = 1;
            while (slotnum < 51)
            {
                comboBox_maxPlayers.Items.Add(slotnum);
                slotnum++;
            }
            // end fill slot
            // maxkills
            for (int maxkills = 0; maxkills < 501; maxkills++)
            {
                comboBox_gameScore.Items.Add(maxkills);
                comboBox_flagsScored.Items.Add(maxkills);
            }
            //end maxkills
            comboBox_timeLimit.Items.Insert(0, "No Limit");
            comboBox_zoneTimer.Items.Insert(0, 0);
            for (int timelimit = 1; timelimit < 61; timelimit++)
            {
                comboBox_timeLimit.Items.Add(timelimit);
                comboBox_zoneTimer.Items.Add(timelimit);
            }
            comboBox_respawnTime.Items.Insert(0, "Instant Respawn");
            for (int respawntime = 1; respawntime < 121; respawntime++)
            {
                comboBox_respawnTime.Items.Add(respawntime);
            }

            // read folder path for custom maps

            foreach (var item in _state.autoRes.gameTypes)
            {
                comboBox10.Items.Add(item.Value.Name);
            }
            comboBox10.SelectedIndex = 0;
            foreach (var map in customMaps)
            {
                if (map.gameTypeBits.Contains(2))
                {
                    if (!listBox1.Items.Contains(map.MapName + " <" + map.FileName + ">"))
                    {
                        listBox1.Items.Add(map.MapName + " <" + map.FileName + ">");
                    }
                }
            }
            textBox_serverName.Text = _state.Instances[ArrayID].gameServerName;
            textBox_MOTD.Text = _state.Instances[ArrayID].gameMOTD;
            textBox_countryCode.Text = _state.Instances[ArrayID].gameCountryCode;
            textBox_serverPassword.Text = _state.Instances[ArrayID].gamePasswordLobby;
            comboBox_sessionType.SelectedIndex = 0;
            comboBox_sessionType.Enabled = false;
            comboBox_maxPlayers.SelectedItem = _state.Instances[ArrayID].gameMaxSlots;
            comboBox_startDelay.SelectedIndex = _state.Instances[ArrayID].gameStartDelay;
            checkBox_loopMaps.Checked = Convert.ToBoolean(_state.Instances[ArrayID].gameLoopMaps);
            comboBox_gameScore.SelectedIndex = _state.Instances[ArrayID].gameScoreKills;
            comboBox_flagsScored.SelectedIndex = _state.Instances[ArrayID].gameScoreFlags;
            comboBox_zoneTimer.SelectedIndex = _state.Instances[ArrayID].gameScoreZoneTime;
            comboBox_respawnTime.SelectedIndex = _state.Instances[ArrayID].gameRespawnTime;
            comboBox_timeLimit.SelectedIndex = _state.Instances[ArrayID].gameTimeLimit;
            checkBox_reqNova.Checked = _state.Instances[ArrayID].gameRequireNova;
            checkBox_windowMode.Checked = _state.Instances[ArrayID].gameWindowedMode;
            checkBox_customSkin.Checked = _state.Instances[ArrayID].gameCustomSkins;
            checkBox_runDedicated.Checked = _state.Instances[ArrayID].gameDedicated;
            textBox_passBlue.Text = _state.Instances[ArrayID].gamePasswordBlue;
            textBox_passRed.Text = _state.Instances[ArrayID].gamePasswordRed;
            checkBox_ffire.Checked = _state.Instances[ArrayID].gameOptionFF;
            checkBox_ffireWarn.Checked = _state.Instances[ArrayID].gameOptionFFWarn;
            checkBox_ftags.Checked = _state.Instances[ArrayID].gameOptionFriendlyTags;
            checkBox_autoBal.Checked = _state.Instances[ArrayID].gameOptionAutoBalance;
            checkBox_showTrace.Checked = _state.Instances[ArrayID].gameOptionShowTracers;
            checkBox_showTeamClays.Checked = _state.Instances[ArrayID].gameShowTeamClays;
            checkBox_autoRange.Checked = _state.Instances[ArrayID].gameOptionAutoRange;
            selectedMapList = new List<MapList>();
            switch (_state.Instances[ArrayID].gameMinPing)
            {
                case false:
                    checkBox_minPing.Checked = false;
                    textBox_minPing.Text = "0";
                    break;
                case true:
                    textBox_minPing.Text = Convert.ToString(_state.Instances[ArrayID].gameMinPingValue);
                    checkBox_minPing.Checked = true;
                    break;
            }
            switch (_state.Instances[ArrayID].gameMaxPing)
            {
                case false:
                    checkBox_maxPing.Checked = false;
                    textBox_maxPing.Text = "0";
                    break;
                case true:
                    textBox_maxPing.Text = Convert.ToString(_state.Instances[ArrayID].gameMaxPingValue);
                    checkBox_maxPing.Checked = true;
                    break;
            }

            foreach (var map in _state.Instances[ArrayID].MapListCurrent)
            {
                listBox2.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " " + "<" + map.Value.MapFile + ">");
                label_mapCount.Text = $"{listBox2.Items.Count} / {max_start_maps}";
            }

            db.Close();
            db.Dispose();

            foreach (var map in _state.Instances[ArrayID].MapListCurrent)
            {
                selectedMapList.Add(map.Value);
            }

            // detect mods in each instance, and bind the results to Instance class for _state.
            List<int> detectedMods = new List<int>();
            comboBox_gameMod.Items.Add("None");
            comboBox_gameMod.SelectedIndex = 0;
            foreach (var mod in _state.Mods)
            {
                comboBox_gameMod.Items.Add(mod.Value.ModName);
            }
            foreach (var modPack in _state.Mods)
            {
                if (File.Exists(Path.Combine(_state.Instances[ArrayID].profileServerPath, modPack.Value.Pff)))
                {
                    detectedMods.Add(modPack.Value.Id);
                    comboBox_gameMod.Items.Add(modPack.Value.ModName);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SM_PopupLoadRotation loadRotation = new SM_PopupLoadRotation(_state, ArrayID);
            loadRotation.ShowDialog();
            if (SM_PopupLoadRotation._mapList.Count > 0)
            {
                listBox2.Items.Clear();
                foreach (var map in _state.Instances[ArrayID].MapListCurrent)
                {
                    listBox2.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " <" + map.Value.MapFile + ">");
                }
                selectedMapList = new List<MapList>();
                selectedMapList = SM_PopupLoadRotation._mapList;
                label_mapCount.Text = _state.Instances[ArrayID].MapListCurrent.Count + " / 128";
            }
        }

    }
}