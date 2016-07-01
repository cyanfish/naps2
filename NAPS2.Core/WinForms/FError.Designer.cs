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
            this.btnOK = new System.Windows.Forms.Button();
            this.linkDetails = new System.Windows.Forms.LinkLabel();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblErrorText
            // 
            resources.ApplyResources(this.lblErrorText, "lblErrorText");
            this.lblErrorText.Name = "lblErrorText";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // linkDetails
            // 
            resources.ApplyResources(this.linkDetails, "linkDetails");
            this.linkDetails.Name = "linkDetails";
            this.linkDetails.TabStop = true;
            this.linkDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDetails_LinkClicked);
            // 
            // txtDetails
            // 
            resources.ApplyResources(this.txtDetails, "txtDetails");
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            // 
            // FError
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.linkDetails);
            this.Controls.Add(this.btnOK);
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
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.LinkLabel linkDetails;
        private System.Windows.Forms.TextBox txtDetails;
    }
}