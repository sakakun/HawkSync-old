using HawkSync_RC.classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_RC
{
    public partial class PlayerInfo : Form
    {
        AppState _state;
        RCSetup RCSetup;
        int playerSlot = -1;
        int ArrayID = -1;
        IPQualityClass PlayerInfoRC;
        List<playerHistory> playerHistory;
        List<adminNotes> adminNotes;
        DataTable aliases = new DataTable();
        DataTable AdminNotes = new DataTable();
        string playerName = string.Empty;
        string playerAddr = string.Empty;
        Timer updateInfo;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            {
                label26.Text = (_state.Instances[ArrayID].PlayerList[playerSlot].kills / _state.Instances[ArrayID].PlayerList[playerSlot].deaths).ToString();
            }
            else
            {
                label26.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            }
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
                Dictionary<dynamic, dynamic> requestIPInformation = new Dictionary<dynamic, dynamic>
                {
                    { "action", "BMTRC.GetPlayerIPInfo" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "slot", playerSlot }
                };

                Dictionary<string, dynamic> IPInformationresponse = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.ASCII.GetString(RCSetup.SendCMD(requestIPInformation)));
                if ((OpenClass.Status)IPInformationresponse["Status"] == OpenClass.Status.SUCCESS)
                {
                    PlayerInfoRC = JsonConvert.DeserializeObject<IPQualityClass>(IPInformationresponse["PlayerInfo"]);

                }
                else if ((OpenClass.Status)IPInformationresponse["Status"] == OpenClass.Status.FAILURE)
                {
                    MessageBox.Show("Something went wrong! Please try again.", "Error");
                    this.Close();
                }
            }

            Dictionary<dynamic, dynamic> requestPlayerHistory = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.GetPlayerHistory" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", playerSlot }
            };

            Dictionary<string, dynamic> responsePlayerHistory = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.ASCII.GetString(RCSetup.SendCMD(requestPlayerHistory)));
            if ((OpenClass.Status)responsePlayerHistory["Status"] == OpenClass.Status.SUCCESS)
            {
                playerHistory = JsonConvert.DeserializeObject<List<playerHistory>>(responsePlayerHistory["history"]);

            }
            else if ((OpenClass.Status)responsePlayerHistory["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong! Please try again.", "Error");
                this.Close();
            }
            Dictionary<dynamic, dynamic> requestAdminNotes = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.GetAdminNotes" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", playerSlot }
            };

            Dictionary<string, dynamic> responseAdminNotes = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.ASCII.GetString(RCSetup.SendCMD(requestAdminNotes)));
            if ((OpenClass.Status)responseAdminNotes["Status"] == OpenClass.Status.SUCCESS)
            {
                adminNotes = JsonConvert.DeserializeObject<List<adminNotes>>(responseAdminNotes["notes"]);

            }
            else if ((OpenClass.Status)responseAdminNotes["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong! Please try again.", "Error");
                this.Close();
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
            for (int whitelist = 0; whitelist < _state.Instances[ArrayID].VPNWhiteList.Count; whitelist++)
            {
                if (_state.Instances[ArrayID].VPNWhiteList[whitelist].IPAddress == IPAddress.Parse(_state.Instances[ArrayID].PlayerList[playerSlot].address).ToString())
                {
                    label20.Text = "True";
                    break;
                }
            }

            for (int banned = 0; banned < _state.Instances[ArrayID].BanList.Count; banned++)
            {
                if (_state.Instances[ArrayID].BanList[banned].ipaddress == _state.Instances[ArrayID].PlayerList[playerSlot].address)
                {
                    label23.Text = "True";
                    label24.Text = _state.Instances[ArrayID].BanList[banned].reason;
                    break;
                }
            }
            label23.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            label28.Text = _state.Instances[ArrayID].PlayerList[playerSlot].deaths.ToString();
            if (_state.Instances[ArrayID].PlayerList[playerSlot].deaths != 0)
            {
                label26.Text = (_state.Instances[ArrayID].PlayerList[playerSlot].kills / _state.Instances[ArrayID].PlayerList[playerSlot].deaths).ToString();
            }
            else
            {
                label26.Text = _state.Instances[ArrayID].PlayerList[playerSlot].kills.ToString();
            }
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
            foreach (var player in playerHistory)
            {
                DataRow newEntry = aliases.NewRow();
                newEntry["Name"] = player.playerName;
                newEntry["IP"] = player.playerIP;
                aliases.Rows.Add(newEntry);
                /*foreach (var note in _state.adminNotes)
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
                }*/
            }
            foreach (var note in adminNotes)
            {
                DataRow newEntry = AdminNotes.NewRow();
                newEntry["Msg"] = note.msg;
                AdminNotes.Rows.Add(newEntry);
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
            Dictionary<dynamic, dynamic> requestAddAdminNotes = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.AddAdminNote" },
                { "SessionID", RCSetup.SessionID },
                { "PlayerIP", playerAddr },
                { "PlayerName", playerName },
                { "newMsg", textBox1.Text }
            };

            Dictionary<string, dynamic> responseAddAdminNotes = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.ASCII.GetString(RCSetup.SendCMD(requestAddAdminNotes)));
            if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.SUCCESS)
            {
                DataRow newEntry = AdminNotes.NewRow();
                newEntry["Msg"] = textBox1.Text;
                AdminNotes.Rows.Add(newEntry);
                textBox1.Text = string.Empty;
                MessageBox.Show("Admin Note was successfully added.", "Success");
            }
            else if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong! Please try again.", "Error");
                this.Close();
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
            Dictionary<dynamic, dynamic> requestAddAdminNotes = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.RemoveAdminNote" },
                { "SessionID", RCSetup.SessionID },
                { "playerName", adminNotes[dataGridView13.CurrentRow.Index].name },
                { "newMsg", adminNotes[dataGridView13.CurrentRow.Index].msg }
            };
            try
            {
                Dictionary<string, dynamic> responseAddAdminNotes = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.ASCII.GetString(RCSetup.SendCMD(requestAddAdminNotes)));
                if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.SUCCESS)
                {
                    DataRow newEntry = AdminNotes.NewRow();
                    newEntry["Msg"] = textBox1.Text;
                    AdminNotes.Rows.Add(newEntry);
                    textBox1.Text = string.Empty;
                    MessageBox.Show("Admin Note was successfully added.", "Success");
                }
                else if ((OpenClass.Status)responseAddAdminNotes["Status"] == OpenClass.Status.FAILURE)
                {
                    MessageBox.Show("Something went wrong! Please try again.", "Error");
                    this.Close();
                }
            }
            catch
            {
                this.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // delete all notes
        }
    }
}
