using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class Edit_Profile : Form
    {
        AppState _state;
        int ArrayID;
        public List<string> IPv4Addresses = new List<string>();
        public static bool profileUpdated;
        public Edit_Profile(AppState state, int id)
        {
            InitializeComponent();
            _state = state;
            ArrayID = id;
            IPv4Addresses.Add("0.0.0.0");
            profileUpdated = false;
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
            textBox1.Text = _state.Instances[id].GameName;
            textBox2.Text = _state.Instances[id].GamePath;
            comboBox1.SelectedIndex = _state.Instances[id].GameType;
            comboBox2.SelectedIndex = _state.Instances[id].WebStatsSoftware;
            textBox5.Text = _state.Instances[id].HostName;
            numericUpDown1.Value = _state.Instances[id].GamePort;
            switch (_state.Instances[id].anti_stat_padding)
            {
                case 0:
                    checkBox1.Checked = false;
                    break;
                case 1:
                    checkBox1.Checked = true;
                    numericUpDown2.Value = _state.Instances[id].anti_stat_padding_min_minutes;
                    numericUpDown3.Value = _state.Instances[id].anti_stat_padding_min_players;
                    break;
                default:
                    MessageBox.Show("An unexpected error occurred. Please submit a bug report.\n\r\n\ranti_stat_padding value is not an acceptable value!\n\rError: #10021-517", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    break;
            }
            checkBox2.Checked = _state.Instances[id].CrashRecovery;
            switch (_state.Instances[id].ReportMaster)
            {
                case false:
                    checkBox3.Checked = false;
                    break;
                case true:
                    checkBox3.Checked = true;
                    break;
            }
            switch (_state.Instances[id].misc_show_ranks)
            {
                case false:
                    checkBox4.Checked = false;
                    break;
                case true:
                    checkBox4.Checked = true;
                    break;
            }
            comboBox3.SelectedIndex = _state.Instances[id].misc_left_leaning;
            comboBox4.SelectedItem = _state.Instances[id].BindAddress;
            checkBox7.Checked = _state.Instances[id].ReportMaster;
            checkBox5.Checked = _state.Instances[id].ReportNovaHQ;
            checkBox6.Checked = _state.Instances[id].ReportNovaCC;
            cbList_plugins.SetItemChecked(0, _state.Instances[id].Plugins.WelcomeMessage);
            cbList_plugins.SetItemChecked(1, _state.Instances[id].Plugins.VoteMaps);
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

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(textBox5.Text, "^[a-zA-Z0-9]*$"))
            {
                MessageBox.Show("Please enter a valid HostName!", "Error");
                return;
            }
            _state.Instances[ArrayID].GameName = textBox1.Text;
            _state.Instances[ArrayID].GamePath = textBox2.Text;
            _state.Instances[ArrayID].GameType = comboBox1.SelectedIndex;
            _state.Instances[ArrayID].WebStatsSoftware = comboBox2.SelectedIndex;
            _state.Instances[ArrayID].WebstatsURL = textBox3.Text;
            //_state.Instances[ArrayID].WebStatsId = Convert.ToInt32(textBox4.Text);
            _state.Instances[ArrayID].HostName = textBox5.Text;
            _state.Instances[ArrayID].BindAddress = comboBox4.SelectedItem.ToString();
            _state.Instances[ArrayID].GamePort = Convert.ToInt32(numericUpDown1.Value);
            _state.Instances[ArrayID].anti_stat_padding = Convert.ToInt32(checkBox1.Checked);
            _state.Instances[ArrayID].anti_stat_padding_min_minutes = Convert.ToInt32(numericUpDown2.Value);
            _state.Instances[ArrayID].anti_stat_padding_min_players = Convert.ToInt32(numericUpDown3.Value);
            _state.Instances[ArrayID].CrashRecovery = checkBox2.Checked;
            _state.Instances[ArrayID].ReportMaster = checkBox3.Checked;
            _state.Instances[ArrayID].misc_show_ranks = checkBox4.Checked;
            _state.Instances[ArrayID].misc_left_leaning = comboBox3.SelectedIndex;
            _state.Instances[ArrayID].Plugins.WelcomeMessage = cbList_plugins.GetItemChecked(0);
            _state.Instances[ArrayID].Plugins.VoteMaps = cbList_plugins.GetItemChecked(1);



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
            cmd.Parameters.AddWithValue("@misc_babstats_master", Convert.ToInt32(_state.Instances[ArrayID].ReportMaster));
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
    }
}
