using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FBatchScan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FBatchScan));
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnStart = new System.Windows.Forms.Button();
            this.groupboxScanConfig = new System.Windows.Forms.GroupBox();
            this.panelScanDetails = new System.Windows.Forms.Panel();
            this.txtTimeBetweenScans = new System.Windows.Forms.TextBox();
            this.lblTimeBetweenScans = new System.Windows.Forms.Label();
            this.txtNumberOfScans = new System.Windows.Forms.TextBox();
            this.lblNumberOfScans = new System.Windows.Forms.Label();
            this.panelScanType = new System.Windows.Forms.Panel();
            this.rdMultipleScansPrompt = new System.Windows.Forms.RadioButton();
            this.rdMultipleScansDelay = new System.Windows.Forms.RadioButton();
            this.rdSingleScan = new System.Windows.Forms.RadioButton();
            this.BtnAddProfile = new System.Windows.Forms.Button();
            this.BtnEditProfile = new System.Windows.Forms.Button();
            this.comboProfile = new System.Windows.Forms.ComboBox();
            this.lblProfile = new System.Windows.Forms.Label();
            this.groupboxOutput = new System.Windows.Forms.GroupBox();
            this.panelSaveSeparator = new System.Windows.Forms.Panel();
            this.LinkPatchCodeInfo = new System.Windows.Forms.LinkLabel();
            this.rdSeparateByPatchT = new System.Windows.Forms.RadioButton();
            this.rdFilePerPage = new System.Windows.Forms.RadioButton();
            this.rdFilePerScan = new System.Windows.Forms.RadioButton();
            this.panelSaveTo = new System.Windows.Forms.Panel();
            this.BtnChooseFolder = new System.Windows.Forms.Button();
            this.LinkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.TxtFilePath = new System.Windows.Forms.TextBox();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.panelSaveType = new System.Windows.Forms.Panel();
            this.rdSaveToMultipleFiles = new System.Windows.Forms.RadioButton();
            this.rdSaveToSingleFile = new System.Windows.Forms.RadioButton();
            this.rdLoadIntoNaps2 = new System.Windows.Forms.RadioButton();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupboxScanConfig.SuspendLayout();
            this.panelScanDetails.SuspendLayout();
            this.panelScanType.SuspendLayout();
            this.groupboxOutput.SuspendLayout();
            this.panelSaveSeparator.SuspendLayout();
            this.panelSaveTo.SuspendLayout();
            this.panelSaveType.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // lblStatus
            // 
            this.lblStatus.AutoEllipsis = true;
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnStart
            // 
            resources.ApplyResources(this.BtnStart, "BtnStart");
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // groupboxScanConfig
            // 
            this.groupboxScanConfig.Controls.Add(this.panelScanDetails);
            this.groupboxScanConfig.Controls.Add(this.panelScanType);
            this.groupboxScanConfig.Controls.Add(this.BtnAddProfile);
            this.groupboxScanConfig.Controls.Add(this.BtnEditProfile);
            this.groupboxScanConfig.Controls.Add(this.comboProfile);
            this.groupboxScanConfig.Controls.Add(this.lblProfile);
            resources.ApplyResources(this.groupboxScanConfig, "groupboxScanConfig");
            this.groupboxScanConfig.Name = "groupboxScanConfig";
            this.groupboxScanConfig.TabStop = false;
            // 
            // panelScanDetails
            // 
            this.panelScanDetails.Controls.Add(this.txtTimeBetweenScans);
            this.panelScanDetails.Controls.Add(this.lblTimeBetweenScans);
            this.panelScanDetails.Controls.Add(this.txtNumberOfScans);
            this.panelScanDetails.Controls.Add(this.lblNumberOfScans);
            resources.ApplyResources(this.panelScanDetails, "panelScanDetails");
            this.panelScanDetails.Name = "panelScanDetails";
            // 
            // txtTimeBetweenScans
            // 
            resources.ApplyResources(this.txtTimeBetweenScans, "txtTimeBetweenScans");
            this.txtTimeBetweenScans.Name = "txtTimeBetweenScans";
            // 
            // lblTimeBetweenScans
            // 
            resources.ApplyResources(this.lblTimeBetweenScans, "lblTimeBetweenScans");
            this.lblTimeBetweenScans.Name = "lblTimeBetweenScans";
            // 
            // txtNumberOfScans
            // 
            resources.ApplyResources(this.txtNumberOfScans, "txtNumberOfScans");
            this.txtNumberOfScans.Name = "txtNumberOfScans";
            // 
            // lblNumberOfScans
            // 
            resources.ApplyResources(this.lblNumberOfScans, "lblNumberOfScans");
            this.lblNumberOfScans.Name = "lblNumberOfScans";
            // 
            // panelScanType
            // 
            this.panelScanType.Controls.Add(this.rdMultipleScansPrompt);
            this.panelScanType.Controls.Add(this.rdMultipleScansDelay);
            this.panelScanType.Controls.Add(this.rdSingleScan);
            resources.ApplyResources(this.panelScanType, "panelScanType");
            this.panelScanType.Name = "panelScanType";
            // 
            // rdMultipleScansPrompt
            // 
            resources.ApplyResources(this.rdMultipleScansPrompt, "rdMultipleScansPrompt");
            this.rdMultipleScansPrompt.Name = "rdMultipleScansPrompt";
            this.rdMultipleScansPrompt.UseVisualStyleBackColor = true;
            // 
            // rdMultipleScansDelay
            // 
            resources.ApplyResources(this.rdMultipleScansDelay, "rdMultipleScansDelay");
            this.rdMultipleScansDelay.Checked = true;
            this.rdMultipleScansDelay.Name = "rdMultipleScansDelay";
            this.rdMultipleScansDelay.TabStop = true;
            this.rdMultipleScansDelay.UseVisualStyleBackColor = true;
            this.rdMultipleScansDelay.CheckedChanged += new System.EventHandler(this.rdMultipleScansDelay_CheckedChanged);
            // 
            // rdSingleScan
            // 
            resources.ApplyResources(this.rdSingleScan, "rdSingleScan");
            this.rdSingleScan.Name = "rdSingleScan";
            this.rdSingleScan.UseVisualStyleBackColor = true;
            this.rdSingleScan.CheckedChanged += new System.EventHandler(this.rdSingleScan_CheckedChanged);
            // 
            // BtnAddProfile
            // 
            this.BtnAddProfile.Image = global::NAPS2.Icons.add_small;
            resources.ApplyResources(this.BtnAddProfile, "BtnAddProfile");
            this.BtnAddProfile.Name = "BtnAddProfile";
            this.BtnAddProfile.UseVisualStyleBackColor = true;
            this.BtnAddProfile.Click += new System.EventHandler(this.BtnAddProfile_Click);
            // 
            // BtnEditProfile
            // 
            this.BtnEditProfile.Image = global::NAPS2.Icons.pencil_small;
            resources.ApplyResources(this.BtnEditProfile, "BtnEditProfile");
            this.BtnEditProfile.Name = "BtnEditProfile";
            this.BtnEditProfile.UseVisualStyleBackColor = true;
            this.BtnEditProfile.Click += new System.EventHandler(this.BtnEditProfile_Click);
            // 
            // comboProfile
            // 
            this.comboProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboProfile.FormattingEnabled = true;
            resources.ApplyResources(this.comboProfile, "comboProfile");
            this.comboProfile.Name = "comboProfile";
            this.comboProfile.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.comboProfile_Format);
            // 
            // lblProfile
            // 
            resources.ApplyResources(this.lblProfile, "lblProfile");
            this.lblProfile.Name = "lblProfile";
            // 
            // groupboxOutput
            // 
            this.groupboxOutput.Controls.Add(this.panelSaveSeparator);
            this.groupboxOutput.Controls.Add(this.panelSaveTo);
            this.groupboxOutput.Controls.Add(this.panelSaveType);
            resources.ApplyResources(this.groupboxOutput, "groupboxOutput");
            this.groupboxOutput.Name = "groupboxOutput";
            this.groupboxOutput.TabStop = false;
            // 
            // panelSaveSeparator
            // 
            this.panelSaveSeparator.Controls.Add(this.LinkPatchCodeInfo);
            this.panelSaveSeparator.Controls.Add(this.rdSeparateByPatchT);
            this.panelSaveSeparator.Controls.Add(this.rdFilePerPage);
            this.panelSaveSeparator.Controls.Add(this.rdFilePerScan);
            resources.ApplyResources(this.panelSaveSeparator, "panelSaveSeparator");
            this.panelSaveSeparator.Name = "panelSaveSeparator";
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
            this.rdFilePerPage.Name = "rdFilePerPage";
            this.rdFilePerPage.UseVisualStyleBackColor = true;
            // 
            // rdFilePerScan
            // 
            resources.ApplyResources(this.rdFilePerScan, "rdFilePerScan");
            this.rdFilePerScan.Checked = true;
            this.rdFilePerScan.Name = "rdFilePerScan";
            this.rdFilePerScan.TabStop = true;
            this.rdFilePerScan.UseVisualStyleBackColor = true;
            // 
            // panelSaveTo
            // 
            this.panelSaveTo.Controls.Add(this.BtnChooseFolder);
            this.panelSaveTo.Controls.Add(this.LinkPlaceholders);
            this.panelSaveTo.Controls.Add(this.TxtFilePath);
            this.panelSaveTo.Controls.Add(this.lblFilePath);
            resources.ApplyResources(this.panelSaveTo, "panelSaveTo");
            this.panelSaveTo.Name = "panelSaveTo";
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
            // panelSaveType
            // 
            this.panelSaveType.Controls.Add(this.rdSaveToMultipleFiles);
            this.panelSaveType.Controls.Add(this.rdSaveToSingleFile);
            this.panelSaveType.Controls.Add(this.rdLoadIntoNaps2);
            resources.ApplyResources(this.panelSaveType, "panelSaveType");
            this.panelSaveType.Name = "panelSaveType";
            // 
            // rdSaveToMultipleFiles
            // 
            resources.ApplyResources(this.rdSaveToMultipleFiles, "rdSaveToMultipleFiles");
            this.rdSaveToMultipleFiles.Checked = true;
            this.rdSaveToMultipleFiles.Name = "rdSaveToMultipleFiles";
            this.rdSaveToMultipleFiles.TabStop = true;
            this.rdSaveToMultipleFiles.UseVisualStyleBackColor = true;
            this.rdSaveToMultipleFiles.CheckedChanged += new System.EventHandler(this.rdSaveToMultipleFiles_CheckedChanged);
            // 
            // rdSaveToSingleFile
            // 
            resources.ApplyResources(this.rdSaveToSingleFile, "rdSaveToSingleFile");
            this.rdSaveToSingleFile.Name = "rdSaveToSingleFile";
            this.rdSaveToSingleFile.UseVisualStyleBackColor = true;
            // 
            // rdLoadIntoNaps2
            // 
            resources.ApplyResources(this.rdLoadIntoNaps2, "rdLoadIntoNaps2");
            this.rdLoadIntoNaps2.Name = "rdLoadIntoNaps2";
            this.rdLoadIntoNaps2.UseVisualStyleBackColor = true;
            this.rdLoadIntoNaps2.CheckedChanged += new System.EventHandler(this.rdLoadIntoNaps2_CheckedChanged);
            // 
            // FBatchScan
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupboxOutput);
            this.Controls.Add(this.groupboxScanConfig);
            this.Controls.Add(this.BtnStart);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.lblStatus);
            this.MaximizeBox = false;
            this.Name = "FBatchScan";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FBatchScan_FormClosing);
            this.groupboxScanConfig.ResumeLayout(false);
            this.groupboxScanConfig.PerformLayout();
            this.panelScanDetails.ResumeLayout(false);
            this.panelScanDetails.PerformLayout();
            this.panelScanType.ResumeLayout(false);
            this.panelScanType.PerformLayout();
            this.groupboxOutput.ResumeLayout(false);
            this.panelSaveSeparator.ResumeLayout(false);
            this.panelSaveSeparator.PerformLayout();
            this.panelSaveTo.ResumeLayout(false);
            this.panelSaveTo.PerformLayout();
            this.panelSaveType.ResumeLayout(false);
            this.panelSaveType.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnStart;
        private System.Windows.Forms.GroupBox groupboxScanConfig;
        private System.Windows.Forms.Button BtnAddProfile;
        private System.Windows.Forms.Button BtnEditProfile;
        private System.Windows.Forms.ComboBox comboProfile;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.Panel panelScanType;
        private System.Windows.Forms.RadioButton rdMultipleScansDelay;
        private System.Windows.Forms.RadioButton rdSingleScan;
        private System.Windows.Forms.Panel panelScanDetails;
        private System.Windows.Forms.Label lblNumberOfScans;
        private System.Windows.Forms.TextBox txtNumberOfScans;
        private System.Windows.Forms.TextBox txtTimeBetweenScans;
        private System.Windows.Forms.Label lblTimeBetweenScans;
        private System.Windows.Forms.GroupBox groupboxOutput;
        private System.Windows.Forms.Panel panelSaveTo;
        private System.Windows.Forms.Button BtnChooseFolder;
        private System.Windows.Forms.LinkLabel LinkPlaceholders;
        private System.Windows.Forms.TextBox TxtFilePath;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Panel panelSaveType;
        private System.Windows.Forms.RadioButton rdSaveToMultipleFiles;
        private System.Windows.Forms.RadioButton rdSaveToSingleFile;
        private System.Windows.Forms.RadioButton rdLoadIntoNaps2;
        private System.Windows.Forms.Panel panelSaveSeparator;
        private System.Windows.Forms.LinkLabel LinkPatchCodeInfo;
        private System.Windows.Forms.RadioButton rdSeparateByPatchT;
        private System.Windows.Forms.RadioButton rdFilePerPage;
        private System.Windows.Forms.RadioButton rdFilePerScan;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.RadioButton rdMultipleScansPrompt;
    }
}
