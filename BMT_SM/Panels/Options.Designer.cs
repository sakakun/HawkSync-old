
namespace HawkSync_SM
{
    partial class Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));
            this.btn_save = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.remoteSettingsBox = new System.Windows.Forms.GroupBox();
            this.num_remotePort = new System.Windows.Forms.NumericUpDown();
            this.label_remotePort = new System.Windows.Forms.Label();
            this.chbox_enableRemote = new System.Windows.Forms.CheckBox();
            this.vpnBox = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.ipQualityScore_APIKEY = new System.Windows.Forms.TextBox();
            this.label_apikey = new System.Windows.Forms.Label();
            this.label_apiNo = new System.Windows.Forms.Label();
            this.radio_apibtn2 = new System.Windows.Forms.RadioButton();
            this.label_apiYes = new System.Windows.Forms.Label();
            this.radio_apibtn1 = new System.Windows.Forms.RadioButton();
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
            this.remoteSettingsBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_remotePort)).BeginInit();
            this.vpnBox.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(4, 319);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(776, 34);
            this.btn_save.TabIndex = 0;
            this.btn_save.Text = "Save and Close";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.Button1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_general);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(788, 317);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.remoteSettingsBox);
            this.tabPage_general.Controls.Add(this.vpnBox);
            this.tabPage_general.Location = new System.Drawing.Point(4, 22);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_general.Size = new System.Drawing.Size(780, 291);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "General";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // remoteSettingsBox
            // 
            this.remoteSettingsBox.Controls.Add(this.num_remotePort);
            this.remoteSettingsBox.Controls.Add(this.label_remotePort);
            this.remoteSettingsBox.Controls.Add(this.chbox_enableRemote);
            this.remoteSettingsBox.Location = new System.Drawing.Point(519, 6);
            this.remoteSettingsBox.Name = "remoteSettingsBox";
            this.remoteSettingsBox.Size = new System.Drawing.Size(255, 97);
            this.remoteSettingsBox.TabIndex = 13;
            this.remoteSettingsBox.TabStop = false;
            this.remoteSettingsBox.Text = "Remote Management Settings";
            // 
            // num_remotePort
            // 
            this.num_remotePort.Location = new System.Drawing.Point(26, 54);
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
            this.label_remotePort.AutoSize = true;
            this.label_remotePort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_remotePort.Location = new System.Drawing.Point(95, 57);
            this.label_remotePort.Name = "label_remotePort";
            this.label_remotePort.Size = new System.Drawing.Size(102, 13);
            this.label_remotePort.TabIndex = 17;
            this.label_remotePort.Text = "Remote Control Port";
            // 
            // chbox_enableRemote
            // 
            this.chbox_enableRemote.Location = new System.Drawing.Point(75, 20);
            this.chbox_enableRemote.Name = "chbox_enableRemote";
            this.chbox_enableRemote.Size = new System.Drawing.Size(147, 34);
            this.chbox_enableRemote.TabIndex = 16;
            this.chbox_enableRemote.Text = "Enable Remote Access";
            this.chbox_enableRemote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // vpnBox
            // 
            this.vpnBox.Controls.Add(this.linkLabel1);
            this.vpnBox.Controls.Add(this.ipQualityScore_APIKEY);
            this.vpnBox.Controls.Add(this.label_apikey);
            this.vpnBox.Controls.Add(this.label_apiNo);
            this.vpnBox.Controls.Add(this.radio_apibtn2);
            this.vpnBox.Controls.Add(this.label_apiYes);
            this.vpnBox.Controls.Add(this.radio_apibtn1);
            this.vpnBox.Location = new System.Drawing.Point(275, 6);
            this.vpnBox.Name = "vpnBox";
            this.vpnBox.Size = new System.Drawing.Size(238, 97);
            this.vpnBox.TabIndex = 12;
            this.vpnBox.TabStop = false;
            this.vpnBox.Text = "Check for VPN";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(137, 77);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(100, 13);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "IPQualityScore.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // ipQualityScore_APIKEY
            // 
            this.ipQualityScore_APIKEY.Location = new System.Drawing.Point(58, 43);
            this.ipQualityScore_APIKEY.Name = "ipQualityScore_APIKEY";
            this.ipQualityScore_APIKEY.Size = new System.Drawing.Size(174, 20);
            this.ipQualityScore_APIKEY.TabIndex = 5;
            // 
            // label_apikey
            // 
            this.label_apikey.AutoSize = true;
            this.label_apikey.Location = new System.Drawing.Point(7, 46);
            this.label_apikey.Name = "label_apikey";
            this.label_apikey.Size = new System.Drawing.Size(45, 13);
            this.label_apikey.TabIndex = 4;
            this.label_apikey.Text = "API Key";
            // 
            // label_apiNo
            // 
            this.label_apiNo.AutoSize = true;
            this.label_apiNo.Location = new System.Drawing.Point(151, 21);
            this.label_apiNo.Name = "label_apiNo";
            this.label_apiNo.Size = new System.Drawing.Size(21, 13);
            this.label_apiNo.TabIndex = 3;
            this.label_apiNo.Text = "No";
            // 
            // radio_apibtn2
            // 
            this.radio_apibtn2.AutoSize = true;
            this.radio_apibtn2.Location = new System.Drawing.Point(135, 21);
            this.radio_apibtn2.Name = "radio_apibtn2";
            this.radio_apibtn2.Size = new System.Drawing.Size(14, 13);
            this.radio_apibtn2.TabIndex = 2;
            this.radio_apibtn2.TabStop = true;
            this.radio_apibtn2.UseVisualStyleBackColor = true;
            this.radio_apibtn2.Click += new System.EventHandler(this.radioButton2_Click);
            // 
            // label_apiYes
            // 
            this.label_apiYes.AutoSize = true;
            this.label_apiYes.Location = new System.Drawing.Point(87, 21);
            this.label_apiYes.Name = "label_apiYes";
            this.label_apiYes.Size = new System.Drawing.Size(25, 13);
            this.label_apiYes.TabIndex = 1;
            this.label_apiYes.Text = "Yes";
            // 
            // radio_apibtn1
            // 
            this.radio_apibtn1.AutoSize = true;
            this.radio_apibtn1.Location = new System.Drawing.Point(71, 21);
            this.radio_apibtn1.Name = "radio_apibtn1";
            this.radio_apibtn1.Size = new System.Drawing.Size(14, 13);
            this.radio_apibtn1.TabIndex = 0;
            this.radio_apibtn1.TabStop = true;
            this.radio_apibtn1.UseVisualStyleBackColor = true;
            this.radio_apibtn1.Click += new System.EventHandler(this.radioButton1_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(780, 291);
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
            this.tabPage2.Size = new System.Drawing.Size(780, 291);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "About";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 357);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btn_save);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Options";
            this.Text = "Babstats Multi-Tracker - Options";
            this.Load += new System.EventHandler(this.Options_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.remoteSettingsBox.ResumeLayout(false);
            this.remoteSettingsBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_remotePort)).EndInit();
            this.vpnBox.ResumeLayout(false);
            this.vpnBox.PerformLayout();
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
        private System.Windows.Forms.Label label_apiNo;
        private System.Windows.Forms.RadioButton radio_apibtn2;
        private System.Windows.Forms.Label label_apiYes;
        private System.Windows.Forms.RadioButton radio_apibtn1;
        private System.Windows.Forms.TextBox ipQualityScore_APIKEY;
        private System.Windows.Forms.Label label_apikey;
        private System.Windows.Forms.LinkLabel linkLabel1;
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
    }
}