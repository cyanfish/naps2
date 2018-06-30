using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FEmailSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FEmailSettings));
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.cbRememberSettings = new System.Windows.Forms.CheckBox();
            this.BtnRestoreDefaults = new System.Windows.Forms.Button();
            this.TxtAttachmentName = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.LinkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // cbRememberSettings
            // 
            resources.ApplyResources(this.cbRememberSettings, "cbRememberSettings");
            this.cbRememberSettings.Name = "cbRememberSettings";
            this.cbRememberSettings.UseVisualStyleBackColor = true;
            // 
            // BtnRestoreDefaults
            // 
            resources.ApplyResources(this.BtnRestoreDefaults, "BtnRestoreDefaults");
            this.BtnRestoreDefaults.Name = "BtnRestoreDefaults";
            this.BtnRestoreDefaults.UseVisualStyleBackColor = true;
            this.BtnRestoreDefaults.Click += new System.EventHandler(this.BtnRestoreDefaults_Click);
            // 
            // TxtAttachmentName
            // 
            resources.ApplyResources(this.TxtAttachmentName, "TxtAttachmentName");
            this.TxtAttachmentName.Name = "TxtAttachmentName";
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // LinkPlaceholders
            // 
            resources.ApplyResources(this.LinkPlaceholders, "LinkPlaceholders");
            this.LinkPlaceholders.Name = "LinkPlaceholders";
            this.LinkPlaceholders.TabStop = true;
            this.LinkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkPlaceholders_LinkClicked);
            // 
            // FEmailSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LinkPlaceholders);
            this.Controls.Add(this.TxtAttachmentName);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.BtnRestoreDefaults);
            this.Controls.Add(this.cbRememberSettings);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEmailSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.CheckBox cbRememberSettings;
        private System.Windows.Forms.Button BtnRestoreDefaults;
        private System.Windows.Forms.TextBox TxtAttachmentName;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.LinkLabel LinkPlaceholders;
    }
}
