using HawkSync_SM.classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace HawkSync_SM
{
    public partial class SM_ServerProfile : Form
    {
        AppState state;
        public Instance newProfile;
        public List<string> IPv4Addresses = new List<string>();
        public bool profileUpdated = false;

        public SM_ServerProfile(AppState _state, int ArrayID)
        {
            InitializeComponent();

            state = _state;
            // Does instance exist? No. "Create new profile" else "Load existing profile"
            if (state.Instances.Count > 0 && ArrayID < state.Instances.Count)
                onload_existingProfile(state, ArrayID);
            else
                onload_freshProfile(state);

        }

        private void onload_freshProfile(AppState state)
        {
            // Set default "form" values. Instance doesn't get made until "btn_submit" button is clicked.
            btn_submit.Text = "Add";
            // Default Values
            cb_serverIP.Items.AddRange(availableIPAddresses().ToArray());
            cb_serverIP.SelectedIndex = 0;
            cb_selectGame.SelectedIndex = 0;
            cb_statSystem.SelectedIndex = 0;
            cb_enableAutoRestart.Checked = true;
            cb_enableAntiPadding.Checked = false;
            cb_enableAnnoucments.Checked = false;
            cb_enableHBNHQ.Checked = true;
            cb_enableHBNW.Checked = true;
        }

        public void event_addProfile(ref AppState _state, int ArrayID)
        {
            try
            {

                // Instance Generation
                newProfile = new Instance();
                newProfile.DataTableColumnId = state.Instances.Count;
                newProfile.GameName = textBox_profileName.Text;
                newProfile.GameType = cb_selectGame.SelectedIndex;
                newProfile.GamePath = textbox_serverPath.Text;
                newProfile.WebStatsSoftware = cb_statSystem.SelectedIndex;
                newProfile.WebstatsURL = textBox_statsURL.Text;
                newProfile.WebStatsId = textBox_statsIdent.Text;
                newProfile.HostName = textBox_hostName.Text;
                newProfile.BindAddress = cb_serverIP.SelectedItem.ToString();
                newProfile.GamePort = Convert.ToInt32(num_serverPort.Value);
                newProfile.anti_stat_padding = Convert.ToInt32(cb_enableAntiPadding.Checked);
                newProfile.CrashRecovery = cb_enableAutoRestart.Checked;
                newProfile.misc_show_ranks = cb_enableAnnoucments.Checked;
                newProfile.ReportNovaHQ = cb_enableHBNHQ.Checked;
                newProfile.ReportNovaCC = cb_enableHBNW.Checked;
                if (cb_selectGame.SelectedIndex == 0 && File.Exists(Path.Combine(newProfile.GamePath, "EXP1.pff")))
                {
                    newProfile.IsTeamSabre = true;
                }

                // SQL Add Instance
                SQLiteConnection m_dbConnection = new SQLiteConnection(ProgramConfig.DBConfig);
                m_dbConnection.Open();

                SQLiteCommand add_profile = new SQLiteCommand("INSERT INTO `instances` (`id`, `name`, `game_type`, `gamepath`, `stats`, `stats_url`, `stats_server_id`, `stats_verified`, `host_name`, `port`, `anti_stat_padding`, `anti_stat_padding_min_minutes`, `anti_stat_padding_min_players`, `misc_crashrecovery`, `misc_babstats_master`, `misc_show_ranks`, `misc_left_leaning`, `bind_address`, `novahq_master`, `novacc_master`, `plugins`) VALUES (NULL, @profilename, @gametype, @gamepath, @statssoftware, @statsurl, @statsserverid, 0, @host, @port, @anti_stat_pad, @anti_stat_padding_min_minutes, @anti_stat_padding_min_players, @misc_crashrecovery, @misc_babstats_master, @announceranks, @leftleaning, @bindingaddress, @novahq_master, @novacc_master, @plugins); ", m_dbConnection);
                add_profile.Parameters.AddWithValue("@profilename", newProfile.GameName);
                add_profile.Parameters.AddWithValue("@gametype", newProfile.GameType);
                add_profile.Parameters.AddWithValue("@gamepath", newProfile.GamePath);
                add_profile.Parameters.AddWithValue("@statssoftware", newProfile.WebStatsSoftware);
                add_profile.Parameters.AddWithValue("@statsurl", newProfile.WebstatsURL);
                add_profile.Parameters.AddWithValue("@statsserverid", newProfile.WebStatsId);
                add_profile.Parameters.AddWithValue("@host", newProfile.HostName);
                add_profile.Parameters.AddWithValue("@port", newProfile.GamePort);
                add_profile.Parameters.AddWithValue("@anti_stat_pad", newProfile.anti_stat_padding);
                add_profile.Parameters.AddWithValue("@anti_stat_padding_min_minutes", newProfile.anti_stat_padding_min_minutes);
                add_profile.Parameters.AddWithValue("@anti_stat_padding_min_players", newProfile.anti_stat_padding_min_players);
                add_profile.Parameters.AddWithValue("@misc_crashrecovery", newProfile.CrashRecovery);
                add_profile.Parameters.AddWithValue("@misc_babstats_master", false);
                add_profile.Parameters.AddWithValue("@announceranks", newProfile.misc_show_ranks);
                add_profile.Parameters.AddWithValue("@leftleaning", newProfile.misc_left_leaning);
                add_profile.Parameters.AddWithValue("@bindingaddress", newProfile.BindAddress);
                add_profile.Parameters.AddWithValue("@novahq_master", newProfile.ReportNovaHQ);
                add_profile.Parameters.AddWithValue("@novacc_master", newProfile.ReportNovaCC);
                add_profile.Parameters.AddWithValue("@plugins", JsonConvert.SerializeObject(new PluginsClass()));
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
                m_dbConnection.Close();

                // Add Instance to Current AppState
                _state.Instances.Add(ArrayID, newProfile);
                _state.ChatLogs.Add(_state.ChatLogs.Count, new ob_ChatLogs
                {
                    CurrentIndex = 0,
                    Messages = new BindingList<ob_PlayerChatLog>()
                });
                //_state.PlayerStats.Add(_state.PlayerStats.Count, new CollectedPlayerStatsPlayers());
                _state.IPQualityCache.Add(_state.IPQualityCache.Count, new ipqualityscore());


            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while trying to add the profile to the database. Please submit a bug report.\n\r\n\rError: " + ex.Message, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            this.Hide();
        }

        public void event_editProfile(ref AppState _state, int ArrayID)
        {
            if (!Regex.IsMatch(textBox_hostName.Text, "^[a-zA-Z0-9-]*$"))
            {
                MessageBox.Show("Please enter a valid HostName!", "Error");
                return;
            }
            _state.Instances[ArrayID].GameName = textBox_profileName.Text;
            _state.Instances[ArrayID].GamePath = textbox_serverPath.Text;
            _state.Instances[ArrayID].GameType = cb_selectGame.SelectedIndex;
            _state.Instances[ArrayID].WebStatsSoftware = cb_statSystem.SelectedIndex;
            _state.Instances[ArrayID].WebstatsURL = textBox_statsURL.Text;
            _state.Instances[ArrayID].WebStatsId = textBox_statsIdent.Text;
            _state.Instances[ArrayID].HostName = textBox_hostName.Text;
            _state.Instances[ArrayID].BindAddress = cb_serverIP.SelectedItem.ToString();
            _state.Instances[ArrayID].GamePort = Convert.ToInt32(num_serverPort.Value);
            _state.Instances[ArrayID].anti_stat_padding = Convert.ToInt32(cb_enableAntiPadding.Checked);
            _state.Instances[ArrayID].CrashRecovery = cb_enableAutoRestart.Checked;
            _state.Instances[ArrayID].misc_show_ranks = cb_enableAnnoucments.Checked;

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances` SET `name` = @profilename, `gamepath` = @gamepath, `game_type` = @gametype, `stats` = @stats, `stats_url` = @statsurl, `stats_server_id` = @statsserverid, `host_name` = @hostname, `bind_address` = @bindaddress, `port` = @serverport, `anti_stat_padding` = @anti_stat_padding, `anti_stat_padding_min_minutes` = @anti_stat_padding_min_minutes, `anti_stat_padding_min_players` = @anti_stat_padding_min_players, `misc_crashrecovery` = @misc_CrashRecovery, `misc_babstats_master` = @misc_babstats_master, `misc_show_ranks` = @misc_show_ranks, `misc_left_leaning` = @misc_left_leaning, `novahq_master` = @novahq, `novacc_master` = @novacc, `plugins` = @plugins WHERE `id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@profilename", _state.Instances[ArrayID].GameName);
            cmd.Parameters.AddWithValue("@gamepath", _state.Instances[ArrayID].GamePath);
            cmd.Parameters.AddWithValue("@gametype", _state.Instances[ArrayID].GameType);
            cmd.Parameters.AddWithValue("@stats", _state.Instances[ArrayID].WebStatsSoftware);
            cmd.Parameters.AddWithValue("@statsurl", _state.Instances[ArrayID].WebstatsURL);
            cmd.Parameters.AddWithValue("@statsserverid", _state.Instances[ArrayID].WebStatsId);
            cmd.Parameters.AddWithValue("@hostname", _state.Instances[ArrayID].HostName);
            cmd.Parameters.AddWithValue("@bindaddress", _state.Instances[ArrayID].BindAddress);
            cmd.Parameters.AddWithValue("@serverport", _state.Instances[ArrayID].GamePort);
            cmd.Parameters.AddWithValue("@anti_stat_padding", _state.Instances[ArrayID].anti_stat_padding);
            cmd.Parameters.AddWithValue("@anti_stat_padding_min_minutes", _state.Instances[ArrayID].anti_stat_padding_min_minutes);
            cmd.Parameters.AddWithValue("@anti_stat_padding_min_players", _state.Instances[ArrayID].anti_stat_padding_min_players);
            cmd.Parameters.AddWithValue("@misc_CrashRecovery", Convert.ToInt32(_state.Instances[ArrayID].CrashRecovery));
            cmd.Parameters.AddWithValue("@misc_babstats_master", 1);
            cmd.Parameters.AddWithValue("@misc_show_ranks", _state.Instances[ArrayID].misc_show_ranks);
            cmd.Parameters.AddWithValue("@misc_left_leaning", _state.Instances[ArrayID].misc_left_leaning);
            cmd.Parameters.AddWithValue("@novahq", Convert.ToInt32(_state.Instances[ArrayID].ReportNovaHQ));
            cmd.Parameters.AddWithValue("@novacc", Convert.ToInt32(_state.Instances[ArrayID].ReportNovaCC));
            cmd.Parameters.AddWithValue("@plugins", JsonConvert.SerializeObject(_state.Instances[ArrayID].Plugins));
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
            db.Dispose();
            profileUpdated = true;

            this.Hide();
        }

        private void onload_existingProfile(AppState state, int ArrayID)
        {
            cb_serverIP.Items.AddRange(availableIPAddresses().ToArray());
            cb_serverIP.SelectedIndex = cb_serverIP.FindStringExact(state.Instances[ArrayID].BindAddress);
            cb_selectGame.SelectedIndex = state.Instances[ArrayID].GameType;
            cb_statSystem.SelectedIndex = state.Instances[ArrayID].WebStatsSoftware;
            cb_enableAutoRestart.Checked = state.Instances[ArrayID].CrashRecovery;
            cb_enableAntiPadding.Checked = Convert.ToBoolean(state.Instances[ArrayID].anti_stat_padding);
            cb_enableAnnoucments.Checked = Convert.ToBoolean(state.Instances[ArrayID].misc_show_ranks);
            cb_enableHBNHQ.Checked = Convert.ToBoolean(state.Instances[ArrayID].ReportNovaHQ);
            cb_enableHBNW.Checked = Convert.ToBoolean(state.Instances[ArrayID].ReportNovaCC);
            textBox_profileName.Text = state.Instances[ArrayID].GameName;
            textbox_serverPath.Text = state.Instances[ArrayID].GamePath;
            textBox_statsURL.Text = state.Instances[ArrayID].WebstatsURL;
            textBox_statsIdent.Text = state.Instances[ArrayID].WebStatsId;
            textBox_hostName.Text = state.Instances[ArrayID].HostName;
            num_serverPort.Value = state.Instances[ArrayID].GamePort;

        }

        private List<string> availableIPAddresses()
        {
            var ipv4Addresses = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.Address.ToString())
                .ToList();

            ipv4Addresses.Insert(0, "0.0.0.0");
            return ipv4Addresses;
        }

        private void event_webStatsChanged(object sender, EventArgs e)
        {
            switch (cb_statSystem.SelectedItem)
            {
                case "None":
                    textBox_statsURL.Enabled = false;
                    textBox_statsIdent.Enabled = false;
                    cb_enableAnnoucments.Enabled = false;
                    cb_enableAntiPadding.Enabled = false;
                    break;
                default:
                    textBox_statsURL.Enabled = true;
                    textBox_statsIdent.Enabled = true;
                    cb_enableAnnoucments.Enabled = true;
                    cb_enableAntiPadding.Enabled = true;
                    break;
            }
        }

        private void event_openFileBrowser(object sender, EventArgs e)
        {
            string TextBoxValue = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                TextBoxValue = folderBrowserDialog1.SelectedPath;
            }
            textbox_serverPath.Text = TextBoxValue;
        }

        public void btn_hideWindow(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(textBox_hostName.Text, "^[a-zA-Z0-9-]*$"))
            {
                MessageBox.Show("Please enter a valid HostName!", "Error");
                return;
            }
            // sanity checks, and form validation:
            if (textBox_profileName.Text == string.Empty)
            {
                DialogResult result = MessageBox.Show("You MUST enter a Profile Name!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (textbox_serverPath.Text == string.Empty)
            {
                DialogResult result = MessageBox.Show("You MUST enter a Game Server Path!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (cb_selectGame.SelectedIndex == -1)
            {
                DialogResult result = MessageBox.Show("You MUST select the Server Game!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (textBox_hostName.Text == string.Empty)
            {
                DialogResult result = MessageBox.Show("You MUST a Host Name!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }
            if (num_serverPort.Value < 1024 || num_serverPort.Value > 49151)
            {
                DialogResult result = MessageBox.Show("You need to select a port number between 1024 - 49151!", "Error", MessageBoxButtons.OK);
                if (result == DialogResult.OK) return;
            }

            this.profileUpdated = true;
            this.Hide();
        }

        private void event_closeWindow(object sender, EventArgs e)
        {
            // cancel do nothing
            this.Close();
        }

    }
}
