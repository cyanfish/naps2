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

namespace NAPS2
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
                tsZoom.Text = tiffviewer1.Zoom.ToString("G") + "%";
            }
        }

        private void tsZoomPlus_Click(object sender, EventArgs e)
        {
            tiffviewer1.Zoom += 10;
            tsZoom.Text = tiffviewer1.Zoom.ToString("G") + "%";
        }

        private void tsZoomOut_Click(object sender, EventArgs e)
        {
            tiffviewer1.Zoom -= 10;
            tsZoom.Text = tiffviewer1.Zoom.ToString("G") + "%";
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
            tsZoom.Text = tiffviewer1.Zoom.ToString("G") + "%";
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tStrip = new System.Windows.Forms.ToolStrip();
            this.tsStretch = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsZoomPlus = new System.Windows.Forms.ToolStripButton();
            this.tsZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsZoom = new System.Windows.Forms.ToolStripLabel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.tiffviewer1 = new TiffViewer();
            this.tsZoomActual = new System.Windows.Forms.ToolStripButton();
            this.tStrip.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tStrip
            // 
            this.tStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.tStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsStretch,
                this.toolStripSeparator1,
                this.tsZoomActual,
                this.tsZoomPlus,
                this.tsZoomOut,
                this.toolStripSeparator2,
                this.tsZoom});
            this.tStrip.Location = new System.Drawing.Point(3, 0);
            this.tStrip.Name = "tStrip";
            this.tStrip.Size = new System.Drawing.Size(182, 25);
            this.tStrip.TabIndex = 7;
            this.tStrip.Text = "toolStrip1";
            // 
            // tsStretch
            // 
            this.tsStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsStretch.Image = global::NAPS2.Icons.arrow_out;
            this.tsStretch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsStretch.Name = "tsStretch";
            this.tsStretch.Size = new System.Drawing.Size(23, 22);
            this.tsStretch.Text = "toolStripButton1";
            this.tsStretch.ToolTipText = "Scale";
            this.tsStretch.CheckedChanged += new System.EventHandler(this.tsStretch_CheckedChanged);
            this.tsStretch.Click += new System.EventHandler(this.tsStretch_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsZoomPlus
            // 
            this.tsZoomPlus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomPlus.Image = global::NAPS2.Icons.zoom_in;
            this.tsZoomPlus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsZoomPlus.Name = "tsZoomPlus";
            this.tsZoomPlus.Size = new System.Drawing.Size(23, 22);
            this.tsZoomPlus.ToolTipText = "Zoom in";
            this.tsZoomPlus.Click += new System.EventHandler(this.tsZoomPlus_Click);
            // 
            // tsZoomOut
            // 
            this.tsZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomOut.Image = global::NAPS2.Icons.zoom_out;
            this.tsZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsZoomOut.Name = "tsZoomOut";
            this.tsZoomOut.Size = new System.Drawing.Size(23, 22);
            this.tsZoomOut.ToolTipText = "Zoom out";
            this.tsZoomOut.Click += new System.EventHandler(this.tsZoomOut_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsZoom
            // 
            this.tsZoom.Name = "tsZoom";
            this.tsZoom.Size = new System.Drawing.Size(35, 22);
            this.tsZoom.Text = "100%";
            this.tsZoom.ToolTipText = "Zoom";
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tiffviewer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(784, 527);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(784, 552);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tStrip);
            // 
            // tiffviewer1
            // 
            this.tiffviewer1.AutoScroll = true;
            this.tiffviewer1.BackColor = System.Drawing.Color.White;
            this.tiffviewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tiffviewer1.Location = new System.Drawing.Point(0, 0);
            this.tiffviewer1.Name = "tiffviewer1";
            this.tiffviewer1.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.tiffviewer1.Size = new System.Drawing.Size(784, 527);
            this.tiffviewer1.TabIndex = 0;
            this.tiffviewer1.Zoom = 0;
            // 
            // tsZoomActual
            // 
            this.tsZoomActual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomActual.Image = global::NAPS2.Icons.zoom_actual;
            this.tsZoomActual.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsZoomActual.Name = "tsZoomActual";
            this.tsZoomActual.Size = new System.Drawing.Size(23, 22);
            this.tsZoomActual.ToolTipText = "Zoom out";
            this.tsZoomActual.Click += new System.EventHandler(this.tsZoomActual_Click);
            // 
            // UTiffViewerCtl
            // 
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "UTiffViewerCtl";
            this.Size = new System.Drawing.Size(784, 552);
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
