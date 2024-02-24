using System;
using System.ComponentModel;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace HawkSync_RC
{
    partial class RC_ServerManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RC_ServerManager));
            this.serverManager_container = new System.Windows.Forms.TabControl();
            this.page_players = new System.Windows.Forms.TabPage();
            this.label_currentSpec = new System.Windows.Forms.Label();
            this.group_currentPlayers = new System.Windows.Forms.GroupBox();
            this.grid_playerList = new System.Windows.Forms.DataGridView();
            this.tabControl_playerSelection = new System.Windows.Forms.TabControl();
            this.tab_bannedPlayers = new System.Windows.Forms.TabPage();
            this.group_addBan = new System.Windows.Forms.GroupBox();
            this.text_adPlayerName = new System.Windows.Forms.TextBox();
            this.button_addBan = new System.Windows.Forms.Button();
            this.text_abIPAddress = new System.Windows.Forms.TextBox();
            this.label_abPlayerName = new System.Windows.Forms.Label();
            this.combo_abReason = new System.Windows.Forms.ComboBox();
            this.label_abIPAddress = new System.Windows.Forms.Label();
            this.button_removeBan = new System.Windows.Forms.Button();
            this.group_banDetails = new System.Windows.Forms.GroupBox();
            this.value_banIPAddress = new System.Windows.Forms.Label();
            this.label_banIPAddress = new System.Windows.Forms.Label();
            this.value_banAdmin = new System.Windows.Forms.Label();
            this.value_banDateAdded = new System.Windows.Forms.Label();
            this.value_banReason = new System.Windows.Forms.Label();
            this.value_bdPlayerName = new System.Windows.Forms.Label();
            this.label_banAdmin = new System.Windows.Forms.Label();
            this.label_banReason = new System.Windows.Forms.Label();
            this.label_banDateAdded = new System.Windows.Forms.Label();
            this.label_bdPlayerName = new System.Windows.Forms.Label();
            this.grid_bannedPlayerList = new System.Windows.Forms.DataGridView();
            this.tab_vpnSettings = new System.Windows.Forms.TabPage();
            this.group_vpnWhitelist = new System.Windows.Forms.GroupBox();
            this.btn_cancelChanges = new System.Windows.Forms.Button();
            this.btn_deleteAddress = new System.Windows.Forms.Button();
            this.btn_addAddress = new System.Windows.Forms.Button();
            this.textBox_vpnAddress = new System.Windows.Forms.TextBox();
            this.textBox_vpnDescription = new System.Windows.Forms.TextBox();
            this.label_vpnAddress = new System.Windows.Forms.Label();
            this.label_vpnDescription = new System.Windows.Forms.Label();
            this.grid_vpn_allowlist = new System.Windows.Forms.DataGridView();
            this.group_vpnSettings = new System.Windows.Forms.GroupBox();
            this.label_vpn_abuse = new System.Windows.Forms.Label();
            this.value_vpn_abuselevel = new System.Windows.Forms.NumericUpDown();
            this.checkBox_vpn_disallow = new System.Windows.Forms.CheckBox();
            this.tab_warnMsgs = new System.Windows.Forms.TabPage();
            this.groupBox_playerWarnMsg = new System.Windows.Forms.GroupBox();
            this.btn_playerWarnMessageAdd = new System.Windows.Forms.Button();
            this.textBox_playerWarnMessageAdd = new System.Windows.Forms.TextBox();
            this.listBox_playerWarnMessages = new System.Windows.Forms.ListBox();
            this.label69 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.page_serverSettings = new System.Windows.Forms.TabPage();
            this.gB_ServerSettings = new System.Windows.Forms.GroupBox();
            this.gB_motd = new System.Windows.Forms.GroupBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.cb_gameDedicated = new System.Windows.Forms.CheckBox();
            this.cb_requireNova = new System.Windows.Forms.CheckBox();
            this.cb_customSkin = new System.Windows.Forms.CheckBox();
            this.cb_respawnTime = new System.Windows.Forms.ComboBox();
            this.cb_autoBalance = new System.Windows.Forms.CheckBox();
            this.cb_startDelay = new System.Windows.Forms.ComboBox();
            this.label_timeLimit = new System.Windows.Forms.Label();
            this.cb_timeLimit = new System.Windows.Forms.ComboBox();
            this.label_startDelay = new System.Windows.Forms.Label();
            this.label_replayMaps = new System.Windows.Forms.Label();
            this.label_respawnTime = new System.Windows.Forms.Label();
            this.cb_replayMaps = new System.Windows.Forms.ComboBox();
            this.gB_ServerDetails = new System.Windows.Forms.GroupBox();
            this.label_serverName = new System.Windows.Forms.Label();
            this.rcT_serverName = new System.Windows.Forms.TextBox();
            this.label_Country = new System.Windows.Forms.Label();
            this.rcCB_country = new System.Windows.Forms.ComboBox();
            this.label_ServerPassword = new System.Windows.Forms.Label();
            this.rcT_serverPassword = new System.Windows.Forms.TextBox();
            this.label_SessionType = new System.Windows.Forms.Label();
            this.rcCB_sessionType = new System.Windows.Forms.ComboBox();
            this.label_maxSlots = new System.Windows.Forms.Label();
            this.rcNum_maxSlots = new System.Windows.Forms.NumericUpDown();
            this.gB_options = new System.Windows.Forms.GroupBox();
            this.btn_UpdateServerSettings = new System.Windows.Forms.Button();
            this.btn_settingsRevertChanges = new System.Windows.Forms.Button();
            this.tabCtl_ServerSettings = new System.Windows.Forms.TabControl();
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.gb_pingOptions = new System.Windows.Forms.GroupBox();
            this.num_maxPing = new System.Windows.Forms.NumericUpDown();
            this.num_minPing = new System.Windows.Forms.NumericUpDown();
            this.cb_minPing = new System.Windows.Forms.CheckBox();
            this.cb_maxPing = new System.Windows.Forms.CheckBox();
            this.gb_misc = new System.Windows.Forms.GroupBox();
            this.cb_destroyBuildings = new System.Windows.Forms.CheckBox();
            this.cb_fatBullets = new System.Windows.Forms.CheckBox();
            this.cb_oneShotKills = new System.Windows.Forms.CheckBox();
            this.gb_GamePlay = new System.Windows.Forms.GroupBox();
            this.cb_Tracers = new System.Windows.Forms.CheckBox();
            this.label_flagReturnTimer = new System.Windows.Forms.Label();
            this.numericUpDown12 = new System.Windows.Forms.NumericUpDown();
            this.num_MaxTeamLives = new System.Windows.Forms.NumericUpDown();
            this.cb_TeamClays = new System.Windows.Forms.CheckBox();
            this.label_maxTeamLives = new System.Windows.Forms.Label();
            this.cb_AutoRange = new System.Windows.Forms.CheckBox();
            this.num_pspTimer = new System.Windows.Forms.NumericUpDown();
            this.label_PSPtime = new System.Windows.Forms.Label();
            this.gb_FriendlyFire = new System.Windows.Forms.GroupBox();
            this.checkBox40 = new System.Windows.Forms.CheckBox();
            this.cb_showFriendTags = new System.Windows.Forms.CheckBox();
            this.label_maxFriendKills = new System.Windows.Forms.Label();
            this.num_maxFriendKills = new System.Windows.Forms.NumericUpDown();
            this.cb_ffWarning = new System.Windows.Forms.CheckBox();
            this.gB_TeamPasswords = new System.Windows.Forms.GroupBox();
            this.label_bluePass = new System.Windows.Forms.Label();
            this.label_redPass = new System.Windows.Forms.Label();
            this.text_redPass = new System.Windows.Forms.TextBox();
            this.text_bluePass = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.num_scoreDM = new System.Windows.Forms.NumericUpDown();
            this.num_scoreKOTH = new System.Windows.Forms.NumericUpDown();
            this.num_scoreFB = new System.Windows.Forms.NumericUpDown();
            this.label_scoreDM = new System.Windows.Forms.Label();
            this.label_scoreKOTH = new System.Windows.Forms.Label();
            this.label_scoreFB = new System.Windows.Forms.Label();
            this.cb_autoLastKnown = new System.Windows.Forms.CheckBox();
            this.tabPage14 = new System.Windows.Forms.TabPage();
            this.groupBox22 = new System.Windows.Forms.GroupBox();
            this.rb_WeaonSelNone = new System.Windows.Forms.RadioButton();
            this.rb_WeaonSelAll = new System.Windows.Forms.RadioButton();
            this.cbl_weaponSelection = new System.Windows.Forms.CheckedListBox();
            this.gb_roleRestrictions = new System.Windows.Forms.GroupBox();
            this.rb_roleSelNone = new System.Windows.Forms.RadioButton();
            this.rb_roleSelAll = new System.Windows.Forms.RadioButton();
            this.cbl_roleSelection = new System.Windows.Forms.CheckedListBox();
            this.page_maps = new System.Windows.Forms.TabPage();
            this.gb_MapsAvailable = new System.Windows.Forms.GroupBox();
            this.label_numMapsAvailable = new System.Windows.Forms.Label();
            this.label_gameType = new System.Windows.Forms.Label();
            this.listBox_mapsAvailable = new System.Windows.Forms.ListBox();
            this.dropDown_mapSettingsGameType = new System.Windows.Forms.ComboBox();
            this.label_totalMaps = new System.Windows.Forms.Label();
            this.gB_mapRotation = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.list_mapRotation = new System.Windows.Forms.ListBox();
            this.label_mapsSelected = new System.Windows.Forms.Label();
            this.label_CurrentMap = new System.Windows.Forms.Label();
            this.label_currentMapPlaying = new System.Windows.Forms.Label();
            this.endOfMapTimer_TrackBar = new System.Windows.Forms.TrackBar();
            this.label_currentMapCount = new System.Windows.Forms.Label();
            this.label_EOMtimer = new System.Windows.Forms.Label();
            this.panel_mapControls = new System.Windows.Forms.Panel();
            this.label103 = new System.Windows.Forms.Label();
            this.label96 = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            this.btn_SaveRoation = new System.Windows.Forms.Button();
            this.btn_loadRotation = new System.Windows.Forms.Button();
            this.btn_updateMaps = new System.Windows.Forms.Button();
            this.btn_playMapNext = new System.Windows.Forms.Button();
            this.btn_scoreSkip = new System.Windows.Forms.Button();
            this.btn_moveMapDown = new System.Windows.Forms.Button();
            this.btn_moveMapUp = new System.Windows.Forms.Button();
            this.btn_ShuffleMaps = new System.Windows.Forms.Button();
            this.page_chat = new System.Windows.Forms.TabPage();
            this.panel_chatMessage = new System.Windows.Forms.Panel();
            this.chat_channelSelection = new System.Windows.Forms.ComboBox();
            this.chat_textBoxMsg = new System.Windows.Forms.TextBox();
            this.btn_sendChat = new System.Windows.Forms.Button();
            this.panel_chatControls = new System.Windows.Forms.Panel();
            this.cb_chatPlayerSelect = new System.Windows.Forms.ComboBox();
            this.groupBox_chatChannel = new System.Windows.Forms.GroupBox();
            this.rb_chatAll = new System.Windows.Forms.RadioButton();
            this.rb_chatGlobal = new System.Windows.Forms.RadioButton();
            this.rb_chatPlayerHist = new System.Windows.Forms.RadioButton();
            this.rb_chatRedTeam = new System.Windows.Forms.RadioButton();
            this.rb_chatBlueTeam = new System.Windows.Forms.RadioButton();
            this.data_chatViewer = new System.Windows.Forms.DataGridView();
            this.page_autoMessages = new System.Windows.Forms.TabPage();
            this.gb_autoMessages = new System.Windows.Forms.GroupBox();
            this.gb_addMessages = new System.Windows.Forms.GroupBox();
            this.text_newAutoMessage = new System.Windows.Forms.TextBox();
            this.btn_addAutoMsg = new System.Windows.Forms.Button();
            this.label_interval = new System.Windows.Forms.Label();
            this.listBox_AutoMessages = new System.Windows.Forms.ListBox();
            this.num_autoMsgInterval = new System.Windows.Forms.NumericUpDown();
            this.cb_enableAutoMsg = new System.Windows.Forms.CheckBox();
            this.playerList_contextMenu = new System.Windows.Forms.ContextMenuStrip();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem133 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem76 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem129 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem130 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem131 = new System.Windows.Forms.ToolStripMenuItem();
            this.cm_kickPlayer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.playerListMenu_tempBan = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_1Day = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_2Day = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_3Day = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_4Day = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_5Day = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_6Day = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_1Week = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_2Week = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenu_1Month = new System.Windows.Forms.ToolStripMenuItem();
            this.playerListMenu_permBanPlayer = new System.Windows.Forms.ToolStripMenuItem();
            this.playerListMenu_changeTeams = new System.Windows.Forms.ToolStripMenuItem();
            this.playerListMenu_spectate = new System.Windows.Forms.ToolStripMenuItem();
            this.playerListMenu_Seperator = new System.Windows.Forms.ToolStripSeparator();
            this.actionReason_Abusive = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Racism = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Cheating = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_WallHack = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Aimbot = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Speed = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Disrespect = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Camping = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_TPK = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Rules = new System.Windows.Forms.ToolStripMenuItem();
            this.actionReason_Custom = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem37 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem38 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem39 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem40 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem41 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem42 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem43 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem44 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem45 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox3 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem46 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem47 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem48 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem49 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem50 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem51 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem52 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem53 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem54 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox4 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem55 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem56 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem57 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem58 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem59 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem60 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem61 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem62 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem63 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem64 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox5 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem65 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem66 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem67 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem68 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem69 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem70 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem71 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem72 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem73 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem74 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox6 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem75 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem77 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem78 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem79 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem80 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem81 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem82 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem83 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem84 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem85 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox7 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem86 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem87 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem88 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem89 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem90 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem91 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem92 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem93 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem94 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem95 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox8 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem96 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem97 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem98 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem99 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem100 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem101 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem102 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem103 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem104 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem105 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox9 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem106 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem107 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem108 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem109 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem110 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem111 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem112 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem113 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem114 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem115 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox10 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem116 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem117 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem118 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem119 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem120 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem121 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem122 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem123 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem124 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem125 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox11 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem25 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem26 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem27 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem28 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem29 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem30 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem31 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem32 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem33 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem34 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip();
            this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem23 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem24 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem126 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem127 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem128 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox2 = new System.Windows.Forms.ToolStripTextBox();
            this.serverManager_container.SuspendLayout();
            this.page_players.SuspendLayout();
            this.group_currentPlayers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid_playerList)).BeginInit();
            this.tabControl_playerSelection.SuspendLayout();
            this.tab_bannedPlayers.SuspendLayout();
            this.group_addBan.SuspendLayout();
            this.group_banDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid_bannedPlayerList)).BeginInit();
            this.tab_vpnSettings.SuspendLayout();
            this.group_vpnWhitelist.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid_vpn_allowlist)).BeginInit();
            this.group_vpnSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.value_vpn_abuselevel)).BeginInit();
            this.tab_warnMsgs.SuspendLayout();
            this.groupBox_playerWarnMsg.SuspendLayout();
            this.page_serverSettings.SuspendLayout();
            this.gB_ServerSettings.SuspendLayout();
            this.gB_motd.SuspendLayout();
            this.gB_ServerDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rcNum_maxSlots)).BeginInit();
            this.gB_options.SuspendLayout();
            this.tabCtl_ServerSettings.SuspendLayout();
            this.tabPage9.SuspendLayout();
            this.gb_pingOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_maxPing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_minPing)).BeginInit();
            this.gb_misc.SuspendLayout();
            this.gb_GamePlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_MaxTeamLives)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_pspTimer)).BeginInit();
            this.gb_FriendlyFire.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_maxFriendKills)).BeginInit();
            this.gB_TeamPasswords.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_scoreDM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_scoreKOTH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_scoreFB)).BeginInit();
            this.tabPage14.SuspendLayout();
            this.groupBox22.SuspendLayout();
            this.gb_roleRestrictions.SuspendLayout();
            this.page_maps.SuspendLayout();
            this.gb_MapsAvailable.SuspendLayout();
            this.gB_mapRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.endOfMapTimer_TrackBar)).BeginInit();
            this.panel_mapControls.SuspendLayout();
            this.page_chat.SuspendLayout();
            this.panel_chatMessage.SuspendLayout();
            this.panel_chatControls.SuspendLayout();
            this.groupBox_chatChannel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.data_chatViewer)).BeginInit();
            this.page_autoMessages.SuspendLayout();
            this.gb_autoMessages.SuspendLayout();
            this.gb_addMessages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_autoMsgInterval)).BeginInit();
            this.playerList_contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // serverManager_container
            // 
            this.serverManager_container.Controls.Add(this.page_players);
            this.serverManager_container.Controls.Add(this.page_serverSettings);
            this.serverManager_container.Controls.Add(this.page_maps);
            this.serverManager_container.Controls.Add(this.page_chat);
            this.serverManager_container.Controls.Add(this.page_autoMessages);
            this.serverManager_container.Location = new System.Drawing.Point(12, 12);
            this.serverManager_container.Name = "serverManager_container";
            this.serverManager_container.SelectedIndex = 0;
            this.serverManager_container.Size = new System.Drawing.Size(808, 427);
            this.serverManager_container.TabIndex = 2;
            // 
            // page_players
            // 
            this.page_players.Controls.Add(this.label_currentSpec);
            this.page_players.Controls.Add(this.group_currentPlayers);
            this.page_players.Controls.Add(this.tabControl_playerSelection);
            this.page_players.Controls.Add(this.label69);
            this.page_players.Controls.Add(this.label2);
            this.page_players.Location = new System.Drawing.Point(4, 22);
            this.page_players.Name = "page_players";
            this.page_players.Padding = new System.Windows.Forms.Padding(3);
            this.page_players.Size = new System.Drawing.Size(800, 401);
            this.page_players.TabIndex = 1;
            this.page_players.Text = "Players";
            this.page_players.UseVisualStyleBackColor = true;
            // 
            // label_currentSpec
            // 
            this.label_currentSpec.AutoSize = true;
            this.label_currentSpec.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_currentSpec.ForeColor = System.Drawing.Color.Red;
            this.label_currentSpec.Location = new System.Drawing.Point(711, 12);
            this.label_currentSpec.Name = "label_currentSpec";
            this.label_currentSpec.Size = new System.Drawing.Size(72, 13);
            this.label_currentSpec.TabIndex = 9;
            this.label_currentSpec.Text = "Spectating:";
            this.label_currentSpec.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // group_currentPlayers
            // 
            this.group_currentPlayers.Controls.Add(this.grid_playerList);
            this.group_currentPlayers.Location = new System.Drawing.Point(3, 6);
            this.group_currentPlayers.Name = "group_currentPlayers";
            this.group_currentPlayers.Size = new System.Drawing.Size(333, 394);
            this.group_currentPlayers.TabIndex = 10;
            this.group_currentPlayers.TabStop = false;
            this.group_currentPlayers.Text = "Current Players: ";
            // 
            // grid_playerList
            // 
            this.grid_playerList.AllowUserToAddRows = false;
            this.grid_playerList.AllowUserToDeleteRows = false;
            this.grid_playerList.AllowUserToResizeColumns = false;
            this.grid_playerList.AllowUserToResizeRows = false;
            this.grid_playerList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_playerList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.grid_playerList.Location = new System.Drawing.Point(6, 19);
            this.grid_playerList.MultiSelect = false;
            this.grid_playerList.Name = "grid_playerList";
            this.grid_playerList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid_playerList.ShowCellErrors = false;
            this.grid_playerList.ShowCellToolTips = false;
            this.grid_playerList.ShowEditingIcon = false;
            this.grid_playerList.ShowRowErrors = false;
            this.grid_playerList.Size = new System.Drawing.Size(321, 369);
            this.grid_playerList.TabIndex = 0;
            this.grid_playerList.DoubleClick += new System.EventHandler(this.playerList_doubleClickPlayerInfo);
            this.grid_playerList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.playerList_menuToggle);
            // 
            // tabControl_playerSelection
            // 
            this.tabControl_playerSelection.Controls.Add(this.tab_bannedPlayers);
            this.tabControl_playerSelection.Controls.Add(this.tab_vpnSettings);
            this.tabControl_playerSelection.Controls.Add(this.tab_warnMsgs);
            this.tabControl_playerSelection.Location = new System.Drawing.Point(342, 12);
            this.tabControl_playerSelection.Name = "tabControl_playerSelection";
            this.tabControl_playerSelection.SelectedIndex = 0;
            this.tabControl_playerSelection.Size = new System.Drawing.Size(454, 389);
            this.tabControl_playerSelection.TabIndex = 8;
            // 
            // tab_bannedPlayers
            // 
            this.tab_bannedPlayers.Controls.Add(this.group_addBan);
            this.tab_bannedPlayers.Controls.Add(this.button_removeBan);
            this.tab_bannedPlayers.Controls.Add(this.group_banDetails);
            this.tab_bannedPlayers.Controls.Add(this.grid_bannedPlayerList);
            this.tab_bannedPlayers.Location = new System.Drawing.Point(4, 22);
            this.tab_bannedPlayers.Name = "tab_bannedPlayers";
            this.tab_bannedPlayers.Padding = new System.Windows.Forms.Padding(3);
            this.tab_bannedPlayers.Size = new System.Drawing.Size(446, 363);
            this.tab_bannedPlayers.TabIndex = 0;
            this.tab_bannedPlayers.Text = "Banned Players";
            this.tab_bannedPlayers.UseVisualStyleBackColor = true;
            // 
            // group_addBan
            // 
            this.group_addBan.Controls.Add(this.text_adPlayerName);
            this.group_addBan.Controls.Add(this.button_addBan);
            this.group_addBan.Controls.Add(this.text_abIPAddress);
            this.group_addBan.Controls.Add(this.label_abPlayerName);
            this.group_addBan.Controls.Add(this.combo_abReason);
            this.group_addBan.Controls.Add(this.label_abIPAddress);
            this.group_addBan.Location = new System.Drawing.Point(6, 283);
            this.group_addBan.Name = "group_addBan";
            this.group_addBan.Size = new System.Drawing.Size(434, 73);
            this.group_addBan.TabIndex = 4;
            this.group_addBan.TabStop = false;
            this.group_addBan.Text = "Add Ban";
            // 
            // text_adPlayerName
            // 
            this.text_adPlayerName.Location = new System.Drawing.Point(86, 18);
            this.text_adPlayerName.Name = "text_adPlayerName";
            this.text_adPlayerName.Size = new System.Drawing.Size(124, 20);
            this.text_adPlayerName.TabIndex = 4;
            // 
            // button_addBan
            // 
            this.button_addBan.Location = new System.Drawing.Point(353, 17);
            this.button_addBan.Name = "button_addBan";
            this.button_addBan.Size = new System.Drawing.Size(75, 23);
            this.button_addBan.TabIndex = 2;
            this.button_addBan.Text = "Add Ban";
            this.button_addBan.UseVisualStyleBackColor = true;
            this.button_addBan.Click += new System.EventHandler(this.banList_addEntry);
            // 
            // text_abIPAddress
            // 
            this.text_abIPAddress.Location = new System.Drawing.Point(86, 42);
            this.text_abIPAddress.Name = "text_abIPAddress";
            this.text_abIPAddress.Size = new System.Drawing.Size(124, 20);
            this.text_abIPAddress.TabIndex = 5;
            // 
            // label_abPlayerName
            // 
            this.label_abPlayerName.AutoSize = true;
            this.label_abPlayerName.Location = new System.Drawing.Point(13, 21);
            this.label_abPlayerName.Name = "label_abPlayerName";
            this.label_abPlayerName.Size = new System.Drawing.Size(67, 13);
            this.label_abPlayerName.TabIndex = 0;
            this.label_abPlayerName.Text = "Player Name";
            this.label_abPlayerName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // combo_abReason
            // 
            this.combo_abReason.FormattingEnabled = true;
            this.combo_abReason.Items.AddRange(new object[] {
            "Abusive",
            "Racism",
            "General cheating/exploits",
            "Wallhack",
            "Aimbot",
            "Speedhacking",
            "Admin disrespect",
            "Camping",
            "Team killing",
            "Breaking Server Rules"});
            this.combo_abReason.Location = new System.Drawing.Point(216, 41);
            this.combo_abReason.Name = "combo_abReason";
            this.combo_abReason.Size = new System.Drawing.Size(212, 21);
            this.combo_abReason.TabIndex = 3;
            this.combo_abReason.Text = "Select Reason or Enter Custom";
            // 
            // label_abIPAddress
            // 
            this.label_abIPAddress.AutoSize = true;
            this.label_abIPAddress.Location = new System.Drawing.Point(22, 46);
            this.label_abIPAddress.Name = "label_abIPAddress";
            this.label_abIPAddress.Size = new System.Drawing.Size(58, 13);
            this.label_abIPAddress.TabIndex = 1;
            this.label_abIPAddress.Text = "IP Address";
            this.label_abIPAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button_removeBan
            // 
            this.button_removeBan.Location = new System.Drawing.Point(343, 153);
            this.button_removeBan.Name = "button_removeBan";
            this.button_removeBan.Size = new System.Drawing.Size(97, 23);
            this.button_removeBan.TabIndex = 1;
            this.button_removeBan.Text = "Remove Ban";
            this.button_removeBan.UseVisualStyleBackColor = true;
            this.button_removeBan.Click += new System.EventHandler(this.banList_clickRemove);
            // 
            // group_banDetails
            // 
            this.group_banDetails.Controls.Add(this.value_banIPAddress);
            this.group_banDetails.Controls.Add(this.label_banIPAddress);
            this.group_banDetails.Controls.Add(this.value_banAdmin);
            this.group_banDetails.Controls.Add(this.value_banDateAdded);
            this.group_banDetails.Controls.Add(this.value_banReason);
            this.group_banDetails.Controls.Add(this.value_bdPlayerName);
            this.group_banDetails.Controls.Add(this.label_banAdmin);
            this.group_banDetails.Controls.Add(this.label_banReason);
            this.group_banDetails.Controls.Add(this.label_banDateAdded);
            this.group_banDetails.Controls.Add(this.label_bdPlayerName);
            this.group_banDetails.Location = new System.Drawing.Point(6, 177);
            this.group_banDetails.Name = "group_banDetails";
            this.group_banDetails.Size = new System.Drawing.Size(434, 100);
            this.group_banDetails.TabIndex = 3;
            this.group_banDetails.TabStop = false;
            this.group_banDetails.Text = "Ban Details";
            // 
            // value_banIPAddress
            // 
            this.value_banIPAddress.AutoSize = true;
            this.value_banIPAddress.Location = new System.Drawing.Point(83, 38);
            this.value_banIPAddress.Name = "value_banIPAddress";
            this.value_banIPAddress.Size = new System.Drawing.Size(16, 13);
            this.value_banIPAddress.TabIndex = 9;
            this.value_banIPAddress.Text = "...";
            // 
            // label_banIPAddress
            // 
            this.label_banIPAddress.AutoSize = true;
            this.label_banIPAddress.Location = new System.Drawing.Point(22, 38);
            this.label_banIPAddress.Name = "label_banIPAddress";
            this.label_banIPAddress.Size = new System.Drawing.Size(58, 13);
            this.label_banIPAddress.TabIndex = 8;
            this.label_banIPAddress.Text = "IP Address";
            this.label_banIPAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // value_banAdmin
            // 
            this.value_banAdmin.AutoSize = true;
            this.value_banAdmin.Location = new System.Drawing.Point(283, 38);
            this.value_banAdmin.Name = "value_banAdmin";
            this.value_banAdmin.Size = new System.Drawing.Size(16, 13);
            this.value_banAdmin.TabIndex = 7;
            this.value_banAdmin.Text = "...";
            // 
            // value_banDateAdded
            // 
            this.value_banDateAdded.AutoSize = true;
            this.value_banDateAdded.Location = new System.Drawing.Point(283, 19);
            this.value_banDateAdded.Name = "value_banDateAdded";
            this.value_banDateAdded.Size = new System.Drawing.Size(16, 13);
            this.value_banDateAdded.TabIndex = 6;
            this.value_banDateAdded.Text = "...";
            // 
            // value_banReason
            // 
            this.value_banReason.AutoSize = true;
            this.value_banReason.Location = new System.Drawing.Point(83, 57);
            this.value_banReason.Name = "value_banReason";
            this.value_banReason.Size = new System.Drawing.Size(16, 13);
            this.value_banReason.TabIndex = 5;
            this.value_banReason.Text = "...";
            // 
            // value_bdPlayerName
            // 
            this.value_bdPlayerName.AutoSize = true;
            this.value_bdPlayerName.Location = new System.Drawing.Point(83, 19);
            this.value_bdPlayerName.Name = "value_bdPlayerName";
            this.value_bdPlayerName.Size = new System.Drawing.Size(16, 13);
            this.value_bdPlayerName.TabIndex = 4;
            this.value_bdPlayerName.Text = "...";
            // 
            // label_banAdmin
            // 
            this.label_banAdmin.AutoSize = true;
            this.label_banAdmin.Location = new System.Drawing.Point(224, 38);
            this.label_banAdmin.Name = "label_banAdmin";
            this.label_banAdmin.Size = new System.Drawing.Size(53, 13);
            this.label_banAdmin.TabIndex = 3;
            this.label_banAdmin.Text = "Added By";
            // 
            // label_banReason
            // 
            this.label_banReason.AutoSize = true;
            this.label_banReason.Location = new System.Drawing.Point(36, 57);
            this.label_banReason.Name = "label_banReason";
            this.label_banReason.Size = new System.Drawing.Size(44, 13);
            this.label_banReason.TabIndex = 2;
            this.label_banReason.Text = "Reason";
            // 
            // label_banDateAdded
            // 
            this.label_banDateAdded.AutoSize = true;
            this.label_banDateAdded.Location = new System.Drawing.Point(213, 19);
            this.label_banDateAdded.Name = "label_banDateAdded";
            this.label_banDateAdded.Size = new System.Drawing.Size(64, 13);
            this.label_banDateAdded.TabIndex = 1;
            this.label_banDateAdded.Text = "Date Added";
            // 
            // label_bdPlayerName
            // 
            this.label_bdPlayerName.AutoSize = true;
            this.label_bdPlayerName.Location = new System.Drawing.Point(13, 19);
            this.label_bdPlayerName.Name = "label_bdPlayerName";
            this.label_bdPlayerName.Size = new System.Drawing.Size(67, 13);
            this.label_bdPlayerName.TabIndex = 0;
            this.label_bdPlayerName.Text = "Player Name";
            this.label_bdPlayerName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grid_bannedPlayerList
            // 
            this.grid_bannedPlayerList.AllowUserToAddRows = false;
            this.grid_bannedPlayerList.AllowUserToDeleteRows = false;
            this.grid_bannedPlayerList.AllowUserToResizeColumns = false;
            this.grid_bannedPlayerList.AllowUserToResizeRows = false;
            this.grid_bannedPlayerList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_bannedPlayerList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.grid_bannedPlayerList.Location = new System.Drawing.Point(8, 10);
            this.grid_bannedPlayerList.MultiSelect = false;
            this.grid_bannedPlayerList.Name = "grid_bannedPlayerList";
            this.grid_bannedPlayerList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid_bannedPlayerList.ShowCellErrors = false;
            this.grid_bannedPlayerList.ShowCellToolTips = false;
            this.grid_bannedPlayerList.ShowEditingIcon = false;
            this.grid_bannedPlayerList.ShowRowErrors = false;
            this.grid_bannedPlayerList.Size = new System.Drawing.Size(432, 137);
            this.grid_bannedPlayerList.TabIndex = 0;
            this.grid_bannedPlayerList.SelectionChanged += new System.EventHandler(this.banList_SelectionChanged);
            // 
            // tab_vpnSettings
            // 
            this.tab_vpnSettings.Controls.Add(this.group_vpnWhitelist);
            this.tab_vpnSettings.Controls.Add(this.group_vpnSettings);
            this.tab_vpnSettings.Location = new System.Drawing.Point(4, 22);
            this.tab_vpnSettings.Name = "tab_vpnSettings";
            this.tab_vpnSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tab_vpnSettings.Size = new System.Drawing.Size(446, 363);
            this.tab_vpnSettings.TabIndex = 1;
            this.tab_vpnSettings.Text = "VPN Settings";
            this.tab_vpnSettings.UseVisualStyleBackColor = true;
            this.tab_vpnSettings.Enter += new System.EventHandler(this.event_enterVPNsettings);
            // 
            // group_vpnWhitelist
            // 
            this.group_vpnWhitelist.Controls.Add(this.btn_cancelChanges);
            this.group_vpnWhitelist.Controls.Add(this.btn_deleteAddress);
            this.group_vpnWhitelist.Controls.Add(this.btn_addAddress);
            this.group_vpnWhitelist.Controls.Add(this.textBox_vpnAddress);
            this.group_vpnWhitelist.Controls.Add(this.textBox_vpnDescription);
            this.group_vpnWhitelist.Controls.Add(this.label_vpnAddress);
            this.group_vpnWhitelist.Controls.Add(this.label_vpnDescription);
            this.group_vpnWhitelist.Controls.Add(this.grid_vpn_allowlist);
            this.group_vpnWhitelist.Location = new System.Drawing.Point(5, 65);
            this.group_vpnWhitelist.Name = "group_vpnWhitelist";
            this.group_vpnWhitelist.Size = new System.Drawing.Size(437, 291);
            this.group_vpnWhitelist.TabIndex = 1;
            this.group_vpnWhitelist.TabStop = false;
            this.group_vpnWhitelist.Text = "VPN Whitelist:";
            // 
            // btn_cancelChanges
            // 
            this.btn_cancelChanges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_cancelChanges.Location = new System.Drawing.Point(340, 262);
            this.btn_cancelChanges.Name = "btn_cancelChanges";
            this.btn_cancelChanges.Size = new System.Drawing.Size(93, 23);
            this.btn_cancelChanges.TabIndex = 10;
            this.btn_cancelChanges.Text = "Cancel Changes";
            this.btn_cancelChanges.UseVisualStyleBackColor = true;
            // 
            // btn_deleteAddress
            // 
            this.btn_deleteAddress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_deleteAddress.Location = new System.Drawing.Point(179, 263);
            this.btn_deleteAddress.Name = "btn_deleteAddress";
            this.btn_deleteAddress.Size = new System.Drawing.Size(93, 23);
            this.btn_deleteAddress.TabIndex = 9;
            this.btn_deleteAddress.Text = "Delete Address";
            this.btn_deleteAddress.UseVisualStyleBackColor = true;
            // 
            // btn_addAddress
            // 
            this.btn_addAddress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_addAddress.Location = new System.Drawing.Point(179, 237);
            this.btn_addAddress.Name = "btn_addAddress";
            this.btn_addAddress.Size = new System.Drawing.Size(93, 23);
            this.btn_addAddress.TabIndex = 8;
            this.btn_addAddress.Text = "Add Address";
            this.btn_addAddress.UseVisualStyleBackColor = true;
            this.btn_addAddress.Click += new System.EventHandler(this.vpnList_addEntry);
            // 
            // textBox_vpnAddress
            // 
            this.textBox_vpnAddress.Location = new System.Drawing.Point(73, 265);
            this.textBox_vpnAddress.Name = "textBox_vpnAddress";
            this.textBox_vpnAddress.Size = new System.Drawing.Size(100, 20);
            this.textBox_vpnAddress.TabIndex = 4;
            // 
            // textBox_vpnDescription
            // 
            this.textBox_vpnDescription.Location = new System.Drawing.Point(73, 239);
            this.textBox_vpnDescription.Name = "textBox_vpnDescription";
            this.textBox_vpnDescription.Size = new System.Drawing.Size(100, 20);
            this.textBox_vpnDescription.TabIndex = 3;
            // 
            // label_vpnAddress
            // 
            this.label_vpnAddress.AutoSize = true;
            this.label_vpnAddress.Location = new System.Drawing.Point(11, 268);
            this.label_vpnAddress.Name = "label_vpnAddress";
            this.label_vpnAddress.Size = new System.Drawing.Size(58, 13);
            this.label_vpnAddress.TabIndex = 2;
            this.label_vpnAddress.Text = "IP Address";
            this.label_vpnAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_vpnDescription
            // 
            this.label_vpnDescription.AutoSize = true;
            this.label_vpnDescription.Location = new System.Drawing.Point(7, 242);
            this.label_vpnDescription.Name = "label_vpnDescription";
            this.label_vpnDescription.Size = new System.Drawing.Size(60, 13);
            this.label_vpnDescription.TabIndex = 1;
            this.label_vpnDescription.Text = "Description";
            this.label_vpnDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grid_vpn_allowlist
            // 
            this.grid_vpn_allowlist.AllowUserToAddRows = false;
            this.grid_vpn_allowlist.AllowUserToDeleteRows = false;
            this.grid_vpn_allowlist.AllowUserToResizeColumns = false;
            this.grid_vpn_allowlist.AllowUserToResizeRows = false;
            this.grid_vpn_allowlist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_vpn_allowlist.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.grid_vpn_allowlist.Location = new System.Drawing.Point(6, 19);
            this.grid_vpn_allowlist.MultiSelect = false;
            this.grid_vpn_allowlist.Name = "grid_vpn_allowlist";
            this.grid_vpn_allowlist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid_vpn_allowlist.ShowCellErrors = false;
            this.grid_vpn_allowlist.ShowCellToolTips = false;
            this.grid_vpn_allowlist.ShowEditingIcon = false;
            this.grid_vpn_allowlist.ShowRowErrors = false;
            this.grid_vpn_allowlist.Size = new System.Drawing.Size(427, 212);
            this.grid_vpn_allowlist.TabIndex = 0;
            // 
            // group_vpnSettings
            // 
            this.group_vpnSettings.Controls.Add(this.label_vpn_abuse);
            this.group_vpnSettings.Controls.Add(this.value_vpn_abuselevel);
            this.group_vpnSettings.Controls.Add(this.checkBox_vpn_disallow);
            this.group_vpnSettings.Location = new System.Drawing.Point(5, 6);
            this.group_vpnSettings.Name = "group_vpnSettings";
            this.group_vpnSettings.Size = new System.Drawing.Size(437, 58);
            this.group_vpnSettings.TabIndex = 0;
            this.group_vpnSettings.TabStop = false;
            this.group_vpnSettings.Text = "VPN Settings:";
            // 
            // label_vpn_abuse
            // 
            this.label_vpn_abuse.AutoSize = true;
            this.label_vpn_abuse.Location = new System.Drawing.Point(70, 25);
            this.label_vpn_abuse.Name = "label_vpn_abuse";
            this.label_vpn_abuse.Size = new System.Drawing.Size(133, 13);
            this.label_vpn_abuse.TabIndex = 2;
            this.label_vpn_abuse.Text = "Unfavourable Abuse Level";
            // 
            // value_vpn_abuselevel
            // 
            this.value_vpn_abuselevel.Location = new System.Drawing.Point(15, 22);
            this.value_vpn_abuselevel.Name = "value_vpn_abuselevel";
            this.value_vpn_abuselevel.Size = new System.Drawing.Size(43, 20);
            this.value_vpn_abuselevel.TabIndex = 1;
            this.value_vpn_abuselevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.value_vpn_abuselevel.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.value_vpn_abuselevel.ValueChanged += new System.EventHandler(this.vpn_changeWarnLevel);
            // 
            // checkBox_vpn_disallow
            // 
            this.checkBox_vpn_disallow.AutoSize = true;
            this.checkBox_vpn_disallow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox_vpn_disallow.Location = new System.Drawing.Point(325, 24);
            this.checkBox_vpn_disallow.Name = "checkBox_vpn_disallow";
            this.checkBox_vpn_disallow.Size = new System.Drawing.Size(95, 17);
            this.checkBox_vpn_disallow.TabIndex = 0;
            this.checkBox_vpn_disallow.Text = "Disallow VPNs";
            this.checkBox_vpn_disallow.UseVisualStyleBackColor = true;
            this.checkBox_vpn_disallow.CheckedChanged += new System.EventHandler(this.vpn_changeDisallowVPN);
            // 
            // tab_warnMsgs
            // 
            this.tab_warnMsgs.Controls.Add(this.groupBox_playerWarnMsg);
            this.tab_warnMsgs.Location = new System.Drawing.Point(4, 22);
            this.tab_warnMsgs.Name = "tab_warnMsgs";
            this.tab_warnMsgs.Padding = new System.Windows.Forms.Padding(3);
            this.tab_warnMsgs.Size = new System.Drawing.Size(446, 363);
            this.tab_warnMsgs.TabIndex = 2;
            this.tab_warnMsgs.Text = "Warning Messages";
            this.tab_warnMsgs.UseVisualStyleBackColor = true;
            // 
            // groupBox_playerWarnMsg
            // 
            this.groupBox_playerWarnMsg.Controls.Add(this.btn_playerWarnMessageAdd);
            this.groupBox_playerWarnMsg.Controls.Add(this.textBox_playerWarnMessageAdd);
            this.groupBox_playerWarnMsg.Controls.Add(this.listBox_playerWarnMessages);
            this.groupBox_playerWarnMsg.Location = new System.Drawing.Point(3, 3);
            this.groupBox_playerWarnMsg.Name = "groupBox_playerWarnMsg";
            this.groupBox_playerWarnMsg.Size = new System.Drawing.Size(440, 357);
            this.groupBox_playerWarnMsg.TabIndex = 0;
            this.groupBox_playerWarnMsg.TabStop = false;
            this.groupBox_playerWarnMsg.Text = "Player Warning Messages";
            // 
            // btn_playerWarnMessageAdd
            // 
            this.btn_playerWarnMessageAdd.Location = new System.Drawing.Point(347, 330);
            this.btn_playerWarnMessageAdd.Name = "btn_playerWarnMessageAdd";
            this.btn_playerWarnMessageAdd.Size = new System.Drawing.Size(90, 21);
            this.btn_playerWarnMessageAdd.TabIndex = 3;
            this.btn_playerWarnMessageAdd.Text = "Add Message";
            this.btn_playerWarnMessageAdd.UseVisualStyleBackColor = true;
            this.btn_playerWarnMessageAdd.Click += new System.EventHandler(this.playerWarn_addWarnClick);
            // 
            // textBox_playerWarnMessageAdd
            // 
            this.textBox_playerWarnMessageAdd.Location = new System.Drawing.Point(6, 331);
            this.textBox_playerWarnMessageAdd.Name = "textBox_playerWarnMessageAdd";
            this.textBox_playerWarnMessageAdd.Size = new System.Drawing.Size(338, 20);
            this.textBox_playerWarnMessageAdd.TabIndex = 2;
            // 
            // listBox_playerWarnMessages
            // 
            this.listBox_playerWarnMessages.FormattingEnabled = true;
            this.listBox_playerWarnMessages.Location = new System.Drawing.Point(6, 19);
            this.listBox_playerWarnMessages.Name = "listBox_playerWarnMessages";
            this.listBox_playerWarnMessages.Size = new System.Drawing.Size(428, 303);
            this.listBox_playerWarnMessages.TabIndex = 0;
            this.listBox_playerWarnMessages.DoubleClick += new System.EventHandler(this.playerWarn_doubleClick);
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.Location = new System.Drawing.Point(217, 6);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(0, 13);
            this.label69.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 1;
            // 
            // page_serverSettings
            // 
            this.page_serverSettings.Controls.Add(this.gB_ServerSettings);
            this.page_serverSettings.Controls.Add(this.gB_ServerDetails);
            this.page_serverSettings.Controls.Add(this.gB_options);
            this.page_serverSettings.Controls.Add(this.tabCtl_ServerSettings);
            this.page_serverSettings.Location = new System.Drawing.Point(4, 22);
            this.page_serverSettings.Name = "page_serverSettings";
            this.page_serverSettings.Padding = new System.Windows.Forms.Padding(3);
            this.page_serverSettings.Size = new System.Drawing.Size(800, 401);
            this.page_serverSettings.TabIndex = 2;
            this.page_serverSettings.Text = "Server Settings";
            this.page_serverSettings.UseVisualStyleBackColor = true;
            // 
            // gB_ServerSettings
            // 
            this.gB_ServerSettings.Controls.Add(this.gB_motd);
            this.gB_ServerSettings.Controls.Add(this.cb_gameDedicated);
            this.gB_ServerSettings.Controls.Add(this.cb_requireNova);
            this.gB_ServerSettings.Controls.Add(this.cb_customSkin);
            this.gB_ServerSettings.Controls.Add(this.cb_respawnTime);
            this.gB_ServerSettings.Controls.Add(this.cb_autoBalance);
            this.gB_ServerSettings.Controls.Add(this.cb_startDelay);
            this.gB_ServerSettings.Controls.Add(this.label_timeLimit);
            this.gB_ServerSettings.Controls.Add(this.cb_timeLimit);
            this.gB_ServerSettings.Controls.Add(this.label_startDelay);
            this.gB_ServerSettings.Controls.Add(this.label_replayMaps);
            this.gB_ServerSettings.Controls.Add(this.label_respawnTime);
            this.gB_ServerSettings.Controls.Add(this.cb_replayMaps);
            this.gB_ServerSettings.Location = new System.Drawing.Point(4, 118);
            this.gB_ServerSettings.Name = "gB_ServerSettings";
            this.gB_ServerSettings.Size = new System.Drawing.Size(380, 215);
            this.gB_ServerSettings.TabIndex = 38;
            this.gB_ServerSettings.TabStop = false;
            this.gB_ServerSettings.Text = "Server Settings";
            // 
            // gB_motd
            // 
            this.gB_motd.Controls.Add(this.richTextBox1);
            this.gB_motd.Location = new System.Drawing.Point(0, 128);
            this.gB_motd.Name = "gB_motd";
            this.gB_motd.Size = new System.Drawing.Size(380, 87);
            this.gB_motd.TabIndex = 36;
            this.gB_motd.TabStop = false;
            this.gB_motd.Text = "Message of The Day";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(5, 16);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(370, 65);
            this.richTextBox1.TabIndex = 14;
            this.richTextBox1.Text = "";
            // 
            // cb_gameDedicated
            // 
            this.cb_gameDedicated.AutoSize = true;
            this.cb_gameDedicated.Enabled = false;
            this.cb_gameDedicated.Location = new System.Drawing.Point(252, 17);
            this.cb_gameDedicated.Name = "cb_gameDedicated";
            this.cb_gameDedicated.Size = new System.Drawing.Size(75, 17);
            this.cb_gameDedicated.TabIndex = 10;
            this.cb_gameDedicated.Text = "Dedicated";
            this.cb_gameDedicated.UseVisualStyleBackColor = true;
            // 
            // cb_requireNova
            // 
            this.cb_requireNova.AutoSize = true;
            this.cb_requireNova.Location = new System.Drawing.Point(252, 34);
            this.cb_requireNova.Name = "cb_requireNova";
            this.cb_requireNova.Size = new System.Drawing.Size(121, 17);
            this.cb_requireNova.TabIndex = 11;
            this.cb_requireNova.Text = "Require Nova Login";
            this.cb_requireNova.UseVisualStyleBackColor = true;
            // 
            // cb_customSkin
            // 
            this.cb_customSkin.AutoSize = true;
            this.cb_customSkin.Location = new System.Drawing.Point(252, 51);
            this.cb_customSkin.Name = "cb_customSkin";
            this.cb_customSkin.Size = new System.Drawing.Size(118, 17);
            this.cb_customSkin.TabIndex = 12;
            this.cb_customSkin.Text = "Allow Custom Skins";
            this.cb_customSkin.UseVisualStyleBackColor = true;
            // 
            // cb_respawnTime
            // 
            this.cb_respawnTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_respawnTime.FormattingEnabled = true;
            this.cb_respawnTime.Items.AddRange(new object[] {
            "No Delay",
            "1 Minute",
            "2 Minutes",
            "3 Minutes"});
            this.cb_respawnTime.Location = new System.Drawing.Point(94, 100);
            this.cb_respawnTime.Name = "cb_respawnTime";
            this.cb_respawnTime.Size = new System.Drawing.Size(130, 21);
            this.cb_respawnTime.TabIndex = 9;
            // 
            // cb_autoBalance
            // 
            this.cb_autoBalance.AutoSize = true;
            this.cb_autoBalance.Location = new System.Drawing.Point(252, 68);
            this.cb_autoBalance.Name = "cb_autoBalance";
            this.cb_autoBalance.Size = new System.Drawing.Size(125, 17);
            this.cb_autoBalance.TabIndex = 13;
            this.cb_autoBalance.Text = "Auto Balance Teams";
            this.cb_autoBalance.UseVisualStyleBackColor = true;
            // 
            // cb_startDelay
            // 
            this.cb_startDelay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_startDelay.FormattingEnabled = true;
            this.cb_startDelay.Items.AddRange(new object[] {
            "No Delay",
            "1 Minute",
            "2 Minutes",
            "3 Minutes"});
            this.cb_startDelay.Location = new System.Drawing.Point(94, 46);
            this.cb_startDelay.Name = "cb_startDelay";
            this.cb_startDelay.Size = new System.Drawing.Size(130, 21);
            this.cb_startDelay.TabIndex = 7;
            // 
            // label_timeLimit
            // 
            this.label_timeLimit.AutoSize = true;
            this.label_timeLimit.Location = new System.Drawing.Point(7, 22);
            this.label_timeLimit.Name = "label_timeLimit";
            this.label_timeLimit.Size = new System.Drawing.Size(57, 13);
            this.label_timeLimit.TabIndex = 11;
            this.label_timeLimit.Text = "Time Limit:";
            // 
            // cb_timeLimit
            // 
            this.cb_timeLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_timeLimit.FormattingEnabled = true;
            this.cb_timeLimit.Location = new System.Drawing.Point(94, 19);
            this.cb_timeLimit.Name = "cb_timeLimit";
            this.cb_timeLimit.Size = new System.Drawing.Size(130, 21);
            this.cb_timeLimit.TabIndex = 6;
            // 
            // label_startDelay
            // 
            this.label_startDelay.AutoSize = true;
            this.label_startDelay.Location = new System.Drawing.Point(7, 49);
            this.label_startDelay.Name = "label_startDelay";
            this.label_startDelay.Size = new System.Drawing.Size(62, 13);
            this.label_startDelay.TabIndex = 12;
            this.label_startDelay.Text = "Start Delay:";
            // 
            // label_replayMaps
            // 
            this.label_replayMaps.AutoSize = true;
            this.label_replayMaps.Location = new System.Drawing.Point(7, 76);
            this.label_replayMaps.Name = "label_replayMaps";
            this.label_replayMaps.Size = new System.Drawing.Size(72, 13);
            this.label_replayMaps.TabIndex = 13;
            this.label_replayMaps.Text = "Replay Maps:";
            // 
            // label_respawnTime
            // 
            this.label_respawnTime.AutoSize = true;
            this.label_respawnTime.Location = new System.Drawing.Point(7, 103);
            this.label_respawnTime.Name = "label_respawnTime";
            this.label_respawnTime.Size = new System.Drawing.Size(81, 13);
            this.label_respawnTime.TabIndex = 14;
            this.label_respawnTime.Text = "Respawn Time:";
            // 
            // cb_replayMaps
            // 
            this.cb_replayMaps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_replayMaps.FormattingEnabled = true;
            this.cb_replayMaps.Items.AddRange(new object[] {
            "No",
            "Yes",
            "Cycle"});
            this.cb_replayMaps.Location = new System.Drawing.Point(94, 73);
            this.cb_replayMaps.Name = "cb_replayMaps";
            this.cb_replayMaps.Size = new System.Drawing.Size(130, 21);
            this.cb_replayMaps.TabIndex = 8;
            // 
            // gB_ServerDetails
            // 
            this.gB_ServerDetails.Controls.Add(this.label_serverName);
            this.gB_ServerDetails.Controls.Add(this.rcT_serverName);
            this.gB_ServerDetails.Controls.Add(this.label_Country);
            this.gB_ServerDetails.Controls.Add(this.rcCB_country);
            this.gB_ServerDetails.Controls.Add(this.label_ServerPassword);
            this.gB_ServerDetails.Controls.Add(this.rcT_serverPassword);
            this.gB_ServerDetails.Controls.Add(this.label_SessionType);
            this.gB_ServerDetails.Controls.Add(this.rcCB_sessionType);
            this.gB_ServerDetails.Controls.Add(this.label_maxSlots);
            this.gB_ServerDetails.Controls.Add(this.rcNum_maxSlots);
            this.gB_ServerDetails.Location = new System.Drawing.Point(4, 4);
            this.gB_ServerDetails.Name = "gB_ServerDetails";
            this.gB_ServerDetails.Size = new System.Drawing.Size(380, 107);
            this.gB_ServerDetails.TabIndex = 37;
            this.gB_ServerDetails.TabStop = false;
            this.gB_ServerDetails.Text = "Server Details";
            // 
            // label_serverName
            // 
            this.label_serverName.AutoSize = true;
            this.label_serverName.Location = new System.Drawing.Point(7, 20);
            this.label_serverName.Name = "label_serverName";
            this.label_serverName.Size = new System.Drawing.Size(72, 13);
            this.label_serverName.TabIndex = 1;
            this.label_serverName.Text = "Server Name:";
            // 
            // rcT_serverName
            // 
            this.rcT_serverName.Location = new System.Drawing.Point(85, 17);
            this.rcT_serverName.Name = "rcT_serverName";
            this.rcT_serverName.Size = new System.Drawing.Size(130, 20);
            this.rcT_serverName.TabIndex = 1;
            // 
            // label_Country
            // 
            this.label_Country.AutoSize = true;
            this.label_Country.Location = new System.Drawing.Point(7, 46);
            this.label_Country.Name = "label_Country";
            this.label_Country.Size = new System.Drawing.Size(46, 13);
            this.label_Country.TabIndex = 5;
            this.label_Country.Text = "Country:";
            // 
            // rcCB_country
            // 
            this.rcCB_country.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rcCB_country.FormattingEnabled = true;
            this.rcCB_country.Location = new System.Drawing.Point(85, 46);
            this.rcCB_country.Name = "rcCB_country";
            this.rcCB_country.Size = new System.Drawing.Size(130, 21);
            this.rcCB_country.TabIndex = 2;
            // 
            // label_ServerPassword
            // 
            this.label_ServerPassword.AutoSize = true;
            this.label_ServerPassword.Location = new System.Drawing.Point(7, 76);
            this.label_ServerPassword.Name = "label_ServerPassword";
            this.label_ServerPassword.Size = new System.Drawing.Size(56, 13);
            this.label_ServerPassword.TabIndex = 9;
            this.label_ServerPassword.Text = "Password:";
            // 
            // rcT_serverPassword
            // 
            this.rcT_serverPassword.Enabled = false;
            this.rcT_serverPassword.Location = new System.Drawing.Point(85, 73);
            this.rcT_serverPassword.Name = "rcT_serverPassword";
            this.rcT_serverPassword.ReadOnly = true;
            this.rcT_serverPassword.Size = new System.Drawing.Size(130, 20);
            this.rcT_serverPassword.TabIndex = 3;
            // 
            // label_SessionType
            // 
            this.label_SessionType.AutoSize = true;
            this.label_SessionType.Location = new System.Drawing.Point(221, 20);
            this.label_SessionType.Name = "label_SessionType";
            this.label_SessionType.Size = new System.Drawing.Size(74, 13);
            this.label_SessionType.TabIndex = 7;
            this.label_SessionType.Text = "Session Type:";
            // 
            // rcCB_sessionType
            // 
            this.rcCB_sessionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rcCB_sessionType.FormattingEnabled = true;
            this.rcCB_sessionType.Items.AddRange(new object[] {
            "NovaWorld",
            "LAN"});
            this.rcCB_sessionType.Location = new System.Drawing.Point(301, 17);
            this.rcCB_sessionType.Name = "rcCB_sessionType";
            this.rcCB_sessionType.Size = new System.Drawing.Size(72, 21);
            this.rcCB_sessionType.TabIndex = 4;
            // 
            // label_maxSlots
            // 
            this.label_maxSlots.AutoSize = true;
            this.label_maxSlots.Location = new System.Drawing.Point(221, 49);
            this.label_maxSlots.Name = "label_maxSlots";
            this.label_maxSlots.Size = new System.Drawing.Size(53, 13);
            this.label_maxSlots.TabIndex = 3;
            this.label_maxSlots.Text = "Max Slots";
            // 
            // rcNum_maxSlots
            // 
            this.rcNum_maxSlots.Location = new System.Drawing.Point(301, 47);
            this.rcNum_maxSlots.Name = "rcNum_maxSlots";
            this.rcNum_maxSlots.Size = new System.Drawing.Size(73, 20);
            this.rcNum_maxSlots.TabIndex = 5;
            // 
            // gB_options
            // 
            this.gB_options.Controls.Add(this.btn_UpdateServerSettings);
            this.gB_options.Controls.Add(this.btn_settingsRevertChanges);
            this.gB_options.Location = new System.Drawing.Point(4, 340);
            this.gB_options.Name = "gB_options";
            this.gB_options.Size = new System.Drawing.Size(380, 57);
            this.gB_options.TabIndex = 36;
            this.gB_options.TabStop = false;
            this.gB_options.Text = "Options";
            // 
            // btn_UpdateServerSettings
            // 
            this.btn_UpdateServerSettings.Location = new System.Drawing.Point(6, 19);
            this.btn_UpdateServerSettings.Name = "btn_UpdateServerSettings";
            this.btn_UpdateServerSettings.Size = new System.Drawing.Size(129, 23);
            this.btn_UpdateServerSettings.TabIndex = 38;
            this.btn_UpdateServerSettings.Text = "Update Server Settings";
            this.btn_UpdateServerSettings.UseVisualStyleBackColor = true;
            this.btn_UpdateServerSettings.Click += new System.EventHandler(this.serverSettings_updateSettingsClick);
            // 
            // btn_settingsRevertChanges
            // 
            this.btn_settingsRevertChanges.Location = new System.Drawing.Point(141, 19);
            this.btn_settingsRevertChanges.Name = "btn_settingsRevertChanges";
            this.btn_settingsRevertChanges.Size = new System.Drawing.Size(105, 23);
            this.btn_settingsRevertChanges.TabIndex = 39;
            this.btn_settingsRevertChanges.Text = "Revert Changes";
            this.btn_settingsRevertChanges.UseVisualStyleBackColor = true;
            this.btn_settingsRevertChanges.Click += new System.EventHandler(this.settingsRevertChanges_click);
            // 
            // tabCtl_ServerSettings
            // 
            this.tabCtl_ServerSettings.Controls.Add(this.tabPage9);
            this.tabCtl_ServerSettings.Controls.Add(this.tabPage14);
            this.tabCtl_ServerSettings.ItemSize = new System.Drawing.Size(84, 18);
            this.tabCtl_ServerSettings.Location = new System.Drawing.Point(392, 4);
            this.tabCtl_ServerSettings.Name = "tabCtl_ServerSettings";
            this.tabCtl_ServerSettings.SelectedIndex = 0;
            this.tabCtl_ServerSettings.Size = new System.Drawing.Size(405, 393);
            this.tabCtl_ServerSettings.TabIndex = 0;
            // 
            // tabPage9
            // 
            this.tabPage9.Controls.Add(this.gb_pingOptions);
            this.tabPage9.Controls.Add(this.gb_misc);
            this.tabPage9.Controls.Add(this.gb_GamePlay);
            this.tabPage9.Controls.Add(this.gb_FriendlyFire);
            this.tabPage9.Controls.Add(this.gB_TeamPasswords);
            this.tabPage9.Controls.Add(this.groupBox6);
            this.tabPage9.Controls.Add(this.cb_autoLastKnown);
            this.tabPage9.Location = new System.Drawing.Point(4, 22);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage9.Size = new System.Drawing.Size(397, 367);
            this.tabPage9.TabIndex = 0;
            this.tabPage9.Text = "Server Settings";
            this.tabPage9.UseVisualStyleBackColor = true;
            // 
            // gb_pingOptions
            // 
            this.gb_pingOptions.Controls.Add(this.num_maxPing);
            this.gb_pingOptions.Controls.Add(this.num_minPing);
            this.gb_pingOptions.Controls.Add(this.cb_minPing);
            this.gb_pingOptions.Controls.Add(this.cb_maxPing);
            this.gb_pingOptions.Location = new System.Drawing.Point(203, 6);
            this.gb_pingOptions.Name = "gb_pingOptions";
            this.gb_pingOptions.Size = new System.Drawing.Size(188, 75);
            this.gb_pingOptions.TabIndex = 7;
            this.gb_pingOptions.TabStop = false;
            this.gb_pingOptions.Text = "Ping Options:";
            // 
            // num_maxPing
            // 
            this.num_maxPing.Location = new System.Drawing.Point(116, 43);
            this.num_maxPing.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.num_maxPing.Name = "num_maxPing";
            this.num_maxPing.Size = new System.Drawing.Size(56, 20);
            this.num_maxPing.TabIndex = 20;
            this.num_maxPing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // num_minPing
            // 
            this.num_minPing.Location = new System.Drawing.Point(116, 19);
            this.num_minPing.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.num_minPing.Name = "num_minPing";
            this.num_minPing.Size = new System.Drawing.Size(56, 20);
            this.num_minPing.TabIndex = 18;
            this.num_minPing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cb_minPing
            // 
            this.cb_minPing.AutoSize = true;
            this.cb_minPing.Location = new System.Drawing.Point(22, 22);
            this.cb_minPing.Name = "cb_minPing";
            this.cb_minPing.Size = new System.Drawing.Size(83, 17);
            this.cb_minPing.TabIndex = 17;
            this.cb_minPing.Text = "Minium Ping";
            this.cb_minPing.UseVisualStyleBackColor = true;
            this.cb_minPing.CheckedChanged += new System.EventHandler(this.server_enforceMinPing);
            this.cb_minPing.CheckStateChanged += new System.EventHandler(this.server_enforceMinPing);
            // 
            // cb_maxPing
            // 
            this.cb_maxPing.AutoSize = true;
            this.cb_maxPing.Location = new System.Drawing.Point(22, 45);
            this.cb_maxPing.Name = "cb_maxPing";
            this.cb_maxPing.Size = new System.Drawing.Size(94, 17);
            this.cb_maxPing.TabIndex = 19;
            this.cb_maxPing.Text = "Maximum Ping";
            this.cb_maxPing.UseVisualStyleBackColor = true;
            this.cb_maxPing.CheckedChanged += new System.EventHandler(this.server_enforceMaxPing);
            // 
            // gb_misc
            // 
            this.gb_misc.Controls.Add(this.cb_destroyBuildings);
            this.gb_misc.Controls.Add(this.cb_fatBullets);
            this.gb_misc.Controls.Add(this.cb_oneShotKills);
            this.gb_misc.Location = new System.Drawing.Point(203, 244);
            this.gb_misc.Name = "gb_misc";
            this.gb_misc.Size = new System.Drawing.Size(188, 94);
            this.gb_misc.TabIndex = 8;
            this.gb_misc.TabStop = false;
            this.gb_misc.Text = "Miscellaneous Options";
            // 
            // cb_destroyBuildings
            // 
            this.cb_destroyBuildings.AutoSize = true;
            this.cb_destroyBuildings.Location = new System.Drawing.Point(22, 63);
            this.cb_destroyBuildings.Name = "cb_destroyBuildings";
            this.cb_destroyBuildings.Size = new System.Drawing.Size(107, 17);
            this.cb_destroyBuildings.TabIndex = 36;
            this.cb_destroyBuildings.Text = "Destroy Buildings";
            this.cb_destroyBuildings.UseVisualStyleBackColor = true;
            // 
            // cb_fatBullets
            // 
            this.cb_fatBullets.AutoSize = true;
            this.cb_fatBullets.Location = new System.Drawing.Point(22, 42);
            this.cb_fatBullets.Name = "cb_fatBullets";
            this.cb_fatBullets.Size = new System.Drawing.Size(75, 17);
            this.cb_fatBullets.TabIndex = 35;
            this.cb_fatBullets.Text = "Fat Bullets";
            this.cb_fatBullets.UseVisualStyleBackColor = true;
            // 
            // cb_oneShotKills
            // 
            this.cb_oneShotKills.AutoSize = true;
            this.cb_oneShotKills.Location = new System.Drawing.Point(22, 21);
            this.cb_oneShotKills.Name = "cb_oneShotKills";
            this.cb_oneShotKills.Size = new System.Drawing.Size(92, 17);
            this.cb_oneShotKills.TabIndex = 34;
            this.cb_oneShotKills.Text = "One Shot Kills";
            this.cb_oneShotKills.UseVisualStyleBackColor = true;
            // 
            // gb_GamePlay
            // 
            this.gb_GamePlay.Controls.Add(this.cb_Tracers);
            this.gb_GamePlay.Controls.Add(this.label_flagReturnTimer);
            this.gb_GamePlay.Controls.Add(this.numericUpDown12);
            this.gb_GamePlay.Controls.Add(this.num_MaxTeamLives);
            this.gb_GamePlay.Controls.Add(this.cb_TeamClays);
            this.gb_GamePlay.Controls.Add(this.label_maxTeamLives);
            this.gb_GamePlay.Controls.Add(this.cb_AutoRange);
            this.gb_GamePlay.Controls.Add(this.num_pspTimer);
            this.gb_GamePlay.Controls.Add(this.label_PSPtime);
            this.gb_GamePlay.Location = new System.Drawing.Point(6, 151);
            this.gb_GamePlay.Name = "gb_GamePlay";
            this.gb_GamePlay.Size = new System.Drawing.Size(385, 90);
            this.gb_GamePlay.TabIndex = 28;
            this.gb_GamePlay.TabStop = false;
            this.gb_GamePlay.Text = "Game Play:";
            // 
            // cb_Tracers
            // 
            this.cb_Tracers.AutoSize = true;
            this.cb_Tracers.Location = new System.Drawing.Point(219, 19);
            this.cb_Tracers.Name = "cb_Tracers";
            this.cb_Tracers.Size = new System.Drawing.Size(92, 17);
            this.cb_Tracers.TabIndex = 28;
            this.cb_Tracers.Text = "Show Tracers";
            this.cb_Tracers.UseVisualStyleBackColor = true;
            // 
            // label_flagReturnTimer
            // 
            this.label_flagReturnTimer.AutoSize = true;
            this.label_flagReturnTimer.Location = new System.Drawing.Point(18, 41);
            this.label_flagReturnTimer.Name = "label_flagReturnTimer";
            this.label_flagReturnTimer.Size = new System.Drawing.Size(88, 13);
            this.label_flagReturnTimer.TabIndex = 16;
            this.label_flagReturnTimer.Text = "Flag Return Time";
            // 
            // numericUpDown12
            // 
            this.numericUpDown12.Location = new System.Drawing.Point(116, 39);
            this.numericUpDown12.Name = "numericUpDown12";
            this.numericUpDown12.Size = new System.Drawing.Size(48, 20);
            this.numericUpDown12.TabIndex = 26;
            this.numericUpDown12.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // num_MaxTeamLives
            // 
            this.num_MaxTeamLives.Location = new System.Drawing.Point(116, 63);
            this.num_MaxTeamLives.Name = "num_MaxTeamLives";
            this.num_MaxTeamLives.Size = new System.Drawing.Size(48, 20);
            this.num_MaxTeamLives.TabIndex = 27;
            this.num_MaxTeamLives.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cb_TeamClays
            // 
            this.cb_TeamClays.AutoSize = true;
            this.cb_TeamClays.Location = new System.Drawing.Point(219, 40);
            this.cb_TeamClays.Name = "cb_TeamClays";
            this.cb_TeamClays.Size = new System.Drawing.Size(134, 17);
            this.cb_TeamClays.TabIndex = 29;
            this.cb_TeamClays.Text = "Show Team Claymores";
            this.cb_TeamClays.UseVisualStyleBackColor = true;
            // 
            // label_maxTeamLives
            // 
            this.label_maxTeamLives.AutoSize = true;
            this.label_maxTeamLives.Location = new System.Drawing.Point(21, 64);
            this.label_maxTeamLives.Name = "label_maxTeamLives";
            this.label_maxTeamLives.Size = new System.Drawing.Size(85, 13);
            this.label_maxTeamLives.TabIndex = 17;
            this.label_maxTeamLives.Text = "Max Team Lives";
            // 
            // cb_AutoRange
            // 
            this.cb_AutoRange.AutoSize = true;
            this.cb_AutoRange.Location = new System.Drawing.Point(219, 61);
            this.cb_AutoRange.Name = "cb_AutoRange";
            this.cb_AutoRange.Size = new System.Drawing.Size(111, 17);
            this.cb_AutoRange.TabIndex = 30;
            this.cb_AutoRange.Text = "Allow Auto Range";
            this.cb_AutoRange.UseVisualStyleBackColor = true;
            // 
            // num_pspTimer
            // 
            this.num_pspTimer.Location = new System.Drawing.Point(116, 14);
            this.num_pspTimer.Name = "num_pspTimer";
            this.num_pspTimer.Size = new System.Drawing.Size(48, 20);
            this.num_pspTimer.TabIndex = 25;
            this.num_pspTimer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_PSPtime
            // 
            this.label_PSPtime.AutoSize = true;
            this.label_PSPtime.Location = new System.Drawing.Point(3, 16);
            this.label_PSPtime.Name = "label_PSPtime";
            this.label_PSPtime.Size = new System.Drawing.Size(103, 13);
            this.label_PSPtime.TabIndex = 15;
            this.label_PSPtime.Text = "PSP Takeover Time";
            // 
            // gb_FriendlyFire
            // 
            this.gb_FriendlyFire.Controls.Add(this.checkBox40);
            this.gb_FriendlyFire.Controls.Add(this.cb_showFriendTags);
            this.gb_FriendlyFire.Controls.Add(this.label_maxFriendKills);
            this.gb_FriendlyFire.Controls.Add(this.num_maxFriendKills);
            this.gb_FriendlyFire.Controls.Add(this.cb_ffWarning);
            this.gb_FriendlyFire.Location = new System.Drawing.Point(6, 82);
            this.gb_FriendlyFire.Name = "gb_FriendlyFire";
            this.gb_FriendlyFire.Size = new System.Drawing.Size(385, 68);
            this.gb_FriendlyFire.TabIndex = 27;
            this.gb_FriendlyFire.TabStop = false;
            this.gb_FriendlyFire.Text = "Friendly Fire:";
            // 
            // checkBox40
            // 
            this.checkBox40.AutoSize = true;
            this.checkBox40.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox40.Location = new System.Drawing.Point(21, 19);
            this.checkBox40.Name = "checkBox40";
            this.checkBox40.Size = new System.Drawing.Size(142, 17);
            this.checkBox40.TabIndex = 21;
            this.checkBox40.Text = "Friendly Fire Kills             ";
            this.checkBox40.UseVisualStyleBackColor = true;
            // 
            // cb_showFriendTags
            // 
            this.cb_showFriendTags.AutoSize = true;
            this.cb_showFriendTags.Location = new System.Drawing.Point(219, 17);
            this.cb_showFriendTags.Name = "cb_showFriendTags";
            this.cb_showFriendTags.Size = new System.Drawing.Size(119, 17);
            this.cb_showFriendTags.TabIndex = 23;
            this.cb_showFriendTags.Text = "Show Friendly Tags";
            this.cb_showFriendTags.UseVisualStyleBackColor = true;
            // 
            // label_maxFriendKills
            // 
            this.label_maxFriendKills.AutoSize = true;
            this.label_maxFriendKills.Location = new System.Drawing.Point(19, 42);
            this.label_maxFriendKills.Name = "label_maxFriendKills";
            this.label_maxFriendKills.Size = new System.Drawing.Size(87, 13);
            this.label_maxFriendKills.TabIndex = 10;
            this.label_maxFriendKills.Text = "Max Friendly Kills";
            // 
            // num_maxFriendKills
            // 
            this.num_maxFriendKills.Location = new System.Drawing.Point(116, 40);
            this.num_maxFriendKills.Name = "num_maxFriendKills";
            this.num_maxFriendKills.Size = new System.Drawing.Size(48, 20);
            this.num_maxFriendKills.TabIndex = 22;
            this.num_maxFriendKills.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cb_ffWarning
            // 
            this.cb_ffWarning.AutoSize = true;
            this.cb_ffWarning.Location = new System.Drawing.Point(219, 39);
            this.cb_ffWarning.Name = "cb_ffWarning";
            this.cb_ffWarning.Size = new System.Drawing.Size(125, 17);
            this.cb_ffWarning.TabIndex = 24;
            this.cb_ffWarning.Text = "Friendly Fire Warning";
            this.cb_ffWarning.UseVisualStyleBackColor = true;
            // 
            // gB_TeamPasswords
            // 
            this.gB_TeamPasswords.Controls.Add(this.label_bluePass);
            this.gB_TeamPasswords.Controls.Add(this.label_redPass);
            this.gB_TeamPasswords.Controls.Add(this.text_redPass);
            this.gB_TeamPasswords.Controls.Add(this.text_bluePass);
            this.gB_TeamPasswords.Location = new System.Drawing.Point(6, 6);
            this.gB_TeamPasswords.Name = "gB_TeamPasswords";
            this.gB_TeamPasswords.Size = new System.Drawing.Size(191, 75);
            this.gB_TeamPasswords.TabIndex = 26;
            this.gB_TeamPasswords.TabStop = false;
            this.gB_TeamPasswords.Text = "Team Passwords:";
            // 
            // label_bluePass
            // 
            this.label_bluePass.AutoSize = true;
            this.label_bluePass.Location = new System.Drawing.Point(9, 20);
            this.label_bluePass.Name = "label_bluePass";
            this.label_bluePass.Size = new System.Drawing.Size(58, 13);
            this.label_bluePass.TabIndex = 1;
            this.label_bluePass.Text = "Blue Team";
            // 
            // label_redPass
            // 
            this.label_redPass.AutoSize = true;
            this.label_redPass.Location = new System.Drawing.Point(10, 45);
            this.label_redPass.Name = "label_redPass";
            this.label_redPass.Size = new System.Drawing.Size(57, 13);
            this.label_redPass.TabIndex = 2;
            this.label_redPass.Text = "Red Team";
            // 
            // text_redPass
            // 
            this.text_redPass.Location = new System.Drawing.Point(69, 42);
            this.text_redPass.Name = "text_redPass";
            this.text_redPass.Size = new System.Drawing.Size(116, 20);
            this.text_redPass.TabIndex = 16;
            // 
            // text_bluePass
            // 
            this.text_bluePass.Location = new System.Drawing.Point(69, 16);
            this.text_bluePass.Name = "text_bluePass";
            this.text_bluePass.Size = new System.Drawing.Size(116, 20);
            this.text_bluePass.TabIndex = 15;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.num_scoreDM);
            this.groupBox6.Controls.Add(this.num_scoreKOTH);
            this.groupBox6.Controls.Add(this.num_scoreFB);
            this.groupBox6.Controls.Add(this.label_scoreDM);
            this.groupBox6.Controls.Add(this.label_scoreKOTH);
            this.groupBox6.Controls.Add(this.label_scoreFB);
            this.groupBox6.Location = new System.Drawing.Point(6, 244);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(191, 94);
            this.groupBox6.TabIndex = 25;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Scoring Options:";
            // 
            // num_scoreDM
            // 
            this.num_scoreDM.Location = new System.Drawing.Point(117, 67);
            this.num_scoreDM.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.num_scoreDM.Name = "num_scoreDM";
            this.num_scoreDM.Size = new System.Drawing.Size(48, 20);
            this.num_scoreDM.TabIndex = 33;
            this.num_scoreDM.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // num_scoreKOTH
            // 
            this.num_scoreKOTH.Location = new System.Drawing.Point(117, 42);
            this.num_scoreKOTH.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.num_scoreKOTH.Name = "num_scoreKOTH";
            this.num_scoreKOTH.Size = new System.Drawing.Size(48, 20);
            this.num_scoreKOTH.TabIndex = 32;
            this.num_scoreKOTH.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // num_scoreFB
            // 
            this.num_scoreFB.Location = new System.Drawing.Point(117, 17);
            this.num_scoreFB.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.num_scoreFB.Name = "num_scoreFB";
            this.num_scoreFB.Size = new System.Drawing.Size(48, 20);
            this.num_scoreFB.TabIndex = 31;
            this.num_scoreFB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_scoreDM
            // 
            this.label_scoreDM.AutoSize = true;
            this.label_scoreDM.Location = new System.Drawing.Point(54, 69);
            this.label_scoreDM.Name = "label_scoreDM";
            this.label_scoreDM.Size = new System.Drawing.Size(53, 13);
            this.label_scoreDM.TabIndex = 2;
            this.label_scoreDM.Text = "DM/TDM";
            // 
            // label_scoreKOTH
            // 
            this.label_scoreKOTH.AutoSize = true;
            this.label_scoreKOTH.Location = new System.Drawing.Point(28, 44);
            this.label_scoreKOTH.Name = "label_scoreKOTH";
            this.label_scoreKOTH.Size = new System.Drawing.Size(79, 13);
            this.label_scoreKOTH.TabIndex = 1;
            this.label_scoreKOTH.Text = "KOTH/TKOTH";
            // 
            // label_scoreFB
            // 
            this.label_scoreFB.AutoSize = true;
            this.label_scoreFB.Location = new System.Drawing.Point(29, 19);
            this.label_scoreFB.Name = "label_scoreFB";
            this.label_scoreFB.Size = new System.Drawing.Size(78, 13);
            this.label_scoreFB.TabIndex = 0;
            this.label_scoreFB.Text = "Flag Ball Score";
            // 
            // cb_autoLastKnown
            // 
            this.cb_autoLastKnown.AutoSize = true;
            this.cb_autoLastKnown.Location = new System.Drawing.Point(103, 344);
            this.cb_autoLastKnown.Name = "cb_autoLastKnown";
            this.cb_autoLastKnown.Size = new System.Drawing.Size(199, 17);
            this.cb_autoLastKnown.TabIndex = 37;
            this.cb_autoLastKnown.Text = "Autoload last known settings on start";
            this.cb_autoLastKnown.UseVisualStyleBackColor = true;
            // 
            // tabPage14
            // 
            this.tabPage14.Controls.Add(this.groupBox22);
            this.tabPage14.Controls.Add(this.gb_roleRestrictions);
            this.tabPage14.Location = new System.Drawing.Point(4, 22);
            this.tabPage14.Name = "tabPage14";
            this.tabPage14.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage14.Size = new System.Drawing.Size(397, 367);
            this.tabPage14.TabIndex = 2;
            this.tabPage14.Text = "Restrictions";
            this.tabPage14.UseVisualStyleBackColor = true;
            // 
            // groupBox22
            // 
            this.groupBox22.Controls.Add(this.rb_WeaonSelNone);
            this.groupBox22.Controls.Add(this.rb_WeaonSelAll);
            this.groupBox22.Controls.Add(this.cbl_weaponSelection);
            this.groupBox22.Location = new System.Drawing.Point(203, 6);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Size = new System.Drawing.Size(188, 352);
            this.groupBox22.TabIndex = 2;
            this.groupBox22.TabStop = false;
            this.groupBox22.Text = "Weapon Restrictions";
            // 
            // rb_WeaonSelNone
            // 
            this.rb_WeaonSelNone.AutoSize = true;
            this.rb_WeaonSelNone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_WeaonSelNone.Location = new System.Drawing.Point(53, 318);
            this.rb_WeaonSelNone.Name = "rb_WeaonSelNone";
            this.rb_WeaonSelNone.Size = new System.Drawing.Size(84, 17);
            this.rb_WeaonSelNone.TabIndex = 5;
            this.rb_WeaonSelNone.TabStop = true;
            this.rb_WeaonSelNone.Text = "Select None";
            this.rb_WeaonSelNone.UseVisualStyleBackColor = true;
            // 
            // rb_WeaonSelAll
            // 
            this.rb_WeaonSelAll.AutoSize = true;
            this.rb_WeaonSelAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_WeaonSelAll.Location = new System.Drawing.Point(53, 296);
            this.rb_WeaonSelAll.Name = "rb_WeaonSelAll";
            this.rb_WeaonSelAll.Size = new System.Drawing.Size(69, 17);
            this.rb_WeaonSelAll.TabIndex = 4;
            this.rb_WeaonSelAll.TabStop = true;
            this.rb_WeaonSelAll.Text = "Select All";
            this.rb_WeaonSelAll.UseVisualStyleBackColor = true;
            // 
            // cbl_weaponSelection
            // 
            this.cbl_weaponSelection.FormattingEnabled = true;
            this.cbl_weaponSelection.Items.AddRange(new object[] {
            "Colt .45",
            "M9 Beretta",
            "Shotgun",
            "CAR15",
            "CAR15/203",
            "M16",
            "M16/203",
            "M21",
            "M24",
            "MCRT .300 Tactical",
            "Barrett",
            "SAW",
            "M60",
            "M240",
            "MP5",
            "G3",
            "G36",
            "PSG-1",
            "Flash Bang",
            "Frag Grenade",
            "Smoke Grenade",
            "C4 (Satchel Charge)",
            "Claymore",
            "AT4"});
            this.cbl_weaponSelection.Location = new System.Drawing.Point(13, 30);
            this.cbl_weaponSelection.Name = "cbl_weaponSelection";
            this.cbl_weaponSelection.Size = new System.Drawing.Size(164, 259);
            this.cbl_weaponSelection.TabIndex = 3;
            // 
            // gb_roleRestrictions
            // 
            this.gb_roleRestrictions.Controls.Add(this.rb_roleSelNone);
            this.gb_roleRestrictions.Controls.Add(this.rb_roleSelAll);
            this.gb_roleRestrictions.Controls.Add(this.cbl_roleSelection);
            this.gb_roleRestrictions.Location = new System.Drawing.Point(4, 6);
            this.gb_roleRestrictions.Name = "gb_roleRestrictions";
            this.gb_roleRestrictions.Size = new System.Drawing.Size(191, 352);
            this.gb_roleRestrictions.TabIndex = 1;
            this.gb_roleRestrictions.TabStop = false;
            this.gb_roleRestrictions.Text = "Role Restrictions";
            // 
            // rb_roleSelNone
            // 
            this.rb_roleSelNone.AutoSize = true;
            this.rb_roleSelNone.Enabled = false;
            this.rb_roleSelNone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_roleSelNone.Location = new System.Drawing.Point(55, 122);
            this.rb_roleSelNone.Name = "rb_roleSelNone";
            this.rb_roleSelNone.Size = new System.Drawing.Size(84, 17);
            this.rb_roleSelNone.TabIndex = 2;
            this.rb_roleSelNone.TabStop = true;
            this.rb_roleSelNone.Text = "Select None";
            this.rb_roleSelNone.UseVisualStyleBackColor = true;
            // 
            // rb_roleSelAll
            // 
            this.rb_roleSelAll.AutoSize = true;
            this.rb_roleSelAll.Enabled = false;
            this.rb_roleSelAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_roleSelAll.Location = new System.Drawing.Point(55, 100);
            this.rb_roleSelAll.Name = "rb_roleSelAll";
            this.rb_roleSelAll.Size = new System.Drawing.Size(69, 17);
            this.rb_roleSelAll.TabIndex = 1;
            this.rb_roleSelAll.TabStop = true;
            this.rb_roleSelAll.Text = "Select All";
            this.rb_roleSelAll.UseVisualStyleBackColor = true;
            // 
            // cbl_roleSelection
            // 
            this.cbl_roleSelection.Enabled = false;
            this.cbl_roleSelection.FormattingEnabled = true;
            this.cbl_roleSelection.Items.AddRange(new object[] {
            "CQB",
            "Gunner",
            "Medic",
            "Sniper"});
            this.cbl_roleSelection.Location = new System.Drawing.Point(30, 30);
            this.cbl_roleSelection.Name = "cbl_roleSelection";
            this.cbl_roleSelection.Size = new System.Drawing.Size(134, 64);
            this.cbl_roleSelection.TabIndex = 0;
            // 
            // page_maps
            // 
            this.page_maps.Controls.Add(this.gb_MapsAvailable);
            this.page_maps.Controls.Add(this.gB_mapRotation);
            this.page_maps.Controls.Add(this.panel_mapControls);
            this.page_maps.Location = new System.Drawing.Point(4, 22);
            this.page_maps.Name = "page_maps";
            this.page_maps.Padding = new System.Windows.Forms.Padding(3);
            this.page_maps.Size = new System.Drawing.Size(800, 401);
            this.page_maps.TabIndex = 3;
            this.page_maps.Text = "Map Settings";
            this.page_maps.UseVisualStyleBackColor = true;
            // 
            // gb_MapsAvailable
            // 
            this.gb_MapsAvailable.Controls.Add(this.label_numMapsAvailable);
            this.gb_MapsAvailable.Controls.Add(this.label_gameType);
            this.gb_MapsAvailable.Controls.Add(this.listBox_mapsAvailable);
            this.gb_MapsAvailable.Controls.Add(this.dropDown_mapSettingsGameType);
            this.gb_MapsAvailable.Controls.Add(this.label_totalMaps);
            this.gb_MapsAvailable.Location = new System.Drawing.Point(6, 15);
            this.gb_MapsAvailable.Name = "gb_MapsAvailable";
            this.gb_MapsAvailable.Size = new System.Drawing.Size(341, 375);
            this.gb_MapsAvailable.TabIndex = 5;
            this.gb_MapsAvailable.TabStop = false;
            this.gb_MapsAvailable.Text = "Maps Available";
            // 
            // label_numMapsAvailable
            // 
            this.label_numMapsAvailable.AutoSize = true;
            this.label_numMapsAvailable.Location = new System.Drawing.Point(64, 17);
            this.label_numMapsAvailable.Name = "label_numMapsAvailable";
            this.label_numMapsAvailable.Size = new System.Drawing.Size(16, 13);
            this.label_numMapsAvailable.TabIndex = 4;
            this.label_numMapsAvailable.Text = "...";
            // 
            // label_gameType
            // 
            this.label_gameType.AutoSize = true;
            this.label_gameType.Location = new System.Drawing.Point(133, 17);
            this.label_gameType.Name = "label_gameType";
            this.label_gameType.Size = new System.Drawing.Size(65, 13);
            this.label_gameType.TabIndex = 1;
            this.label_gameType.Text = "Game Type:";
            // 
            // listBox_mapsAvailable
            // 
            this.listBox_mapsAvailable.FormattingEnabled = true;
            this.listBox_mapsAvailable.Location = new System.Drawing.Point(6, 39);
            this.listBox_mapsAvailable.Name = "listBox_mapsAvailable";
            this.listBox_mapsAvailable.Size = new System.Drawing.Size(329, 329);
            this.listBox_mapsAvailable.TabIndex = 0;
            this.listBox_mapsAvailable.DoubleClick += new System.EventHandler(this.mapsAvailable_DoubleClick);
            // 
            // dropDown_mapSettingsGameType
            // 
            this.dropDown_mapSettingsGameType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dropDown_mapSettingsGameType.FormattingEnabled = true;
            this.dropDown_mapSettingsGameType.Location = new System.Drawing.Point(199, 14);
            this.dropDown_mapSettingsGameType.Name = "dropDown_mapSettingsGameType";
            this.dropDown_mapSettingsGameType.Size = new System.Drawing.Size(136, 21);
            this.dropDown_mapSettingsGameType.TabIndex = 0;
            this.dropDown_mapSettingsGameType.SelectedIndexChanged += new System.EventHandler(this.mapSettingsGameType_changed);
            // 
            // label_totalMaps
            // 
            this.label_totalMaps.AutoSize = true;
            this.label_totalMaps.Location = new System.Drawing.Point(6, 17);
            this.label_totalMaps.Name = "label_totalMaps";
            this.label_totalMaps.Size = new System.Drawing.Size(63, 13);
            this.label_totalMaps.TabIndex = 3;
            this.label_totalMaps.Text = "Total Maps:";
            // 
            // gB_mapRotation
            // 
            this.gB_mapRotation.Controls.Add(this.button1);
            this.gB_mapRotation.Controls.Add(this.list_mapRotation);
            this.gB_mapRotation.Controls.Add(this.label_mapsSelected);
            this.gB_mapRotation.Controls.Add(this.label_CurrentMap);
            this.gB_mapRotation.Controls.Add(this.label_currentMapPlaying);
            this.gB_mapRotation.Controls.Add(this.endOfMapTimer_TrackBar);
            this.gB_mapRotation.Controls.Add(this.label_currentMapCount);
            this.gB_mapRotation.Controls.Add(this.label_EOMtimer);
            this.gB_mapRotation.Location = new System.Drawing.Point(462, 15);
            this.gB_mapRotation.Name = "gB_mapRotation";
            this.gB_mapRotation.Size = new System.Drawing.Size(332, 375);
            this.gB_mapRotation.TabIndex = 7;
            this.gB_mapRotation.TabStop = false;
            this.gB_mapRotation.Text = "Maps in Rotation:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(282, 274);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(44, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Reset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // list_mapRotation
            // 
            this.list_mapRotation.FormattingEnabled = true;
            this.list_mapRotation.Location = new System.Drawing.Point(6, 22);
            this.list_mapRotation.Name = "list_mapRotation";
            this.list_mapRotation.Size = new System.Drawing.Size(320, 238);
            this.list_mapRotation.TabIndex = 0;
            this.list_mapRotation.DoubleClick += new System.EventHandler(this.mapRotation_DoubleClick);
            // 
            // label_mapsSelected
            // 
            this.label_mapsSelected.AutoSize = true;
            this.label_mapsSelected.Location = new System.Drawing.Point(6, 268);
            this.label_mapsSelected.Name = "label_mapsSelected";
            this.label_mapsSelected.Size = new System.Drawing.Size(81, 13);
            this.label_mapsSelected.TabIndex = 2;
            this.label_mapsSelected.Text = "Maps Selected:";
            // 
            // label_CurrentMap
            // 
            this.label_CurrentMap.AutoSize = true;
            this.label_CurrentMap.Location = new System.Drawing.Point(6, 284);
            this.label_CurrentMap.Name = "label_CurrentMap";
            this.label_CurrentMap.Size = new System.Drawing.Size(105, 13);
            this.label_CurrentMap.TabIndex = 4;
            this.label_CurrentMap.Text = "Current Map Playing:";
            // 
            // label_currentMapPlaying
            // 
            this.label_currentMapPlaying.AutoSize = true;
            this.label_currentMapPlaying.Location = new System.Drawing.Point(118, 284);
            this.label_currentMapPlaying.Name = "label_currentMapPlaying";
            this.label_currentMapPlaying.Size = new System.Drawing.Size(16, 13);
            this.label_currentMapPlaying.TabIndex = 6;
            this.label_currentMapPlaying.Text = "...";
            // 
            // endOfMapTimer_TrackBar
            // 
            this.endOfMapTimer_TrackBar.Location = new System.Drawing.Point(6, 320);
            this.endOfMapTimer_TrackBar.Maximum = 46;
            this.endOfMapTimer_TrackBar.Name = "endOfMapTimer_TrackBar";
            this.endOfMapTimer_TrackBar.Size = new System.Drawing.Size(320, 45);
            this.endOfMapTimer_TrackBar.TabIndex = 5;
            this.endOfMapTimer_TrackBar.TickFrequency = 2;
            this.endOfMapTimer_TrackBar.Value = 45;
            this.endOfMapTimer_TrackBar.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.endOfMapTimer_TrackBar.MouseHover += new System.EventHandler(this.trackBar1_MouseHover);
            // 
            // label_currentMapCount
            // 
            this.label_currentMapCount.AutoSize = true;
            this.label_currentMapCount.Location = new System.Drawing.Point(97, 268);
            this.label_currentMapCount.Name = "label_currentMapCount";
            this.label_currentMapCount.Size = new System.Drawing.Size(36, 13);
            this.label_currentMapCount.TabIndex = 3;
            this.label_currentMapCount.Text = "../120";
            // 
            // label_EOMtimer
            // 
            this.label_EOMtimer.AutoSize = true;
            this.label_EOMtimer.Location = new System.Drawing.Point(6, 303);
            this.label_EOMtimer.Name = "label_EOMtimer";
            this.label_EOMtimer.Size = new System.Drawing.Size(128, 13);
            this.label_EOMtimer.TabIndex = 4;
            this.label_EOMtimer.Text = "End of Map Display Time:";
            // 
            // panel_mapControls
            // 
            this.panel_mapControls.Controls.Add(this.label103);
            this.panel_mapControls.Controls.Add(this.label96);
            this.panel_mapControls.Controls.Add(this.label92);
            this.panel_mapControls.Controls.Add(this.btn_SaveRoation);
            this.panel_mapControls.Controls.Add(this.btn_loadRotation);
            this.panel_mapControls.Controls.Add(this.btn_updateMaps);
            this.panel_mapControls.Controls.Add(this.btn_playMapNext);
            this.panel_mapControls.Controls.Add(this.btn_scoreSkip);
            this.panel_mapControls.Controls.Add(this.btn_moveMapDown);
            this.panel_mapControls.Controls.Add(this.btn_moveMapUp);
            this.panel_mapControls.Controls.Add(this.btn_ShuffleMaps);
            this.panel_mapControls.Location = new System.Drawing.Point(353, 44);
            this.panel_mapControls.Name = "panel_mapControls";
            this.panel_mapControls.Size = new System.Drawing.Size(103, 316);
            this.panel_mapControls.TabIndex = 2;
            // 
            // label103
            // 
            this.label103.AutoSize = true;
            this.label103.ForeColor = System.Drawing.Color.Blue;
            this.label103.Location = new System.Drawing.Point(11, 238);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(83, 13);
            this.label103.TabIndex = 12;
            this.label103.Text = "Save Rotations:";
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.ForeColor = System.Drawing.Color.Blue;
            this.label96.Location = new System.Drawing.Point(9, 92);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(83, 13);
            this.label96.TabIndex = 11;
            this.label96.Text = "Map Commands";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.ForeColor = System.Drawing.Color.Blue;
            this.label92.Location = new System.Drawing.Point(22, 11);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(58, 13);
            this.label92.TabIndex = 10;
            this.label92.Text = "Move Map";
            // 
            // btn_SaveRoation
            // 
            this.btn_SaveRoation.Location = new System.Drawing.Point(3, 255);
            this.btn_SaveRoation.Name = "btn_SaveRoation";
            this.btn_SaveRoation.Size = new System.Drawing.Size(97, 23);
            this.btn_SaveRoation.TabIndex = 9;
            this.btn_SaveRoation.Text = "Save Rotation";
            this.btn_SaveRoation.UseVisualStyleBackColor = true;
            this.btn_SaveRoation.Click += new System.EventHandler(this.mapAction_clickSaveRotation);
            // 
            // btn_loadRotation
            // 
            this.btn_loadRotation.Location = new System.Drawing.Point(3, 281);
            this.btn_loadRotation.Name = "btn_loadRotation";
            this.btn_loadRotation.Size = new System.Drawing.Size(97, 23);
            this.btn_loadRotation.TabIndex = 8;
            this.btn_loadRotation.Text = "Load Rotation";
            this.btn_loadRotation.UseVisualStyleBackColor = true;
            this.btn_loadRotation.Click += new System.EventHandler(this.mapAction_clickLoadRotation);
            // 
            // btn_updateMaps
            // 
            this.btn_updateMaps.Location = new System.Drawing.Point(2, 171);
            this.btn_updateMaps.Name = "btn_updateMaps";
            this.btn_updateMaps.Size = new System.Drawing.Size(97, 23);
            this.btn_updateMaps.TabIndex = 7;
            this.btn_updateMaps.Text = "Update Maps";
            this.btn_updateMaps.UseVisualStyleBackColor = true;
            this.btn_updateMaps.Click += new System.EventHandler(this.mapAction_clickUpdateActiveMaps);
            // 
            // btn_playMapNext
            // 
            this.btn_playMapNext.Location = new System.Drawing.Point(2, 141);
            this.btn_playMapNext.Name = "btn_playMapNext";
            this.btn_playMapNext.Size = new System.Drawing.Size(97, 23);
            this.btn_playMapNext.TabIndex = 6;
            this.btn_playMapNext.Text = "Play Map Next";
            this.btn_playMapNext.UseVisualStyleBackColor = true;
            this.btn_playMapNext.Click += new System.EventHandler(this.mapAction_clickPlayMapNext);
            // 
            // btn_scoreSkip
            // 
            this.btn_scoreSkip.Location = new System.Drawing.Point(2, 202);
            this.btn_scoreSkip.Name = "btn_scoreSkip";
            this.btn_scoreSkip.Size = new System.Drawing.Size(97, 23);
            this.btn_scoreSkip.TabIndex = 5;
            this.btn_scoreSkip.Text = "Score/Skip Map";
            this.btn_scoreSkip.UseVisualStyleBackColor = true;
            this.btn_scoreSkip.Click += new System.EventHandler(this.mapAction_clickScore);
            // 
            // btn_moveMapDown
            // 
            this.btn_moveMapDown.Location = new System.Drawing.Point(2, 60);
            this.btn_moveMapDown.Name = "btn_moveMapDown";
            this.btn_moveMapDown.Size = new System.Drawing.Size(97, 23);
            this.btn_moveMapDown.TabIndex = 4;
            this.btn_moveMapDown.Text = "Down";
            this.btn_moveMapDown.UseVisualStyleBackColor = true;
            this.btn_moveMapDown.Click += new System.EventHandler(this.mapAction_MoveMapEntry);
            // 
            // btn_moveMapUp
            // 
            this.btn_moveMapUp.Location = new System.Drawing.Point(2, 30);
            this.btn_moveMapUp.Name = "btn_moveMapUp";
            this.btn_moveMapUp.Size = new System.Drawing.Size(97, 23);
            this.btn_moveMapUp.TabIndex = 3;
            this.btn_moveMapUp.Text = "Up";
            this.btn_moveMapUp.UseVisualStyleBackColor = true;
            this.btn_moveMapUp.Click += new System.EventHandler(this.mapAction_MoveMapEntry);
            // 
            // btn_ShuffleMaps
            // 
            this.btn_ShuffleMaps.Location = new System.Drawing.Point(3, 111);
            this.btn_ShuffleMaps.Name = "btn_ShuffleMaps";
            this.btn_ShuffleMaps.Size = new System.Drawing.Size(97, 23);
            this.btn_ShuffleMaps.TabIndex = 2;
            this.btn_ShuffleMaps.Text = "Shuffle Maps";
            this.btn_ShuffleMaps.UseVisualStyleBackColor = true;
            this.btn_ShuffleMaps.Click += new System.EventHandler(this.mapAction_clickShuffleMaps);
            // 
            // page_chat
            // 
            this.page_chat.Controls.Add(this.panel_chatMessage);
            this.page_chat.Controls.Add(this.panel_chatControls);
            this.page_chat.Controls.Add(this.data_chatViewer);
            this.page_chat.Location = new System.Drawing.Point(4, 22);
            this.page_chat.Name = "page_chat";
            this.page_chat.Padding = new System.Windows.Forms.Padding(3);
            this.page_chat.Size = new System.Drawing.Size(800, 401);
            this.page_chat.TabIndex = 4;
            this.page_chat.Text = "Chat";
            this.page_chat.UseVisualStyleBackColor = true;
            // 
            // panel_chatMessage
            // 
            this.panel_chatMessage.Controls.Add(this.chat_channelSelection);
            this.panel_chatMessage.Controls.Add(this.chat_textBoxMsg);
            this.panel_chatMessage.Controls.Add(this.btn_sendChat);
            this.panel_chatMessage.Location = new System.Drawing.Point(65, 366);
            this.panel_chatMessage.Name = "panel_chatMessage";
            this.panel_chatMessage.Size = new System.Drawing.Size(674, 29);
            this.panel_chatMessage.TabIndex = 22;
            // 
            // chat_channelSelection
            // 
            this.chat_channelSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chat_channelSelection.FormattingEnabled = true;
            this.chat_channelSelection.Items.AddRange(new object[] {
            "Global",
            "Yellow",
            "Orange",
            "Team Red",
            "Team Blue"});
            this.chat_channelSelection.Location = new System.Drawing.Point(521, 4);
            this.chat_channelSelection.Name = "chat_channelSelection";
            this.chat_channelSelection.Size = new System.Drawing.Size(74, 21);
            this.chat_channelSelection.TabIndex = 8;
            // 
            // chat_textBoxMsg
            // 
            this.chat_textBoxMsg.Location = new System.Drawing.Point(5, 4);
            this.chat_textBoxMsg.MaxLength = 30;
            this.chat_textBoxMsg.Name = "chat_textBoxMsg";
            this.chat_textBoxMsg.Size = new System.Drawing.Size(514, 20);
            this.chat_textBoxMsg.TabIndex = 7;
            this.chat_textBoxMsg.KeyUp += new System.Windows.Forms.KeyEventHandler(this.keyup_textBoxMsg);
            // 
            // btn_sendChat
            // 
            this.btn_sendChat.Location = new System.Drawing.Point(595, 3);
            this.btn_sendChat.Name = "btn_sendChat";
            this.btn_sendChat.Size = new System.Drawing.Size(75, 23);
            this.btn_sendChat.TabIndex = 9;
            this.btn_sendChat.Text = "Send Chat";
            this.btn_sendChat.UseVisualStyleBackColor = true;
            this.btn_sendChat.Click += new System.EventHandler(this.chat_SendMsg);
            // 
            // panel_chatControls
            // 
            this.panel_chatControls.Controls.Add(this.cb_chatPlayerSelect);
            this.panel_chatControls.Controls.Add(this.groupBox_chatChannel);
            this.panel_chatControls.Location = new System.Drawing.Point(3, 332);
            this.panel_chatControls.Name = "panel_chatControls";
            this.panel_chatControls.Size = new System.Drawing.Size(794, 29);
            this.panel_chatControls.TabIndex = 21;
            // 
            // cb_chatPlayerSelect
            // 
            this.cb_chatPlayerSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_chatPlayerSelect.Enabled = false;
            this.cb_chatPlayerSelect.FormattingEnabled = true;
            this.cb_chatPlayerSelect.Items.AddRange(new object[] {
            "Select Player"});
            this.cb_chatPlayerSelect.Location = new System.Drawing.Point(657, 4);
            this.cb_chatPlayerSelect.Name = "cb_chatPlayerSelect";
            this.cb_chatPlayerSelect.Size = new System.Drawing.Size(133, 21);
            this.cb_chatPlayerSelect.TabIndex = 6;
            this.cb_chatPlayerSelect.SelectedIndexChanged += new System.EventHandler(this.chat_dropDownPlayerNameChanged);
            // 
            // groupBox_chatChannel
            // 
            this.groupBox_chatChannel.BackColor = System.Drawing.Color.Transparent;
            this.groupBox_chatChannel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox_chatChannel.Controls.Add(this.rb_chatAll);
            this.groupBox_chatChannel.Controls.Add(this.rb_chatGlobal);
            this.groupBox_chatChannel.Controls.Add(this.rb_chatPlayerHist);
            this.groupBox_chatChannel.Controls.Add(this.rb_chatRedTeam);
            this.groupBox_chatChannel.Controls.Add(this.rb_chatBlueTeam);
            this.groupBox_chatChannel.Location = new System.Drawing.Point(131, -6);
            this.groupBox_chatChannel.Name = "groupBox_chatChannel";
            this.groupBox_chatChannel.Size = new System.Drawing.Size(505, 35);
            this.groupBox_chatChannel.TabIndex = 4;
            this.groupBox_chatChannel.TabStop = false;
            // 
            // rb_chatAll
            // 
            this.rb_chatAll.AutoSize = true;
            this.rb_chatAll.Checked = true;
            this.rb_chatAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_chatAll.Location = new System.Drawing.Point(5, 12);
            this.rb_chatAll.Name = "rb_chatAll";
            this.rb_chatAll.Size = new System.Drawing.Size(61, 17);
            this.rb_chatAll.TabIndex = 1;
            this.rb_chatAll.TabStop = true;
            this.rb_chatAll.Text = "All Chat";
            this.rb_chatAll.UseVisualStyleBackColor = true;
            this.rb_chatAll.Click += new System.EventHandler(this.chat_btnClickChannelAll);
            // 
            // rb_chatGlobal
            // 
            this.rb_chatGlobal.AutoSize = true;
            this.rb_chatGlobal.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_chatGlobal.Location = new System.Drawing.Point(72, 12);
            this.rb_chatGlobal.Name = "rb_chatGlobal";
            this.rb_chatGlobal.Size = new System.Drawing.Size(80, 17);
            this.rb_chatGlobal.TabIndex = 2;
            this.rb_chatGlobal.Text = "Global Chat";
            this.rb_chatGlobal.UseVisualStyleBackColor = true;
            this.rb_chatGlobal.Click += new System.EventHandler(this.chat_btnClickChannelGlobal);
            // 
            // rb_chatPlayerHist
            // 
            this.rb_chatPlayerHist.AutoSize = true;
            this.rb_chatPlayerHist.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_chatPlayerHist.Location = new System.Drawing.Point(371, 12);
            this.rb_chatPlayerHist.Name = "rb_chatPlayerHist";
            this.rb_chatPlayerHist.Size = new System.Drawing.Size(134, 17);
            this.rb_chatPlayerHist.TabIndex = 5;
            this.rb_chatPlayerHist.Text = "Selected Player History";
            this.rb_chatPlayerHist.UseVisualStyleBackColor = true;
            this.rb_chatPlayerHist.CheckedChanged += new System.EventHandler(this.chat_btnSelectPlayerChanged);
            this.rb_chatPlayerHist.Click += new System.EventHandler(this.chat_btnClickChannelPlayer);
            // 
            // rb_chatRedTeam
            // 
            this.rb_chatRedTeam.AccessibleDescription = "rb_chatRedTeam";
            this.rb_chatRedTeam.AutoSize = true;
            this.rb_chatRedTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_chatRedTeam.Location = new System.Drawing.Point(158, 12);
            this.rb_chatRedTeam.Name = "rb_chatRedTeam";
            this.rb_chatRedTeam.Size = new System.Drawing.Size(100, 17);
            this.rb_chatRedTeam.TabIndex = 3;
            this.rb_chatRedTeam.Text = "Red Team Chat";
            this.rb_chatRedTeam.UseVisualStyleBackColor = true;
            this.rb_chatRedTeam.Click += new System.EventHandler(this.chat_btnClickChannelRed);
            // 
            // rb_chatBlueTeam
            // 
            this.rb_chatBlueTeam.AutoSize = true;
            this.rb_chatBlueTeam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rb_chatBlueTeam.Location = new System.Drawing.Point(264, 12);
            this.rb_chatBlueTeam.Name = "rb_chatBlueTeam";
            this.rb_chatBlueTeam.Size = new System.Drawing.Size(101, 17);
            this.rb_chatBlueTeam.TabIndex = 4;
            this.rb_chatBlueTeam.Text = "Blue Team Chat";
            this.rb_chatBlueTeam.UseVisualStyleBackColor = true;
            this.rb_chatBlueTeam.Click += new System.EventHandler(this.chat_btnClickChannelBlue);
            // 
            // data_chatViewer
            // 
            this.data_chatViewer.AllowUserToAddRows = false;
            this.data_chatViewer.AllowUserToDeleteRows = false;
            this.data_chatViewer.AllowUserToOrderColumns = true;
            this.data_chatViewer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.data_chatViewer.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.data_chatViewer.Location = new System.Drawing.Point(3, 3);
            this.data_chatViewer.Name = "data_chatViewer";
            this.data_chatViewer.ReadOnly = true;
            this.data_chatViewer.RowHeadersWidth = 51;
            this.data_chatViewer.ShowCellErrors = false;
            this.data_chatViewer.ShowCellToolTips = false;
            this.data_chatViewer.ShowEditingIcon = false;
            this.data_chatViewer.ShowRowErrors = false;
            this.data_chatViewer.Size = new System.Drawing.Size(794, 323);
            this.data_chatViewer.TabIndex = 20;
            // 
            // page_autoMessages
            // 
            this.page_autoMessages.Controls.Add(this.gb_autoMessages);
            this.page_autoMessages.Location = new System.Drawing.Point(4, 22);
            this.page_autoMessages.Name = "page_autoMessages";
            this.page_autoMessages.Padding = new System.Windows.Forms.Padding(3);
            this.page_autoMessages.Size = new System.Drawing.Size(800, 401);
            this.page_autoMessages.TabIndex = 5;
            this.page_autoMessages.Text = "Auto Messages";
            this.page_autoMessages.UseVisualStyleBackColor = true;
            // 
            // gb_autoMessages
            // 
            this.gb_autoMessages.Controls.Add(this.gb_addMessages);
            this.gb_autoMessages.Controls.Add(this.label_interval);
            this.gb_autoMessages.Controls.Add(this.listBox_AutoMessages);
            this.gb_autoMessages.Controls.Add(this.num_autoMsgInterval);
            this.gb_autoMessages.Controls.Add(this.cb_enableAutoMsg);
            this.gb_autoMessages.Location = new System.Drawing.Point(5, 5);
            this.gb_autoMessages.Name = "gb_autoMessages";
            this.gb_autoMessages.Size = new System.Drawing.Size(790, 390);
            this.gb_autoMessages.TabIndex = 1;
            this.gb_autoMessages.TabStop = false;
            this.gb_autoMessages.Text = "Messages";
            // 
            // gb_addMessages
            // 
            this.gb_addMessages.Controls.Add(this.text_newAutoMessage);
            this.gb_addMessages.Controls.Add(this.btn_addAutoMsg);
            this.gb_addMessages.Location = new System.Drawing.Point(6, 342);
            this.gb_addMessages.Name = "gb_addMessages";
            this.gb_addMessages.Size = new System.Drawing.Size(778, 44);
            this.gb_addMessages.TabIndex = 8;
            this.gb_addMessages.TabStop = false;
            this.gb_addMessages.Text = "Add Message";
            // 
            // text_newAutoMessage
            // 
            this.text_newAutoMessage.Location = new System.Drawing.Point(6, 16);
            this.text_newAutoMessage.Name = "text_newAutoMessage";
            this.text_newAutoMessage.Size = new System.Drawing.Size(685, 20);
            this.text_newAutoMessage.TabIndex = 4;
            // 
            // btn_addAutoMsg
            // 
            this.btn_addAutoMsg.Location = new System.Drawing.Point(697, 14);
            this.btn_addAutoMsg.Name = "btn_addAutoMsg";
            this.btn_addAutoMsg.Size = new System.Drawing.Size(75, 23);
            this.btn_addAutoMsg.TabIndex = 3;
            this.btn_addAutoMsg.Text = "Add";
            this.btn_addAutoMsg.UseVisualStyleBackColor = true;
            this.btn_addAutoMsg.Click += new System.EventHandler(this.autoMessages_clickAdd);
            // 
            // label_interval
            // 
            this.label_interval.AutoSize = true;
            this.label_interval.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_interval.Location = new System.Drawing.Point(648, 326);
            this.label_interval.Name = "label_interval";
            this.label_interval.Size = new System.Drawing.Size(88, 13);
            this.label_interval.TabIndex = 7;
            this.label_interval.Text = "Interval (Minutes)";
            // 
            // listBox_AutoMessages
            // 
            this.listBox_AutoMessages.FormattingEnabled = true;
            this.listBox_AutoMessages.Location = new System.Drawing.Point(5, 15);
            this.listBox_AutoMessages.Name = "listBox_AutoMessages";
            this.listBox_AutoMessages.Size = new System.Drawing.Size(780, 303);
            this.listBox_AutoMessages.TabIndex = 0;
            this.listBox_AutoMessages.DoubleClick += new System.EventHandler(this.autoMessages_doubleClickDelete);
            this.listBox_AutoMessages.KeyDown += new System.Windows.Forms.KeyEventHandler(this.autoMessage_clickMoveDown);
            this.listBox_AutoMessages.KeyUp += new System.Windows.Forms.KeyEventHandler(this.autoMessage_clickMoveUp);
            // 
            // num_autoMsgInterval
            // 
            this.num_autoMsgInterval.Location = new System.Drawing.Point(737, 322);
            this.num_autoMsgInterval.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.num_autoMsgInterval.Name = "num_autoMsgInterval";
            this.num_autoMsgInterval.Size = new System.Drawing.Size(40, 20);
            this.num_autoMsgInterval.TabIndex = 6;
            this.num_autoMsgInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.num_autoMsgInterval.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.num_autoMsgInterval.ValueChanged += new System.EventHandler(this.autoMessage_intervalChange);
            // 
            // cb_enableAutoMsg
            // 
            this.cb_enableAutoMsg.AutoSize = true;
            this.cb_enableAutoMsg.Location = new System.Drawing.Point(514, 325);
            this.cb_enableAutoMsg.Name = "cb_enableAutoMsg";
            this.cb_enableAutoMsg.Size = new System.Drawing.Size(135, 17);
            this.cb_enableAutoMsg.TabIndex = 5;
            this.cb_enableAutoMsg.Text = "Enable Auto Messages";
            this.cb_enableAutoMsg.UseVisualStyleBackColor = true;
            this.cb_enableAutoMsg.Click += new System.EventHandler(this.autoMessages_stateCheckbox);
            // 
            // playerList_contextMenu
            // 
            this.playerList_contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem12,
            this.toolStripMenuItem133,
            this.toolStripMenuItem76,
            this.toolStripMenuItem129,
            this.cm_kickPlayer,
            this.playerListMenu_tempBan,
            this.playerListMenu_permBanPlayer,
            this.playerListMenu_changeTeams,
            this.playerListMenu_spectate});
            this.playerList_contextMenu.Name = "playerList_contextMenu";
            this.playerList_contextMenu.Size = new System.Drawing.Size(160, 202);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem13,
            this.toolStripMenuItem14});
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem12.Text = "Arm/Disarm";
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItem13.Text = "Disarm Player";
            this.toolStripMenuItem13.Click += new System.EventHandler(this.playerListAction_clickDisarm);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItem14.Text = "Rearm Player";
            this.toolStripMenuItem14.Click += new System.EventHandler(this.playerListAction_clickRearm);
            // 
            // toolStripMenuItem133
            // 
            this.toolStripMenuItem133.Name = "toolStripMenuItem133";
            this.toolStripMenuItem133.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem133.Text = "Warn Player";
            // 
            // toolStripMenuItem76
            // 
            this.toolStripMenuItem76.Name = "toolStripMenuItem76";
            this.toolStripMenuItem76.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem76.Text = "Kill Player";
            this.toolStripMenuItem76.Click += new System.EventHandler(this.playerListAction_clickKill);
            // 
            // toolStripMenuItem129
            // 
            this.toolStripMenuItem129.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem130,
            this.toolStripMenuItem131});
            this.toolStripMenuItem129.Name = "toolStripMenuItem129";
            this.toolStripMenuItem129.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItem129.Text = "Fun Commands";
            // 
            // toolStripMenuItem130
            // 
            this.toolStripMenuItem130.Name = "toolStripMenuItem130";
            this.toolStripMenuItem130.Size = new System.Drawing.Size(188, 22);
            this.toolStripMenuItem130.Text = "Activate God Mode";
            this.toolStripMenuItem130.Click += new System.EventHandler(this.playerList_actionActivateGodMode);
            // 
            // toolStripMenuItem131
            // 
            this.toolStripMenuItem131.Name = "toolStripMenuItem131";
            this.toolStripMenuItem131.Size = new System.Drawing.Size(188, 22);
            this.toolStripMenuItem131.Text = "Deactivate God Mode";
            this.toolStripMenuItem131.Click += new System.EventHandler(this.playerList_actionDeactivateGodMode);
            // 
            // cm_kickPlayer
            // 
            this.cm_kickPlayer.Name = "cm_kickPlayer";
            this.cm_kickPlayer.Size = new System.Drawing.Size(159, 22);
            this.cm_kickPlayer.Text = "Kick Player";
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(206, 6);
            // 
            // playerListMenu_tempBan
            // 
            this.playerListMenu_tempBan.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StripMenu_1Day,
            this.StripMenu_2Day,
            this.StripMenu_3Day,
            this.StripMenu_4Day,
            this.StripMenu_5Day,
            this.StripMenu_6Day,
            this.StripMenu_1Week,
            this.StripMenu_2Week,
            this.StripMenu_1Month});
            this.playerListMenu_tempBan.Name = "playerListMenu_tempBan";
            this.playerListMenu_tempBan.Size = new System.Drawing.Size(159, 22);
            this.playerListMenu_tempBan.Text = "Temp Ban";
            // 
            // StripMenu_1Day
            // 
            this.StripMenu_1Day.Name = "StripMenu_1Day";
            this.StripMenu_1Day.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_1Day.Text = "1 Day";
            // 
            // StripMenu_2Day
            // 
            this.StripMenu_2Day.Name = "StripMenu_2Day";
            this.StripMenu_2Day.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_2Day.Text = "2 Days";
            // 
            // StripMenu_3Day
            // 
            this.StripMenu_3Day.Name = "StripMenu_3Day";
            this.StripMenu_3Day.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_3Day.Text = "3 Days";
            // 
            // StripMenu_4Day
            // 
            this.StripMenu_4Day.Name = "StripMenu_4Day";
            this.StripMenu_4Day.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_4Day.Text = "4 Days";
            // 
            // StripMenu_5Day
            // 
            this.StripMenu_5Day.Name = "StripMenu_5Day";
            this.StripMenu_5Day.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_5Day.Text = "5 Days";
            // 
            // StripMenu_6Day
            // 
            this.StripMenu_6Day.Name = "StripMenu_6Day";
            this.StripMenu_6Day.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_6Day.Text = "6 Days";
            // 
            // StripMenu_1Week
            // 
            this.StripMenu_1Week.Name = "StripMenu_1Week";
            this.StripMenu_1Week.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_1Week.Text = "1 Week";
            // 
            // StripMenu_2Week
            // 
            this.StripMenu_2Week.Name = "StripMenu_2Week";
            this.StripMenu_2Week.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_2Week.Text = "2 Weeks";
            // 
            // StripMenu_1Month
            // 
            this.StripMenu_1Month.Name = "StripMenu_1Month";
            this.StripMenu_1Month.Size = new System.Drawing.Size(180, 22);
            this.StripMenu_1Month.Text = "1 Month";
            // 
            // playerListMenu_permBanPlayer
            // 
            this.playerListMenu_permBanPlayer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator});
            this.playerListMenu_permBanPlayer.Name = "playerListMenu_permBanPlayer";
            this.playerListMenu_permBanPlayer.Size = new System.Drawing.Size(159, 22);
            this.playerListMenu_permBanPlayer.Text = "Perm Ban";
            // 
            // playerListMenu_changeTeams
            // 
            this.playerListMenu_changeTeams.Name = "playerListMenu_changeTeams";
            this.playerListMenu_changeTeams.Size = new System.Drawing.Size(159, 22);
            this.playerListMenu_changeTeams.Text = "Change Team";
            this.playerListMenu_changeTeams.Click += new System.EventHandler(this.playerListAction_clickChangeTeams);
            // 
            // playerListMenu_spectate
            // 
            this.playerListMenu_spectate.Name = "playerListMenu_spectate";
            this.playerListMenu_spectate.Size = new System.Drawing.Size(159, 22);
            this.playerListMenu_spectate.Text = "Spectate";
            this.playerListMenu_spectate.Click += new System.EventHandler(this.actionSpectate_click);
            // 
            // playerListMenu_Seperator
            // 
            this.playerListMenu_Seperator.Name = "playerListMenu_Seperator";
            this.playerListMenu_Seperator.Size = new System.Drawing.Size(177, 6);
            // 
            // actionReason_Abusive
            // 
            this.actionReason_Abusive.Name = "actionReason_Abusive";
            this.actionReason_Abusive.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Racism
            // 
            this.actionReason_Racism.Name = "actionReason_Racism";
            this.actionReason_Racism.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Cheating
            // 
            this.actionReason_Cheating.Name = "actionReason_Cheating";
            this.actionReason_Cheating.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_WallHack
            // 
            this.actionReason_WallHack.Name = "actionReason_WallHack";
            this.actionReason_WallHack.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Aimbot
            // 
            this.actionReason_Aimbot.Name = "actionReason_Aimbot";
            this.actionReason_Aimbot.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Speed
            // 
            this.actionReason_Speed.Name = "actionReason_Speed";
            this.actionReason_Speed.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Disrespect
            // 
            this.actionReason_Disrespect.Name = "actionReason_Disrespect";
            this.actionReason_Disrespect.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Camping
            // 
            this.actionReason_Camping.Name = "actionReason_Camping";
            this.actionReason_Camping.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_TPK
            // 
            this.actionReason_TPK.Name = "actionReason_TPK";
            this.actionReason_TPK.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Rules
            // 
            this.actionReason_Rules.Name = "actionReason_Rules";
            this.actionReason_Rules.Size = new System.Drawing.Size(32, 19);
            // 
            // actionReason_Custom
            // 
            this.actionReason_Custom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.actionReason_Custom.Name = "actionReason_Custom";
            this.actionReason_Custom.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            this.toolStripMenuItem15.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem37
            // 
            this.toolStripMenuItem37.Name = "toolStripMenuItem37";
            this.toolStripMenuItem37.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem38
            // 
            this.toolStripMenuItem38.Name = "toolStripMenuItem38";
            this.toolStripMenuItem38.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem39
            // 
            this.toolStripMenuItem39.Name = "toolStripMenuItem39";
            this.toolStripMenuItem39.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem40
            // 
            this.toolStripMenuItem40.Name = "toolStripMenuItem40";
            this.toolStripMenuItem40.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem41
            // 
            this.toolStripMenuItem41.Name = "toolStripMenuItem41";
            this.toolStripMenuItem41.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem42
            // 
            this.toolStripMenuItem42.Name = "toolStripMenuItem42";
            this.toolStripMenuItem42.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem43
            // 
            this.toolStripMenuItem43.Name = "toolStripMenuItem43";
            this.toolStripMenuItem43.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem44
            // 
            this.toolStripMenuItem44.Name = "toolStripMenuItem44";
            this.toolStripMenuItem44.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem45
            // 
            this.toolStripMenuItem45.Name = "toolStripMenuItem45";
            this.toolStripMenuItem45.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox3
            // 
            this.toolStripTextBox3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox3.Name = "toolStripTextBox3";
            this.toolStripTextBox3.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem17
            // 
            this.toolStripMenuItem17.Name = "toolStripMenuItem17";
            this.toolStripMenuItem17.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem46
            // 
            this.toolStripMenuItem46.Name = "toolStripMenuItem46";
            this.toolStripMenuItem46.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem47
            // 
            this.toolStripMenuItem47.Name = "toolStripMenuItem47";
            this.toolStripMenuItem47.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem48
            // 
            this.toolStripMenuItem48.Name = "toolStripMenuItem48";
            this.toolStripMenuItem48.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem49
            // 
            this.toolStripMenuItem49.Name = "toolStripMenuItem49";
            this.toolStripMenuItem49.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem50
            // 
            this.toolStripMenuItem50.Name = "toolStripMenuItem50";
            this.toolStripMenuItem50.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem51
            // 
            this.toolStripMenuItem51.Name = "toolStripMenuItem51";
            this.toolStripMenuItem51.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem52
            // 
            this.toolStripMenuItem52.Name = "toolStripMenuItem52";
            this.toolStripMenuItem52.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem53
            // 
            this.toolStripMenuItem53.Name = "toolStripMenuItem53";
            this.toolStripMenuItem53.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem54
            // 
            this.toolStripMenuItem54.Name = "toolStripMenuItem54";
            this.toolStripMenuItem54.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox4
            // 
            this.toolStripTextBox4.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox4.Name = "toolStripTextBox4";
            this.toolStripTextBox4.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem55
            // 
            this.toolStripMenuItem55.Name = "toolStripMenuItem55";
            this.toolStripMenuItem55.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem56
            // 
            this.toolStripMenuItem56.Name = "toolStripMenuItem56";
            this.toolStripMenuItem56.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem57
            // 
            this.toolStripMenuItem57.Name = "toolStripMenuItem57";
            this.toolStripMenuItem57.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem58
            // 
            this.toolStripMenuItem58.Name = "toolStripMenuItem58";
            this.toolStripMenuItem58.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem59
            // 
            this.toolStripMenuItem59.Name = "toolStripMenuItem59";
            this.toolStripMenuItem59.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem60
            // 
            this.toolStripMenuItem60.Name = "toolStripMenuItem60";
            this.toolStripMenuItem60.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem61
            // 
            this.toolStripMenuItem61.Name = "toolStripMenuItem61";
            this.toolStripMenuItem61.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem62
            // 
            this.toolStripMenuItem62.Name = "toolStripMenuItem62";
            this.toolStripMenuItem62.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem63
            // 
            this.toolStripMenuItem63.Name = "toolStripMenuItem63";
            this.toolStripMenuItem63.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem64
            // 
            this.toolStripMenuItem64.Name = "toolStripMenuItem64";
            this.toolStripMenuItem64.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox5
            // 
            this.toolStripTextBox5.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox5.Name = "toolStripTextBox5";
            this.toolStripTextBox5.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem65
            // 
            this.toolStripMenuItem65.Name = "toolStripMenuItem65";
            this.toolStripMenuItem65.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem66
            // 
            this.toolStripMenuItem66.Name = "toolStripMenuItem66";
            this.toolStripMenuItem66.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem67
            // 
            this.toolStripMenuItem67.Name = "toolStripMenuItem67";
            this.toolStripMenuItem67.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem68
            // 
            this.toolStripMenuItem68.Name = "toolStripMenuItem68";
            this.toolStripMenuItem68.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem69
            // 
            this.toolStripMenuItem69.Name = "toolStripMenuItem69";
            this.toolStripMenuItem69.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem70
            // 
            this.toolStripMenuItem70.Name = "toolStripMenuItem70";
            this.toolStripMenuItem70.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem71
            // 
            this.toolStripMenuItem71.Name = "toolStripMenuItem71";
            this.toolStripMenuItem71.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem72
            // 
            this.toolStripMenuItem72.Name = "toolStripMenuItem72";
            this.toolStripMenuItem72.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem73
            // 
            this.toolStripMenuItem73.Name = "toolStripMenuItem73";
            this.toolStripMenuItem73.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem74
            // 
            this.toolStripMenuItem74.Name = "toolStripMenuItem74";
            this.toolStripMenuItem74.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox6
            // 
            this.toolStripTextBox6.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox6.Name = "toolStripTextBox6";
            this.toolStripTextBox6.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem75
            // 
            this.toolStripMenuItem75.Name = "toolStripMenuItem75";
            this.toolStripMenuItem75.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem77
            // 
            this.toolStripMenuItem77.Name = "toolStripMenuItem77";
            this.toolStripMenuItem77.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem78
            // 
            this.toolStripMenuItem78.Name = "toolStripMenuItem78";
            this.toolStripMenuItem78.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem79
            // 
            this.toolStripMenuItem79.Name = "toolStripMenuItem79";
            this.toolStripMenuItem79.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem80
            // 
            this.toolStripMenuItem80.Name = "toolStripMenuItem80";
            this.toolStripMenuItem80.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem81
            // 
            this.toolStripMenuItem81.Name = "toolStripMenuItem81";
            this.toolStripMenuItem81.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem82
            // 
            this.toolStripMenuItem82.Name = "toolStripMenuItem82";
            this.toolStripMenuItem82.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem83
            // 
            this.toolStripMenuItem83.Name = "toolStripMenuItem83";
            this.toolStripMenuItem83.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem84
            // 
            this.toolStripMenuItem84.Name = "toolStripMenuItem84";
            this.toolStripMenuItem84.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem85
            // 
            this.toolStripMenuItem85.Name = "toolStripMenuItem85";
            this.toolStripMenuItem85.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox7
            // 
            this.toolStripTextBox7.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox7.Name = "toolStripTextBox7";
            this.toolStripTextBox7.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem86
            // 
            this.toolStripMenuItem86.Name = "toolStripMenuItem86";
            this.toolStripMenuItem86.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem87
            // 
            this.toolStripMenuItem87.Name = "toolStripMenuItem87";
            this.toolStripMenuItem87.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem88
            // 
            this.toolStripMenuItem88.Name = "toolStripMenuItem88";
            this.toolStripMenuItem88.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem89
            // 
            this.toolStripMenuItem89.Name = "toolStripMenuItem89";
            this.toolStripMenuItem89.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem90
            // 
            this.toolStripMenuItem90.Name = "toolStripMenuItem90";
            this.toolStripMenuItem90.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem91
            // 
            this.toolStripMenuItem91.Name = "toolStripMenuItem91";
            this.toolStripMenuItem91.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem92
            // 
            this.toolStripMenuItem92.Name = "toolStripMenuItem92";
            this.toolStripMenuItem92.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem93
            // 
            this.toolStripMenuItem93.Name = "toolStripMenuItem93";
            this.toolStripMenuItem93.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem94
            // 
            this.toolStripMenuItem94.Name = "toolStripMenuItem94";
            this.toolStripMenuItem94.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem95
            // 
            this.toolStripMenuItem95.Name = "toolStripMenuItem95";
            this.toolStripMenuItem95.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox8
            // 
            this.toolStripTextBox8.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox8.Name = "toolStripTextBox8";
            this.toolStripTextBox8.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem96
            // 
            this.toolStripMenuItem96.Name = "toolStripMenuItem96";
            this.toolStripMenuItem96.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem97
            // 
            this.toolStripMenuItem97.Name = "toolStripMenuItem97";
            this.toolStripMenuItem97.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem98
            // 
            this.toolStripMenuItem98.Name = "toolStripMenuItem98";
            this.toolStripMenuItem98.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem99
            // 
            this.toolStripMenuItem99.Name = "toolStripMenuItem99";
            this.toolStripMenuItem99.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem100
            // 
            this.toolStripMenuItem100.Name = "toolStripMenuItem100";
            this.toolStripMenuItem100.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem101
            // 
            this.toolStripMenuItem101.Name = "toolStripMenuItem101";
            this.toolStripMenuItem101.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem102
            // 
            this.toolStripMenuItem102.Name = "toolStripMenuItem102";
            this.toolStripMenuItem102.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem103
            // 
            this.toolStripMenuItem103.Name = "toolStripMenuItem103";
            this.toolStripMenuItem103.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem104
            // 
            this.toolStripMenuItem104.Name = "toolStripMenuItem104";
            this.toolStripMenuItem104.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem105
            // 
            this.toolStripMenuItem105.Name = "toolStripMenuItem105";
            this.toolStripMenuItem105.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox9
            // 
            this.toolStripTextBox9.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox9.Name = "toolStripTextBox9";
            this.toolStripTextBox9.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem106
            // 
            this.toolStripMenuItem106.Name = "toolStripMenuItem106";
            this.toolStripMenuItem106.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem107
            // 
            this.toolStripMenuItem107.Name = "toolStripMenuItem107";
            this.toolStripMenuItem107.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem108
            // 
            this.toolStripMenuItem108.Name = "toolStripMenuItem108";
            this.toolStripMenuItem108.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem109
            // 
            this.toolStripMenuItem109.Name = "toolStripMenuItem109";
            this.toolStripMenuItem109.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem110
            // 
            this.toolStripMenuItem110.Name = "toolStripMenuItem110";
            this.toolStripMenuItem110.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem111
            // 
            this.toolStripMenuItem111.Name = "toolStripMenuItem111";
            this.toolStripMenuItem111.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem112
            // 
            this.toolStripMenuItem112.Name = "toolStripMenuItem112";
            this.toolStripMenuItem112.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem113
            // 
            this.toolStripMenuItem113.Name = "toolStripMenuItem113";
            this.toolStripMenuItem113.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem114
            // 
            this.toolStripMenuItem114.Name = "toolStripMenuItem114";
            this.toolStripMenuItem114.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem115
            // 
            this.toolStripMenuItem115.Name = "toolStripMenuItem115";
            this.toolStripMenuItem115.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox10
            // 
            this.toolStripTextBox10.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox10.Name = "toolStripTextBox10";
            this.toolStripTextBox10.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem116
            // 
            this.toolStripMenuItem116.Name = "toolStripMenuItem116";
            this.toolStripMenuItem116.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem117
            // 
            this.toolStripMenuItem117.Name = "toolStripMenuItem117";
            this.toolStripMenuItem117.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem118
            // 
            this.toolStripMenuItem118.Name = "toolStripMenuItem118";
            this.toolStripMenuItem118.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem119
            // 
            this.toolStripMenuItem119.Name = "toolStripMenuItem119";
            this.toolStripMenuItem119.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem120
            // 
            this.toolStripMenuItem120.Name = "toolStripMenuItem120";
            this.toolStripMenuItem120.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem121
            // 
            this.toolStripMenuItem121.Name = "toolStripMenuItem121";
            this.toolStripMenuItem121.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem122
            // 
            this.toolStripMenuItem122.Name = "toolStripMenuItem122";
            this.toolStripMenuItem122.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem123
            // 
            this.toolStripMenuItem123.Name = "toolStripMenuItem123";
            this.toolStripMenuItem123.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem124
            // 
            this.toolStripMenuItem124.Name = "toolStripMenuItem124";
            this.toolStripMenuItem124.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem125
            // 
            this.toolStripMenuItem125.Name = "toolStripMenuItem125";
            this.toolStripMenuItem125.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox11
            // 
            this.toolStripTextBox11.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox11.Name = "toolStripTextBox11";
            this.toolStripTextBox11.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripMenuItem25
            // 
            this.toolStripMenuItem25.Name = "toolStripMenuItem25";
            this.toolStripMenuItem25.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem26
            // 
            this.toolStripMenuItem26.Name = "toolStripMenuItem26";
            this.toolStripMenuItem26.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem27
            // 
            this.toolStripMenuItem27.Name = "toolStripMenuItem27";
            this.toolStripMenuItem27.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem28
            // 
            this.toolStripMenuItem28.Name = "toolStripMenuItem28";
            this.toolStripMenuItem28.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem29
            // 
            this.toolStripMenuItem29.Name = "toolStripMenuItem29";
            this.toolStripMenuItem29.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem30
            // 
            this.toolStripMenuItem30.Name = "toolStripMenuItem30";
            this.toolStripMenuItem30.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem31
            // 
            this.toolStripMenuItem31.Name = "toolStripMenuItem31";
            this.toolStripMenuItem31.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem32
            // 
            this.toolStripMenuItem32.Name = "toolStripMenuItem32";
            this.toolStripMenuItem32.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem33
            // 
            this.toolStripMenuItem33.Name = "toolStripMenuItem33";
            this.toolStripMenuItem33.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem34
            // 
            this.toolStripMenuItem34.Name = "toolStripMenuItem34";
            this.toolStripMenuItem34.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 23);
            // 
            // toolTip1
            // 
            this.toolTip1.UseAnimation = false;
            // 
            // toolStripMenuItem18
            // 
            this.toolStripMenuItem18.Name = "toolStripMenuItem18";
            this.toolStripMenuItem18.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem19
            // 
            this.toolStripMenuItem19.Name = "toolStripMenuItem19";
            this.toolStripMenuItem19.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem20
            // 
            this.toolStripMenuItem20.Name = "toolStripMenuItem20";
            this.toolStripMenuItem20.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem21
            // 
            this.toolStripMenuItem21.Name = "toolStripMenuItem21";
            this.toolStripMenuItem21.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem22
            // 
            this.toolStripMenuItem22.Name = "toolStripMenuItem22";
            this.toolStripMenuItem22.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem23
            // 
            this.toolStripMenuItem23.Name = "toolStripMenuItem23";
            this.toolStripMenuItem23.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem24
            // 
            this.toolStripMenuItem24.Name = "toolStripMenuItem24";
            this.toolStripMenuItem24.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem126
            // 
            this.toolStripMenuItem126.Name = "toolStripMenuItem126";
            this.toolStripMenuItem126.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem127
            // 
            this.toolStripMenuItem127.Name = "toolStripMenuItem127";
            this.toolStripMenuItem127.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem128
            // 
            this.toolStripMenuItem128.Name = "toolStripMenuItem128";
            this.toolStripMenuItem128.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripTextBox2
            // 
            this.toolStripTextBox2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox2.Name = "toolStripTextBox2";
            this.toolStripTextBox2.Size = new System.Drawing.Size(100, 23);
            // 
            // RC_ServerManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(829, 511);
            this.Controls.Add(this.serverManager_container);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RC_ServerManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Server Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerManager_FormClosing);
            this.Load += new System.EventHandler(this.ServerManager_Load);
            this.serverManager_container.ResumeLayout(false);
            this.page_players.ResumeLayout(false);
            this.page_players.PerformLayout();
            this.group_currentPlayers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid_playerList)).EndInit();
            this.tabControl_playerSelection.ResumeLayout(false);
            this.tab_bannedPlayers.ResumeLayout(false);
            this.group_addBan.ResumeLayout(false);
            this.group_addBan.PerformLayout();
            this.group_banDetails.ResumeLayout(false);
            this.group_banDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid_bannedPlayerList)).EndInit();
            this.tab_vpnSettings.ResumeLayout(false);
            this.group_vpnWhitelist.ResumeLayout(false);
            this.group_vpnWhitelist.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid_vpn_allowlist)).EndInit();
            this.group_vpnSettings.ResumeLayout(false);
            this.group_vpnSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.value_vpn_abuselevel)).EndInit();
            this.tab_warnMsgs.ResumeLayout(false);
            this.groupBox_playerWarnMsg.ResumeLayout(false);
            this.groupBox_playerWarnMsg.PerformLayout();
            this.page_serverSettings.ResumeLayout(false);
            this.gB_ServerSettings.ResumeLayout(false);
            this.gB_ServerSettings.PerformLayout();
            this.gB_motd.ResumeLayout(false);
            this.gB_ServerDetails.ResumeLayout(false);
            this.gB_ServerDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rcNum_maxSlots)).EndInit();
            this.gB_options.ResumeLayout(false);
            this.tabCtl_ServerSettings.ResumeLayout(false);
            this.tabPage9.ResumeLayout(false);
            this.tabPage9.PerformLayout();
            this.gb_pingOptions.ResumeLayout(false);
            this.gb_pingOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_maxPing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_minPing)).EndInit();
            this.gb_misc.ResumeLayout(false);
            this.gb_misc.PerformLayout();
            this.gb_GamePlay.ResumeLayout(false);
            this.gb_GamePlay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_MaxTeamLives)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_pspTimer)).EndInit();
            this.gb_FriendlyFire.ResumeLayout(false);
            this.gb_FriendlyFire.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_maxFriendKills)).EndInit();
            this.gB_TeamPasswords.ResumeLayout(false);
            this.gB_TeamPasswords.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_scoreDM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_scoreKOTH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_scoreFB)).EndInit();
            this.tabPage14.ResumeLayout(false);
            this.groupBox22.ResumeLayout(false);
            this.groupBox22.PerformLayout();
            this.gb_roleRestrictions.ResumeLayout(false);
            this.gb_roleRestrictions.PerformLayout();
            this.page_maps.ResumeLayout(false);
            this.gb_MapsAvailable.ResumeLayout(false);
            this.gb_MapsAvailable.PerformLayout();
            this.gB_mapRotation.ResumeLayout(false);
            this.gB_mapRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.endOfMapTimer_TrackBar)).EndInit();
            this.panel_mapControls.ResumeLayout(false);
            this.panel_mapControls.PerformLayout();
            this.page_chat.ResumeLayout(false);
            this.panel_chatMessage.ResumeLayout(false);
            this.panel_chatMessage.PerformLayout();
            this.panel_chatControls.ResumeLayout(false);
            this.groupBox_chatChannel.ResumeLayout(false);
            this.groupBox_chatChannel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.data_chatViewer)).EndInit();
            this.page_autoMessages.ResumeLayout(false);
            this.gb_autoMessages.ResumeLayout(false);
            this.gb_autoMessages.PerformLayout();
            this.gb_addMessages.ResumeLayout(false);
            this.gb_addMessages.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_autoMsgInterval)).EndInit();
            this.playerList_contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private ToolStripMenuItem CreateReasonItem(string text, string action, bool permBan = false, int days = 0)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Name = "actionReason_" + text.Replace(" ", ""); // Unique name for each item
            item.Size = new System.Drawing.Size(209, 22);
            item.Text = text;
            item.Click += (sender, e) => playerListAction_click(sender, e, action, permBan, days); // Set the click event with the specified action
            return item;
        }

        private ToolStripTextBox CreateReasonTextBox(string text, string action, bool permBan = false, int days = 0)
        {
            ToolStripTextBox textBox = new ToolStripTextBox();
            textBox.Name = "actionReason_" + text.Replace(" ", ""); // Unique name for each item
            textBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            textBox.Size = new System.Drawing.Size(209, 23);
            textBox.Text = "Custom Reason";
            textBox.KeyDown += (sender, e) => playerListAction_custom(sender, e, action, permBan, days); // Set the keydown event with the specified action
            return textBox;
        }

        #endregion

        private System.Windows.Forms.TabControl serverManager_container;
        private System.Windows.Forms.TabPage page_players;
        private System.Windows.Forms.TabControl tabControl_playerSelection;
        private System.Windows.Forms.TabPage tab_bannedPlayers;
        private System.Windows.Forms.GroupBox group_addBan;
        private System.Windows.Forms.TextBox text_abIPAddress;
        private System.Windows.Forms.TextBox text_adPlayerName;
        private System.Windows.Forms.ComboBox combo_abReason;
        private System.Windows.Forms.Button button_addBan;
        private System.Windows.Forms.Label label_abIPAddress;
        private System.Windows.Forms.Label label_abPlayerName;
        private System.Windows.Forms.GroupBox group_banDetails;
        private System.Windows.Forms.Label value_banAdmin;
        private System.Windows.Forms.Label value_banDateAdded;
        private System.Windows.Forms.Label value_banReason;
        private System.Windows.Forms.Label value_bdPlayerName;
        private System.Windows.Forms.Label label_banAdmin;
        private System.Windows.Forms.Label label_banReason;
        private System.Windows.Forms.Label label_banDateAdded;
        private System.Windows.Forms.Label label_bdPlayerName;
        private System.Windows.Forms.Button button_removeBan;
        private System.Windows.Forms.DataGridView grid_bannedPlayerList;
        private System.Windows.Forms.TabPage tab_vpnSettings;
        private System.Windows.Forms.GroupBox group_vpnWhitelist;
        private System.Windows.Forms.TextBox textBox_vpnAddress;
        private System.Windows.Forms.TextBox textBox_vpnDescription;
        private System.Windows.Forms.Label label_vpnAddress;
        private System.Windows.Forms.Label label_vpnDescription;
        private System.Windows.Forms.DataGridView grid_vpn_allowlist;
        private System.Windows.Forms.GroupBox group_vpnSettings;
        private System.Windows.Forms.Label label_vpn_abuse;
        private System.Windows.Forms.NumericUpDown value_vpn_abuselevel;
        private System.Windows.Forms.CheckBox checkBox_vpn_disallow;
        private System.Windows.Forms.TabPage tab_warnMsgs;
        private System.Windows.Forms.GroupBox groupBox_playerWarnMsg;
        private System.Windows.Forms.Button btn_playerWarnMessageAdd;
        private System.Windows.Forms.TextBox textBox_playerWarnMessageAdd;
        private System.Windows.Forms.ListBox listBox_playerWarnMessages;
        private System.Windows.Forms.Label label69;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage page_serverSettings;
        private System.Windows.Forms.ComboBox cb_respawnTime;
        private System.Windows.Forms.ComboBox cb_startDelay;
        private System.Windows.Forms.ComboBox cb_timeLimit;
        private System.Windows.Forms.Button btn_settingsRevertChanges;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btn_UpdateServerSettings;
        private System.Windows.Forms.CheckBox cb_autoBalance;
        private System.Windows.Forms.CheckBox cb_customSkin;
        private System.Windows.Forms.CheckBox cb_requireNova;
        private System.Windows.Forms.CheckBox cb_gameDedicated;
        private System.Windows.Forms.ComboBox cb_replayMaps;
        private System.Windows.Forms.Label label_respawnTime;
        private System.Windows.Forms.Label label_replayMaps;
        private System.Windows.Forms.Label label_startDelay;
        private System.Windows.Forms.Label label_timeLimit;
        private System.Windows.Forms.TextBox rcT_serverPassword;
        private System.Windows.Forms.Label label_ServerPassword;
        private System.Windows.Forms.ComboBox rcCB_sessionType;
        private System.Windows.Forms.Label label_SessionType;
        private System.Windows.Forms.ComboBox rcCB_country;
        private System.Windows.Forms.Label label_Country;
        private System.Windows.Forms.NumericUpDown rcNum_maxSlots;
        private System.Windows.Forms.Label label_maxSlots;
        private System.Windows.Forms.TextBox rcT_serverName;
        private System.Windows.Forms.Label label_serverName;
        private System.Windows.Forms.TabControl tabCtl_ServerSettings;
        private System.Windows.Forms.TabPage tabPage9;
        private System.Windows.Forms.GroupBox gb_GamePlay;
        private System.Windows.Forms.Label label_flagReturnTimer;
        private System.Windows.Forms.NumericUpDown numericUpDown12;
        private System.Windows.Forms.NumericUpDown num_MaxTeamLives;
        private System.Windows.Forms.Label label_maxTeamLives;
        private System.Windows.Forms.NumericUpDown num_pspTimer;
        private System.Windows.Forms.Label label_PSPtime;
        private System.Windows.Forms.GroupBox gb_FriendlyFire;
        private System.Windows.Forms.CheckBox checkBox40;
        private System.Windows.Forms.CheckBox cb_showFriendTags;
        private System.Windows.Forms.Label label_maxFriendKills;
        private System.Windows.Forms.NumericUpDown num_maxFriendKills;
        private System.Windows.Forms.CheckBox cb_ffWarning;
        private System.Windows.Forms.GroupBox gB_TeamPasswords;
        private System.Windows.Forms.Label label_bluePass;
        private System.Windows.Forms.Label label_redPass;
        private System.Windows.Forms.TextBox text_redPass;
        private System.Windows.Forms.TextBox text_bluePass;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.NumericUpDown num_scoreDM;
        private System.Windows.Forms.NumericUpDown num_scoreKOTH;
        private System.Windows.Forms.NumericUpDown num_scoreFB;
        private System.Windows.Forms.Label label_scoreDM;
        private System.Windows.Forms.Label label_scoreKOTH;
        private System.Windows.Forms.Label label_scoreFB;
        private System.Windows.Forms.CheckBox cb_autoLastKnown;
        private System.Windows.Forms.GroupBox gb_misc;
        private System.Windows.Forms.CheckBox cb_destroyBuildings;
        private System.Windows.Forms.CheckBox cb_fatBullets;
        private System.Windows.Forms.CheckBox cb_oneShotKills;
        private System.Windows.Forms.GroupBox gb_pingOptions;
        private System.Windows.Forms.NumericUpDown num_maxPing;
        private System.Windows.Forms.NumericUpDown num_minPing;
        private System.Windows.Forms.CheckBox cb_minPing;
        private System.Windows.Forms.CheckBox cb_maxPing;
        private System.Windows.Forms.TabPage tabPage14;
        private System.Windows.Forms.TabPage page_maps;
        private System.Windows.Forms.Panel panel_mapControls;
        private System.Windows.Forms.Label label103;
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.Button btn_SaveRoation;
        private System.Windows.Forms.Button btn_loadRotation;
        private System.Windows.Forms.Button btn_updateMaps;
        private System.Windows.Forms.Button btn_playMapNext;
        private System.Windows.Forms.Button btn_scoreSkip;
        private System.Windows.Forms.Button btn_moveMapDown;
        private System.Windows.Forms.Button btn_moveMapUp;
        private System.Windows.Forms.Button btn_ShuffleMaps;
        private System.Windows.Forms.GroupBox gB_mapRotation;
        private System.Windows.Forms.ListBox list_mapRotation;
        private System.Windows.Forms.Label label_currentMapPlaying;
        private System.Windows.Forms.Label label_EOMtimer;
        private System.Windows.Forms.TrackBar endOfMapTimer_TrackBar;
        private System.Windows.Forms.Label label_CurrentMap;
        private System.Windows.Forms.Label label_currentMapCount;
        private System.Windows.Forms.Label label_mapsSelected;
        private System.Windows.Forms.GroupBox gb_MapsAvailable;
        private System.Windows.Forms.ListBox listBox_mapsAvailable;
        private System.Windows.Forms.Label label_numMapsAvailable;
        private System.Windows.Forms.Label label_totalMaps;
        private System.Windows.Forms.Label label_gameType;
        private System.Windows.Forms.ComboBox dropDown_mapSettingsGameType;
        private System.Windows.Forms.TabPage page_chat;
        private System.Windows.Forms.DataGridView data_chatViewer;
        private System.Windows.Forms.ComboBox cb_chatPlayerSelect;
        private System.Windows.Forms.TextBox chat_textBoxMsg;
        private System.Windows.Forms.Button btn_sendChat;
        private System.Windows.Forms.TabPage page_autoMessages;
        private System.Windows.Forms.NumericUpDown num_autoMsgInterval;
        private System.Windows.Forms.CheckBox cb_enableAutoMsg;
        private System.Windows.Forms.TextBox text_newAutoMessage;
        private System.Windows.Forms.Button btn_addAutoMsg;
        private System.Windows.Forms.ListBox listBox_AutoMessages;
        private System.Windows.Forms.ContextMenuStrip playerList_contextMenu;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_1Day;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_2Day;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_3Day;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_4Day;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_5Day;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_6Day;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_1Week;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_2Week;
        private System.Windows.Forms.ToolStripMenuItem StripMenu_1Month;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem14;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem133;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem76;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem129;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem130;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem131;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Abusive;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Racism;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Cheating;
        private System.Windows.Forms.ToolStripMenuItem actionReason_WallHack;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Aimbot;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Speed;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Disrespect;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Camping;
        private System.Windows.Forms.ToolStripMenuItem actionReason_TPK;
        private System.Windows.Forms.ToolStripMenuItem actionReason_Rules;
        private System.Windows.Forms.ToolStripSeparator playerListMenu_Seperator;
        private System.Windows.Forms.ToolStripTextBox actionReason_Custom;
        private System.Windows.Forms.ToolStripMenuItem playerListMenu_tempBan;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem15;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem37;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem38;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem39;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem40;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem41;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem42;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem43;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem44;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem45;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem17;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem46;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem47;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem48;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem49;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem50;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem51;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem52;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem53;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem54;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem55;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem56;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem57;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem58;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem59;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem60;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem61;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem62;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem63;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem64;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem65;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem66;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem67;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem68;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem69;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem70;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem71;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem72;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem73;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem74;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem75;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem77;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem78;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem79;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem80;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem81;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem82;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem83;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem84;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem85;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem86;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem87;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem88;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem89;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem90;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem91;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem92;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem93;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem94;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem95;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem96;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem97;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem98;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem99;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem100;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem101;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem102;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem103;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem104;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem105;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem106;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem107;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem108;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem109;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem110;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem111;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem112;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem113;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem114;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem115;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem116;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem117;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem118;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem119;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem120;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem121;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem122;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem123;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem124;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem125;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox11;
        private System.Windows.Forms.ToolStripMenuItem playerListMenu_permBanPlayer;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem25;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem26;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem27;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem28;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem29;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem30;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem31;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem32;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem33;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem34;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripMenuItem playerListMenu_changeTeams;
        private System.Windows.Forms.ComboBox chat_channelSelection;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem playerListMenu_spectate;
        private System.Windows.Forms.Label label_currentSpec;
        private System.Windows.Forms.GroupBox group_currentPlayers;
        private System.Windows.Forms.DataGridView grid_playerList;
        private System.Windows.Forms.Label value_banIPAddress;
        private System.Windows.Forms.Label label_banIPAddress;
        private System.Windows.Forms.Button btn_cancelChanges;
        private System.Windows.Forms.Button btn_deleteAddress;
        private System.Windows.Forms.Button btn_addAddress;
        private GroupBox gB_options;
        private GroupBox gB_ServerDetails;
        private GroupBox gB_ServerSettings;
        private GroupBox gB_motd;
        private CheckBox cb_Tracers;
        private CheckBox cb_TeamClays;
        private CheckBox cb_AutoRange;
        private GroupBox gb_roleRestrictions;
        private RadioButton rb_roleSelNone;
        private RadioButton rb_roleSelAll;
        private CheckedListBox cbl_roleSelection;
        private GroupBox groupBox22;
        private RadioButton rb_WeaonSelNone;
        private RadioButton rb_WeaonSelAll;
        private CheckedListBox cbl_weaponSelection;
        private Panel panel_chatMessage;
        private Panel panel_chatControls;
        private GroupBox gb_autoMessages;
        private Label label_interval;
        private GroupBox gb_addMessages;
        private GroupBox groupBox_chatChannel;
        private RadioButton rb_chatAll;
        private RadioButton rb_chatGlobal;
        private RadioButton rb_chatPlayerHist;
        private RadioButton rb_chatRedTeam;
        private RadioButton rb_chatBlueTeam;
        private ToolStripMenuItem cm_kickPlayer;
        private ToolStripMenuItem toolStripMenuItem18;
        private ToolStripMenuItem toolStripMenuItem19;
        private ToolStripMenuItem toolStripMenuItem20;
        private ToolStripMenuItem toolStripMenuItem21;
        private ToolStripMenuItem toolStripMenuItem22;
        private ToolStripMenuItem toolStripMenuItem23;
        private ToolStripMenuItem toolStripMenuItem24;
        private ToolStripMenuItem toolStripMenuItem126;
        private ToolStripMenuItem toolStripMenuItem127;
        private ToolStripMenuItem toolStripMenuItem128;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripTextBox toolStripTextBox2;
    }
}