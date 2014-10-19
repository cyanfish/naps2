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
            this.progressBarSub = new System.Windows.Forms.ProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelSub = new System.Windows.Forms.Label();
            this.labelTop = new System.Windows.Forms.Label();
            this.progressBarTop = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBarSub
            // 
            this.progressBarSub.Location = new System.Drawing.Point(12, 77);
            this.progressBarSub.Name = "progressBarSub";
            this.progressBarSub.Size = new System.Drawing.Size(417, 38);
            this.progressBarSub.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarSub.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(354, 121);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // labelSub
            // 
            this.labelSub.AutoSize = true;
            this.labelSub.Location = new System.Drawing.Point(12, 118);
            this.labelSub.Name = "labelSub";
            this.labelSub.Size = new System.Drawing.Size(65, 13);
            this.labelSub.TabIndex = 2;
            this.labelSub.Text = "{0} / {1} MB";
            // 
            // labelTop
            // 
            this.labelTop.AutoSize = true;
            this.labelTop.Location = new System.Drawing.Point(12, 53);
            this.labelTop.Name = "labelTop";
            this.labelTop.Size = new System.Drawing.Size(67, 13);
            this.labelTop.TabIndex = 4;
            this.labelTop.Text = "{0} / {1} files";
            // 
            // progressBarTop
            // 
            this.progressBarTop.Location = new System.Drawing.Point(14, 12);
            this.progressBarTop.Name = "progressBarTop";
            this.progressBarTop.Size = new System.Drawing.Size(417, 38);
            this.progressBarTop.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarTop.TabIndex = 3;
            // 
            // FDownloadProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 156);
            this.Controls.Add(this.labelTop);
            this.Controls.Add(this.progressBarTop);
            this.Controls.Add(this.labelSub);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.progressBarSub);
            this.MaximumSize = new System.Drawing.Size(1000, 195);
            this.MinimumSize = new System.Drawing.Size(350, 195);
            this.Name = "FDownloadProgress";
            this.Text = "Download Progress";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarSub;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labelSub;
        private System.Windows.Forms.Label labelTop;
        private System.Windows.Forms.ProgressBar progressBarTop;
    }
}