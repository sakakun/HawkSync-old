using System;
using System.Data;
using System.Data.SQLite;
using System.Net;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_SM
{
    public partial class PlayerInfo : Form
    {
        AppState _state;
        int playerSlot = -1;
        int ArrayID = -1;
        ipqualityClass ipquality;
        DataTable aliases = new DataTable();
        DataTable AdminNotes = new DataTable();
        int userid = -1;
        string playerName = "";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Timer updateInfo;
        public PlayerInfo(AppState state, int InstanceID, int SlotNum)
        {
            InitializeComponent();
            _state = state;
            playerSlot = SlotNum;
            ArrayID = InstanceID;
            playerName = _state.Instances[ArrayID].PlayerList[playerSlot].name;
            updateInfo = new Timer
            {
                Enabled = true
            };
            updateInfo.Tick += UpdateInfo_Tick;

            // setup player history DataTable...
            aliases.Columns.Add("Name");
            aliases.Columns.Add("IP");

            // setup Admin Notes...
            AdminNotes.Columns.Add("Msg");

            foreach (var player in _state.playerHistories)
            {
                if (player.playerName == _state.Instances[ArrayID].PlayerList[playerSlot].name && player.playerIP == _state.Instances[ArrayID].PlayerList[playerSlot].address)
                {
                    userid = player.DatabaseId;
                    break;
                }
            }
            if (userid == -1)
            {
                if (ProgramConfig.ApplicationDebug)
                {
                    log.Error("Something went wrong while fetching Player History Users...");
                }
                MessageBox.Show("Something went wrong while fetching Player History Users...\nPlease report this to Babstats.net Staff.");
            }
        }

        private void UpdateInfo_Tick(object sender, EventArgs e)
        {
            if (!_state.Instances[ArrayID].PlayerList.ContainsKey(playerSlot))
            {
                updateInfo.Enabled = false;
                return; // since the slot is empty
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

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PlayerInfo_Load(object sender, EventArgs e)
        {
            label16.Text = _state.Instances[ArrayID].PlayerList[playerSlot].name;
            label17.Text = _state.Instances[ArrayID].PlayerList[playerSlot].address;
            for (int i = 0; i < _state.IPQualityCache[ArrayID].IPInformation.Count; i++)
            {
                if (_state.IPQualityCache[ArrayID].IPInformation[i].address == _state.Instances[ArrayID].PlayerList[playerSlot].address)
                {
                    ipquality = _state.IPQualityCache[ArrayID].IPInformation[i];
                    break;
                }
            }
            if (ipquality != null)
            {
                switch (ipquality.active_vpn)
                {
                    case true:
                        label18.Text = "True";
                        break;
                    case false:
                        label18.Text = "False";
                        break;
                }
                label19.Text = ipquality.fraud_score.ToString();
                label20.Text = "False";
                for (int whitelist = 0; whitelist < _state.Instances[ArrayID].VPNWhiteList.Count; whitelist++)
                {
                    if (_state.Instances[ArrayID].VPNWhiteList[whitelist].IPAddress == IPAddress.Parse(_state.Instances[ArrayID].PlayerList[playerSlot].address).ToString())
                    {
                        label20.Text = "True";
                        break;
                    }
                }
                label21.Text = ipquality.country_code;
                label22.Text = ipquality.host;
            }

            label23.Text = "False";
            label24.Text = "N/A";
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
                label26.Text = ((decimal)_state.Instances[ArrayID].PlayerList[playerSlot].kills / _state.Instances[ArrayID].PlayerList[playerSlot].deaths).ToString();
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
            foreach (var player in _state.playerHistories)
            {
                if ((player.playerName == _state.Instances[ArrayID].PlayerList[playerSlot].name) || (player.playerIP == _state.Instances[ArrayID].PlayerList[playerSlot].address))
                {
                    DataRow newEntry = aliases.NewRow();
                    newEntry["Name"] = player.playerName;
                    newEntry["IP"] = player.playerIP;
                    aliases.Rows.Add(newEntry);
                    foreach (var note in _state.adminNotes)
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
            int PlayerID = -1;
            bool found = false;
            foreach (var player in _state.playerHistories)
            {
                if ((player.playerName == _state.Instances[ArrayID].PlayerList[playerSlot].name) && (player.playerIP == _state.Instances[ArrayID].PlayerList[playerSlot].address))
                {
                    found = true;
                    PlayerID = player.DatabaseId;
                    break;
                }
            }
            if (found == false || PlayerID == -1)
            {
                if (ProgramConfig.ApplicationDebug)
                {
                    log.Error("Something went wrong when attempting to form a Player ID.");
                }
                MessageBox.Show("Something went wrong when attempting to form a Player ID.", "Error");
                return;
            }
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                _state.adminNotes.Add(new adminnotes
                {
                    userid = PlayerID,
                    name = _state.Instances[ArrayID].PlayerList[playerSlot].name,
                    msg = textBox1.Text
                });
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `adminnotes` (`userid`, `name`, `msg`) VALUES (@newid, @playername, @msg);", db);
                cmd.Parameters.AddWithValue("@newid", PlayerID);
                cmd.Parameters.AddWithValue("@playername", _state.Instances[ArrayID].PlayerList[playerSlot].name);
                cmd.Parameters.AddWithValue("@msg", textBox1.Text);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
                db.Dispose();
                DataRow newMsgEntry = AdminNotes.NewRow();
                newMsgEntry["Msg"] = textBox1.Text;
                AdminNotes.Rows.Add(newMsgEntry);
                textBox1.Text = string.Empty;
                MessageBox.Show("The message has been successfully added!", "Success");
            }
            else
            {
                MessageBox.Show("Please enter a valid message.", "Error");
                return;
            }
        }

        private void PlayerInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();

                e.SuppressKeyPress = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure", "Confirm", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `adminnotes` WHERE `name`= @playername;", db);
                cmd.Parameters.AddWithValue("@playername", _state.Instances[ArrayID].PlayerList[playerSlot].name);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
                db.Dispose();
                for (int i = 0; i < _state.adminNotes.Count; i++)
                {
                    if (_state.adminNotes[i].name == _state.Instances[ArrayID].PlayerList[playerSlot].name)
                    {
                        _state.adminNotes.RemoveAt(i);
                    }
                }
                AdminNotes.Rows.Clear();
                MessageBox.Show("Messages cleared!", "Success");
            }
            else if (result == DialogResult.No)
            {
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete this message?", "Confirm", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                int selectedIndex = dataGridView13.CurrentCell.RowIndex;
                string msg = AdminNotes.Rows[selectedIndex]["Msg"].ToString();
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `adminnotes` WHERE `name` = @playername AND `msg` = @msg;", db);
                cmd.Parameters.AddWithValue("@playername", playerName);
                cmd.Parameters.AddWithValue("@msg", msg);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
                db.Dispose();
                AdminNotes.Rows.RemoveAt(selectedIndex);

                int noteIndex = 0;
                foreach (var note in _state.adminNotes)
                {
                    if ((note.name == _state.Instances[ArrayID].PlayerList[playerSlot].name) && (note.msg == msg))
                    {
                        break;
                    }
                    noteIndex++;
                }

                _state.adminNotes.RemoveAt(noteIndex);

                MessageBox.Show("Message Deleted!", "Success");
            }
            else if (result == DialogResult.No)
            {
                return;
            }
        }
    }
}
