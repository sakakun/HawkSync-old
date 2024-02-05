using Equin.ApplicationFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_SM
{
    public partial class SM_ServerManager : Form
    {
        public DataTable adminTable = new DataTable();
        public DataTable playersTable = new DataTable();
        public DataTable specificPlayerTable = new DataTable();
        BindingList<PlayerChatLog> playerMessages = new BindingList<PlayerChatLog>();

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

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public Process process;
        public int Profile_ID;
        public int ArrayID;
        private AppState _state;
        DataTable chatLog = new DataTable();
        BanPlayerFunction cmdPlayer;
        public static List<MapList> selectedMaps;
        Dictionary<int, MapList> availableMaps;
        bool MapListEdited = false;
        Timer ServerManagerTimer;
        Timer playerTimer;
        DataTable bannedPlayersTable;
        bool IgnoreBMTMsgs = false;

        public SM_ServerManager(AppState state, int id)
        {
            InitializeComponent();
            _state = state;

            // set ID to global form...
            Profile_ID = _state.Instances[id].Id;
            ArrayID = id;
            ServerManagerTimer = new Timer
            {
                Interval = 2,
                Enabled = true
            };
            ServerManagerTimer.Tick += ServerManagerTimer_Tick;
        }

        private void ServerManagerTimer_Tick(object sender, EventArgs e)
        {
            label_currentMapPlaying.Text = _state.Instances[ArrayID].Map;
        }

        private void CustomWarningMenu(object sender, EventArgs e)
        {
            int selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            _state.Instances[ArrayID].WarningQueue.Add(new WarnPlayerClass
            {
                slot = selectedPlayerSlot,
                warningMsg = sender.ToString()
            });
            MessageBox.Show("Player has been warned!", "Success");
        }

        private void PlayerTimer_Tick(object sender, EventArgs e)
        {
            CheckInstanceStatus();
            UpdatePlayerlist();
            UpdatePlayerCounter();
            UpdateBanList();
            //UpdateChatlogDisplay();
            //UpdateChatlogColors();
        }

        private void UpdatePlayerCounter()
        {
            group_currentPlayers.Text = "Current Players: " + _state.Instances[ArrayID].PlayerList.Count.ToString();
        }

        private void UpdateBanList()
        {
            if (_state.Instances[ArrayID].BanList.Count == 0)
            {
                return;
            }
            else
            {
                foreach (var ban in _state.Instances[ArrayID].BanList)
                {
                    DataRow[] displayedBans = bannedPlayersTable.Select("`Name` = '" + ban.player + "' AND `IP Address` = '" + ban.ipaddress + "'");
                    if (displayedBans.Length == 0)
                    {
                        DataRow newRow = bannedPlayersTable.NewRow();
                        newRow["Name"] = ban.player;
                        newRow["IP Address"] = ban.ipaddress;
                        if (ban.expires == "-1")
                        {
                            newRow["Time Remaining"] = "Permanent";
                        }
                        else
                        {
                            newRow["Time Remaining"] = ban.expires;
                        }
                        bannedPlayersTable.Rows.Add(newRow);
                    }
                }
            }
        }

        private void CheckInstanceStatus()
        {
            if (_state.Instances[ArrayID].Status == InstanceStatus.OFFLINE)
            {
                this.Close();
                return;
            }
        }

        private void UpdateChatlogColors()
        {
            for (int i = 0; i < chatLog.Rows.Count; i++)
            {
                DataRow dr = chatLog.Rows[i];
                string dateTime = dr["Date & Time"].ToString();
                string playerName = dr["Player Name"].ToString();
                playerlist.Teams team = playerlist.Teams.TEAM_SPEC;

                foreach (var item in _state.ChatLogs[ArrayID].Messages)
                {
                    string searchPlayer = item.PlayerName;
                    string searchDateTime = item.dateSent.ToString();
                    if (searchPlayer == playerName && searchDateTime == dateTime)
                    {
                        if (item.msgType == "Team")
                        {
                            if (item.team == String.Empty)
                            {
                                foreach (var player in _state.Instances[ArrayID].PlayerList)
                                {
                                    if (player.Value.name == playerName)
                                    {
                                        team = (playerlist.Teams)Enum.Parse(typeof(playerlist.Teams), player.Value.team.ToString());
                                    }
                                }
                            }
                            else
                            {
                                team = (playerlist.Teams)Enum.Parse(typeof(playerlist.Teams), item.team);
                            }
                            break;
                        }
                    }
                }

                switch (team)
                {
                    case playerlist.Teams.TEAM_GREEN:
                        data_chatViewer.Rows[i].Cells[0].Style.BackColor = Color.Green;
                        data_chatViewer.Rows[i].Cells[1].Style.BackColor = Color.Green;
                        data_chatViewer.Rows[i].Cells[2].Style.BackColor = Color.Green;
                        break;
                    case playerlist.Teams.TEAM_BLUE:
                        data_chatViewer.Rows[i].Cells[0].Style.BackColor = Color.Blue;
                        data_chatViewer.Rows[i].Cells[1].Style.BackColor = Color.Blue;
                        data_chatViewer.Rows[i].Cells[2].Style.BackColor = Color.Blue;
                        break;
                    case playerlist.Teams.TEAM_RED:
                        data_chatViewer.Rows[i].Cells[0].Style.BackColor = Color.Red;
                        data_chatViewer.Rows[i].Cells[1].Style.BackColor = Color.Red;
                        data_chatViewer.Rows[i].Cells[2].Style.BackColor = Color.Red;
                        break;
                    case playerlist.Teams.TEAM_YELLOW:
                        data_chatViewer.Rows[i].Cells[0].Style.BackColor = Color.Yellow;
                        data_chatViewer.Rows[i].Cells[1].Style.BackColor = Color.Yellow;
                        data_chatViewer.Rows[i].Cells[2].Style.BackColor = Color.Yellow;
                        break;
                    case playerlist.Teams.TEAM_PURPLE:
                        data_chatViewer.Rows[i].Cells[0].Style.BackColor = Color.Purple;
                        data_chatViewer.Rows[i].Cells[1].Style.BackColor = Color.Purple;
                        data_chatViewer.Rows[i].Cells[2].Style.BackColor = Color.Purple;
                        break;
                    case playerlist.Teams.TEAM_SPEC:
                        data_chatViewer.Rows[i].Cells[0].Style.BackColor = Color.White;
                        data_chatViewer.Rows[i].Cells[1].Style.BackColor = Color.White;
                        data_chatViewer.Rows[i].Cells[2].Style.BackColor = Color.White;
                        break;
                }
            }
        }

        private void UpdateChatlogDisplay()
        {
            if (_state.ChatLogs[ArrayID].Messages.Count > 0)
            {
                //....// to do...
            }
        }

        private void UpdatePlayerlist()
        {
            Dictionary<int, Instance> instanceList = _state.Instances;
            // update current players
            if (instanceList[ArrayID].PlayerList.Count == playersTable.Rows.Count)
            {
                foreach (DataRow playerData in playersTable.Rows)
                {
                    try
                    {
                        playerlist serverData = _state.Instances[ArrayID].PlayerList[Convert.ToInt32(playerData["Slot #"])];

                        if (serverData == null)
                        {
                            break;
                        }

                        playerData["Name"] = serverData.name;
                        playerData["Address"] = serverData.address;
                        playerData["Ping"] = serverData.ping;
                        BindingSource Bnd = new BindingSource()
                        {
                            DataSource = playersTable
                        };
                        int index = Bnd.Find("Slot #", playerData["Slot #"]);
                        if (index == -1)
                        {
                            return;
                        }
                        switch (serverData.team)
                        {
                            case (int)playerlist.Teams.TEAM_GREEN:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Green;
                                break;
                            case (int)playerlist.Teams.TEAM_BLUE:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Blue;
                                break;
                            case (int)playerlist.Teams.TEAM_RED:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Red;
                                break;
                            case (int)playerlist.Teams.TEAM_YELLOW:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Yellow;
                                break;
                            case (int)playerlist.Teams.TEAM_PURPLE:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Purple;
                                break;
                            case (int)playerlist.Teams.TEAM_SPEC:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.White;
                                break;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else if (_state.Instances[ArrayID].PlayerList.Count < playersTable.Rows.Count)
            {
                foreach (DataRow playerData in playersTable.Rows)
                {
                    if (_state.Instances[ArrayID].PlayerList.ContainsKey(Convert.ToInt32(playerData["Slot #"])) == false)
                    {
                        playersTable.Rows.Remove(playerData);
                        break;
                    }
                }
            }
            else if (_state.Instances[ArrayID].PlayerList.Count > playersTable.Rows.Count)
            {
                // we gained a player
                foreach (var player in _state.Instances[ArrayID].PlayerList)
                {
                    BindingSource Bnd = new BindingSource
                    {
                        DataSource = playersTable
                    };
                    int playerRow = Bnd.Find("Slot #", player.Value.slot);
                    if (playerRow == -1)
                    {
                        DataRow newPlayer = playersTable.NewRow();
                        newPlayer["Slot #"] = player.Value.slot;
                        newPlayer["Name"] = player.Value.name;
                        newPlayer["Address"] = player.Value.address;
                        newPlayer["Ping"] = player.Value.ping;
                        playersTable.Rows.Add(newPlayer);
                        int index = Bnd.Find("Slot #", player.Value.slot); //  find the newly created player
                        //int index = playersTable.Rows.Count;
                        switch (player.Value.team)
                        {
                            case (int)playerlist.Teams.TEAM_GREEN:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Green;
                                break;
                            case (int)playerlist.Teams.TEAM_BLUE:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Blue;
                                break;
                            case (int)playerlist.Teams.TEAM_RED:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Red;
                                break;
                            case (int)playerlist.Teams.TEAM_YELLOW:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Yellow;
                                break;
                            case (int)playerlist.Teams.TEAM_PURPLE:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.Purple;
                                break;
                            case (int)playerlist.Teams.TEAM_SPEC:
                                grid_playerList.Rows[index].Cells[0].Style.BackColor = Color.White;
                                break;
                        }
                    }
                    Bnd.Dispose();
                }
            }
        }
        private void minPing_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_minPing.Checked)
            {
                num_minPing.Enabled = true;
            }
            else
            {
                num_minPing.Enabled = false;
            }
        }

        private void maxPing_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_maxPing.Checked)
            {
                num_maxPing.Enabled = true;
            }
            else
            {
                num_maxPing.Enabled = false;
            }
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox_mapsAvailable.Items.Clear();
            availableMaps.Clear();
            foreach (var map in _state.Instances[ArrayID].availableMaps)
            {
                int gametypeIndex = -1;
                string gametypeString = "";
                foreach (var gametype in _state.autoRes.gameTypes)
                {
                    if (gametype.Value.Name == dropDown_mapSettingsGameType.SelectedItem.ToString())
                    {
                        gametypeIndex = gametype.Value.DatabaseId;
                        gametypeString = gametype.Value.ShortName;
                        break;
                    }
                }
                if (gametypeIndex == -1)
                {
                    throw new Exception("Something went wrong when parsing game types.");
                }
                if (map.Value.GameTypes.Contains(gametypeIndex))
                {
                    availableMaps.Add(availableMaps.Count, new MapList
                    {
                        GameType = gametypeString,
                        MapFile = map.Value.MapFile,
                        MapName = map.Value.MapName,
                        CustomMap = map.Value.CustomMap
                    });
                    listBox_mapsAvailable.Items.Add(map.Value.MapName + " <" + map.Value.MapFile + ">");
                }
            }
            label_numMapsAvailable.Text = listBox_mapsAvailable.Items.Count.ToString();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (list_mapRotation.Items.Count >= 128)
            {
                MessageBox.Show("You can only select 128 maps!", "Error");
                return;
            }
            string gameTypeShortCode = String.Empty;
            foreach (var gametype in _state.autoRes.gameTypes)
            {
                if (dropDown_mapSettingsGameType.SelectedItem.ToString() == gametype.Value.Name)
                {
                    gameTypeShortCode = gametype.Key;
                    break;
                }
            }
            if (gameTypeShortCode == "")
            {
                throw new Exception("We could not find the gametype Shortcode.");
            }
            selectedMaps.Add(new MapList
            {
                CustomMap = availableMaps[listBox_mapsAvailable.SelectedIndex].CustomMap,
                GameType = gameTypeShortCode,
                MapFile = availableMaps[listBox_mapsAvailable.SelectedIndex].MapFile,
                MapName = availableMaps[listBox_mapsAvailable.SelectedIndex].MapName
            });
            list_mapRotation.Items.Add("|" + gameTypeShortCode + "| " + availableMaps[listBox_mapsAvailable.SelectedIndex].MapName + " <" + availableMaps[listBox_mapsAvailable.SelectedIndex].MapFile + ">");
            label_currentMapCount.Text = list_mapRotation.Items.Count.ToString() + " / 128";
            MapListEdited = true;
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            selectedMaps.RemoveAt(list_mapRotation.SelectedIndex);
            list_mapRotation.Items.Remove(list_mapRotation.SelectedItem);
            label_currentMapCount.Text = list_mapRotation.Items.Count.ToString() + " / 128";
            MapListEdited = true;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (process != null)
            {
                if (_state.Instances[ArrayID].Status == InstanceStatus.LOADINGMAP)
                {
                    MessageBox.Show("You cannot score the map while the server is changing maps!", "Error");
                    return;
                }
                else if (_state.Instances[ArrayID].Status == InstanceStatus.OFFLINE)
                {
                    MessageBox.Show("The server is offline! You shouldn't even be here!", "Error");
                    return;
                }
                else if (_state.Instances[ArrayID].Status == InstanceStatus.SCORING)
                {
                    MessageBox.Show("The server is already scoring! Please wait!", "Error");
                    return;
                }
                else if (_state.Instances[ArrayID].Status == InstanceStatus.STARTDELAY || _state.Instances[ArrayID].Status == InstanceStatus.ONLINE)
                {
                    IntPtr h = process.MainWindowHandle;
                    IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                    // open console
                    PostMessage(h, (ushort)WM_KEYDOWN, (ushort)console, 0);
                    PostMessage(h, (ushort)WM_KEYUP, (ushort)console, 0);
                    System.Threading.Thread.Sleep(50);
                    int bytesWritten = 0;
                    byte[] buffer = Encoding.Default.GetBytes("resetgames\0"); // '\0' marks the end of string
                    WriteProcessMemory((int)processHandle, 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                    System.Threading.Thread.Sleep(50);
                    PostMessage(h, (ushort)WM_KEYDOWN, (ushort)VK_ENTER, 0);
                    PostMessage(h, (ushort)WM_KEYUP, (ushort)VK_ENTER, 0);
                    MessageBox.Show("Command sent!", "Success");
                    CloseHandle(processHandle);

                    return;
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            sqlite sqlite = new sqlite();
            int val = (int)num_autoMsgInterval.Value;
            sqlite.ExecuteNonQuery("UPDATE `instances_config` SET `auto_msg_interval` = " + val + " WHERE `profile_id` = " + Profile_ID + ";");
            _state.Instances[ArrayID].AutoMessages.interval = val;
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
            _state.Instances[ArrayID].AutoMessages.messages.Remove(listBox_AutoMessages.SelectedItem.ToString());
            listBox_AutoMessages.Items.Remove(listBox_AutoMessages.SelectedItem);
            var json = JsonConvert.SerializeObject(listBox_AutoMessages.Items);
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
            conn.Open();
            SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
            automessages.Parameters.AddWithValue("@profileid", Profile_ID);
            automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(listBox_AutoMessages.Items));
            automessages.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            _state.Instances[ArrayID].AutoMessages.MsgNumber = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox_AutoMessages.Items.Add(text_newAutoMessage.Text);
            _state.Instances[ArrayID].AutoMessages.messages.Add(text_newAutoMessage.Text);
            text_newAutoMessage.Text = "";
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
            conn.Open();
            SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
            automessages.Parameters.AddWithValue("@profileid", Profile_ID);
            automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(listBox_AutoMessages.Items));
            automessages.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            _state.Instances[ArrayID].AutoMessages.MsgNumber = 0;
            MessageBox.Show("Message added!", "Success!");
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                listBox_AutoMessages.Items.Add(text_newAutoMessage.Text);
                _state.Instances[ArrayID].AutoMessages.messages.Add(text_newAutoMessage.Text);
                text_newAutoMessage.Text = "";
                SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                conn.Open();
                SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
                automessages.Parameters.AddWithValue("@profileid", Profile_ID);
                automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(listBox_AutoMessages.Items));
                automessages.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                MessageBox.Show("Message added!", "Success!");
            }
            else
            {
                return;
            }
        }

        private void MoveMessage(int direction)
        {
            if (listBox_AutoMessages.SelectedItem == null || listBox_AutoMessages.SelectedIndex < 0)
            {
                return; // we haven't seleccted a message so it will return a -1 or NULL value.
            }
            else
            {
                int newIndex = listBox_AutoMessages.SelectedIndex + direction;

                // setup array bounds so we don't fuck up again...
                if (newIndex < 0 || newIndex >= listBox_AutoMessages.Items.Count)
                {
                    return; // Index out of range - nothing to do...
                }
                else
                {
                    // remove item then re-add at the correct position
                    object selected = listBox_AutoMessages.SelectedItem;
                    listBox_AutoMessages.Items.Remove(selected);
                    listBox_AutoMessages.Items.Insert(newIndex, selected);
                    listBox_AutoMessages.SetSelected(newIndex, true);
                }
            }
        }
        private void MoveMapEntry(int direction)
        {
            if (list_mapRotation.SelectedItem == null || list_mapRotation.SelectedIndex < 0)
            {
                return; // we haven't seleccted a message so it will return a -1 or NULL value.
            }
            else
            {
                int newIndex = list_mapRotation.SelectedIndex + direction;

                // setup array bounds so we don't fuck up again...
                if (newIndex < 0 || newIndex >= list_mapRotation.Items.Count)
                {
                    return; // Index out of range - nothing to do...
                }
                else
                {
                    MapListEdited = true;
                    // remove item then re-add at the correct position
                    object selected = list_mapRotation.SelectedItem;
                    MapList mapEntry = selectedMaps[list_mapRotation.SelectedIndex];
                    selectedMaps.RemoveAt(list_mapRotation.SelectedIndex);
                    list_mapRotation.Items.Remove(selected);
                    selectedMaps.Insert(newIndex, mapEntry);
                    list_mapRotation.Items.Insert(newIndex, selected);
                    list_mapRotation.SetSelected(newIndex, true);
                    _state.Instances[ArrayID].previousMapList = new Dictionary<int, MapList>();

                    foreach (var map in _state.Instances[ArrayID].MapList)
                    {
                        _state.Instances[ArrayID].previousMapList.Add(_state.Instances[ArrayID].previousMapList.Count, map.Value);
                    }
                    _state.Instances[ArrayID].MapList = new Dictionary<int, MapList>();
                    foreach (var selectedMapListEntry in selectedMaps)
                    {
                        _state.Instances[ArrayID].MapList.Add(_state.Instances[ArrayID].MapList.Count, selectedMapListEntry);
                    }
                }
            }
        }

        // display ban entry
        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (grid_bannedPlayerList.CurrentCell == null)
            {
                value_bdPlayerName.Text = "";
                value_banReason.Text = "";
                value_banDateAdded.Text = "";
                value_banAdmin.Text = "";
                return;
            }
            if (_state.Instances[ArrayID].BanList.Count == 0)
            {
                value_bdPlayerName.Text = "";
                value_banReason.Text = "";
                value_banDateAdded.Text = "";
                value_banAdmin.Text = "";
                return;
            }
            string playerName = grid_bannedPlayerList.Rows[grid_bannedPlayerList.CurrentCell.RowIndex].Cells[0].Value.ToString();
            string playerIP = grid_bannedPlayerList.Rows[grid_bannedPlayerList.CurrentCell.RowIndex].Cells[1].Value.ToString();
            value_bdPlayerName.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentRow.Index].player;
            value_banReason.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentRow.Index].reason;
            value_banDateAdded.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentRow.Index].addedDate.ToString("MMM dd, yyyy h:mm tt");
            value_banAdmin.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentRow.Index].bannedBy;
        }

        private void dataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int numPlayers = _state.Instances[ArrayID].NumPlayers;
                if (numPlayers == 0)
                {
                    return;
                }
                else
                {
                    var hitTest = grid_playerList.HitTest(e.X, e.Y);
                    if (hitTest.RowIndex == -1)
                    {
                        return;
                    }
                    grid_playerList.Rows[hitTest.RowIndex].Selected = true;
                    contextMenuStrip1.Show(this, new Point(e.X + ((Control)sender).Left + 20, e.Y + ((Control)sender).Top + 20));
                }
            }
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            // abusive perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Abusive");
        }

        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            // Racism perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Racism");
        }

        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            // general hack / cheating perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "General cheating/exploits");
        }

        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            // wall hacking perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Wallhack");
        }

        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            // aimbot perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Aimbot");
        }

        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            // speed hacking perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Speedhacking");
        }

        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            // admin disrespect perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Admin disrespect");
        }

        private void toolStripMenuItem32_Click(object sender, EventArgs e)
        {
            // camping perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Camping");
        }

        private void toolStripMenuItem33_Click(object sender, EventArgs e)
        {
            // team killing perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Team killing");
        }

        private void toolStripMenuItem34_Click(object sender, EventArgs e)
        {
            // breaking server rules perm ban
            int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "-1", "", "Breaking Server Rules");
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // other -- enter custom ban - perm ban
            ToolStripTextBox field = toolStripTextBox1;
            string custom_bantext = field.Text;
            if (e.KeyChar == (char)Keys.Enter)
            {
                int selectedrowindex = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], selectedrowindex, "", "-1", custom_bantext);
                contextMenuStrip1.Close();
            }
            else
            {
                return;
            }
        }

        private void dataGridView2_DoubleClick(object sender, EventArgs e)
        {
            //MessageBox.Show("You've clicked on ID: " + dataGridView2.CurrentCell.RowIndex);
            int numPlayers = _state.Instances[ArrayID].NumPlayers;
            if (numPlayers == 0)
            {
                return;
            }
            PlayerInfo playerInfo = new PlayerInfo(_state, ArrayID, Convert.ToInt32(grid_playerList.SelectedCells[0].Value));
            playerInfo.ShowDialog();
        }

        private void dataGridView4_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Show(Cursor.Position);
            }
            else
            {
                return;
            }
        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_vpn_disallow.Checked)
            {
                value_vpn_abuselevel.Enabled = true;
                _state.Instances[ArrayID].enableVPNCheck = true;
                _state.IPQualityCache[ArrayID].WarnLevel = (int)value_vpn_abuselevel.Value;
            }
            else
            {
                value_vpn_abuselevel.Enabled = false;
                _state.Instances[ArrayID].enableVPNCheck = true;
            }
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `enableVPNCheck` = @vpnCheck WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@vpnCheck", Convert.ToInt32(_state.Instances[ArrayID].enableVPNCheck));
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
            db.Dispose();
        }

        bool specificPlayerTmr_IsRunning;
        Timer specificPlayerTmr = new Timer()
        {
            Enabled = true,
            Interval = 1000
        };


        string specificPlayer;
        private void radioButton11_Click(object sender, EventArgs e)
        {
            rb_chatAll.Checked = false;
            rb_chatGlobal.Checked = false;
            rb_chatRedTeam.Checked = false;
            rb_chatBlueTeam.Checked = false;
            rb_chatPlayerHist.Checked = true;
            cb_chatPlayerSelect.Enabled = true;
            cb_chatPlayerSelect.Items.Clear();
            cb_chatPlayerSelect.Items.Add("Select Player");
            cb_chatPlayerSelect.SelectedIndex = 0;
            specificPlayer = "";

            // populate comboBox9 with current player names
            foreach (var player in _state.Instances[ArrayID].PlayerList)
            {
                cb_chatPlayerSelect.Items.Add(player.Value.name);
            }
            data_chatViewer.DataSource = "";
            if (specificPlayerTmr_IsRunning == true)
            {
                specificPlayerTmr_IsRunning = false;
                specificPlayerTmr.Stop();
            }
        }

        private void radioButton7_Click(object sender, EventArgs e)
        {
            rb_chatAll.Checked = true;
            rb_chatGlobal.Checked = false;
            rb_chatRedTeam.Checked = false;
            rb_chatBlueTeam.Checked = false;
            rb_chatPlayerHist.Checked = false;
            specificPlayer = "";
            if (cb_chatPlayerSelect.Items.Count > 0)
            {
                cb_chatPlayerSelect.Items.Clear();
            }
            cb_chatPlayerSelect.Enabled = false;
            //dataGridView5.DataSource = chatLog;
            data_chatViewer.Sort(data_chatViewer.Columns[0], ListSortDirection.Descending);
            data_chatViewer.Columns["Date & Time"].Width = 100;
            data_chatViewer.Columns["Type"].Width = 40;
            data_chatViewer.Columns["Player Name"].Width = 104;
            data_chatViewer.Columns["Message"].Width = 290;
            if (specificPlayerTmr_IsRunning == true)
            {
                specificPlayerTmr.Stop();
                specificPlayerTmr_IsRunning = false;
            }
        }

        private void radioButton8_Click(object sender, EventArgs e)
        {
            rb_chatAll.Checked = false;
            rb_chatGlobal.Checked = true;
            rb_chatRedTeam.Checked = false;
            rb_chatBlueTeam.Checked = false;
            rb_chatPlayerHist.Checked = false;
            specificPlayer = "";
            if (cb_chatPlayerSelect.Items.Count > 0)
            {
                cb_chatPlayerSelect.Items.Clear();
            }
            cb_chatPlayerSelect.Enabled = false;
            if (specificPlayerTmr_IsRunning == true)
            {
                specificPlayerTmr_IsRunning = false;
                specificPlayerTmr.Stop();
            }
        }

        private void radioButton9_Click(object sender, EventArgs e)
        {
            rb_chatAll.Checked = false;
            rb_chatGlobal.Checked = false;
            rb_chatRedTeam.Checked = true;
            rb_chatBlueTeam.Checked = false;
            rb_chatPlayerHist.Checked = false;
            specificPlayer = "";
            if (cb_chatPlayerSelect.Items.Count > 0)
            {
                cb_chatPlayerSelect.Items.Clear();
            }
            cb_chatPlayerSelect.Enabled = false;
            if (specificPlayerTmr_IsRunning == true)
            {
                specificPlayerTmr_IsRunning = false;
                specificPlayerTmr.Stop();
            }
        }

        private void radioButton10_Click(object sender, EventArgs e)
        {
            rb_chatAll.Checked = false;
            rb_chatGlobal.Checked = false;
            rb_chatRedTeam.Checked = false;
            rb_chatBlueTeam.Checked = true;
            rb_chatPlayerHist.Checked = false;
            specificPlayer = "";
            if (cb_chatPlayerSelect.Items.Count > 0)
            {
                cb_chatPlayerSelect.Items.Clear();
            }
            cb_chatPlayerSelect.Enabled = false;
            if (specificPlayerTmr_IsRunning == true)
            {
                specificPlayerTmr_IsRunning = false;
                specificPlayerTmr.Stop();
            }
        }
        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            specificPlayerTable.Rows.Clear();
            if (cb_chatPlayerSelect.SelectedIndex == 0)
            {
                if (specificPlayerTmr_IsRunning == true)
                {
                    specificPlayerTmr_IsRunning = false;
                    specificPlayerTmr.Stop();
                }
                return;
            }
            //dataGridView5.CurrentCell = dataGridView5[0, 0];
            DataRow[] checkEntries = chatLog.Select("`Player Name` = '" + cb_chatPlayerSelect.SelectedItem + "'");
            specificPlayer = cb_chatPlayerSelect.SelectedItem.ToString();
            foreach (DataRow dataRow in checkEntries)
            {
                specificPlayerTable.ImportRow(dataRow);
            }
            //dataGridView5.DataSource = specificPlayerTable;
            data_chatViewer.Sort(data_chatViewer.Columns[0], ListSortDirection.Descending);
            data_chatViewer.Columns["Date & Time"].Width = 100;
            data_chatViewer.Columns["Type"].Width = 40;
            data_chatViewer.Columns["Player Name"].Width = 104;
            data_chatViewer.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            specificPlayerTmr.Tick += specificPlayerTmr_Tick;
            specificPlayerTmr.Start();
            specificPlayerTmr_IsRunning = true;
        }

        private void specificPlayerTmr_Tick(object sender, EventArgs e)
        {
            if (_state.ChatLogs[ArrayID].Messages.Count > 0)
            {
                DataRow[] checkEntries = chatLog.Select("`Player Name` = '" + specificPlayer + "'");

                foreach (DataRow playerEntry in checkEntries)
                {
                    string dateSent = playerEntry["Date & Time"].ToString();
                    string playerName = playerEntry["Player Name"].ToString();
                    string message = playerEntry["Message"].ToString();

                    DataRow[] checkSpecificPlayerEntries = specificPlayerTable.Select("`Date & Time` = '" + dateSent + "' AND `Player Name` = '" + playerName + "' AND `Message` = '" + message + "'");

                    if (checkSpecificPlayerEntries.Length == 0)
                    {
                        specificPlayerTable.ImportRow(playerEntry);
                    }
                }
            }
            else if (_state.ChatLogs[ArrayID].Messages.Count == 0)
            {
                chatLog.Rows.Clear();
            }
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            // disarm player
            int selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            if (!_state.Instances[ArrayID].DisarmPlayers.Contains(selectedPlayerSlot))
            {
                _state.Instances[ArrayID].DisarmPlayers.Add(selectedPlayerSlot);
                MessageBox.Show("Player has been disarmed!", "Success");
            }
            else
            {
                MessageBox.Show("Player has already been disarmed!", "Error");
            }
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            // rearm player
            int selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            _state.Instances[ArrayID].DisarmPlayers.Remove(selectedPlayerSlot);
            Process process = Process.GetProcessById((int)_state.Instances[ArrayID].PID.GetValueOrDefault());
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

            int buffer = 0;
            byte[] PointerAddr9 = new byte[4];
            var baseAddr = 0x400000;
            var Pointer = baseAddr + 0x005ED600;
            ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
            var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;

            byte[] playerListStartingLocationByteArray = new byte[4];
            int playerListStartingLocationBuffer = 0;
            ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

            int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
            for (int slot = 1; slot < selectedPlayerSlot; slot++)
            {
                playerlistStartingLocation += 0xAF33C;
            }
            byte[] disablePlayerWeapon = BitConverter.GetBytes(1);
            int disablePlayerWeaponWrite = 0;
            WriteProcessMemory((int)processHandle, playerlistStartingLocation + 0xADE08, disablePlayerWeapon, disablePlayerWeapon.Length, ref disablePlayerWeaponWrite);
            CloseHandle(processHandle);
            process.Dispose();

            MessageBox.Show("Player has been rearmed!", "Success");
        }

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            // kick player - abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Abusive", true);
        }

        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            // kick player - Racism
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Racism", true);
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            // kick player - General cheating/exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "General Cheating / Exploits", true);
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            // kick player - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Wall Hacking", true);
        }

        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            // kick player - Aimbot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Aimbot", true);
        }

        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            // kick player - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Speed Hacking", true);
        }

        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            // kick player - admin disrespect
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Admin Disrespect", true);
        }

        private void toolStripMenuItem126_Click(object sender, EventArgs e)
        {
            // kick player - camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Camping", true);
        }

        private void toolStripMenuItem127_Click(object sender, EventArgs e)
        {
            // kick player - team killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Team Killing", true);
        }

        private void toolStripMenuItem128_Click(object sender, EventArgs e)
        {
            // kick player - breaking server rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Breaking Server Rules", true);
        }

        private void toolStripTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // kick player - custom reason
                string customReason = toolStripTextBox2.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", customReason, true);
            }
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem37_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - racism
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Racism");
        }

        private void toolStripMenuItem38_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - general cheating/exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem39_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - wall hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem40_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - aimbot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem41_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - speed hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem42_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - admin disrespect
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem43_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Camping");
        }

        private void toolStripMenuItem44_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - team killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem45_Click(object sender, EventArgs e)
        {
            // temp ban - 1 day - breaking server rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 1 day - custom reason
                string customReason = toolStripTextBox3.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(1).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem46_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - racism
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Racism");
        }

        private void toolStripMenuItem47_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - general cheating/exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem48_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - wall hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem49_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - aimbot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem50_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - speed hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem51_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - admin disrespect
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem52_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Camping");
        }

        private void toolStripMenuItem53_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem54_Click(object sender, EventArgs e)
        {
            // temp ban - 2 days - Breaking server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(2).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 2 days - custom reason
                string customReason = toolStripTextBox4.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem55_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem56_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Racism");
        }

        private void toolStripMenuItem57_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - General Cheating/Exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem58_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem59_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem60_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem61_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Admin Disrepect
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem62_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Camping");
        }

        private void toolStripMenuItem63_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Team killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem64_Click(object sender, EventArgs e)
        {
            // temp ban - 3 days - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 3days - custom reason
                string customReason = toolStripTextBox5.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(3).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem65_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem66_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Racism");
        }

        private void toolStripMenuItem67_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - General Cheating/Exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem68_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Wall hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem69_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem70_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Speed hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem71_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Admin Disrepect
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem72_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Camping");
        }

        private void toolStripMenuItem73_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem74_Click(object sender, EventArgs e)
        {
            // temp ban - 4days - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(4).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 4days - custom reason
                string customReason = toolStripTextBox6.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem75_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem77_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Racism");
        }

        private void toolStripMenuItem78_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - General Cheating/Exploit
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem79_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem80_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem81_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem82_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Admin Disrepecting
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem83_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Camping");
        }

        private void toolStripMenuItem84_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem85_Click(object sender, EventArgs e)
        {
            // temp ban - 5days - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 5days - custom reason
                string customReason = toolStripTextBox7.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(5).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem86_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem87_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Racism");
        }

        private void toolStripMenuItem88_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - General Cheating/Exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem89_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem90_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem91_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem92_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Admin Disrepecting
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem93_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Camping");
        }

        private void toolStripMenuItem94_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem95_Click(object sender, EventArgs e)
        {
            // temp ban - 6days - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 6days - custom reason
                string customReason = toolStripTextBox8.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(6).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem96_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem97_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Racism");
        }

        private void toolStripMenuItem98_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - General Cheating/Exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem99_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem100_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem101_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem102_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Admin Disrepecting
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem103_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Camping");
        }

        private void toolStripMenuItem104_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem105_Click(object sender, EventArgs e)
        {
            // temp ban - 1week - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox9_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 1week - custom reason
                string customReason = toolStripTextBox9.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(7).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem106_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem107_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Racism");
        }

        private void toolStripMenuItem108_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - General Cheating/Exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem109_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem110_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem111_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem112_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Admin Disrepecting
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem113_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Camping");
        }

        private void toolStripMenuItem114_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem115_Click(object sender, EventArgs e)
        {
            // temp ban - 2week - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox10_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 2week - custom reason
                string customReason = toolStripTextBox10.Text;
                int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(14).ToString(), "", customReason);
            }
        }

        private void toolStripMenuItem116_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Abusive
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Abusive");
        }

        private void toolStripMenuItem117_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Racisum
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Racism");
        }

        private void toolStripMenuItem118_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - General Cheating/Exploits
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "General Cheating / Exploits");
        }

        private void toolStripMenuItem119_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Wall Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Wall Hacking");
        }

        private void toolStripMenuItem120_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - AimBot
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Aimbot");
        }

        private void toolStripMenuItem121_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Speed Hacking
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Speed Hacking");
        }

        private void toolStripMenuItem122_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Admin Disrepecting
            BanPlayerFunction cmdPlayer = new BanPlayerFunction();
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Admin Disrespect");
        }

        private void toolStripMenuItem123_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Camping
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Camping");
        }

        private void toolStripMenuItem124_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Team Killing
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Team Killing");
        }

        private void toolStripMenuItem125_Click(object sender, EventArgs e)
        {
            // temp ban - 1month - Breaking Server Rules
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", "Breaking Server Rules");
        }

        private void toolStripTextBox11_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // temp ban - 1month - custom reason
                if (toolStripTextBox11.Text == string.Empty)
                {
                    return;
                }
                else
                {
                    string customReason = toolStripTextBox11.Text;
                    int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                    cmdPlayer.BanPlayer(_state.Instances[ArrayID], slotNum, DateTime.Now.AddDays(30).ToString(), "", customReason);
                }
            }
        }

        // update server setttings button
        private void click_updateServerSettings(object sender, EventArgs e)
        {
            ServerManagerUpdateMemory serverManagerUpdateMemory = new ServerManagerUpdateMemory();

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();

            /*
             * update Auto Balance
             * update Friendly Fire Warning
             * update Show Tracers
             * GAMEPLAY OPTIONS 1
             * Need to find where the AutoBalance field is being held
             * 
             */
            if ((cb_autoBalance.Checked != _state.Instances[ArrayID].AutoBalance) || (cb_ffWarning.Checked != _state.Instances[ArrayID].FriendlyFireWarning) || (cb_Tracers.Checked != _state.Instances[ArrayID].ShowTracers) || (cb_friendFireKills.Checked != _state.Instances[ArrayID].FriendlyFire) || (cb_showFriendTags.Checked != _state.Instances[ArrayID].FriendlyTags) || (cb_TeamClays.Checked != _state.Instances[ArrayID].ShowTeamClays) || (cb_AutoRange.Checked != _state.Instances[ArrayID].AllowAutoRange))
            {
                _state.Instances[ArrayID].AutoBalance = cb_autoBalance.Checked;
                _state.Instances[ArrayID].FriendlyFireWarning = cb_ffWarning.Checked;
                _state.Instances[ArrayID].ShowTracers = cb_Tracers.Checked;
                _state.Instances[ArrayID].FriendlyFire = cb_friendFireKills.Checked;
                _state.Instances[ArrayID].FriendlyTags = cb_showFriendTags.Checked;
                _state.Instances[ArrayID].ShowTeamClays = cb_TeamClays.Checked;
                _state.Instances[ArrayID].AllowAutoRange = cb_AutoRange.Checked;
                serverManagerUpdateMemory.GamePlayOptions(_state, ArrayID);
                SQLiteCommand UpdateGamePlayOptionsOneCmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_balance` = @AutoBalance, `friendly_fire_warning` = @FriendlyFireWarning, `show_tracers` = @ShowTracers, `friendly_fire` = @friendly_fire, `friendly_tags` = @friendlytags, `show_team_clays` = @show_team_clays, `allow_auto_range` = @AllowAutoRange WHERE `profile_id` = @profileid;", db);
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@AutoBalance", Convert.ToInt32(cb_autoBalance.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@FriendlyFireWarning", Convert.ToInt32(cb_ffWarning.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@ShowTracers", Convert.ToInt32(cb_Tracers.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@friendly_fire", Convert.ToInt32(cb_friendFireKills.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@friendlytags", Convert.ToInt32(cb_showFriendTags.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@show_team_clays", Convert.ToInt32(cb_TeamClays.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@AllowAutoRange", Convert.ToInt32(cb_AutoRange.Checked));
                UpdateGamePlayOptionsOneCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                UpdateGamePlayOptionsOneCmd.ExecuteNonQuery();
                UpdateGamePlayOptionsOneCmd.Dispose();
            }

            // update server name
            if (smT_serverName.Text != _state.Instances[ArrayID].ServerName)
            {
                _state.Instances[ArrayID].ServerName = smT_serverName.Text;
                serverManagerUpdateMemory.UpdateServerName(_state, ArrayID);
                SQLiteCommand UpdateServerNameCmd = new SQLiteCommand("UPDATE `instances_config` SET `server_name` = @servername WHERE `profile_id` = @profileid;", db);
                UpdateServerNameCmd.Parameters.AddWithValue("@servername", smT_serverName.Text);
                UpdateServerNameCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                UpdateServerNameCmd.ExecuteNonQuery();
                UpdateServerNameCmd.Dispose();
            }

            // update country code
            if (smCB_country.Text != _state.Instances[ArrayID].CountryCode)
            {
                _state.Instances[ArrayID].CountryCode = smCB_country.Text;
                serverManagerUpdateMemory.UpdateCountryCode(_state, ArrayID);
                SQLiteCommand updateCountryCode = new SQLiteCommand("UPDATE `instances_config` SET `country_code` = @countrycode WHERE `profile_id` = @profileid;", db);
                updateCountryCode.Parameters.AddWithValue("@countrycode", smCB_country.Text);
                updateCountryCode.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateCountryCode.ExecuteNonQuery();
                updateCountryCode.Dispose();
            }

            /*
             * update server password
             * Need to find where the password field is being held
             */
            if (smT_serverPassword.Text != _state.Instances[ArrayID].Password)
            {
                int oldPw = _state.Instances[ArrayID].Password.Length;
                _state.Instances[ArrayID].Password = smT_serverPassword.Text;
                serverManagerUpdateMemory.UpdateServerPassword(_state, ArrayID, oldPw);
                SQLiteCommand updateServerPasswordCmd = new SQLiteCommand("UPDATE `instances_config` SET `server_password` = @serverpassword WHERE `profile_id` = @profileid;", db);
                updateServerPasswordCmd.Parameters.AddWithValue("@serverpassword", smT_serverPassword.Text);
                updateServerPasswordCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateServerPasswordCmd.ExecuteNonQuery();
                updateServerPasswordCmd.Dispose();
            }

            /*
             * update Session Type
             * Need to find where the SessionType field is being held
             */
            if (smCB_sessionType.SelectedIndex != _state.Instances[ArrayID].SessionType)
            {
                _state.Instances[ArrayID].SessionType = smCB_sessionType.SelectedIndex;
                serverManagerUpdateMemory.UpdateSessionType(_state, ArrayID);
                SQLiteCommand updateSessionTypeCmd = new SQLiteCommand("UPDATE `instances_config` SET `session_type` = @sessiontype WHERE `profile_id` = @profileid;", db);
                updateSessionTypeCmd.Parameters.AddWithValue("@sessiontype", smCB_sessionType.SelectedIndex);
                updateSessionTypeCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateSessionTypeCmd.ExecuteNonQuery();
                updateSessionTypeCmd.Dispose();
            }

            /*
             * update Max Slots
             * Need to find where the MaxSlots field is being held
             */
            if (Convert.ToInt32(smNum_maxSlots.Value) != _state.Instances[ArrayID].MaxSlots)
            {
                _state.Instances[ArrayID].MaxSlots = Convert.ToInt32(smNum_maxSlots.Value);
                serverManagerUpdateMemory.UpdateMaxSlots(_state, ArrayID);
                SQLiteCommand updateMaxSlotsCmd = new SQLiteCommand("UPDATE `instances_config` SET `max_slots` = @maxslots WHERE `profile_id` = @profileid;", db);
                updateMaxSlotsCmd.Parameters.AddWithValue("@maxslots", Convert.ToInt32(smNum_maxSlots.Value));
                updateMaxSlotsCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateMaxSlotsCmd.ExecuteNonQuery();
                updateMaxSlotsCmd.Dispose();
            }

            /*
             * update Time Limit
             * Need to find where the TimeLimit field is being held
             * 
             */
            if (cb_timeLimit.SelectedIndex != _state.Instances[ArrayID].TimeLimit)
            {
                _state.Instances[ArrayID].TimeLimit = cb_timeLimit.SelectedIndex;
                serverManagerUpdateMemory.UpdateTimeLimit(_state, ArrayID);
                SQLiteCommand updateTimeLimitCmd = new SQLiteCommand("UPDATE `instances_config` SET `time_limit` = @timelimit WHERE `profile_id` = @profileid;", db);
                updateTimeLimitCmd.Parameters.AddWithValue("@timelimit", cb_timeLimit.SelectedIndex);
                updateTimeLimitCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateTimeLimitCmd.ExecuteNonQuery();
                updateTimeLimitCmd.Dispose();
            }

            /*
             * update Start Delay
             * Need to find where the StartDelay field is being held
             * 
             */
            if (cb_startDelay.SelectedIndex != _state.Instances[ArrayID].StartDelay)
            {
                _state.Instances[ArrayID].StartDelay = cb_startDelay.SelectedIndex;
                serverManagerUpdateMemory.UpdateStartDelay(_state, ArrayID);
                SQLiteCommand updateStartDelayCmd = new SQLiteCommand("UPDATE `instances_config` SET `start_delay` = @startdelay WHERE `profile_id` = @profileid;", db);
                updateStartDelayCmd.Parameters.AddWithValue("@startdelay", cb_startDelay.SelectedIndex);
                updateStartDelayCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateStartDelayCmd.ExecuteNonQuery();
                updateStartDelayCmd.Dispose();
            }

            /*
             * update Loop Maps
             * Need to find where the Loop Maps field is being held
             * 
             */
            if (cb_replayMaps.SelectedIndex != _state.Instances[ArrayID].LoopMaps)
            {
                _state.Instances[ArrayID].LoopMaps = cb_replayMaps.SelectedIndex;
                serverManagerUpdateMemory.UpdateLoopMaps(_state, ArrayID);
                SQLiteCommand updateLoopMapsCmd = new SQLiteCommand("UPDATE `instances_config` SET `loop_maps` = @loopmaps WHERE `profile_id` = @profileid;", db);
                updateLoopMapsCmd.Parameters.AddWithValue("@loopmaps", cb_replayMaps.SelectedIndex);
                updateLoopMapsCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateLoopMapsCmd.ExecuteNonQuery();
                updateLoopMapsCmd.Dispose();
            }

            /*
             * update Respawn Time
             * Need to find where the Respawn Time field is being held
             * 
             */
            if (cb_respawnTime.SelectedIndex != _state.Instances[ArrayID].RespawnTime)
            {
                _state.Instances[ArrayID].RespawnTime = cb_respawnTime.SelectedIndex;
                serverManagerUpdateMemory.UpdateRespawnTime(_state, ArrayID);
                SQLiteCommand updateRespawnTimeCmd = new SQLiteCommand("UPDATE `instances_config` SET `respawn_time` = @respawntime WHERE `profile_id` = @profileid;", db);
                updateRespawnTimeCmd.Parameters.AddWithValue("@respawntime", cb_respawnTime.SelectedIndex);
                updateRespawnTimeCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateRespawnTimeCmd.ExecuteNonQuery();
                updateRespawnTimeCmd.Dispose();
            }

            /*
             * update Require Nova Login
             * Need to find where the RequireNovaLogin field is being held
             * 
             */
            if (cb_requireNova.Checked != _state.Instances[ArrayID].RequireNovaLogin)
            {
                _state.Instances[ArrayID].RequireNovaLogin = cb_requireNova.Checked;
                serverManagerUpdateMemory.UpdateRequireNovaLogin(_state, ArrayID);
                SQLiteCommand updateRequireNovaLoginCmd = new SQLiteCommand("UPDATE `instances_config` SET `require_novalogic` = @requirenovalogic WHERE `profile_id` = @profileid;", db);
                updateRequireNovaLoginCmd.Parameters.AddWithValue("@requirenovalogic", Convert.ToInt32(cb_requireNova.Checked));
                updateRequireNovaLoginCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateRequireNovaLoginCmd.ExecuteNonQuery();
                updateRequireNovaLoginCmd.Dispose();

            }

            /*
             * update Allow Custom Skins
             * Need to find where the AllowCustomSkins field is being held
             * 
             */
            if (cb_customSkin.Checked != _state.Instances[ArrayID].AllowCustomSkins)
            {
                _state.Instances[ArrayID].AllowCustomSkins = cb_customSkin.Checked;
                serverManagerUpdateMemory.UpdateAllowCustomSkins(_state, ArrayID);
                SQLiteCommand updateCustomSkinsCmd = new SQLiteCommand("UPDATE `instances_config` SET `allow_custom_skins` = @customskins WHERE `profile_id` = @profileid;", db);
                updateCustomSkinsCmd.Parameters.AddWithValue("@customskins", Convert.ToInt32(cb_customSkin.Checked));
                updateCustomSkinsCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateCustomSkinsCmd.ExecuteNonQuery();
                updateCustomSkinsCmd.Dispose();
            }

            /*
             * update MOTD
             * Need to find where the MOTD field is being held
             * 
             */
            if (richTextBox1.Text != _state.Instances[ArrayID].MOTD)
            {
                _state.Instances[ArrayID].MOTD = richTextBox1.Text;
                serverManagerUpdateMemory.UpdateMOTD(_state, ArrayID);
                SQLiteCommand updateMOTDCmd = new SQLiteCommand("UPDATE `instances_config` SET `motd` = @motd WHERE `profile_id` = @profileid;", db);
                updateMOTDCmd.Parameters.AddWithValue("@motd", richTextBox1.Text);
                updateMOTDCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateMOTDCmd.ExecuteNonQuery();
                updateMOTDCmd.Dispose();
            }

            /*
             * update MinPing /w MinPingValue
             * Need to find where the MinPing and MinPingValue field are being held
             * 
             */
            if (cb_minPing.Checked != _state.Instances[ArrayID].MinPing || Convert.ToInt32(num_minPing.Text) != _state.Instances[ArrayID].MinPingValue)
            {
                _state.Instances[ArrayID].MinPing = cb_minPing.Checked;
                if (cb_minPing.Checked == true)
                {
                    _state.Instances[ArrayID].MinPingValue = Convert.ToInt32(num_minPing.Text);
                }
                else
                {
                    _state.Instances[ArrayID].MinPingValue = 0;
                }
                serverManagerUpdateMemory.UpdateMinPing(_state, ArrayID);
                serverManagerUpdateMemory.UpdateMinPingValue(_state, ArrayID);
                SQLiteCommand updateMinPingCmd = new SQLiteCommand("UPDATE `instances_config` SET `enable_min_ping` = @enable_min_ping, `min_ping` = @minpingvalue  WHERE `profile_id` = @profileid;", db);
                updateMinPingCmd.Parameters.AddWithValue("@enable_min_ping", Convert.ToInt32(cb_minPing.Checked));
                if (cb_minPing.Checked == true)
                {
                    updateMinPingCmd.Parameters.AddWithValue("@minpingvalue", Convert.ToInt32(num_minPing.Text));
                }
                else
                {
                    updateMinPingCmd.Parameters.AddWithValue("@minpingvalue", 0);
                }
                updateMinPingCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateMinPingCmd.ExecuteNonQuery();
                updateMinPingCmd.Dispose();
            }

            /*
             * update MaxPing /w MaxPingValue
             * Need to find where the MaxPing and MaxPingValue field is being held
             * 
             */
            if (cb_maxPing.Checked != _state.Instances[ArrayID].MaxPing || Convert.ToInt32(num_maxPing.Text) != _state.Instances[ArrayID].MaxPingValue)
            {
                _state.Instances[ArrayID].MaxPing = cb_maxPing.Checked;
                if (cb_maxPing.Checked == true)
                {
                    _state.Instances[ArrayID].MaxPingValue = Convert.ToInt32(num_maxPing.Text);
                }
                else
                {
                    _state.Instances[ArrayID].MinPingValue = 0;
                }
                serverManagerUpdateMemory.UpdateMaxPing(_state, ArrayID);
                serverManagerUpdateMemory.UpdateMaxPingValue(_state, ArrayID);
                SQLiteCommand updateMaxPingCmd = new SQLiteCommand("UPDATE `instances_config` SET `enable_min_ping` = @enable_max_ping, `min_ping` = @maxpingvalue  WHERE `profile_id` = @profileid;", db);
                updateMaxPingCmd.Parameters.AddWithValue("@enable_max_ping", Convert.ToInt32(cb_maxPing.Checked));
                if (cb_minPing.Checked == true)
                {
                    updateMaxPingCmd.Parameters.AddWithValue("@maxpingvalue", Convert.ToInt32(num_maxPing.Text));
                }
                else
                {
                    updateMaxPingCmd.Parameters.AddWithValue("@maxpingvalue", 0);
                }
                updateMaxPingCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateMaxPingCmd.ExecuteNonQuery();
                updateMaxPingCmd.Dispose();
            }

            /*
             * update One Shot Kills
             * Need to find where the OneShotKills field is being held
             * 
             */
            if (cb_oneShotKills.Checked != _state.Instances[ArrayID].OneShotKills)
            {
                _state.Instances[ArrayID].OneShotKills = cb_oneShotKills.Checked;
                serverManagerUpdateMemory.UpdateOneShotKills(_state, ArrayID);
                SQLiteCommand updateOneShotCmd = new SQLiteCommand("UPDATE `instances_config` SET `oneshotkills` = @oneshotkills WHERE `profile_id` = @profileid;", db);
                updateOneShotCmd.Parameters.AddWithValue("@oneshotkills", Convert.ToInt32(cb_oneShotKills.Checked));
                updateOneShotCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateOneShotCmd.ExecuteNonQuery();
                updateOneShotCmd.Dispose();
            }

            /*
             * update Fat Bullets
             * Need to find where the FatBullets field is being held
             * 
             */
            if (cb_fatBullets.Checked != _state.Instances[ArrayID].FatBullets)
            {
                _state.Instances[ArrayID].FatBullets = cb_fatBullets.Checked;
                serverManagerUpdateMemory.UpdateFatBullets(_state, ArrayID);
                SQLiteCommand updateFatBulletsCmd = new SQLiteCommand("UPDATE `instances_config` SET `fatbullets` = @fatbullets WHERE `profile_id` = @profileid;", db);
                updateFatBulletsCmd.Parameters.AddWithValue("@fatbullets", Convert.ToInt32(cb_fatBullets.Checked));
                updateFatBulletsCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateFatBulletsCmd.ExecuteNonQuery();
                updateFatBulletsCmd.Dispose();
            }

            /*
             * update Destroy Buildings
             * Need to find where the DestroyBuildings field is being held
             * 
             */
            if (cb_destroyBuildings.Checked != _state.Instances[ArrayID].DestroyBuildings)
            {
                _state.Instances[ArrayID].DestroyBuildings = cb_destroyBuildings.Checked;
                serverManagerUpdateMemory.UpdateDestroyBuildings(_state, ArrayID);
                SQLiteCommand updateDestroyBuildingsCmd = new SQLiteCommand("UPDATE `instances_config` SET `destroybuildings` = @destroybuildings WHERE `profile_id` = @profileid;", db);
                updateDestroyBuildingsCmd.Parameters.AddWithValue("@destroybuildings", Convert.ToInt32(cb_destroyBuildings.Checked));
                updateDestroyBuildingsCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateDestroyBuildingsCmd.ExecuteNonQuery();
                updateDestroyBuildingsCmd.Dispose();
            }

            /*
             * update Blue Password
             * Need to find where the BluePassword field is being held
             * 
             */
            if (text_bluePass.Text != _state.Instances[ArrayID].BluePassword)
            {
                int oldPw = _state.Instances[ArrayID].BluePassword.Length;
                _state.Instances[ArrayID].BluePassword = text_bluePass.Text;
                serverManagerUpdateMemory.UpdateBluePassword(_state, ArrayID, oldPw);
                SQLiteCommand updateBluePasswordCmd = new SQLiteCommand("UPDATE `instances_config` SET `blue_team_password` = @BluePassword WHERE `profile_id` = @profileid;", db);
                updateBluePasswordCmd.Parameters.AddWithValue("@BluePassword", text_bluePass.Text);
                updateBluePasswordCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateBluePasswordCmd.ExecuteNonQuery();
                updateBluePasswordCmd.Dispose();
                MessageBox.Show("Blue Team Password will be applied on the next map!");
            }

            /*
             * update Red Password
             * Need to find where the RedPassword field is being held
             * 
             */
            if (text_redPass.Text != _state.Instances[ArrayID].RedPassword)
            {
                int oldPw = _state.Instances[ArrayID].RedPassword.Length;
                _state.Instances[ArrayID].RedPassword = text_redPass.Text;
                serverManagerUpdateMemory.UpdateRedPassword(_state, ArrayID, oldPw);
                SQLiteCommand updateRedPasswordCmd = new SQLiteCommand("UPDATE `instances_config` SET `red_team_password` = @RedPassword WHERE `profile_id` = @profileid;", db);
                updateRedPasswordCmd.Parameters.AddWithValue("@RedPassword", text_redPass.Text);
                updateRedPasswordCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateRedPasswordCmd.ExecuteNonQuery();
                updateRedPasswordCmd.Dispose();
                MessageBox.Show("Red Team Password will be applied on the next map!");
            }

            if (endOfMapTimer_TrackBar.Value != _state.Instances[ArrayID].ScoreBoardDelay)
            {
                _state.Instances[ArrayID].ScoreBoardDelay = endOfMapTimer_TrackBar.Value;
                //serverManagerUpdateMemory.UpdateScoreBoardDelay(_state, ArrayID);
            }

            if ((Convert.ToInt32(numericUpDown5.Value) != _state.Instances[ArrayID].FlagReturnTime) || (Convert.ToInt32(num_pspTimer.Value) != _state.Instances[ArrayID].PSPTakeOverTime))
            {
                _state.Instances[ArrayID].FlagReturnTime = Convert.ToInt32(numericUpDown5.Value);
                _state.Instances[ArrayID].PSPTakeOverTime = Convert.ToInt32(num_pspTimer.Value);
                serverManagerUpdateMemory.UpdateFlagReturnTime(_state, ArrayID);
                serverManagerUpdateMemory.UpdatePSPTakeOverTime(_state, ArrayID);
                SQLiteCommand updateFlagReturnTimeCmd = new SQLiteCommand("UPDATE `instances_config` SET `flagreturntime` = @flagreturntime WHERE `profile_id` = @profileid;", db);
                updateFlagReturnTimeCmd.Parameters.AddWithValue("@flagreturntime", Convert.ToInt32(numericUpDown5.Value));
                updateFlagReturnTimeCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateFlagReturnTimeCmd.ExecuteNonQuery();
                updateFlagReturnTimeCmd.Dispose();
                SQLiteCommand updatePSPTakeOverTimeCmd = new SQLiteCommand("UPDATE `instances_config` SET `psptakeover` = @psptakeover WHERE `profile_id` = @profileid;", db);
                updatePSPTakeOverTimeCmd.Parameters.AddWithValue("@psptakeover", Convert.ToInt32(num_pspTimer.Value));
                updatePSPTakeOverTimeCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updatePSPTakeOverTimeCmd.ExecuteNonQuery();
                updatePSPTakeOverTimeCmd.Dispose();
            }

            if (Convert.ToInt32(num_MaxTeamLives.Value) != _state.Instances[ArrayID].MaxTeamLives)
            {
                _state.Instances[ArrayID].MaxTeamLives = Convert.ToInt32(num_MaxTeamLives.Value);
                serverManagerUpdateMemory.UpdateMaxTeamLives(_state, ArrayID);
                SQLiteCommand updateMaxTeamLivesCmd = new SQLiteCommand("UPDATE `instances_config` SET `max_team_lives` = @MaxTeamLives WHERE `profile_id` = @profileid;", db);
                updateMaxTeamLivesCmd.Parameters.AddWithValue("@MaxTeamLives", Convert.ToInt32(num_MaxTeamLives.Value));
                updateMaxTeamLivesCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateMaxTeamLivesCmd.ExecuteNonQuery();
                updateMaxTeamLivesCmd.Dispose();
            }

            /*
             * update Friendly Fire Kills
             * Need to find where the FriendlyFireKills field is being held
             * Kicks a player if their FF is above this point.
             * 
             */
            if (Convert.ToInt32(num_maxFriendKills.Value) != _state.Instances[ArrayID].FriendlyFireKills)
            {
                _state.Instances[ArrayID].FriendlyFireKills = Convert.ToInt32(num_maxFriendKills.Value);
                serverManagerUpdateMemory.UpdateFriendlyFireKills(_state, ArrayID);
                SQLiteCommand updateFriendlyFireKillsCmd = new SQLiteCommand("UPDATE `instances_config` SET `friendly_fire_kills` = @FriendlyFireKills WHERE `profile_id` = @profileid;", db);
                updateFriendlyFireKillsCmd.Parameters.AddWithValue("@FriendlyFireKills", Convert.ToInt32(num_maxFriendKills.Value));
                updateFriendlyFireKillsCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateFriendlyFireKillsCmd.ExecuteNonQuery();
                updateFriendlyFireKillsCmd.Dispose();
            }

            // Flag Ball Score
            if (num_scoreFB.Value != _state.Instances[ArrayID].FBScore)
            {
                _state.Instances[ArrayID].FBScore = Convert.ToInt32(num_scoreFB.Value);
                SQLiteCommand updateFBScoreCmd = new SQLiteCommand("UPDATE `instances_config` SET `fbscore` = @fbscore WHERE `profile_id` = @profileid;", db);
                updateFBScoreCmd.Parameters.AddWithValue("@fbscore", _state.Instances[ArrayID].FBScore);
                updateFBScoreCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateFBScoreCmd.ExecuteNonQuery();
                updateFBScoreCmd.Dispose();
            }
            if (num_scoreKOTH.Value != _state.Instances[ArrayID].KOTHScore)
            {
                _state.Instances[ArrayID].KOTHScore = Convert.ToInt32(num_scoreKOTH.Value);
                SQLiteCommand updateKOTHScoreCmd = new SQLiteCommand("UPDATE `instances_config` SET `kothscore` = @kothscore WHERE `profile_id` = @profileid;", db);
                updateKOTHScoreCmd.Parameters.AddWithValue("@kothscore", _state.Instances[ArrayID].FBScore);
                updateKOTHScoreCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateKOTHScoreCmd.ExecuteNonQuery();
                updateKOTHScoreCmd.Dispose();
            }
            if (num_scoreDM.Value != _state.Instances[ArrayID].GameScore)
            {
                _state.Instances[ArrayID].GameScore = Convert.ToInt32(num_scoreDM.Value);
                SQLiteCommand updateGameScoreCmd = new SQLiteCommand("UPDATE `instances_config` SET `game_score` = @gamescore WHERE `profile_id` = @profileid;", db);
                updateGameScoreCmd.Parameters.AddWithValue("@gamescore", _state.Instances[ArrayID].GameScore);
                updateGameScoreCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateGameScoreCmd.ExecuteNonQuery();
                updateGameScoreCmd.Dispose();
            }

            /*
             * update Role Restrictions
             * Need to find where the RoleRestrictions field is being held
             * 
             */
            /*foreach (int index in checkedListBox1.CheckedIndices)
            {
                if ()
                {
                    _state.Instances[ArrayID].RoleRestrictions = checkedListBox1.CheckedItems;
                    serverManagerUpdateMemory.UpdateRoleRestrictions(_state, ArrayID);
                }
            }*/

            WeaponsClass weaponsClass = new WeaponsClass
            {
                WPN_COLT45 = cbl_weaponSelection.GetItemChecked(0),
                WPN_M9BERETTA = cbl_weaponSelection.GetItemChecked(1),
                WPN_REMMINGTONSG = cbl_weaponSelection.GetItemChecked(2),
                WPN_CAR15 = cbl_weaponSelection.GetItemChecked(3),
                WPN_CAR15_203 = cbl_weaponSelection.GetItemChecked(4),
                WPN_M16_BURST = cbl_weaponSelection.GetItemChecked(5),
                WPN_M16_BURST_203 = cbl_weaponSelection.GetItemChecked(6),
                WPN_M21 = cbl_weaponSelection.GetItemChecked(7),
                WPN_M24 = cbl_weaponSelection.GetItemChecked(8),
                WPN_MCRT_300_TACTICAL = cbl_weaponSelection.GetItemChecked(9),
                WPN_BARRETT = cbl_weaponSelection.GetItemChecked(10),
                WPN_SAW = cbl_weaponSelection.GetItemChecked(11),
                WPN_M60 = cbl_weaponSelection.GetItemChecked(12),
                WPN_M240 = cbl_weaponSelection.GetItemChecked(13),
                WPN_MP5 = cbl_weaponSelection.GetItemChecked(14),
                WPN_G3 = cbl_weaponSelection.GetItemChecked(15),
                WPN_G36 = cbl_weaponSelection.GetItemChecked(16),
                WPN_PSG1 = cbl_weaponSelection.GetItemChecked(17),
                WPN_XM84_STUN = cbl_weaponSelection.GetItemChecked(18),
                WPN_M67_FRAG = cbl_weaponSelection.GetItemChecked(19),
                WPN_AN_M8_SMOKE = cbl_weaponSelection.GetItemChecked(20),
                WPN_SATCHEL_CHARGE = cbl_weaponSelection.GetItemChecked(21),
                WPN_CLAYMORE = cbl_weaponSelection.GetItemChecked(22),
                WPN_AT4 = cbl_weaponSelection.GetItemChecked(23)
            };
            _state.Instances[ArrayID].WeaponRestrictions = weaponsClass;
            serverManagerUpdateMemory.UpdateWeaponRestrictions(_state, ArrayID);

            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `weaponrestrictions` = @weaponrestrictions WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.Parameters.AddWithValue("@weaponrestrictions", JsonConvert.SerializeObject(weaponsClass));
            cmd.ExecuteNonQuery();

            if (endOfMapTimer_TrackBar.Value != _state.Instances[ArrayID].ScoreBoardDelay)
            {
                _state.Instances[ArrayID].ScoreBoardDelay = endOfMapTimer_TrackBar.Value;
                SQLiteCommand updateGameScoreCmd = new SQLiteCommand("UPDATE `instances_config` SET `scoreboard_override` = @scoreboard_override WHERE `profile_id` = @profileid;", db);
                updateGameScoreCmd.Parameters.AddWithValue("@scoreboard_override", _state.Instances[ArrayID].ScoreBoardDelay);
                updateGameScoreCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                updateGameScoreCmd.ExecuteNonQuery();
                updateGameScoreCmd.Dispose();
            }




            cmd.Dispose();
            db.Close();
            MessageBox.Show("Settings updated and saved!", "Success");
        }

        private void toolStripMenuItem76_Click(object sender, EventArgs e)
        {
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            DialogResult killPlayer = MessageBox.Show("Are you sure you want to kill: " + _state.Instances[ArrayID].PlayerList[slotNum].name + " ?", "Important!", MessageBoxButtons.YesNo);
            if (killPlayer == DialogResult.Yes)
            {
                Process process = Process.GetProcessById((int)_state.Instances[ArrayID].PID.GetValueOrDefault());
                IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                int buffer = 0;
                byte[] PointerAddr9 = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                // read the playerlist memory address from the game...
                ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                byte[] playerListStartingLocationByteArray = new byte[4];
                int playerListStartingLocationBuffer = 0;
                ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                for (int i = 1; i < slotNum; i++)
                {
                    playerlistStartingLocation += 0xAF33C;
                }
                byte[] playerObjectLocationBytes = new byte[4];
                int playerObjectLocationRead = 0;
                ReadProcessMemory((int)processHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                byte[] setPlayerHealth = BitConverter.GetBytes(0);
                int setPlayerHealthWrite = 0;

                WriteProcessMemory((int)processHandle, playerObjectLocation + 0x138, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                WriteProcessMemory((int)processHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                process.Dispose();
                CloseHandle(processHandle);


                MessageBox.Show("Command sent.\nPlayer has been killed.", "Success");
            }
            else
            {
                return;
            }
        }

        private void toolStripMenuItem130_Click(object sender, EventArgs e)
        {
            // activate God Mode
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            // check if user is already in GodMode...
            if (!_state.Instances[ArrayID].GodModeList.Contains(slotNum))
            {
                _state.Instances[ArrayID].GodModeList.Add(slotNum);
                MessageBox.Show("God mode has been activated successfully!", "Success");
            }
            else
            {
                MessageBox.Show("You cannot activate GodMode on this user!\nThe user is already in GodMode!", "Error Activating GodMode");
            }
        }

        private void toolStripMenuItem131_Click(object sender, EventArgs e)
        {
            // deactivate God Mode
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);

            if (_state.Instances[ArrayID].GodModeList.Contains(slotNum))
            {
                _state.Instances[ArrayID].GodModeList.Remove(slotNum);
                Process process = Process.GetProcessById((int)_state.Instances[ArrayID].PID.GetValueOrDefault());
                IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                int buffer = 0;
                byte[] PointerAddr9 = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                // read the playerlist memory address from the game...
                ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                byte[] playerListStartingLocationByteArray = new byte[4];
                int playerListStartingLocationBuffer = 0;
                ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                for (int i = 1; i < slotNum; i++)
                {
                    playerlistStartingLocation += 0xAF33C;
                }
                byte[] playerObjectLocationBytes = new byte[4];
                int playerObjectLocationRead = 0;
                ReadProcessMemory((int)processHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                byte[] setPlayerHealth = BitConverter.GetBytes(100); //set god mode health
                int setPlayerHealthWrite = 0;

                byte[] setDamageBy = BitConverter.GetBytes(0);
                int setDamageByWrite = 0;

                WriteProcessMemory((int)processHandle, playerObjectLocation + 0x138, setDamageBy, setDamageBy.Length, ref setDamageByWrite);
                WriteProcessMemory((int)processHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                process.Dispose();
                CloseHandle(processHandle);
                MessageBox.Show("God mode has been deactivated successfully.", "Success");
            }
            else
            {
                MessageBox.Show("You cannot deactivate GodMode on this user!\nThe user is not in GodMode!", "Error Deactivating GodMode");
            }
        }

        private void toolStripMenuItem132_Click(object sender, EventArgs e)
        {
            int slotNum = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            // pull player info
            playerlist.Teams currentTeam = (playerlist.Teams)_state.Instances[ArrayID].PlayerList[slotNum].team;
            playerlist.Teams switchTeam = (playerlist.Teams)playerlist.Teams.TEAM_SPEC;

            if (currentTeam == playerlist.Teams.TEAM_BLUE)
            {
                switchTeam = playerlist.Teams.TEAM_RED;
            }
            else if (currentTeam == playerlist.Teams.TEAM_RED)
            {
                switchTeam = playerlist.Teams.TEAM_BLUE;
            }

            _state.Instances[ArrayID].ChangeTeamList.Add(new ChangeTeamClass
            {
                slotNum = slotNum,
                Team = (int)switchTeam
            });
            MessageBox.Show("The selected Player will be switched at the end of the current map.", "Change Team");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string description = textBox14.Text;
            bool ipaddressAttempt = IPAddress.TryParse(textBox15.Text, out IPAddress ipaddress);
            if (ipaddressAttempt == false)
            {
                MessageBox.Show("Invalid IP Address Provided!", "Error");
                return;
            }
            int index = _state.Instances[ArrayID].VPNWhiteList.Count;
            _state.Instances[ArrayID].VPNWhiteList.Add(index, new VPNWhiteListClass
            {
                Description = description,
                IPAddress = ipaddress.ToString()
            });
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `vpnwhitelist` (`profile_id`, `description`, `address`) VALUES (@profileid, @description, @PublicIP);", db);
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@PublicIP", ipaddress.ToString());
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
            textBox14.Text = string.Empty;
            textBox15.Text = string.Empty;
            dataGridView4.DataSource = GenerateVPNTable();
            MessageBox.Show("IP Address Added Successfully!", "Success");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            textBox14.Text = string.Empty;
            textBox15.Text = string.Empty;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            _state.IPQualityCache[ArrayID].WarnLevel = (int)value_vpn_abuselevel.Value;
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `warnlevel` = @warnlevel WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@warnlevel", _state.IPQualityCache[ArrayID].WarnLevel);
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            int index = dataGridView4.SelectedCells[0].RowIndex;
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `vpnwhitelist` WHERE `profile_id` = @profileid AND `description` = @description AND `address` = @PublicIP;", db);
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.Parameters.AddWithValue("@description", _state.Instances[ArrayID].VPNWhiteList[index].Description);
            cmd.Parameters.AddWithValue("@PublicIP", _state.Instances[ArrayID].VPNWhiteList[index].IPAddress.ToString());
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
            _state.Instances[ArrayID].VPNWhiteList.Remove(index);
            dataGridView4.DataSource = GenerateVPNTable();
            MessageBox.Show("IP Address Removed Successfully.", "Success");
        }

        private void Server_Manager_Load(object sender, EventArgs e)
        {
            selectedMaps = new List<MapList>();
            availableMaps = new Dictionary<int, MapList>();

            // setup player functions
            cmdPlayer = new BanPlayerFunction();



            /*
             * Begin playlist
             * 0x02137A54
             * 15 bytes
             */
            playersTable.Columns.Add("Slot #".ToString());
            playersTable.Columns.Add("Name".ToString());
            playersTable.Columns.Add("Address".ToString());
            playersTable.Columns.Add("Ping".ToString());
            grid_playerList.DataSource = playersTable;
            /*dataGridView2.Columns["Slot #"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView2.Columns["Slot #"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.NotSet;*/
            grid_playerList.Columns["Slot #"].Width = 45;
            grid_playerList.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_playerList.Columns["Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            /*dataGridView2.Columns["Address"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns["Address"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;*/
            grid_playerList.Columns["Address"].Width = 90;
            /*dataGridView2.Columns["Ping"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns["Ping"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;*/
            grid_playerList.Columns["Ping"].Width = 32;
            grid_playerList.Sort(grid_playerList.Columns["Slot #"], ListSortDirection.Ascending);

            bannedPlayersTable = new DataTable();
            bannedPlayersTable.Columns.Add("Name".ToString());
            bannedPlayersTable.Columns.Add("IP Address".ToString());
            bannedPlayersTable.Columns.Add("Time Remaining".ToString());


            foreach (var item in _state.Instances[ArrayID].BanList)
            {
                DataRow bannedRow = bannedPlayersTable.NewRow();
                bannedRow["Name"] = item.player;
                bannedRow["IP Address"] = item.ipaddress;
                if (item.expires == "-1")
                {
                    bannedRow["Time Remaining"] = "Permanent";
                }
                else
                {
                    bannedRow["Time Remaining"] = DateTime.Parse(item.expires);
                }
                bannedPlayersTable.Rows.Add(bannedRow);
            }


            // bannedTable List setup
            grid_bannedPlayerList.DataSource = bannedPlayersTable;
            grid_bannedPlayerList.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_bannedPlayerList.Columns["Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid_bannedPlayerList.Columns["IP Address"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_bannedPlayerList.Columns["IP Address"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid_bannedPlayerList.Columns["Time Remaining"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_bannedPlayerList.Columns["Time Remaining"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // setup VPN White List Table
            dataGridView4.DataSource = GenerateVPNTable();


            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
            conn.Open();
            SQLiteCommand country_cmd = new SQLiteCommand("SELECT iso FROM `country`;", conn);
            SQLiteDataReader country_read = country_cmd.ExecuteReader();
            while (country_read.Read())
            {
                smCB_country.Items.Add(country_read.GetString(country_read.GetOrdinal("iso")));
            }
            country_read.Close();
            conn.Close();
            conn.Dispose();
            smCB_sessionType.Items.Add("NovaWorld");
            smCB_sessionType.Items.Add("LAN");
            smNum_maxSlots.Minimum = 1;
            smNum_maxSlots.Maximum = 50;
            cb_timeLimit.Items.Insert(0, "No Limit");
            for (int timelimit = 1; timelimit < 60; timelimit++)
            {
                cb_timeLimit.Items.Add(timelimit);
            }
            cb_respawnTime.Items.Insert(0, "Instant Respawn");
            for (int respawntime = 1; respawntime < 121; respawntime++)
            {
                cb_respawnTime.Items.Add(respawntime);
            }
            foreach (var gametype in _state.autoRes.gameTypes)
            {
                dropDown_mapSettingsGameType.Items.Add(gametype.Value.Name);
            }
            dropDown_mapSettingsGameType.SelectedIndex = 0;

            // get PID from database
            process = Process.GetProcessById(_state.Instances[ArrayID].PID.GetValueOrDefault());
            smT_serverName.Text = _state.Instances[ArrayID].ServerName;
            smT_serverPassword.Text = _state.Instances[ArrayID].Password;
            smCB_country.SelectedItem = _state.Instances[ArrayID].CountryCode;
            smCB_sessionType.SelectedIndex = _state.Instances[ArrayID].SessionType;
            smNum_maxSlots.Value = _state.Instances[ArrayID].MaxSlots;
            cb_timeLimit.SelectedIndex = _state.Instances[ArrayID].TimeLimit;
            cb_startDelay.SelectedIndex = _state.Instances[ArrayID].StartDelay;
            cb_replayMaps.SelectedIndex = _state.Instances[ArrayID].LoopMaps;
            num_pspTimer.Value = _state.Instances[ArrayID].PSPTakeOverTime;
            cb_respawnTime.SelectedIndex = _state.Instances[ArrayID].RespawnTime;
            cb_gameDedicated.Checked = _state.Instances[ArrayID].Dedicated;
            cb_requireNova.Checked = _state.Instances[ArrayID].RequireNovaLogin;
            cb_customSkin.Checked = _state.Instances[ArrayID].AllowCustomSkins;
            cb_autoBalance.Checked = _state.Instances[ArrayID].AutoBalance;
            richTextBox1.Text = _state.Instances[ArrayID].MOTD;
            cb_minPing.Checked = _state.Instances[ArrayID].MinPing;
            num_minPing.Text = _state.Instances[ArrayID].MinPingValue.ToString();
            cb_maxPing.Checked = _state.Instances[ArrayID].MaxPing;
            num_maxPing.Text = _state.Instances[ArrayID].MaxPingValue.ToString();
            cb_friendFireKills.Checked = _state.Instances[ArrayID].FriendlyFire;
            cb_showFriendTags.Checked = _state.Instances[ArrayID].FriendlyTags;
            cb_ffWarning.Checked = _state.Instances[ArrayID].FriendlyFireWarning;
            text_bluePass.Text = _state.Instances[ArrayID].BluePassword;
            text_redPass.Text = _state.Instances[ArrayID].RedPassword;
            cb_Tracers.Checked = _state.Instances[ArrayID].ShowTracers;
            cb_TeamClays.Checked = _state.Instances[ArrayID].ShowTeamClays;
            cb_AutoRange.Checked = _state.Instances[ArrayID].AllowAutoRange;
            cb_fatBullets.Checked = _state.Instances[ArrayID].FatBullets;
            cb_enableAutoMsg.Checked = Convert.ToBoolean(_state.Instances[ArrayID].AutoMessages.enable_msg);
            num_autoMsgInterval.ValueChanged -= new EventHandler(this.numericUpDown1_ValueChanged);
            num_autoMsgInterval.Value = _state.Instances[ArrayID].AutoMessages.interval;
            num_autoMsgInterval.ValueChanged += numericUpDown1_ValueChanged;
            foreach (var map in _state.Instances[ArrayID].MapList)
            {
                list_mapRotation.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " <" + map.Value.MapFile + ">");
                selectedMaps.Add(new MapList
                {
                    MapFile = map.Value.MapFile,
                    MapName = map.Value.MapName,
                    CustomMap = map.Value.CustomMap,
                    GameType = map.Value.GameType
                });
            }
            label_currentMapCount.Text = list_mapRotation.Items.Count.ToString() + " / 128";
            foreach (var item in _state.Instances[ArrayID].AutoMessages.messages)
            {
                listBox_AutoMessages.Items.Add(item);
            }

            Dictionary<int, Instance> instanceList = _state.Instances;
            foreach (var item in instanceList[ArrayID].PlayerList)
            {
                var key = item.Key;
                var val = item.Value;
                DataRow playerRow = playersTable.NewRow();
                playerRow["Slot #"] = val.slot;
                playerRow["Name"] = val.name;
                playerRow["Address"] = val.address;
                playerRow["Ping"] = val.ping;
                playersTable.Rows.Add(playerRow);
            }

            // playerTable timer
            // we don't care about the refresh time here, only on the RC.
            playerTimer = new Timer
            {
                Enabled = true,
                Interval = 100
            };
            playerTimer.Tick += PlayerTimer_Tick;

            // Chatlog
            rb_chatAll.Checked = true;
            rb_chatGlobal.Checked = false;
            rb_chatRedTeam.Checked = false;
            rb_chatBlueTeam.Checked = false;
            rb_chatPlayerHist.Checked = false;
            if (cb_chatPlayerSelect.Items.Count > 0)
            {
                cb_chatPlayerSelect.Items.Clear();
            }
            cb_chatPlayerSelect.Enabled = false;
            chatLog.Columns.Add("Date & Time", typeof(DateTime));
            chatLog.Columns.Add("Type", typeof(string));
            chatLog.Columns.Add("Player Name", typeof(string));
            chatLog.Columns.Add("Message", typeof(string));
            BindingListView<PlayerChatLog> blv = new BindingListView<PlayerChatLog>(_state.ChatLogs[ArrayID].Messages);
            data_chatViewer.DataSource = blv;
            //dataGridView5.Sort(dataGridView5.Columns[0], ListSortDirection.Descending);
            data_chatViewer.Columns["dateSent"].Width = 100;
            data_chatViewer.Columns["msgType"].Width = 40;
            data_chatViewer.Columns["PlayerName"].Width = 104;
            data_chatViewer.Columns["msg"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            data_chatViewer.Columns["dateSent"].HeaderText = "Date & Time";
            data_chatViewer.Columns["msgType"].HeaderText = "Type";
            data_chatViewer.Columns["PlayerName"].HeaderText = "Player Name";
            data_chatViewer.Columns["msg"].HeaderText = "Message";
            data_chatViewer.Columns["team"].Visible = false;
            data_chatViewer.Sort(data_chatViewer.Columns[0], ListSortDirection.Descending);


            // setup chatLog options
            specificPlayerTable.Columns.Add("Date & Time", typeof(DateTime));
            specificPlayerTable.Columns.Add("Type", typeof(string));
            specificPlayerTable.Columns.Add("Player Name", typeof(string));
            specificPlayerTable.Columns.Add("Message", typeof(string));


            // setup warnings
            foreach (var item in _state.Instances[ArrayID].CustomWarnings)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem
                {
                    Text = item
                };
                toolStripItem.Click += CustomWarningMenu;
                toolStripMenuItem133.DropDownItems.Add(toolStripItem);
            }

            // enable/Disable VPNCheck for Instance
            checkBox_vpn_disallow.Checked = _state.Instances[ArrayID].enableVPNCheck;

            // weapon restrictions
            WeaponsClass weapons = _state.Instances[ArrayID].WeaponRestrictions;
            cbl_weaponSelection.SetItemChecked(0, weapons.WPN_COLT45);
            cbl_weaponSelection.SetItemChecked(1, weapons.WPN_M9BERETTA);
            cbl_weaponSelection.SetItemChecked(2, weapons.WPN_REMMINGTONSG);
            cbl_weaponSelection.SetItemChecked(3, weapons.WPN_CAR15);
            cbl_weaponSelection.SetItemChecked(4, weapons.WPN_CAR15_203);
            cbl_weaponSelection.SetItemChecked(5, weapons.WPN_M16_BURST);
            cbl_weaponSelection.SetItemChecked(6, weapons.WPN_M16_BURST_203);
            cbl_weaponSelection.SetItemChecked(7, weapons.WPN_M21);
            cbl_weaponSelection.SetItemChecked(8, weapons.WPN_M24);
            cbl_weaponSelection.SetItemChecked(9, weapons.WPN_MCRT_300_TACTICAL);
            cbl_weaponSelection.SetItemChecked(10, weapons.WPN_BARRETT);
            cbl_weaponSelection.SetItemChecked(11, weapons.WPN_SAW);
            cbl_weaponSelection.SetItemChecked(12, weapons.WPN_M60);
            cbl_weaponSelection.SetItemChecked(13, weapons.WPN_M240);
            cbl_weaponSelection.SetItemChecked(14, weapons.WPN_MP5);
            cbl_weaponSelection.SetItemChecked(15, weapons.WPN_G3);
            cbl_weaponSelection.SetItemChecked(16, weapons.WPN_G36);
            cbl_weaponSelection.SetItemChecked(17, weapons.WPN_PSG1);
            cbl_weaponSelection.SetItemChecked(18, weapons.WPN_XM84_STUN);
            cbl_weaponSelection.SetItemChecked(19, weapons.WPN_M67_FRAG);
            cbl_weaponSelection.SetItemChecked(20, weapons.WPN_AN_M8_SMOKE);
            cbl_weaponSelection.SetItemChecked(21, weapons.WPN_SATCHEL_CHARGE);
            cbl_weaponSelection.SetItemChecked(22, weapons.WPN_CLAYMORE);
            cbl_weaponSelection.SetItemChecked(23, weapons.WPN_AT4);

            // chat log drop down default selection
            chat_channelSelection.SelectedIndex = 0;
            if (_state.Users.Count == 0)
            {
                drop_adminList.Items.Add("Admin");
                drop_adminList.SelectedIndex = 0;
            }
            else
            {
                foreach (var userObj in _state.Users)
                {
                    drop_adminList.Items.Add(userObj.Key);
                }
            }
            foreach (var warnMsg in _state.Instances[ArrayID].CustomWarnings)
            {
                listBox_playerWarnMessages.Items.Add(warnMsg);
            }
            label_currentMapPlaying.Text = _state.Instances[ArrayID].Map;
            value_vpn_abuselevel.Value = _state.IPQualityCache[ArrayID].WarnLevel;

            num_scoreFB.Value = _state.Instances[ArrayID].FBScore;
            num_scoreKOTH.Value = _state.Instances[ArrayID].KOTHScore;
            num_scoreDM.Value = _state.Instances[ArrayID].GameScore;
            endOfMapTimer_TrackBar.Value = _state.Instances[ArrayID].ScoreBoardDelay;

        }

        private DataTable GenerateVPNTable()
        {
            DataTable VPNTable = new DataTable();
            VPNTable.Columns.Add("Description".ToString());
            VPNTable.Columns.Add("IP Address".ToString());

            foreach (var item in _state.Instances[ArrayID].VPNWhiteList)
            {
                DataRow newRow = VPNTable.NewRow();
                newRow["Description"] = item.Value.Description;
                newRow["IP Address"] = item.Value.IPAddress.ToString();
                VPNTable.Rows.Add(newRow);
            }

            return VPNTable;
        }

        private void textBox16_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendServerMessage(chat_channelSelection.SelectedIndex, chat_textBoxMsg.Text);
            }
            else
            {
                return;
            }
        }

        private void SendServerMessage(int MsgLocation, string Msg)
        {
            /*  
                0. Global Blue
                1. Global White
                2. Team Red
                3. Team Blue
            */
            int colorbuffer_written = 0;
            byte[] colorcode;
            Process process = Process.GetProcessById(_state.Instances[ArrayID].PID.GetValueOrDefault());
            IntPtr h = process.MainWindowHandle;
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);
            switch (MsgLocation)
            {
                case 1:
                    colorcode = HexConverter.ToByteArray("6A 09".Replace(" ", ""));
                    WriteProcessMemory((int)processHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                    break;
                case 2:
                    colorcode = HexConverter.ToByteArray("6A 04".Replace(" ", ""));
                    WriteProcessMemory((int)processHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                    break;
                case 3:
                    colorcode = HexConverter.ToByteArray("6A 05".Replace(" ", ""));
                    WriteProcessMemory((int)processHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                    break;
                default:
                    colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                    WriteProcessMemory((int)processHandle, 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                    break;
            }
            // post message
            PostMessage(h, (ushort)WM_KEYDOWN, (ushort)GlobalChat, 0);
            PostMessage(h, (ushort)WM_KEYUP, (ushort)GlobalChat, 0);
            Thread.Sleep(50);
            int bytesWritten = 0;
            byte[] buffer;
            buffer = Encoding.Default.GetBytes($"{Msg}\0"); // '\0' marks the end of string
            WriteProcessMemory((int)processHandle, 0x00879A14, buffer, buffer.Length, ref bytesWritten);
            Thread.Sleep(50);
            PostMessage(h, (ushort)WM_KEYDOWN, (ushort)VK_ENTER, 0);
            PostMessage(h, (ushort)WM_KEYUP, (ushort)VK_ENTER, 0);

            // change color to normal
            Thread.Sleep(50);
            int revert_colorbuffer = 0;
            byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
            WriteProcessMemory((int)processHandle, 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
            process.Dispose();
            CloseHandle(processHandle);
            chat_textBoxMsg.Text = string.Empty;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            SendServerMessage(chat_channelSelection.SelectedIndex, chat_textBoxMsg.Text);
        }

        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            data_chatViewer.ClearSelection();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            // add new ban
            string playerName = text_adPlayerName.Text;
            IPAddress ipaddress;

            if (string.IsNullOrWhiteSpace(playerName) == true)
            {
                MessageBox.Show("Please specifiy a player name!", "Missing Entry");
                return;
            }
            if (IPAddress.TryParse(text_abIPAddress.Text, out ipaddress) == false)
            {
                MessageBox.Show("Please enter a valid IP Address!", "Invalid Entry");
                return;
            }
            if (string.IsNullOrWhiteSpace(combo_abReason.Text) == true || combo_abReason.Text == "Select One or Enter Custom")
            {
                MessageBox.Show("Please specifiy reason!", "Missing Entry");
                return;
            }
            if (drop_adminList.SelectedIndex == -1)
            {
                if (_state.Users.Count != 0)
                {
                    MessageBox.Show("Please specifiy an Admin!", "Missing Entry");
                    return;
                }
            }

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();

            SQLiteCommand query = new SQLiteCommand("INSERT INTO `playerbans` (`id`, `profileid`, `player`, `PublicIP`, `dateadded`, `lastseen`, `reason`, `expires`, `bannedby`) VALUES (NULL, @profileid, @playername, @playerip, @dateadded, @date, @reason, @expires, @bannedby);", db);
            query.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            query.Parameters.AddWithValue("@playername", playerName);
            query.Parameters.AddWithValue("@playerip", ipaddress);
            query.Parameters.AddWithValue("@date", DateTime.Now);
            query.Parameters.AddWithValue("@dateadded", DateTime.Now);
            query.Parameters.AddWithValue("@reason", combo_abReason.Text);
            query.Parameters.AddWithValue("@expires", "-1");
            query.Parameters.AddWithValue("@bannedby", drop_adminList.SelectedItem.ToString());
            query.ExecuteNonQuery();
            query.Dispose();
            _state.Instances[ArrayID].BanList.Add(new playerbans
            {
                expires = "-1",
                id = (int)db.LastInsertRowId,
                ipaddress = ipaddress.ToString(),
                lastseen = DateTime.Now,
                newBan = true,
                player = playerName,
                reason = combo_abReason.Text,
                retry = DateTime.Now,
                bannedBy = drop_adminList.SelectedItem.ToString(),
                addedDate = DateTime.Now,
                onlykick = false,
                VPNBan = false
            });
            db.Close();
            db.Dispose();

            DataRow bannedRow = bannedPlayersTable.NewRow();
            bannedRow["Name"] = playerName;
            bannedRow["IP Address"] = ipaddress;
            bannedRow["Time Remaining"] = "Permanent";
            bannedPlayersTable.Rows.Add(bannedRow);
            int index = -1;
            if (bannedPlayersTable.Rows.Count > 0)
            {
                for (int i = 0; i < bannedPlayersTable.Rows.Count; i++) index++;
                grid_bannedPlayerList.CurrentCell = grid_bannedPlayerList[0, index];
                grid_bannedPlayerList.Rows[index].Selected = true;
            }

            text_adPlayerName.Text = string.Empty;
            text_abIPAddress.Text = string.Empty;
            combo_abReason.Text = "Select One or Enter Custom";
            MessageBox.Show("Ban has been added successfully!", "Success");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (grid_bannedPlayerList.CurrentCell == null)
            {
                return;
            }

            string playerName = grid_bannedPlayerList.Rows[grid_bannedPlayerList.CurrentCell.RowIndex].Cells[0].Value.ToString();
            string playerIP = grid_bannedPlayerList.Rows[grid_bannedPlayerList.CurrentCell.RowIndex].Cells[1].Value.ToString();
            int playerIndex = -1;
            for (int i = 0; i < _state.Instances[ArrayID].BanList.Count; i++)
            {
                if (_state.Instances[ArrayID].BanList[i].ipaddress == playerIP && _state.Instances[ArrayID].BanList[i].player == playerName)
                {
                    playerIndex = i;
                    break;
                }
            }
            if (playerIndex == -1)
            {
                MessageBox.Show("Failed to find player index on Server Manager. Error Code: 11");
                return;
            }
            else
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `playerbans` WHERE `id` = @banid AND `profileid` = @profileid;", db);
                cmd.Parameters.AddWithValue("@banid", _state.Instances[ArrayID].BanList[playerIndex].id);
                cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
                db.Dispose();
                _state.Instances[ArrayID].BanList.RemoveAt(playerIndex);
                bannedPlayersTable.Rows.Remove(bannedPlayersTable.Rows[grid_bannedPlayerList.CurrentRow.Index]);

                int index = -1;
                if (bannedPlayersTable.Rows.Count > 0)
                {
                    for (int i = 0; i < bannedPlayersTable.Rows.Count; i++) index++;
                    grid_bannedPlayerList.CurrentCell = grid_bannedPlayerList[0, index];
                    grid_bannedPlayerList.Rows[index].Selected = true;
                }

                MessageBox.Show("The player has been successfully removed from the banlist.", "Success");
            }
        }

        private void comboBox2_TextUpdate(object sender, EventArgs e)
        {
            if (combo_abReason.Text == string.Empty)
            {
                combo_abReason.Text = "Select One or Enter Custom";
            }
        }

        private void listBox4_DoubleClick(object sender, EventArgs e)
        {
            object findString = listBox_playerWarnMessages.SelectedItem;
            bool found = false;
            int index = 0;
            foreach (var item in _state.Instances[ArrayID].CustomWarnings)
            {
                if (item == findString.ToString())
                {
                    found = true;
                    break;
                }
                index++;
            }

            if (found == false)
            {
                MessageBox.Show("Something went wrong while trying to remove the custom warning.", "Oh No!");
            }

            int menuIndex = 0;

            foreach (ToolStripItem contextMenu in toolStripMenuItem133.DropDownItems)
            {
                if (findString.ToString() == contextMenu.Text)
                {
                    break;
                }
                menuIndex++;
            }
            toolStripMenuItem133.DropDownItems.RemoveAt(menuIndex);

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `customwarnings` WHERE `id` = @warningid AND `instanceid` = @instanceid;", db);
            cmd.Parameters.AddWithValue("@warningid", index);
            cmd.Parameters.AddWithValue("@instanceid", _state.Instances[ArrayID].Id);
            cmd.ExecuteNonQuery();
            db.Close();
            _state.Instances[ArrayID].CustomWarnings.RemoveAt(index);
            cmd.Dispose();
            db.Dispose();
            listBox_playerWarnMessages.Items.Remove(findString);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_playerWarnMessageAdd.Text) == true)
            {
                MessageBox.Show("Please put in a message and try again.", "Error");
                return;
            }
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `customwarnings` (`id`, `instanceid`, `message`) VALUES (NULL, @instanceid, @warningmsg);", db);
            cmd.Parameters.AddWithValue("@instanceid", _state.Instances[ArrayID].Id);
            cmd.Parameters.AddWithValue("@warningmsg", textBox_playerWarnMessageAdd.Text);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
            db.Dispose();
            _state.Instances[ArrayID].CustomWarnings.Add(textBox_playerWarnMessageAdd.Text);
            listBox_playerWarnMessages.Items.Add(textBox_playerWarnMessageAdd.Text);
            ToolStripItem toolStripItem = new ToolStripMenuItem
            {
                Text = textBox_playerWarnMessageAdd.Text
            };
            toolStripItem.Click += CustomWarningMenu;
            toolStripMenuItem133.DropDownItems.Add(toolStripItem);
            textBox_playerWarnMessageAdd.Text = string.Empty;
            MessageBox.Show("Warning message has been added sucessfully!", "Success");
        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            rb_roleSelNone.Checked = false;
            for (int i = 0; i < cbl_roleSelection.Items.Count; i++)
            {
                cbl_roleSelection.SetItemChecked(i, true);
            }
        }

        private void radioButton4_Click(object sender, EventArgs e)
        {
            rb_roleSelAll.Checked = false;
            for (int i = 0; i < cbl_roleSelection.Items.Count; i++)
            {
                cbl_roleSelection.SetItemChecked(i, false);
            }
        }

        private void radioButton6_Click(object sender, EventArgs e)
        {
            rb_WeaonSelNone.Checked = false;
            for (int i = 0; i < cbl_weaponSelection.Items.Count; i++)
            {
                cbl_weaponSelection.SetItemChecked(i, true);
            }
        }

        private void radioButton5_Click(object sender, EventArgs e)
        {
            rb_WeaonSelAll.Checked = false;
            for (int i = 0; i < cbl_weaponSelection.Items.Count; i++)
            {
                cbl_weaponSelection.SetItemChecked(i, false);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChatHistory chatHistory = new ChatHistory(_state);
            chatHistory.ShowDialog();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (_state.Instances[ArrayID].Status == InstanceStatus.LOADINGMAP || _state.Instances[ArrayID].Status == InstanceStatus.SCORING)
            {
                MessageBox.Show("Please wait for the server to change maps before updating the map cycle!", "Error");
                return;
            }
            _state.Instances[ArrayID].previousMapList.Clear(); // because fuck you
            _state.Instances[ArrayID].previousMapList = new Dictionary<int, MapList>();
            foreach (var mapEntry in _state.Instances[ArrayID].MapList)
            {
                _state.Instances[ArrayID].previousMapList.Add(_state.Instances[ArrayID].previousMapList.Count, mapEntry.Value);
            }

            _state.Instances[ArrayID].MapList.Clear(); // because fuck you
            _state.Instances[ArrayID].MapList = new Dictionary<int, MapList>();
            foreach (var map in selectedMaps)
            {
                _state.Instances[ArrayID].MapList.Add(_state.Instances[ArrayID].MapList.Count, map);
            }
            ServerManagerUpdateMemory serverManagerUpdateMemory = new ServerManagerUpdateMemory();
            serverManagerUpdateMemory.UpdateMapCycle(_state, ArrayID);
            serverManagerUpdateMemory.UpdateMapCycle2(_state, ArrayID);

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `mapcycle` = @updateMapCycle WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@updateMapCycle", JsonConvert.SerializeObject(_state.Instances[ArrayID].MapList));
            cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            db.Close();
            db.Dispose();

            MapListEdited = false;

            MessageBox.Show("Map list was updated successfully!", "Success");
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (MapListEdited == true)
            {
                MessageBox.Show("You MUST update the maplist before setting the next map.", "Error");
                return;
            }
            if (_state.Instances[ArrayID].Status == InstanceStatus.LOADINGMAP || _state.Instances[ArrayID].Status == InstanceStatus.OFFLINE || _state.Instances[ArrayID].Status == InstanceStatus.SCORING)
            {
                MessageBox.Show("Please wait for the server to be ready. Please try again later.", "Error");
                return;
            }
            if (list_mapRotation.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a map on the RIGHT side.", "Error");
                return;
            }
            ServerManagerUpdateMemory serverManagerUpdateMemory = new ServerManagerUpdateMemory();
            serverManagerUpdateMemory.UpdateNextMap(_state, ArrayID, list_mapRotation.SelectedIndex);
            //_state.Instances[ArrayID].mapListCount = _state.Instances[ArrayID].MapList.Count;
            MessageBox.Show("Next Map Updated Successfully!", "Success");
        }

        private void button24_Click(object sender, EventArgs e)
        {
            MapListEdited = true;
            list_mapRotation.Items.Clear();
            List<MapList> newMapList = _state.autoRes.ShuffleSelectedMapList(selectedMaps);
            selectedMaps.Clear();
            selectedMaps = new List<MapList>();
            foreach (var map in newMapList)
            {
                selectedMaps.Add(map);
                list_mapRotation.Items.Add("|" + map.GameType + "| " + map.MapName + " " + "<" + map.MapFile + ">");
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            // save rotation
            SM_PopupSaveRotation SM_PopupSaveRotation = new SM_PopupSaveRotation(_state, ArrayID, ref selectedMaps);
            SM_PopupSaveRotation.ShowDialog();
        }
        private void button21_Click(object sender, EventArgs e)
        {
            // load rotation
            SM_PopupLoadRotation SM_PopupLoadRotation = new SM_PopupLoadRotation(_state, ArrayID);
            SM_PopupLoadRotation.ShowDialog();
            if (SM_PopupLoadRotation._mapList.Count > 0)
            {
                MapListEdited = true; // set flag forcing the user to click "Update Maps"
                list_mapRotation.Items.Clear();
                foreach (var map in selectedMaps)
                {
                    list_mapRotation.Items.Add("|" + map.GameType + "| " + map.MapName + " <" + map.MapFile + ">");
                }
                selectedMaps = new List<MapList>();
                selectedMaps = SM_PopupLoadRotation._mapList;
                MessageBox.Show("Map Rotation Loaded!", "Success");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MoveMapEntry(-1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MoveMapEntry(1);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            toolTip1.Show(endOfMapTimer_TrackBar.Value.ToString() + " Secs", endOfMapTimer_TrackBar);
        }

        private void Server_Manager_FormClosing(object sender, FormClosingEventArgs e)
        {
            playerTimer.Stop();
            playerTimer.Enabled = false;
            ServerManagerTimer.Enabled = false;
            ServerManagerTimer.Stop();
        }

        private void toolStripMenuItem134_Click(object sender, EventArgs e)
        {
            // next map
            text_newAutoMessage.Text += " $(NextMap)";
        }

        private void toolStripMenuItem135_Click(object sender, EventArgs e)
        {
            // highest exp
            text_newAutoMessage.Text += " $(HighestEXP)";
        }

        private void toolStripMenuItem136_Click(object sender, EventArgs e)
        {
            // most kills
            text_newAutoMessage.Text += " $(MostKills)";
        }

        private void toolStripMenuItem137_Click(object sender, EventArgs e)
        {
            // most deaths
            text_newAutoMessage.Text += " $(MostDeaths)";
        }

        private void toolStripMenuItem138_Click(object sender, EventArgs e)
        {
            // best kdr
            text_newAutoMessage.Text += " $(BestKDR)";
        }

        private void toolStripMenuItem139_Click(object sender, EventArgs e)
        {
            // most med saves
            text_newAutoMessage.Text += " $(MostMedSaves)";
        }

        private void toolStripMenuItem140_Click(object sender, EventArgs e)
        {
            // most revives
            text_newAutoMessage.Text += " $(MostRevives)";
        }

        private void toolStripMenuItem141_Click(object sender, EventArgs e)
        {
            // most headshots
            text_newAutoMessage.Text += " $(MostHeadshots)";
        }

        private void toolStripMenuItem142_Click(object sender, EventArgs e)
        {
            // most suicides
            text_newAutoMessage.Text += " $(MostSuicides)";
        }

        private void toolStripMenuItem143_Click(object sender, EventArgs e)
        {
            // most team kills
            text_newAutoMessage.Text += " $(MostTK)";
        }

        private void toolStripMenuItem144_Click(object sender, EventArgs e)
        {
            text_newAutoMessage.Text += " $(LowestExp)";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // save plugin settings
        }

        private void dataGridView5_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Check if this is a cell in the "Priority" column
            if (data_chatViewer.Columns[e.ColumnIndex].Name == "Type")
            {
                int currentRowIndex = e.RowIndex;
                // Get the cell value
                string teamColumn = e.Value as string;

                // Set the background color based on the priority value
                if (teamColumn == "Team")
                {
                    DataRow currentRow = chatLog.Rows[currentRowIndex];
                    string playerName = currentRow["Player Name"].ToString();
                    foreach (var player in _state.Instances[ArrayID].PlayerList)
                    {
                        if (player.Value.name == playerName)
                        {
                            playerlist.Teams teams = (playerlist.Teams)Enum.Parse(typeof(playerlist.Teams), player.Value.team.ToString());
                            switch (teams)
                            {
                                case playerlist.Teams.TEAM_RED:
                                    e.CellStyle.BackColor = Color.Red;
                                    break;
                                case playerlist.Teams.TEAM_BLUE:
                                    e.CellStyle.BackColor = Color.Blue;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void action_moveAutoMsgUp(object sender, KeyEventArgs e)
        {
            // move the selected item up the list...
            MoveMessage(-1);
            // update the database...
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
            conn.Open();
            SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
            automessages.Parameters.AddWithValue("@profileid", Profile_ID);
            automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(listBox_AutoMessages.Items));
            automessages.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            _state.Instances[ArrayID].AutoMessages.messages.Clear();
            foreach (var item in listBox_AutoMessages.Items)
            {
                _state.Instances[ArrayID].AutoMessages.messages.Add(item.ToString());
            }
        }

        private void action_moveAutoMsgDown(object sender, KeyEventArgs e)
        {
            // move the selected item lower on the list...
            MoveMessage(1);
            // update the database...
            SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
            conn.Open();
            SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @messages WHERE `profile_id` = @profileid;", conn);
            automessages.Parameters.AddWithValue("@profileid", Profile_ID);
            automessages.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(listBox_AutoMessages.Items));
            automessages.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            _state.Instances[ArrayID].AutoMessages.messages.Clear();
            foreach (var item in listBox_AutoMessages.Items)
            {
                _state.Instances[ArrayID].AutoMessages.messages.Add(item.ToString());
            }
        }

        private void cb_enableAutoMsg_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_enableAutoMsg.Checked == true)
            {
                SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                conn.Open();
                SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `enable_msg` = 1 WHERE `profile_id` = @profileid;", conn);
                automessages.Parameters.AddWithValue("@profileid", Profile_ID);
                automessages.ExecuteNonQuery();
                automessages.Dispose();
                conn.Close();
                conn.Dispose();
                _state.Instances[ArrayID].AutoMessages.enable_msg = true;
                _state.Instances[ArrayID].AutoMessages.MsgNumber = 0;
            }
            else
            {
                SQLiteConnection conn = new SQLiteConnection(ProgramConfig.DBConfig);
                conn.Open();
                SQLiteCommand automessages = new SQLiteCommand("UPDATE `instances_config` SET `enable_msg` = 0 WHERE `profile_id` = @profileid;", conn);
                automessages.Parameters.AddWithValue("@profileid", Profile_ID);
                automessages.ExecuteNonQuery();
                automessages.Dispose();
                conn.Close();
                conn.Dispose();
                _state.Instances[ArrayID].AutoMessages.enable_msg = false;
                _state.Instances[ArrayID].AutoMessages.MsgNumber = 0;
            }
        }
    }
}