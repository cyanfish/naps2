namespace NAPS2.WinForms
{
    partial class FProgress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FProgress));
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelTop = new System.Windows.Forms.Label();
            this.progressBarTop = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // labelTop
            // 
            resources.ApplyResources(this.labelTop, "labelTop");
            this.labelTop.Name = "labelTop";
            // 
            // progressBarTop
            // 
            resources.ApplyResources(this.progressBarTop, "progressBarTop");
            this.progressBarTop.MarqueeAnimationSpeed = 50;
            this.progressBarTop.Name = "progressBarTop";
            this.progressBarTop.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // FProgress
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelTop);
            this.Controls.Add(this.progressBarTop);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FProgress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FDownloadProgress_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labelTop;
        private System.Windows.Forms.ProgressBar progressBarTop;
    }
}