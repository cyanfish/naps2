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
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.groupMetadata = new System.Windows.Forms.GroupBox();
            this.txtKeywords = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.TxtSubject = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.TxtAuthor = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.groupProtection = new System.Windows.Forms.GroupBox();
            this.clbPerms = new System.Windows.Forms.CheckedListBox();
            this.CbShowUserPassword = new System.Windows.Forms.CheckBox();
            this.CbShowOwnerPassword = new System.Windows.Forms.CheckBox();
            this.txtUserPassword = new System.Windows.Forms.TextBox();
            this.lblUserPassword = new System.Windows.Forms.Label();
            this.txtOwnerPassword = new System.Windows.Forms.TextBox();
            this.lblOwnerPassword = new System.Windows.Forms.Label();
            this.CbEncryptPdf = new System.Windows.Forms.CheckBox();
            this.cbRememberSettings = new System.Windows.Forms.CheckBox();
            this.BtnRestoreDefaults = new System.Windows.Forms.Button();
            this.LinkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.TxtDefaultFilePath = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.BtnChooseFolder = new System.Windows.Forms.Button();
            this.cbSkipSavePrompt = new System.Windows.Forms.CheckBox();
            this.groupCompat = new System.Windows.Forms.GroupBox();
            this.cmbCompat = new System.Windows.Forms.ComboBox();
            this.groupMetadata.SuspendLayout();
            this.groupProtection.SuspendLayout();
            this.groupCompat.SuspendLayout();
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
            // groupMetadata
            // 
            this.groupMetadata.Controls.Add(this.txtKeywords);
            this.groupMetadata.Controls.Add(this.Label6);
            this.groupMetadata.Controls.Add(this.TxtSubject);
            this.groupMetadata.Controls.Add(this.Label5);
            this.groupMetadata.Controls.Add(this.TxtAuthor);
            this.groupMetadata.Controls.Add(this.Label3);
            this.groupMetadata.Controls.Add(this.txtTitle);
            this.groupMetadata.Controls.Add(this.Label4);
            resources.ApplyResources(this.groupMetadata, "groupMetadata");
            this.groupMetadata.Name = "groupMetadata";
            this.groupMetadata.TabStop = false;
            // 
            // txtKeywords
            // 
            resources.ApplyResources(this.txtKeywords, "txtKeywords");
            this.txtKeywords.Name = "txtKeywords";
            // 
            // Label6
            // 
            resources.ApplyResources(this.Label6, "Label6");
            this.Label6.Name = "Label6";
            // 
            // TxtSubject
            // 
            resources.ApplyResources(this.TxtSubject, "TxtSubject");
            this.TxtSubject.Name = "TxtSubject";
            // 
            // Label5
            // 
            resources.ApplyResources(this.Label5, "Label5");
            this.Label5.Name = "Label5";
            // 
            // TxtAuthor
            // 
            resources.ApplyResources(this.TxtAuthor, "TxtAuthor");
            this.TxtAuthor.Name = "TxtAuthor";
            // 
            // Label3
            // 
            resources.ApplyResources(this.Label3, "Label3");
            this.Label3.Name = "Label3";
            // 
            // txtTitle
            // 
            resources.ApplyResources(this.txtTitle, "txtTitle");
            this.txtTitle.Name = "txtTitle";
            // 
            // Label4
            // 
            resources.ApplyResources(this.Label4, "Label4");
            this.Label4.Name = "Label4";
            // 
            // groupProtection
            // 
            this.groupProtection.Controls.Add(this.clbPerms);
            this.groupProtection.Controls.Add(this.CbShowUserPassword);
            this.groupProtection.Controls.Add(this.CbShowOwnerPassword);
            this.groupProtection.Controls.Add(this.txtUserPassword);
            this.groupProtection.Controls.Add(this.lblUserPassword);
            this.groupProtection.Controls.Add(this.txtOwnerPassword);
            this.groupProtection.Controls.Add(this.lblOwnerPassword);
            this.groupProtection.Controls.Add(this.CbEncryptPdf);
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
            // CbShowUserPassword
            // 
            resources.ApplyResources(this.CbShowUserPassword, "CbShowUserPassword");
            this.CbShowUserPassword.Name = "CbShowUserPassword";
            this.CbShowUserPassword.UseVisualStyleBackColor = true;
            this.CbShowUserPassword.CheckedChanged += new System.EventHandler(this.CbShowUserPassword_CheckedChanged);
            // 
            // CbShowOwnerPassword
            // 
            resources.ApplyResources(this.CbShowOwnerPassword, "CbShowOwnerPassword");
            this.CbShowOwnerPassword.Name = "CbShowOwnerPassword";
            this.CbShowOwnerPassword.UseVisualStyleBackColor = true;
            this.CbShowOwnerPassword.CheckedChanged += new System.EventHandler(this.CbShowOwnerPassword_CheckedChanged);
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
            // CbEncryptPdf
            // 
            resources.ApplyResources(this.CbEncryptPdf, "CbEncryptPdf");
            this.CbEncryptPdf.Name = "CbEncryptPdf";
            this.CbEncryptPdf.UseVisualStyleBackColor = true;
            this.CbEncryptPdf.CheckedChanged += new System.EventHandler(this.CbEncryptPdf_CheckedChanged);
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
            // LinkPlaceholders
            // 
            resources.ApplyResources(this.LinkPlaceholders, "LinkPlaceholders");
            this.LinkPlaceholders.Name = "LinkPlaceholders";
            this.LinkPlaceholders.TabStop = true;
            this.LinkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkPlaceholders_LinkClicked);
            // 
            // TxtDefaultFilePath
            // 
            resources.ApplyResources(this.TxtDefaultFilePath, "TxtDefaultFilePath");
            this.TxtDefaultFilePath.Name = "TxtDefaultFilePath";
            this.TxtDefaultFilePath.TextChanged += new System.EventHandler(this.TxtDefaultFilePath_TextChanged);
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // BtnChooseFolder
            // 
            resources.ApplyResources(this.BtnChooseFolder, "BtnChooseFolder");
            this.BtnChooseFolder.Name = "BtnChooseFolder";
            this.BtnChooseFolder.UseVisualStyleBackColor = true;
            this.BtnChooseFolder.Click += new System.EventHandler(this.BtnChooseFolder_Click);
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
            // FPdfSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupCompat);
            this.Controls.Add(this.cbSkipSavePrompt);
            this.Controls.Add(this.BtnChooseFolder);
            this.Controls.Add(this.LinkPlaceholders);
            this.Controls.Add(this.TxtDefaultFilePath);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.BtnRestoreDefaults);
            this.Controls.Add(this.cbRememberSettings);
            this.Controls.Add(this.groupProtection);
            this.Controls.Add(this.groupMetadata);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
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

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.GroupBox groupMetadata;
        private System.Windows.Forms.GroupBox groupProtection;
        private System.Windows.Forms.CheckBox CbEncryptPdf;
        private System.Windows.Forms.CheckBox cbRememberSettings;
        private System.Windows.Forms.TextBox txtOwnerPassword;
        private System.Windows.Forms.Label lblOwnerPassword;
        private System.Windows.Forms.TextBox txtUserPassword;
        private System.Windows.Forms.Label lblUserPassword;
        private System.Windows.Forms.Button BtnRestoreDefaults;
        private System.Windows.Forms.TextBox TxtAuthor;
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label Label4;
        private System.Windows.Forms.TextBox TxtSubject;
        private System.Windows.Forms.Label Label5;
        private System.Windows.Forms.TextBox txtKeywords;
        private System.Windows.Forms.Label Label6;
        private System.Windows.Forms.CheckBox CbShowUserPassword;
        private System.Windows.Forms.CheckBox CbShowOwnerPassword;
        private System.Windows.Forms.LinkLabel LinkPlaceholders;
        private System.Windows.Forms.TextBox TxtDefaultFilePath;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Button BtnChooseFolder;
        private System.Windows.Forms.CheckBox cbSkipSavePrompt;
        private System.Windows.Forms.CheckedListBox clbPerms;
        private System.Windows.Forms.GroupBox groupCompat;
        private System.Windows.Forms.ComboBox cmbCompat;
    }
}
