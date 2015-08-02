/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;

namespace NAPS2.WinForms
{
    public class FViewer : FormBase
    {
        private readonly Container components = null;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private ToolStripTextBox tbPageCurrent;
        private ToolStripLabel lblPageTotal;
        private ToolStripButton tsPrev;
        private ToolStripButton tsNext;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripDropDownButton tsdRotate;
        private ToolStripMenuItem tsRotateLeft;
        private ToolStripMenuItem tsRotateRight;
        private ToolStripMenuItem tsFlip;
        private ToolStripMenuItem tsCustomRotation;
        private ToolStripButton tsCrop;
        private ToolStripButton tsBrightness;
        private ToolStripButton tsContrast;
        private TiffViewerCtl tiffViewer1;

        public FViewer()
        {
            InitializeComponent();
        }

        public ScannedImageList ImageList { get; set; }
        public int ImageIndex { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            tiffViewer1.Image = ImageList.Images[ImageIndex].GetImage();
            tbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.InvariantCulture);
            lblPageTotal.Text = string.Format(lblPageTotal.Text, ImageList.Images.Count);
        }

        private void GoTo(int index)
        {
            if (index == ImageIndex || index < 0 || index >= ImageList.Images.Count)
            {
                return;
            }
            ImageIndex = index;
            UpdateImage();
            tbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.CurrentCulture);
        }

        private void UpdateImage()
        {
            tiffViewer1.Image.Dispose();
            tiffViewer1.Image = ImageList.Images[ImageIndex].GetImage();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (tiffViewer1 != null)
                {
                    tiffViewer1.Image.Dispose();
                    tiffViewer1.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FViewer));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.tiffViewer1 = new NAPS2.WinForms.TiffViewerCtl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tbPageCurrent = new System.Windows.Forms.ToolStripTextBox();
            this.lblPageTotal = new System.Windows.Forms.ToolStripLabel();
            this.tsPrev = new System.Windows.Forms.ToolStripButton();
            this.tsNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsdRotate = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRotateRight = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCustomRotation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCrop = new System.Windows.Forms.ToolStripButton();
            this.tsBrightness = new System.Windows.Forms.ToolStripButton();
            this.tsContrast = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tiffViewer1);
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // tiffViewer1
            // 
            resources.ApplyResources(this.tiffViewer1, "tiffViewer1");
            this.tiffViewer1.Image = null;
            this.tiffViewer1.Name = "tiffViewer1";
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbPageCurrent,
            this.lblPageTotal,
            this.tsPrev,
            this.tsNext,
            this.toolStripSeparator1,
            this.tsdRotate,
            this.tsCrop,
            this.tsBrightness,
            this.tsContrast});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // tbPageCurrent
            // 
            this.tbPageCurrent.Name = "tbPageCurrent";
            resources.ApplyResources(this.tbPageCurrent, "tbPageCurrent");
            this.tbPageCurrent.TextChanged += new System.EventHandler(this.tbPageCurrent_TextChanged);
            // 
            // lblPageTotal
            // 
            this.lblPageTotal.Name = "lblPageTotal";
            resources.ApplyResources(this.lblPageTotal, "lblPageTotal");
            // 
            // tsPrev
            // 
            this.tsPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsPrev.Image = global::NAPS2.Icons.arrow_left;
            resources.ApplyResources(this.tsPrev, "tsPrev");
            this.tsPrev.Name = "tsPrev";
            this.tsPrev.Click += new System.EventHandler(this.tsPrev_Click);
            // 
            // tsNext
            // 
            this.tsNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsNext.Image = global::NAPS2.Icons.arrow_right;
            resources.ApplyResources(this.tsNext, "tsNext");
            this.tsNext.Name = "tsNext";
            this.tsNext.Click += new System.EventHandler(this.tsNext_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsdRotate
            // 
            this.tsdRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsdRotate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsRotateLeft,
            this.tsRotateRight,
            this.tsFlip,
            this.tsCustomRotation});
            this.tsdRotate.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            resources.ApplyResources(this.tsdRotate, "tsdRotate");
            this.tsdRotate.Name = "tsdRotate";
            this.tsdRotate.ShowDropDownArrow = false;
            // 
            // tsRotateLeft
            // 
            this.tsRotateLeft.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            this.tsRotateLeft.Name = "tsRotateLeft";
            resources.ApplyResources(this.tsRotateLeft, "tsRotateLeft");
            this.tsRotateLeft.Click += new System.EventHandler(this.tsRotateLeft_Click);
            // 
            // tsRotateRight
            // 
            this.tsRotateRight.Image = global::NAPS2.Icons.arrow_rotate_clockwise_small;
            this.tsRotateRight.Name = "tsRotateRight";
            resources.ApplyResources(this.tsRotateRight, "tsRotateRight");
            this.tsRotateRight.Click += new System.EventHandler(this.tsRotateRight_Click);
            // 
            // tsFlip
            // 
            this.tsFlip.Image = global::NAPS2.Icons.arrow_switch_small;
            this.tsFlip.Name = "tsFlip";
            resources.ApplyResources(this.tsFlip, "tsFlip");
            this.tsFlip.Click += new System.EventHandler(this.tsFlip_Click);
            // 
            // tsCustomRotation
            // 
            this.tsCustomRotation.Name = "tsCustomRotation";
            resources.ApplyResources(this.tsCustomRotation, "tsCustomRotation");
            this.tsCustomRotation.Click += new System.EventHandler(this.tsCustomRotation_Click);
            // 
            // tsCrop
            // 
            this.tsCrop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsCrop.Image = global::NAPS2.Icons.transform_crop;
            resources.ApplyResources(this.tsCrop, "tsCrop");
            this.tsCrop.Name = "tsCrop";
            this.tsCrop.Click += new System.EventHandler(this.tsCrop_Click);
            // 
            // tsBrightness
            // 
            this.tsBrightness.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBrightness.Image = global::NAPS2.Icons.weather_sun;
            resources.ApplyResources(this.tsBrightness, "tsBrightness");
            this.tsBrightness.Name = "tsBrightness";
            this.tsBrightness.Click += new System.EventHandler(this.tsBrightness_Click);
            // 
            // tsContrast
            // 
            this.tsContrast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsContrast.Image = global::NAPS2.Icons.contrast;
            resources.ApplyResources(this.tsContrast, "tsContrast");
            this.tsContrast.Name = "tsContrast";
            this.tsContrast.Click += new System.EventHandler(this.tsContrast_Click);
            // 
            // FViewer
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "FViewer";
            this.ShowInTaskbar = false;
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void tbPageCurrent_TextChanged(object sender, EventArgs e)
        {
            int indexOffBy1;
            if (int.TryParse(tbPageCurrent.Text, out indexOffBy1))
            {
                GoTo(indexOffBy1 - 1);
            }
        }

        private void tsNext_Click(object sender, EventArgs e)
        {
            GoTo(ImageIndex + 1);
        }

        private void tsPrev_Click(object sender, EventArgs e)
        {
            GoTo(ImageIndex - 1);
        }

        private void tsRotateLeft_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate270FlipNone);
            UpdateImage();
        }

        private void tsRotateRight_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate90FlipNone);
            UpdateImage();
        }

        private void tsFlip_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate180FlipNone);
            UpdateImage();
        }

        private void tsCustomRotation_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FRotate>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
        }

        private void tsCrop_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FCrop>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
        }

        private void tsBrightness_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBrightness>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
        }

        private void tsContrast_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FContrast>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
        }
    }
}
