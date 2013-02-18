/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

namespace NAPS2
{
    partial class FEditScanSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FEditScanSettings));
            this.txtDevice = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChooseDevice = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbSource = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdbConfig = new System.Windows.Forms.RadioButton();
            this.rdbNativeWIA = new System.Windows.Forms.RadioButton();
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
            this.cbHighQuality = new System.Windows.Forms.CheckBox();
            this.txtBrightness = new System.Windows.Forms.TextBox();
            this.txtContrast = new System.Windows.Forms.TextBox();
            this.ilProfileIcons = new NAPS2.ILProfileIcons(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trContrast)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDevice
            // 
            this.txtDevice.Location = new System.Drawing.Point(15, 98);
            this.txtDevice.Name = "txtDevice";
            this.txtDevice.ReadOnly = true;
            this.txtDevice.Size = new System.Drawing.Size(226, 20);
            this.txtDevice.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 82);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Device:";
            // 
            // btnChooseDevice
            // 
            this.btnChooseDevice.Location = new System.Drawing.Point(247, 95);
            this.btnChooseDevice.Name = "btnChooseDevice";
            this.btnChooseDevice.Size = new System.Drawing.Size(102, 23);
            this.btnChooseDevice.TabIndex = 2;
            this.btnChooseDevice.Text = "Choose device";
            this.btnChooseDevice.UseVisualStyleBackColor = true;
            this.btnChooseDevice.Click += new System.EventHandler(this.btnChooseDevice_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 198);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Paper source:";
            // 
            // cmbSource
            // 
            this.cmbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSource.FormattingEnabled = true;
            this.cmbSource.Items.AddRange(new object[] {
            "Glass",
            "Feeder",
            "Duplex"});
            this.cmbSource.Location = new System.Drawing.Point(15, 214);
            this.cmbSource.Name = "cmbSource";
            this.cmbSource.Size = new System.Drawing.Size(183, 21);
            this.cmbSource.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(262, 396);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "Ok";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdbConfig);
            this.panel1.Controls.Add(this.rdbNativeWIA);
            this.panel1.Location = new System.Drawing.Point(15, 140);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(334, 25);
            this.panel1.TabIndex = 8;
            // 
            // rdbConfig
            // 
            this.rdbConfig.AutoSize = true;
            this.rdbConfig.Location = new System.Drawing.Point(0, 3);
            this.rdbConfig.Name = "rdbConfig";
            this.rdbConfig.Size = new System.Drawing.Size(136, 17);
            this.rdbConfig.TabIndex = 1;
            this.rdbConfig.TabStop = true;
            this.rdbConfig.Text = "Use predefined settings";
            this.rdbConfig.UseVisualStyleBackColor = true;
            // 
            // rdbNativeWIA
            // 
            this.rdbNativeWIA.AutoSize = true;
            this.rdbNativeWIA.Location = new System.Drawing.Point(220, 3);
            this.rdbNativeWIA.Name = "rdbNativeWIA";
            this.rdbNativeWIA.Size = new System.Drawing.Size(114, 17);
            this.rdbNativeWIA.TabIndex = 0;
            this.rdbNativeWIA.TabStop = true;
            this.rdbNativeWIA.Text = "Use native WIA UI";
            this.rdbNativeWIA.UseVisualStyleBackColor = true;
            this.rdbNativeWIA.CheckedChanged += new System.EventHandler(this.rdbNativeWIA_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(232, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Bit depth:";
            // 
            // cmbDepth
            // 
            this.cmbDepth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDepth.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbDepth.FormattingEnabled = true;
            this.cmbDepth.Items.AddRange(new object[] {
            "24 bit Color",
            "Grayscale",
            "Black & White"});
            this.cmbDepth.Location = new System.Drawing.Point(235, 214);
            this.cmbDepth.Name = "cmbDepth";
            this.cmbDepth.Size = new System.Drawing.Size(183, 21);
            this.cmbDepth.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 238);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Page size:";
            // 
            // cmbPage
            // 
            this.cmbPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbPage.FormattingEnabled = true;
            this.cmbPage.Location = new System.Drawing.Point(15, 254);
            this.cmbPage.Name = "cmbPage";
            this.cmbPage.Size = new System.Drawing.Size(183, 21);
            this.cmbPage.TabIndex = 12;
            // 
            // cmbResolution
            // 
            this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResolution.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbResolution.FormattingEnabled = true;
            this.cmbResolution.Items.AddRange(new object[] {
            "100 dpi",
            "200 dpi",
            "300 dpi",
            "600 dpi",
            "1200 dpi"});
            this.cmbResolution.Location = new System.Drawing.Point(15, 294);
            this.cmbResolution.Name = "cmbResolution";
            this.cmbResolution.Size = new System.Drawing.Size(183, 21);
            this.cmbResolution.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 278);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Resolution:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 318);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Brightness:";
            // 
            // trBrightness
            // 
            this.trBrightness.Location = new System.Drawing.Point(15, 334);
            this.trBrightness.Maximum = 1000;
            this.trBrightness.Minimum = -1000;
            this.trBrightness.Name = "trBrightness";
            this.trBrightness.Size = new System.Drawing.Size(139, 45);
            this.trBrightness.TabIndex = 16;
            this.trBrightness.TickFrequency = 200;
            this.trBrightness.Scroll += new System.EventHandler(this.trBrightness_Scroll);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(232, 318);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Contrast:";
            // 
            // trContrast
            // 
            this.trContrast.Location = new System.Drawing.Point(235, 334);
            this.trContrast.Maximum = 1000;
            this.trContrast.Minimum = -1000;
            this.trContrast.Name = "trContrast";
            this.trContrast.Size = new System.Drawing.Size(139, 45);
            this.trContrast.TabIndex = 19;
            this.trContrast.TickFrequency = 200;
            this.trContrast.Scroll += new System.EventHandler(this.trContrast_Scroll);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(343, 396);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pctIcon
            // 
            this.pctIcon.Image = ((System.Drawing.Image)(resources.GetObject("pctIcon.Image")));
            this.pctIcon.Location = new System.Drawing.Point(370, 25);
            this.pctIcon.Name = "pctIcon";
            this.pctIcon.Size = new System.Drawing.Size(48, 48);
            this.pctIcon.TabIndex = 22;
            this.pctIcon.TabStop = false;
            this.pctIcon.DoubleClick += new System.EventHandler(this.pctIcon_DoubleClick);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Display name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(15, 25);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(334, 20);
            this.txtName.TabIndex = 24;
            // 
            // cmbAlign
            // 
            this.cmbAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAlign.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbAlign.FormattingEnabled = true;
            this.cmbAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
            this.cmbAlign.Location = new System.Drawing.Point(235, 254);
            this.cmbAlign.Name = "cmbAlign";
            this.cmbAlign.Size = new System.Drawing.Size(183, 21);
            this.cmbAlign.TabIndex = 26;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(232, 238);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(82, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Horizontal align:";
            // 
            // cmbScale
            // 
            this.cmbScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScale.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbScale.FormattingEnabled = true;
            this.cmbScale.Items.AddRange(new object[] {
            "1:1",
            "1:2",
            "1:4",
            "1:8"});
            this.cmbScale.Location = new System.Drawing.Point(235, 294);
            this.cmbScale.Name = "cmbScale";
            this.cmbScale.Size = new System.Drawing.Size(183, 21);
            this.cmbScale.TabIndex = 28;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(232, 278);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Scale:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rdTWAIN);
            this.panel2.Controls.Add(this.rdWIA);
            this.panel2.Location = new System.Drawing.Point(15, 51);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(334, 23);
            this.panel2.TabIndex = 29;
            // 
            // rdTWAIN
            // 
            this.rdTWAIN.AutoSize = true;
            this.rdTWAIN.Location = new System.Drawing.Point(220, 4);
            this.rdTWAIN.Name = "rdTWAIN";
            this.rdTWAIN.Size = new System.Drawing.Size(92, 17);
            this.rdTWAIN.TabIndex = 1;
            this.rdTWAIN.TabStop = true;
            this.rdTWAIN.Text = "TWAIN Driver";
            this.rdTWAIN.UseVisualStyleBackColor = true;
            // 
            // rdWIA
            // 
            this.rdWIA.AutoSize = true;
            this.rdWIA.Location = new System.Drawing.Point(4, 4);
            this.rdWIA.Name = "rdWIA";
            this.rdWIA.Size = new System.Drawing.Size(77, 17);
            this.rdWIA.TabIndex = 0;
            this.rdWIA.TabStop = true;
            this.rdWIA.Text = "WIA Driver";
            this.rdWIA.UseVisualStyleBackColor = true;
            this.rdWIA.CheckedChanged += new System.EventHandler(this.rdWIA_CheckedChanged);
            // 
            // cbHighQuality
            // 
            this.cbHighQuality.AutoSize = true;
            this.cbHighQuality.Location = new System.Drawing.Point(15, 373);
            this.cbHighQuality.Name = "cbHighQuality";
            this.cbHighQuality.Size = new System.Drawing.Size(81, 17);
            this.cbHighQuality.TabIndex = 30;
            this.cbHighQuality.Text = "Maximum quality (large files)";
            this.cbHighQuality.UseVisualStyleBackColor = true;
            // 
            // txtBrightness
            // 
            this.txtBrightness.Location = new System.Drawing.Point(160, 334);
            this.txtBrightness.Name = "txtBrightness";
            this.txtBrightness.Size = new System.Drawing.Size(38, 20);
            this.txtBrightness.TabIndex = 31;
            this.txtBrightness.Text = "0";
            this.txtBrightness.TextChanged += new System.EventHandler(this.txtBrightness_TextChanged);
            // 
            // txtContrast
            // 
            this.txtContrast.Location = new System.Drawing.Point(380, 334);
            this.txtContrast.Name = "txtContrast";
            this.txtContrast.Size = new System.Drawing.Size(38, 20);
            this.txtContrast.TabIndex = 32;
            this.txtContrast.Text = "0";
            this.txtContrast.TextChanged += new System.EventHandler(this.txtContrast_TextChanged);
            // 
            // FEditScanSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 429);
            this.Controls.Add(this.txtContrast);
            this.Controls.Add(this.txtBrightness);
            this.Controls.Add(this.cbHighQuality);
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
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbSource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnChooseDevice);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDevice);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEditScanSettings";
            this.Text = "Profile settings";
            this.Load += new System.EventHandler(this.FEditScanSettings_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trContrast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rdbConfig;
        private System.Windows.Forms.RadioButton rdbNativeWIA;
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
        private System.Windows.Forms.CheckBox cbHighQuality;
        private System.Windows.Forms.TextBox txtBrightness;
        private System.Windows.Forms.TextBox txtContrast;
    }
}
