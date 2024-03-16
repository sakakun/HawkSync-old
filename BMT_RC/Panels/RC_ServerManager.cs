using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Equin.ApplicationFramework;
using HawkSync_RC.classes;
using HawkSync_RC.classes.RCClasses;
using Newtonsoft.Json;
using static log4net.Appender.RollingFileAppender;
using TextBox = System.Windows.Forms.TextBox;
using Timer = System.Windows.Forms.Timer;

/*
using System.Threading.Tasks;
using WatsonTcp;
using System.Dynamic;
using System.Windows.Forms;
using HawkSync_RC.RCFunctions;
using System.Web.UI.WebControls;
*/

namespace HawkSync_RC
{
    public partial class RC_ServerManager : Form
    {
        private const int PROCESS_WM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;

        private readonly AppState _state;

        private readonly int ArrayID = -1;
        private Dictionary<int, MapList> availableMapList;
        private readonly DataTable bannedTable;
        public BindingListView<PlayerChatLog> ChatLogMessages;
        private readonly DataTable ChatLogTable;
        private bool MapListEdited;
        private readonly DataTable playersTable;

        private Timer playerTableTimer;
        private readonly RCSetup RCSetup;
        private DataTable searchBannedTable;
        private List<MapList> selectedMaps;
        private readonly DataTable VPNWhiteListTable;

