using HawkSync_SM.classes.MapManagement;
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
    public partial class Start_Game : Form
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

        public Start_Game(int InstanceID, AppState state)
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
                label33.Text = $"{listBox2.Items.Count} / {max_start_maps}";
                SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                conn.Open();
                SQLiteCommand clearmaps = new SQLiteCommand($"UPDATE `instances_config` SET `mapcycle` = '[]' WHERE `profile_id` = @profileid;", conn);
                clearmaps.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                clearmaps.ExecuteNonQuery();
                clearmaps.Dispose();
                conn.Close();
                conn.Dispose();
            }
            else if (listBox2.Items.Count == 128)
            {
                MessageBox.Show("Due to limitations set by NovaLogic,\nwhen starting a server you are only allowed to choose 128 maps.\nYou can add more maps, after the server has been started\nby using the Server Manager.", "Map List Error");
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
                label33.Text = $"{listBox2.Items.Count} / {max_start_maps}";
            }
        }

        private void ListBox2_DoubleClick(object sender, EventArgs e)
        {
            selectedMapList.RemoveAt(listBox2.SelectedIndex);
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            label33.Text = $"{listBox2.Items.Count} / {max_start_maps}";
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

            try
            {

                // get form vars... FUCK.

                int id = _state.Instances[ArrayID].Id;
                string server_name = textBox_serverName.Text;
                string motd = textBox_MOTD.Text;
                string country_code = textBox_countryCode.Text;
                string server_password = textBox_passServer.Text;
                int session_type = 0;
                int max_slots = Convert.ToInt32(comboBox_maxPlayers.SelectedItem);
                int start_delay = comboBox_startDelay.SelectedIndex;
                int loop_maps;
                if (checkBox_loopMaps.Checked)
                {
                    loop_maps = 1;
                }
                else
                {
                    loop_maps = 0;
                }
                int max_kills = comboBox_maxKills.SelectedIndex;
                int game_score = comboBox_flagsScored.SelectedIndex;
                int zone_timer = comboBox_zoneTimer.SelectedIndex;
                int respawn_time = comboBox_respawnTime.SelectedIndex;
                int time_limit = comboBox_timeLimit.SelectedIndex;
                int require_novalogic;
                bool require_novalogic_flag;
                if (checkBox_reqNova.Checked)
                {
                    require_novalogic_flag = true;
                    require_novalogic = 1;
                }
                else
                {
                    require_novalogic_flag = false;
                    require_novalogic = 0;
                }
                int run_windowed;
                if (checkBox_windowMode.Checked)
                {
                    run_windowed = 1;
                }
                else
                {
                    run_windowed = 0;
                }
                int allow_custom_skins;
                bool allow_custom_skins_flag;
                if (checkBox_customSkin.Checked)
                {
                    allow_custom_skins_flag = true;
                    allow_custom_skins = 1;
                }
                else
                {
                    allow_custom_skins_flag = false;
                    allow_custom_skins = 0;
                }
                int game_mod = comboBox_gameMod.SelectedIndex;
                string blue_team_password = textBox_passBlue.Text;
                string red_team_password = textBox_passRed.Text;
                int friendly_fire;
                bool friendly_fire_flag;
                if (checkBox_ffire.Checked)
                {
                    friendly_fire = 1;
                    friendly_fire_flag = true;
                }
                else
                {
                    friendly_fire = 0;
                    friendly_fire_flag = false;
                }
                int friendly_fire_warning;
                bool friendly_fire_warning_flag;
                if (checkBox_ffireWarn.Checked)
                {
                    friendly_fire_warning = 1;
                    friendly_fire_warning_flag = true;
                }
                else
                {
                    friendly_fire_warning = 0;
                    friendly_fire_warning_flag = false;
                }
                int friendly_tags;
                bool friendly_tags_flag;
                if (checkBox_ftags.Checked)
                {
                    friendly_tags = 1;
                    friendly_tags_flag = true;
                }
                else
                {
                    friendly_tags = 0;
                    friendly_tags_flag = false;
                }
                int auto_balance;
                bool auto_balance_flag;
                if (checkBox_autoBal.Checked)
                {
                    auto_balance = 1;
                    auto_balance_flag = true;
                }
                else
                {
                    auto_balance = 0;
                    auto_balance_flag = false;
                }
                int show_tracers;
                bool show_tracers_flag;
                if (checkBox_showTrace.Checked)
                {
                    show_tracers = 1;
                    show_tracers_flag = true;
                }
                else
                {
                    show_tracers = 0;
                    show_tracers_flag = false;
                }
                int show_team_clays;
                bool show_team_clays_flag;
                if (checkBox_showTeamClays.Checked)
                {
                    show_team_clays = 1;
                    show_team_clays_flag = true;
                }
                else
                {
                    show_team_clays = 0;
                    show_team_clays_flag = false;
                }
                int allow_auto_range;
                bool allow_auto_range_flag;
                if (checkBox_autoRange.Checked)
                {
                    allow_auto_range_flag = true;
                    allow_auto_range = 1;
                }
                else
                {
                    allow_auto_range_flag = false;
                    allow_auto_range = 0;
                }
                int enable_min_ping;
                int min_ping;
                bool min_ping_flag;
                if (checkBox_minPing.Checked)
                {
                    enable_min_ping = 1;
                    min_ping_flag = true;
                    min_ping = Convert.ToInt32(textBox_minPing.Text);
                }
                else
                {
                    enable_min_ping = 0;
                    min_ping_flag = false;
                    min_ping = 0;
                }
                int enable_max_ping;
                int max_ping;
                bool max_ping_flag;
                if (checkBox_maxPing.Checked)
                {
                    enable_max_ping = 1;
                    max_ping_flag = true;
                    max_ping = Convert.ToInt32(textBox_maxPing.Text);
                }
                else
                {
                    enable_max_ping = 0;
                    max_ping_flag = false;
                    max_ping = 0;
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


                SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                conn.Open();

                SQLiteCommand update_query = new SQLiteCommand("UPDATE `instances_config` SET `server_name` = @servername, `motd` = @motd, `country_code` = @countrycode, `server_password` = @server_password,`session_type` = @sessiontype, `max_slots` = @max_slots, `start_delay` = @start_delay, `loop_maps` = @loop_maps, `max_kills` = @max_kills, `game_score` = @game_score, `zone_timer` = @zone_timer, `respawn_time` = @respawn_time, `time_limit` = @time_limit, `require_novalogic` = @require_novalogic, `windowed_mode` = @run_windowed, `allow_custom_skins` = @allow_custom_skins, `run_dedicated` = @run_dedicated, `game_mod` = @game_mod, `mapcycle` = @selected_maps, `blue_team_password` = @blue_team_password, `red_team_password` = @red_team_password, `friendly_fire` = @friendly_fire, `friendly_fire_warning` = @friendly_fire_warning, `friendly_tags` = @friendly_tags, `auto_balance` = @auto_balance, `show_tracers` = @show_tracers, `show_team_clays` = @show_team_clays, `allow_auto_range` = @allow_auto_range, `enable_min_ping` = @enable_min_ping, `min_ping` = @min_ping, `enable_max_ping` = @enable_max_ping, `max_ping` = @max_ping, `availablemaps` = @availablemaps WHERE `profile_id` = @profile_id", conn);

                update_query.Parameters.AddWithValue("@servername", server_name);
                update_query.Parameters.AddWithValue("@motd", motd);
                update_query.Parameters.AddWithValue("@countrycode", country_code);
                update_query.Parameters.AddWithValue("@sessiontype", _state.Instances[ArrayID].SessionType);
                update_query.Parameters.AddWithValue("@server_password", server_password);
                update_query.Parameters.AddWithValue("@max_slots", max_slots);
                update_query.Parameters.AddWithValue("@start_delay", start_delay);
                update_query.Parameters.AddWithValue("@loop_maps", loop_maps);
                update_query.Parameters.AddWithValue("@max_kills", max_kills);
                update_query.Parameters.AddWithValue("@game_score", game_score);
                update_query.Parameters.AddWithValue("@zone_timer", zone_timer);
                update_query.Parameters.AddWithValue("@respawn_time", respawn_time);
                update_query.Parameters.AddWithValue("@time_limit", time_limit);
                update_query.Parameters.AddWithValue("@require_novalogic", require_novalogic);
                update_query.Parameters.AddWithValue("@run_windowed", run_windowed);
                update_query.Parameters.AddWithValue("@allow_custom_skins", allow_custom_skins);
                update_query.Parameters.AddWithValue("@run_dedicated", Convert.ToInt32(checkBox_runDedicated.Checked));
                update_query.Parameters.AddWithValue("@game_mod", game_mod);
                update_query.Parameters.AddWithValue("@blue_team_password", blue_team_password);
                update_query.Parameters.AddWithValue("@red_team_password", red_team_password);
                update_query.Parameters.AddWithValue("@friendly_fire", friendly_fire);
                update_query.Parameters.AddWithValue("@friendly_fire_warning", friendly_fire_warning);
                update_query.Parameters.AddWithValue("@friendly_tags", friendly_tags);
                update_query.Parameters.AddWithValue("@auto_balance", auto_balance);
                update_query.Parameters.AddWithValue("@show_tracers", show_tracers);
                update_query.Parameters.AddWithValue("@show_team_clays", show_team_clays);
                update_query.Parameters.AddWithValue("@allow_auto_range", allow_auto_range);
                update_query.Parameters.AddWithValue("@enable_min_ping", enable_min_ping);
                update_query.Parameters.AddWithValue("@min_ping", min_ping);
                update_query.Parameters.AddWithValue("@enable_max_ping", enable_max_ping);
                update_query.Parameters.AddWithValue("@max_ping", max_ping);
                update_query.Parameters.AddWithValue("@profile_id", _state.Instances[ArrayID].Id);
                //string path = "";
                string file_name = "";
                //string profile_name = "Delta Force, V1.5.0.5";
                string sql1 = "select * from instances WHERE id = " + _state.Instances[ArrayID].Id + ";";
                SQLiteCommand command = new SQLiteCommand(sql1, conn);
                SQLiteDataReader result1 = command.ExecuteReader();
                while (result1.Read())
                {
                    switch (result1.GetInt32(result1.GetOrdinal("game_type")))
                    {
                        case 0:
                            file_name = "dfbhd.exe";
                            break;
                        case 1:
                            file_name = "jops.exe";
                            break;
                    }
                }
                command.Dispose();
                result1.Close();
                result1.Dispose();
                // sanity check

                SQLiteCommand checkPidQuery = new SQLiteCommand("SELECT COUNT(*) FROM `instances_pid` WHERE `profile_id` = @instanceId;", conn);
                checkPidQuery.Parameters.AddWithValue("@instanceId", _state.Instances[ArrayID].Id);
                int checkPid = Convert.ToInt32(checkPidQuery.ExecuteScalar());
                checkPidQuery.Dispose();

                if (checkPid == 0)
                {
                    SQLiteCommand insert_cmd = new SQLiteCommand("INSERT INTO `instances_pid` (`profile_id`, `pid`) VALUES (@instanceid, 0)", conn);
                    insert_cmd.Parameters.AddWithValue("@instanceid", _state.Instances[ArrayID].Id);
                    insert_cmd.ExecuteNonQuery();
                }

                // get instance information
                string bind_address = _state.Instances[ArrayID].BindAddress;
                int game_port = _state.Instances[ArrayID].GamePort;

                string autoResPath = Path.Combine(_state.Instances[ArrayID].GamePath, "autores.bin");

                string dfvCFGPath = Path.Combine(_state.Instances[ArrayID].GamePath, "dfv.cfg");

                string text = File.ReadAllText(dfvCFGPath);
                text = text.Replace("// DISPLAY", "[Display]");
                text = text.Replace("// CONTROLS", "[Controls]");
                text = text.Replace("// MULTIPLAYER", "[Multiplayer]");
                text = text.Replace("// MAP", "[Map]");
                text = text.Replace("// SYSTEM", "[System]");

                var configFileFromString = new ConfigParser(text,
                  new ConfigParserSettings
                  {
                      MultiLineValues = MultiLineValues.Simple | MultiLineValues.AllowValuelessKeys | MultiLineValues.QuoteDelimitedValues
                  });
                // get string vars
                string hw3d_name = configFileFromString.GetValue("Display", "hw3d_name");
                string hw3d_guid = configFileFromString.GetValue("Display", "hw3d_guid");

                if (listBox2.Items.Count == 0)
                {
                    MessageBox.Show("Please select at least one map.", "Uh Oh!");
                    return;
                }


                // delete existing autores file if it exists
                if (File.Exists(autoResPath))
                {
                    File.Delete(autoResPath);
                }


                _state.Instances[ArrayID].ServerName = server_name;
                _state.Instances[ArrayID].MaxSlots = max_slots;
                _state.Instances[ArrayID].LastUpdateTime = DateTime.Now;
                _state.Instances[ArrayID].NextUpdateTime = DateTime.Now.AddSeconds(5.0);
                _state.Instances[ArrayID].AutoBalance = auto_balance_flag;
                _state.Instances[ArrayID].AllowAutoRange = allow_auto_range_flag;
                _state.Instances[ArrayID].AllowCustomSkins = allow_custom_skins_flag;
                _state.Instances[ArrayID].BindAddress = bind_address;
                _state.Instances[ArrayID].BluePassword = blue_team_password;
                _state.Instances[ArrayID].CountryCode = country_code;
                _state.Instances[ArrayID].Dedicated = checkBox_runDedicated.Checked;
                _state.Instances[ArrayID].FriendlyFire = friendly_fire_flag;
                _state.Instances[ArrayID].FriendlyFireKills = friendly_fire;
                _state.Instances[ArrayID].FriendlyFireWarning = friendly_fire_warning_flag;
                _state.Instances[ArrayID].FriendlyTags = friendly_tags_flag;
                _state.Instances[ArrayID].GamePort = game_port;
                _state.Instances[ArrayID].LoopMaps = loop_maps;
                _state.Instances[ArrayID].MaxKills = max_kills;
                _state.Instances[ArrayID].MaxPing = max_ping_flag;
                _state.Instances[ArrayID].MaxPingValue = max_ping;
                _state.Instances[ArrayID].MinPing = min_ping_flag;
                _state.Instances[ArrayID].MinPingValue = min_ping;
                _state.Instances[ArrayID].MOTD = motd;
                _state.Instances[ArrayID].Password = server_password;
                _state.Instances[ArrayID].RedPassword = red_team_password;
                _state.Instances[ArrayID].RequireNovaLogin = require_novalogic_flag;
                _state.Instances[ArrayID].RespawnTime = respawn_time;
                _state.Instances[ArrayID].SessionType = session_type;
                _state.Instances[ArrayID].ShowTeamClays = show_team_clays_flag;
                _state.Instances[ArrayID].ShowTracers = show_tracers_flag;
                _state.Instances[ArrayID].StartDelay = start_delay;
                _state.Instances[ArrayID].availableMaps = availableMaps;
                _state.Instances[ArrayID].GameScore = game_score;
                _state.Instances[ArrayID].TimeLimit = time_limit;

                _state.Instances[ArrayID].MapList = new Dictionary<int, MapList>();
                foreach (var map in selectedMapList)
                {
                    _state.Instances[ArrayID].MapList.Add(_state.Instances[ArrayID].MapList.Count, map);
                }

                update_query.Parameters.AddWithValue("@availablemaps", JsonConvert.SerializeObject(_state.Instances[ArrayID].availableMaps));
                update_query.Parameters.AddWithValue("@selected_maps", JsonConvert.SerializeObject(_state.Instances[ArrayID].MapList));
                update_query.ExecuteNonQuery();
                update_query.Dispose();

                
                MemoryStream ms = new MemoryStream();
                int dedicatedSlots = _state.Instances[ArrayID].MaxSlots + Convert.ToInt32(_state.Instances[ArrayID].Dedicated);
                bool loopMaps = true;

                int gamePlayOptionsInt = serverManagerUpdateMemory.CalulateGameOptions(_state, ArrayID);

                string _miscGraphicSettings = "00 0E 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 CD CC 4C 3F 06 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 10 00 00 00 10 00 00 00 10 00 00 08 00 00 00 01 00 00 00 00 10 00 00 00 00 D0 1E 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 01 00 00 00 1E 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 02 00 00 00 02 00 00 00 00 00 00 00 03 00 00 00 03 00 00 00 02 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 03 00 00 00 02 00 00 00 00 00 00 00 03 00 00 00 03 00 00 00 02 00 00 00 04 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF 00 00 00 C0 00 00 00 C0 00 00 00 C0 00 00 00 C0 00 00 00 02 00 00 00 01 00 00 00";
                string applicationSettings = "01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00";

                byte[] autoRestart = Encoding.Default.GetBytes("autorestartV0.0");
                byte[] numberOfMapsBytes = BitConverter.GetBytes(128);

                byte[] graphicsSetup_Name = Encoding.Default.GetBytes(hw3d_name);
                byte[] graphicsSetup_GUID = Encoding.Default.GetBytes(hw3d_guid);
                byte[] graphicsSetupMisc_Settings = HexConverter.ToByteArray(_miscGraphicSettings.Replace(" ", ""));
                byte[] applicationSettingBytes = HexConverter.ToByteArray(applicationSettings.Replace(" ", ""));
                byte[] windowedModeBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].WindowedMode));
                byte[] ServerNameBytes = Encoding.Default.GetBytes(_state.Instances[ArrayID].ServerName);
                byte[] countryCodeBytes = Encoding.Default.GetBytes(_state.Instances[ArrayID].CountryCode);
                byte[] BindAddress = Encoding.Default.GetBytes(_state.Instances[ArrayID].BindAddress);
                byte[] firstMapFile = Encoding.Default.GetBytes(_state.Instances[ArrayID].MapList[0].MapFile);
                byte[] maxSlotsBytes = BitConverter.GetBytes(_state.Instances[ArrayID].MaxSlots);
                byte[] dedicatedBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].Dedicated));
                byte[] GameScoreBytes = BitConverter.GetBytes(_state.Instances[ArrayID].GameScore);
                byte[] StartDelayBytes = BitConverter.GetBytes(_state.Instances[ArrayID].StartDelay);
                byte[] serverPasswordBytes = Encoding.Default.GetBytes(_state.Instances[ArrayID].Password);
                byte[] redTeamPasswordBytes = Encoding.Default.GetBytes(_state.Instances[ArrayID].RedPassword);
                byte[] blueTeamPasswordBytes = Encoding.Default.GetBytes(_state.Instances[ArrayID].BluePassword);
                byte[] gamePlayOptionsBytes = BitConverter.GetBytes(gamePlayOptionsInt);
                byte[] loopMapsBytes;

                if (loopMaps == true)
                {
                    loopMapsBytes = BitConverter.GetBytes(2);
                }
                else
                {
                    loopMapsBytes = BitConverter.GetBytes(1);
                }

                byte[] gameTypeBytes = BitConverter.GetBytes(_state.autoRes.gameTypes[_state.Instances[ArrayID].MapList[0].GameType].DatabaseId);
                byte[] timeLimitBytes = BitConverter.GetBytes(_state.Instances[ArrayID].TimeLimit);
                byte[] respawnTimeBytes = BitConverter.GetBytes(_state.Instances[ArrayID].RespawnTime);
                byte[] allowCustomSkinsBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].AllowCustomSkins));
                byte[] requireNovaLoginBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].RequireNovaLogin));
                byte[] MOTDBytes = Encoding.Default.GetBytes(_state.Instances[ArrayID].MOTD);
                byte[] sessionTypeBytes = BitConverter.GetBytes(_state.Instances[ArrayID].SessionType);
                byte[] dedicatedSlotsBytes = BitConverter.GetBytes(dedicatedSlots);
                byte[] graphicsHeaderSettings = BitConverter.GetBytes(-1);
                byte[] graphicsSetting_1 = BitConverter.GetBytes(8);
                byte[] startDelayBytes = BitConverter.GetBytes(_state.Instances[ArrayID].StartDelay);
                byte[] minPingValueBytes = BitConverter.GetBytes(_state.Instances[ArrayID].MinPingValue);
                byte[] enableMinPingBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].MinPing));
                byte[] maxPingValueBytes = BitConverter.GetBytes(_state.Instances[ArrayID].MaxPingValue);
                byte[] enableMaxPingBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].MaxPing));
                byte[] gamePortBytes = BitConverter.GetBytes(_state.Instances[ArrayID].GamePort);
                byte[] flagBallScoreBytes = BitConverter.GetBytes(_state.Instances[ArrayID].FBScore);
                byte[] zoneTimerBytes = BitConverter.GetBytes(_state.Instances[ArrayID].ZoneTimer);
                byte[] customMapFlagBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[ArrayID].MapList[0].CustomMap));

                byte[] mapListPrehandle = BitConverter.GetBytes(10621344);
                byte[] finalAppSetup = HexConverter.ToByteArray("00 00 00 00 00 00 00 00 05 00 00 00 00".Replace(" ", ""));
                byte[] resolutionSetup = HexConverter.ToByteArray("02 00 00 00 00 01 00 00 00".Replace(" ", ""));
                byte[] graphicsPrehandle = HexConverter.ToByteArray("02 00 00 00 01 00 00 00".Replace(" ", ""));
                byte[] defaultWeaponSetup = HexConverter.ToByteArray("05 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00".Replace(" ", ""));
                byte[] endOfMapCfg = HexConverter.ToByteArray("20 B5 B6 01".Replace(" ", ""));
                byte[] endOfMapCfg2 = HexConverter.ToByteArray("53 01 00 00 00 13 00 00 00 13 00 00 00 04 00 00 00".Replace(" ", ""));


                ms.Seek(0, SeekOrigin.Begin);
                // autorestart header + Number of Total Maps
                ms.Write(autoRestart, 0, autoRestart.Length);
                ms.Write(numberOfMapsBytes, 0, numberOfMapsBytes.Length);

                ms.Seek(0x4D, SeekOrigin.Begin);
                ms.Write(firstMapFile, 0, firstMapFile.Length);

                ms.Seek(0xAF, SeekOrigin.Begin);
                ms.Write(customMapFlagBytes, 0, customMapFlagBytes.Length);

                ms.Seek(0x68F, SeekOrigin.Begin);
                ms.Write(resolutionSetup, 0, resolutionSetup.Length);

                ms.Seek(0x277, SeekOrigin.Begin);
                ms.Write(sessionTypeBytes, 0, sessionTypeBytes.Length);

                ms.Seek(0x1C7, SeekOrigin.Begin);
                ms.Write(applicationSettingBytes, 0, applicationSettingBytes.Length);

                ms.Seek(0x283, SeekOrigin.Begin);
                ms.Write(dedicatedSlotsBytes, 0, dedicatedSlotsBytes.Length);

                ms.Seek(0x28F, SeekOrigin.Begin);
                ms.Write(gameTypeBytes, 0, gameTypeBytes.Length);

                ms.Seek(0x293, SeekOrigin.Begin);
                ms.Write(finalAppSetup, 0, finalAppSetup.Length);

                ms.Seek(0x1347, SeekOrigin.Begin);
                ms.Write(graphicsPrehandle, 0, graphicsPrehandle.Length);

                ms.Seek(0x134F, SeekOrigin.Begin);
                ms.Write(graphicsHeaderSettings, 0, graphicsHeaderSettings.Length);

                ms.Seek(0x1353, SeekOrigin.Begin);
                ms.Write(graphicsSetting_1, 0, graphicsSetting_1.Length);

                ms.Seek(0x1357, SeekOrigin.Begin);
                ms.Write(windowedModeBytes, 0, windowedModeBytes.Length);

                ms.Seek(0x135F, SeekOrigin.Begin);
                ms.Write(graphicsSetup_Name, 0, graphicsSetup_Name.Length);

                ms.Seek(0x137F, SeekOrigin.Begin);
                ms.Write(graphicsSetup_GUID, 0, graphicsSetup_GUID.Length);
                ms.Write(graphicsSetupMisc_Settings, 0, graphicsSetupMisc_Settings.Length);

                ms.Seek(0x152F, SeekOrigin.Begin);
                ms.Write(serverPasswordBytes, 0, serverPasswordBytes.Length);

                ms.Seek(0x1562, SeekOrigin.Begin);
                ms.Write(redTeamPasswordBytes, 0, redTeamPasswordBytes.Length);

                ms.Seek(0x1573, SeekOrigin.Begin);
                ms.Write(blueTeamPasswordBytes, 0, blueTeamPasswordBytes.Length);

                ms.Seek(0x151F, SeekOrigin.Begin);
                ms.Write(gamePlayOptionsBytes, 0, gamePlayOptionsBytes.Length);

                ms.Seek(0x15A6, SeekOrigin.Begin);
                ms.Write(ServerNameBytes, 0, ServerNameBytes.Length);

                ms.Seek(0x15C6, SeekOrigin.Begin);
                ms.Write(countryCodeBytes, 0, countryCodeBytes.Length);

                ms.Seek(0x1613, SeekOrigin.Begin);
                ms.Write(dedicatedBytes, 0, dedicatedBytes.Length);

                ms.Seek(0x15EA, SeekOrigin.Begin);
                ms.Write(BindAddress, 0, BindAddress.Length);

                ms.Seek(0x160B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x161F, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(100), 0, BitConverter.GetBytes(100).Length);

                ms.Seek(0x162F, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1633, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(210), 0, BitConverter.GetBytes(210).Length);

                ms.Seek(0x1637, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x163B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(2), 0, BitConverter.GetBytes(2).Length);

                ms.Seek(0x164B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1693, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1697, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x169B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(15), 0, BitConverter.GetBytes(15).Length);

                ms.Seek(0x16B7, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(2), 0, BitConverter.GetBytes(2).Length);

                ms.Seek(0x16BB, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x16C7, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x16CB, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(2), 0, BitConverter.GetBytes(2).Length);

                ms.Seek(0x16EF, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(4), 0, BitConverter.GetBytes(4).Length);

                ms.Seek(0x16F4, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x16FC, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1703, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1707, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1627, SeekOrigin.Begin);
                ms.Write(startDelayBytes, 0, startDelayBytes.Length);

                ms.Seek(0x16F3, SeekOrigin.Begin);
                ms.Write(minPingValueBytes, 0, minPingValueBytes.Length);

                ms.Seek(0x16F7, SeekOrigin.Begin);
                ms.Write(enableMinPingBytes, 0, enableMinPingBytes.Length);

                ms.Seek(0x16FB, SeekOrigin.Begin);
                ms.Write(maxPingValueBytes, 0, maxPingValueBytes.Length);

                ms.Seek(0x16FF, SeekOrigin.Begin);
                ms.Write(enableMaxPingBytes, 0, enableMaxPingBytes.Length);

                ms.Seek(0x160F, SeekOrigin.Begin);
                ms.Write(maxSlotsBytes, 0, maxSlotsBytes.Length);

                ms.Seek(0x16CF, SeekOrigin.Begin);
                ms.Write(gamePortBytes, 0, gamePortBytes.Length);

                ms.Seek(0x16DB, SeekOrigin.Begin);
                ms.Write(requireNovaLoginBytes, 0, requireNovaLoginBytes.Length);

                ms.Seek(0x16D7, SeekOrigin.Begin);
                ms.Write(allowCustomSkinsBytes, 0, allowCustomSkinsBytes.Length);

                ms.Seek(0x170B, SeekOrigin.Begin);
                ms.Write(MOTDBytes, 0, MOTDBytes.Length);

                ms.Seek(0x1623, SeekOrigin.Begin);
                ms.Write(flagBallScoreBytes, 0, flagBallScoreBytes.Length);

                ms.Seek(0x1643, SeekOrigin.Begin);
                ms.Write(zoneTimerBytes, 0, zoneTimerBytes.Length);

                ms.Seek(0x1647, SeekOrigin.Begin);
                ms.Write(respawnTimeBytes, 0, respawnTimeBytes.Length);

                ms.Seek(0x163F, SeekOrigin.Begin);
                ms.Write(timeLimitBytes, 0, timeLimitBytes.Length);

                ms.Seek(0x1DA4, SeekOrigin.Begin);
                ms.Write(GameScoreBytes, 0, GameScoreBytes.Length);

                ms.Seek(0x178B, SeekOrigin.Begin);
                ms.Write(defaultWeaponSetup, 0, defaultWeaponSetup.Length);

                ms.Seek(0x187F, SeekOrigin.Begin);
                ms.Write(mapListPrehandle, 0, mapListPrehandle.Length);

                byte[] endOfMap = HexConverter.ToByteArray("20 B5 B6 01 00 00 00 00 53 01 00 00 00 13 00 00 00 13 00 00 00 04 00 00 00".Replace(" ", ""));

                foreach (var map in _state.Instances[ArrayID].MapList)
                {
                    byte[] mapFile = Encoding.Default.GetBytes(map.Value.MapFile);
                    ms.Write(mapFile, 0, mapFile.Length);

                    ms.Seek(ms.Position + (0x20F - mapFile.Length), SeekOrigin.Begin);
                    byte[] mapName = Encoding.Default.GetBytes(map.Value.MapName);
                    ms.Write(mapName, 0, mapName.Length);

                    ms.Seek(ms.Position + (0x305 - mapName.Length), SeekOrigin.Begin);
                    ms.Write(endOfMap, 0, endOfMap.Length);

                    ms.Seek(ms.Position + 0x1E3, SeekOrigin.Begin);
                    byte[] customMap = BitConverter.GetBytes(Convert.ToInt32(map.Value.CustomMap));
                    ms.Write(customMap, 0, customMap.Length);

                    // prepare for next entry
                    ms.Seek(ms.Position + 0x1C, SeekOrigin.Begin);
                }
                for (int i = _state.Instances[ArrayID].MapList.Count; i < 128; i++)
                {
                    byte[] mapFile = Encoding.Default.GetBytes("NA.bms");
                    ms.Write(mapFile, 0, mapFile.Length);

                    ms.Seek(ms.Position + (0x20F - mapFile.Length), SeekOrigin.Begin);
                    byte[] mapName = Encoding.Default.GetBytes("NA");
                    ms.Write(mapName, 0, mapName.Length);

                    ms.Seek(ms.Position + (0x305 - mapName.Length), SeekOrigin.Begin);
                    ms.Write(endOfMap, 0, endOfMap.Length);

                    ms.Seek(ms.Position + 0x1E3, SeekOrigin.Begin);
                    byte[] customMap = BitConverter.GetBytes(Convert.ToInt32(false));
                    ms.Write(customMap, 0, customMap.Length);

                    // prepare for next entry
                    ms.Seek(ms.Position + 0x1C, SeekOrigin.Begin);
                }


                BinaryWriter writer = new BinaryWriter(File.Open(autoResPath, FileMode.OpenOrCreate, FileAccess.ReadWrite));
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write(ms.ToArray());
                writer.Close();

                Thread.Sleep(1000); // sleep 100ms to allow flushing the file to complete

                Process process;
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(_state.Instances[ArrayID].GamePath, file_name),
                    WorkingDirectory = _state.Instances[ArrayID].GamePath,
                    Arguments = "/w /LOADBAR /NOSYSDUMP /serveonly /autorestart",
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                process = Process.Start(startInfo);

                //- /w /LOADBAR /NOSYSDUMP /serveonly /autorestart
                List<int> currentPIDs = new List<int>();
                foreach (var instance in _state.Instances)
                {
                    if (instance.Value.PID != 0)
                    {
                        currentPIDs.Add(instance.Value.PID.GetValueOrDefault());
                    }
                }
                Process[] processes = Process.GetProcessesByName("dfbhd");
                foreach (var activeProcess in processes)
                {
                    if (!currentPIDs.Contains(activeProcess.Id) && activeProcess.StartTime > DateTime.Now.AddMinutes(-1))
                    {
                        activeProcess.MaxWorkingSet = new IntPtr(0x7fffffff);
                        _state.Instances[ArrayID].PID = activeProcess.Id;
                        _state.ApplicationProcesses[ArrayID] = activeProcess;
                    }
                }

                string pid_update_db = "UPDATE instances_pid SET pid = " + _state.ApplicationProcesses[ArrayID].Id + " WHERE profile_id = " + _state.Instances[ArrayID].Id + ";";
                SQLiteCommand pid_update = new SQLiteCommand(pid_update_db, conn);
                pid_update.ExecuteNonQuery();
                pid_update.Dispose();


                _state.Instances[ArrayID].gameCrashCounter = 0;


                IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _state.Instances[ArrayID].PID.GetValueOrDefault());
                _state.Instances[ArrayID].ProcessHandle = processHandle;
                var baseAddr = 0x400000;

                Thread.Sleep(1500);
                SetWindowText(_state.ApplicationProcesses[ArrayID].MainWindowHandle, $"{_state.Instances[ArrayID].GameName}");
                if (_state.Instances[ArrayID].HostName != "Host")
                {
                    int buffer = 0;
                    byte[] PointerAddr = new byte[4];
                    var Pointer = baseAddr + 0x005ED600;
                    ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                    int buffer2 = 0;
                    byte[] Hostname = Encoding.Default.GetBytes(_state.Instances[ArrayID].HostName + "\0");
                    var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                    WriteProcessMemory((int)processHandle, (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
                }
                conn.Close();
                conn.Dispose();

                // HOLD THE FUCK UP. (suspend game server)
                //ProcessExtended.SuspendProcess(process.Id);

                // map list fix... I hope...
                int MapListMoveGarbageAddress = baseAddr + 0x5EA7B8;
                byte[] CurrentAddressBytes = new byte[4];
                int CurrentAddressRead = 0;
                ReadProcessMemory((int)processHandle, MapListMoveGarbageAddress, CurrentAddressBytes, CurrentAddressBytes.Length, ref CurrentAddressRead);
                int CurrentAddress = BitConverter.ToInt32(CurrentAddressBytes, 0);
                int NewAddress = CurrentAddress + 0x350;

                byte[] NewAddressBytes = BitConverter.GetBytes(NewAddress);
                int NewAddressWritten = 0;
                WriteProcessMemory((int)processHandle, MapListMoveGarbageAddress, NewAddressBytes, NewAddressBytes.Length, ref NewAddressWritten);

                // resume game server
                //ProcessExtended.ResumeProcess(process.Id);
                // null out garbage data near server maplist

                /*string nullBytes = "";
                int nullLength = 15 + _state.Instances[ArrayID].HostName.Length + 1 + 16 + _state.Instances[ArrayID].MapList[_state.Instances[ArrayID].mapIndex].MapName.Length + 1 + 17 + 15 + _state.Instances[ArrayID].MOTD.Length;
                for (int i = 0; i < nullLength; i++)
                {
                    nullBytes += "00 ";
                }
                byte[] nullByteArray = HexConverter.ToByteArray(nullBytes.Replace(" ", ""));
                int nullByteArrayWritten = 0;
                WriteProcessMemory((int)processHandle, CurrentAddress, nullByteArray, nullByteArray.Length, ref nullByteArrayWritten);*/

                int mapListLocationPtr = baseAddr + 0x005ED5F8;
                byte[] mapListLocationPtrBytes = new byte[4];
                int mapListLocationBytesPtrRead = 0;
                ReadProcessMemory((int)processHandle, mapListLocationPtr, mapListLocationPtrBytes, mapListLocationPtrBytes.Length, ref mapListLocationBytesPtrRead);

                int mapListNumberOfMaps = BitConverter.ToInt32(mapListLocationPtrBytes, 0) + 0x4;
                byte[] numberOfMaps = BitConverter.GetBytes(_state.Instances[ArrayID].MapList.Count);
                int numberofMapsWritten = 0;
                WriteProcessMemory((int)processHandle, mapListNumberOfMaps, numberOfMaps, numberOfMaps.Length, ref numberofMapsWritten);

                mapListNumberOfMaps += 0x4;
                byte[] TotalnumberOfMaps = BitConverter.GetBytes(_state.Instances[ArrayID].MapList.Count);
                int TotalnumberofMapsWritten = 0;
                WriteProcessMemory((int)processHandle, mapListNumberOfMaps, TotalnumberOfMaps, TotalnumberOfMaps.Length, ref TotalnumberofMapsWritten);


                // this was commented out because it was forcing a rebind of the TcpListener
                //Main_Profilelist main_Profilelist = new Main_Profilelist();
                //Main_Profilelist.tid = id;
                // this was commented out because it was forcing a rebind of the TcpListener

                // add checks for 0.0.0.0

                if (require_novalogic == 0)
                {
                    /*Thread.Sleep(1200);
                    // setup to ignore nova
                    IntPtr processhandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);
                    int bytesWritten = 0;
                    byte[] buffer = BitConverter.GetBytes((int)0);
                    WriteProcessMemory((int)processhandle, 0x009DDA44, buffer, buffer.Length, ref bytesWritten);*/
                }


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
                _state.Instances[ArrayID].AutoMessages.NextMessage = DateTime.Now.AddMinutes(1.0);

                // Add Firewall Rules
                _state.Instances[ArrayID].Firewall.AllowTraffic(_state.Instances[ArrayID].GameName, _state.Instances[ArrayID].GamePath);
                _state.Instances[ArrayID].Firewall.DenyTraffic(_state.Instances[ArrayID].GameName, _state.Instances[ArrayID].GamePath, _state.Instances[ArrayID].BanList);

                this.Close();
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
            label33.Text = listBox2.Items.Count.ToString() + " / 128";
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

            availableMaps = _state.Instances[ArrayID].availableMaps;

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
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
            for (int maxkills = 1; maxkills < 501; maxkills++)
            {
                comboBox_maxKills.Items.Add(maxkills);
                comboBox_flagsScored.Items.Add(maxkills);
            }
            //end maxkills
            for (int zonetimer = 1; zonetimer < 61; zonetimer++)
            {
                comboBox_zoneTimer.Items.Add(zonetimer);
                comboBox_timeLimit.Items.Add(zonetimer);
            }
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
            textBox_serverName.Text = _state.Instances[ArrayID].ServerName;
            textBox_MOTD.Text = _state.Instances[ArrayID].MOTD;
            textBox_countryCode.Text = _state.Instances[ArrayID].CountryCode;
            textBox_passServer.Text = _state.Instances[ArrayID].Password;
            comboBox_sessionType.SelectedIndex = _state.Instances[ArrayID].SessionType;
            comboBox_sessionType.Enabled = false;
            comboBox_maxPlayers.SelectedItem = _state.Instances[ArrayID].MaxSlots;
            comboBox_startDelay.SelectedIndex = _state.Instances[ArrayID].StartDelay;
            checkBox_loopMaps.Checked = Convert.ToBoolean(_state.Instances[ArrayID].LoopMaps);
            comboBox_maxKills.SelectedIndex = _state.Instances[ArrayID].MaxKills;
            comboBox_flagsScored.SelectedIndex = _state.Instances[ArrayID].GameScore;
            comboBox_zoneTimer.SelectedIndex = _state.Instances[ArrayID].ZoneTimer;
            comboBox_respawnTime.SelectedIndex = _state.Instances[ArrayID].RespawnTime;
            comboBox_timeLimit.SelectedIndex = _state.Instances[ArrayID].TimeLimit;
            checkBox_reqNova.Checked = _state.Instances[ArrayID].RequireNovaLogin;
            checkBox_windowMode.Checked = Convert.ToBoolean(_state.Instances[ArrayID].WindowedMode);
            checkBox_customSkin.Checked = _state.Instances[ArrayID].AllowCustomSkins;
            checkBox_runDedicated.Checked = Convert.ToBoolean(_state.Instances[ArrayID].Dedicated);
            textBox_passBlue.Text = _state.Instances[ArrayID].BluePassword;
            textBox_passRed.Text = _state.Instances[ArrayID].RedPassword;
            checkBox_ffire.Checked = _state.Instances[ArrayID].FriendlyFire;
            checkBox_ffireWarn.Checked = _state.Instances[ArrayID].FriendlyFireWarning;
            checkBox_ftags.Checked = _state.Instances[ArrayID].FriendlyTags;
            checkBox_autoBal.Checked = _state.Instances[ArrayID].AutoBalance;
            checkBox_showTrace.Checked = _state.Instances[ArrayID].ShowTracers;
            checkBox_showTeamClays.Checked = _state.Instances[ArrayID].ShowTeamClays;
            checkBox_autoRange.Checked = _state.Instances[ArrayID].AllowAutoRange;
            selectedMapList = new List<MapList>();
            switch (_state.Instances[ArrayID].MinPing)
            {
                case false:
                    checkBox_minPing.Checked = false;
                    textBox_minPing.Text = "0";
                    break;
                case true:
                    textBox_minPing.Text = Convert.ToString(_state.Instances[ArrayID].MinPingValue);
                    checkBox_minPing.Checked = true;
                    break;
            }
            switch (_state.Instances[ArrayID].MaxPing)
            {
                case false:
                    checkBox_maxPing.Checked = false;
                    textBox_maxPing.Text = "0";
                    break;
                case true:
                    textBox_maxPing.Text = Convert.ToString(_state.Instances[ArrayID].MaxPingValue);
                    checkBox_maxPing.Checked = true;
                    break;
            }

            foreach (var map in _state.Instances[ArrayID].MapList)
            {
                listBox2.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " " + "<" + map.Value.MapFile + ">");
                label33.Text = $"{listBox2.Items.Count} / {max_start_maps}";
            }
            
            db.Close();
            db.Dispose();

            foreach (var map in _state.Instances[ArrayID].MapList)
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
                if (File.Exists(Path.Combine(_state.Instances[ArrayID].GamePath, modPack.Value.Pff)))
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
                foreach (var map in _state.Instances[ArrayID].MapList)
                {
                    listBox2.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " <" + map.Value.MapFile + ">");
                }
                selectedMapList = new List<MapList>();
                selectedMapList = SM_PopupLoadRotation._mapList;
                label33.Text = _state.Instances[ArrayID].MapList.Count + " / 128";
            }
        }
    }
}