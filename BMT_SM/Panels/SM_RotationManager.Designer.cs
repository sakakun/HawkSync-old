
namespace HawkSync_SM
{
    partial class SM_RotationManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SM_RotationManager));
            this.btn_saveMapRotation = new System.Windows.Forms.Button();
            this.btn_deleteRotation = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_moveMapUp = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_moveMapDown = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.text_RotationDesc = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.list_selectedRotation = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.list_savedMapRotation = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.list_availableMaps = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.drop_gameTypes = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_saveMapRotation
            // 
            resources.ApplyResources(this.btn_saveMapRotation, "btn_saveMapRotation");
            this.btn_saveMapRotation.Name = "btn_saveMapRotation";
            this.btn_saveMapRotation.UseVisualStyleBackColor = true;
            this.btn_saveMapRotation.Click += new System.EventHandler(this.save_maprotation);
            // 
            // btn_deleteRotation
            // 
            resources.ApplyResources(this.btn_deleteRotation, "btn_deleteRotation");
            this.btn_deleteRotation.Name = "btn_deleteRotation";
            this.btn_deleteRotation.UseVisualStyleBackColor = true;
            this.btn_deleteRotation.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_saveMapRotation);
            this.groupBox1.Controls.Add(this.btn_moveMapUp);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btn_moveMapDown);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.text_RotationDesc);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.list_selectedRotation);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btn_moveMapUp
            // 
            resources.ApplyResources(this.btn_moveMapUp, "btn_moveMapUp");
            this.btn_moveMapUp.Name = "btn_moveMapUp";
            this.btn_moveMapUp.UseVisualStyleBackColor = true;
            this.btn_moveMapUp.Click += new System.EventHandler(this.button4_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btn_moveMapDown
            // 
            resources.ApplyResources(this.btn_moveMapDown, "btn_moveMapDown");
            this.btn_moveMapDown.Name = "btn_moveMapDown";
            this.btn_moveMapDown.UseVisualStyleBackColor = true;
            this.btn_moveMapDown.Click += new System.EventHandler(this.button5_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // text_RotationDesc
            // 
            resources.ApplyResources(this.text_RotationDesc, "text_RotationDesc");
            this.text_RotationDesc.Name = "text_RotationDesc";
            this.text_RotationDesc.Enter += new System.EventHandler(this.textBox1_Enter);
            this.text_RotationDesc.Leave += new System.EventHandler(this.textBox1_Leave);
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Name = "label4";
            // 
            // list_selectedRotation
            // 
            this.list_selectedRotation.FormattingEnabled = true;
            resources.ApplyResources(this.list_selectedRotation, "list_selectedRotation");
            this.list_selectedRotation.Name = "list_selectedRotation";
            this.list_selectedRotation.DoubleClick += new System.EventHandler(this.listBox2_DoubleClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.list_savedMapRotation);
            this.groupBox2.Controls.Add(this.btn_deleteRotation);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Name = "label3";
            // 
            // list_savedMapRotation
            // 
            this.list_savedMapRotation.FormattingEnabled = true;
            resources.ApplyResources(this.list_savedMapRotation, "list_savedMapRotation");
            this.list_savedMapRotation.Name = "list_savedMapRotation";
            this.list_savedMapRotation.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.list_availableMaps);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.drop_gameTypes);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Name = "label8";
            // 
            // list_availableMaps
            // 
            this.list_availableMaps.FormattingEnabled = true;
            resources.ApplyResources(this.list_availableMaps, "list_availableMaps");
            this.list_availableMaps.Name = "list_availableMaps";
            this.list_availableMaps.DoubleClick += new System.EventHandler(this.availMaps_DoubleClick);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // drop_gameTypes
            // 
            this.drop_gameTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drop_gameTypes.FormattingEnabled = true;
            resources.ApplyResources(this.drop_gameTypes, "drop_gameTypes");
            this.drop_gameTypes.Name = "drop_gameTypes";
            this.drop_gameTypes.SelectedIndexChanged += new System.EventHandler(this.dropGameTypeChanged);
            // 
            // RotationManager
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "RotationManager";
            this.Load += new System.EventHandler(this.RotationManager_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_saveMapRotation;
        private System.Windows.Forms.Button btn_deleteRotation;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btn_moveMapUp;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_moveMapDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox list_selectedRotation;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox list_savedMapRotation;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListBox list_availableMaps;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox drop_gameTypes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox text_RotationDesc;
        private System.Windows.Forms.Label label3;
    }
}