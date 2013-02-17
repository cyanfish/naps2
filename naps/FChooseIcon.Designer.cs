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

namespace NAPS
{
    partial class FChooseIcon
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
            this.iconList = new System.Windows.Forms.ListView();
            this.ilProfileIcons = new NAPS.ILProfileIcons(this.components);
            this.SuspendLayout();
            // 
            // iconList
            // 
            this.iconList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconList.Location = new System.Drawing.Point(0, 0);
            this.iconList.MultiSelect = false;
            this.iconList.Name = "iconList";
            this.iconList.Size = new System.Drawing.Size(316, 302);
            this.iconList.TabIndex = 0;
            this.iconList.UseCompatibleStateImageBehavior = false;
            this.iconList.ItemActivate += new System.EventHandler(this.listView1_ItemActivate);
            // 
            // FChooseIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 302);
            this.Controls.Add(this.iconList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FChooseIcon";
            this.Text = "FChooseIcon";
            this.Load += new System.EventHandler(this.FChooseIcon_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView iconList;
        private ILProfileIcons ilProfileIcons;
    }
}
