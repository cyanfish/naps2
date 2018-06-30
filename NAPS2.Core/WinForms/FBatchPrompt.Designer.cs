namespace NAPS2.WinForms
{
    partial class FBatchPrompt
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FBatchPrompt));
            this.BtnDone = new System.Windows.Forms.Button();
            this.BtnScan = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnDone
            // 
            resources.ApplyResources(this.BtnDone, "BtnDone");
            this.BtnDone.Name = "BtnDone";
            this.BtnDone.UseVisualStyleBackColor = true;
            this.BtnDone.Click += new System.EventHandler(this.BtnDone_Click);
            // 
            // BtnScan
            // 
            resources.ApplyResources(this.BtnScan, "BtnScan");
            this.BtnScan.Image = global::NAPS2.Icons.control_play_blue_small;
            this.BtnScan.Name = "BtnScan";
            this.BtnScan.UseVisualStyleBackColor = true;
            this.BtnScan.Click += new System.EventHandler(this.BtnScan_Click);
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // FBatchPrompt
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnDone);
            this.Controls.Add(this.BtnScan);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FBatchPrompt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnDone;
        private System.Windows.Forms.Button BtnScan;
        private System.Windows.Forms.Label lblStatus;

    }
}