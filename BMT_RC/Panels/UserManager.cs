using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HawkSync_RC.classes;
using HawkSync_RC.TVFunctions;

namespace HawkSync_RC
{
    public partial class UserManager : Form
    {
        RCSetup _setup;
        AppState _state;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        TVUserManager userManager;
        DataTable usersTable;
        DataTable rcLogs;
        DataTable rcConnections;
        public Dictionary<int, InstancePermissions> tmpCodes;
        Dictionary<string, UserCodes> _users { get; set; }
        bool profileChanged = false;
        int subadmin = 0;
        public UserManager(AppState state, RCSetup setup)
        {
            InitializeComponent();
            _state = state;
            _setup = setup;
            userManager = new TVUserManager(_state, _setup);
            // usersTable
            usersTable = new DataTable();
            usersTable.Columns.Add("ID", typeof(string));
            usersTable.Columns.Add("Username", typeof(string));
            usersTable.Columns.Add("Role", typeof(string));
            _users = new Dictionary<string, UserCodes>();

            // rc logs
            rcLogs = new DataTable();
            rcLogs.Columns.Add("Date", typeof(string));
            rcLogs.Columns.Add("Username", typeof(string));
            rcLogs.Columns.Add("Action", typeof(string));

            // rc connections
            rcConnections = new DataTable();
            rcConnections.Columns.Add("Username", typeof(string));
            rcConnections.Columns.Add("IP Address", typeof(string));

            // tmpCodes
            tmpCodes = new Dictionary<int, InstancePermissions>();

            listView1.CreateGraphics();
        }

