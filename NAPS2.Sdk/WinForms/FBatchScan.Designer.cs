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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
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
            this.btnAddProfile = new System.Windows.Forms.Button();
            this.btnEditProfile = new System.Windows.Forms.Button();
            this.comboProfile = new System.Windows.Forms.ComboBox();
            this.lblProfile = new System.Windows.Forms.Label();
            this.groupboxOutput = new System.Windows.Forms.GroupBox();
            this.panelSaveSeparator = new System.Windows.Forms.Panel();
            this.linkPatchCodeInfo = new System.Windows.Forms.LinkLabel();
            this.rdSeparateByPatchT = new System.Windows.Forms.RadioButton();
            this.rdFilePerPage = new System.Windows.Forms.RadioButton();
            this.rdFilePerScan = new System.Windows.Forms.RadioButton();
            this.panelSaveTo = new System.Windows.Forms.Panel();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.linkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.txtFilePath = new System.Windows.Forms.TextBox();
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
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnStart
            // 
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.Name = "btnStart";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // groupboxScanConfig
            // 
            this.groupboxScanConfig.Controls.Add(this.panelScanDetails);
            this.groupboxScanConfig.Controls.Add(this.panelScanType);
            this.groupboxScanConfig.Controls.Add(this.btnAddProfile);
            this.groupboxScanConfig.Controls.Add(this.btnEditProfile);
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
            // btnAddProfile
            // 
            this.btnAddProfile.Image = global::NAPS2.Icons.add_small;
            resources.ApplyResources(this.btnAddProfile, "btnAddProfile");
            this.btnAddProfile.Name = "btnAddProfile";
            this.btnAddProfile.UseVisualStyleBackColor = true;
            this.btnAddProfile.Click += new System.EventHandler(this.btnAddProfile_Click);
            // 
            // btnEditProfile
            // 
            this.btnEditProfile.Image = global::NAPS2.Icons.pencil_small;
            resources.ApplyResources(this.btnEditProfile, "btnEditProfile");
            this.btnEditProfile.Name = "btnEditProfile";
            this.btnEditProfile.UseVisualStyleBackColor = true;
            this.btnEditProfile.Click += new System.EventHandler(this.btnEditProfile_Click);
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
            this.panelSaveSeparator.Controls.Add(this.linkPatchCodeInfo);
            this.panelSaveSeparator.Controls.Add(this.rdSeparateByPatchT);
            this.panelSaveSeparator.Controls.Add(this.rdFilePerPage);
            this.panelSaveSeparator.Controls.Add(this.rdFilePerScan);
            resources.ApplyResources(this.panelSaveSeparator, "panelSaveSeparator");
            this.panelSaveSeparator.Name = "panelSaveSeparator";
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
            this.panelSaveTo.Controls.Add(this.btnChooseFolder);
            this.panelSaveTo.Controls.Add(this.linkPlaceholders);
            this.panelSaveTo.Controls.Add(this.txtFilePath);
            this.panelSaveTo.Controls.Add(this.lblFilePath);
            resources.ApplyResources(this.panelSaveTo, "panelSaveTo");
            this.panelSaveTo.Name = "panelSaveTo";
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
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnCancel);
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
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.GroupBox groupboxScanConfig;
        private System.Windows.Forms.Button btnAddProfile;
        private System.Windows.Forms.Button btnEditProfile;
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
        private System.Windows.Forms.Button btnChooseFolder;
        private System.Windows.Forms.LinkLabel linkPlaceholders;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Panel panelSaveType;
        private System.Windows.Forms.RadioButton rdSaveToMultipleFiles;
        private System.Windows.Forms.RadioButton rdSaveToSingleFile;
        private System.Windows.Forms.RadioButton rdLoadIntoNaps2;
        private System.Windows.Forms.Panel panelSaveSeparator;
        private System.Windows.Forms.LinkLabel linkPatchCodeInfo;
        private System.Windows.Forms.RadioButton rdSeparateByPatchT;
        private System.Windows.Forms.RadioButton rdFilePerPage;
        private System.Windows.Forms.RadioButton rdFilePerScan;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.RadioButton rdMultipleScansPrompt;
    }
}
