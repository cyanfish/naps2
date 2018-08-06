namespace NAPS2.WinForms
{
    partial class FAuthorize
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAuthorize));
            this.lblWaiting = new System.Windows.Forms.Label();
            this.linkTryAgain = new System.Windows.Forms.LinkLabel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblWaiting
            // 
            this.lblWaiting.AutoSize = true;
            this.lblWaiting.Location = new System.Drawing.Point(12, 9);
            this.lblWaiting.Name = "lblWaiting";
            this.lblWaiting.Size = new System.Drawing.Size(130, 13);
            this.lblWaiting.TabIndex = 0;
            this.lblWaiting.Text = "Waiting for authorization...";
            // 
            // linkTryAgain
            // 
            this.linkTryAgain.AutoSize = true;
            this.linkTryAgain.Location = new System.Drawing.Point(12, 25);
            this.linkTryAgain.Name = "linkTryAgain";
            this.linkTryAgain.Size = new System.Drawing.Size(51, 13);
            this.linkTryAgain.TabIndex = 1;
            this.linkTryAgain.TabStop = true;
            this.linkTryAgain.Text = "Try again";
            this.linkTryAgain.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkTryAgain_LinkClicked);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(169, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FAuthorize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(256, 47);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.linkTryAgain);
            this.Controls.Add(this.lblWaiting);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FAuthorize";
            this.Text = "Authorize";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FAuthorize_FormClosed);
            this.Load += new System.EventHandler(this.FAuthorize_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWaiting;
        private System.Windows.Forms.LinkLabel linkTryAgain;
        private System.Windows.Forms.Button btnCancel;
    }
}