using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NAPS2.Util;

namespace NAPS2.WinForms;

public class TiffViewerCtl : UserControl
{
    private readonly Container _components = null;
    private Image _image;
    private ToolStrip _tStrip;
    private TiffViewer _tiffviewer1;
    private ToolStripContainer _toolStripContainer1;
    private ToolStripSeparator _toolStripSeparator1;
    private ToolStripSeparator _toolStripSeparator2;
    private ToolStripButton _tsStretch;
    private ToolStripLabel _tsZoom;
    private ToolStripButton _tsZoomActual;
    private ToolStripButton _tsZoomOut;
    private ToolStripButton _tsZoomPlus;

    public TiffViewerCtl()
    {
        InitializeComponent();
        _tiffviewer1.ZoomChanged += Tiffviewer1OnZoomChanged;
        tsStretch_Click(null, null);
    }

    private void Tiffviewer1OnZoomChanged(object sender, EventArgs eventArgs)
    {
        _tsZoom.Text = (_tiffviewer1.Zoom / 100.0).ToString("P1");
    }

    public Image Image
    {
        get => _image;
        set
        {
            _image = value;
            _tiffviewer1.Image = value;
            _tStrip.Enabled = value != null;
            AdjustZoom();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void TiffViewer_SizeChanged(object sender, EventArgs e)
    {
        AdjustZoom();
    }

    private void AdjustZoom()
    {
        if (_tsStretch.Checked && _tiffviewer1.ImageWidth != 0 && _tiffviewer1.ImageHeight != 0)
        {
            double containerWidth = Math.Max(_tiffviewer1.Width - 20, 0);
            double containerHeight = Math.Max(_tiffviewer1.Height - 20, 0);
            double zoomX = containerWidth / _tiffviewer1.ImageWidth * 100;
            double zoomY = containerHeight / _tiffviewer1.ImageHeight * 100;
            _tiffviewer1.Zoom = Math.Min(zoomX, zoomY);
        }
    }

    private void tsZoomPlus_Click(object sender, EventArgs e)
    {
        _tiffviewer1.StepZoom(1);
    }

    private void tsZoomOut_Click(object sender, EventArgs e)
    {
        _tiffviewer1.StepZoom(-1);
    }

    private void tsStretch_Click(object sender, EventArgs e)
    {
        _tsStretch.Checked = !_tsStretch.Checked;
    }

    private void tsStretch_CheckedChanged(object sender, EventArgs e)
    {
        AdjustZoom();
    }

    private void tsZoomActual_Click(object sender, EventArgs e)
    {
        _tiffviewer1.Zoom = 100;
    }

    private void tiffviewer1_KeyDown(object sender, KeyEventArgs e)
    {
        // Pass through events to the parent form in case it listens for them
        OnKeyDown(e);

        if (e.Control || e.Alt || e.Shift)
        {
            int m = e.Control ? 10 : e.Alt ? 5 : 1;
            if (e.KeyCode == Keys.Up)
            {
                DeltaScroll(_tiffviewer1.VerticalScroll, -m);
            }
            if (e.KeyCode == Keys.Down)
            {
                DeltaScroll(_tiffviewer1.VerticalScroll, m);
            }
            if (e.KeyCode == Keys.Left)
            {
                DeltaScroll(_tiffviewer1.HorizontalScroll, -m);
            }
            if (e.KeyCode == Keys.Right)
            {
                DeltaScroll(_tiffviewer1.HorizontalScroll, m);
            }
        }
    }

    private void tiffviewer1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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
        int newValue = (scroll.Value + scroll.SmallChange * direction).Clamp(scroll.Minimum, scroll.Maximum);
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
        this._toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
        this._tiffviewer1 = new NAPS2.WinForms.TiffViewer();
        this._tStrip = new System.Windows.Forms.ToolStrip();
        this._tsStretch = new System.Windows.Forms.ToolStripButton();
        this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        this._tsZoomActual = new System.Windows.Forms.ToolStripButton();
        this._tsZoomPlus = new System.Windows.Forms.ToolStripButton();
        this._tsZoomOut = new System.Windows.Forms.ToolStripButton();
        this._toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
        this._tsZoom = new System.Windows.Forms.ToolStripLabel();
        this._toolStripContainer1.ContentPanel.SuspendLayout();
        this._toolStripContainer1.TopToolStripPanel.SuspendLayout();
        this._toolStripContainer1.SuspendLayout();
        this._tStrip.SuspendLayout();
        this.SuspendLayout();
        // 
        // toolStripContainer1
        // 
        // 
        // toolStripContainer1.ContentPanel
        // 
        this._toolStripContainer1.ContentPanel.Controls.Add(this._tiffviewer1);
        resources.ApplyResources(this._toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
        resources.ApplyResources(this._toolStripContainer1, "_toolStripContainer1");
        this._toolStripContainer1.Name = "_toolStripContainer1";
        // 
        // toolStripContainer1.TopToolStripPanel
        // 
        this._toolStripContainer1.TopToolStripPanel.Controls.Add(this._tStrip);
        // 
        // tiffviewer1
        // 
        resources.ApplyResources(this._tiffviewer1, "_tiffviewer1");
        this._tiffviewer1.BackColor = System.Drawing.Color.White;
        this._tiffviewer1.Name = "_tiffviewer1";
        this._tiffviewer1.Zoom = 0D;
        this._tiffviewer1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tiffviewer1_KeyDown);
        this._tiffviewer1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.tiffviewer1_PreviewKeyDown);
        // 
        // tStrip
        // 
        resources.ApplyResources(this._tStrip, "_tStrip");
        this._tStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsStretch,
            this._toolStripSeparator1,
            this._tsZoomActual,
            this._tsZoomPlus,
            this._tsZoomOut,
            this._toolStripSeparator2,
            this._tsZoom});
        this._tStrip.Name = "_tStrip";
        // 
        // tsStretch
        // 
        this._tsStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsStretch.Image = global::NAPS2.Icons.arrow_out;
        resources.ApplyResources(this._tsStretch, "_tsStretch");
        this._tsStretch.Name = "_tsStretch";
        this._tsStretch.CheckedChanged += new System.EventHandler(this.tsStretch_CheckedChanged);
        this._tsStretch.Click += new System.EventHandler(this.tsStretch_Click);
        // 
        // toolStripSeparator1
        // 
        this._toolStripSeparator1.Name = "_toolStripSeparator1";
        resources.ApplyResources(this._toolStripSeparator1, "_toolStripSeparator1");
        // 
        // tsZoomActual
        // 
        this._tsZoomActual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsZoomActual.Image = global::NAPS2.Icons.zoom_actual;
        resources.ApplyResources(this._tsZoomActual, "_tsZoomActual");
        this._tsZoomActual.Name = "_tsZoomActual";
        this._tsZoomActual.Click += new System.EventHandler(this.tsZoomActual_Click);
        // 
        // tsZoomPlus
        // 
        this._tsZoomPlus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsZoomPlus.Image = global::NAPS2.Icons.zoom_in;
        resources.ApplyResources(this._tsZoomPlus, "_tsZoomPlus");
        this._tsZoomPlus.Name = "_tsZoomPlus";
        this._tsZoomPlus.Click += new System.EventHandler(this.tsZoomPlus_Click);
        // 
        // tsZoomOut
        // 
        this._tsZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsZoomOut.Image = global::NAPS2.Icons.zoom_out;
        resources.ApplyResources(this._tsZoomOut, "_tsZoomOut");
        this._tsZoomOut.Name = "_tsZoomOut";
        this._tsZoomOut.Click += new System.EventHandler(this.tsZoomOut_Click);
        // 
        // toolStripSeparator2
        // 
        this._toolStripSeparator2.Name = "_toolStripSeparator2";
        resources.ApplyResources(this._toolStripSeparator2, "_toolStripSeparator2");
        // 
        // tsZoom
        // 
        this._tsZoom.Name = "_tsZoom";
        resources.ApplyResources(this._tsZoom, "_tsZoom");
        // 
        // TiffViewerCtl
        // 
        this.Controls.Add(this._toolStripContainer1);
        this.Name = "TiffViewerCtl";
        resources.ApplyResources(this, "$this");
        this.SizeChanged += new System.EventHandler(this.TiffViewer_SizeChanged);
        this._toolStripContainer1.ContentPanel.ResumeLayout(false);
        this._toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
        this._toolStripContainer1.TopToolStripPanel.PerformLayout();
        this._toolStripContainer1.ResumeLayout(false);
        this._toolStripContainer1.PerformLayout();
        this._tStrip.ResumeLayout(false);
        this._tStrip.PerformLayout();
        this.ResumeLayout(false);

    }
    #endregion
}