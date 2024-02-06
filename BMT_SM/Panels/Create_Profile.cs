using HawkSync_SM.classes;
using HawkSync_SM.classes.Plugins.pl_VoteMaps;
using HawkSync_SM.classes.Plugins.pl_WelcomePlayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class Create_Profile : Form
    {
        public static bool ProfileCreated;
        public List<string> IPv4Addresses = new List<string>();
        AppState _state;
        public Create_Profile(AppState state)
        {
            InitializeComponent();
            _state = state;
            ProfileCreated = false;
            comboBox2.SelectedItem = "None";
            comboBox3.SelectedItem = "Ignore Player";
            IPv4Addresses.Add("0.0.0.0"); // bind to all addresses
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            IPv4Addresses.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            foreach (string ip_addr in IPv4Addresses)
            {
                comboBox4.Items.Add(ip_addr);
            }
            comboBox4.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string TextBoxValue = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                TextBoxValue = folderBrowserDialog1.SelectedPath;
            }
            textBox2.Text = TextBoxValue;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedItem)
            {
                case "None":
                    textBox3.Enabled = false;
                    textBox4.Enabled = false;
                    break;
                default:
                    textBox3.Enabled = true;
                    textBox4.Enabled = true;
                    break;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
            }
            else
            {
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
            }
        }

        private void btn_addProfileClick(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(textBox5.Text, "^[a-zA-Z0-9]*$"))
            {
                MessageBox.Show("Please enter a valid HostName!", "Error");
                return;
            }
            SQLiteConnection m_dbConnection = new SQLiteConnection(ProgramConfig.DBConfig);
            m_dbConnection.Open();
            string stats_url;
            string stats_server_id;
            string stat_padding;
            string stat_padding_num_mins;
            string stat_padding_num_players;
            string babstats_master_server;
            string announce_ranks;
            string left_leaning;
            switch (comboBox2.SelectedItem)
            {
                case "None":
                    {
                        stats_url = "0";
                        stats_server_id = "0";
                        break;
                    }

                default:
                    {
                        stats_url = textBox3.Text;
                        stats_server_id = textBox4.Text;
                        break;
                    }
            }
            if (checkBox1.Checked)
            {
                stat_padding = "1";
                stat_padding_num_mins = Convert.ToString(numericUpDown2.Value);
                stat_padding_num_players = Convert.ToString(numericUpDown3.Value);
            }
            else
            {
                stat_padding = "0";
                stat_padding_num_mins = "0";
                stat_padding_num_players = "0";
            }
            if (checkBox4.Checked)
            {
                announce_ranks = "1";
            }
            else
            {
                announce_ranks = "0";
            }
            switch (comboBox3.SelectedItem)
            {
                case "Disarm Player":
                    {
                        left_leaning = "1";
                        break;
                    }
                case "Kill Player":
                    {
                        left_leaning = "2";
                        break;
                    }
                case "Kick Player":
                    {
                        left_leaning = "3";
                        break;
                    }
                default:
                    {
                        // ignore player
                        left_leaning = "0";
                        break;
                    }
            }
            // sanity checks, and form validation:
            if (textBox1.Text == string.Empty)
            {
                DialogResult result = MessageBox.Show("You MUST enter a Profile Name!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (textBox2.Text == string.Empty)
            {
                DialogResult result = MessageBox.Show("You MUST enter a Game Server Path!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (comboBox1.SelectedIndex == -1)
            {
                DialogResult result = MessageBox.Show("You MUST select the Server Game!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (textBox5.Text == string.Empty)
            {
                DialogResult result = MessageBox.Show("You MUST a Host Name!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (numericUpDown1.Value < 1024 || numericUpDown1.Value > 49151)
            {
                DialogResult result = MessageBox.Show("You need to select a port number between 1024 - 49151!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            string bind_address = comboBox4.Text;

            PluginsClass defaultPlugins = new PluginsClass
            {
                WelcomeMessage = checkedListBox1.GetItemChecked(0),
                VoteMaps = checkedListBox1.GetItemChecked(1),
                WelcomeMessageSettings = new wp_PluginSettings
                {
                    NewPlayerMsg = "Welcome $(PlayerName)!",
                    ReturningPlayerMsg = "Welcome back $(PlayerName)!"
                },
                VoteMapSettings = new vm_PluginSettings
                {
                    CoolDown = 1,
                    CoolDownType = vm_internal.CoolDownTypes.NUM_OF_MAPS,
                    MinPlayers = 3
                }
            };

            SQLiteCommand add_profile = new SQLiteCommand("INSERT INTO `instances` (`id`, `name`, `game_type`, `gamepath`, `stats`, `stats_url`, `stats_server_id`, `stats_verified`, `host_name`, `port`, `anti_stat_padding`, `anti_stat_padding_min_minutes`, `anti_stat_padding_min_players`, `misc_crashrecovery`, `misc_babstats_master`, `misc_show_ranks`, `misc_left_leaning`, `bind_address`, `novahq_master`, `novacc_master`, `plugins`) VALUES (NULL, @profilename, @gametype, @gamepath, @statssoftware, @statsurl, @statsserverid, 0, @host, @port, @anti_stat_pad, @anti_stat_padding_min_minutes, @anti_stat_padding_min_players, @misc_crashrecovery, @misc_babstats_master, @announceranks, @leftleaning, @bindingaddress, @novahq_master, @novacc_master, @plugins); ", m_dbConnection);
            add_profile.Parameters.AddWithValue("@profilename", textBox1.Text);
            add_profile.Parameters.AddWithValue("@gametype", comboBox1.SelectedIndex);
            add_profile.Parameters.AddWithValue("@gamepath", textBox2.Text);
            add_profile.Parameters.AddWithValue("@statssoftware", comboBox2.SelectedIndex);
            add_profile.Parameters.AddWithValue("@statsurl", stats_url);
            add_profile.Parameters.AddWithValue("@statsserverid", stats_server_id);
            add_profile.Parameters.AddWithValue("@host", textBox5.Text);
            add_profile.Parameters.AddWithValue("@port", numericUpDown1.Value);
            add_profile.Parameters.AddWithValue("@anti_stat_pad", stat_padding);
            add_profile.Parameters.AddWithValue("@anti_stat_padding_min_minutes", stat_padding_num_mins);
            add_profile.Parameters.AddWithValue("@anti_stat_padding_min_players", stat_padding_num_players);
            add_profile.Parameters.AddWithValue("@misc_crashrecovery", Convert.ToInt32(checkBox2.Checked));
            add_profile.Parameters.AddWithValue("@misc_babstats_master", false);
            add_profile.Parameters.AddWithValue("@announceranks", announce_ranks);
            add_profile.Parameters.AddWithValue("@leftleaning", left_leaning);
            add_profile.Parameters.AddWithValue("@bindingaddress", bind_address);
            add_profile.Parameters.AddWithValue("@novahq_master", Convert.ToInt32(checkBox5.Checked));
            add_profile.Parameters.AddWithValue("@novacc_master", Convert.ToInt32(checkBox6.Checked));
            add_profile.Parameters.AddWithValue("@plugins", JsonConvert.SerializeObject(defaultPlugins));
            add_profile.ExecuteNonQuery();
            add_profile.Dispose();
            int newID = (int)m_dbConnection.LastInsertRowId;
            SQLiteCommand add_profile_config = new SQLiteCommand("INSERT INTO instances_config (`profile_id`, `server_name`, `motd`, `country_code`, `server_password`, `session_type`, `max_slots`, `start_delay`, `loop_maps`, `max_kills`, `game_score`, `zone_timer`, `respawn_time`, `time_limit`, `max_team_lives`, `require_novalogic`, `windowed_mode`, `allow_custom_skins`, `run_dedicated`, `game_mod`, `mapcycle`, `blue_team_password`, `red_team_password`, `friendly_fire`, `friendly_fire_warning`, `friendly_fire_kills`, `friendly_tags`, `auto_balance`, `show_tracers`, `show_team_clays`, `allow_auto_range`, `flagreturntime`, `oneshotkills`, `fatbullets`, `destroybuildings`, `rolerestrictions`, `weaponrestrictions`, `enable_min_ping`, `min_ping`, `enable_max_ping`, `max_ping`, `enable_msg`, `auto_messages`, `auto_msg_interval`, `enableVPNCheck`, `warnlevel`, `psptakeover`, `availablemaps`, `fbscore`, `kothscore`, `scoreboard_override`) VALUES (@profileid, 'Untitled', 'Put your message here', 'US', '', '0', '50', '0', '1', '19', '19', '4', '4', '4', '10', '0', '1', '0', '1', '0', '[]', '', '', '0', '0', '0', '1', '0', '0', '0', '0', '10', '0', '0', '0', '[]', '[]', '0', '0', '0', '0', '0', '[]', '3', '0', '65', '1', '[]', '20', '20', '45');", m_dbConnection);
            add_profile_config.Parameters.AddWithValue("@profileid", newID);
            add_profile_config.ExecuteNonQuery();
            add_profile_config.Dispose();
            SQLiteCommand add_profile_pid = new SQLiteCommand("INSERT INTO `instances_pid` (`profile_id`, `pid`) VALUES (@profileid, -1);", m_dbConnection);
            add_profile_pid.Parameters.AddWithValue("@profileid", newID);
            add_profile_pid.ExecuteNonQuery();
            add_profile_pid.Dispose();
            ProfileCreated = true;
            m_dbConnection.Close();
            bool TeamSabre = false;
            if (comboBox1.SelectedIndex == 0 && File.Exists(Path.Combine(textBox2.Text, "EXP1.pff")))
            {
                TeamSabre = true;
            }


            Instance newInstance = new Instance
            {
                Id = newID,
                DataTableColumnId = _state.Instances.Count,
                GamePath = textBox2.Text,
                GameType = comboBox1.SelectedIndex,
                GameName = textBox1.Text,
                HostName = textBox5.Text,
                CountryCode = "US",
                ServerName = "Untitled",
                Password = "",
                MOTD = "Put your message here",
                Dedicated = true,
                SessionType = 0,
                MaxSlots = 50,
                GameScore = 19,
                ZoneTimer = 4,
                WindowedMode = true,
                LoopMaps = 1,
                RespawnTime = 4,
                RequireNovaLogin = false,
                AllowCustomSkins = false,
                MaxKills = 19,
                BindAddress = bind_address,
                GamePort = Convert.ToInt32(numericUpDown1.Value),
                TimeLimit = 4,
                StartDelay = 0,
                MinPing = false,
                MinPingValue = 0,
                MaxPing = false,
                MaxPingValue = 0,
                OneShotKills = false,
                FatBullets = false,
                DestroyBuildings = false,
                FriendlyFire = false,
                FriendlyFireWarning = false,
                FriendlyTags = true,
                FriendlyFireKills = 0,
                AllowAutoRange = false,
                AutoBalance = false,
                BluePassword = "",
                RedPassword = "",
                FlagReturnTime = 10,
                MaxTeamLives = 0,
                ShowTeamClays = false,
                ShowTracers = false,
                FBScore = 20,
                KOTHScore = 20,
                ScoreBoardDelay = 45,
                RoleRestrictions = new PlayerRoles
                {
                    CQB = true,
                    Gunner = true,
                    Medic = true,
                    Sniper = true
                },
                WeaponRestrictions = new WeaponsClass
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
                },
                LastUpdateTime = DateTime.Now,
                NextUpdateTime = DateTime.Now.AddSeconds(2.0),
                nextWebStatsStatusUpdate = DateTime.Now.AddSeconds(2.0),
                EnableWebStats = false,
                enableVPNCheck = false,
                WebStatsSoftware = comboBox2.SelectedIndex,
                WebstatsURL = stats_url,
                MapList = new Dictionary<int, MapList>(),
                savedmaprotations = new List<savedmaprotations>(),
                previousMapList = new Dictionary<int, MapList>(),
                WebstatsIdVerified = false,
                ReportMaster = false,
                Status = InstanceStatus.OFFLINE,
                AutoMessages = new auto_messages
                {
                    enable_msg = false,
                    interval = 3,
                    messages = new List<string>(),
                    MsgNumber = 0,
                    NextMessage = DateTime.Now
                },
                BanList = new List<playerbans>(),
                ChangeTeamList = new List<ChangeTeamClass>(),
                CustomWarnings = new List<string>(),
                DisarmPlayers = new List<int>(),
                GodModeList = new List<int>(),
                IPWhiteList = new Dictionary<string, string>(),
                PlayerList = new Dictionary<int, playerlist>(),
                VPNWhiteList = new Dictionary<int, VPNWhiteListClass>(),
                WarningQueue = new List<WarnPlayerClass>(),
                previousTeams = new List<PreviousTeams>(),
                gameCrashCounter = 0,
                CrashRecovery = checkBox2.Checked,
                ReportNovaCC = checkBox2.Checked,
                ReportNovaHQ = checkBox2.Checked,
                Plugins = defaultPlugins,
                IsTeamSabre = TeamSabre,
                availableMaps = new Dictionary<int, MapList>()
            };
            _state.Instances.Add(_state.Instances.Count, newInstance);
            _state.ChatLogs.Add(_state.ChatLogs.Count, new ChatLogs
            {
                CurrentIndex = 0,
                Messages = new BindingList<PlayerChatLog>()
            });
            _state.PlayerStats.Add(_state.PlayerStats.Count, new CollectedPlayerStatsPlayers());
            _state.IPQualityCache.Add(_state.IPQualityCache.Count, new ipqualityscore());
            this.Close();
            // replace close() with database action as well as refresh the main BMT table_profileList
        }
    }
}
