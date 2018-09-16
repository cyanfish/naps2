using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAbout));
            this.labelProductName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.btnDonate = new System.Windows.Forms.PictureBox();
            this.cbCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.lblUpdateStatus = new System.Windows.Forms.Label();
            this.linkInstall = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDonate)).BeginInit();
            this.SuspendLayout();
            // 
            // labelProductName
            // 
            resources.ApplyResources(this.labelProductName, "labelProductName");
            this.labelProductName.Name = "labelProductName";
            // 
            // labelVersion
            // 
            resources.ApplyResources(this.labelVersion, "labelVersion");
            this.labelVersion.Name = "labelVersion";
            // 
            // labelCopyright
            // 
            resources.ApplyResources(this.labelCopyright, "labelCopyright");
            this.labelCopyright.Name = "labelCopyright";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Name = "okButton";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // linkLabel2
            // 
            resources.ApplyResources(this.linkLabel2, "linkLabel2");
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.TabStop = true;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // btnDonate
            // 
            this.btnDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDonate.Image = global::NAPS2.Icons.btn_donate_LG;
            resources.ApplyResources(this.btnDonate, "btnDonate");
            this.btnDonate.Name = "btnDonate";
            this.btnDonate.TabStop = false;
            this.btnDonate.Click += new System.EventHandler(this.btnDonate_Click);
            // 
            // cbCheckForUpdates
            // 
            resources.ApplyResources(this.cbCheckForUpdates, "cbCheckForUpdates");
            this.cbCheckForUpdates.Name = "cbCheckForUpdates";
            this.cbCheckForUpdates.UseVisualStyleBackColor = true;
            this.cbCheckForUpdates.CheckedChanged += new System.EventHandler(this.cbCheckForUpdates_CheckedChanged);
            // 
            // lblUpdateStatus
            // 
            resources.ApplyResources(this.lblUpdateStatus, "lblUpdateStatus");
            this.lblUpdateStatus.Name = "lblUpdateStatus";
            // 
            // linkInstall
            // 
            resources.ApplyResources(this.linkInstall, "linkInstall");
            this.linkInstall.Name = "linkInstall";
            this.linkInstall.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkInstall_LinkClicked);
            // 
            // FAbout
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkInstall);
            this.Controls.Add(this.lblUpdateStatus);
            this.Controls.Add(this.cbCheckForUpdates);
            this.Controls.Add(this.btnDonate);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.logoPictureBox);
            this.Controls.Add(this.labelProductName);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelCopyright);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FAbout";
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDonate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.PictureBox btnDonate;
        private System.Windows.Forms.CheckBox cbCheckForUpdates;
        private System.Windows.Forms.Label lblUpdateStatus;
        private System.Windows.Forms.LinkLabel linkInstall;
    }
}
