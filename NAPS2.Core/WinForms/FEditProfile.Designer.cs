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
            this.TxtDevice = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.BtnChooseDevice = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.cmbSource = new System.Windows.Forms.ComboBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.RdbConfig = new System.Windows.Forms.RadioButton();
            this.rdbNative = new System.Windows.Forms.RadioButton();
            this.Label3 = new System.Windows.Forms.Label();
            this.cmbDepth = new System.Windows.Forms.ComboBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.CmbPage = new System.Windows.Forms.ComboBox();
            this.cmbResolution = new System.Windows.Forms.ComboBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.TrBrightness = new System.Windows.Forms.TrackBar();
            this.Label7 = new System.Windows.Forms.Label();
            this.TrContrast = new System.Windows.Forms.TrackBar();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.pctIcon = new System.Windows.Forms.PictureBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cmbAlign = new System.Windows.Forms.ComboBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.cmbScale = new System.Windows.Forms.ComboBox();
            this.Label10 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rdTWAIN = new System.Windows.Forms.RadioButton();
            this.RdWIA = new System.Windows.Forms.RadioButton();
            this.TxtBrightness = new System.Windows.Forms.TextBox();
            this.TxtContrast = new System.Windows.Forms.TextBox();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.CbAutoSave = new System.Windows.Forms.CheckBox();
            this.LinkAutoSaveSettings = new System.Windows.Forms.LinkLabel();
            this.BtnAdvanced = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrContrast)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TxtDevice
            // 
            resources.ApplyResources(this.TxtDevice, "TxtDevice");
            this.TxtDevice.Name = "TxtDevice";
            this.TxtDevice.ReadOnly = true;
            this.TxtDevice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtDevice_KeyDown);
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // BtnChooseDevice
            // 
            resources.ApplyResources(this.BtnChooseDevice, "BtnChooseDevice");
            this.BtnChooseDevice.Name = "BtnChooseDevice";
            this.BtnChooseDevice.UseVisualStyleBackColor = true;
            this.BtnChooseDevice.Click += new System.EventHandler(this.BtnChooseDevice_Click);
            // 
            // Label2
            // 
            resources.ApplyResources(this.Label2, "Label2");
            this.Label2.Name = "Label2";
            // 
            // cmbSource
            // 
            this.cmbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSource.FormattingEnabled = true;
            resources.ApplyResources(this.cmbSource, "cmbSource");
            this.cmbSource.Name = "cmbSource";
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.RdbConfig);
            this.panel1.Controls.Add(this.rdbNative);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // RdbConfig
            // 
            resources.ApplyResources(this.RdbConfig, "RdbConfig");
            this.RdbConfig.Name = "RdbConfig";
            this.RdbConfig.TabStop = true;
            this.RdbConfig.UseVisualStyleBackColor = true;
            this.RdbConfig.CheckedChanged += new System.EventHandler(this.RdbConfig_CheckedChanged);
            // 
            // rdbNative
            // 
            resources.ApplyResources(this.rdbNative, "rdbNative");
            this.rdbNative.Name = "rdbNative";
            this.rdbNative.TabStop = true;
            this.rdbNative.UseVisualStyleBackColor = true;
            this.rdbNative.CheckedChanged += new System.EventHandler(this.RdbNativeWIA_CheckedChanged);
            // 
            // Label3
            // 
            resources.ApplyResources(this.Label3, "Label3");
            this.Label3.Name = "Label3";
            // 
            // cmbDepth
            // 
            this.cmbDepth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbDepth, "cmbDepth");
            this.cmbDepth.FormattingEnabled = true;
            this.cmbDepth.Name = "cmbDepth";
            // 
            // Label4
            // 
            resources.ApplyResources(this.Label4, "Label4");
            this.Label4.Name = "Label4";
            // 
            // CmbPage
            // 
            this.CmbPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CmbPage, "CmbPage");
            this.CmbPage.FormattingEnabled = true;
            this.CmbPage.Name = "CmbPage";
            this.CmbPage.SelectedIndexChanged += new System.EventHandler(this.CmbPage_SelectedIndexChanged);
            // 
            // cmbResolution
            // 
            this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbResolution, "cmbResolution");
            this.cmbResolution.FormattingEnabled = true;
            this.cmbResolution.Name = "cmbResolution";
            // 
            // Label5
            // 
            resources.ApplyResources(this.Label5, "Label5");
            this.Label5.Name = "Label5";
            // 
            // Label6
            // 
            resources.ApplyResources(this.Label6, "Label6");
            this.Label6.Name = "Label6";
            // 
            // TrBrightness
            // 
            resources.ApplyResources(this.TrBrightness, "TrBrightness");
            this.TrBrightness.Maximum = 1000;
            this.TrBrightness.Minimum = -1000;
            this.TrBrightness.Name = "TrBrightness";
            this.TrBrightness.TickFrequency = 200;
            this.TrBrightness.Scroll += new System.EventHandler(this.TrBrightness_Scroll);
            // 
            // Label7
            // 
            resources.ApplyResources(this.Label7, "Label7");
            this.Label7.Name = "Label7";
            // 
            // TrContrast
            // 
            resources.ApplyResources(this.TrContrast, "TrContrast");
            this.TrContrast.Maximum = 1000;
            this.TrContrast.Minimum = -1000;
            this.TrContrast.Name = "TrContrast";
            this.TrContrast.TickFrequency = 200;
            this.TrContrast.Scroll += new System.EventHandler(this.TrContrast_Scroll);
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pctIcon
            // 
            resources.ApplyResources(this.pctIcon, "pctIcon");
            this.pctIcon.Name = "pctIcon";
            this.pctIcon.TabStop = false;
            // 
            // Label8
            // 
            resources.ApplyResources(this.Label8, "Label8");
            this.Label8.Name = "Label8";
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
            // Label9
            // 
            resources.ApplyResources(this.Label9, "Label9");
            this.Label9.Name = "Label9";
            // 
            // cmbScale
            // 
            this.cmbScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cmbScale, "cmbScale");
            this.cmbScale.FormattingEnabled = true;
            this.cmbScale.Name = "cmbScale";
            // 
            // Label10
            // 
            resources.ApplyResources(this.Label10, "Label10");
            this.Label10.Name = "Label10";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rdTWAIN);
            this.panel2.Controls.Add(this.RdWIA);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // rdTWAIN
            // 
            resources.ApplyResources(this.rdTWAIN, "rdTWAIN");
            this.rdTWAIN.Name = "rdTWAIN";
            this.rdTWAIN.TabStop = true;
            this.rdTWAIN.UseVisualStyleBackColor = true;
            // 
            // RdWIA
            // 
            resources.ApplyResources(this.RdWIA, "RdWIA");
            this.RdWIA.Name = "RdWIA";
            this.RdWIA.TabStop = true;
            this.RdWIA.UseVisualStyleBackColor = true;
            this.RdWIA.CheckedChanged += new System.EventHandler(this.RdWIA_CheckedChanged);
            // 
            // TxtBrightness
            // 
            resources.ApplyResources(this.TxtBrightness, "TxtBrightness");
            this.TxtBrightness.Name = "TxtBrightness";
            this.TxtBrightness.TextChanged += new System.EventHandler(this.TxtBrightness_TextChanged);
            // 
            // TxtContrast
            // 
            resources.ApplyResources(this.TxtContrast, "TxtContrast");
            this.TxtContrast.Name = "TxtContrast";
            this.TxtContrast.TextChanged += new System.EventHandler(this.TxtContrast_TextChanged);
            // 
            // CbAutoSave
            // 
            resources.ApplyResources(this.CbAutoSave, "CbAutoSave");
            this.CbAutoSave.Name = "CbAutoSave";
            this.CbAutoSave.UseVisualStyleBackColor = true;
            this.CbAutoSave.CheckedChanged += new System.EventHandler(this.CbAutoSave_CheckedChanged);
            // 
            // LinkAutoSaveSettings
            // 
            resources.ApplyResources(this.LinkAutoSaveSettings, "LinkAutoSaveSettings");
            this.LinkAutoSaveSettings.Name = "LinkAutoSaveSettings";
            this.LinkAutoSaveSettings.TabStop = true;
            this.LinkAutoSaveSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkAutoSaveSettings_LinkClicked);
            // 
            // BtnAdvanced
            // 
            resources.ApplyResources(this.BtnAdvanced, "BtnAdvanced");
            this.BtnAdvanced.Name = "BtnAdvanced";
            this.BtnAdvanced.UseVisualStyleBackColor = true;
            this.BtnAdvanced.Click += new System.EventHandler(this.BtnAdvanced_Click);
            // 
            // FEditProfile
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnAdvanced);
            this.Controls.Add(this.LinkAutoSaveSettings);
            this.Controls.Add(this.CbAutoSave);
            this.Controls.Add(this.TxtContrast);
            this.Controls.Add(this.TxtBrightness);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.cmbScale);
            this.Controls.Add(this.Label10);
            this.Controls.Add(this.cmbAlign);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.pctIcon);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.TrContrast);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.TrBrightness);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.cmbResolution);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.CmbPage);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.cmbDepth);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.cmbSource);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.BtnChooseDevice);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.TxtDevice);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEditProfile";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TrBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrContrast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtDevice;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Button BtnChooseDevice;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.ComboBox cmbSource;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton RdbConfig;
        private System.Windows.Forms.RadioButton rdbNative;
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.ComboBox cmbDepth;
        private System.Windows.Forms.Label Label4;
        private System.Windows.Forms.ComboBox CmbPage;
        private System.Windows.Forms.ComboBox cmbResolution;
        private System.Windows.Forms.Label Label5;
        private System.Windows.Forms.Label Label6;
        private System.Windows.Forms.TrackBar TrBrightness;
        private System.Windows.Forms.Label Label7;
        private System.Windows.Forms.TrackBar TrContrast;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.PictureBox pctIcon;
        private System.Windows.Forms.Label Label8;
        private System.Windows.Forms.TextBox txtName;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.ComboBox cmbAlign;
        private System.Windows.Forms.Label Label9;
        private System.Windows.Forms.ComboBox cmbScale;
        private System.Windows.Forms.Label Label10;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rdTWAIN;
        private System.Windows.Forms.RadioButton RdWIA;
        private System.Windows.Forms.TextBox TxtBrightness;
        private System.Windows.Forms.TextBox TxtContrast;
        private System.Windows.Forms.CheckBox CbAutoSave;
        private System.Windows.Forms.LinkLabel LinkAutoSaveSettings;
        private System.Windows.Forms.Button BtnAdvanced;
    }
}
