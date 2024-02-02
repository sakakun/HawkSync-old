using HawkSync_SM.classes.logs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class UserManager : Form
    {
        AppState _state;
        DataTable adminTable = new DataTable();
        DataTable connectionLog = new DataTable();
        DataTable profileAccess = new DataTable();
        DataTable activityLog = new DataTable();
        Dictionary<int, Permissions> newUserPermission = new Dictionary<int, Permissions>();
        Dictionary<int, InstancePermissions> tempUserPermissions = new Dictionary<int, InstancePermissions>();
        MainAppPermissions appPermissions = new MainAppPermissions()
        {
            UserManagerPermissions = new UserManagerPermissions()
        };
        DataTable activeConnections = new DataTable();
        private bool changedInstance { get; set; }
        private bool editUser { get; set; }
        private bool loadingUser { get; set; }
        private int primaryUser { get; set; }
        public UserManager(AppState state)
        {
            InitializeComponent();
            _state = state;
            // admin table setup
            adminTable.Columns.Add("ID");
            adminTable.Columns.Add("Username");
            adminTable.Columns.Add("Role");

            // connection log
            connectionLog.Columns.Add("Date");
            connectionLog.Columns.Add("Username");

            // profile access
            profileAccess.Columns.Add("ID");
            profileAccess.Columns.Add("Profile Name");

            // active connections
            activeConnections.Columns.Add("Username");
            activeConnections.Columns.Add("IP");

            // activity log
            activityLog.Columns.Add("Date");
            activityLog.Columns.Add("Username");
            activityLog.Columns.Add("Action");

            listView1.CreateGraphics();
        }

        private void UserManager_Load(object sender, EventArgs e)
        {
            // admin table
            dataGridView1.DataSource = adminTable;
            dataGridView1.Columns["ID"].Width = 20;
            dataGridView1.Columns["ID"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Username"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            foreach (var item in _state.Users)
            {
                DataRow newRow = adminTable.NewRow();
                newRow["ID"] = item.Value.UserID.ToString();
                newRow["Username"] = item.Key;
                if (item.Value.SuperAdmin == true)
                {
                    newRow["Role"] = "Super Admin";
                }
                else
                {
                    if (item.Value.SubAdmin == -1 || item.Value.SubAdmin == 0)
                    {
                        newRow["Role"] = "Primary User";
                    }
                    else
                    {
                        newRow["Role"] = "Sub User";
                    }
                }
                adminTable.Rows.Add(newRow);
            }

            // instances
            if (_state.Instances.Count > 0)
            {
                foreach (var instance in _state.Instances)
                {
                    comboBox1.Items.Add(instance.Value.GameName);
                }
                comboBox1.SelectedIndex = 0;
            }
            textBox2.PasswordChar = '*';

            // connection log
            dataGridView2.DataSource = GetRCClients();
            dataGridView2.Columns["Date"].Width = 130;
            dataGridView2.Columns["Date"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns["Username"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            foreach (var RCClient in _state.rcClients)
            {
                if (_state.server.IsClientConnected(RCClient.Value.RemoteAddress.ToString() + ":" + RCClient.Value.RemotePort))
                {
                    DataRow activeConnectionRow = activeConnections.NewRow();
                    activeConnectionRow["Username"] = RCClient.Value._username;
                    activeConnectionRow["IP"] = RCClient.Value.RemoteAddress.ToString();
                    activeConnections.Rows.Add(activeConnectionRow);
                }
            }
            dataGridView3.DataSource = activeConnections;
            dataGridView3.Columns["Username"].Width = 80;
            dataGridView3.Columns["IP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView5.DataSource = profileAccess;
            dataGridView5.Columns["ID"].Width = 20;
            dataGridView5.Columns["ID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


            // activity log
            dataGridView4.DataSource = activityLog;
            dataGridView4.Columns["Date"].Width = 130;
            dataGridView4.Columns["Date"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView4.Columns["Username"].Width = 80;
            dataGridView4.Columns["Username"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView4.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView4.Columns["Action"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            foreach (var action in _state.RCLogs)
            {
                DataRow newRow = activityLog.NewRow();
                newRow["Date"] = action.Date;
                newRow["Username"] = action.Username;
                newRow["Action"] = action.Action;
                activityLog.Rows.Add(newRow);
            }

            // uncomment to loop through the listview to get a more accurate list.
            /*for (int i = 0; i < listView1.Groups.Count; i++)
            {
                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine("Index: " + i + " Group: " + listView1.Groups[i].Header);
                Console.WriteLine("--------------------------------------------------------");
                for (int x = 0; x < listView1.Groups[i].Items.Count; x++)
                {
                    Console.WriteLine("Index: " + x + " " + listView1.Groups[i].Items[x].Text);
                }
            }*/
        }

        private void ListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (changedInstance == true || loadingUser == true)
            {
                return;
            }
            int serverId = comboBox1.SelectedIndex;
            if (tempUserPermissions.ContainsKey(serverId) == false)
            {
                addNewTempPerms(serverId);
            }

            InstancePermissions instancePermissions = tempUserPermissions[serverId];

            // allow access to instance
            instancePermissions.Access = listView1.Groups[0].Items[0].Checked;
            // instance settings
            instancePermissions.StartInstance = listView1.Groups[0].Items[1].Checked;
            instancePermissions.StopInstance = listView1.Groups[0].Items[2].Checked;
            instancePermissions.ModifyInstance = listView1.Groups[0].Items[3].Checked;

            // auto messages
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.Access = listView1.Groups[1].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.AddMsg = listView1.Groups[1].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.DeleteMsg = listView1.Groups[1].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.ModifyInterval = listView1.Groups[1].Items[3].Checked;

            // player manager
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.Access = listView1.Groups[2].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ViewPlayersList = listView1.Groups[2].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionKickPlayer = listView1.Groups[2].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionTempBanPlayer = listView1.Groups[2].Items[3].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionBanPlayer = listView1.Groups[2].Items[4].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionWarnPlayer = listView1.Groups[2].Items[5].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListAddPlayer = listView1.Groups[2].Items[6].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListDeletePlayer = listView1.Groups[2].Items[7].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangeBanSettings = listView1.Groups[2].Items[8].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListAddSlap = listView1.Groups[2].Items[9].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListDeleteSlap = listView1.Groups[2].Items[10].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.EnableGodMode = listView1.Groups[2].Items[11].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisableGodMode = listView1.Groups[2].Items[12].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisarmPlayer = listView1.Groups[2].Items[13].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.RearmPlayer = listView1.Groups[2].Items[14].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangePlayerTeam = listView1.Groups[2].Items[15].Checked;

            // vpn settings
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.Access = listView1.Groups[3].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListAddPlayer = listView1.Groups[3].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListDeletePlayer = listView1.Groups[3].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.ModifyWarnLevel = listView1.Groups[3].Items[3].Checked;

            // server settings
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Access = listView1.Groups[4].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyServerName = listView1.Groups[4].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifySessionType = listView1.Groups[4].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxSlots = listView1.Groups[4].Items[3].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyCountryCode = listView1.Groups[4].Items[4].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPassword = listView1.Groups[4].Items[5].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyTimeLimit = listView1.Groups[4].Items[6].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyStartDelay = listView1.Groups[4].Items[7].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyReplayMaps = listView1.Groups[4].Items[8].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRespawnTime = listView1.Groups[4].Items[9].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMOTD = listView1.Groups[4].Items[10].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRequireNovaLogin = listView1.Groups[4].Items[11].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyCustomSkins = listView1.Groups[4].Items[12].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAutoBalance = listView1.Groups[4].Items[13].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMinPing = listView1.Groups[4].Items[14].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMinPing = listView1.Groups[4].Items[15].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMaxPing = listView1.Groups[4].Items[16].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxPing = listView1.Groups[4].Items[17].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyOneShotKills = listView1.Groups[4].Items[18].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFatBullets = listView1.Groups[4].Items[19].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDestroyBuildings = listView1.Groups[4].Items[20].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyBlueTeamPassword = listView1.Groups[4].Items[21].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRedTeamPassword = listView1.Groups[4].Items[22].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFire = listView1.Groups[4].Items[23].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFireKills = listView1.Groups[4].Items[24].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyTags = listView1.Groups[4].Items[25].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyWarning = listView1.Groups[4].Items[26].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyScoreBoard = listView1.Groups[4].Items[27].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPSPTakeOver = listView1.Groups[4].Items[28].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagReturnTime = listView1.Groups[4].Items[29].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxTeamLives = listView1.Groups[4].Items[30].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTracers = listView1.Groups[4].Items[31].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTeamClays = listView1.Groups[4].Items[32].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAllowAutoRange = listView1.Groups[4].Items[33].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagBallScore = listView1.Groups[4].Items[34].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyKOTHScore = listView1.Groups[4].Items[35].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDMScore = listView1.Groups[4].Items[36].Checked;

            // restrictions
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.Access = listView1.Groups[5].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyRoleRestrictions = listView1.Groups[5].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyWeaponRestrictions = listView1.Groups[5].Items[2].Checked;

            // map manager
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.Access = listView1.Groups[6].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.ModifyMapList = listView1.Groups[6].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.UpdateMapList = listView1.Groups[6].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.ScoreMap = listView1.Groups[6].Items[3].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.ShuffleMaps = listView1.Groups[6].Items[4].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.SetNextMap = listView1.Groups[6].Items[5].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.SaveRotation = listView1.Groups[6].Items[6].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.LoadRotation = listView1.Groups[6].Items[7].Checked;

            // chat manager
            instancePermissions.ServerManagerPermissions.ChatManagerPermissions.Access = listView1.Groups[7].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.ChatManagerPermissions.SendMsg = listView1.Groups[7].Items[1].Checked;

            // rotation manager
            instancePermissions.RotationManagerPermissions.Access = listView1.Groups[8].Items[0].Checked;
            instancePermissions.RotationManagerPermissions.CreateNewRotation = listView1.Groups[8].Items[1].Checked;
            instancePermissions.RotationManagerPermissions.DeleteRotation = listView1.Groups[8].Items[2].Checked;

            // file manager
            instancePermissions.FileManagerPermissions.Access = listView1.Groups[9].Items[0].Checked;
            instancePermissions.FileManagerPermissions.Download = listView1.Groups[9].Items[1].Checked;
            instancePermissions.FileManagerPermissions.Upload = listView1.Groups[9].Items[2].Checked;
        }

        private DataTable GetRCClients()
        {
            List<RCLogs> logs = _state.RCLogs;
            logs.Reverse();
            int i = 0;
            foreach (var connectionStateLog in _state.RCLogs)
            {
                i++;
                if (i == 50)
                {
                    break;
                }
                DataRow newRow = connectionLog.NewRow();
                newRow["Date"] = connectionStateLog.Date.ToString("MM/dd/yyyy hh:mm tt");
                newRow["Username"] = connectionStateLog.Username;
                connectionLog.Rows.Add(newRow);
            }
            return connectionLog;
        }

        /*private DataTable GetAdminTable()
        {
            BindingSource Bnd = new BindingSource
            {
                DataSource = adminTable
            };
            foreach (var item in _state.Users)
            {
                int Index = Bnd.Find("Username", item.Key);
                if (Index == -1)
                {
                    DataRow newRow = adminTable.NewRow();
                    newRow["ID"] = item.Value.UserID.ToString();
                    newRow["Username"] = item.Key;
                    adminTable.Rows.Add(newRow);
                }
            }
            Bnd.Dispose();
            return adminTable;
        }*/

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                listView1.Enabled = false;
                comboBox1.Enabled = false;
            }
            else if (checkBox1.Checked == false)
            {
                listView1.Enabled = true;
                comboBox1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Modify User")
            {
                editUser = true;
            }
            if (string.IsNullOrWhiteSpace(textBox1.Text) == true)
            {
                MessageBox.Show("Please enter a valid username.", "Error");
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox2.Text) == true && editUser == false)
            {
                MessageBox.Show("Please enter a valid password.", "Error");
                return;
            }
            if (_state.Users.ContainsKey(textBox1.Text))
            {
                MessageBox.Show("Username already exists!\nPlease choose a different username.", "Error");
                return;
            }
            string passwordMD5 = Crypt.CreateMD5(textBox2.Text);
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            if (editUser == false)
            {
                if (checkBox1.Checked == true)
                {
                    tempUserPermissions = new Dictionary<int, InstancePermissions>();
                    appPermissions = new MainAppPermissions();
                }
                Permissions permissions = new Permissions
                {
                    InstancePermissions = tempUserPermissions,
                    RemoteAdmin = checkBox2.Checked,
                    WebAdmin = checkBox3.Checked,
                    MainAppPermissions = appPermissions
                };
                SQLiteCommand insertCmd = new SQLiteCommand("INSERT INTO `users` (`id`, `username`, `password`, `permissions`, `superadmin`, `subadmin`) VALUES (NULL, @username, @password, @permissions, @superadmin, @subadmin);", db);
                insertCmd.Parameters.AddWithValue("@username", textBox1.Text);
                insertCmd.Parameters.AddWithValue("@password", passwordMD5.ToLower());
                insertCmd.Parameters.AddWithValue("@permissions", JsonConvert.SerializeObject(permissions));
                insertCmd.Parameters.AddWithValue("@superadmin", Convert.ToInt32(checkBox1.Checked));
                insertCmd.Parameters.AddWithValue("@subadmin", primaryUser);
                insertCmd.ExecuteNonQuery();
                insertCmd.Dispose();
                int newID = (int)db.LastInsertRowId;
                _state.Users.Add(textBox1.Text, new UserCodes
                {
                    Password = passwordMD5.ToLower(),
                    UserID = newID,
                    SuperAdmin = checkBox1.Checked,
                    Permissions = permissions,
                    SubAdmin = primaryUser
                });

                db.Close();
                db.Dispose();
                DataRow row = adminTable.NewRow();
                row["ID"] = newID;
                row["Username"] = textBox1.Text;
                if (checkBox1.Checked == true)
                {
                    row["Role"] = "Super Admin";
                }
                else
                {
                    if (primaryUser == 0)
                    {
                        row["Role"] = "Primary User";
                    }
                    else
                    {
                        row["Role"] = "Sub User";
                    }
                }
                adminTable.Rows.Add(row);
            }
            else
            {
                textBox1.Enabled = true;
            }
            resetUserForm();
            //dataGridView1.DataSource = adminTable;
            if (editUser == false)
                MessageBox.Show("User added successfully!", "Success");
            if (editUser == true)
                MessageBox.Show("User modified successfully!", "Success");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            changedInstance = true;
            int serverId = comboBox1.SelectedIndex;
            if (tempUserPermissions.ContainsKey(serverId) == false)
            {
                addNewTempPerms(serverId);
            }


            InstancePermissions instancePermissions = tempUserPermissions[serverId];

            // allow access to instance
            listView1.Groups[0].Items[0].Checked = instancePermissions.Access;
            // instance settings
            listView1.Groups[0].Items[1].Checked = instancePermissions.StartInstance;
            listView1.Groups[0].Items[2].Checked = instancePermissions.StopInstance;
            listView1.Groups[0].Items[3].Checked = instancePermissions.ModifyInstance;

            // auto messages
            listView1.Groups[1].Items[0].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.Access;
            listView1.Groups[1].Items[1].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.AddMsg;
            listView1.Groups[1].Items[2].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.DeleteMsg;
            listView1.Groups[1].Items[3].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.ModifyInterval;

            // player manager
            listView1.Groups[2].Items[0].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.Access;
            listView1.Groups[2].Items[1].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ViewPlayersList;
            listView1.Groups[2].Items[2].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionKickPlayer;
            listView1.Groups[2].Items[3].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionTempBanPlayer;
            listView1.Groups[2].Items[4].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionBanPlayer;
            listView1.Groups[2].Items[5].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionWarnPlayer;
            listView1.Groups[2].Items[6].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListAddPlayer;
            listView1.Groups[2].Items[7].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListDeletePlayer;
            listView1.Groups[2].Items[8].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangeBanSettings;
            listView1.Groups[2].Items[9].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListAddSlap;
            listView1.Groups[2].Items[10].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListDeleteSlap;
            listView1.Groups[2].Items[11].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.EnableGodMode;
            listView1.Groups[2].Items[12].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisableGodMode;
            listView1.Groups[2].Items[13].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisarmPlayer;
            listView1.Groups[2].Items[14].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.RearmPlayer;
            listView1.Groups[2].Items[15].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangePlayerTeam;

            // vpn settings
            listView1.Groups[3].Items[0].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.Access;
            listView1.Groups[3].Items[1].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListAddPlayer;
            listView1.Groups[3].Items[2].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListDeletePlayer;
            listView1.Groups[3].Items[3].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.ModifyWarnLevel;

            // server settings
            listView1.Groups[4].Items[0].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Access;
            listView1.Groups[4].Items[1].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyServerName;
            listView1.Groups[4].Items[2].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifySessionType;
            listView1.Groups[4].Items[3].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxSlots;
            listView1.Groups[4].Items[4].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyCountryCode;
            listView1.Groups[4].Items[5].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPassword;
            listView1.Groups[4].Items[6].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyTimeLimit;
            listView1.Groups[4].Items[7].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyStartDelay;
            listView1.Groups[4].Items[8].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyReplayMaps;
            listView1.Groups[4].Items[9].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRespawnTime;
            listView1.Groups[4].Items[10].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMOTD;
            listView1.Groups[4].Items[11].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRequireNovaLogin;
            listView1.Groups[4].Items[12].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyCustomSkins;
            listView1.Groups[4].Items[13].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAutoBalance;
            listView1.Groups[4].Items[14].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMinPing;
            listView1.Groups[4].Items[15].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMinPing;
            listView1.Groups[4].Items[16].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMaxPing;
            listView1.Groups[4].Items[17].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxPing;
            listView1.Groups[4].Items[18].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyOneShotKills;
            listView1.Groups[4].Items[19].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFatBullets;
            listView1.Groups[4].Items[20].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDestroyBuildings;
            listView1.Groups[4].Items[21].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyBlueTeamPassword;
            listView1.Groups[4].Items[22].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRedTeamPassword;
            listView1.Groups[4].Items[23].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFire;
            listView1.Groups[4].Items[24].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFireKills;
            listView1.Groups[4].Items[25].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyTags;
            listView1.Groups[4].Items[26].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyWarning;
            listView1.Groups[4].Items[27].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyScoreBoard;
            listView1.Groups[4].Items[28].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPSPTakeOver;
            listView1.Groups[4].Items[29].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagReturnTime;
            listView1.Groups[4].Items[30].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxTeamLives;
            listView1.Groups[4].Items[31].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTracers;
            listView1.Groups[4].Items[32].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTeamClays;
            listView1.Groups[4].Items[33].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAllowAutoRange;
            listView1.Groups[4].Items[34].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagBallScore;
            listView1.Groups[4].Items[35].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyKOTHScore;
            listView1.Groups[4].Items[36].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDMScore;

            // restrictions
            listView1.Groups[5].Items[0].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.Access;
            listView1.Groups[5].Items[1].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyRoleRestrictions;
            listView1.Groups[5].Items[2].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyWeaponRestrictions;

            // map manager
            listView1.Groups[6].Items[0].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.Access;
            listView1.Groups[6].Items[1].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.ModifyMapList;
            listView1.Groups[6].Items[2].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.UpdateMapList;
            listView1.Groups[6].Items[3].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.ScoreMap;
            listView1.Groups[6].Items[4].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.ShuffleMaps;
            listView1.Groups[6].Items[5].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.SetNextMap;
            listView1.Groups[6].Items[6].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.SaveRotation;
            listView1.Groups[6].Items[7].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.LoadRotation;

            // chat manager
            listView1.Groups[7].Items[0].Checked = instancePermissions.ServerManagerPermissions.ChatManagerPermissions.Access;
            listView1.Groups[7].Items[1].Checked = instancePermissions.ServerManagerPermissions.ChatManagerPermissions.SendMsg;

            // rotation manager
            listView1.Groups[8].Items[0].Checked = instancePermissions.RotationManagerPermissions.Access;
            listView1.Groups[8].Items[1].Checked = instancePermissions.RotationManagerPermissions.CreateNewRotation;
            listView1.Groups[8].Items[2].Checked = instancePermissions.RotationManagerPermissions.DeleteRotation;

            // file manager
            listView1.Groups[9].Items[0].Checked = instancePermissions.FileManagerPermissions.Access;
            listView1.Groups[9].Items[1].Checked = instancePermissions.FileManagerPermissions.Download;
            listView1.Groups[9].Items[2].Checked = instancePermissions.FileManagerPermissions.Upload;


            changedInstance = false;
        }

        private void addNewTempPerms(int serverId)
        {
            tempUserPermissions.Add(serverId, new InstancePermissions
            {
                Access = true,
                ServerManagerPermissions = new ServerManagerPermissions
                {
                    Access = true,
                    AutoMessagesPermissions = new AutoMessagesPermissions
                    {
                        Access = true,
                        AddMsg = true,
                        DeleteMsg = true,
                        ModifyInterval = true
                    },
                    ChatManagerPermissions = new ChatManagerPermissions
                    {
                        Access = true,
                        SendMsg = true
                    },
                    MapManagerPermissions = new MapManagerPermissions
                    {
                        Access = true,
                        ModifyMapList = true,
                        ScoreMap = true,
                        SetNextMap = true,
                        ShuffleMaps = true,
                        UpdateMapList = true,
                        SaveRotation = true,
                        LoadRotation = true
                    },
                    PlayerManagerPermissions = new PlayerManagerPermissions
                    {
                        Access = true,
                        ActionBanPlayer = true,
                        ActionKickPlayer = true,
                        ActionTempBanPlayer = true,
                        ActionWarnPlayer = true,
                        BanListAddPlayer = true,
                        BanListDeletePlayer = true,
                        ChangeBanSettings = true,
                        ChangePlayerTeam = true,
                        DisableGodMode = true,
                        DisarmPlayer = true,
                        EnableGodMode = true,
                        RearmPlayer = true,
                        SlapListAddSlap = true,
                        SlapListDeleteSlap = true,
                        ViewPlayersList = true,
                        VPNSettings = new VPNSettings
                        {
                            Access = true,
                            ModifyWarnLevel = false,
                            WhiteListAddPlayer = true,
                            WhiteListDeletePlayer = true
                        }
                    },
                    ServerSettingsPermissions = new ServerSettingsPermissions
                    {
                        Access = true,
                        ModifyAllowAutoRange = true,
                        ModifyAutoBalance = true,
                        ModifyBlueTeamPassword = true,
                        ModifyCountryCode = true,
                        ModifyCustomSkins = true,
                        ModifyDestroyBuildings = true,
                        ModifyDMScore = true,
                        ModifyEnableMaxPing = true,
                        ModifyEnableMinPing = true,
                        ModifyFatBullets = true,
                        ModifyFlagBallScore = true,
                        ModifyFlagReturnTime = true,
                        ModifyFriendlyFire = true,
                        ModifyFriendlyFireKills = true,
                        ModifyFriendlyTags = true,
                        ModifyFriendlyWarning = true,
                        ModifyKOTHScore = true,
                        ModifyMaxPing = true,
                        ModifyMaxSlots = true,
                        ModifyMaxTeamLives = true,
                        ModifyMinPing = true,
                        ModifyMOTD = true,
                        ModifyOneShotKills = true,
                        ModifyPassword = true,
                        ModifyPSPTakeOver = true,
                        ModifyRedTeamPassword = true,
                        ModifyReplayMaps = true,
                        ModifyRequireNovaLogin = true,
                        ModifyRespawnTime = true,
                        ModifyScoreBoard = true,
                        ModifyServerName = true,
                        ModifySessionType = true,
                        ModifyShowTeamClays = true,
                        ModifyShowTracers = true,
                        ModifyStartDelay = true,
                        ModifyTimeLimit = true,
                        Restrictions = new Restrictions
                        {
                            Access = true,
                            ModifyRoleRestrictions = true,
                            ModifyWeaponRestrictions = true
                        }
                    }
                },
                RotationManagerPermissions = new RotationManagerPermissions
                {
                    Access = true,
                    CreateNewRotation = true,
                    DeleteRotation = true
                },
                FileManagerPermissions = new FileManagerPermissions
                {
                    Access = true,
                    Download = true,
                    Upload = true
                }
            });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            resetUserForm();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.CurrentCell == null)
            {
                return;
            }
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridView1.HitTest(e.X, e.Y);
                if (hitTest.RowIndex == -1)
                {
                    return;
                }
                dataGridView1.Rows[hitTest.RowIndex].Selected = true;
                contextMenuStrip1.Show(dataGridView1, e.X, e.Y);
            }
        }

        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            DialogResult results = MessageBox.Show("Are you sure you want to delete this user?", "Delete User", MessageBoxButtons.YesNo);
            if (results == DialogResult.Yes)
            {
                editUser = true;
                int userid = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                string username = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                _state.Users.Remove(dataGridView1.SelectedRows[0].Cells[1].Value.ToString());
                int userIndex = dataGridView1.SelectedRows[0].Index;
                List<string> usersToDelete = new List<string>();
                foreach (var user in _state.Users)
                {
                    if (user.Value.SubAdmin == userid)
                    {
                        usersToDelete.Add(user.Key);
                    }
                }
                foreach (string user in usersToDelete)
                {
                    for (int i = 0; i < adminTable.Rows.Count; i++)
                    {
                        DataRow admin = adminTable.Rows[i];
                        if (admin["Username"].ToString() == user)
                        {
                            adminTable.Rows.Remove(admin);
                        }
                    }
                    _state.Users.Remove(user);
                }
                usersToDelete.Clear();
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `users` WHERE `username` = @username AND `id` = @userid;", db);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                SQLiteCommand subAdminCmd = new SQLiteCommand("DELETE FROM `users` WHERE `subadmin` = @id;", db);
                subAdminCmd.Parameters.AddWithValue("@id", userid);
                subAdminCmd.ExecuteNonQuery();
                subAdminCmd.Dispose();
                db.Close();
                db.Dispose();
                dataGridView1.ClearSelection();
                adminTable.Rows.RemoveAt(userIndex);
                if (adminTable.Rows.Count != 0)
                {
                    dataGridView1.Rows[0].Selected = true;
                }
                editUser = false;
                MessageBox.Show("User has been deleted successfully.", "Success");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ViewUserActivity viewUserActivity = new ViewUserActivity(_state);
            viewUserActivity.ShowDialog();
        }

        private void toolStripMenuItem4_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null || dataGridView1.CurrentCell.RowIndex == -1)
            {
                MessageBox.Show("Please select an admin from the list.", "Error");
                return;
            }
            DataRow data = adminTable.Rows[dataGridView1.CurrentCell.RowIndex];
            string username = data["Username"].ToString();
            textBox1.Text = username;
            textBox1.Enabled = false;
            textBox2.Text = "*** CLICK TO CHANGE ***";
            button1.Text = "Modify User";
            textBox2.PasswordChar = '\0';
            tempUserPermissions = _state.Users[username].Permissions.InstancePermissions;
            loadingUser = true;

            if (tempUserPermissions != null && tempUserPermissions.Count > 0)
            {
                int serverId = comboBox1.SelectedIndex;
                InstancePermissions instancePermissions = tempUserPermissions[serverId];
                // allow access to instance
                listView1.Groups[0].Items[0].Checked = instancePermissions.Access;
                // instance settings
                listView1.Groups[0].Items[1].Checked = instancePermissions.StartInstance;
                listView1.Groups[0].Items[2].Checked = instancePermissions.StopInstance;
                listView1.Groups[0].Items[3].Checked = instancePermissions.ModifyInstance;

                // auto messages
                listView1.Groups[1].Items[0].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.Access;
                listView1.Groups[1].Items[1].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.AddMsg;
                listView1.Groups[1].Items[2].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.DeleteMsg;
                listView1.Groups[1].Items[3].Checked = instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.ModifyInterval;

                // player manager
                listView1.Groups[2].Items[0].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.Access;
                listView1.Groups[2].Items[1].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ViewPlayersList;
                listView1.Groups[2].Items[2].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionKickPlayer;
                listView1.Groups[2].Items[3].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionTempBanPlayer;
                listView1.Groups[2].Items[4].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionBanPlayer;
                listView1.Groups[2].Items[5].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionWarnPlayer;
                listView1.Groups[2].Items[6].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListAddPlayer;
                listView1.Groups[2].Items[7].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListDeletePlayer;
                listView1.Groups[2].Items[8].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangeBanSettings;
                listView1.Groups[2].Items[9].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListAddSlap;
                listView1.Groups[2].Items[10].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListDeleteSlap;
                listView1.Groups[2].Items[11].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.EnableGodMode;
                listView1.Groups[2].Items[12].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisableGodMode;
                listView1.Groups[2].Items[13].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisarmPlayer;
                listView1.Groups[2].Items[14].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.RearmPlayer;
                listView1.Groups[2].Items[15].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangePlayerTeam;

                // vpn settings
                listView1.Groups[3].Items[0].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.Access;
                listView1.Groups[3].Items[1].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListAddPlayer;
                listView1.Groups[3].Items[2].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListDeletePlayer;
                listView1.Groups[3].Items[3].Checked = instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.ModifyWarnLevel;

                // server settings
                listView1.Groups[4].Items[0].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Access;
                listView1.Groups[4].Items[1].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyServerName;
                listView1.Groups[4].Items[2].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifySessionType;
                listView1.Groups[4].Items[3].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxSlots;
                listView1.Groups[4].Items[4].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyCountryCode;
                listView1.Groups[4].Items[5].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPassword;
                listView1.Groups[4].Items[6].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyTimeLimit;
                listView1.Groups[4].Items[7].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyStartDelay;
                listView1.Groups[4].Items[8].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyReplayMaps;
                listView1.Groups[4].Items[9].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRespawnTime;
                listView1.Groups[4].Items[10].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMOTD;
                listView1.Groups[4].Items[11].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRequireNovaLogin;
                listView1.Groups[4].Items[12].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyCustomSkins;
                listView1.Groups[4].Items[13].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAutoBalance;
                listView1.Groups[4].Items[14].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMinPing;
                listView1.Groups[4].Items[15].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMinPing;
                listView1.Groups[4].Items[16].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMaxPing;
                listView1.Groups[4].Items[17].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxPing;
                listView1.Groups[4].Items[18].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyOneShotKills;
                listView1.Groups[4].Items[19].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFatBullets;
                listView1.Groups[4].Items[20].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDestroyBuildings;
                listView1.Groups[4].Items[21].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyBlueTeamPassword;
                listView1.Groups[4].Items[22].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRedTeamPassword;
                listView1.Groups[4].Items[23].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFire;
                listView1.Groups[4].Items[24].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFireKills;
                listView1.Groups[4].Items[25].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyTags;
                listView1.Groups[4].Items[26].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyWarning;
                listView1.Groups[4].Items[27].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyScoreBoard;
                listView1.Groups[4].Items[28].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPSPTakeOver;
                listView1.Groups[4].Items[29].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagReturnTime;
                listView1.Groups[4].Items[30].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxTeamLives;
                listView1.Groups[4].Items[31].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTracers;
                listView1.Groups[4].Items[32].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTeamClays;
                listView1.Groups[4].Items[33].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAllowAutoRange;
                listView1.Groups[4].Items[34].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagBallScore;
                listView1.Groups[4].Items[35].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyKOTHScore;
                listView1.Groups[4].Items[36].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDMScore;

                // restrictions
                listView1.Groups[5].Items[0].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.Access;
                listView1.Groups[5].Items[1].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyRoleRestrictions;
                listView1.Groups[5].Items[2].Checked = instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyWeaponRestrictions;

                // map manager
                listView1.Groups[6].Items[0].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.Access;
                listView1.Groups[6].Items[1].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.ModifyMapList;
                listView1.Groups[6].Items[2].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.UpdateMapList;
                listView1.Groups[6].Items[3].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.ScoreMap;
                listView1.Groups[6].Items[4].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.ShuffleMaps;
                listView1.Groups[6].Items[5].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.SetNextMap;
                listView1.Groups[6].Items[6].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.SaveRotation;
                listView1.Groups[6].Items[7].Checked = instancePermissions.ServerManagerPermissions.MapManagerPermissions.LoadRotation;

                // chat manager
                listView1.Groups[7].Items[0].Checked = instancePermissions.ServerManagerPermissions.ChatManagerPermissions.Access;
                listView1.Groups[7].Items[1].Checked = instancePermissions.ServerManagerPermissions.ChatManagerPermissions.SendMsg;

                // rotation manager
                listView1.Groups[8].Items[0].Checked = instancePermissions.RotationManagerPermissions.Access;
                listView1.Groups[8].Items[1].Checked = instancePermissions.RotationManagerPermissions.CreateNewRotation;
                listView1.Groups[8].Items[2].Checked = instancePermissions.RotationManagerPermissions.DeleteRotation;

                // file manager
                listView1.Groups[9].Items[0].Checked = instancePermissions.FileManagerPermissions.Access;
                listView1.Groups[9].Items[1].Checked = instancePermissions.FileManagerPermissions.Download;
                listView1.Groups[9].Items[2].Checked = instancePermissions.FileManagerPermissions.Upload;
            }
            checkBox1.Checked = _state.Users[username].SuperAdmin;
            checkBox2.Checked = _state.Users[username].Permissions.RemoteAdmin;
            checkBox3.Checked = _state.Users[username].Permissions.WebAdmin;
            loadingUser = false;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
            }
            else
            {
                checkBox4.Enabled = false;
                checkBox1.Enabled = true;
                primaryUser = 0;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null || dataGridView1.CurrentCell.RowIndex == -1 || editUser == true)
            {
                return;
            }

            checkBox4.Checked = false;
            checkBox4.Enabled = false;
            // populate the listview
            profileAccess.Rows.Clear();
            DataRow dr = adminTable.Rows[dataGridView1.CurrentCell.RowIndex];
            string username = dr["Username"].ToString();
            if (_state.Users[username].SuperAdmin == true)
            {
                foreach (var profile in _state.Instances)
                {
                    DataRow newRow = profileAccess.NewRow();
                    newRow["ID"] = profile.Value.Id;
                    newRow["Profile Name"] = profile.Value.GameName;
                    profileAccess.Rows.Add(newRow);
                }
            }
            else
            {
                foreach (var profile in _state.Users[username].Permissions.InstancePermissions)
                {
                    if (profile.Value.Access == true)
                    {
                        DataRow newRow = profileAccess.NewRow();
                        newRow["ID"] = _state.Instances[profile.Key].Id;
                        newRow["Profile Name"] = _state.Instances[profile.Key].GameName;
                        profileAccess.Rows.Add(newRow);
                    }
                }
            }
        }

        private void resetUserForm()
        {
            editUser = false;
            button1.Text = "Add User";
            textBox1.Enabled = true;
            textBox1.Text = "";
            textBox2.Text = "";
            textBox2.PasswordChar = '*';
            tempUserPermissions = new Dictionary<int, InstancePermissions>();
            if (_state.Instances.Count > 0) comboBox1.SelectedIndex = 0;
            primaryUser = 0;
            checkBox4.Checked = false;
            checkBox4.Enabled = false;
            comboBox1.Enabled = true;
            listView1.Enabled = true;
            checkBox1.Checked = false;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // reset form
            resetUserForm();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            // sub user
            if (dataGridView1.CurrentCell != null || dataGridView1.CurrentCell.RowIndex != -1)
            {
                DataRow selectedAdmin = adminTable.Rows[dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Index];
                string selectedAdminUsername = selectedAdmin["Username"].ToString();
                if (_state.Users[selectedAdminUsername].SuperAdmin == true)
                {
                    MessageBox.Show("Super Admins cannot have sub users.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Dictionary<int, InstancePermissions> primaryUserPermissions = _state.Users[selectedAdminUsername].Permissions.InstancePermissions;
                if (primaryUserPermissions.Count == 0)
                {
                    MessageBox.Show("This user does not have any permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                foreach (var user in _state.Users)
                {
                    if (user.Key == selectedAdminUsername)
                    {
                        primaryUser = user.Value.UserID;
                        checkBox4.Checked = true;
                        checkBox4.Enabled = true;
                        break;
                    }
                }
                button1.Text = "Add Sub User";
                tempUserPermissions = new Dictionary<int, InstancePermissions>();
                comboBox1.Items.Clear();
                foreach (var item in primaryUserPermissions)
                {
                    comboBox1.Items.Add(_state.Instances[item.Key].GameName);
                }
                comboBox1.SelectedIndex = 0;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (editUser == false || loadingUser == true)
            {
                return;
            }
            else
            {
                textBox2.Text = String.Empty;
            }
        }
    }
}
