using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FAutoSaveSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAutoSaveSettings));
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.LinkPatchCodeInfo = new System.Windows.Forms.LinkLabel();
            this.rdSeparateByPatchT = new System.Windows.Forms.RadioButton();
            this.rdFilePerPage = new System.Windows.Forms.RadioButton();
            this.rdFilePerScan = new System.Windows.Forms.RadioButton();
            this.BtnChooseFolder = new System.Windows.Forms.Button();
            this.LinkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.TxtFilePath = new System.Windows.Forms.TextBox();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.cbClearAfterSave = new System.Windows.Forms.CheckBox();
            this.cbPromptForFilePath = new System.Windows.Forms.CheckBox();
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
            // LinkPatchCodeInfo
            // 
            resources.ApplyResources(this.LinkPatchCodeInfo, "LinkPatchCodeInfo");
            this.LinkPatchCodeInfo.Name = "LinkPatchCodeInfo";
            this.LinkPatchCodeInfo.TabStop = true;
            this.LinkPatchCodeInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkPatchCodeInfo_LinkClicked);
            // 
            // rdSeparateByPatchT
            // 
            resources.ApplyResources(this.rdSeparateByPatchT, "rdSeparateByPatchT");
            this.rdSeparateByPatchT.Name = "rdSeparateByPatchT";
            this.rdSeparateByPatchT.UseVisualStyleBackColor = true;
            // 
            // rdFilePerPage
            // 
            resources.ApplyResources(this.rdFilePerPage, "rdFilePerPage");
            this.rdFilePerPage.Checked = true;
            this.rdFilePerPage.Name = "rdFilePerPage";
            this.rdFilePerPage.TabStop = true;
            this.rdFilePerPage.UseVisualStyleBackColor = true;
            // 
            // rdFilePerScan
            // 
            resources.ApplyResources(this.rdFilePerScan, "rdFilePerScan");
            this.rdFilePerScan.Name = "rdFilePerScan";
            this.rdFilePerScan.UseVisualStyleBackColor = true;
            // 
            // BtnChooseFolder
            // 
            resources.ApplyResources(this.BtnChooseFolder, "BtnChooseFolder");
            this.BtnChooseFolder.Name = "BtnChooseFolder";
            this.BtnChooseFolder.UseVisualStyleBackColor = true;
            this.BtnChooseFolder.Click += new System.EventHandler(this.BtnChooseFolder_Click);
            // 
            // LinkPlaceholders
            // 
            resources.ApplyResources(this.LinkPlaceholders, "LinkPlaceholders");
            this.LinkPlaceholders.Name = "LinkPlaceholders";
            this.LinkPlaceholders.TabStop = true;
            this.LinkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkPlaceholders_LinkClicked);
            // 
            // TxtFilePath
            // 
            resources.ApplyResources(this.TxtFilePath, "TxtFilePath");
            this.TxtFilePath.Name = "TxtFilePath";
            // 
            // lblFilePath
            // 
            resources.ApplyResources(this.lblFilePath, "lblFilePath");
            this.lblFilePath.Name = "lblFilePath";
            // 
            // cbClearAfterSave
            // 
            resources.ApplyResources(this.cbClearAfterSave, "cbClearAfterSave");
            this.cbClearAfterSave.Name = "cbClearAfterSave";
            this.cbClearAfterSave.UseVisualStyleBackColor = true;
            // 
            // cbPromptForFilePath
            // 
            resources.ApplyResources(this.cbPromptForFilePath, "cbPromptForFilePath");
            this.cbPromptForFilePath.Name = "cbPromptForFilePath";
            this.cbPromptForFilePath.UseVisualStyleBackColor = true;
            // 
            // FAutoSaveSettings
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbPromptForFilePath);
            this.Controls.Add(this.cbClearAfterSave);
            this.Controls.Add(this.BtnChooseFolder);
            this.Controls.Add(this.LinkPlaceholders);
            this.Controls.Add(this.TxtFilePath);
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.LinkPatchCodeInfo);
            this.Controls.Add(this.rdSeparateByPatchT);
            this.Controls.Add(this.rdFilePerPage);
            this.Controls.Add(this.rdFilePerScan);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FAutoSaveSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.LinkLabel LinkPatchCodeInfo;
        private System.Windows.Forms.RadioButton rdSeparateByPatchT;
        private System.Windows.Forms.RadioButton rdFilePerPage;
        private System.Windows.Forms.RadioButton rdFilePerScan;
        private System.Windows.Forms.Button BtnChooseFolder;
        private System.Windows.Forms.LinkLabel LinkPlaceholders;
        private System.Windows.Forms.TextBox TxtFilePath;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.CheckBox cbClearAfterSave;
        private System.Windows.Forms.CheckBox cbPromptForFilePath;
    }
}
