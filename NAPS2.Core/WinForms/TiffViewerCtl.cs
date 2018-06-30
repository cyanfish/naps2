using NAPS2.Util;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class TiffViewerCtl : UserControl
    {
        private Image image;
        private ToolStrip TStrip;
        private TiffViewer Tiffviewer1;
        private ToolStripContainer toolStripContainer1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton TsStretch;
        private ToolStripLabel TsZoom;
        private ToolStripButton TsZoomActual;
        private ToolStripButton TsZoomOut;
        private ToolStripButton TsZoomPlus;

        public TiffViewerCtl()
        {
            InitializeComponent();
            Tiffviewer1.ZoomChanged += Tiffviewer1OnZoomChanged;
            TsStretch_Click(null, null);
        }

        private void Tiffviewer1OnZoomChanged(object sender, EventArgs eventArgs)
        {
            TsZoom.Text = (Tiffviewer1.Zoom / 100.0).ToString("P1");
        }

        public Image Image
        {
            get => image;
            set
            {
                image = value;
                Tiffviewer1.Image = value;
                TStrip.Enabled = value != null;
                AdjustZoom();
            }
        }

        private void TiffViewer_SizeChanged(object sender, EventArgs e)
        {
            AdjustZoom();
        }

        private void AdjustZoom()
        {
            if (TsStretch.Checked && Tiffviewer1.ImageWidth != 0 && Tiffviewer1.ImageHeight != 0)
            {
                double containerWidth = Math.Max(Tiffviewer1.Width - 20, 0);
                double containerHeight = Math.Max(Tiffviewer1.Height - 20, 0);
                double zoomX = containerWidth / Tiffviewer1.ImageWidth * 100;
                double zoomY = containerHeight / Tiffviewer1.ImageHeight * 100;
                Tiffviewer1.Zoom = Math.Min(zoomX, zoomY);
            }
        }

        private void TsZoomPlus_Click(object sender, EventArgs e)
        {
            Tiffviewer1.StepZoom(1);
        }

        private void TsZoomOut_Click(object sender, EventArgs e)
        {
            Tiffviewer1.StepZoom(-1);
        }

        private void TsStretch_Click(object sender, EventArgs e)
        {
            TsStretch.Checked = !TsStretch.Checked;
        }

        private void TsStretch_CheckedChanged(object sender, EventArgs e)
        {
            AdjustZoom();
        }

        private void TsZoomActual_Click(object sender, EventArgs e)
        {
            Tiffviewer1.Zoom = 100;
        }

        private void Tiffviewer1_KeyDown(object sender, KeyEventArgs e)
        {
            // Pass through events to the parent form in case it listens for them
            OnKeyDown(e);

            if (e.Control || e.Alt || e.Shift)
            {
                int m = e.Control ? 10 : e.Alt ? 5 : 1;
                if (e.KeyCode == Keys.Up)
                {
                    DeltaScroll(Tiffviewer1.VerticalScroll, -m);
                }
                if (e.KeyCode == Keys.Down)
                {
                    DeltaScroll(Tiffviewer1.VerticalScroll, m);
                }
                if (e.KeyCode == Keys.Left)
                {
                    DeltaScroll(Tiffviewer1.HorizontalScroll, -m);
                }
                if (e.KeyCode == Keys.Right)
                {
                    DeltaScroll(Tiffviewer1.HorizontalScroll, m);
                }
            }
        }

        private void Tiffviewer1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }

        private void DeltaScroll(ScrollProperties scroll, int direction)
        {
            int newValue = (scroll.Value + (scroll.SmallChange * direction)).Clamp(scroll.Minimum, scroll.Maximum);
            // For whatever reason the scroll value is "sticky". Changing it twice seems to work fine.
            scroll.Value = newValue;
            scroll.Value = newValue;
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiffViewerCtl));
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            Tiffviewer1 = new NAPS2.WinForms.TiffViewer();
            TStrip = new System.Windows.Forms.ToolStrip();
            TsStretch = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            TsZoomActual = new System.Windows.Forms.ToolStripButton();
            TsZoomPlus = new System.Windows.Forms.ToolStripButton();
            TsZoomOut = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            TsZoom = new System.Windows.Forms.ToolStripLabel();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            TStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStripContainer1
            //
            //
            // toolStripContainer1.ContentPanel
            //
            toolStripContainer1.ContentPanel.Controls.Add(Tiffviewer1);
            resources.ApplyResources(toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(toolStripContainer1, "toolStripContainer1");
            toolStripContainer1.Name = "toolStripContainer1";
            //
            // toolStripContainer1.TopToolStripPanel
            //
            toolStripContainer1.TopToolStripPanel.Controls.Add(TStrip);
            //
            // Tiffviewer1
            //
            resources.ApplyResources(Tiffviewer1, "Tiffviewer1");
            Tiffviewer1.BackColor = System.Drawing.Color.White;
            Tiffviewer1.Name = "Tiffviewer1";
            Tiffviewer1.Zoom = 0D;
            Tiffviewer1.KeyDown += Tiffviewer1_KeyDown;
            Tiffviewer1.PreviewKeyDown += Tiffviewer1_PreviewKeyDown;
            //
            // TStrip
            //
            resources.ApplyResources(TStrip, "TStrip");
            TStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            TsStretch,
            toolStripSeparator1,
            TsZoomActual,
            TsZoomPlus,
            TsZoomOut,
            toolStripSeparator2,
            TsZoom});
            TStrip.Name = "TStrip";
            //
            // TsStretch
            //
            TsStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsStretch.Image = global::NAPS2.Icons.arrow_out;
            resources.ApplyResources(TsStretch, "TsStretch");
            TsStretch.Name = "TsStretch";
            TsStretch.CheckedChanged += TsStretch_CheckedChanged;
            TsStretch.Click += TsStretch_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            //
            // TsZoomActual
            //
            TsZoomActual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsZoomActual.Image = global::NAPS2.Icons.zoom_actual;
            resources.ApplyResources(TsZoomActual, "TsZoomActual");
            TsZoomActual.Name = "TsZoomActual";
            TsZoomActual.Click += TsZoomActual_Click;
            //
            // TsZoomPlus
            //
            TsZoomPlus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsZoomPlus.Image = global::NAPS2.Icons.zoom_in;
            resources.ApplyResources(TsZoomPlus, "TsZoomPlus");
            TsZoomPlus.Name = "TsZoomPlus";
            TsZoomPlus.Click += TsZoomPlus_Click;
            //
            // TsZoomOut
            //
            TsZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsZoomOut.Image = global::NAPS2.Icons.zoom_out;
            resources.ApplyResources(TsZoomOut, "TsZoomOut");
            TsZoomOut.Name = "TsZoomOut";
            TsZoomOut.Click += TsZoomOut_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            //
            // TsZoom
            //
            TsZoom.Name = "TsZoom";
            resources.ApplyResources(TsZoom, "TsZoom");
            //
            // TiffViewerCtl
            //
            Controls.Add(toolStripContainer1);
            Name = "TiffViewerCtl";
            resources.ApplyResources(this, "$this");
            SizeChanged += TiffViewer_SizeChanged;
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            TStrip.ResumeLayout(false);
            TStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion Component Designer generated code
    }
}