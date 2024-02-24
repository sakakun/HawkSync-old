namespace HawkSync_RC
{
    partial class RC_PopupLoadRotation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RC_PopupLoadRotation));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_loadMaps = new System.Windows.Forms.Button();
            this.btn_close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(13, 27);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(180, 186);
            this.listBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Load Saved Rotation";
            // 
            // btn_loadMaps
            // 
            this.btn_loadMaps.Location = new System.Drawing.Point(13, 219);
            this.btn_loadMaps.Name = "btn_loadMaps";
            this.btn_loadMaps.Size = new System.Drawing.Size(68, 23);
            this.btn_loadMaps.TabIndex = 2;
            this.btn_loadMaps.Text = "Load";
            this.btn_loadMaps.UseVisualStyleBackColor = true;
            this.btn_loadMaps.Click += new System.EventHandler(this.event_loadMaps);
            // 
            // btn_close
            // 
            this.btn_close.Location = new System.Drawing.Point(118, 219);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(75, 23);
            this.btn_close.TabIndex = 3;
            this.btn_close.Text = "Cancel";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.event_closeWindow);
            // 
            // RC_PopupLoadRotation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(208, 255);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.btn_loadMaps);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RC_PopupLoadRotation";
            this.Text = "Load Rotation";
            this.Load += new System.EventHandler(this.Popup_LoadRotation_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_loadMaps;
        private System.Windows.Forms.Button btn_close;
    }
}