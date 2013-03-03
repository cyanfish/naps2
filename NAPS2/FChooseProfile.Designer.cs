/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
    partial class FChooseProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FChooseProfile));
            this.lvProfiles = new System.Windows.Forms.ListView();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnProfiles = new System.Windows.Forms.Button();
            this.btnDone = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.ILProfileIcons(this.components);
            this.SuspendLayout();
            // 
            // lvProfiles
            // 
            this.lvProfiles.HideSelection = false;
            this.lvProfiles.Location = new System.Drawing.Point(12, 12);
            this.lvProfiles.MultiSelect = false;
            this.lvProfiles.Name = "lvProfiles";
            this.lvProfiles.Size = new System.Drawing.Size(563, 80);
            this.lvProfiles.TabIndex = 0;
            this.lvProfiles.UseCompatibleStateImageBehavior = false;
            this.lvProfiles.ItemActivate += new System.EventHandler(this.lvProfiles_ItemActivate);
            // 
            // btnScan
            // 
            this.btnScan.BackColor = System.Drawing.SystemColors.Control;
            this.btnScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScan.Image = global::NAPS2.Icons.scanner_72;
            this.btnScan.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnScan.Location = new System.Drawing.Point(581, 12);
            this.btnScan.Name = "btnScan";
            this.btnScan.Padding = new System.Windows.Forms.Padding(0, 2, 0, 6);
            this.btnScan.Size = new System.Drawing.Size(117, 118);
            this.btnScan.TabIndex = 4;
            this.btnScan.Text = "&Scan";
            this.btnScan.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnScan.UseVisualStyleBackColor = false;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnProfiles
            // 
            this.btnProfiles.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnProfiles.Image = global::NAPS2.Icons.blueprints_small;
            this.btnProfiles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProfiles.Location = new System.Drawing.Point(12, 98);
            this.btnProfiles.Name = "btnProfiles";
            this.btnProfiles.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnProfiles.Size = new System.Drawing.Size(128, 32);
            this.btnProfiles.TabIndex = 5;
            this.btnProfiles.Text = "Manage &Profiles";
            this.btnProfiles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnProfiles.UseVisualStyleBackColor = true;
            this.btnProfiles.Click += new System.EventHandler(this.btnProfiles_Click);
            // 
            // btnDone
            // 
            this.btnDone.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnDone.Image = global::NAPS2.Icons.tick_small;
            this.btnDone.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDone.Location = new System.Drawing.Point(481, 98);
            this.btnDone.Name = "btnDone";
            this.btnDone.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnDone.Size = new System.Drawing.Size(94, 32);
            this.btnDone.TabIndex = 6;
            this.btnDone.Text = "&Done";
            this.btnDone.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // FChooseProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 142);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.btnProfiles);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.lvProfiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FChooseProfile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Choose Profile";
            this.Load += new System.EventHandler(this.FChooseProfile_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvProfiles;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnProfiles;
        private System.Windows.Forms.Button btnDone;
    }
}
