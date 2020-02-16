using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class TiffViewer : UserControl
    {
        private readonly Container _components = null;

        private Image _image;
        private PictureBox _pbox;
        private double _xzoom;

        public TiffViewer()
        {
            InitializeComponent();
        }

        public Image Image
        {
            set
            {
                if (value != null)
                {
                    _image = value;
                    Zoom = 100;
                }
                else
                {
                    ClearImage();
                    _image = null;
                }
            }
        }

        public int ImageWidth => _image?.Width ?? 0;

        public int ImageHeight => _image?.Height ?? 0;

        public double Zoom
        {
            set
            {
                if (_image != null)
                {
                    double maxZoom = Math.Sqrt(1e8 / (_image.Width * (double) _image.Height)) * 100;
                    _xzoom = Math.Max(Math.Min(value, maxZoom), 10);
                    double displayWidth = _image.Width * (_xzoom / 100);
                    double displayHeight = _image.Height * (_xzoom / 100);
                    if (_image.HorizontalResolution > 0 && _image.VerticalResolution > 0)
                    {
                        displayHeight *= _image.HorizontalResolution / (double)_image.VerticalResolution;
                    }
                    _pbox.Image = _image;
                    _pbox.BorderStyle = BorderStyle.FixedSingle;
                    _pbox.Width = (int)displayWidth;
                    _pbox.Height = (int)displayHeight;
                    if (ZoomChanged != null)
                    {
                        _pbox.Cursor = HorizontalScroll.Visible || VerticalScroll.Visible ? Cursors.Hand : Cursors.Default;
                        ZoomChanged.Invoke(this, new EventArgs());
                    }
                }
            }
            get => _xzoom;
        }

        public event EventHandler<EventArgs> ZoomChanged;

        private void ClearImage()
        {
            _pbox.Image = Icons.hourglass_grey;
            _pbox.BorderStyle = BorderStyle.None;
            _pbox.Width = 32;
            _pbox.Height = 32;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                StepZoom(e.Delta / (double)SystemInformation.MouseWheelScrollDelta);
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.OemMinus:
                    if (e.Control)
                    {
                        StepZoom(-1);
                    }
                    break;
                case Keys.Oemplus:
                    if (e.Control)
                    {
                        StepZoom(1);
                    }
                    break;
            }
        }

        public void StepZoom(double steps)
        {
            Zoom = Math.Round(Zoom * Math.Pow(1.2, steps));
        }

        private Point _mousePos;

        private void pbox_MouseDown(object sender, MouseEventArgs e)
        {
            _mousePos = e.Location;
        }

        private void pbox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AutoScrollPosition = new Point(-AutoScrollPosition.X + _mousePos.X - e.X, -AutoScrollPosition.Y + _mousePos.Y - e.Y);
            }
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiffViewer));
            this._pbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._pbox)).BeginInit();
            this.SuspendLayout();
            // 
            // pbox
            // 
            resources.ApplyResources(this._pbox, "_pbox");
            this._pbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pbox.Name = "_pbox";
            this._pbox.TabStop = false;
            this._pbox.SizeMode = PictureBoxSizeMode.Zoom;
            this._pbox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbox_MouseDown);
            this._pbox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbox_MouseMove);
            // 
            // TiffViewer
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this._pbox);
            this.Name = "TiffViewer";
            ((System.ComponentModel.ISupportInitialize)(this._pbox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
