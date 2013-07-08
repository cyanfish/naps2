namespace NAPS2
{
    partial class FChooseProfile
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FChooseProfile));
            this.lvProfiles = new System.Windows.Forms.ListView();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnProfiles = new System.Windows.Forms.Button();
            this.btnDone = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.ILProfileIcons(this.components);
            this.SuspendLayout();
            // 
            // lvProfiles
            // 
            this.lvProfiles.HideSelection = false;
            resources.ApplyResources(this.lvProfiles, "lvProfiles");
            this.lvProfiles.MultiSelect = false;
            this.lvProfiles.Name = "lvProfiles";
            this.lvProfiles.UseCompatibleStateImageBehavior = false;
            this.lvProfiles.ItemActivate += new System.EventHandler(this.lvProfiles_ItemActivate);
            // 
            // btnScan
            // 
            this.btnScan.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.btnScan, "btnScan");
            this.btnScan.Image = global::NAPS2.Icons.scanner_72;
            this.btnScan.Name = "btnScan";
            this.btnScan.UseVisualStyleBackColor = false;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnProfiles
            // 
            resources.ApplyResources(this.btnProfiles, "btnProfiles");
            this.btnProfiles.Image = global::NAPS2.Icons.blueprints_small;
            this.btnProfiles.Name = "btnProfiles";
            this.btnProfiles.UseVisualStyleBackColor = true;
            this.btnProfiles.Click += new System.EventHandler(this.btnProfiles_Click);
            // 
            // btnDone
            // 
            resources.ApplyResources(this.btnDone, "btnDone");
            this.btnDone.Name = "btnDone";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // FChooseProfile
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.btnProfiles);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.lvProfiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FChooseProfile";
            this.Load += new System.EventHandler(this.FChooseProfile_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvProfiles;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnProfiles;
        private System.Windows.Forms.Button btnDone;
    }
}
