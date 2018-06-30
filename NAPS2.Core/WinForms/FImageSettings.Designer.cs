using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FImageSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FImageSettings));
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.cbRememberSettings = new System.Windows.Forms.CheckBox();
            this.BtnRestoreDefaults = new System.Windows.Forms.Button();
            this.TxtDefaultFilePath = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.LinkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.TxtJpegQuality = new System.Windows.Forms.TextBox();
            this.TbJpegQuality = new System.Windows.Forms.TrackBar();
            this.lblWarning = new System.Windows.Forms.Label();
            this.BtnChooseFolder = new System.Windows.Forms.Button();
            this.cbSkipSavePrompt = new System.Windows.Forms.CheckBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.cmbTiffCompr = new System.Windows.Forms.ComboBox();
            this.cbSinglePageTiff = new System.Windows.Forms.CheckBox();
            this.groupJpeg = new System.Windows.Forms.GroupBox();
            this.groupTiff = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.TbJpegQuality)).BeginInit();
            this.groupJpeg.SuspendLayout();
            this.groupTiff.SuspendLayout();
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
            // LinkPlaceholders
            // 
            resources.ApplyResources(this.LinkPlaceholders, "LinkPlaceholders");
            this.LinkPlaceholders.Name = "LinkPlaceholders";
            this.LinkPlaceholders.TabStop = true;
            this.LinkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkPlaceholders_LinkClicked);
            // 
            // TxtJpegQuality
            // 
            resources.ApplyResources(this.TxtJpegQuality, "TxtJpegQuality");
            this.TxtJpegQuality.Name = "TxtJpegQuality";
            this.TxtJpegQuality.TextChanged += new System.EventHandler(this.TxtJpegQuality_TextChanged);
            // 
            // TbJpegQuality
            // 
            resources.ApplyResources(this.TbJpegQuality, "TbJpegQuality");
            this.TbJpegQuality.Maximum = 100;
            this.TbJpegQuality.Name = "TbJpegQuality";
            this.TbJpegQuality.TickFrequency = 25;
            this.TbJpegQuality.Scroll += new System.EventHandler(this.TbJpegQuality_Scroll);
            // 
            // lblWarning
            // 
            resources.ApplyResources(this.lblWarning, "lblWarning");
            this.lblWarning.Name = "lblWarning";
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
            // Label3
            // 
            resources.ApplyResources(this.Label3, "Label3");
            this.Label3.Name = "Label3";
            // 
            // cmbTiffCompr
            // 
            this.cmbTiffCompr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTiffCompr.FormattingEnabled = true;
            resources.ApplyResources(this.cmbTiffCompr, "cmbTiffCompr");
            this.cmbTiffCompr.Name = "cmbTiffCompr";
            // 
            // cbSinglePageTiff
            // 
            resources.ApplyResources(this.cbSinglePageTiff, "cbSinglePageTiff");
            this.cbSinglePageTiff.Name = "cbSinglePageTiff";
            this.cbSinglePageTiff.UseVisualStyleBackColor = true;
            // 
            // groupJpeg
            // 
            this.groupJpeg.Controls.Add(this.lblWarning);
            this.groupJpeg.Controls.Add(this.TbJpegQuality);
            this.groupJpeg.Controls.Add(this.TxtJpegQuality);
            resources.ApplyResources(this.groupJpeg, "groupJpeg");
            this.groupJpeg.Name = "groupJpeg";
            this.groupJpeg.TabStop = false;
            // 
            // groupTiff
            // 
            this.groupTiff.Controls.Add(this.Label3);
            this.groupTiff.Controls.Add(this.cmbTiffCompr);
            this.groupTiff.Controls.Add(this.cbSinglePageTiff);
            resources.ApplyResources(this.groupTiff, "groupTiff");
            this.groupTiff.Name = "groupTiff";
            this.groupTiff.TabStop = false;
            // 
            // FImageSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupTiff);
            this.Controls.Add(this.groupJpeg);
            this.Controls.Add(this.cbSkipSavePrompt);
            this.Controls.Add(this.BtnChooseFolder);
            this.Controls.Add(this.LinkPlaceholders);
            this.Controls.Add(this.TxtDefaultFilePath);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.BtnRestoreDefaults);
            this.Controls.Add(this.cbRememberSettings);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FImageSettings";
            ((System.ComponentModel.ISupportInitialize)(this.TbJpegQuality)).EndInit();
            this.groupJpeg.ResumeLayout(false);
            this.groupJpeg.PerformLayout();
            this.groupTiff.ResumeLayout(false);
            this.groupTiff.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.CheckBox cbRememberSettings;
        private System.Windows.Forms.Button BtnRestoreDefaults;
        private System.Windows.Forms.TextBox TxtDefaultFilePath;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.LinkLabel LinkPlaceholders;
        private System.Windows.Forms.TextBox TxtJpegQuality;
        private System.Windows.Forms.TrackBar TbJpegQuality;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Button BtnChooseFolder;
        private System.Windows.Forms.CheckBox cbSkipSavePrompt;
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.ComboBox cmbTiffCompr;
        private System.Windows.Forms.CheckBox cbSinglePageTiff;
        private System.Windows.Forms.GroupBox groupJpeg;
        private System.Windows.Forms.GroupBox groupTiff;
    }
}
