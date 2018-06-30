namespace NAPS2.WinForms
{
    partial class FError
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FError));
            this.lblErrorText = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.LinkDetails = new System.Windows.Forms.LinkLabel();
            this.TxtDetails = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblErrorText
            // 
            resources.ApplyResources(this.lblErrorText, "lblErrorText");
            this.lblErrorText.Name = "lblErrorText";
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // LinkDetails
            // 
            resources.ApplyResources(this.LinkDetails, "LinkDetails");
            this.LinkDetails.Name = "LinkDetails";
            this.LinkDetails.TabStop = true;
            this.LinkDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkDetails_LinkClicked);
            // 
            // TxtDetails
            // 
            resources.ApplyResources(this.TxtDetails, "TxtDetails");
            this.TxtDetails.Name = "TxtDetails";
            this.TxtDetails.ReadOnly = true;
            // 
            // FError
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TxtDetails);
            this.Controls.Add(this.LinkDetails);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.lblErrorText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FError";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblErrorText;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.LinkLabel LinkDetails;
        private System.Windows.Forms.TextBox TxtDetails;
    }
}