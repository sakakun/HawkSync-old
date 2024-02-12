
namespace HawkSync_SM
{
    partial class SM_Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SM_Options));
            this.btn_save = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.gb_WindowsFirewall = new System.Windows.Forms.GroupBox();
            this.pbInfo_WFB = new System.Windows.Forms.PictureBox();
            this.cb_enableWFB = new System.Windows.Forms.CheckBox();
            this.remoteSettingsBox = new System.Windows.Forms.GroupBox();
            this.num_remotePort = new System.Windows.Forms.NumericUpDown();
            this.label_remotePort = new System.Windows.Forms.Label();
            this.chbox_enableRemote = new System.Windows.Forms.CheckBox();
            this.vpnBox = new System.Windows.Forms.GroupBox();
            this.cb_enableVPNChecks = new System.Windows.Forms.CheckBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.ipQualityScore_APIKEY = new System.Windows.Forms.TextBox();
            this.label_IPQS = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button3 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            this.gb_WindowsFirewall.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbInfo_WFB)).BeginInit();
            this.remoteSettingsBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_remotePort)).BeginInit();
            this.vpnBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(12, 227);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(763, 34);
            this.btn_save.TabIndex = 0;
            this.btn_save.Text = "Save and Close";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.event_saveClose);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_general);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(767, 209);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.gb_WindowsFirewall);
            this.tabPage_general.Controls.Add(this.remoteSettingsBox);
            this.tabPage_general.Controls.Add(this.vpnBox);
            this.tabPage_general.Location = new System.Drawing.Point(4, 22);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_general.Size = new System.Drawing.Size(759, 183);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "General";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // gb_WindowsFirewall
            // 
            this.gb_WindowsFirewall.Controls.Add(this.pbInfo_WFB);
            this.gb_WindowsFirewall.Controls.Add(this.cb_enableWFB);
            this.gb_WindowsFirewall.Location = new System.Drawing.Point(522, 103);
            this.gb_WindowsFirewall.Name = "gb_WindowsFirewall";
            this.gb_WindowsFirewall.Size = new System.Drawing.Size(231, 50);
            this.gb_WindowsFirewall.TabIndex = 14;
            this.gb_WindowsFirewall.TabStop = false;
            this.gb_WindowsFirewall.Text = "Windows Firewall Settings";
            // 
            // pbInfo_WFB
            // 
            this.pbInfo_WFB.Image = global::HawkSync_SM.Properties.Resources.info;
            this.pbInfo_WFB.Location = new System.Drawing.Point(15, 21);
            this.pbInfo_WFB.Name = "pbInfo_WFB";
            this.pbInfo_WFB.Size = new System.Drawing.Size(16, 16);
            this.pbInfo_WFB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbInfo_WFB.TabIndex = 15;
            this.pbInfo_WFB.TabStop = false;
            // 
            // cb_enableWFB
            // 
            this.cb_enableWFB.AutoSize = true;
            this.cb_enableWFB.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_enableWFB.Location = new System.Drawing.Point(77, 21);
            this.cb_enableWFB.Name = "cb_enableWFB";
            this.cb_enableWFB.Size = new System.Drawing.Size(148, 17);
            this.cb_enableWFB.TabIndex = 0;
            this.cb_enableWFB.Text = "Allow Banning via Firewall";
            this.cb_enableWFB.UseVisualStyleBackColor = true;
            // 
            // remoteSettingsBox
            // 
            this.remoteSettingsBox.Controls.Add(this.num_remotePort);
            this.remoteSettingsBox.Controls.Add(this.label_remotePort);
            this.remoteSettingsBox.Controls.Add(this.chbox_enableRemote);
            this.remoteSettingsBox.Location = new System.Drawing.Point(522, 6);
            this.remoteSettingsBox.Name = "remoteSettingsBox";
            this.remoteSettingsBox.Size = new System.Drawing.Size(231, 91);
            this.remoteSettingsBox.TabIndex = 13;
            this.remoteSettingsBox.TabStop = false;
            this.remoteSettingsBox.Text = "Remote Management Settings";
            // 
            // num_remotePort
            // 
            this.num_remotePort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.num_remotePort.Location = new System.Drawing.Point(162, 47);
            this.num_remotePort.Maximum = new decimal(new int[] {
            65565,
            0,
            0,
            0});
            this.num_remotePort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.num_remotePort.Name = "num_remotePort";
            this.num_remotePort.Size = new System.Drawing.Size(63, 20);
            this.num_remotePort.TabIndex = 18;
            this.num_remotePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.num_remotePort.Value = new decimal(new int[] {
            4173,
            0,
            0,
            0});
            // 
            // label_remotePort
            // 
            this.label_remotePort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_remotePort.AutoSize = true;
            this.label_remotePort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_remotePort.Location = new System.Drawing.Point(54, 49);
            this.label_remotePort.Name = "label_remotePort";
            this.label_remotePort.Size = new System.Drawing.Size(102, 13);
            this.label_remotePort.TabIndex = 17;
            this.label_remotePort.Text = "Remote Control Port";
            // 
            // chbox_enableRemote
            // 
            this.chbox_enableRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chbox_enableRemote.AutoSize = true;
            this.chbox_enableRemote.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chbox_enableRemote.Location = new System.Drawing.Point(88, 24);
            this.chbox_enableRemote.Name = "chbox_enableRemote";
            this.chbox_enableRemote.Size = new System.Drawing.Size(137, 17);
            this.chbox_enableRemote.TabIndex = 16;
            this.chbox_enableRemote.Text = "Enable Remote Access";
            this.chbox_enableRemote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // vpnBox
            // 
            this.vpnBox.Controls.Add(this.cb_enableVPNChecks);
            this.vpnBox.Controls.Add(this.pictureBox2);
            this.vpnBox.Controls.Add(this.ipQualityScore_APIKEY);
            this.vpnBox.Controls.Add(this.label_IPQS);
            this.vpnBox.Location = new System.Drawing.Point(8, 6);
            this.vpnBox.Name = "vpnBox";
            this.vpnBox.Size = new System.Drawing.Size(238, 147);
            this.vpnBox.TabIndex = 12;
            this.vpnBox.TabStop = false;
            this.vpnBox.Text = "VPN Monitoring";
            // 
            // cb_enableVPNChecks
            // 
            this.cb_enableVPNChecks.AutoSize = true;
            this.cb_enableVPNChecks.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_enableVPNChecks.Location = new System.Drawing.Point(104, 24);
            this.cb_enableVPNChecks.Name = "cb_enableVPNChecks";
            this.cb_enableVPNChecks.Size = new System.Drawing.Size(128, 17);
            this.cb_enableVPNChecks.TabIndex = 15;
            this.cb_enableVPNChecks.Text = "Allow VPN Monitoring";
            this.cb_enableVPNChecks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_enableVPNChecks.UseVisualStyleBackColor = true;
            this.cb_enableVPNChecks.CheckedChanged += new System.EventHandler(this.event_vpnCheckingChanged);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::HawkSync_SM.Properties.Resources.ipqs;
            this.pictureBox2.Location = new System.Drawing.Point(6, 55);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(16, 16);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 14;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.link_Browse2IPQS);
            // 
            // ipQualityScore_APIKEY
            // 
            this.ipQualityScore_APIKEY.Enabled = false;
            this.ipQualityScore_APIKEY.Location = new System.Drawing.Point(6, 71);
            this.ipQualityScore_APIKEY.Name = "ipQualityScore_APIKEY";
            this.ipQualityScore_APIKEY.Size = new System.Drawing.Size(226, 20);
            this.ipQualityScore_APIKEY.TabIndex = 5;
            // 
            // label_IPQS
            // 
            this.label_IPQS.AutoSize = true;
            this.label_IPQS.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label_IPQS.Location = new System.Drawing.Point(102, 55);
            this.label_IPQS.Name = "label_IPQS";
            this.label_IPQS.Size = new System.Drawing.Size(130, 13);
            this.label_IPQS.TabIndex = 4;
            this.label_IPQS.Text = "IP Quality Score (API Key)";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(759, 183);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Mods";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.pictureBox1);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.textBox5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.textBox4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Location = new System.Drawing.Point(8, 181);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(761, 100);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Add New Exp/Mod";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Black Hawk Down",
            "Joint Operations"});
            this.comboBox1.Location = new System.Drawing.Point(75, 42);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(132, 21);
            this.comboBox1.TabIndex = 13;
            // 
            // button3
            // 
            this.button3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button3.Location = new System.Drawing.Point(448, 44);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "Browse";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(310, 72);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(26, 18);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(242, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Mod Icon:";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(310, 47);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(132, 20);
            this.textBox5.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(242, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "PFF:";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(310, 20);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(132, 20);
            this.textBox4.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(7, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Args";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(75, 66);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(132, 20);
            this.textBox1.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(7, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Game:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(7, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mod Name:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(75, 19);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(132, 20);
            this.textBox2.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(763, 169);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Saved Mods";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(7, 20);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.ShowCellErrors = false;
            this.dataGridView1.ShowCellToolTips = false;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.ShowRowErrors = false;
            this.dataGridView1.Size = new System.Drawing.Size(750, 143);
            this.dataGridView1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(759, 183);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "About";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // SM_Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 271);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btn_save);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SM_Options";
            this.Text = "Babstats Multi-Tracker - Options";
            this.Load += new System.EventHandler(this.Options_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.gb_WindowsFirewall.ResumeLayout(false);
            this.gb_WindowsFirewall.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbInfo_WFB)).EndInit();
            this.remoteSettingsBox.ResumeLayout(false);
            this.remoteSettingsBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_remotePort)).EndInit();
            this.vpnBox.ResumeLayout(false);
            this.vpnBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.GroupBox vpnBox;
        private System.Windows.Forms.TextBox ipQualityScore_APIKEY;
        private System.Windows.Forms.Label label_IPQS;
        private System.Windows.Forms.GroupBox remoteSettingsBox;
        private System.Windows.Forms.NumericUpDown num_remotePort;
        private System.Windows.Forms.Label label_remotePort;
        private System.Windows.Forms.CheckBox chbox_enableRemote;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.GroupBox gb_WindowsFirewall;
        private System.Windows.Forms.CheckBox cb_enableWFB;
        private System.Windows.Forms.CheckBox cb_enableVPNChecks;
        private System.Windows.Forms.PictureBox pbInfo_WFB;
    }
}