using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAbout));
            this.LabelProductName = new System.Windows.Forms.Label();
            this.LabelVersion = new System.Windows.Forms.Label();
            this.LabelCopyright = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.LinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.Label1 = new System.Windows.Forms.Label();
            this.LinkLabel2 = new System.Windows.Forms.LinkLabel();
            this.BtnDonate = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnDonate)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelProductName
            // 
            resources.ApplyResources(this.LabelProductName, "LabelProductName");
            this.LabelProductName.Name = "LabelProductName";
            // 
            // LabelVersion
            // 
            resources.ApplyResources(this.LabelVersion, "LabelVersion");
            this.LabelVersion.Name = "LabelVersion";
            // 
            // LabelCopyright
            // 
            resources.ApplyResources(this.LabelCopyright, "LabelCopyright");
            this.LabelCopyright.Name = "LabelCopyright";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Name = "okButton";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // LinkLabel1
            // 
            resources.ApplyResources(this.LinkLabel1, "LinkLabel1");
            this.LinkLabel1.Name = "LinkLabel1";
            this.LinkLabel1.TabStop = true;
            this.LinkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // LinkLabel2
            // 
            resources.ApplyResources(this.LinkLabel2, "LinkLabel2");
            this.LinkLabel2.Name = "LinkLabel2";
            this.LinkLabel2.TabStop = true;
            this.LinkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2_LinkClicked);
            // 
            // BtnDonate
            // 
            this.BtnDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnDonate.Image = global::NAPS2.Icons.Btn_donate_LG;
            resources.ApplyResources(this.BtnDonate, "BtnDonate");
            this.BtnDonate.Name = "BtnDonate";
            this.BtnDonate.TabStop = false;
            this.BtnDonate.Click += new System.EventHandler(this.BtnDonate_Click);
            // 
            // FAbout
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnDonate);
            this.Controls.Add(this.LinkLabel2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.LinkLabel1);
            this.Controls.Add(this.logoPictureBox);
            this.Controls.Add(this.LabelProductName);
            this.Controls.Add(this.LabelVersion);
            this.Controls.Add(this.LabelCopyright);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FAbout";
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnDonate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelProductName;
        private System.Windows.Forms.Label LabelVersion;
        private System.Windows.Forms.Label LabelCopyright;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.LinkLabel LinkLabel1;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.LinkLabel LinkLabel2;
        private System.Windows.Forms.PictureBox BtnDonate;
    }
}
