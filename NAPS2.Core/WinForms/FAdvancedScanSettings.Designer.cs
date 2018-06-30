using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FAdvancedScanSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAdvancedScanSettings));
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.CbHighQuality = new System.Windows.Forms.CheckBox();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.txtImageQuality = new System.Windows.Forms.TextBox();
            this.tbImageQuality = new System.Windows.Forms.TrackBar();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.txtWiaDelayBetweenScansSeconds = new System.Windows.Forms.TextBox();
            this.CbWiaDelayBetweenScans = new System.Windows.Forms.CheckBox();
            this.CbWiaRetryOnFailure = new System.Windows.Forms.CheckBox();
            this.CbForcePageSizeCrop = new System.Windows.Forms.CheckBox();
            this.cbFlipDuplex = new System.Windows.Forms.CheckBox();
            this.CbWiaOffsetWidth = new System.Windows.Forms.CheckBox();
            this.CbForcePageSize = new System.Windows.Forms.CheckBox();
            this.cbBrightnessContrastAfterScan = new System.Windows.Forms.CheckBox();
            this.cmbTwainImpl = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.TbCoverageThreshold = new System.Windows.Forms.TrackBar();
            this.TxtCoverageThreshold = new System.Windows.Forms.TextBox();
            this.TbWhiteThreshold = new System.Windows.Forms.TrackBar();
            this.TxtWhiteThreshold = new System.Windows.Forms.TextBox();
            this.CbExcludeBlankPages = new System.Windows.Forms.CheckBox();
            this.BtnRestoreDefaults = new System.Windows.Forms.Button();
            this.GroupBox4 = new System.Windows.Forms.GroupBox();
            this.CbAutoDeskew = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbImageQuality)).BeginInit();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.GroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TbCoverageThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbWhiteThreshold)).BeginInit();
            this.GroupBox4.SuspendLayout();
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
            // CbHighQuality
            // 
            resources.ApplyResources(this.CbHighQuality, "CbHighQuality");
            this.CbHighQuality.Name = "CbHighQuality";
            this.CbHighQuality.UseVisualStyleBackColor = true;
            this.CbHighQuality.CheckedChanged += new System.EventHandler(this.CbHighQuality_CheckedChanged);
            // 
            // txtImageQuality
            // 
            resources.ApplyResources(this.txtImageQuality, "txtImageQuality");
            this.txtImageQuality.Name = "txtImageQuality";
            this.txtImageQuality.TextChanged += new System.EventHandler(this.TxtImageQuality_TextChanged);
            // 
            // tbImageQuality
            // 
            resources.ApplyResources(this.tbImageQuality, "tbImageQuality");
            this.tbImageQuality.Maximum = 100;
            this.tbImageQuality.Name = "tbImageQuality";
            this.tbImageQuality.TickFrequency = 25;
            this.tbImageQuality.Scroll += new System.EventHandler(this.TbImageQuality_Scroll);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.tbImageQuality);
            this.GroupBox1.Controls.Add(this.txtImageQuality);
            this.GroupBox1.Controls.Add(this.CbHighQuality);
            resources.ApplyResources(this.GroupBox1, "GroupBox1");
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.TabStop = false;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.txtWiaDelayBetweenScansSeconds);
            this.GroupBox2.Controls.Add(this.CbWiaDelayBetweenScans);
            this.GroupBox2.Controls.Add(this.CbWiaRetryOnFailure);
            this.GroupBox2.Controls.Add(this.CbForcePageSizeCrop);
            this.GroupBox2.Controls.Add(this.cbFlipDuplex);
            this.GroupBox2.Controls.Add(this.CbWiaOffsetWidth);
            this.GroupBox2.Controls.Add(this.CbForcePageSize);
            this.GroupBox2.Controls.Add(this.cbBrightnessContrastAfterScan);
            this.GroupBox2.Controls.Add(this.cmbTwainImpl);
            this.GroupBox2.Controls.Add(this.Label1);
            resources.ApplyResources(this.GroupBox2, "GroupBox2");
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.TabStop = false;
            // 
            // txtWiaDelayBetweenScansSeconds
            // 
            resources.ApplyResources(this.txtWiaDelayBetweenScansSeconds, "txtWiaDelayBetweenScansSeconds");
            this.txtWiaDelayBetweenScansSeconds.Name = "txtWiaDelayBetweenScansSeconds";
            // 
            // CbWiaDelayBetweenScans
            // 
            resources.ApplyResources(this.CbWiaDelayBetweenScans, "CbWiaDelayBetweenScans");
            this.CbWiaDelayBetweenScans.Name = "CbWiaDelayBetweenScans";
            this.CbWiaDelayBetweenScans.UseVisualStyleBackColor = true;
            this.CbWiaDelayBetweenScans.CheckedChanged += new System.EventHandler(this.CbWiaDelayBetweenScans_CheckedChanged);
            // 
            // CbWiaRetryOnFailure
            // 
            resources.ApplyResources(this.CbWiaRetryOnFailure, "CbWiaRetryOnFailure");
            this.CbWiaRetryOnFailure.Name = "CbWiaRetryOnFailure";
            this.CbWiaRetryOnFailure.UseVisualStyleBackColor = true;
            // 
            // CbForcePageSizeCrop
            // 
            resources.ApplyResources(this.CbForcePageSizeCrop, "CbForcePageSizeCrop");
            this.CbForcePageSizeCrop.Name = "CbForcePageSizeCrop";
            this.CbForcePageSizeCrop.UseVisualStyleBackColor = true;
            // 
            // cbFlipDuplex
            // 
            resources.ApplyResources(this.cbFlipDuplex, "cbFlipDuplex");
            this.cbFlipDuplex.Name = "cbFlipDuplex";
            this.cbFlipDuplex.UseVisualStyleBackColor = true;
            // 
            // CbWiaOffsetWidth
            // 
            resources.ApplyResources(this.CbWiaOffsetWidth, "CbWiaOffsetWidth");
            this.CbWiaOffsetWidth.Name = "CbWiaOffsetWidth";
            this.CbWiaOffsetWidth.UseVisualStyleBackColor = true;
            // 
            // CbForcePageSize
            // 
            resources.ApplyResources(this.CbForcePageSize, "CbForcePageSize");
            this.CbForcePageSize.Name = "CbForcePageSize";
            this.CbForcePageSize.UseVisualStyleBackColor = true;
            // 
            // cbBrightnessContrastAfterScan
            // 
            resources.ApplyResources(this.cbBrightnessContrastAfterScan, "cbBrightnessContrastAfterScan");
            this.cbBrightnessContrastAfterScan.Name = "cbBrightnessContrastAfterScan";
            this.cbBrightnessContrastAfterScan.UseVisualStyleBackColor = true;
            // 
            // cmbTwainImpl
            // 
            this.cmbTwainImpl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTwainImpl.FormattingEnabled = true;
            resources.ApplyResources(this.cmbTwainImpl, "cmbTwainImpl");
            this.cmbTwainImpl.Name = "cmbTwainImpl";
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // GroupBox3
            // 
            this.GroupBox3.Controls.Add(this.Label3);
            this.GroupBox3.Controls.Add(this.Label2);
            this.GroupBox3.Controls.Add(this.TbCoverageThreshold);
            this.GroupBox3.Controls.Add(this.TxtCoverageThreshold);
            this.GroupBox3.Controls.Add(this.TbWhiteThreshold);
            this.GroupBox3.Controls.Add(this.TxtWhiteThreshold);
            this.GroupBox3.Controls.Add(this.CbExcludeBlankPages);
            resources.ApplyResources(this.GroupBox3, "GroupBox3");
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.TabStop = false;
            // 
            // Label3
            // 
            resources.ApplyResources(this.Label3, "Label3");
            this.Label3.Name = "Label3";
            // 
            // Label2
            // 
            resources.ApplyResources(this.Label2, "Label2");
            this.Label2.Name = "Label2";
            // 
            // TbCoverageThreshold
            // 
            resources.ApplyResources(this.TbCoverageThreshold, "TbCoverageThreshold");
            this.TbCoverageThreshold.Maximum = 100;
            this.TbCoverageThreshold.Name = "TbCoverageThreshold";
            this.TbCoverageThreshold.TickFrequency = 25;
            this.TbCoverageThreshold.Scroll += new System.EventHandler(this.TbCoverageThreshold_Scroll);
            // 
            // TxtCoverageThreshold
            // 
            resources.ApplyResources(this.TxtCoverageThreshold, "TxtCoverageThreshold");
            this.TxtCoverageThreshold.Name = "TxtCoverageThreshold";
            this.TxtCoverageThreshold.TextChanged += new System.EventHandler(this.TxtCoverageThreshold_TextChanged);
            // 
            // TbWhiteThreshold
            // 
            resources.ApplyResources(this.TbWhiteThreshold, "TbWhiteThreshold");
            this.TbWhiteThreshold.Maximum = 100;
            this.TbWhiteThreshold.Name = "TbWhiteThreshold";
            this.TbWhiteThreshold.TickFrequency = 25;
            this.TbWhiteThreshold.Scroll += new System.EventHandler(this.TbWhiteThreshold_Scroll);
            // 
            // TxtWhiteThreshold
            // 
            resources.ApplyResources(this.TxtWhiteThreshold, "TxtWhiteThreshold");
            this.TxtWhiteThreshold.Name = "TxtWhiteThreshold";
            this.TxtWhiteThreshold.TextChanged += new System.EventHandler(this.TxtWhiteThreshold_TextChanged);
            // 
            // CbExcludeBlankPages
            // 
            resources.ApplyResources(this.CbExcludeBlankPages, "CbExcludeBlankPages");
            this.CbExcludeBlankPages.Name = "CbExcludeBlankPages";
            this.CbExcludeBlankPages.UseVisualStyleBackColor = true;
            this.CbExcludeBlankPages.CheckedChanged += new System.EventHandler(this.CbExcludeBlankPages_CheckedChanged);
            // 
            // BtnRestoreDefaults
            // 
            resources.ApplyResources(this.BtnRestoreDefaults, "BtnRestoreDefaults");
            this.BtnRestoreDefaults.Name = "BtnRestoreDefaults";
            this.BtnRestoreDefaults.UseVisualStyleBackColor = true;
            this.BtnRestoreDefaults.Click += new System.EventHandler(this.BtnRestoreDefaults_Click);
            // 
            // GroupBox4
            // 
            this.GroupBox4.Controls.Add(this.CbAutoDeskew);
            resources.ApplyResources(this.GroupBox4, "GroupBox4");
            this.GroupBox4.Name = "GroupBox4";
            this.GroupBox4.TabStop = false;
            // 
            // CbAutoDeskew
            // 
            resources.ApplyResources(this.CbAutoDeskew, "CbAutoDeskew");
            this.CbAutoDeskew.Name = "CbAutoDeskew";
            this.CbAutoDeskew.UseVisualStyleBackColor = true;
            // 
            // FAdvancedScanSettings
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GroupBox4);
            this.Controls.Add(this.BtnRestoreDefaults);
            this.Controls.Add(this.GroupBox3);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FAdvancedScanSettings";
            ((System.ComponentModel.ISupportInitialize)(this.tbImageQuality)).EndInit();
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.GroupBox3.ResumeLayout(false);
            this.GroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TbCoverageThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbWhiteThreshold)).EndInit();
            this.GroupBox4.ResumeLayout(false);
            this.GroupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.CheckBox CbHighQuality;
        private System.Windows.Forms.TextBox txtImageQuality;
        private System.Windows.Forms.TrackBar tbImageQuality;
        private System.Windows.Forms.GroupBox GroupBox1;
        private System.Windows.Forms.GroupBox GroupBox2;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.ComboBox cmbTwainImpl;
        private System.Windows.Forms.CheckBox cbBrightnessContrastAfterScan;
        private System.Windows.Forms.CheckBox CbForcePageSize;
        private System.Windows.Forms.GroupBox GroupBox3;
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.TrackBar TbCoverageThreshold;
        private System.Windows.Forms.TextBox TxtCoverageThreshold;
        private System.Windows.Forms.TrackBar TbWhiteThreshold;
        private System.Windows.Forms.TextBox TxtWhiteThreshold;
        private System.Windows.Forms.CheckBox CbExcludeBlankPages;
        private System.Windows.Forms.CheckBox CbWiaOffsetWidth;
        private System.Windows.Forms.CheckBox cbFlipDuplex;
        private System.Windows.Forms.Button BtnRestoreDefaults;
        private System.Windows.Forms.CheckBox CbForcePageSizeCrop;
        private System.Windows.Forms.CheckBox CbWiaDelayBetweenScans;
        private System.Windows.Forms.CheckBox CbWiaRetryOnFailure;
        private System.Windows.Forms.TextBox txtWiaDelayBetweenScansSeconds;
        private System.Windows.Forms.GroupBox GroupBox4;
        private System.Windows.Forms.CheckBox CbAutoDeskew;
    }
}
