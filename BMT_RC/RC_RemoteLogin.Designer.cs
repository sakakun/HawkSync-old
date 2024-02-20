
namespace HawkSync_RC
{
    partial class RC_RemoteLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RC_RemoteLogin));
            this.tb_userPassword = new System.Windows.Forms.TextBox();
            this.label_userName = new System.Windows.Forms.Label();
            this.tb_userName = new System.Windows.Forms.TextBox();
            this.label_profileSelection = new System.Windows.Forms.Label();
            this.cb_serverSelection = new System.Windows.Forms.ComboBox();
            this.label_hostPort = new System.Windows.Forms.Label();
            this.label_profileName = new System.Windows.Forms.Label();
            this.tb_hostIP = new System.Windows.Forms.TextBox();
            this.tb_profileName = new System.Windows.Forms.TextBox();
            this.label_hostIP = new System.Windows.Forms.Label();
            this.num_hostPort = new System.Windows.Forms.NumericUpDown();
            this.tabContainerMainRemote = new System.Windows.Forms.TabControl();
            this.loginPage = new System.Windows.Forms.TabPage();
            this.btn_openOptions = new System.Windows.Forms.Button();
            this.btn_quit = new System.Windows.Forms.Button();
            this.btn_launchRC = new System.Windows.Forms.Button();
            this.gp_loginForm = new System.Windows.Forms.GroupBox();
            this.label_conStatus = new System.Windows.Forms.Label();
            this.connectionStatus = new System.Windows.Forms.Label();
            this.label_userPassword = new System.Windows.Forms.Label();
            this.btn_connectServer = new System.Windows.Forms.Button();
            this.btn_saveProfile = new System.Windows.Forms.Button();
            this.btn_deleteProfile = new System.Windows.Forms.Button();
            this.optionsPage = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.num_hostPort)).BeginInit();
            this.tabContainerMainRemote.SuspendLayout();
            this.loginPage.SuspendLayout();
            this.gp_loginForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // tb_userPassword
            // 
            this.tb_userPassword.Location = new System.Drawing.Point(83, 143);
            this.tb_userPassword.Name = "tb_userPassword";
            this.tb_userPassword.Size = new System.Drawing.Size(150, 20);
            this.tb_userPassword.TabIndex = 5;
            this.tb_userPassword.PasswordChar = '*';
            // 
            // label_userName
            // 
            this.label_userName.AutoSize = true;
            this.label_userName.Location = new System.Drawing.Point(19, 120);
            this.label_userName.Name = "label_userName";
            this.label_userName.Size = new System.Drawing.Size(58, 13);
            this.label_userName.TabIndex = 11;
            this.label_userName.Text = "Username:";
            this.label_userName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_userName
            // 
            this.tb_userName.Location = new System.Drawing.Point(83, 117);
            this.tb_userName.Name = "tb_userName";
            this.tb_userName.Size = new System.Drawing.Size(150, 20);
            this.tb_userName.TabIndex = 4;
            // 
            // label_profileSelection
            // 
            this.label_profileSelection.AutoSize = true;
            this.label_profileSelection.Location = new System.Drawing.Point(4, 15);
            this.label_profileSelection.Name = "label_profileSelection";
            this.label_profileSelection.Size = new System.Drawing.Size(73, 13);
            this.label_profileSelection.TabIndex = 1;
            this.label_profileSelection.Text = "Server Profile:";
            this.label_profileSelection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cb_serverSelection
            // 
            this.cb_serverSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_serverSelection.FormattingEnabled = true;
            this.cb_serverSelection.Location = new System.Drawing.Point(83, 12);
            this.cb_serverSelection.Name = "cb_serverSelection";
            this.cb_serverSelection.Size = new System.Drawing.Size(150, 21);
            this.cb_serverSelection.TabIndex = 0;
            this.cb_serverSelection.SelectedIndexChanged += new System.EventHandler(this.event_serverSelectionChanged);
            // 
            // label_hostPort
            // 
            this.label_hostPort.AutoSize = true;
            this.label_hostPort.Location = new System.Drawing.Point(48, 93);
            this.label_hostPort.Name = "label_hostPort";
            this.label_hostPort.Size = new System.Drawing.Size(29, 13);
            this.label_hostPort.TabIndex = 9;
            this.label_hostPort.Text = "Port:";
            this.label_hostPort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_profileName
            // 
            this.label_profileName.AutoSize = true;
            this.label_profileName.Location = new System.Drawing.Point(7, 42);
            this.label_profileName.Name = "label_profileName";
            this.label_profileName.Size = new System.Drawing.Size(70, 13);
            this.label_profileName.TabIndex = 5;
            this.label_profileName.Text = "Profile Name:";
            this.label_profileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_hostIP
            // 
            this.tb_hostIP.Location = new System.Drawing.Point(83, 65);
            this.tb_hostIP.Name = "tb_hostIP";
            this.tb_hostIP.Size = new System.Drawing.Size(150, 20);
            this.tb_hostIP.TabIndex = 2;
            // 
            // tb_profileName
            // 
            this.tb_profileName.Location = new System.Drawing.Point(83, 39);
            this.tb_profileName.Name = "tb_profileName";
            this.tb_profileName.Size = new System.Drawing.Size(150, 20);
            this.tb_profileName.TabIndex = 1;
            // 
            // label_hostIP
            // 
            this.label_hostIP.AutoSize = true;
            this.label_hostIP.Location = new System.Drawing.Point(19, 68);
            this.label_hostIP.Name = "label_hostIP";
            this.label_hostIP.Size = new System.Drawing.Size(58, 13);
            this.label_hostIP.TabIndex = 7;
            this.label_hostIP.Text = "Hostname:";
            this.label_hostIP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // num_hostPort
            // 
            this.num_hostPort.Location = new System.Drawing.Point(83, 91);
            this.num_hostPort.Maximum = new decimal(new int[] {
            65555,
            0,
            0,
            0});
            this.num_hostPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.num_hostPort.Name = "num_hostPort";
            this.num_hostPort.Size = new System.Drawing.Size(60, 20);
            this.num_hostPort.TabIndex = 3;
            this.num_hostPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.num_hostPort.Value = new decimal(new int[] {
            4173,
            0,
            0,
            0});
            // 
            // tabContainerMainRemote
            // 
            this.tabContainerMainRemote.Controls.Add(this.loginPage);
            this.tabContainerMainRemote.Controls.Add(this.optionsPage);
            this.tabContainerMainRemote.Location = new System.Drawing.Point(12, 12);
            this.tabContainerMainRemote.Name = "tabContainerMainRemote";
            this.tabContainerMainRemote.SelectedIndex = 0;
            this.tabContainerMainRemote.Size = new System.Drawing.Size(352, 230);
            this.tabContainerMainRemote.TabIndex = 1;
            // 
            // loginPage
            // 
            this.loginPage.Controls.Add(this.btn_openOptions);
            this.loginPage.Controls.Add(this.btn_quit);
            this.loginPage.Controls.Add(this.btn_launchRC);
            this.loginPage.Controls.Add(this.gp_loginForm);
            this.loginPage.Controls.Add(this.btn_connectServer);
            this.loginPage.Controls.Add(this.btn_saveProfile);
            this.loginPage.Controls.Add(this.btn_deleteProfile);
            this.loginPage.Location = new System.Drawing.Point(4, 22);
            this.loginPage.Name = "loginPage";
            this.loginPage.Size = new System.Drawing.Size(344, 204);
            this.loginPage.TabIndex = 0;
            this.loginPage.Text = "Login";
            this.loginPage.UseVisualStyleBackColor = true;
            // 
            // btn_openOptions
            // 
            this.btn_openOptions.Location = new System.Drawing.Point(257, 132);
            this.btn_openOptions.Name = "btn_openOptions";
            this.btn_openOptions.Size = new System.Drawing.Size(79, 23);
            this.btn_openOptions.TabIndex = 10;
            this.btn_openOptions.Text = "Options";
            this.btn_openOptions.UseVisualStyleBackColor = true;
            this.btn_openOptions.Click += new System.EventHandler(this.event_openOptions);
            // 
            // btn_quit
            // 
            this.btn_quit.Location = new System.Drawing.Point(257, 175);
            this.btn_quit.Name = "btn_quit";
            this.btn_quit.Size = new System.Drawing.Size(79, 23);
            this.btn_quit.TabIndex = 11;
            this.btn_quit.Text = "Quit";
            this.btn_quit.UseVisualStyleBackColor = true;
            this.btn_quit.Click += new System.EventHandler(this.event_quitRemoteControl);
            // 
            // btn_launchRC
            // 
            this.btn_launchRC.Enabled = false;
            this.btn_launchRC.Location = new System.Drawing.Point(257, 106);
            this.btn_launchRC.Name = "btn_launchRC";
            this.btn_launchRC.Size = new System.Drawing.Size(79, 23);
            this.btn_launchRC.TabIndex = 7;
            this.btn_launchRC.Text = "Launch RC";
            this.btn_launchRC.UseVisualStyleBackColor = true;
            this.btn_launchRC.Click += new System.EventHandler(this.event_openServerManager);
            // 
            // gp_loginForm
            // 
            this.gp_loginForm.Controls.Add(this.label_conStatus);
            this.gp_loginForm.Controls.Add(this.label_profileSelection);
            this.gp_loginForm.Controls.Add(this.connectionStatus);
            this.gp_loginForm.Controls.Add(this.cb_serverSelection);
            this.gp_loginForm.Controls.Add(this.label_userName);
            this.gp_loginForm.Controls.Add(this.label_userPassword);
            this.gp_loginForm.Controls.Add(this.num_hostPort);
            this.gp_loginForm.Controls.Add(this.tb_userPassword);
            this.gp_loginForm.Controls.Add(this.tb_userName);
            this.gp_loginForm.Controls.Add(this.label_profileName);
            this.gp_loginForm.Controls.Add(this.label_hostIP);
            this.gp_loginForm.Controls.Add(this.label_hostPort);
            this.gp_loginForm.Controls.Add(this.tb_hostIP);
            this.gp_loginForm.Controls.Add(this.tb_profileName);
            this.gp_loginForm.Location = new System.Drawing.Point(3, 3);
            this.gp_loginForm.Name = "gp_loginForm";
            this.gp_loginForm.Size = new System.Drawing.Size(248, 196);
            this.gp_loginForm.TabIndex = 15;
            this.gp_loginForm.TabStop = false;
            // 
            // label_conStatus
            // 
            this.label_conStatus.AutoSize = true;
            this.label_conStatus.Location = new System.Drawing.Point(37, 172);
            this.label_conStatus.Name = "label_conStatus";
            this.label_conStatus.Size = new System.Drawing.Size(40, 13);
            this.label_conStatus.TabIndex = 12;
            this.label_conStatus.Text = "Status:";
            // 
            // connectionStatus
            // 
            this.connectionStatus.AutoSize = true;
            this.connectionStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.connectionStatus.Location = new System.Drawing.Point(114, 172);
            this.connectionStatus.Name = "connectionStatus";
            this.connectionStatus.Size = new System.Drawing.Size(79, 13);
            this.connectionStatus.TabIndex = 13;
            this.connectionStatus.Text = "Not Connected";
            this.connectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_userPassword
            // 
            this.label_userPassword.AutoSize = true;
            this.label_userPassword.Location = new System.Drawing.Point(21, 146);
            this.label_userPassword.Name = "label_userPassword";
            this.label_userPassword.Size = new System.Drawing.Size(56, 13);
            this.label_userPassword.TabIndex = 14;
            this.label_userPassword.Text = "Password:";
            this.label_userPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_connectServer
            // 
            this.btn_connectServer.Location = new System.Drawing.Point(257, 8);
            this.btn_connectServer.Name = "btn_connectServer";
            this.btn_connectServer.Size = new System.Drawing.Size(79, 23);
            this.btn_connectServer.TabIndex = 6;
            this.btn_connectServer.Text = "Connect";
            this.btn_connectServer.UseVisualStyleBackColor = true;
            this.btn_connectServer.Click += new System.EventHandler(this.event_connectServer);
            // 
            // btn_saveProfile
            // 
            this.btn_saveProfile.Location = new System.Drawing.Point(257, 35);
            this.btn_saveProfile.Name = "btn_saveProfile";
            this.btn_saveProfile.Size = new System.Drawing.Size(79, 23);
            this.btn_saveProfile.TabIndex = 8;
            this.btn_saveProfile.Text = "Save Profile";
            this.btn_saveProfile.UseVisualStyleBackColor = true;
            this.btn_saveProfile.Click += new System.EventHandler(this.event_saveProfile);
            // 
            // btn_deleteProfile
            // 
            this.btn_deleteProfile.Location = new System.Drawing.Point(257, 61);
            this.btn_deleteProfile.Name = "btn_deleteProfile";
            this.btn_deleteProfile.Size = new System.Drawing.Size(79, 23);
            this.btn_deleteProfile.TabIndex = 9;
            this.btn_deleteProfile.Text = "Delete Profile";
            this.btn_deleteProfile.UseVisualStyleBackColor = true;
            this.btn_deleteProfile.Click += new System.EventHandler(this.event_deleteProfile);
            // 
            // optionsPage
            // 
            this.optionsPage.Location = new System.Drawing.Point(4, 22);
            this.optionsPage.Name = "optionsPage";
            this.optionsPage.Padding = new System.Windows.Forms.Padding(3);
            this.optionsPage.Size = new System.Drawing.Size(344, 204);
            this.optionsPage.TabIndex = 1;
            this.optionsPage.Text = "Options";
            this.optionsPage.UseVisualStyleBackColor = true;
            // 
            // RC_RemoteLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 252);
            this.Controls.Add(this.tabContainerMainRemote);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RC_RemoteLogin";
            this.Text = "HawkSync Remote Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_Login_FormClosing);
            this.Load += new System.EventHandler(this.Main_Login_Load);
            ((System.ComponentModel.ISupportInitialize)(this.num_hostPort)).EndInit();
            this.tabContainerMainRemote.ResumeLayout(false);
            this.loginPage.ResumeLayout(false);
            this.gp_loginForm.ResumeLayout(false);
            this.gp_loginForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox tb_userPassword;
        private System.Windows.Forms.Label label_userName;
        private System.Windows.Forms.TextBox tb_userName;
        private System.Windows.Forms.Label label_profileSelection;
        private System.Windows.Forms.ComboBox cb_serverSelection;
        private System.Windows.Forms.Label label_hostPort;
        private System.Windows.Forms.Label label_profileName;
        private System.Windows.Forms.TextBox tb_hostIP;
        private System.Windows.Forms.TextBox tb_profileName;
        private System.Windows.Forms.Label label_hostIP;
        private System.Windows.Forms.NumericUpDown num_hostPort;
        private System.Windows.Forms.TabControl tabContainerMainRemote;
        private System.Windows.Forms.TabPage loginPage;
        private System.Windows.Forms.Button btn_openOptions;
        private System.Windows.Forms.Button btn_quit;
        private System.Windows.Forms.Button btn_saveProfile;
        private System.Windows.Forms.Label connectionStatus;
        private System.Windows.Forms.Label label_conStatus;
        private System.Windows.Forms.Label label_userPassword;
        private System.Windows.Forms.Button btn_deleteProfile;
        private System.Windows.Forms.Button btn_connectServer;
        private System.Windows.Forms.Button btn_launchRC;
        private System.Windows.Forms.TabPage optionsPage;
        private System.Windows.Forms.GroupBox gp_loginForm;
    }
}

