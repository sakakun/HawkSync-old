
namespace HawkSync_SM
{
    partial class SM_ServerProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SM_ServerProfile));
            this.cb_enableAnnoucments = new System.Windows.Forms.CheckBox();
            this.cb_enableAutoRestart = new System.Windows.Forms.CheckBox();
            this.gp_Options = new System.Windows.Forms.GroupBox();
            this.cb_enableHBNWCC = new System.Windows.Forms.CheckBox();
            this.cb_enableHBNHQ = new System.Windows.Forms.CheckBox();
            this.label_portNumber = new System.Windows.Forms.Label();
            this.num_serverPort = new System.Windows.Forms.NumericUpDown();
            this.textBox_hostName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_selectGame = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_submit = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.textBox_profileName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_statSystem = new System.Windows.Forms.ComboBox();
            this.btn_browse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textbox_serverPath = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cb_enableAntiPadding = new System.Windows.Forms.CheckBox();
            this.label_serverIP = new System.Windows.Forms.Label();
            this.label_statsID = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_statsURL = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_statsIdent = new System.Windows.Forms.TextBox();
            this.cb_serverIP = new System.Windows.Forms.ComboBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.gb_HBServers = new System.Windows.Forms.GroupBox();
            this.gp_Options.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_serverPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gb_HBServers.SuspendLayout();
            this.SuspendLayout();
            // 
            // cb_enableAnnoucments
            // 
            resources.ApplyResources(this.cb_enableAnnoucments, "cb_enableAnnoucments");
            this.cb_enableAnnoucments.Name = "cb_enableAnnoucments";
            this.cb_enableAnnoucments.UseVisualStyleBackColor = true;
            // 
            // cb_enableAutoRestart
            // 
            resources.ApplyResources(this.cb_enableAutoRestart, "cb_enableAutoRestart");
            this.cb_enableAutoRestart.Checked = true;
            this.cb_enableAutoRestart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_enableAutoRestart.Name = "cb_enableAutoRestart";
            this.cb_enableAutoRestart.UseVisualStyleBackColor = true;
            // 
            // gp_Options
            // 
            this.gp_Options.Controls.Add(this.cb_enableAnnoucments);
            this.gp_Options.Controls.Add(this.cb_enableAutoRestart);
            resources.ApplyResources(this.gp_Options, "gp_Options");
            this.gp_Options.Name = "gp_Options";
            this.gp_Options.TabStop = false;
            // 
            // cb_enableHBNWCC
            // 
            resources.ApplyResources(this.cb_enableHBNWCC, "cb_enableHBNWCC");
            this.cb_enableHBNWCC.Checked = true;
            this.cb_enableHBNWCC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_enableHBNWCC.Name = "cb_enableHBNWCC";
            this.cb_enableHBNWCC.UseVisualStyleBackColor = true;
            // 
            // cb_enableHBNHQ
            // 
            resources.ApplyResources(this.cb_enableHBNHQ, "cb_enableHBNHQ");
            this.cb_enableHBNHQ.Checked = true;
            this.cb_enableHBNHQ.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_enableHBNHQ.Name = "cb_enableHBNHQ";
            this.cb_enableHBNHQ.UseVisualStyleBackColor = true;
            // 
            // label_portNumber
            // 
            resources.ApplyResources(this.label_portNumber, "label_portNumber");
            this.label_portNumber.Name = "label_portNumber";
            // 
            // num_serverPort
            // 
            resources.ApplyResources(this.num_serverPort, "num_serverPort");
            this.num_serverPort.Maximum = new decimal(new int[] {
            49151,
            0,
            0,
            0});
            this.num_serverPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.num_serverPort.Name = "num_serverPort";
            this.num_serverPort.Value = new decimal(new int[] {
            17479,
            0,
            0,
            0});
            // 
            // textBox_hostName
            // 
            resources.ApplyResources(this.textBox_hostName, "textBox_hostName");
            this.textBox_hostName.Name = "textBox_hostName";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cb_selectGame);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btn_submit);
            this.groupBox1.Controls.Add(this.btn_cancel);
            this.groupBox1.Controls.Add(this.textBox_profileName);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cb_selectGame
            // 
            this.cb_selectGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_selectGame.FormattingEnabled = true;
            this.cb_selectGame.Items.AddRange(new object[] {
            resources.GetString("cb_selectGame.Items")});
            resources.ApplyResources(this.cb_selectGame, "cb_selectGame");
            this.cb_selectGame.Name = "cb_selectGame";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btn_submit
            // 
            resources.ApplyResources(this.btn_submit, "btn_submit");
            this.btn_submit.Name = "btn_submit";
            this.btn_submit.UseVisualStyleBackColor = true;
            this.btn_submit.Click += new System.EventHandler(this.btn_hideWindow);
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btn_cancel, "btn_cancel");
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.event_closeWindow);
            // 
            // textBox_profileName
            // 
            resources.ApplyResources(this.textBox_profileName, "textBox_profileName");
            this.textBox_profileName.Name = "textBox_profileName";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cb_statSystem
            // 
            this.cb_statSystem.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.cb_statSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_statSystem.FormattingEnabled = true;
            this.cb_statSystem.Items.AddRange(new object[] {
            resources.GetString("cb_statSystem.Items"),
            resources.GetString("cb_statSystem.Items1"),
            resources.GetString("cb_statSystem.Items2")});
            resources.ApplyResources(this.cb_statSystem, "cb_statSystem");
            this.cb_statSystem.Name = "cb_statSystem";
            this.cb_statSystem.SelectedIndexChanged += new System.EventHandler(this.event_webStatsChanged);
            // 
            // btn_browse
            // 
            resources.ApplyResources(this.btn_browse, "btn_browse");
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.event_openFileBrowser);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // textbox_serverPath
            // 
            resources.ApplyResources(this.textbox_serverPath, "textbox_serverPath");
            this.textbox_serverPath.Name = "textbox_serverPath";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cb_enableAntiPadding);
            this.groupBox2.Controls.Add(this.label_serverIP);
            this.groupBox2.Controls.Add(this.label_statsID);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBox_statsURL);
            this.groupBox2.Controls.Add(this.label_portNumber);
            this.groupBox2.Controls.Add(this.cb_statSystem);
            this.groupBox2.Controls.Add(this.btn_browse);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.textBox_hostName);
            this.groupBox2.Controls.Add(this.textbox_serverPath);
            this.groupBox2.Controls.Add(this.textBox_statsIdent);
            this.groupBox2.Controls.Add(this.cb_serverIP);
            this.groupBox2.Controls.Add(this.num_serverPort);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // cb_enableAntiPadding
            // 
            resources.ApplyResources(this.cb_enableAntiPadding, "cb_enableAntiPadding");
            this.cb_enableAntiPadding.Name = "cb_enableAntiPadding";
            this.cb_enableAntiPadding.UseVisualStyleBackColor = true;
            // 
            // label_serverIP
            // 
            resources.ApplyResources(this.label_serverIP, "label_serverIP");
            this.label_serverIP.Name = "label_serverIP";
            // 
            // label_statsID
            // 
            resources.ApplyResources(this.label_statsID, "label_statsID");
            this.label_statsID.Name = "label_statsID";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // textBox_statsURL
            // 
            resources.ApplyResources(this.textBox_statsURL, "textBox_statsURL");
            this.textBox_statsURL.Name = "textBox_statsURL";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // textBox_statsIdent
            // 
            resources.ApplyResources(this.textBox_statsIdent, "textBox_statsIdent");
            this.textBox_statsIdent.Name = "textBox_statsIdent";
            // 
            // cb_serverIP
            // 
            this.cb_serverIP.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.cb_serverIP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_serverIP.FormattingEnabled = true;
            resources.ApplyResources(this.cb_serverIP, "cb_serverIP");
            this.cb_serverIP.Name = "cb_serverIP";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // gb_HBServers
            // 
            this.gb_HBServers.Controls.Add(this.cb_enableHBNWCC);
            this.gb_HBServers.Controls.Add(this.cb_enableHBNHQ);
            resources.ApplyResources(this.gb_HBServers, "gb_HBServers");
            this.gb_HBServers.Name = "gb_HBServers";
            this.gb_HBServers.TabStop = false;
            // 
            // SM_ServerProfile
            // 
            this.AcceptButton = this.btn_submit;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.Controls.Add(this.gb_HBServers);
            this.Controls.Add(this.gp_Options);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SM_ServerProfile";
            this.gp_Options.ResumeLayout(false);
            this.gp_Options.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_serverPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gb_HBServers.ResumeLayout(false);
            this.gb_HBServers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox cb_enableAnnoucments;
        private System.Windows.Forms.CheckBox cb_enableAutoRestart;
        private System.Windows.Forms.GroupBox gp_Options;
        private System.Windows.Forms.Label label_portNumber;
        private System.Windows.Forms.NumericUpDown num_serverPort;
        private System.Windows.Forms.TextBox textBox_hostName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cb_statSystem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_selectGame;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textbox_serverPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_profileName;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_submit;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_statsIdent;
        private System.Windows.Forms.Label label_statsID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_statsURL;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label label_serverIP;
        private System.Windows.Forms.ComboBox cb_serverIP;
        private System.Windows.Forms.GroupBox gb_HBServers;
        private System.Windows.Forms.CheckBox cb_enableHBNWCC;
        private System.Windows.Forms.CheckBox cb_enableHBNHQ;
        private System.Windows.Forms.CheckBox cb_enableAntiPadding;
    }
}