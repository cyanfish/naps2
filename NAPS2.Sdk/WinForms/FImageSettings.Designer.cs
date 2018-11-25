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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.cbRememberSettings = new System.Windows.Forms.CheckBox();
            this.btnRestoreDefaults = new System.Windows.Forms.Button();
            this.txtDefaultFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.linkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.txtJpegQuality = new System.Windows.Forms.TextBox();
            this.tbJpegQuality = new System.Windows.Forms.TrackBar();
            this.lblWarning = new System.Windows.Forms.Label();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.cbSkipSavePrompt = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbTiffCompr = new System.Windows.Forms.ComboBox();
            this.cbSinglePageTiff = new System.Windows.Forms.CheckBox();
            this.groupJpeg = new System.Windows.Forms.GroupBox();
            this.groupTiff = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbJpegQuality)).BeginInit();
            this.groupJpeg.SuspendLayout();
            this.groupTiff.SuspendLayout();
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
            // linkPlaceholders
            // 
            resources.ApplyResources(this.linkPlaceholders, "linkPlaceholders");
            this.linkPlaceholders.Name = "linkPlaceholders";
            this.linkPlaceholders.TabStop = true;
            this.linkPlaceholders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPlaceholders_LinkClicked);
            // 
            // txtJpegQuality
            // 
            resources.ApplyResources(this.txtJpegQuality, "txtJpegQuality");
            this.txtJpegQuality.Name = "txtJpegQuality";
            this.txtJpegQuality.TextChanged += new System.EventHandler(this.txtJpegQuality_TextChanged);
            // 
            // tbJpegQuality
            // 
            resources.ApplyResources(this.tbJpegQuality, "tbJpegQuality");
            this.tbJpegQuality.Maximum = 100;
            this.tbJpegQuality.Name = "tbJpegQuality";
            this.tbJpegQuality.TickFrequency = 25;
            this.tbJpegQuality.Scroll += new System.EventHandler(this.tbJpegQuality_Scroll);
            // 
            // lblWarning
            // 
            resources.ApplyResources(this.lblWarning, "lblWarning");
            this.lblWarning.Name = "lblWarning";
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
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
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
            this.groupJpeg.Controls.Add(this.tbJpegQuality);
            this.groupJpeg.Controls.Add(this.txtJpegQuality);
            resources.ApplyResources(this.groupJpeg, "groupJpeg");
            this.groupJpeg.Name = "groupJpeg";
            this.groupJpeg.TabStop = false;
            // 
            // groupTiff
            // 
            this.groupTiff.Controls.Add(this.label3);
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
            this.Controls.Add(this.btnChooseFolder);
            this.Controls.Add(this.linkPlaceholders);
            this.Controls.Add(this.txtDefaultFilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRestoreDefaults);
            this.Controls.Add(this.cbRememberSettings);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FImageSettings";
            ((System.ComponentModel.ISupportInitialize)(this.tbJpegQuality)).EndInit();
            this.groupJpeg.ResumeLayout(false);
            this.groupJpeg.PerformLayout();
            this.groupTiff.ResumeLayout(false);
            this.groupTiff.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.CheckBox cbRememberSettings;
        private System.Windows.Forms.Button btnRestoreDefaults;
        private System.Windows.Forms.TextBox txtDefaultFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkPlaceholders;
        private System.Windows.Forms.TextBox txtJpegQuality;
        private System.Windows.Forms.TrackBar tbJpegQuality;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Button btnChooseFolder;
        private System.Windows.Forms.CheckBox cbSkipSavePrompt;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbTiffCompr;
        private System.Windows.Forms.CheckBox cbSinglePageTiff;
        private System.Windows.Forms.GroupBox groupJpeg;
        private System.Windows.Forms.GroupBox groupTiff;
    }
}
