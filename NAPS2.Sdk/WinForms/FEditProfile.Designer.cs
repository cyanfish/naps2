using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FEditProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FEditProfile));
            this.txtDevice = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChooseDevice = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbSource = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.panelUI = new System.Windows.Forms.Panel();
            this.rdbConfig = new System.Windows.Forms.RadioButton();
            this.rdbNative = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbDepth = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbPage = new System.Windows.Forms.ComboBox();
            this.cmbResolution = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.trBrightness = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.trContrast = new System.Windows.Forms.TrackBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pctIcon = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cmbAlign = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbScale = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rdTWAIN = new System.Windows.Forms.RadioButton();
            this.rdWIA = new System.Windows.Forms.RadioButton();
            this.rdSANE = new System.Windows.Forms.RadioButton();
            this.txtBrightness = new System.Windows.Forms.TextBox();
            this.txtContrast = new System.Windows.Forms.TextBox();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.cbAutoSave = new System.Windows.Forms.CheckBox();
            this.linkAutoSaveSettings = new System.Windows.Forms.LinkLabel();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.btnNetwork = new System.Windows.Forms.Button();
            this.panelUI.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trContrast)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDevice
            // 
            resources.ApplyResources(this.txtDevice, "txtDevice");
            this.txtDevice.Name = "txtDevice";
            this.txtDevice.ReadOnly = true;
            this.txtDevice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDevice_KeyDown);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnChooseDevice
            // 
            resources.ApplyResources(this.btnChooseDevice, "btnChooseDevice");
            this.btnChooseDevice.Name = "btnChooseDevice";
            this.btnChooseDevice.UseVisualStyleBackColor = true;
            this.btnChooseDevice.Click += new System.EventHandler(this.btnChooseDevice_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // cmbSource
            // 
            this.cmbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSource.FormattingEnabled = true;
            resources.ApplyResources(this.cmbSource, "cmbSource");
            this.cmbSource.Name = "cmbSource";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panelUI
            // 
            this.panelUI.Controls.Add(this.rdbConfig);
            this.panelUI.Controls.Add(this.rdbNative);
            resources.ApplyResources(this.panelUI, "panelUI");
            this.panelUI.Name = "panelUI";
            // 
            // rdbConfig
            // 
            resources.ApplyResources(this.rdbConfig, "rdbConfig");
            this.rdbConfig.Name = "rdbConfig";
            this.rdbConfig.TabStop = true;
            this.rdbConfig.UseVisualStyleBackColor = true;
            this.rdbConfig.CheckedChanged += new System.EventHandler(this.rdbConfig_CheckedChanged);
            // 
            // rdbNative
            // 
            resources.ApplyResources(this.rdbNative, "rdbNative");
            this.rdbNative.Name = "rdbNative";
            this.rdbNative.TabStop = true;
            this.rdbNative.UseVisualStyleBackColor = true;
            this.rdbNative.CheckedChanged += new System.EventHandler(this.rdbNativeWIA_CheckedChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbDepth
            // 
            this.cmbDepth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbDepth, "cmbDepth");
            this.cmbDepth.FormattingEnabled = true;
            this.cmbDepth.Name = "cmbDepth";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cmbPage
            // 
            this.cmbPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbPage, "cmbPage");
            this.cmbPage.FormattingEnabled = true;
            this.cmbPage.Name = "cmbPage";
            this.cmbPage.SelectedIndexChanged += new System.EventHandler(this.cmbPage_SelectedIndexChanged);
            // 
            // cmbResolution
            // 
            this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbResolution, "cmbResolution");
            this.cmbResolution.FormattingEnabled = true;
            this.cmbResolution.Name = "cmbResolution";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // trBrightness
            // 
            resources.ApplyResources(this.trBrightness, "trBrightness");
            this.trBrightness.Maximum = 1000;
            this.trBrightness.Minimum = -1000;
            this.trBrightness.Name = "trBrightness";
            this.trBrightness.TickFrequency = 200;
            this.trBrightness.Scroll += new System.EventHandler(this.trBrightness_Scroll);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // trContrast
            // 
            resources.ApplyResources(this.trContrast, "trContrast");
            this.trContrast.Maximum = 1000;
            this.trContrast.Minimum = -1000;
            this.trContrast.Name = "trContrast";
            this.trContrast.TickFrequency = 200;
            this.trContrast.Scroll += new System.EventHandler(this.trContrast_Scroll);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pctIcon
            // 
            resources.ApplyResources(this.pctIcon, "pctIcon");
            this.pctIcon.Name = "pctIcon";
            this.pctIcon.TabStop = false;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            // 
            // cmbAlign
            // 
            this.cmbAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbAlign, "cmbAlign");
            this.cmbAlign.FormattingEnabled = true;
            this.cmbAlign.Name = "cmbAlign";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // cmbScale
            // 
            this.cmbScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbScale, "cmbScale");
            this.cmbScale.FormattingEnabled = true;
            this.cmbScale.Name = "cmbScale";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rdTWAIN);
            this.panel2.Controls.Add(this.rdWIA);
            this.panel2.Controls.Add(this.rdSANE);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // rdTWAIN
            // 
            resources.ApplyResources(this.rdTWAIN, "rdTWAIN");
            this.rdTWAIN.Name = "rdTWAIN";
            this.rdTWAIN.TabStop = true;
            this.rdTWAIN.UseVisualStyleBackColor = true;
            this.rdTWAIN.CheckedChanged += new System.EventHandler(this.rdDriver_CheckedChanged);
            // 
            // rdWIA
            // 
            resources.ApplyResources(this.rdWIA, "rdWIA");
            this.rdWIA.Name = "rdWIA";
            this.rdWIA.TabStop = true;
            this.rdWIA.UseVisualStyleBackColor = true;
            this.rdWIA.CheckedChanged += new System.EventHandler(this.rdDriver_CheckedChanged);
            // 
            // rdSANE
            // 
            resources.ApplyResources(this.rdSANE, "rdSANE");
            this.rdSANE.Name = "rdSANE";
            this.rdSANE.TabStop = true;
            this.rdSANE.UseVisualStyleBackColor = true;
            this.rdSANE.CheckedChanged += new System.EventHandler(this.rdDriver_CheckedChanged);
            // 
            // txtBrightness
            // 
            resources.ApplyResources(this.txtBrightness, "txtBrightness");
            this.txtBrightness.Name = "txtBrightness";
            this.txtBrightness.TextChanged += new System.EventHandler(this.txtBrightness_TextChanged);
            // 
            // txtContrast
            // 
            resources.ApplyResources(this.txtContrast, "txtContrast");
            this.txtContrast.Name = "txtContrast";
            this.txtContrast.TextChanged += new System.EventHandler(this.txtContrast_TextChanged);
            // 
            // cbAutoSave
            // 
            resources.ApplyResources(this.cbAutoSave, "cbAutoSave");
            this.cbAutoSave.Name = "cbAutoSave";
            this.cbAutoSave.UseVisualStyleBackColor = true;
            this.cbAutoSave.CheckedChanged += new System.EventHandler(this.cbAutoSave_CheckedChanged);
            // 
            // linkAutoSaveSettings
            // 
            resources.ApplyResources(this.linkAutoSaveSettings, "linkAutoSaveSettings");
            this.linkAutoSaveSettings.Name = "linkAutoSaveSettings";
            this.linkAutoSaveSettings.TabStop = true;
            this.linkAutoSaveSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAutoSaveSettings_LinkClicked);
            // 
            // btnAdvanced
            // 
            resources.ApplyResources(this.btnAdvanced, "btnAdvanced");
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // btnNetwork
            // 
            resources.ApplyResources(this.btnNetwork, "btnNetwork");
            this.btnNetwork.Image = global::NAPS2.Icons.wireless16;
            this.btnNetwork.Name = "btnNetwork";
            this.btnNetwork.UseVisualStyleBackColor = true;
            this.btnNetwork.Click += new System.EventHandler(this.btnNetwork_Click);
            // 
            // FEditProfile
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnNetwork);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.linkAutoSaveSettings);
            this.Controls.Add(this.cbAutoSave);
            this.Controls.Add(this.txtContrast);
            this.Controls.Add(this.txtBrightness);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.cmbScale);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cmbAlign);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pctIcon);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.trContrast);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.trBrightness);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbResolution);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbPage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbDepth);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panelUI);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbSource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnChooseDevice);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDevice);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEditProfile";
            this.panelUI.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trContrast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDevice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnChooseDevice;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbSource;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panelUI;
        private System.Windows.Forms.RadioButton rdbConfig;
        private System.Windows.Forms.RadioButton rdbNative;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbDepth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbPage;
        private System.Windows.Forms.ComboBox cmbResolution;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar trBrightness;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar trContrast;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox pctIcon;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtName;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.ComboBox cmbAlign;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbScale;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rdTWAIN;
        private System.Windows.Forms.RadioButton rdWIA;
        private System.Windows.Forms.TextBox txtBrightness;
        private System.Windows.Forms.TextBox txtContrast;
        private System.Windows.Forms.CheckBox cbAutoSave;
        private System.Windows.Forms.LinkLabel linkAutoSaveSettings;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.RadioButton rdSANE;
        private System.Windows.Forms.Button btnNetwork;
    }
}
