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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbHighQuality = new System.Windows.Forms.CheckBox();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.txtImageQuality = new System.Windows.Forms.TextBox();
            this.tbImageQuality = new System.Windows.Forms.TrackBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbForcePageSizeCrop = new System.Windows.Forms.CheckBox();
            this.cbFlipDuplex = new System.Windows.Forms.CheckBox();
            this.cbWiaOffsetWidth = new System.Windows.Forms.CheckBox();
            this.cbForcePageSize = new System.Windows.Forms.CheckBox();
            this.cbBrightnessContrastAfterScan = new System.Windows.Forms.CheckBox();
            this.cmbTwainImpl = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbCoverageThreshold = new System.Windows.Forms.TrackBar();
            this.txtCoverageThreshold = new System.Windows.Forms.TextBox();
            this.tbWhiteThreshold = new System.Windows.Forms.TrackBar();
            this.txtWhiteThreshold = new System.Windows.Forms.TextBox();
            this.cbExcludeBlankPages = new System.Windows.Forms.CheckBox();
            this.btnRestoreDefaults = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbAutoDeskew = new System.Windows.Forms.CheckBox();
            this.cmbWiaVersion = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tbImageQuality)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCoverageThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbWhiteThreshold)).BeginInit();
            this.groupBox4.SuspendLayout();
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
            // cbHighQuality
            // 
            resources.ApplyResources(this.cbHighQuality, "cbHighQuality");
            this.cbHighQuality.Name = "cbHighQuality";
            this.cbHighQuality.UseVisualStyleBackColor = true;
            this.cbHighQuality.CheckedChanged += new System.EventHandler(this.cbHighQuality_CheckedChanged);
            // 
            // txtImageQuality
            // 
            resources.ApplyResources(this.txtImageQuality, "txtImageQuality");
            this.txtImageQuality.Name = "txtImageQuality";
            this.txtImageQuality.TextChanged += new System.EventHandler(this.txtImageQuality_TextChanged);
            // 
            // tbImageQuality
            // 
            resources.ApplyResources(this.tbImageQuality, "tbImageQuality");
            this.tbImageQuality.Maximum = 100;
            this.tbImageQuality.Name = "tbImageQuality";
            this.tbImageQuality.TickFrequency = 25;
            this.tbImageQuality.Scroll += new System.EventHandler(this.tbImageQuality_Scroll);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbImageQuality);
            this.groupBox1.Controls.Add(this.txtImageQuality);
            this.groupBox1.Controls.Add(this.cbHighQuality);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cmbWiaVersion);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cbForcePageSizeCrop);
            this.groupBox2.Controls.Add(this.cbFlipDuplex);
            this.groupBox2.Controls.Add(this.cbWiaOffsetWidth);
            this.groupBox2.Controls.Add(this.cbForcePageSize);
            this.groupBox2.Controls.Add(this.cbBrightnessContrastAfterScan);
            this.groupBox2.Controls.Add(this.cmbTwainImpl);
            this.groupBox2.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // cbForcePageSizeCrop
            // 
            resources.ApplyResources(this.cbForcePageSizeCrop, "cbForcePageSizeCrop");
            this.cbForcePageSizeCrop.Name = "cbForcePageSizeCrop";
            this.cbForcePageSizeCrop.UseVisualStyleBackColor = true;
            // 
            // cbFlipDuplex
            // 
            resources.ApplyResources(this.cbFlipDuplex, "cbFlipDuplex");
            this.cbFlipDuplex.Name = "cbFlipDuplex";
            this.cbFlipDuplex.UseVisualStyleBackColor = true;
            // 
            // cbWiaOffsetWidth
            // 
            resources.ApplyResources(this.cbWiaOffsetWidth, "cbWiaOffsetWidth");
            this.cbWiaOffsetWidth.Name = "cbWiaOffsetWidth";
            this.cbWiaOffsetWidth.UseVisualStyleBackColor = true;
            // 
            // cbForcePageSize
            // 
            resources.ApplyResources(this.cbForcePageSize, "cbForcePageSize");
            this.cbForcePageSize.Name = "cbForcePageSize";
            this.cbForcePageSize.UseVisualStyleBackColor = true;
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
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.tbCoverageThreshold);
            this.groupBox3.Controls.Add(this.txtCoverageThreshold);
            this.groupBox3.Controls.Add(this.tbWhiteThreshold);
            this.groupBox3.Controls.Add(this.txtWhiteThreshold);
            this.groupBox3.Controls.Add(this.cbExcludeBlankPages);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // tbCoverageThreshold
            // 
            resources.ApplyResources(this.tbCoverageThreshold, "tbCoverageThreshold");
            this.tbCoverageThreshold.Maximum = 100;
            this.tbCoverageThreshold.Name = "tbCoverageThreshold";
            this.tbCoverageThreshold.TickFrequency = 25;
            this.tbCoverageThreshold.Scroll += new System.EventHandler(this.tbCoverageThreshold_Scroll);
            // 
            // txtCoverageThreshold
            // 
            resources.ApplyResources(this.txtCoverageThreshold, "txtCoverageThreshold");
            this.txtCoverageThreshold.Name = "txtCoverageThreshold";
            this.txtCoverageThreshold.TextChanged += new System.EventHandler(this.txtCoverageThreshold_TextChanged);
            // 
            // tbWhiteThreshold
            // 
            resources.ApplyResources(this.tbWhiteThreshold, "tbWhiteThreshold");
            this.tbWhiteThreshold.Maximum = 100;
            this.tbWhiteThreshold.Name = "tbWhiteThreshold";
            this.tbWhiteThreshold.TickFrequency = 25;
            this.tbWhiteThreshold.Scroll += new System.EventHandler(this.tbWhiteThreshold_Scroll);
            // 
            // txtWhiteThreshold
            // 
            resources.ApplyResources(this.txtWhiteThreshold, "txtWhiteThreshold");
            this.txtWhiteThreshold.Name = "txtWhiteThreshold";
            this.txtWhiteThreshold.TextChanged += new System.EventHandler(this.txtWhiteThreshold_TextChanged);
            // 
            // cbExcludeBlankPages
            // 
            resources.ApplyResources(this.cbExcludeBlankPages, "cbExcludeBlankPages");
            this.cbExcludeBlankPages.Name = "cbExcludeBlankPages";
            this.cbExcludeBlankPages.UseVisualStyleBackColor = true;
            this.cbExcludeBlankPages.CheckedChanged += new System.EventHandler(this.cbExcludeBlankPages_CheckedChanged);
            // 
            // btnRestoreDefaults
            // 
            resources.ApplyResources(this.btnRestoreDefaults, "btnRestoreDefaults");
            this.btnRestoreDefaults.Name = "btnRestoreDefaults";
            this.btnRestoreDefaults.UseVisualStyleBackColor = true;
            this.btnRestoreDefaults.Click += new System.EventHandler(this.btnRestoreDefaults_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbAutoDeskew);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // cbAutoDeskew
            // 
            resources.ApplyResources(this.cbAutoDeskew, "cbAutoDeskew");
            this.cbAutoDeskew.Name = "cbAutoDeskew";
            this.cbAutoDeskew.UseVisualStyleBackColor = true;
            // 
            // cmbWiaVersion
            // 
            this.cmbWiaVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWiaVersion.FormattingEnabled = true;
            resources.ApplyResources(this.cmbWiaVersion, "cmbWiaVersion");
            this.cmbWiaVersion.Name = "cmbWiaVersion";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // FAdvancedScanSettings
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.btnRestoreDefaults);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FAdvancedScanSettings";
            ((System.ComponentModel.ISupportInitialize)(this.tbImageQuality)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCoverageThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbWhiteThreshold)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.CheckBox cbHighQuality;
        private System.Windows.Forms.TextBox txtImageQuality;
        private System.Windows.Forms.TrackBar tbImageQuality;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbTwainImpl;
        private System.Windows.Forms.CheckBox cbBrightnessContrastAfterScan;
        private System.Windows.Forms.CheckBox cbForcePageSize;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar tbCoverageThreshold;
        private System.Windows.Forms.TextBox txtCoverageThreshold;
        private System.Windows.Forms.TrackBar tbWhiteThreshold;
        private System.Windows.Forms.TextBox txtWhiteThreshold;
        private System.Windows.Forms.CheckBox cbExcludeBlankPages;
        private System.Windows.Forms.CheckBox cbWiaOffsetWidth;
        private System.Windows.Forms.CheckBox cbFlipDuplex;
        private System.Windows.Forms.Button btnRestoreDefaults;
        private System.Windows.Forms.CheckBox cbForcePageSizeCrop;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox cbAutoDeskew;
        private System.Windows.Forms.ComboBox cmbWiaVersion;
        private System.Windows.Forms.Label label4;
    }
}
