namespace NAPS2.WinForms
{
    partial class FDownloadProgress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FDownloadProgress));
            this.progressBarSub = new System.Windows.Forms.ProgressBar();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.LabelSub = new System.Windows.Forms.Label();
            this.LabelTop = new System.Windows.Forms.Label();
            this.progressBarTop = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBarSub
            // 
            resources.ApplyResources(this.progressBarSub, "progressBarSub");
            this.progressBarSub.Name = "progressBarSub";
            this.progressBarSub.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // LabelSub
            // 
            resources.ApplyResources(this.LabelSub, "LabelSub");
            this.LabelSub.Name = "LabelSub";
            // 
            // LabelTop
            // 
            resources.ApplyResources(this.LabelTop, "LabelTop");
            this.LabelTop.Name = "LabelTop";
            // 
            // progressBarTop
            // 
            resources.ApplyResources(this.progressBarTop, "progressBarTop");
            this.progressBarTop.Name = "progressBarTop";
            this.progressBarTop.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // FDownloadProgress
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LabelTop);
            this.Controls.Add(this.progressBarTop);
            this.Controls.Add(this.LabelSub);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.progressBarSub);
            this.MaximizeBox = false;
            this.Name = "FDownloadProgress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FDownloadProgress_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarSub;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Label LabelSub;
        private System.Windows.Forms.Label LabelTop;
        private System.Windows.Forms.ProgressBar progressBarTop;
    }
}