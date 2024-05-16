using HawkSync_SM.classes;
using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Web.Http.Results;
using System.Windows.Forms;
using WatsonTcp;

namespace HawkSync_SM
{
    public partial class SM_Options : Form
    {
        AppState _state;

        DataTable modTable = null;

        public static bool EnableWebServer;
        public SM_Options(AppState state)
        {
            InitializeComponent();
            _state = state;

            modTable = new DataTable();
            modTable.Columns.Add("Expansion / profileGameMod Name");
            modTable.Columns.Add("Game");
            modTable.Columns.Add("Args");
            modTable.Columns.Add("PFF File");
            modTable.Columns.Add("profileGameMod Icon", typeof(byte[]));
            dataGridView1.DataSource = modTable;
            dataGridView1.Columns["Expansion / profileGameMod Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Expansion / profileGameMod Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Game"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Game"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Args"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Args"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["PFF File"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["PFF File"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["profileGameMod Icon"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["profileGameMod Icon"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            chbox_enableRemote.Checked = ProgramConfig.RCEnabled;
            num_remotePort.Value = ProgramConfig.RCPort;
            num_remotePort.Enabled = ProgramConfig.RCEnabled;

            cb_enableWFB.Checked = ProgramConfig.EnableWFB;

            cb_enableVPNChecks.Checked = ProgramConfig.EnableVPNCheck;
            ipQualityScore_APIKEY.Text = ProgramConfig.ip_quality_score_apikey;

        }

        private void event_saveClose(object sender, EventArgs e)
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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
            if (cb_enableVPNChecks.Checked != ProgramConfig.EnableVPNCheck)
            {
                ProgramConfig.EnableVPNCheck = cb_enableVPNChecks.Checked;
                cmd.Parameters.AddWithValue("@newValue", Convert.ToInt32(cb_enableVPNChecks.Checked));
                cmd.Parameters.AddWithValue("@key", "enable_vpnChecking");
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
            if (cb_enableWFB.Checked != ProgramConfig.EnableWFB)
            {
                DialogResult result = DialogResult.OK;
                if (ProgramConfig.EnableWFB == true)
                {
                    result = MessageBox.Show("Active servers will no longer get updated firewall records.\nRemaining Firewall records for active servers will be deleted, once server stopped. Continue?", "Warning!", MessageBoxButtons.OKCancel);
                }
                if (result == DialogResult.Cancel && ProgramConfig.EnableWFB == true)
                {
                    MessageBox.Show("Changes to WFB skipped!", "Success");
                }
                else
                {
                    ProgramConfig.EnableWFB = cb_enableWFB.Checked;
                    cmd.Parameters.AddWithValue("@newValue", Convert.ToInt32(cb_enableWFB.Checked));
                    cmd.Parameters.AddWithValue("@key", "enable_wfb");
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }

            }

            cmd.Dispose();
            db.Close();
            db.Dispose();
            MessageBox.Show("Settings updated successfully!", "Success");
            this.Close();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < _state.Mods.Count; i++)
            {
                DataRow newRow = modTable.NewRow();
                newRow["Expansion / profileGameMod Name"] = _state.Mods[i].ModName;
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
                newRow["profileGameMod Icon"] = _state.Mods[i].ModIcon;
                modTable.Rows.Add(newRow);
            }
        }

        private void link_Browse2IPQS(object sender, EventArgs e)
        {
            Process.Start("https://ipqualityscore.com");
        }

        private void event_vpnCheckingChanged(object sender, EventArgs e)
        {
            ipQualityScore_APIKEY.Enabled = cb_enableVPNChecks.Checked;
        }
    }
}
