using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using HawkSync_RC.classes;
using log4net;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_RC
{
    public partial class PlayerInfo : Form
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AppState _state;
        private List<adminNotes> adminNotes;
        private readonly DataTable AdminNotes = new DataTable();
        private readonly DataTable aliases = new DataTable();
        private readonly int ArrayID = -1;
        private readonly string playerAddr = string.Empty;
        private List<playerHistory> playerHistory;
        private IPQualityClass PlayerInfoRC;
        private readonly string playerName = string.Empty;
        private readonly int playerSlot = -1;
        private readonly RCSetup RCSetup;
        private readonly Timer updateInfo;

        public PlayerInfo(AppState state, RCSetup setup, int InstanceID, int SlotNum)
        {
            InitializeComponent();
            _state = state;
            RCSetup = setup;
            playerSlot = SlotNum;
            ArrayID = InstanceID;
            playerName = _state.Instances[ArrayID].PlayerList[playerSlot].name;
            playerAddr = _state.Instances[ArrayID].PlayerList[playerSlot].address;

            // setup player history DataTable...
            aliases.Columns.Add("Name");
            aliases.Columns.Add("IP");

            // setup Admin Notes...
            AdminNotes.Columns.Add("Msg");

            updateInfo = new Timer
            {
                Enabled = true
            };
            updateInfo.Tick += UpdateInfo_Tick;
        }

        private void UpdateInfo_Tick(object sender, EventArgs e)
        {
            if (!_state.Instances[ArrayID].PlayerList.ContainsKey(playerSlot))
            {
                updateInfo.Enabled = false;
                return;
            }

            label23.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            label28.Text = _state.Instances[ArrayID].PlayerList[playerSlot].deaths.ToString();
            if (_state.Instances[ArrayID].PlayerList[playerSlot].deaths != 0)
                label26.Text = (_state.Instances[ArrayID].PlayerList[playerSlot].kills /
                                _state.Instances[ArrayID].PlayerList[playerSlot].deaths).ToString();
            else
                label26.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            label24.Text = _state.Instances[ArrayID].PlayerList[playerSlot].selectedWeapon;
            label36.Text = _state.Instances[ArrayID].PlayerList[playerSlot].PlayerClass;
            label25.Text = _state.Instances[ArrayID].PlayerList[playerSlot].knifekills.ToString();
            label30.Text = _state.Instances[ArrayID].PlayerList[playerSlot].headshots.ToString();
            label32.Text = _state.Instances[ArrayID].PlayerList[playerSlot].psptakeover.ToString();
            label34.Text = _state.Instances[ArrayID].PlayerList[playerSlot].revives.ToString();
            label38.Text = _state.Instances[ArrayID].PlayerList[playerSlot].exp.ToString();
            label42.Text = _state.Instances[ArrayID].PlayerList[playerSlot].teamkills.ToString();
            label40.Text = _state.Instances[ArrayID].PlayerList[playerSlot].suicides.ToString();
            label7.Text = _state.Instances[ArrayID].PlayerList[playerSlot].pspattempts.ToString();
            label44.Text = _state.Instances[ArrayID].PlayerList[playerSlot].totalshots.ToString();
        }

        private void PlayerInfo_Load(object sender, EventArgs e)
        {
            label16.Text = _state.Instances[ArrayID].PlayerList[playerSlot].name;
            label17.Text = _state.Instances[ArrayID].PlayerList[playerSlot].address;

            if (_state.Instances[ArrayID].enableVPNCheck)
            {
                var requestIPInformation = new Dictionary<dynamic, dynamic>
                {
                    { "action", "BMTRC.GetPlayerIPInfo" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "slot", playerSlot }
                };

                var IPInformationresponse =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(requestIPInformation)));
                if ((OpenClass.Status)IPInformationresponse["Status"] == OpenClass.Status.SUCCESS)
                {
                    PlayerInfoRC = JsonConvert.DeserializeObject<IPQualityClass>(IPInformationresponse["PlayerInfo"]);
                }
                else if ((OpenClass.Status)IPInformationresponse["Status"] == OpenClass.Status.FAILURE)
                {
                    MessageBox.Show("Something went wrong! Please try again.", "Error");
                    Close();
                }
            }

            var requestPlayerHistory = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.GetPlayerHistory" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", playerSlot }
            };

            var responsePlayerHistory =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(requestPlayerHistory)));
            if ((OpenClass.Status)responsePlayerHistory["Status"] == OpenClass.Status.SUCCESS)
            {
                playerHistory = JsonConvert.DeserializeObject<List<playerHistory>>(responsePlayerHistory["history"]);
            }
            else if ((OpenClass.Status)responsePlayerHistory["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong! Please try again.", "Error");
                Close();
            }

            var requestAdminNotes = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.GetAdminNotes" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", playerSlot }
            };

            var responseAdminNotes =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(requestAdminNotes)));
            if ((OpenClass.Status)responseAdminNotes["Status"] == OpenClass.Status.SUCCESS)
            {
                adminNotes = JsonConvert.DeserializeObject<List<adminNotes>>(responseAdminNotes["notes"]);
            }
            else if ((OpenClass.Status)responseAdminNotes["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong! Please try again.", "Error");
                Close();
            }

            if (_state.Instances[ArrayID].enableVPNCheck)
            {
                switch (PlayerInfoRC.active_vpn)
                {
                    case true:
                        label18.Text = "True";
                        break;
                    case false:
                        label18.Text = "False";
                        break;
                }

                label19.Text = PlayerInfoRC.fraud_score.ToString();
                label21.Text = PlayerInfoRC.country_code;
                label22.Text = PlayerInfoRC.host;
            }
            else
            {
                label18.Text = "N/A - VPN Check Disabled";
                label19.Text = "N/A - VPN Check Disabled";
                label21.Text = "N/A - VPN Check Disabled";
                label22.Text = "N/A - VPN Check Disabled";
            }

            // check VPN whitelist
            label20.Text = "False";
            for (var whitelist = 0; whitelist < _state.Instances[ArrayID].VPNWhiteList.Count; whitelist++)
                if (_state.Instances[ArrayID].VPNWhiteList[whitelist].IPAddress == IPAddress
                        .Parse(_state.Instances[ArrayID].PlayerList[playerSlot].address).ToString())
                {
                    label20.Text = "True";
                    break;
                }

            for (var banned = 0; banned < _state.Instances[ArrayID].BanList.Count; banned++)
                if (_state.Instances[ArrayID].BanList[banned].ipaddress ==
                    _state.Instances[ArrayID].PlayerList[playerSlot].address)
                {
                    label23.Text = "True";
                    label24.Text = _state.Instances[ArrayID].BanList[banned].reason;
                    break;
                }

            label23.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            label28.Text = _state.Instances[ArrayID].PlayerList[playerSlot].deaths.ToString();
            if (_state.Instances[ArrayID].PlayerList[playerSlot].deaths != 0)
                label26.Text = (_state.Instances[ArrayID].PlayerList[playerSlot].kills /
                                _state.Instances[ArrayID].PlayerList[playerSlot].deaths).ToString();
            else
                label26.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            label24.Text = _state.Instances[ArrayID].PlayerList[playerSlot].selectedWeapon;
            label36.Text = _state.Instances[ArrayID].PlayerList[playerSlot].PlayerClass;
            label25.Text = _state.Instances[ArrayID].PlayerList[playerSlot].knifekills.ToString();
            label30.Text = _state.Instances[ArrayID].PlayerList[playerSlot].headshots.ToString();
            label32.Text = _state.Instances[ArrayID].PlayerList[playerSlot].psptakeover.ToString();
            label34.Text = _state.Instances[ArrayID].PlayerList[playerSlot].revives.ToString();
            label38.Text = _state.Instances[ArrayID].PlayerList[playerSlot].exp.ToString();
            label42.Text = _state.Instances[ArrayID].PlayerList[playerSlot].teamkills.ToString();
            label40.Text = _state.Instances[ArrayID].PlayerList[playerSlot].suicides.ToString();
            label7.Text = _state.Instances[ArrayID].PlayerList[playerSlot].pspattempts.ToString();
            label44.Text = _state.Instances[ArrayID].PlayerList[playerSlot].totalshots.ToString();

            foreach (playerHistory player in playerHistory)
            {
                if ((player.playerName == _state.Instances[ArrayID].PlayerList[playerSlot].name) || (player.playerIP == _state.Instances[ArrayID].PlayerList[playerSlot].address))
                {
                    DataRow newEntry = aliases.NewRow();
                    newEntry["Name"] = player.playerName;
                    newEntry["IP"] = player.playerIP;
                    aliases.Rows.Add(newEntry);
                    if (adminNotes != null && adminNotes.Count > 0)
                    {
                        foreach (adminNotes note in adminNotes)
                        {
                            if (note.userid == player.DatabaseId)
                            {
                                DataRow adminNoteNewEntry = AdminNotes.NewRow();
                                adminNoteNewEntry["Msg"] = note.msg;
                                AdminNotes.Rows.Add(adminNoteNewEntry);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            dataGridView11.DataSource = aliases;
            dataGridView13.DataSource = AdminNotes;
            dataGridView11.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView11.Columns["Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView11.Columns["IP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView11.Columns["IP"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView13.Columns["Msg"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView13.Columns["Msg"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // add new note
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid message", "Error");
                return;
            }

            var requestAddAdminNotes = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.AddAdminNote" },
                { "SessionID", RCSetup.SessionID },
                { "PlayerIP", playerAddr },
                { "PlayerName", playerName },
                { "newMsg", textBox1.Text }
            };

            var responseAddAdminNotes =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(requestAddAdminNotes)));
            if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.SUCCESS)
            {
                var newEntry = AdminNotes.NewRow();
                newEntry["Msg"] = textBox1.Text;
                AdminNotes.Rows.Add(newEntry);
                textBox1.Text = string.Empty;
                MessageBox.Show("Admin Note was successfully added.", "Success");
            }
            else if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong! Please try again.", "Error");
                Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // delete selected note
            if (dataGridView13.CurrentRow == null || dataGridView13.CurrentRow.Index == -1)
            {
                MessageBox.Show("You must select an entry before you can remove it.", "Error");
                return;
            }

            var requestAddAdminNotes = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RemoveAdminNote" },
                { "SessionID", RCSetup.SessionID },
                { "playerName", adminNotes[dataGridView13.CurrentRow.Index].name },
                { "newMsg", adminNotes[dataGridView13.CurrentRow.Index].msg }
            };
            try
            {
                var responseAddAdminNotes =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(requestAddAdminNotes)));
                if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.SUCCESS)
                {
                    var newEntry = AdminNotes.NewRow();
                    newEntry["Msg"] = textBox1.Text;
                    AdminNotes.Rows.Add(newEntry);
                    textBox1.Text = string.Empty;
                    MessageBox.Show("Admin Note was successfully added.", "Success");
                }
                else if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.FAILURE)
                {
                    MessageBox.Show("Something went wrong! Please try again.", "Error");
                    Close();
                }
            }
            catch
            {
                Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // delete all notes
        }
    }
}