        /* Server Manager Run */
        public RC_ServerManager(AppState state, RCSetup setup, int profileid)
        {
            InitializeComponent();
            _state = state;

            RCSetup = setup;
            ArrayID = profileid;
            playersTable = new DataTable();
            bannedTable = new DataTable();
            ChatLogTable = new DataTable();
            VPNWhiteListTable = new DataTable();

            new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    playerTableTimer = new Timer();
                    playerTableTimer.Tick += PlayerTableTimer_Tick;
                    playerTableTimer.Interval = 1000; // auto refresh 1 second
                }
            ).Start();
        }

        public static List<MapList> loadList { get; set; }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize,
            ref int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize,
            ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        /* Server Manager Load Settings (Called by InitializeComponent(); */
        private void ServerManager_Load(object sender, EventArgs e)
        {
            // players table
            playersTable.Columns.Add("Slot #");
            playersTable.Columns.Add("Name");
            playersTable.Columns.Add("Address");
            playersTable.Columns.Add("Ping");
            foreach (var playerObj in _state.Instances[ArrayID].PlayerList)
            {
                var row = playersTable.NewRow();
                row["Slot #"] = playerObj.Value.slot;
                row["Name"] = playerObj.Value.name;
                row["Address"] = playerObj.Value.address;
                row["Ping"] = playerObj.Value.ping;
                playersTable.Rows.Add(row);
            }

            playerTableTimer.Interval = 1000;
            playerTableTimer.Enabled = true;
            playerTableTimer.Start();
            grid_playerList.DataSource = playersTable;
            SetupPlayersTable();

            // banned players
            bannedTable.Columns.Add("Name");
            bannedTable.Columns.Add("IP Address");
            bannedTable.Columns.Add("Time Remaining");
            UpdateBannedPlayers();
            SetupBannedPlayersTable();
            RequestVPNWarnLevel(_state.Instances[ArrayID].Id);


            // vpn settings
            checkBox_vpn_disallow.Checked = _state.Instances[ArrayID].enableVPNCheck;
            value_vpn_abuselevel.Value = _state.IPQualityScore[ArrayID].WarnLevel;

            // warnings
            foreach (var warning in _state.Instances[ArrayID].CustomWarnings)
                listBox_playerWarnMessages.Items.Add(warning);

            // Country Codes
            foreach (var countryShortcode in _state.CountryCodes) rcCB_country.Items.Add(countryShortcode);

            rcNum_maxSlots.Minimum = 1;
            rcNum_maxSlots.Maximum = 50;
            cb_timeLimit.Items.Insert(0, "No Limit");
            for (var timelimit = 1; timelimit < 61; timelimit++) { cb_timeLimit.Items.Add(timelimit); }
            cb_respawnTime.Items.Insert(0, "Instant Respawn");
            for (var respawntime = 1; respawntime < 121; respawntime++) { cb_respawnTime.Items.Add(respawntime); }
            foreach (var gametype in _state.autoRes.gameTypes)
            { dropDown_mapSettingsGameType.Items.Add(gametype.Value.Name); }
            dropDown_mapSettingsGameType.SelectedIndex = 0;

            // server settings

            rcT_serverName.Text = _state.Instances[ArrayID].ServerName;
            rcT_serverPassword.Text = _state.Instances[ArrayID].Password;
            rcCB_country.SelectedItem = _state.Instances[ArrayID].CountryCode;
            rcCB_sessionType.SelectedIndex = _state.Instances[ArrayID].SessionType;
            rcNum_maxSlots.Value = _state.Instances[ArrayID].MaxSlots;
            cb_timeLimit.SelectedIndex = _state.Instances[ArrayID].TimeLimit;
            cb_startDelay.SelectedIndex = _state.Instances[ArrayID].StartDelay;
            cb_replayMaps.SelectedIndex = _state.Instances[ArrayID].LoopMaps;
            num_pspTimer.Value = _state.Instances[ArrayID].PSPTakeOverTime;
            cb_respawnTime.SelectedIndex = _state.Instances[ArrayID].RespawnTime;
            cb_gameDedicated.Checked = _state.Instances[ArrayID].Dedicated;
            cb_requireNova.Checked = _state.Instances[ArrayID].RequireNovaLogin;
            cb_customSkin.Checked = _state.Instances[ArrayID].AllowCustomSkins;
            cb_autoBalance.Checked = _state.Instances[ArrayID].AutoBalance;
            num_flagReturn.Value = _state.Instances[ArrayID].FlagReturnTime;

            richTextBox1.Text = _state.Instances[ArrayID].MOTD;


            text_bluePass.Text = _state.Instances[ArrayID].BluePassword;
            text_redPass.Text = _state.Instances[ArrayID].RedPassword;
            
            checkBox40.Checked = _state.Instances[ArrayID].FriendlyFire;
            num_maxFriendKills.Value = _state.Instances[ArrayID].FriendlyFireKills;
            cb_showFriendTags.Checked = _state.Instances[ArrayID].FriendlyTags;
            cb_ffWarning.Checked = _state.Instances[ArrayID].FriendlyFireWarning;
            cb_Tracers.Checked = _state.Instances[ArrayID].ShowTracers;
            cb_TeamClays.Checked = _state.Instances[ArrayID].ShowTeamClays;
            cb_AutoRange.Checked = _state.Instances[ArrayID].AllowAutoRange;
            
            num_scoreFB.Value = _state.Instances[ArrayID].FBScore;
            num_scoreKOTH.Value = _state.Instances[ArrayID].ZoneTimer;
            num_scoreDM.Value = _state.Instances[ArrayID].GameScore;
            num_MaxTeamLives.Value = _state.Instances[ArrayID].MaxTeamLives;
            cb_minPing.Checked = _state.Instances[ArrayID].MinPing;
            num_minPing.Enabled = _state.Instances[ArrayID].MinPing;
            num_minPing.Value = _state.Instances[ArrayID].MinPingValue;
            cb_maxPing.Checked = _state.Instances[ArrayID].MaxPing;
            num_maxPing.Value = _state.Instances[ArrayID].MaxPingValue;
            num_maxPing.Enabled = _state.Instances[ArrayID].MaxPing;
            endOfMapTimer_TrackBar.Value = _state.Instances[ArrayID].ScoreBoardDelay;
            cb_oneShotKills.Checked = _state.Instances[ArrayID].OneShotKills;
            cb_fatBullets.Checked = _state.Instances[ArrayID].FatBullets;
            cb_destroyBuildings.Checked = _state.Instances[ArrayID].DestroyBuildings;
            cbl_weaponSelection.SetItemChecked(0, _state.Instances[ArrayID].WeaponRestrictions.WPN_COLT45);
            cbl_weaponSelection.SetItemChecked(1, _state.Instances[ArrayID].WeaponRestrictions.WPN_M9BERETTA);
            cbl_weaponSelection.SetItemChecked(2, _state.Instances[ArrayID].WeaponRestrictions.WPN_REMMINGTONSG);
            cbl_weaponSelection.SetItemChecked(3, _state.Instances[ArrayID].WeaponRestrictions.WPN_CAR15);
            cbl_weaponSelection.SetItemChecked(4, _state.Instances[ArrayID].WeaponRestrictions.WPN_CAR15_203);
            cbl_weaponSelection.SetItemChecked(5, _state.Instances[ArrayID].WeaponRestrictions.WPN_M16_BURST);
            cbl_weaponSelection.SetItemChecked(6, _state.Instances[ArrayID].WeaponRestrictions.WPN_M16_BURST_203);
            cbl_weaponSelection.SetItemChecked(7, _state.Instances[ArrayID].WeaponRestrictions.WPN_M21);
            cbl_weaponSelection.SetItemChecked(8, _state.Instances[ArrayID].WeaponRestrictions.WPN_M24);
            cbl_weaponSelection.SetItemChecked(9, _state.Instances[ArrayID].WeaponRestrictions.WPN_MCRT_300_TACTICAL);
            cbl_weaponSelection.SetItemChecked(10, _state.Instances[ArrayID].WeaponRestrictions.WPN_BARRETT);
            cbl_weaponSelection.SetItemChecked(11, _state.Instances[ArrayID].WeaponRestrictions.WPN_SAW);
            cbl_weaponSelection.SetItemChecked(12, _state.Instances[ArrayID].WeaponRestrictions.WPN_M60);
            cbl_weaponSelection.SetItemChecked(13, _state.Instances[ArrayID].WeaponRestrictions.WPN_M240);
            cbl_weaponSelection.SetItemChecked(14, _state.Instances[ArrayID].WeaponRestrictions.WPN_MP5);
            cbl_weaponSelection.SetItemChecked(15, _state.Instances[ArrayID].WeaponRestrictions.WPN_G3);
            cbl_weaponSelection.SetItemChecked(16, _state.Instances[ArrayID].WeaponRestrictions.WPN_G36);
            cbl_weaponSelection.SetItemChecked(17, _state.Instances[ArrayID].WeaponRestrictions.WPN_PSG1);
            cbl_weaponSelection.SetItemChecked(18, _state.Instances[ArrayID].WeaponRestrictions.WPN_XM84_STUN);
            cbl_weaponSelection.SetItemChecked(19, _state.Instances[ArrayID].WeaponRestrictions.WPN_M67_FRAG);
            cbl_weaponSelection.SetItemChecked(20, _state.Instances[ArrayID].WeaponRestrictions.WPN_AN_M8_SMOKE);
            cbl_weaponSelection.SetItemChecked(11, _state.Instances[ArrayID].WeaponRestrictions.WPN_SATCHEL_CHARGE);
            cbl_weaponSelection.SetItemChecked(12, _state.Instances[ArrayID].WeaponRestrictions.WPN_CLAYMORE);
            cbl_weaponSelection.SetItemChecked(13, _state.Instances[ArrayID].WeaponRestrictions.WPN_AT4);


            selectedMaps = new List<MapList>();
            foreach (var item in _state.Instances[ArrayID].MapList)
            {
                selectedMaps.Add(item.Value);
                list_mapRotation.Items.Add("|" + item.Value.GameType + "| " + item.Value.MapName + " <" +
                                           item.Value.MapFile + ">");
            }

            label_currentMapCount.Text = list_mapRotation.Items.Count + " / 128";
            foreach (var msg in _state.Instances[ArrayID].AutoMessages.messages) listBox_AutoMessages.Items.Add(msg);
            cb_enableAutoMsg.Checked = _state.Instances[ArrayID].AutoMessages.enable_msg;
            num_autoMsgInterval.ValueChanged -= autoMessage_intervalChange;
            num_autoMsgInterval.Value = _state.Instances[ArrayID].AutoMessages.interval;
            num_autoMsgInterval.ValueChanged += autoMessage_intervalChange;

            rb_chatAll.Checked = true;
            if (cb_chatPlayerSelect.Items.Count > 0) cb_chatPlayerSelect.Items.Clear();

            // All Chat Log
            ChatLogTable.Columns.Add("Date & Time");
            ChatLogTable.Columns.Add("Type");
            ChatLogTable.Columns.Add("Player Name");
            ChatLogTable.Columns.Add("Message");
            ChatLogMessages = new BindingListView<PlayerChatLog>(_state.ChatLogs[ArrayID].Messages);
            data_chatViewer.DataSource = ChatLogMessages;
            data_chatViewer.Columns["dateSent"].Width = 100;
            data_chatViewer.Columns["msgType"].Width = 40;
            data_chatViewer.Columns["PlayerName"].Width = 104;
            data_chatViewer.Columns["msg"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            data_chatViewer.Columns["dateSent"].HeaderText = "Date & Time";
            data_chatViewer.Columns["msgType"].HeaderText = "Type";
            data_chatViewer.Columns["PlayerName"].HeaderText = "Player Name";
            data_chatViewer.Columns["msg"].HeaderText = "Message";
            data_chatViewer.Columns["team"].Visible = true;
            data_chatViewer.Sort(data_chatViewer.Columns[0], ListSortDirection.Descending);

            // vpn whitelist
            VPNWhiteListTable.Columns.Add("Description");
            VPNWhiteListTable.Columns.Add("IP Address");
            foreach (var entry in _state.Instances[ArrayID].VPNWhiteList)
            {
                var newEntry = VPNWhiteListTable.NewRow();
                newEntry["Description"] = entry.Value.Description;
                newEntry["IP Address"] = entry.Value.IPAddress;
                VPNWhiteListTable.Rows.Add(newEntry);
            }

            grid_vpn_allowlist.DataSource = VPNWhiteListTable;

            // setup message box
            chat_channelSelection.SelectedIndex = 0;


            // check if BHD is running to enable/disable Spectate
            var pid = -1;
            foreach (var p in Process.GetProcesses())
                //Console.WriteLine(p.ProcessName + " - " + p.MainWindowTitle);
                if (p.ProcessName == "dfbhd" && p.MainWindowTitle == "Delta Force,  V1.5.0.5")
                {
                    pid = p.Id;
                    RCSetup.gameProcess = p;
                    break;
                }

            if (pid == -1)
            {
                playerListMenu_spectate.Enabled = false;
            }
            else
            {
                RCSetup.GamePID = pid;
                RCSetup.GameHandle =
                    OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION,
                        false, pid);
                playerListMenu_spectate.Enabled = true;
                // read "myPlayerAddress"
                if (RCSetup.myPlayerAddress == 0)
                {
                    var myPlayerAddressBytes = new byte[4];
                    var bytesRead = 0;
                    ReadProcessMemory((int)RCSetup.GameHandle, 0x7F2DD4, myPlayerAddressBytes,
                        myPlayerAddressBytes.Length, ref bytesRead);
                    RCSetup.myPlayerAddress = BitConverter.ToInt32(myPlayerAddressBytes, 0);
                }

                // read "myPlayerName"
                var myPlayerNameBytes = new byte[30];
                var myPlayerNameRead = 0;
                ReadProcessMemory((int)RCSetup.GameHandle, RCSetup.myPlayerAddress + 0xC8, myPlayerNameBytes,
                    myPlayerNameBytes.Length, ref myPlayerNameRead);

                var myPlayerName = Encoding.Default.GetString(myPlayerNameBytes);
                myPlayerName = myPlayerName.Remove(myPlayerName.IndexOf('\0'));

                // determine "myPlayerSlot"
                var myPlayerSlot = -1;
                foreach (var item in _state.Instances[ArrayID].PlayerList)
                    if (item.Value.name == myPlayerName)
                    {
                        myPlayerSlot = item.Key;
                        break;
                    }

                RCSetup.myPlayerSlot = myPlayerSlot;
                if (myPlayerSlot == -1) playerListMenu_spectate.Enabled = false;
                // calulcate HostAddress then we can caluate the address of each slot
                var HostAddr = RCSetup.myPlayerAddress;
                for (var i = 0; i < _state.Instances[ArrayID].MaxSlots; i++)
                {
                    var tryHostNameBytes = new byte[30];
                    var tryHostNameBytes_Read = 0;
                    ReadProcessMemory((int)RCSetup.GameHandle, HostAddr + 0xC8, tryHostNameBytes,
                        tryHostNameBytes.Length, ref tryHostNameBytes_Read);
                    var tryHostName = Encoding.Default.GetString(tryHostNameBytes);
                    tryHostName = tryHostName.Remove(tryHostName.IndexOf('\0'));
                    if (tryHostName != _state.Instances[ArrayID].HostName)
                    {
                        HostAddr -= 0x29C;
                        continue;
                    }

                    if (tryHostName == _state.Instances[ArrayID].HostName) break;
                }

                RCSetup.HostAddr = HostAddr;
            }

            RCSetup.playerAddresses = new Dictionary<string, playerAddress>();
            label_currentSpec.Text = string.Empty;
            RCSetup.SpectateTimer = new Timer
            {
                Enabled = false,
                Interval = 1
            };
            RCSetup.SpectateTimer.Tick += SpectateTimer_Tick;
        }

        /* Server Manager Setup Tables */
        private void SetupPlayersTable()
        {
            grid_playerList.Columns["Slot #"].Width = 45;
            grid_playerList.Columns["Slot #"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid_playerList.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_playerList.Columns["Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid_playerList.Columns["Address"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_playerList.Columns["Address"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid_playerList.Columns["Ping"].Width = 40;
            grid_playerList.Columns["Ping"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        private void SetupBannedPlayersTable()
        {
            grid_bannedPlayerList.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_bannedPlayerList.Columns["Name"].HeaderCell.Style.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            grid_bannedPlayerList.Columns["IP Address"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_bannedPlayerList.Columns["IP Address"].HeaderCell.Style.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            grid_bannedPlayerList.Columns["Time Remaining"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid_bannedPlayerList.Columns["Time Remaining"].HeaderCell.Style.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }
        /* Server Manager Setup Tables END*/

        /* Player Ticker Functions
         *  - This ticker also updates the other lists within the Server Window
         *  - Informational Only - No Return Statements to Server Manager
         */
        private void PlayerTableTimer_Tick(object sender, EventArgs e)
        {
            UpdatePlayerlist();
            UpdatePlayerCounter();
            UpdateWarnList();
            UpdateChatLogs();
            UpdateCurrentMap();
            UpdatePlayerAddress();
            UpdateBannedPlayers();
        }

        private void UpdatePlayerlist()
        {
            if (playersTable.Rows.Count == _state.Instances[ArrayID].PlayerList.Count)
                try
                {
                    foreach (DataRow playerRow in playersTable.Rows)
                    {
                        var playerSlot = Convert.ToInt32(playerRow["Slot #"]);
                        if (!_state.Instances[ArrayID].PlayerList.ContainsKey(playerSlot)) return;
                        var serverData = _state.Instances[ArrayID].PlayerList[playerSlot];
                        playerRow["Name"] = serverData.name;
                        playerRow["Address"] = serverData.address;
                        playerRow["Ping"] = serverData.ping;
                        var Bnd = new BindingSource
                        {
                            DataSource = playersTable
                        };
                        var index = Bnd.Find("Slot #", playerRow["Slot #"]);
                        if (index == -1) return;
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
                }
                catch
                {
                    return; // something went wrong while trying to grab player data... so skip this round.
                }
            else if (playersTable.Rows.Count > _state.Instances[ArrayID].PlayerList.Count)
                try
                {
                    var removePlayerData = new List<DataRow>();
                    foreach (DataRow playerData in playersTable.Rows)
                        if (_state.Instances[ArrayID].PlayerList.ContainsKey(Convert.ToInt32(playerData["Slot #"])) ==
                            false)
                            removePlayerData.Add(playerData);
                    foreach (var row in removePlayerData) playersTable.Rows.Remove(row);
                }
                catch
                {
                    return; // don't crash fix
                }
            else if (playersTable.Rows.Count < _state.Instances[ArrayID].PlayerList.Count)
                try
                {
                    foreach (var playerObj in _state.Instances[ArrayID].PlayerList)
                    {
                        var PlayerFound = false;
                        foreach (DataRow playerRow in playersTable.Rows)
                            if (playerObj.Value.name == playerRow["Name"].ToString())
                            {
                                PlayerFound = true;
                                break;
                            }

                        if (PlayerFound == false)
                        {
                            var newPlayer = playersTable.NewRow();
                            newPlayer["Slot #"] = playerObj.Key;
                            newPlayer["Name"] = playerObj.Value.name;
                            newPlayer["Address"] = playerObj.Value.address;
                            playersTable.Rows.Add(newPlayer);
                            // add player from list...
                        }
                    }
                }
                catch
                {
                    return; // don't crash...
                }

            UpdatePlayerTeamColors();
        }

        private void UpdatePlayerTeamColors()
        {
            /* Called by UpdatePlayerlist() */
            for (var i = 1; i < playersTable.Rows.Count; i++)
            {
                var bhd = new BindingSource
                {
                    DataSource = playersTable
                };
                var index = bhd.Find("Slot #", i);
                if (index == -1) continue;
            }
        }

        private void UpdatePlayerCounter()
        {
            group_currentPlayers.Text = "Current Players: " + _state.Instances[ArrayID].PlayerList.Count;
        }

        private void UpdateWarnList()
        {
            if (_state.Instances[ArrayID].CustomWarnings.Count == 0)
            {
                toolStripMenuItem133.DropDownItems.Clear();
                return; // do nothing since there are no custom warnings
            }

            var warningList = toolStripMenuItem133.DropDownItems;
            if (_state.Instances[ArrayID].CustomWarnings.Count == toolStripMenuItem133.DropDownItems.Count)
            {
                var index = 0;
                foreach (var warning in _state.Instances[ArrayID].CustomWarnings)
                {
                    toolStripMenuItem133.DropDownItems[index].Text = warning;
                    index++;
                }
            }
            else if (_state.Instances[ArrayID].CustomWarnings.Count > toolStripMenuItem133.DropDownItems.Count)
            {
                foreach (var warningItem in _state.Instances[ArrayID].CustomWarnings)
                {
                    var found = false;
                    var newIndex = 0;
                    foreach (ToolStripItem item in toolStripMenuItem133.DropDownItems)
                    {
                        if (warningItem == item.Text)
                        {
                            found = true;
                            break;
                        }

                        newIndex++;
                    }

                    if (found)
                        continue;
                    toolStripMenuItem133.DropDownItems.Add(warningItem, null, SendPlayerWarning);
                }
            }
            else if (_state.Instances[ArrayID].CustomWarnings.Count < toolStripMenuItem133.DropDownItems.Count)
            {
                var removeList = new List<int>();
                foreach (ToolStripItem warningMenuItem in toolStripMenuItem133.DropDownItems)
                {
                    var index = 0;
                    var foundWarning = false;
                    foreach (var warning in _state.Instances[ArrayID].CustomWarnings)
                    {
                        if (warningMenuItem.Text == warning)
                        {
                            foundWarning = true;
                            break;
                        }

                        index++;
                    }

                    if (foundWarning == false) removeList.Add(index);
                }

                foreach (var i in removeList) toolStripMenuItem133.DropDownItems.RemoveAt(i);
            }
        }

        private void UpdateChatLogs()
        {
            ChatLogMessages.DataSource = _state.ChatLogs[ArrayID].Messages;
        }

        private void UpdateCurrentMap()
        {
            label_currentMapPlaying.Text = _state.Instances[ArrayID].Map;
        }

        private void UpdatePlayerAddress()
        {
            if (_state.Instances[ArrayID].Status == InstanceStatus.SCORING ||
                _state.Instances[ArrayID].Status == InstanceStatus.LOADINGMAP) return;

            if (_state.Instances[ArrayID].Status == InstanceStatus.ONLINE ||
                _state.Instances[ArrayID].Status == InstanceStatus.STARTDELAY)
            {
                var playerListAddress = RCSetup.HostAddr;
                for (var p = 0; p < _state.Instances[ArrayID].MaxSlots; p++)
                {
                    playerListAddress += 0x29C;
                    var playerNameBytes = new byte[30];
                    var playerNameBytes_Read = 0;
                    ReadProcessMemory((int)RCSetup.GameHandle, playerListAddress + 0xC8, playerNameBytes,
                        playerNameBytes.Length, ref playerNameBytes_Read);

                    var playerName = Encoding.Default.GetString(playerNameBytes);
                    playerName = playerName.Remove(playerName.IndexOf('\0'));
                    var playerSlot = -1;

                    if (playerName != "")
                    {
                        foreach (var player in _state.Instances[ArrayID].PlayerList)
                            if (player.Value.name == playerName)
                            {
                                playerSlot = player.Key;
                                break;
                            }

                        if (!RCSetup.playerAddresses.ContainsKey(playerName))
                        {
                            RCSetup.playerAddresses.Add(playerName, new playerAddress
                            {
                                memoryAddress = playerListAddress,
                                slot = playerSlot
                            });
                        }
                        else
                        {
                            RCSetup.playerAddresses[playerName].memoryAddress = playerListAddress;
                            RCSetup.playerAddresses[playerName].slot = playerSlot;
                        }
                    }
                }
            }
        }

        private void UpdateBannedPlayers()
        {
            // Track players that are still present in the BanList
            var remainingPlayers = new HashSet<string>();

            // Update existing rows and track remaining players
            foreach (DataRow row in bannedTable.Rows)
            {
                var playerName = row["Name"].ToString();
                var playerIP = row["IP Address"].ToString();
                var expires = row["Time Remaining"].ToString();

                // Check if the player is still in the BanList
                var stillBanned = _state.Instances[ArrayID].BanList.Any(bannedPlayer =>
                    bannedPlayer.player == playerName && bannedPlayer.ipaddress == playerIP);

                if (!stillBanned)
                {
                    // Player is no longer in the BanList, remove the row
                    row.Delete();
                }
                else
                {
                    // Player is still in the BanList, update time remaining if necessary
                    if (expires != "Permanent")
                    {
                        var remainingDays = (DateTime.Parse(expires) - DateTime.Now).TotalDays;
                        if (remainingDays <= 0)
                        {
                            row["Time Remaining"] = "Expired";
                        }
                        else if (remainingDays > 2)
                        {
                            row["Time Remaining"] = $"{(int)remainingDays} Days";
                        }
                        else if (remainingDays > 1.5)
                        {
                            row["Time Remaining"] = "2 Days";
                        }
                        else
                        {
                            row["Time Remaining"] = "Today";
                        }
                    }
                }

                // Add player to remaining players set
                remainingPlayers.Add(playerName);
            }

            // Add new rows for players that are not already in the table
            foreach (var bannedPlayer in _state.Instances[ArrayID].BanList)
            {
                if (!remainingPlayers.Contains(bannedPlayer.player))
                {
                    var newRow = bannedTable.NewRow();
                    newRow["Name"] = bannedPlayer.player;
                    newRow["IP Address"] = bannedPlayer.ipaddress;
                    newRow["Time Remaining"] = bannedPlayer.expires == "-1" ? "Permanent" : "Today"; // or any initial value

                    bannedTable.Rows.Add(newRow);
                }
            }

            // Accept changes after all modifications
            bannedTable.AcceptChanges();

            // Update the data source
            grid_bannedPlayerList.DataSource = bannedTable;
        }

        /* Player Ticker Functions END */


        /* Server Manager: Players Tab
         *  - Current Player List & Right Click Menu
         *  - Banned Players
         *  - VPN Settings
         *  - Warning Messages
         *  - Misc
         *    - Player Spectating
         */

        // Players Tab: Players List
        private void playerList_menuToggle(object sender, MouseEventArgs e)
        {
            /* Player List Context Menu, Called by grid_playerList Mouse Click */
            if (e.Button == MouseButtons.Right)
            {
                var numPlayers = _state.Instances[ArrayID].NumPlayers;
                if (numPlayers == 0) return;

                var hitTest = grid_playerList.HitTest(e.X, e.Y);
                if (hitTest.RowIndex == -1) return;
                grid_playerList.Rows[hitTest.RowIndex].Selected = true;
                playerList_contextMenu.Show(this,
                    new Point(e.X + ((Control)sender).Left + 20, e.Y + ((Control)sender).Top + 20));
            }
        }

        private void playerListAction_click(object sender, EventArgs e, string action, bool permBan = false, int days = 0)
        {
            var textValue = "";
            string actionType = "kicked";
            string dateTime = DateTime.Now.AddDays(days).ToString();

            if (permBan || days > 0)
            {
                actionType = "banned";
            }

            if (permBan)
            {
                dateTime = "-1";
            }

            if (sender is ToolStripMenuItem clickedItem)
            {
                textValue = clickedItem.Text;
            }
            else if (sender is ToolStripTextBox reasonBox)
            {
                if (reasonBox.Text == "Custom Reason" || reasonBox.Text == string.Empty)
                {
                    reasonBox.Text = "Custom Reason";
                    return;
                }

                playerList_contextMenu.Hide();
                textValue = reasonBox.Text;
                reasonBox.Text = "Custom Reason";
            }

            try
            {
                var request = new Dictionary<dynamic, dynamic>
                {
                    { "action", action },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "slot", Convert.ToInt32(grid_playerList.SelectedCells[0].Value) },
                    { "banReason", textValue },
                    { "expires", dateTime }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    MessageBox.Show($"Player has been {actionType} successfully. Reason: " + textValue, "Success");
                else
                    MessageBox.Show("Server has reported an error.");
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("HawkSync_RC", ex.ToString(), EventLogEntryType.Error);
                MessageBox.Show("Connection timed out while trying to send " + action + " command.");
            }
        }

        private void playerListAction_custom(object sender, KeyEventArgs e, string action, bool permBan = false, int days = 0)
        {
            if (e.KeyCode == Keys.Enter) playerListAction_click(sender, e, action, permBan, days);
        }

        // Players Tab: Banned Players
        // Players Tab: VPN Settings
        private void RequestVPNWarnLevel(int id)
        {
            if (!_state.IPQualityScore.ContainsKey(ArrayID))
            {
                var request = new Dictionary<dynamic, dynamic>
                {
                    { "action", "BMTRC.GetVPNSettings" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id }
                };
                var Response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)Response["Status"] == OpenClass.Status.SUCCESS)
                    _state.IPQualityScore.Add(ArrayID, new ipqualityscore
                    {
                        WarnLevel = (int)Response["WarnLevel"]
                    });
            }
        }


        // Players Tab: Warning Messages
        private void playerWarn_doubleClick(object sender, EventArgs e)
        {
            /* Delete Warning Message, Called by listBox_playerWarnMessages Double Click */
            if (listBox_playerWarnMessages.SelectedIndex ==
                -1) return; // triggered a double click without anything selected...
            var request = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.DeleteWarning" },
                { "sessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "warnID", listBox_playerWarnMessages.SelectedIndex }
            };
            var Response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)Response["Status"] == OpenClass.Status.SUCCESS)
            {
                listBox_playerWarnMessages.Items.RemoveAt(listBox_playerWarnMessages.SelectedIndex);
                return;
            }

            if ((OpenClass.Status)Response["Status"] == OpenClass.Status.FAILURE)
                MessageBox.Show("Something went wrong. Please try again later.", "Failure");
            else
                Console.WriteLine("Something royally fucked up. Contact Babstats Staff!");
        }

        private void playerWarn_addWarnClick(object sender, EventArgs e)
        {
            /* Add Warning Message, Called by btn_playerWarnMessageAdd */
            if (string.IsNullOrEmpty(textBox_playerWarnMessageAdd.Text) ||
                string.IsNullOrWhiteSpace(textBox_playerWarnMessageAdd.Text))
            {
                MessageBox.Show("Please enter a valid warning.", "Error");
                return;
            }

            var request = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.CreateWarning" },
                { "sessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "newWarning", textBox_playerWarnMessageAdd.Text }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                listBox_playerWarnMessages.Items.Add(textBox_playerWarnMessageAdd.Text);
                textBox_playerWarnMessageAdd.Text = string.Empty;
            }
            else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong!", "Error");
            }
        }

        // Players Tab - Supporting Functions
        private void SendPlayerWarning(object sender, EventArgs e)
        {
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            var button = (ToolStripItem)sender;
            var request = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.WarnPlayer" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "warning", button.Text },
                { "slot", selectedPlayerSlot }
            };

            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                MessageBox.Show("Player has been warned!", "Success");
            else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                MessageBox.Show("Something went wrong.", "Error");
        }


        // Players Tab: Misc (Spectating)
        private void SpectateTimer_Tick(object sender, EventArgs e)
        {
            var playerNameBytes = new byte[30];
            var playerNameRead = 0;
            ReadProcessMemory((int)RCSetup.GameHandle, RCSetup.spectateAddress + 0xC8, playerNameBytes,
                playerNameBytes.Length, ref playerNameRead);
            var spectatePlayerName = Encoding.Default.GetString(playerNameBytes);
            spectatePlayerName = spectatePlayerName.Remove(spectatePlayerName.IndexOf('\0'));

            // detect if I'm dead? - Reset
            var myPlayerHealthBytes = new byte[4];
            var myPlayerHealthRead = 0;
            ReadProcessMemory((int)RCSetup.GameHandle, RCSetup.myPlayerAddress + 0xE2, myPlayerHealthBytes,
                myPlayerHealthBytes.Length, ref myPlayerHealthRead);
            var myPlayerHealth = BitConverter.ToInt32(myPlayerHealthBytes, 0);

            // reset SpecMod if player disconnects, or server is scoring, or if my player dies
            if (spectatePlayerName == "" || _state.Instances[ArrayID].Status == InstanceStatus.SCORING ||
                myPlayerHealth == 0 || (myPlayerHealth > 100 &&
                                        !_state.Instances[ArrayID].GodModeList.Contains(RCSetup.myPlayerSlot)))
            {
                var blankData = new byte[668];
                var restoreAddrBytes = BitConverter.GetBytes(RCSetup.myPlayerAddress);
                var restoreAddrWritten = 0;
                var blankDataWritten = 0;
                WriteProcessMemory((int)RCSetup.GameHandle, 0x007F2DD4, restoreAddrBytes, restoreAddrBytes.Length,
                    ref restoreAddrWritten);
                WriteProcessMemory((int)RCSetup.GameHandle, RCSetup.tempSpectateAddress, blankData, blankData.Length,
                    ref blankDataWritten);
                label_currentSpec.Text = string.Empty;
                playerListMenu_spectate.Text = "Spectate";
                RCSetup.SpectateTimer.Stop();
                RCSetup.SpectateTimer.Enabled = false;
                return;
            }

            var spectateData = new byte[668];
            var spectateDataRead = 0;
            ReadProcessMemory((int)RCSetup.GameHandle, RCSetup.spectateAddress, spectateData, spectateData.Length,
                ref spectateDataRead);
            var spectateDataWritten = 0;
            WriteProcessMemory((int)RCSetup.GameHandle, RCSetup.tempSpectateAddress, spectateData, spectateData.Length,
                ref spectateDataWritten);
            label_currentSpec.Text = "Spectating: " + RCSetup.spectateName;
        }


        /* Server Manager: Settings Tab
         *
         */
        private void serverSettings_updateSettingsClick(object sender, EventArgs e)
        {
            /* Update Server Settings, Called by btn_serverSettingsUpdate */
            // update changes
            var updateList = new List<string>();
            // server name
            if (!string.IsNullOrEmpty(rcT_serverName.Text) && !string.IsNullOrWhiteSpace(rcT_serverName.Text) &&
                rcT_serverName.Text != _state.Instances[ArrayID].ServerName)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateServerName" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "ServerName", rcT_serverName.Text }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Server Name has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating ServerName.");
            }

            // country code
            if (rcCB_country.SelectedIndex != 0 &&
                rcCB_country.SelectedItem.ToString() != _state.Instances[ArrayID].CountryCode)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateCountryCode" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "CountryCode", rcCB_country.SelectedIndex.ToString() }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("The Country Code has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Country Code.");
            }

            // server password
            /*
            if (rcT_serverPassword.Text != _state.Instances[ArrayID].Password)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateCountryCode" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "password", rcT_serverPassword.Text }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Server Password has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Country Code.");
            }
            */
            // session type
            /*
            if (rcCB_sessionType.SelectedIndex != _state.Instances[ArrayID].SessionType)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateSessionType" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "SessionType", rcCB_sessionType.SelectedIndex }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Session Type has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Session Type.");
            }
            */
            // max slots
            if (Convert.ToInt32(rcNum_maxSlots.Value) != _state.Instances[ArrayID].MaxSlots)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateSessionType" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "SessionType", Convert.ToInt32(rcNum_maxSlots.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Max Player Slots Updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Max Player Slots.");
            }

            // time limit
            if (cb_timeLimit.SelectedIndex != _state.Instances[ArrayID].TimeLimit)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateTimeLimit" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "TimeLimit", cb_timeLimit.SelectedIndex }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Time Limit has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Time Limit.");
            }

            // start delay
            if (cb_startDelay.SelectedIndex != _state.Instances[ArrayID].StartDelay)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateStartDelay" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "StartDelay", cb_startDelay.SelectedIndex }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Start Delay has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Start Delay.");
            }

            // replay maps
            if (cb_replayMaps.SelectedIndex != _state.Instances[ArrayID].LoopMaps)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateLoopMaps" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "LoopMaps", cb_replayMaps.SelectedIndex }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Replay Maps has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Replay Maps setting.");
            }

            // Respawn Time
            if (cb_respawnTime.SelectedIndex != _state.Instances[ArrayID].RespawnTime)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateRespawnTime" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "RespawnTime", cb_respawnTime.SelectedIndex }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Respawn Time has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Respawn Time.");
            }

            // Require Nova
            if (cb_requireNova.Checked != _state.Instances[ArrayID].RequireNovaLogin)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateRequireNovaLogin" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "RequireNovaLogin", cb_requireNova.Checked }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Require Nova Login has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Require Nova Login.");
            }

            // Custom Skins
            if (cb_customSkin.Checked != _state.Instances[ArrayID].AllowCustomSkins)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateAllowCustomSkins" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "AllowCustomSkins", cb_customSkin.Checked }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Allow Custom Skins has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Allow Custom Skins.");
            }

            // MOTD
            if (richTextBox1.Text != _state.Instances[ArrayID].MOTD)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateMOTD" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "MOTD", richTextBox1.Text }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("MOTD has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating MOTD.");
            }

            // Blue Team Password
            if (text_bluePass.Text != _state.Instances[ArrayID].BluePassword)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateBluePassword" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "BluePassword", text_bluePass.Text }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Blue Team Password has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Blue Team Password.");
            }

            // Red Team Password
            if (text_redPass.Text != _state.Instances[ArrayID].RedPassword)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateRedPassword" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "RedPassword", text_redPass.Text }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Red Team Password has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Red Team Password.");
            }

            if (Convert.ToInt32(num_scoreFB.Value) != _state.Instances[ArrayID].FBScore)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateFBScore" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "FBScore", Convert.ToInt32(num_scoreFB.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Flag Ball Score has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Flag Ball Score.");
            }

            if (Convert.ToInt32(num_scoreKOTH.Value) != _state.Instances[ArrayID].ZoneTimer)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateKOTHScore" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "KOTHScore", Convert.ToInt32(num_scoreKOTH.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("KOTH/TKOTH Score has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating KOTH/TKOTH Score.");
            }

            if (Convert.ToInt32(num_scoreDM.Value) != _state.Instances[ArrayID].GameScore)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateGameScore" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "GameScore", Convert.ToInt32(num_scoreDM.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("DM/TDM Score has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating DM/TDM Score.");
            }

            if (checkBox40.Checked != _state.Instances[ArrayID].FriendlyFire ||
                cb_showFriendTags.Checked != _state.Instances[ArrayID].FriendlyTags ||
                cb_TeamClays.Checked != _state.Instances[ArrayID].ShowTeamClays ||
                cb_autoBalance.Checked != _state.Instances[ArrayID].AutoBalance ||
                cb_ffWarning.Checked != _state.Instances[ArrayID].FriendlyFireWarning ||
                cb_Tracers.Checked != _state.Instances[ArrayID].ShowTracers ||
                cb_AutoRange.Checked != _state.Instances[ArrayID].AllowAutoRange)
            {
                updateList.Add("-- Game Play Options --");
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateGamePlayOptions" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "FriendlyFire", checkBox40.Checked },
                    { "FriendlyTags", cb_showFriendTags.Checked },
                    { "ShowTeamClays", cb_TeamClays.Checked },
                    { "AutoBalance", cb_autoBalance.Checked },
                    { "FriendlyFireWarning", cb_ffWarning.Checked },
                    { "ShowTracers", cb_Tracers.Checked },
                    { "AllowAutoRange", cb_AutoRange.Checked }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                {
                    if (checkBox40.Checked != _state.Instances[ArrayID].FriendlyFire)
                        updateList.Add(" - Friendly Fire Updated");
                    if (cb_showFriendTags.Checked != _state.Instances[ArrayID].FriendlyTags)
                        updateList.Add(" - Friendly Tags Updated");
                    if (cb_TeamClays.Checked != _state.Instances[ArrayID].ShowTeamClays)
                        updateList.Add(" - Show Team Clays Updated");
                    if (cb_autoBalance.Checked != _state.Instances[ArrayID].AutoBalance)
                        updateList.Add(" - Auto Balance Updated");
                    if (cb_ffWarning.Checked != _state.Instances[ArrayID].FriendlyFireWarning)
                        updateList.Add(" - Friendly Fire Warning Updated");
                    if (cb_Tracers.Checked != _state.Instances[ArrayID].ShowTracers)
                        updateList.Add(" - Show Tracers Updated");
                    if (cb_AutoRange.Checked != _state.Instances[ArrayID].AllowAutoRange)
                        updateList.Add(" - Allow Auto Range Updated");
                }
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                {
                    updateList.Add("An error occurred while updating Game Play Options.");
                }
            }

            if (cb_minPing.Checked != _state.Instances[ArrayID].MinPing ||
                Convert.ToInt32(num_minPing.Value) != _state.Instances[ArrayID].MinPingValue)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateMinPing" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "EnablePing", cb_minPing.Checked },
                    { "PingValue", Convert.ToInt32(num_minPing.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Min Ping Settings have been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Min Ping Settings.");
            }

            if (cb_maxPing.Checked != _state.Instances[ArrayID].MaxPing ||
                Convert.ToInt32(num_maxPing.Value) != _state.Instances[ArrayID].MaxPingValue)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateMaxPing" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "EnablePing", cb_minPing.Checked },
                    { "PingValue", Convert.ToInt32(num_minPing.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Max Ping Settings have been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Weapon Restrictions.");
            }

            if (cbl_weaponSelection.GetItemChecked(0) != _state.Instances[ArrayID].WeaponRestrictions.WPN_COLT45 ||
                cbl_weaponSelection.GetItemChecked(1) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M9BERETTA ||
                cbl_weaponSelection.GetItemChecked(2) !=
                _state.Instances[ArrayID].WeaponRestrictions.WPN_REMMINGTONSG ||
                cbl_weaponSelection.GetItemChecked(3) != _state.Instances[ArrayID].WeaponRestrictions.WPN_CAR15 ||
                cbl_weaponSelection.GetItemChecked(4) != _state.Instances[ArrayID].WeaponRestrictions.WPN_CAR15_203 ||
                cbl_weaponSelection.GetItemChecked(5) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M16_BURST ||
                cbl_weaponSelection.GetItemChecked(6) !=
                _state.Instances[ArrayID].WeaponRestrictions.WPN_M16_BURST_203 ||
                cbl_weaponSelection.GetItemChecked(7) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M21 ||
                cbl_weaponSelection.GetItemChecked(8) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M24 ||
                cbl_weaponSelection.GetItemChecked(9) !=
                _state.Instances[ArrayID].WeaponRestrictions.WPN_MCRT_300_TACTICAL ||
                cbl_weaponSelection.GetItemChecked(10) != _state.Instances[ArrayID].WeaponRestrictions.WPN_BARRETT ||
                cbl_weaponSelection.GetItemChecked(11) != _state.Instances[ArrayID].WeaponRestrictions.WPN_SAW ||
                cbl_weaponSelection.GetItemChecked(12) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M60 ||
                cbl_weaponSelection.GetItemChecked(13) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M240 ||
                cbl_weaponSelection.GetItemChecked(14) != _state.Instances[ArrayID].WeaponRestrictions.WPN_MP5 ||
                cbl_weaponSelection.GetItemChecked(15) != _state.Instances[ArrayID].WeaponRestrictions.WPN_G3 ||
                cbl_weaponSelection.GetItemChecked(16) != _state.Instances[ArrayID].WeaponRestrictions.WPN_G36 ||
                cbl_weaponSelection.GetItemChecked(17) != _state.Instances[ArrayID].WeaponRestrictions.WPN_PSG1 ||
                cbl_weaponSelection.GetItemChecked(18) != _state.Instances[ArrayID].WeaponRestrictions.WPN_XM84_STUN ||
                cbl_weaponSelection.GetItemChecked(19) != _state.Instances[ArrayID].WeaponRestrictions.WPN_M67_FRAG ||
                cbl_weaponSelection.GetItemChecked(20) !=
                _state.Instances[ArrayID].WeaponRestrictions.WPN_AN_M8_SMOKE ||
                cbl_weaponSelection.GetItemChecked(21) !=
                _state.Instances[ArrayID].WeaponRestrictions.WPN_SATCHEL_CHARGE ||
                cbl_weaponSelection.GetItemChecked(22) != _state.Instances[ArrayID].WeaponRestrictions.WPN_CLAYMORE ||
                cbl_weaponSelection.GetItemChecked(23) != _state.Instances[ArrayID].WeaponRestrictions.WPN_AT4)
            {
                var weaponsClass = new WeaponsClass
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
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateWeapons" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "weapons", weaponsClass }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Weapon Restrictions have been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Weapon Restrictions.");
            }

            if (num_pspTimer.Value != _state.Instances[ArrayID].PSPTakeOverTime)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdatePSPTime" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "PSPTime", Convert.ToInt32(num_pspTimer.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("PSP Time has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating PSP Time.");
            }

            if (num_flagReturn.Value != _state.Instances[ArrayID].FlagReturnTime)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateFlagReturnTime" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "FlagReturnTime", Convert.ToInt32(num_flagReturn.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Flag Return Time has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Flag Return Time.");
            }

            if (num_MaxTeamLives.Value != _state.Instances[ArrayID].MaxTeamLives)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateMaxTeamLives" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "MaxTeamLives", Convert.ToInt32(num_MaxTeamLives.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Max Team Lives has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Max Team Lives.");
            }

            if (num_maxFriendKills.Value != _state.Instances[ArrayID].FriendlyFireKills)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "SessionID", RCSetup.SessionID },
                    { "action", "BMTRC.UpdateFriendlyFireKills" },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "FriendlyFireKills", Convert.ToInt32(num_maxFriendKills.Value) }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    updateList.Add("Friendly Fire Kills has been updated.");
                else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                    updateList.Add("An error occurred while updating Friendly Fire Kills.");
            }

            // do not touch please...
            if (updateList.Count != 0)
            {
                var alertMsg = string.Empty;
                foreach (var entry in updateList) alertMsg += entry + "\n";
                MessageBox.Show(alertMsg, "Results");
            }
            else
            {
                MessageBox.Show("Nothing changed.\nNo updates sent.", "Success");
            }
        }

        private void settingsRevertChanges_click(object sender, EventArgs e)
        {
            /* Settings Revert All Changes, Called by btn_settingsRevertChanges */

            // revert all changes
            // vpn settings
            checkBox_vpn_disallow.Checked = _state.Instances[ArrayID].enableVPNCheck;
            value_vpn_abuselevel.Value = _state.IPQualityScore[ArrayID].WarnLevel;

            // server settings
            rcT_serverName.Text = _state.Instances[ArrayID].ServerName;
            rcCB_sessionType.SelectedIndex = _state.Instances[ArrayID].SessionType;
            rcCB_country.SelectedItem = _state.Instances[ArrayID].CountryCode;
            rcNum_maxSlots.Value = _state.Instances[ArrayID].MaxSlots;
            rcT_serverPassword.Text = _state.Instances[ArrayID].Password;
            cb_timeLimit.SelectedIndex = _state.Instances[ArrayID].TimeLimit;
            cb_startDelay.SelectedIndex = _state.Instances[ArrayID].StartDelay;
            cb_replayMaps.SelectedIndex = _state.Instances[ArrayID].LoopMaps;
            richTextBox1.Text = _state.Instances[ArrayID].MOTD;
            cb_gameDedicated.Checked = Convert.ToBoolean(_state.Instances[ArrayID].Dedicated);
            cb_requireNova.Checked = _state.Instances[ArrayID].RequireNovaLogin;
            cb_customSkin.Checked = _state.Instances[ArrayID].AllowCustomSkins;
            cb_autoBalance.Checked = _state.Instances[ArrayID].AutoBalance;
            text_bluePass.Text = _state.Instances[ArrayID].BluePassword;
            text_redPass.Text = _state.Instances[ArrayID].RedPassword;
            cb_respawnTime.SelectedIndex = _state.Instances[ArrayID].RespawnTime;
            checkBox40.Checked = _state.Instances[ArrayID].FriendlyFire;
            num_maxFriendKills.Value = _state.Instances[ArrayID].FriendlyFireKills;
            cb_showFriendTags.Checked = _state.Instances[ArrayID].FriendlyTags;
            cb_ffWarning.Checked = _state.Instances[ArrayID].FriendlyFireWarning;
            cb_Tracers.Checked = _state.Instances[ArrayID].ShowTracers;
            cb_TeamClays.Checked = _state.Instances[ArrayID].ShowTeamClays;
            cb_AutoRange.Checked = _state.Instances[ArrayID].AllowAutoRange;
            num_pspTimer.Value = _state.Instances[ArrayID].PSPTakeOverTime;
            num_flagReturn.Value = _state.Instances[ArrayID].FlagReturnTime;
            num_MaxTeamLives.Value = _state.Instances[ArrayID].MaxTeamLives;
            cb_minPing.Checked = _state.Instances[ArrayID].MinPing;
            num_minPing.Value = _state.Instances[ArrayID].MinPingValue;
            cb_maxPing.Checked = _state.Instances[ArrayID].MaxPing;
            num_maxPing.Value = _state.Instances[ArrayID].MaxPingValue;
            cb_oneShotKills.Checked = _state.Instances[ArrayID].OneShotKills;
            cb_fatBullets.Checked = _state.Instances[ArrayID].FatBullets;
            cb_destroyBuildings.Checked = _state.Instances[ArrayID].DestroyBuildings;
            cbl_weaponSelection.SetItemChecked(0, _state.Instances[ArrayID].WeaponRestrictions.WPN_COLT45);
            cbl_weaponSelection.SetItemChecked(1, _state.Instances[ArrayID].WeaponRestrictions.WPN_M9BERETTA);
            cbl_weaponSelection.SetItemChecked(2, _state.Instances[ArrayID].WeaponRestrictions.WPN_REMMINGTONSG);
            cbl_weaponSelection.SetItemChecked(3, _state.Instances[ArrayID].WeaponRestrictions.WPN_CAR15);
            cbl_weaponSelection.SetItemChecked(4, _state.Instances[ArrayID].WeaponRestrictions.WPN_CAR15_203);
            cbl_weaponSelection.SetItemChecked(5, _state.Instances[ArrayID].WeaponRestrictions.WPN_M16_BURST);
            cbl_weaponSelection.SetItemChecked(6, _state.Instances[ArrayID].WeaponRestrictions.WPN_M16_BURST_203);
            cbl_weaponSelection.SetItemChecked(7, _state.Instances[ArrayID].WeaponRestrictions.WPN_M21);
            cbl_weaponSelection.SetItemChecked(8, _state.Instances[ArrayID].WeaponRestrictions.WPN_M24);
            cbl_weaponSelection.SetItemChecked(9, _state.Instances[ArrayID].WeaponRestrictions.WPN_MCRT_300_TACTICAL);
            cbl_weaponSelection.SetItemChecked(10, _state.Instances[ArrayID].WeaponRestrictions.WPN_BARRETT);
            cbl_weaponSelection.SetItemChecked(11, _state.Instances[ArrayID].WeaponRestrictions.WPN_SAW);
            cbl_weaponSelection.SetItemChecked(12, _state.Instances[ArrayID].WeaponRestrictions.WPN_M60);
            cbl_weaponSelection.SetItemChecked(13, _state.Instances[ArrayID].WeaponRestrictions.WPN_M240);
            cbl_weaponSelection.SetItemChecked(14, _state.Instances[ArrayID].WeaponRestrictions.WPN_MP5);
            cbl_weaponSelection.SetItemChecked(15, _state.Instances[ArrayID].WeaponRestrictions.WPN_G3);
            cbl_weaponSelection.SetItemChecked(16, _state.Instances[ArrayID].WeaponRestrictions.WPN_G36);
            cbl_weaponSelection.SetItemChecked(17, _state.Instances[ArrayID].WeaponRestrictions.WPN_PSG1);
            cbl_weaponSelection.SetItemChecked(18, _state.Instances[ArrayID].WeaponRestrictions.WPN_XM84_STUN);
            cbl_weaponSelection.SetItemChecked(19, _state.Instances[ArrayID].WeaponRestrictions.WPN_M67_FRAG);
            cbl_weaponSelection.SetItemChecked(20, _state.Instances[ArrayID].WeaponRestrictions.WPN_AN_M8_SMOKE);
            cbl_weaponSelection.SetItemChecked(11, _state.Instances[ArrayID].WeaponRestrictions.WPN_SATCHEL_CHARGE);
            cbl_weaponSelection.SetItemChecked(12, _state.Instances[ArrayID].WeaponRestrictions.WPN_CLAYMORE);
            cbl_weaponSelection.SetItemChecked(13, _state.Instances[ArrayID].WeaponRestrictions.WPN_AT4);
        }

        /* Server Manager: Maps Tab
         *
         */
        private void mapSettingsGameType_changed(object sender, EventArgs e)
        {
            /* Map Settings Game Type Changed, Called by dropDown_mapSettingsGameType */
            var gameTypeId = -1;
            foreach (var gametype in _state.autoRes.gameTypes)
                if (gametype.Value.Name == dropDown_mapSettingsGameType.SelectedItem.ToString())
                {
                    gameTypeId = gametype.Value.DatabaseId;
                    break;
                }

            if (gameTypeId == -1) throw new Exception("FUCK ME UP THE ASSHOLE THIS SHOULD BE WORKING.");
            availableMapList = new Dictionary<int, MapList>();
            listBox_mapsAvailable.Items.Clear();
            foreach (var avMap in _state.Instances[ArrayID].availableMaps)
                if (avMap.Value.GameTypes.Contains(gameTypeId))
                {
                    listBox_mapsAvailable.Items.Add(avMap.Value.MapName + " <" + avMap.Value.MapFile + ">");
                    availableMapList.Add(availableMapList.Count, avMap.Value);
                }

            label_numMapsAvailable.Text = listBox_mapsAvailable.Items.Count.ToString();
        }

        private void mapsAvailable_DoubleClick(object sender, EventArgs e)
        {
            /* Add Map from Available List to Rotation List, Called by listBox_mapsAvailable double click event */
            if (listBox_mapsAvailable.SelectedIndex == -1) return;

            if (list_mapRotation.Items.Count == 128)
            {
                MessageBox.Show(
                    "Due to limitations set by NovaLogic,\nwhen starting a server you are only allowed to choose 128 maps.\nYou can add more maps, after the server has been started\nby using the Server Manager.",
                    "Map List Error");
                return;
            }

            string maptype;
            switch (dropDown_mapSettingsGameType.SelectedIndex)
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

            var maptypeList = "";
            foreach (var gametype in _state.autoRes.gameTypes)
                if (dropDown_mapSettingsGameType.SelectedIndex == gametype.Value.DatabaseId)
                {
                    maptypeList = gametype.Key;
                    break;
                }

            selectedMaps.Add(new MapList
            {
                MapFile = availableMapList[listBox_mapsAvailable.SelectedIndex].MapFile,
                MapName = availableMapList[listBox_mapsAvailable.SelectedIndex].MapName,
                CustomMap = availableMapList[listBox_mapsAvailable.SelectedIndex].CustomMap,
                GameType = maptypeList
            });

            list_mapRotation.Items.Add(maptype + listBox_mapsAvailable.SelectedItem);
            MapListEdited = true;
            label_currentMapCount.Text = list_mapRotation.Items.Count + " / 128";
        }

        private void mapRotation_DoubleClick(object sender, EventArgs e)
        {
            /* Remove Map from Rotation List, Called by listBox_mapRotation double click event */
            if (list_mapRotation.SelectedIndex == -1) return;

            selectedMaps.RemoveAt(list_mapRotation.SelectedIndex);
            list_mapRotation.Items.RemoveAt(list_mapRotation.SelectedIndex);
            MapListEdited = true;
            label_currentMapCount.Text = list_mapRotation.Items.Count + " / 128";
        }

        private void playerListAction_clickDisarm(object sender, EventArgs e)
        {
            // disarm player
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            if (!_state.Instances[ArrayID].DisarmPlayers.Contains(selectedPlayerSlot))
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "action", "BMTRC.DisarmPlayer" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "slot", selectedPlayerSlot }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    MessageBox.Show("Player has been disarmed!", "Success");
                else
                    MessageBox.Show("BMTTV reported an error. Please try again later.");
            }
            else
            {
                MessageBox.Show("Player has already been disarmed!", "Error");
            }
        }

        private void playerListAction_clickRearm(object sender, EventArgs e)
        {
            // rearm player
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            if (_state.Instances[ArrayID].DisarmPlayers.Contains(selectedPlayerSlot))
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "action", "BMTRC.RearmPlayer" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "slot", selectedPlayerSlot }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    MessageBox.Show("Player has been Armed!", "Success");
                else
                    MessageBox.Show("BMTTV reported an error. Please try again later.");
            }
            else
            {
                MessageBox.Show("Player has already been Rearmed!", "Error");
            }
        }

        private void playerListAction_clickKill(object sender, EventArgs e)
        {
            // kill player
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            var killPlayer =
                MessageBox.Show(
                    "Are you sure you want to kill: " + _state.Instances[ArrayID].PlayerList[selectedPlayerSlot].name +
                    " ?", "Important!", MessageBoxButtons.YesNo);
            if (killPlayer == DialogResult.Yes)
            {
                var request = new Dictionary<string, dynamic>
                {
                    { "action", "BMTRC.KillPlayer" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "slot", selectedPlayerSlot }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    MessageBox.Show("Player has been killed!", "Success");
                else
                    MessageBox.Show("BMTTV reported an error. Please try again later.");
            }
        }

        private void playerListAction_clickChangeTeams(object sender, EventArgs e)
        {
            // change player team
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.ChangePlayerTeam" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", selectedPlayerSlot }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                MessageBox.Show("Player will be switched after the current map.", "Success");
            else
                MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void textBox_banSearch_changed(object sender, EventArgs e)
        {
            var search = (TextBox)sender;
            if (search.Text == string.Empty || string.IsNullOrEmpty(search.Text) ||
                string.IsNullOrWhiteSpace(search.Text))
            {
                grid_bannedPlayerList.DataSource = bannedTable;
            }
            else
            {
                if (IPAddress.TryParse(search.Text, out var address))
                {
                    // search for address since we have a valid address...
                    var searchBans = _state.Instances[ArrayID].BanList.Where(x => x.ipaddress == address.ToString())
                        .ToList();
                    searchBannedTable = new DataTable();
                    searchBannedTable.Columns.Add("Name");
                    searchBannedTable.Columns.Add("IP Address");
                    searchBannedTable.Columns.Add("Time Remaining");
                    foreach (var ban in searchBans)
                    {
                        var newRow = searchBannedTable.NewRow();
                        newRow["Name"] = ban.player;
                        newRow["IP Address"] = ban.ipaddress;
                        if (ban.expires == "-1")
                            newRow["Time Remaining"] = "Permanent";
                        else
                            newRow["Time Remaining"] = ban.expires;
                        searchBannedTable.Rows.Add(newRow);
                    }

                    grid_bannedPlayerList.DataSource = searchBannedTable;
                }
                else
                {
                    var searchBans = _state.Instances[ArrayID].BanList.Where(x => x.player == search.Text).ToList();
                    searchBannedTable = new DataTable();
                    searchBannedTable.Columns.Add("Name");
                    searchBannedTable.Columns.Add("IP Address");
                    searchBannedTable.Columns.Add("Time Remaining");
                    foreach (var ban in searchBans)
                    {
                        var newRow = searchBannedTable.NewRow();
                        newRow["Name"] = ban.player;
                        newRow["IP Address"] = ban.ipaddress;
                        if (ban.expires == "-1")
                            newRow["Time Remaining"] = "Permanent";
                        else
                            newRow["Time Remaining"] = ban.expires;
                        searchBannedTable.Rows.Add(newRow);
                    }

                    grid_bannedPlayerList.DataSource = searchBannedTable;
                }
            }
        }

        private void mapAction_clickScore(object sender, EventArgs e)
        {
            // score map
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.ScoreMap" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                MessageBox.Show("Command sent!", "Success");
            else
                MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void mapAction_clickUpdateActiveMaps(object sender, EventArgs e)
        {
            MapListEdited = false;
            // update maps
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.UpdateMapCycle" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "MapCycle", selectedMaps }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                MessageBox.Show("Map List Updated!", "Success");
            else
                MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void mapAction_clickPlayMapNext(object sender, EventArgs e)
        {
            if (MapListEdited)
            {
                MessageBox.Show("You MUST update the maplist before setting the next map.", "Error");
                return;
            }

            if (_state.Instances[ArrayID].Status != InstanceStatus.ONLINE)
            {
                MessageBox.Show("Please wait for the server to be ready. Please try again later.", "Error");
                return;
            }

            // set next map
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.SetNextMap" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", list_mapRotation.SelectedIndex }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                MessageBox.Show("Command sent!", "Success");
            else
                MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void mapAction_clickShuffleMaps(object sender, EventArgs e)
        {
            MapListEdited = true;
            list_mapRotation.Items.Clear();
            selectedMaps = _state.autoRes.ShuffleSelectedMapList(selectedMaps);
            foreach (var item in selectedMaps)
            {
                list_mapRotation.Items.Add("|" + item.GameType + "| " + item.MapName + " " + "<" + item.MapFile + ">");
                selectedMaps.Add(item);
            }
            // shuffle
        }

        private void mapAction_MoveMapEntry(object sender, EventArgs e)
        {
            var direction = 0;

            if (sender is Button clickedButton)
            {
                if (clickedButton.Text == "Up")
                    direction = -1;
                else
                    direction = 1;
            }

            if (list_mapRotation.SelectedItem == null ||
                list_mapRotation.SelectedIndex <
                0) return; // we haven't seleccted a message so it will return a -1 or NULL value.

            var newIndex = list_mapRotation.SelectedIndex + direction;

            // setup array bounds so we don't fuck up again...
            if (newIndex < 0 ||
                newIndex >= list_mapRotation.Items.Count) return; // Index out of range - nothing to do...

            MapListEdited = true;
            // remove item then re-add at the correct position
            var selected = list_mapRotation.SelectedItem;
            var mapEntry = selectedMaps[list_mapRotation.SelectedIndex];
            selectedMaps.RemoveAt(list_mapRotation.SelectedIndex);
            list_mapRotation.Items.Remove(selected);
            selectedMaps.Insert(newIndex, mapEntry);
            list_mapRotation.Items.Insert(newIndex, selected);
            list_mapRotation.SetSelected(newIndex, true);
            _state.Instances[ArrayID].previousMapList = new Dictionary<int, MapList>();

            foreach (var map in _state.Instances[ArrayID].MapList)
                _state.Instances[ArrayID].previousMapList
                    .Add(_state.Instances[ArrayID].previousMapList.Count, map.Value);
            _state.Instances[ArrayID].MapList = new Dictionary<int, MapList>();
            foreach (var selectedMapListEntry in selectedMaps)
                _state.Instances[ArrayID].MapList.Add(_state.Instances[ArrayID].MapList.Count, selectedMapListEntry);
        }

        private void mapAction_clickSaveRotation(object sender, EventArgs e)
        {
            // save roation
            if (MapListEdited)
            {
                MessageBox.Show("You MUST update the maplist before saving the rotation.", "Error");
                return;
            }

            var popup_SaveRotation = new RC_PopupSaveRotation(_state, RCSetup, ArrayID, ref selectedMaps);
            popup_SaveRotation.ShowDialog();
        }

        private void mapAction_clickLoadRotation(object sender, EventArgs e)
        {
            // load rotation
            var popup_Load = new RC_PopupLoadRotation(_state, RCSetup, ArrayID);
            popup_Load.ShowDialog();
            if (loadList.Count > 0) MapListEdited = true;
            if (MapListEdited)
            {
                list_mapRotation.Items.Clear();
                selectedMaps.Clear();
                foreach (var item in loadList)
                {
                    selectedMaps.Add(item);
                    list_mapRotation.Items.Add("|" + item.GameType + "| " + item.MapName + " <" + item.MapFile + ">");
                }

                label_currentMapCount.Text = loadList.Count + " / 128";
                MessageBox.Show("Map rotation has been loaded!\nPlease click update maps to update the server.",
                    "Success");
            }
        }

        private void autoMessages_clickAdd(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(text_newAutoMessage.Text) || string.IsNullOrWhiteSpace(text_newAutoMessage.Text))
            {
                MessageBox.Show("Please enter a valid message.", "Error");
                return;
            }

            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.AddAutoMsg" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "newMsg", text_newAutoMessage.Text }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                listBox_AutoMessages.Items.Add(text_newAutoMessage.Text);
                text_newAutoMessage.Text = string.Empty;
                MessageBox.Show("Message added successfully!", "Success");
            }
            else
            {
                MessageBox.Show("BMTTV reported an error. Please try again later.");
            }
        }

        private void autoMessages_doubleClickDelete(object sender, EventArgs e)
        {
            if (listBox_AutoMessages.SelectedIndex == -1) return;
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.DeleteAutoMsg" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", listBox_AutoMessages.SelectedIndex }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                listBox_AutoMessages.Items.Remove(listBox_AutoMessages.Items[listBox_AutoMessages.SelectedIndex]);
                MessageBox.Show("Message removed successfully!", "Success");
            }
            else
            {
                MessageBox.Show("BMTTV reported an error. Please try again later.");
            }
        }

        private void autoMessage_intervalChange(object sender, EventArgs e)
        {
            // change interval for automsg
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.ChangeAutoMsgInterval" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "interval", Convert.ToInt32(num_autoMsgInterval.Value) }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                return;
            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void banList_clickRemove(object sender, EventArgs e)
        {
            // remove ban
            if (grid_bannedPlayerList.CurrentCell == null) return; // no ban selected
            DataRow playerInfo;
            playerInfo = bannedTable.Rows[grid_bannedPlayerList.CurrentCell.RowIndex];
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.RemoveBan" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "PlayerName", playerInfo["Name"].ToString() },
                { "PlayerIP", playerInfo["IP Address"].ToString() }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                bannedTable.Rows.Remove(playerInfo);

                MessageBox.Show("Ban has been successfully removed!", "Success");
                return;
            }

            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void vpn_changeWarnLevel(object sender, EventArgs e)
        {
            // change abuse level
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.UpdateWarnLevel" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "WarnLevel", Convert.ToInt32(value_vpn_abuselevel.Value) }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                return;
            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void vpn_changeDisallowVPN(object sender, EventArgs e)
        {
            // disallow vpn checkbox
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.DisallowVPN" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "DisallowVPN", checkBox_vpn_disallow.Checked }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                return;
            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void banList_addEntry(object sender, EventArgs e)
        {
            // add ban

            if (string.IsNullOrEmpty(text_adPlayerName.Text) || string.IsNullOrWhiteSpace(text_adPlayerName.Text))
            {
                MessageBox.Show("Invalid Player Name", "Error");
                return;
            }

            if (!IPAddress.TryParse(text_abIPAddress.Text, out var playerIP))
            {
                MessageBox.Show("Please enter a valid IP Address", "Error");
                return;
            }

            if (string.IsNullOrEmpty(combo_abReason.Text) || string.IsNullOrWhiteSpace(combo_abReason.Text) ||
                combo_abReason.Text == "Select One or Enter Custom")
            {
                MessageBox.Show("Invalid reason", "Error");
                return;
            }

            try
            {
                var request = new Dictionary<dynamic, dynamic>
                {
                    { "action", "BMTRC.AddBan" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "PlayerName", text_adPlayerName.Text },
                    { "banReason", combo_abReason.Text },
                    { "PlayerIP", playerIP.ToString() },
                    { "AddBanExpires", "-1" }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                    MessageBox.Show($"Player has been banned successfully. Reason: " + combo_abReason.Text, "Success");
                else
                    MessageBox.Show("Server has reported an error." + (OpenClass.Status)response["Status"]);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("HawkSync_RC", ex.ToString(), EventLogEntryType.Error);
                MessageBox.Show("Connection timed out while trying to send " + "BMTRC.AddBan" + " command.");
                return;
            }

            // Reset Fields on Success
            text_adPlayerName.Text = string.Empty;
            text_abIPAddress.Text = string.Empty;
            combo_abReason.Text = "Select Reason or Enter Custom";

        }

        private void vpnList_addEntry(object sender, EventArgs e)
        {
            // add vpn whitelist
            if (IPAddress.TryParse(textBox_vpnAddress.Text, out var PlayerAddress))
            {
                if (string.IsNullOrEmpty(textBox_vpnDescription.Text) ||
                    string.IsNullOrWhiteSpace(textBox_vpnDescription.Text))
                {
                    MessageBox.Show("Please enter a valid description!", "Invalid Entry");
                    return;
                }

                var request = new Dictionary<string, dynamic>
                {
                    { "action", "BMTRC.AddVPN" },
                    { "SessionID", RCSetup.SessionID },
                    { "serverID", _state.Instances[ArrayID].Id },
                    { "Description", textBox_vpnDescription.Text },
                    { "PlayerIP", PlayerAddress.ToString() }
                };
                var response =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
                if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                {
                    MessageBox.Show("IP has been added to the whitelist successfully!", "Success");
                    return;
                }

                MessageBox.Show("BMTTV reported an error. Please try again later.");
            }
            else
            {
                MessageBox.Show("Please enter a valid IP Address!", "Invalid Entry");
            }
        }

        private void playerList_actionActivateGodMode(object sender, EventArgs e)
        {
            // activate god mode
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            if (_state.Instances[ArrayID].GodModeList.Contains(selectedPlayerSlot))
            {
                MessageBox.Show("The player is already in God Mode!", "Error");
                return;
            }

            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.EnableGodMode" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", selectedPlayerSlot }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                MessageBox.Show("God Mode has been activated successfully!", "Success");
                return;
            }

            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void playerList_actionDeactivateGodMode(object sender, EventArgs e)
        {
            // deactivate god mode
            var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
            if (!_state.Instances[ArrayID].GodModeList.Contains(selectedPlayerSlot))
            {
                MessageBox.Show("Player is NOT in God Mode!", "Error");
                return;
            }

            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.DisableGodMode" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "slot", selectedPlayerSlot }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                MessageBox.Show("God Mode has been deactivated successfully!", "Success");
                return;
            }

            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void chat_SendMsg(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(chat_textBoxMsg.Text) || string.IsNullOrWhiteSpace(chat_textBoxMsg.Text))
            {
                MessageBox.Show("Please enter a valid message!", "Error");
                return;
            }

            var request = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.SendMsg" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "newMsg", chat_textBoxMsg.Text },
                { "slot", chat_channelSelection.SelectedIndex }
            };

            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                chat_textBoxMsg.Text = string.Empty;
                MessageBox.Show("Message has been sent!", "Success");
            }
            else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("Something went wrong.", "Error");
            }
        }

        private void server_enforceMinPing(object sender, EventArgs e)
        {
            num_minPing.Enabled = cb_minPing.Checked;
        }

        private void server_enforceMaxPing(object sender, EventArgs e)
        {
            num_maxPing.Enabled = cb_maxPing.Checked;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var toolTipText = string.Empty;
            switch (endOfMapTimer_TrackBar.Value)
            {
                case 0:
                    toolTipText = "Instantly Skip";
                    break;
                case 46:
                    toolTipText = "Let BHD Decide";
                    break;
                default:
                    toolTipText = endOfMapTimer_TrackBar.Value + " Secs";
                    break;
            }

            var request = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.UpdateScoreboardDelay" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "ScoreboardDelay", Convert.ToInt32(endOfMapTimer_TrackBar.Value) }
            };

            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                toolTipText += "\nServer updated successfully.";
                toolTip1.Show(toolTipText, endOfMapTimer_TrackBar);
            }
            else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
            {
                toolTipText += "\nFailed to update the server.";
                toolTip1.Show(toolTipText, endOfMapTimer_TrackBar);
            }
        }

        private void trackBar1_MouseHover(object sender, EventArgs e)
        {
            var toolTipText = string.Empty;
            switch (endOfMapTimer_TrackBar.Value)
            {
                case 0:
                    toolTipText = "Instantly Skip";
                    break;
                case 46:
                    toolTipText = "Let BHD Decide";
                    break;
                default:
                    toolTipText = endOfMapTimer_TrackBar.Value + " Secs";
                    break;
            }

            toolTip1.Show(toolTipText, endOfMapTimer_TrackBar);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            availableMapList = new Dictionary<int, MapList>();
            list_mapRotation.Items.Clear();
            foreach (var map in _state.Instances[ArrayID].MapList)
            {
                list_mapRotation.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " <" +
                                           map.Value.MapFile + ">");
                availableMapList.Add(availableMapList.Count, map.Value);
            }
        }

        private void banList_SelectionChanged(object sender, EventArgs e)
        {
            if (grid_bannedPlayerList.CurrentCell == null)
            {
                value_bdPlayerName.Text = "";
                value_banReason.Text = "";
                value_banDateAdded.Text = "";
                value_banAdmin.Text = "";
                value_banIPAddress.Text = "";
                return;
            }

            if (_state.Instances[ArrayID].BanList.Count == 0)
            {
                value_bdPlayerName.Text = "";
                value_banReason.Text = "";
                value_banDateAdded.Text = "";
                value_banAdmin.Text = "";
                value_banIPAddress.Text = "";
                return;
            }

            value_bdPlayerName.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentCell.RowIndex].player;
            value_banReason.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentCell.RowIndex].reason;
            value_banDateAdded.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentCell.RowIndex].addedDate.ToString();
            value_banIPAddress.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentCell.RowIndex].ipaddress;
            value_banAdmin.Text = _state.Instances[ArrayID].BanList[grid_bannedPlayerList.CurrentCell.RowIndex].bannedBy;
        }

        private void playerList_doubleClickPlayerInfo(object sender, EventArgs e)
        {
            var numPlayers = _state.Instances[ArrayID].NumPlayers;
            if (numPlayers == 0) return;
            var playerInfo = new PlayerInfo(_state, RCSetup, ArrayID, Convert.ToInt32(grid_playerList.SelectedCells[0].Value));
            playerInfo.ShowDialog();
        }

        private void chat_btnClickChannelAll(object sender, EventArgs e)
        {
            ChatLogMessages.RemoveFilter();
        }

        private void chat_btnClickChannelGlobal(object sender, EventArgs e)
        {
            ChatLogMessages.RemoveFilter();
            ChatLogMessages.ApplyFilter(delegate(PlayerChatLog chatLog)
            {
                return chatLog.msgType == "Global" || chatLog.team == "Host";
            });
        }

        private void chat_btnClickChannelRed(object sender, EventArgs e)
        {
            ChatLogMessages.RemoveFilter();
            ChatLogMessages.ApplyFilter(delegate(PlayerChatLog chatLog)
            {
                return chatLog.msgType == "Team" && (chatLog.team == "Red" || chatLog.team == "Host");
            });
        }

        private void chat_btnClickChannelBlue(object sender, EventArgs e)
        {
            ChatLogMessages.RemoveFilter();
            ChatLogMessages.ApplyFilter(delegate(PlayerChatLog chatLog)
            {
                return chatLog.msgType == "Team" && (chatLog.team == "Blue" || chatLog.team == "Host");
            });
        }

        private void chat_btnClickChannelPlayer(object sender, EventArgs e)
        {
            ChatLogMessages.RemoveFilter();
            cb_chatPlayerSelect.Enabled = true;
            cb_chatPlayerSelect.Items.Clear();
            cb_chatPlayerSelect.Items.Add("Select Player");
            cb_chatPlayerSelect.SelectedIndex = 0;

            _state.Instances[ArrayID].PlayerList.Keys.ToList().ForEach(delegate(int slot)
            {
                cb_chatPlayerSelect.Items.Add(_state.Instances[ArrayID].PlayerList[slot].name);
            });
        }

        private void chat_dropDownPlayerNameChanged(object sender, EventArgs e)
        {
            ChatLogMessages.RemoveFilter();
            ChatLogMessages.ApplyFilter(delegate(PlayerChatLog chatLog)
            {
                return chatLog.PlayerName == cb_chatPlayerSelect.SelectedItem.ToString();
            });
        }

        private void actionSpectate_click(object sender, EventArgs e)
        {
            if (RCSetup.SpectateTimer.Enabled == false)
            {
                if (RCSetup.GamePID != -1)
                {
                    // get selected Player Slot
                    var selectedPlayerSlot = Convert.ToInt32(grid_playerList.SelectedCells[0].Value);
                    var selectedPlayer = _state.Instances[ArrayID].PlayerList[selectedPlayerSlot];
                    if (!RCSetup.playerAddresses.ContainsKey(selectedPlayer.name))
                    {
                        MessageBox.Show("Could not detect player Address. #31", "Error");
                        return;
                    }

                    RCSetup.tempSpectateAddress = RCSetup.HostAddr + 0x1C6C4;
                    RCSetup.spectateName = selectedPlayer.name;

                    var playerData = new byte[668];
                    var playerDataRead = 0;
                    var playerDataWritten = 0;
                    ReadProcessMemory((int)RCSetup.GameHandle,
                        RCSetup.playerAddresses[selectedPlayer.name].memoryAddress, playerData, playerData.Length,
                        ref playerDataRead);
                    WriteProcessMemory((int)RCSetup.GameHandle, RCSetup.tempSpectateAddress, playerData,
                        playerData.Length, ref playerDataWritten);
                    var addrBytes = BitConverter.GetBytes(RCSetup.tempSpectateAddress);
                    var addrWritten = 0;
                    WriteProcessMemory((int)RCSetup.GameHandle, 0x007F2DD4, addrBytes, addrBytes.Length,
                        ref addrWritten);
                    RCSetup.spectateAddress = RCSetup.playerAddresses[selectedPlayer.name].memoryAddress;
                    playerListMenu_spectate.Text = "Stop Spectating";
                    RCSetup.SpectateTimer.Enabled = true;
                    RCSetup.SpectateTimer.Start();
                }
                else
                {
                    MessageBox.Show(
                        "Could not attach to local game. Either the game is not running, or something went wrong.",
                        "Error");
                }
            }
            else
            {
                var restoreAddrBytes = BitConverter.GetBytes(RCSetup.myPlayerAddress);
                var restoreAddrWritten = 0;
                WriteProcessMemory((int)RCSetup.GameHandle, 0x007F2DD4, restoreAddrBytes, restoreAddrBytes.Length,
                    ref restoreAddrWritten);
                var blankData = new byte[668];
                var blankDataWritten = 0;
                WriteProcessMemory((int)RCSetup.GameHandle, RCSetup.tempSpectateAddress, blankData, blankData.Length,
                    ref blankDataWritten);
                RCSetup.SpectateTimer.Stop();
                RCSetup.SpectateTimer.Enabled = false;
                label_currentSpec.Text = string.Empty;
                playerListMenu_spectate.Text = "Spectate";
            }
        }

        private void ServerManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RCSetup.SpectateTimer.Enabled)
            {
                // exit spectator mode before closing
                RCSetup.SpectateTimer.Enabled = false;
                RCSetup.SpectateTimer.Stop();
                var restoreAddress = BitConverter.GetBytes(RCSetup.myPlayerAddress);
                var bytesWritten = 0;
                WriteProcessMemory((int)RCSetup.GameHandle, 0x007F2DD4, restoreAddress, restoreAddress.Length,
                    ref bytesWritten);
                CloseHandle(RCSetup.GameHandle);
            }
        }

        private void autoMessages_stateCheckbox(object sender, EventArgs e)
        {
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.EnableAutoMsg" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id },
                { "active", cb_enableAutoMsg.Checked }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
                return;
            MessageBox.Show("BMTTV reported an error. Please try again later.");
        }

        private void autoMessage_clickMoveUp(object sender, KeyEventArgs e)
        {
        }

        private void autoMessage_clickMoveDown(object sender, KeyEventArgs e)
        {
        }

        private void chat_btnSelectPlayerChanged(object sender, EventArgs e)
        {
            if (rb_chatPlayerHist.Checked == false)
            {
                chat_channelSelection.SelectedIndex = 0;
                chat_channelSelection.Enabled = false;
            }
        }

        private void keyup_textBoxMsg(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) chat_SendMsg(sender, e);
        }

        private void event_enterVPNsettings(object sender, EventArgs e)
        {
            var request = new Dictionary<string, dynamic>
            {
                { "action", "BMTRC.EnableVPNCheck" },
                { "SessionID", RCSetup.SessionID },
                { "serverID", _state.Instances[ArrayID].Id }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));

            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                group_vpnSettings.Enabled = true;
                group_vpnWhitelist.Enabled = true;
            }
            else
            {
                group_vpnSettings.Enabled = false;
                group_vpnWhitelist.Enabled = false;
                MessageBox.Show("VPN Checking is not enabled on this server.\n", "Error");
            }
        }

    }
}