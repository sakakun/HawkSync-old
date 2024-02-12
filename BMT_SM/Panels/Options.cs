using HawkSync_SM.classes;
using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using WatsonTcp;

namespace HawkSync_SM
{
    public partial class Options : Form
    {
        AppState _state;

        DataTable modTable = null;
        bool editEntry = false;

        public static bool EnableWebServer;
        public Options(AppState state)
        {
            InitializeComponent();
            _state = state;

            modTable = new DataTable();
            modTable.Columns.Add("Expansion / Mod Name");
            modTable.Columns.Add("Game");
            modTable.Columns.Add("Args");
            modTable.Columns.Add("PFF File");
            modTable.Columns.Add("Mod Icon", typeof(byte[]));
            dataGridView1.DataSource = modTable;
            dataGridView1.Columns["Expansion / Mod Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Expansion / Mod Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Game"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Game"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Args"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Args"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["PFF File"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["PFF File"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Mod Icon"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Mod Icon"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            chbox_enableRemote.Checked = ProgramConfig.RCEnabled;
            num_remotePort.Value = ProgramConfig.RCPort;
            num_remotePort.Enabled = ProgramConfig.RCEnabled;

            if (ProgramConfig.EnableVPNCheck == true)
            {
                radio_apibtn1.Checked = true;
                radio_apibtn2.Checked = false;
                ipQualityScore_APIKEY.Enabled = true;
            }
            else if (ProgramConfig.EnableVPNCheck == false)
            {
                radio_apibtn1.Checked = false;
                radio_apibtn2.Checked = true;
                ipQualityScore_APIKEY.Enabled = false;
            }
            ipQualityScore_APIKEY.Text = ProgramConfig.ip_quality_score_apikey;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("UPDATE `config` SET `value` = @newValue WHERE `key` = @key;", db);
            // Save() Function
            if (Convert.ToInt32(num_remotePort.Value) != ProgramConfig.RCPort)
            {
                ProgramConfig.RCPort = Convert.ToInt32(num_remotePort.Value);
                cmd.Parameters.AddWithValue("@newValue", ProgramConfig.RCPort);
                cmd.Parameters.AddWithValue("@key", "remote_client_port");
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                if (_state.server.IsListening == true && chbox_enableRemote.Checked == true)
                {
                    _state.server.Stop();
                    WatsonTcpServer setup = _state.server;
                    _state.server = new WatsonTcpServer(null, ProgramConfig.RCPort)
                    {
                        Events = setup.Events,
                        Callbacks = setup.Callbacks,
                        Keepalive = setup.Keepalive,
                        Settings = setup.Settings,
                        SslConfiguration = setup.SslConfiguration
                    };
                    _state.server.Start();
                }
                else
                {
                    _state.server.Start();
                }
            }
            if (ipQualityScore_APIKEY.Text != ProgramConfig.ip_quality_score_apikey)
            {
                ProgramConfig.ip_quality_score_apikey = ipQualityScore_APIKEY.Text;
                cmd.Parameters.AddWithValue("@newValue", ProgramConfig.ip_quality_score_apikey);
                cmd.Parameters.AddWithValue("@key", "ip_quality_score_apikey");
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            if (ProgramConfig.RCEnabled != chbox_enableRemote.Checked)
            {
                cmd.Parameters.AddWithValue("@newValue", Convert.ToInt32(chbox_enableRemote.Checked));
                cmd.Parameters.AddWithValue("@key", "remote_client");
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                ProgramConfig.RCEnabled = chbox_enableRemote.Checked;
                if (chbox_enableRemote.Checked == false && _state.server.IsListening == true)
                {
                    _state.server.Stop();
                }
                else if (chbox_enableRemote.Checked == true && _state.server.IsListening == false)
                {
                    _state.server.Start();
                }
            }
            if (radio_apibtn1.Checked != ProgramConfig.EnableVPNCheck)
            {
                if (radio_apibtn1.Checked)
                {
                    ProgramConfig.EnableVPNCheck = true;
                }
                else
                {
                    ProgramConfig.EnableVPNCheck = false;
                }
                cmd.Parameters.AddWithValue("@newValue", Convert.ToInt32(ProgramConfig.EnableVPNCheck));
                cmd.Parameters.AddWithValue("@key", "check_for_vpn");
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }

            cmd.Dispose();
            db.Close();
            db.Dispose();
            MessageBox.Show("Settings updated successfully!", "Success");
            this.Close();
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            radio_apibtn2.Checked = false;
            ipQualityScore_APIKEY.Enabled = true;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radio_apibtn1.Checked = false;
            ipQualityScore_APIKEY.Text = string.Empty;
            ipQualityScore_APIKEY.Enabled = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://ipqualityscore.com");
        }

        private void Options_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < _state.Mods.Count; i++)
            {
                DataRow newRow = modTable.NewRow();
                newRow["Expansion / Mod Name"] = _state.Mods[i].ModName;
                switch (_state.Mods[i].Game)
                {
                    case 1:
                        newRow["Game"] = "Joint Operations";
                        break;
                    default:
                        newRow["Game"] = "Black Hawk Down";
                        break;
                }
                newRow["Args"] = _state.Mods[i].ExeArgs;
                newRow["PFF File"] = _state.Mods[i].Pff;
                newRow["Mod Icon"] = _state.Mods[i].ModIcon;
                modTable.Rows.Add(newRow);
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
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty || ipQualityScore_APIKEY.Text == string.Empty || textBox5.Text == string.Empty)
            {
                return;
            }
            // save
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            if (editEntry == false)
            {
                SQLiteCommand cmd = new SQLiteCommand(db)
                {
                    CommandType = CommandType.Text,
                    CommandText = "INSERT INTO `mods` (`id`, `name`, `game`, `args`, `pff`, `icon`) VALUES (NULL, @name, @game, @args, @pff, @icon);"
                };
                cmd.Parameters.AddWithValue("@name", textBox1.Text);
                cmd.Parameters.AddWithValue("@game", comboBox1.SelectedIndex);
                cmd.Parameters.AddWithValue("@args", ipQualityScore_APIKEY.Text);
                cmd.Parameters.AddWithValue("@pff", textBox4.Text);

                // upload image to database...
                // setup parameter and declare type.
                SQLiteParameter imgBlob = new SQLiteParameter("@icon", DbType.Binary);

                // load binary data...
                byte[] test = File.ReadAllBytes(textBox5.Text);
                imgBlob.Value = test;
                cmd.Parameters.Add(imgBlob);

                cmd.ExecuteNonQuery();
                cmd.Dispose();

                DataRow newRow = modTable.NewRow();
                newRow["Expansion / Mod Name"] = textBox1.Text;
                switch (comboBox1.SelectedIndex)
                {
                    case 1:
                        newRow["Game"] = "Joint Operations";
                        break;
                    default:
                        newRow["Game"] = "Black Hawk Down";
                        break;
                }

                newRow["Args"] = ipQualityScore_APIKEY.Text;
                newRow["PFF File"] = textBox4.Text;
                newRow["Mod Icon"] = test;
                modTable.Rows.Add(newRow);

                _state.Mods.Add(_state.Mods.Count, new ModsClass
                {
                    Game = comboBox1.SelectedIndex,
                    ExeArgs = ipQualityScore_APIKEY.Text,
                    Id = (int)db.LastInsertRowId,
                    ModIcon = test,
                    ModName = textBox1.Text,
                    Pff = textBox4.Text,
                });
                db.Close();
                db.Dispose();
                textBox1.Text = string.Empty;
                comboBox1.SelectedIndex = 0;
                ipQualityScore_APIKEY.Text = string.Empty;
                textBox4.Text = string.Empty;
                pictureBox1 = new PictureBox();
                editEntry = false;
                MessageBox.Show("Mod saved!");
            }
        }

    }
}
