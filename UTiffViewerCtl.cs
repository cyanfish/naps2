using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace TiffViewerCtl
{
	public class UTiffViewerCtl : System.Windows.Forms.UserControl
	{
        private TiffViewer.UTiffViewer tiffviewer1;
        private ToolStrip tStrip;
        private ToolStripButton tsZoomPlus;
        private ToolStripLabel tsZoom;
        private ToolStripButton tsZoomOut;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton tsStretch;
        private ToolStripContainer toolStripContainer1;
		private System.ComponentModel.Container components = null;

		public UTiffViewerCtl()
		{
			InitializeComponent();
			tsStretch_Click(null,null);
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UTiffViewerCtl));
            this.tStrip = new System.Windows.Forms.ToolStrip();
            this.tsZoomPlus = new System.Windows.Forms.ToolStripButton();
            this.tsZoom = new System.Windows.Forms.ToolStripLabel();
            this.tsZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsStretch = new System.Windows.Forms.ToolStripButton();
            this.tiffviewer1 = new TiffViewer.UTiffViewer();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
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
            this.tsZoomPlus,
            this.tsZoom,
            this.tsZoomOut,
            this.toolStripSeparator1,
            this.tsStretch});
            this.tStrip.Location = new System.Drawing.Point(3, 0);
            this.tStrip.Name = "tStrip";
            this.tStrip.Size = new System.Drawing.Size(152, 25);
            this.tStrip.TabIndex = 7;
            this.tStrip.Text = "toolStrip1";
            // 
            // tsZoomPlus
            // 
            this.tsZoomPlus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomPlus.Image = ((System.Drawing.Image)(resources.GetObject("tsZoomPlus.Image")));
            this.tsZoomPlus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsZoomPlus.Name = "tsZoomPlus";
            this.tsZoomPlus.Size = new System.Drawing.Size(23, 22);
            this.tsZoomPlus.ToolTipText = "Zoom in";
            this.tsZoomPlus.Click += new System.EventHandler(this.tsZoomPlus_Click);
            // 
            // tsZoom
            // 
            this.tsZoom.Name = "tsZoom";
            this.tsZoom.Size = new System.Drawing.Size(36, 22);
            this.tsZoom.Text = "100%";
            this.tsZoom.ToolTipText = "Zoom";
            // 
            // tsZoomOut
            // 
            this.tsZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("tsZoomOut.Image")));
            this.tsZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsZoomOut.Name = "tsZoomOut";
            this.tsZoomOut.Size = new System.Drawing.Size(23, 22);
            this.tsZoomOut.ToolTipText = "Zoom out";
            this.tsZoomOut.Click += new System.EventHandler(this.tsZoomOut_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsStretch
            // 
            this.tsStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsStretch.Image = ((System.Drawing.Image)(resources.GetObject("tsStretch.Image")));
            this.tsStretch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsStretch.Name = "tsStretch";
            this.tsStretch.Size = new System.Drawing.Size(23, 22);
            this.tsStretch.Text = "toolStripButton1";
            this.tsStretch.ToolTipText = "Scale";
            this.tsStretch.Click += new System.EventHandler(this.tsStretch_Click);
            // 
            // tiffviewer1
            // 
            this.tiffviewer1.AutoScroll = true;
            this.tiffviewer1.BackColor = System.Drawing.Color.LightGray;
            this.tiffviewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tiffviewer1.Location = new System.Drawing.Point(0, 0);
            this.tiffviewer1.Name = "tiffviewer1";
            this.tiffviewer1.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.tiffviewer1.Size = new System.Drawing.Size(784, 527);
            this.tiffviewer1.TabIndex = 0;
            this.tiffviewer1.Zoom = ((ushort)(0));
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
		
		private Image image;

        private void TiffViewer_SizeChanged(object sender, System.EventArgs e)
		{
			if(tsStretch.Checked)
			{
				this.tiffviewer1.Zoom = (ushort)((double)(tiffviewer1.Width - 40) / ((double)tiffviewer1.ImageWidth / 100));
				tsZoom.Text = tiffviewer1.Zoom.ToString() + "%";
			}
		}

		public Image Image
		{
			get{return image;}
			set
			{
				image = value;
				this.tiffviewer1.image = value;
				if(value == null)
				{
					tStrip.Enabled = false;
					
				}
				else
				{
                    tStrip.Enabled = true;
				}
				if(tsStretch.Checked)
				{
					this.tiffviewer1.Zoom = (ushort)((double)(tiffviewer1.Width - 40) / ((double)tiffviewer1.ImageWidth / 100));
					tsZoom.Text = tiffviewer1.Zoom.ToString() + "%";
				}
			}
		}

        private void tsZoomPlus_Click(object sender, EventArgs e)
        {
            tiffviewer1.Zoom += 10;
            tsZoom.Text = tiffviewer1.Zoom.ToString() + "%";
        }

        private void tsZoomOut_Click(object sender, EventArgs e)
        {
            if (tiffviewer1.Zoom > 20)
            {
                tiffviewer1.Zoom -= 10;
                tsZoom.Text = tiffviewer1.Zoom.ToString() + "%";
            }
        }

        private void tsStretch_Click(object sender, EventArgs e)
        {
            tsStretch.Checked = !tsStretch.Checked;
            if (tsStretch.Checked)
            {
                this.tiffviewer1.Zoom = (ushort)((double)(tiffviewer1.Width - 40) / ((double)tiffviewer1.ImageWidth / 100));
                tsZoom.Text = tiffviewer1.Zoom.ToString() + "%";
            }
        }
		
	}
}
