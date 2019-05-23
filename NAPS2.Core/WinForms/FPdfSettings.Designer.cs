using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FPdfSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FPdfSettings));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.groupMetadata = new System.Windows.Forms.GroupBox();
            this.txtKeywords = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupProtection = new System.Windows.Forms.GroupBox();
            this.clbPerms = new System.Windows.Forms.CheckedListBox();
            this.cbShowUserPassword = new System.Windows.Forms.CheckBox();
            this.cbShowOwnerPassword = new System.Windows.Forms.CheckBox();
            this.txtUserPassword = new System.Windows.Forms.TextBox();
            this.lblUserPassword = new System.Windows.Forms.Label();
            this.txtOwnerPassword = new System.Windows.Forms.TextBox();
            this.lblOwnerPassword = new System.Windows.Forms.Label();
            this.cbEncryptPdf = new System.Windows.Forms.CheckBox();
            this.cbRememberSettings = new System.Windows.Forms.CheckBox();
            this.btnRestoreDefaults = new System.Windows.Forms.Button();
            this.linkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.txtDefaultFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.cbSkipSavePrompt = new System.Windows.Forms.CheckBox();
            this.groupCompat = new System.Windows.Forms.GroupBox();
            this.cmbCompat = new System.Windows.Forms.ComboBox();
            this.cbSinglePagePdf = new System.Windows.Forms.CheckBox();
            this.groupMetadata.SuspendLayout();
            this.groupProtection.SuspendLayout();
            this.groupCompat.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupMetadata
            // 
            this.groupMetadata.Controls.Add(this.txtKeywords);
            this.groupMetadata.Controls.Add(this.label6);
            this.groupMetadata.Controls.Add(this.txtSubject);
            this.groupMetadata.Controls.Add(this.label5);
            this.groupMetadata.Controls.Add(this.txtAuthor);
            this.groupMetadata.Controls.Add(this.label3);
            this.groupMetadata.Controls.Add(this.txtTitle);
            this.groupMetadata.Controls.Add(this.label4);
            resources.ApplyResources(this.groupMetadata, "groupMetadata");
            this.groupMetadata.Name = "groupMetadata";
            this.groupMetadata.TabStop = false;
            // 
            // txtKeywords
            // 
            resources.ApplyResources(this.txtKeywords, "txtKeywords");
            this.txtKeywords.Name = "txtKeywords";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // txtSubject
            // 
            resources.ApplyResources(this.txtSubject, "txtSubject");
            this.txtSubject.Name = "txtSubject";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // txtAuthor
            // 
            resources.ApplyResources(this.txtAuthor, "txtAuthor");
            this.txtAuthor.Name = "txtAuthor";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // txtTitle
            // 
            resources.ApplyResources(this.txtTitle, "txtTitle");
            this.txtTitle.Name = "txtTitle";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // groupProtection
            // 
            this.groupProtection.Controls.Add(this.clbPerms);
            this.groupProtection.Controls.Add(this.cbShowUserPassword);
            this.groupProtection.Controls.Add(this.cbShowOwnerPassword);
            this.groupProtection.Controls.Add(this.txtUserPassword);
            this.groupProtection.Controls.Add(this.lblUserPassword);
            this.groupProtection.Controls.Add(this.txtOwnerPassword);
            this.groupProtection.Controls.Add(this.lblOwnerPassword);
            this.groupProtection.Controls.Add(this.cbEncryptPdf);
            resources.ApplyResources(this.groupProtection, "groupProtection");
            this.groupProtection.Name = "groupProtection";
            this.groupProtection.TabStop = false;
            // 
            // clbPerms
            // 
            this.clbPerms.BackColor = System.Drawing.SystemColors.Control;
            this.clbPerms.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbPerms.CheckOnClick = true;
            this.clbPerms.FormattingEnabled = true;
            this.clbPerms.Items.AddRange(new object[] {
            resources.GetString("clbPerms.Items"),
            resources.GetString("clbPerms.Items1"),
            resources.GetString("clbPerms.Items2"),
            resources.GetString("clbPerms.Items3"),
            resources.GetString("clbPerms.Items4"),
            resources.GetString("clbPerms.Items5"),
            resources.GetString("clbPerms.Items6"),
            resources.GetString("clbPerms.Items7")});
            resources.ApplyResources(this.clbPerms, "clbPerms");
            this.clbPerms.Name = "clbPerms";
            // 
            // cbShowUserPassword
            // 
            resources.ApplyResources(this.cbShowUserPassword, "cbShowUserPassword");
            this.cbShowUserPassword.Name = "cbShowUserPassword";
            this.cbShowUserPassword.UseVisualStyleBackColor = true;
            this.cbShowUserPassword.CheckedChanged += new System.EventHandler(this.cbShowUserPassword_CheckedChanged);
            // 
            // cbShowOwnerPassword
            // 
            resources.ApplyResources(this.cbShowOwnerPassword, "cbShowOwnerPassword");
            this.cbShowOwnerPassword.Name = "cbShowOwnerPassword";
            this.cbShowOwnerPassword.UseVisualStyleBackColor = true;
            this.cbShowOwnerPassword.CheckedChanged += new System.EventHandler(this.cbShowOwnerPassword_CheckedChanged);
            // 
            // txtUserPassword
            // 
            resources.ApplyResources(this.txtUserPassword, "txtUserPassword");
            this.txtUserPassword.Name = "txtUserPassword";
            this.txtUserPassword.UseSystemPasswordChar = true;
            // 
            // lblUserPassword
            // 
            resources.ApplyResources(this.lblUserPassword, "lblUserPassword");
            this.lblUserPassword.Name = "lblUserPassword";
            // 
            // txtOwnerPassword
            // 
            resources.ApplyResources(this.txtOwnerPassword, "txtOwnerPassword");
            this.txtOwnerPassword.Name = "txtOwnerPassword";
            this.txtOwnerPassword.UseSystemPasswordChar = true;
            // 
            // lblOwnerPassword
            // 
            resources.ApplyResources(this.lblOwnerPassword, "lblOwnerPassword");
            this.lblOwnerPassword.Name = "lblOwnerPassword";
            // 
            // cbEncryptPdf
            // 
            resources.ApplyResources(this.cbEncryptPdf, "cbEncryptPdf");
            this.cbEncryptPdf.Name = "cbEncryptPdf";
            this.cbEncryptPdf.UseVisualStyleBackColor = true;
            this.cbEncryptPdf.CheckedChanged += new System.EventHandler(this.cbEncryptPdf_CheckedChanged);
            // 
            // cbRememberSettings
            // 
            resources.ApplyResources(this.cbRememberSettings, "cbRememberSettings");
            this.cbRememberSettings.Name = "cbRememberSettings";
            this.cbRememberSettings.UseVisualStyleBackColor = true;
            // 
            // btnRestoreDefaults
            // 
            resources.ApplyResources(this.btnRestoreDefaults, "btnRestoreDefaults");
            this.btnRestoreDefaults.Name = "btnRestoreDefaults";
            this.btnRestoreDefaults.UseVisualStyleBackColor = true;
            this.btnRestoreDefaults.Click += new System.EventHandler(this.btnRestoreDefaults_Click);
            // 
            // linkPlaceholders
            // 
            resources.ApplyResources(this.linkPlaceholders, "linkPlaceholders");
            this.linkPlaceholders.Name = "linkPlaceholders";
            this.linkPlaceholders.TabStop = true;
            this.linkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPlaceholders_LinkClicked);
            // 
            // txtDefaultFilePath
            // 
            resources.ApplyResources(this.txtDefaultFilePath, "txtDefaultFilePath");
            this.txtDefaultFilePath.Name = "txtDefaultFilePath";
            this.txtDefaultFilePath.TextChanged += new System.EventHandler(this.txtDefaultFilePath_TextChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnChooseFolder
            // 
            resources.ApplyResources(this.btnChooseFolder, "btnChooseFolder");
            this.btnChooseFolder.Name = "btnChooseFolder";
            this.btnChooseFolder.UseVisualStyleBackColor = true;
            this.btnChooseFolder.Click += new System.EventHandler(this.btnChooseFolder_Click);
            // 
            // cbSkipSavePrompt
            // 
            resources.ApplyResources(this.cbSkipSavePrompt, "cbSkipSavePrompt");
            this.cbSkipSavePrompt.Name = "cbSkipSavePrompt";
            this.cbSkipSavePrompt.UseVisualStyleBackColor = true;
            // 
            // groupCompat
            // 
            this.groupCompat.Controls.Add(this.cmbCompat);
            resources.ApplyResources(this.groupCompat, "groupCompat");
            this.groupCompat.Name = "groupCompat";
            this.groupCompat.TabStop = false;
            // 
            // cmbCompat
            // 
            this.cmbCompat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCompat.FormattingEnabled = true;
            resources.ApplyResources(this.cmbCompat, "cmbCompat");
            this.cmbCompat.Name = "cmbCompat";
            // 
            // cbSinglePagePdf
            // 
            resources.ApplyResources(this.cbSinglePagePdf, "cbSinglePagePdf");
            this.cbSinglePagePdf.Name = "cbSinglePagePdf";
            this.cbSinglePagePdf.UseVisualStyleBackColor = true;
            // 
            // FPdfSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbSinglePagePdf);
            this.Controls.Add(this.groupCompat);
            this.Controls.Add(this.cbSkipSavePrompt);
            this.Controls.Add(this.btnChooseFolder);
            this.Controls.Add(this.linkPlaceholders);
            this.Controls.Add(this.txtDefaultFilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRestoreDefaults);
            this.Controls.Add(this.cbRememberSettings);
            this.Controls.Add(this.groupProtection);
            this.Controls.Add(this.groupMetadata);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FPdfSettings";
            this.groupMetadata.ResumeLayout(false);
            this.groupMetadata.PerformLayout();
            this.groupProtection.ResumeLayout(false);
            this.groupProtection.PerformLayout();
            this.groupCompat.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.GroupBox groupMetadata;
        private System.Windows.Forms.GroupBox groupProtection;
        private System.Windows.Forms.CheckBox cbEncryptPdf;
        private System.Windows.Forms.CheckBox cbRememberSettings;
        private System.Windows.Forms.TextBox txtOwnerPassword;
        private System.Windows.Forms.Label lblOwnerPassword;
        private System.Windows.Forms.TextBox txtUserPassword;
        private System.Windows.Forms.Label lblUserPassword;
        private System.Windows.Forms.Button btnRestoreDefaults;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtKeywords;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbShowUserPassword;
        private System.Windows.Forms.CheckBox cbShowOwnerPassword;
        private System.Windows.Forms.LinkLabel linkPlaceholders;
        private System.Windows.Forms.TextBox txtDefaultFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnChooseFolder;
        private System.Windows.Forms.CheckBox cbSkipSavePrompt;
        private System.Windows.Forms.CheckedListBox clbPerms;
        private System.Windows.Forms.GroupBox groupCompat;
        private System.Windows.Forms.ComboBox cmbCompat;
        private System.Windows.Forms.CheckBox cbSinglePagePdf;
    }
}