        private void UserManager_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = usersTable;
            dataGridView1.Columns["ID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Role"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (userManager.GetUsers(out Dictionary<string, UserCodes> users) == OpenClass.Status.SUCCESS)
            {
                _users = users;
                foreach (var user in users)
                {
                    DataRow row = usersTable.NewRow();
                    row["ID"] = user.Value.UserID;
                    row["Username"] = user.Key;
                    if (user.Value.SuperAdmin == true)
                    {
                        row["Role"] = "Super Admin";
                    }
                    else
                    {
                        if (user.Value.SubAdmin == -1 || user.Value.SubAdmin == 0)
                        {
                            row["Role"] = "Primary User";
                        }
                        else
                        {
                            row["Role"] = "Sub User";
                        }
                    }
                    usersTable.Rows.Add(row);
                }
            }
            else
            {
                MessageBox.Show("Failed to get from TV.", "Error");
                return;
            }
            foreach (var instance in _state.Instances)
            {
                comboBox1.Items.Add(instance.Value.GameName);
            }
            comboBox1.SelectedIndex = 0;
            if (_state.UserCodes.SuperAdmin == false)
            {
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
                checkBox4.Checked = true;
                checkBox4.Enabled = true;
            }
            dataGridView2.DataSource = rcLogs;
            dataGridView2.Columns["Date"].Width = 130;
            dataGridView2.Columns["Date"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (userManager.GetLogs(out List<RCLogs> logs) == OpenClass.Status.SUCCESS)
            {
                foreach (var item in logs)
                {
                    DataRow row = rcLogs.NewRow();
                    row["Date"] = item.Date;
                    row["Username"] = item.Username;
                    row["Action"] = item.Action;
                    rcLogs.Rows.Add(row);
                }
            }
            dataGridView3.DataSource = rcConnections;
            if (userManager.GetCurrentConnections(out List<RCLogs> currentConnections) == OpenClass.Status.SUCCESS)
            {
                foreach (var item in currentConnections)
                {
                    DataRow row = rcConnections.NewRow();
                    row["Username"] = item.Username;
                    row["IP Address"] = item.Address;
                    rcConnections.Rows.Add(row);
                }
            }


            for (int i = 0; i < listView1.Groups.Count; i++)
            {
                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine("Index: " + i + " Group: " + listView1.Groups[i].Header);
                Console.WriteLine("--------------------------------------------------------");
                for (int x = 0; x < listView1.Groups[i].Items.Count; x++)
                {
                    Console.WriteLine("Index: " + x + " " + listView1.Groups[i].Items[x].Text);
                }
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            profileChanged = true;
            int serverId = comboBox1.SelectedIndex;
            if (tmpCodes.ContainsKey(serverId) == false)
            {
                addNewTempPerms(serverId);
            }

            InstancePermissions instancePermissions = tmpCodes[serverId];

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


            profileChanged = false;
        }

        private void addNewTempPerms(int serverId)
        {
            tmpCodes.Add(serverId, new InstancePermissions
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

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (profileChanged == true)
            {
                return;
            }
            int serverId = comboBox1.SelectedIndex;
            InstancePermissions instancePermissions = tmpCodes[serverId];

            // allow access to instance
            instancePermissions.Access = listView1.Groups[0].Items[0].Checked;
            // server settings permissions
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Access = listView1.Groups[5].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyTimeLimit = listView1.Groups[5].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAutoBalance = listView1.Groups[5].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRespawnTime = listView1.Groups[5].Items[3].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMinPing = listView1.Groups[5].Items[4].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMinPing = listView1.Groups[5].Items[5].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyEnableMaxPing = listView1.Groups[5].Items[6].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxPing = listView1.Groups[5].Items[7].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyOneShotKills = listView1.Groups[5].Items[8].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFatBullets = listView1.Groups[5].Items[9].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDestroyBuildings = listView1.Groups[5].Items[10].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyBlueTeamPassword = listView1.Groups[5].Items[11].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyRedTeamPassword = listView1.Groups[5].Items[12].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFire = listView1.Groups[5].Items[13].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyFireKills = listView1.Groups[5].Items[14].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyTags = listView1.Groups[5].Items[15].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFriendlyWarning = listView1.Groups[5].Items[16].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyScoreBoard = listView1.Groups[5].Items[17].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyPSPTakeOver = listView1.Groups[5].Items[18].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagReturnTime = listView1.Groups[5].Items[19].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyMaxTeamLives = listView1.Groups[5].Items[20].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTracers = listView1.Groups[5].Items[21].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyShowTeamClays = listView1.Groups[5].Items[22].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyAllowAutoRange = listView1.Groups[5].Items[23].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyFlagBallScore = listView1.Groups[5].Items[24].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyKOTHScore = listView1.Groups[5].Items[25].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.ModifyDMScore = listView1.Groups[5].Items[26].Checked;
            // restrictions
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyRoleRestrictions = listView1.Groups[6].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.ModifyWeaponRestrictions = listView1.Groups[6].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.ServerSettingsPermissions.Restrictions.Access = listView1.Groups[6].Items[2].Checked;
            // auto messages permissions
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.Access = listView1.Groups[1].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.AddMsg = listView1.Groups[1].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.DeleteMsg = listView1.Groups[1].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.AutoMessagesPermissions.ModifyInterval = listView1.Groups[1].Items[3].Checked;
            // map manager
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.Access = listView1.Groups[7].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.ModifyMapList = listView1.Groups[7].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.ScoreMap = listView1.Groups[7].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.SetNextMap = listView1.Groups[7].Items[3].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.UpdateMapList = listView1.Groups[7].Items[4].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.ShuffleMaps = listView1.Groups[7].Items[5].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.SaveRotation = listView1.Groups[7].Items[6].Checked;
            instancePermissions.ServerManagerPermissions.MapManagerPermissions.LoadRotation = listView1.Groups[7].Items[7].Checked;
            // chat manager
            instancePermissions.ServerManagerPermissions.ChatManagerPermissions.Access = listView1.Groups[8].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.ChatManagerPermissions.SendMsg = listView1.Groups[8].Items[1].Checked;
            // web admin access
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.Access = listView1.Groups[2].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionKickPlayer = listView1.Groups[2].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionTempBanPlayer = listView1.Groups[2].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ActionBanPlayer = listView1.Groups[2].Items[3].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisarmPlayer = listView1.Groups[2].Items[4].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.RearmPlayer = listView1.Groups[2].Items[5].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.ChangePlayerTeam = listView1.Groups[2].Items[6].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.EnableGodMode = listView1.Groups[2].Items[7].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.DisableGodMode = listView1.Groups[2].Items[8].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListAddPlayer = listView1.Groups[2].Items[9].Checked;
             instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.BanListDeletePlayer = listView1.Groups[2].Items[10].Checked;
            // slaps
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListAddSlap = listView1.Groups[4].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.SlapListDeleteSlap = listView1.Groups[4].Items[1].Checked;
            // vpn settings
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.Access = listView1.Groups[3].Items[0].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListAddPlayer = listView1.Groups[3].Items[1].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.WhiteListDeletePlayer = listView1.Groups[3].Items[2].Checked;
            instancePermissions.ServerManagerPermissions.PlayerManagerPermissions.VPNSettings.ModifyWarnLevel = listView1.Groups[3].Items[3].Checked;
            // rotation manager
            instancePermissions.RotationManagerPermissions.Access = listView1.Groups[10].Items[0].Checked;
            instancePermissions.RotationManagerPermissions.CreateNewRotation = listView1.Groups[10].Items[1].Checked;
            instancePermissions.RotationManagerPermissions.DeleteRotation = listView1.Groups[10].Items[2].Checked;
            //File manager
            instancePermissions.FileManagerPermissions.Access = listView1.Groups[11].Items[0].Checked;
            instancePermissions.FileManagerPermissions.Download = listView1.Groups[11].Items[1].Checked;
            instancePermissions.FileManagerPermissions.Upload = listView1.Groups[11].Items[2].Checked;
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                listView1.Enabled = false;
                comboBox1.Enabled = false;
                checkBox4.Enabled = false;
            }
            else
            {
                listView1.Enabled = true;
                comboBox1.Enabled = true;
                checkBox4.Enabled = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            resetForm();
        }

        private void resetForm()
        {
            listView1.Enabled = true;
            comboBox1.Enabled = true;
            checkBox4.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == String.Empty || string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid username!", "Error");
                return;
            }
            if (textBox2.Text == string.Empty || string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please enter a valid password!", "Error");
                return;
            }

            // first sanity check
            if (_users.ContainsKey(textBox1.Text))
            {
                MessageBox.Show("Username already in use!", "Error");
                return;
            }

            // submit form to TV
            OpenClass.Status response = userManager.AddUser(textBox1.Text, textBox2.Text, checkBox1.Checked, subadmin, new Permissions
            {
                InstancePermissions = tmpCodes,
                RemoteAdmin = checkBox2.Checked,
                WebAdmin = checkBox3.Checked,
            });
            if (response == OpenClass.Status.USERALREADYEXISTS)
            {
                MessageBox.Show("Username already exists!\nPlease choose a different username.", "Error");
                return;
            }
            if (response == OpenClass.Status.SUCCESS)
            {
                DataRow row = usersTable.NewRow();
                row["ID"] = _users.Count + 1;
                row["Username"] = textBox1.Text;
                if (checkBox1.Checked == true)
                {
                    row["Role"] = "Super Admin";
                }
                else
                {
                    if (subadmin == 0)
                    {
                        row["Role"] = "Primary User";
                    }
                    else if (subadmin > 0)
                    {
                        row["Role"] = "Sub Admin";
                    }
                }
                usersTable.Rows.Add(row);
                MessageBox.Show("The user was added successfully!", "Success");
                return;
            }
            else
            {
                MessageBox.Show("Something went wrong while trying to add a user.", "Error");
                return;
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            var hittest = dataGridView1.HitTest(e.X, e.Y);
            if (hittest.RowIndex == -1)
            {
                return;
            }
            dataGridView1.Rows[hittest.RowIndex].Selected = true;
            contextMenuStrip1.Show(dataGridView1, e.X, e.Y);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            // edit user
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // delete user
            if (dataGridView1.CurrentCell == null || dataGridView1.CurrentCell.RowIndex == -1)
            {
                return; // don't do anything since nothing is selected...
            }
            DataRow user = usersTable.Rows[dataGridView1.CurrentCell.RowIndex];
            OpenClass.Status response = userManager.DeleteUser(user["Username"].ToString());
            if (response == OpenClass.Status.SUCCESS)
            {
                usersTable.Rows.Remove(user);
                MessageBox.Show("The user has been deleted successfully.", "Success");
            }
            else
            {
                MessageBox.Show("Something went wrong when trying to delete the user!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // new primary user
            resetForm();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            // determine selected user
            if (dataGridView1.CurrentCell.RowIndex == -1 || dataGridView1.CurrentCell == null)
            {
                return; // don't do anything since nothing is selected...
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // disconnect user
            //OpenClass.Status disconnectUser = userManager.DisconnectUser(username);
            MessageBox.Show("This function has not been implemented yet.", "Not Implemented");
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // view activity log
            MessageBox.Show("This function has not been implemented yet.", "Not Implemented");
            return;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
