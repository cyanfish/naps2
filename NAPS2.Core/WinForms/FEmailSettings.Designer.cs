using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FEmailSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FEmailSettings));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.cbRememberSettings = new System.Windows.Forms.CheckBox();
            this.btnRestoreDefaults = new System.Windows.Forms.Button();
            this.txtAttachmentName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.linkPlaceholders = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblProvider = new System.Windows.Forms.Label();
            this.btnChangeProvider = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
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
            // txtAttachmentName
            // 
            resources.ApplyResources(this.txtAttachmentName, "txtAttachmentName");
            this.txtAttachmentName.Name = "txtAttachmentName";
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblProvider);
            this.groupBox1.Controls.Add(this.btnChangeProvider);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // lblProvider
            // 
            resources.ApplyResources(this.lblProvider, "lblProvider");
            this.lblProvider.Name = "lblProvider";
            // 
            // btnChangeProvider
            // 
            resources.ApplyResources(this.btnChangeProvider, "btnChangeProvider");
            this.btnChangeProvider.Name = "btnChangeProvider";
            this.btnChangeProvider.UseVisualStyleBackColor = true;
            this.btnChangeProvider.Click += new System.EventHandler(this.btnChangeProvider_Click);
            // 
            // FEmailSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.linkPlaceholders);
            this.Controls.Add(this.txtAttachmentName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRestoreDefaults);
            this.Controls.Add(this.cbRememberSettings);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEmailSettings";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.CheckBox cbRememberSettings;
        private System.Windows.Forms.Button btnRestoreDefaults;
        private System.Windows.Forms.TextBox txtAttachmentName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkPlaceholders;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnChangeProvider;
        private System.Windows.Forms.Label lblProvider;
    }
}
