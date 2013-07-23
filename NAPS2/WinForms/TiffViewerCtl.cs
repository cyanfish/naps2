/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class TiffViewerCtl : UserControl
    {
        private readonly Container components = null;
        private Image image;
        private ToolStrip tStrip;
        private TiffViewer tiffviewer1;
        private ToolStripContainer toolStripContainer1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton tsStretch;
        private ToolStripLabel tsZoom;
        private ToolStripButton tsZoomActual;
        private ToolStripButton tsZoomOut;
        private ToolStripButton tsZoomPlus;

        public TiffViewerCtl()
        {
            InitializeComponent();
            tsStretch_Click(null, null);
        }

        public Image Image
        {
            get { return image; }
            set
            {
                image = value;
                tiffviewer1.Image = value;
                tStrip.Enabled = value != null;
                AdjustZoom();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        private void TiffViewer_SizeChanged(object sender, EventArgs e)
        {
            AdjustZoom();
        }

        private void AdjustZoom()
        {
            if (tsStretch.Checked)
            {
                double containerWidth = Math.Max(tiffviewer1.Width - 20, 0);
                double containerHeight = Math.Max(tiffviewer1.Height - 20, 0);
                double zoomX = containerWidth / tiffviewer1.ImageWidth * 100;
                double zoomY = containerHeight / tiffviewer1.ImageHeight * 100;
                tiffviewer1.Zoom = (int)Math.Min(zoomX, zoomY);
                UpdateZoomText();
            }
        }

        private void UpdateZoomText()
        {
            tsZoom.Text = (tiffviewer1.Zoom/100.0).ToString("P");
        }

        private void tsZoomPlus_Click(object sender, EventArgs e)
        {
            tiffviewer1.Zoom += 10;
            UpdateZoomText();
        }

        private void tsZoomOut_Click(object sender, EventArgs e)
        {
            tiffviewer1.Zoom -= 10;
            UpdateZoomText();
        }

        private void tsStretch_Click(object sender, EventArgs e)
        {
            tsStretch.Checked = !tsStretch.Checked;
        }

        private void tsStretch_CheckedChanged(object sender, EventArgs e)
        {
            AdjustZoom();
        }

        private void tsZoomActual_Click(object sender, EventArgs e)
        {
            tiffviewer1.Zoom = 100;
            UpdateZoomText();
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiffViewerCtl));
            this.tStrip = new System.Windows.Forms.ToolStrip();
            this.tsStretch = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsZoomActual = new System.Windows.Forms.ToolStripButton();
            this.tsZoomPlus = new System.Windows.Forms.ToolStripButton();
            this.tsZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsZoom = new System.Windows.Forms.ToolStripLabel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.tiffviewer1 = new NAPS2.WinForms.TiffViewer();
            this.tStrip.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tStrip
            // 
            resources.ApplyResources(this.tStrip, "tStrip");
            this.tStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStretch,
            this.toolStripSeparator1,
            this.tsZoomActual,
            this.tsZoomPlus,
            this.tsZoomOut,
            this.toolStripSeparator2,
            this.tsZoom});
            this.tStrip.Name = "tStrip";
            // 
            // tsStretch
            // 
            this.tsStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsStretch.Image = global::NAPS2.Icons.arrow_out;
            resources.ApplyResources(this.tsStretch, "tsStretch");
            this.tsStretch.Name = "tsStretch";
            this.tsStretch.CheckedChanged += new System.EventHandler(this.tsStretch_CheckedChanged);
            this.tsStretch.Click += new System.EventHandler(this.tsStretch_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsZoomActual
            // 
            this.tsZoomActual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomActual.Image = global::NAPS2.Icons.zoom_actual;
            resources.ApplyResources(this.tsZoomActual, "tsZoomActual");
            this.tsZoomActual.Name = "tsZoomActual";
            this.tsZoomActual.Click += new System.EventHandler(this.tsZoomActual_Click);
            // 
            // tsZoomPlus
            // 
            this.tsZoomPlus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomPlus.Image = global::NAPS2.Icons.zoom_in;
            resources.ApplyResources(this.tsZoomPlus, "tsZoomPlus");
            this.tsZoomPlus.Name = "tsZoomPlus";
            this.tsZoomPlus.Click += new System.EventHandler(this.tsZoomPlus_Click);
            // 
            // tsZoomOut
            // 
            this.tsZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomOut.Image = global::NAPS2.Icons.zoom_out;
            resources.ApplyResources(this.tsZoomOut, "tsZoomOut");
            this.tsZoomOut.Name = "tsZoomOut";
            this.tsZoomOut.Click += new System.EventHandler(this.tsZoomOut_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // tsZoom
            // 
            this.tsZoom.Name = "tsZoom";
            resources.ApplyResources(this.tsZoom, "tsZoom");
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tiffviewer1);
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tStrip);
            // 
            // tiffviewer1
            // 
            resources.ApplyResources(this.tiffviewer1, "tiffviewer1");
            this.tiffviewer1.BackColor = System.Drawing.Color.White;
            this.tiffviewer1.Name = "tiffviewer1";
            this.tiffviewer1.Zoom = 0;
            // 
            // TiffViewerCtl
            // 
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "TiffViewerCtl";
            resources.ApplyResources(this, "$this");
            this.SizeChanged += new System.EventHandler(this.TiffViewer_SizeChanged);
            this.tStrip.ResumeLayout(false);
            this.tStrip.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
