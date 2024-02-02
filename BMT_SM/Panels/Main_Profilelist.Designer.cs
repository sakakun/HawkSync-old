
namespace HawkSync_SM
{
	partial class Main_Profilelist
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main_Profilelist));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.button_options = new System.Windows.Forms.Button();
            this.btn_userManager = new System.Windows.Forms.Button();
            this.btn_serverManager = new System.Windows.Forms.Button();
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_create = new System.Windows.Forms.Button();
            this.btn_edit = new System.Windows.Forms.Button();
            this.btn_delete = new System.Windows.Forms.Button();
            this.btn_quit = new System.Windows.Forms.Button();
            this.label_notActive = new System.Windows.Forms.Label();
            this.label_loadingMap = new System.Windows.Forms.Label();
            this.label_hostingMap = new System.Windows.Forms.Label();
            this.label_scoringMap = new System.Windows.Forms.Label();
            this.list_serverProfiles = new System.Windows.Forms.DataGridView();
            this.btn_rotationManager = new System.Windows.Forms.Button();
            this.picture_scoringMap = new System.Windows.Forms.PictureBox();
            this.picture_hostingMap = new System.Windows.Forms.PictureBox();
            this.picture_loadingMap = new System.Windows.Forms.PictureBox();
            this.picture_notActive = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.list_serverProfiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_scoringMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_hostingMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_loadingMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_notActive)).BeginInit();
            this.SuspendLayout();
            // 
            // button_options
            // 
            resources.ApplyResources(this.button_options, "button_options");
            this.button_options.Name = "button_options";
            this.button_options.UseVisualStyleBackColor = true;
            this.button_options.Click += new System.EventHandler(this.btnOptions_click);
            // 
            // btn_userManager
            // 
            resources.ApplyResources(this.btn_userManager, "btn_userManager");
            this.btn_userManager.Name = "btn_userManager";
            this.btn_userManager.UseVisualStyleBackColor = true;
            this.btn_userManager.Click += new System.EventHandler(this.btnUM_click);
            // 
            // btn_serverManager
            // 
            resources.ApplyResources(this.btn_serverManager, "btn_serverManager");
            this.btn_serverManager.Name = "btn_serverManager";
            this.btn_serverManager.UseVisualStyleBackColor = true;
            this.btn_serverManager.Click += new System.EventHandler(this.btnSM_click);
            // 
            // btn_start
            // 
            resources.ApplyResources(this.btn_start, "btn_start");
            this.btn_start.Name = "btn_start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btn_create
            // 
            resources.ApplyResources(this.btn_create, "btn_create");
            this.btn_create.Name = "btn_create";
            this.btn_create.UseVisualStyleBackColor = true;
            this.btn_create.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btn_edit
            // 
            resources.ApplyResources(this.btn_edit, "btn_edit");
            this.btn_edit.Name = "btn_edit";
            this.btn_edit.UseVisualStyleBackColor = true;
            this.btn_edit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btn_delete
            // 
            resources.ApplyResources(this.btn_delete, "btn_delete");
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btn_quit
            // 
            resources.ApplyResources(this.btn_quit, "btn_quit");
            this.btn_quit.Name = "btn_quit";
            this.btn_quit.UseVisualStyleBackColor = true;
            this.btn_quit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // label_notActive
            // 
            resources.ApplyResources(this.label_notActive, "label_notActive");
            this.label_notActive.Name = "label_notActive";
            // 
            // label_loadingMap
            // 
            resources.ApplyResources(this.label_loadingMap, "label_loadingMap");
            this.label_loadingMap.Name = "label_loadingMap";
            // 
            // label_hostingMap
            // 
            resources.ApplyResources(this.label_hostingMap, "label_hostingMap");
            this.label_hostingMap.Name = "label_hostingMap";
            // 
            // label_scoringMap
            // 
            resources.ApplyResources(this.label_scoringMap, "label_scoringMap");
            this.label_scoringMap.Name = "label_scoringMap";
            // 
            // list_serverProfiles
            // 
            this.list_serverProfiles.AllowUserToAddRows = false;
            this.list_serverProfiles.AllowUserToDeleteRows = false;
            this.list_serverProfiles.AllowUserToResizeColumns = false;
            this.list_serverProfiles.AllowUserToResizeRows = false;
            this.list_serverProfiles.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.list_serverProfiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.list_serverProfiles.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            resources.ApplyResources(this.list_serverProfiles, "list_serverProfiles");
            this.list_serverProfiles.MultiSelect = false;
            this.list_serverProfiles.Name = "list_serverProfiles";
            this.list_serverProfiles.ReadOnly = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.list_serverProfiles.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.list_serverProfiles.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.list_serverProfiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.list_serverProfiles.ShowCellErrors = false;
            this.list_serverProfiles.ShowCellToolTips = false;
            this.list_serverProfiles.ShowEditingIcon = false;
            this.list_serverProfiles.ShowRowErrors = false;
            this.list_serverProfiles.SelectionChanged += new System.EventHandler(this.serverProfiles_SelectionChanged);
            // 
            // btn_rotationManager
            // 
            resources.ApplyResources(this.btn_rotationManager, "btn_rotationManager");
            this.btn_rotationManager.Name = "btn_rotationManager";
            this.btn_rotationManager.UseVisualStyleBackColor = true;
            this.btn_rotationManager.Click += new System.EventHandler(this.btnRM_click);
            // 
            // picture_scoringMap
            // 
            this.picture_scoringMap.Image = global::HawkSync_SM.Properties.Resources.scoring;
            resources.ApplyResources(this.picture_scoringMap, "picture_scoringMap");
            this.picture_scoringMap.Name = "picture_scoringMap";
            this.picture_scoringMap.TabStop = false;
            // 
            // picture_hostingMap
            // 
            this.picture_hostingMap.Image = global::HawkSync_SM.Properties.Resources.hosting;
            resources.ApplyResources(this.picture_hostingMap, "picture_hostingMap");
            this.picture_hostingMap.Name = "picture_hostingMap";
            this.picture_hostingMap.TabStop = false;
            // 
            // picture_loadingMap
            // 
            this.picture_loadingMap.Image = global::HawkSync_SM.Properties.Resources.loading;
            resources.ApplyResources(this.picture_loadingMap, "picture_loadingMap");
            this.picture_loadingMap.Name = "picture_loadingMap";
            this.picture_loadingMap.TabStop = false;
            // 
            // picture_notActive
            // 
            this.picture_notActive.Image = global::HawkSync_SM.Properties.Resources.notactive;
            resources.ApplyResources(this.picture_notActive, "picture_notActive");
            this.picture_notActive.Name = "picture_notActive";
            this.picture_notActive.TabStop = false;
            // 
            // Main_Profilelist
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_rotationManager);
            this.Controls.Add(this.list_serverProfiles);
            this.Controls.Add(this.label_scoringMap);
            this.Controls.Add(this.picture_scoringMap);
            this.Controls.Add(this.label_hostingMap);
            this.Controls.Add(this.picture_hostingMap);
            this.Controls.Add(this.label_loadingMap);
            this.Controls.Add(this.picture_loadingMap);
            this.Controls.Add(this.label_notActive);
            this.Controls.Add(this.picture_notActive);
            this.Controls.Add(this.btn_quit);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_edit);
            this.Controls.Add(this.btn_create);
            this.Controls.Add(this.btn_start);
            this.Controls.Add(this.btn_serverManager);
            this.Controls.Add(this.btn_userManager);
            this.Controls.Add(this.button_options);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Main_Profilelist";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_Profilelist_Close);
            this.Load += new System.EventHandler(this.Main_Profilelist_Load);
            ((System.ComponentModel.ISupportInitialize)(this.list_serverProfiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_scoringMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_hostingMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_loadingMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture_notActive)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button button_options;
		private System.Windows.Forms.Button btn_userManager;
		private System.Windows.Forms.Button btn_serverManager;
        private System.Windows.Forms.Button btn_start;
        private System.Windows.Forms.Button btn_create;
        private System.Windows.Forms.Button btn_edit;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Button btn_quit;
        private System.Windows.Forms.PictureBox picture_notActive;
        private System.Windows.Forms.Label label_notActive;
        private System.Windows.Forms.PictureBox picture_loadingMap;
        private System.Windows.Forms.Label label_loadingMap;
        private System.Windows.Forms.PictureBox picture_hostingMap;
        private System.Windows.Forms.Label label_hostingMap;
        private System.Windows.Forms.PictureBox picture_scoringMap;
        private System.Windows.Forms.Label label_scoringMap;
        private System.Windows.Forms.DataGridView list_serverProfiles;
        private System.Windows.Forms.Button btn_rotationManager;
    }
}

