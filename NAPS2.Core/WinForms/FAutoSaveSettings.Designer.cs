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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.linkPatchCodeInfo = new System.Windows.Forms.LinkLabel();
            this.rdSeparateByPatchT = new System.Windows.Forms.RadioButton();
            this.rdFilePerPage = new System.Windows.Forms.RadioButton();
            this.rdFilePerScan = new System.Windows.Forms.RadioButton();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.linkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.cbClearAfterSave = new System.Windows.Forms.CheckBox();
            this.cbPromptForFilePath = new System.Windows.Forms.CheckBox();
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
            // linkPatchCodeInfo
            // 
            resources.ApplyResources(this.linkPatchCodeInfo, "linkPatchCodeInfo");
            this.linkPatchCodeInfo.Name = "linkPatchCodeInfo";
            this.linkPatchCodeInfo.TabStop = true;
            this.linkPatchCodeInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPatchCodeInfo_LinkClicked);
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
            // btnChooseFolder
            // 
            resources.ApplyResources(this.btnChooseFolder, "btnChooseFolder");
            this.btnChooseFolder.Name = "btnChooseFolder";
            this.btnChooseFolder.UseVisualStyleBackColor = true;
            this.btnChooseFolder.Click += new System.EventHandler(this.btnChooseFolder_Click);
            // 
            // linkPlaceholders
            // 
            resources.ApplyResources(this.linkPlaceholders, "linkPlaceholders");
            this.linkPlaceholders.Name = "linkPlaceholders";
            this.linkPlaceholders.TabStop = true;
            this.linkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPlaceholders_LinkClicked);
            // 
            // txtFilePath
            // 
            resources.ApplyResources(this.txtFilePath, "txtFilePath");
            this.txtFilePath.Name = "txtFilePath";
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
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbPromptForFilePath);
            this.Controls.Add(this.cbClearAfterSave);
            this.Controls.Add(this.btnChooseFolder);
            this.Controls.Add(this.linkPlaceholders);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.linkPatchCodeInfo);
            this.Controls.Add(this.rdSeparateByPatchT);
            this.Controls.Add(this.rdFilePerPage);
            this.Controls.Add(this.rdFilePerScan);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FAutoSaveSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.LinkLabel linkPatchCodeInfo;
        private System.Windows.Forms.RadioButton rdSeparateByPatchT;
        private System.Windows.Forms.RadioButton rdFilePerPage;
        private System.Windows.Forms.RadioButton rdFilePerScan;
        private System.Windows.Forms.Button btnChooseFolder;
        private System.Windows.Forms.LinkLabel linkPlaceholders;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.CheckBox cbClearAfterSave;
        private System.Windows.Forms.CheckBox cbPromptForFilePath;
    }
}
