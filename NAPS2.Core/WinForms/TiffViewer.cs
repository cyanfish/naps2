using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class TiffViewer : UserControl
    {
        private readonly Container components = null;

        private Image image;
        private PictureBox pbox;
        private double xzoom;

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
                    image = value;
                    Zoom = 100;
                }
                else
                {
                    ClearImage();
                    image = null;
                }
            }
        }

        public int ImageWidth => image?.Width ?? 0;

        public int ImageHeight => image?.Height ?? 0;

        public double Zoom
        {
            set
            {
                if (image != null)
                {
                    double maxZoom = Math.Sqrt(1e8 / (image.Width * (double) image.Height)) * 100;
                    xzoom = Math.Max(Math.Min(value, maxZoom), 10);
                    double displayWidth = image.Width * (xzoom / 100);
                    double displayHeight = image.Height * (xzoom / 100);
                    if (image.HorizontalResolution > 0 && image.VerticalResolution > 0)
                    {
                        displayHeight *= image.HorizontalResolution / (double)image.VerticalResolution;
                    }
                    pbox.Image = image;
                    pbox.BorderStyle = BorderStyle.FixedSingle;
                    pbox.Width = (int)displayWidth;
                    pbox.Height = (int)displayHeight;
                    if (ZoomChanged != null)
                    {
                        pbox.Cursor = HorizontalScroll.Visible || VerticalScroll.Visible ? Cursors.Hand : Cursors.Default;
                        ZoomChanged.Invoke(this, new EventArgs());
                    }
                }
            }
            get => xzoom;
        }

        public event EventHandler<EventArgs> ZoomChanged;

        private void ClearImage()
        {
            pbox.Image = Icons.hourglass_grey;
            pbox.BorderStyle = BorderStyle.None;
            pbox.Width = 32;
            pbox.Height = 32;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
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

        private Point mousePos;

        private void pbox_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
        }

        private void pbox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AutoScrollPosition = new Point(-AutoScrollPosition.X + mousePos.X - e.X, -AutoScrollPosition.Y + mousePos.Y - e.Y);
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
            this.pbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).BeginInit();
            this.SuspendLayout();
            // 
            // pbox
            // 
            resources.ApplyResources(this.pbox, "pbox");
            this.pbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbox.Name = "pbox";
            this.pbox.TabStop = false;
            this.pbox.SizeMode = PictureBoxSizeMode.Zoom;
            this.pbox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbox_MouseDown);
            this.pbox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbox_MouseMove);
            // 
            // TiffViewer
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.pbox);
            this.Name = "TiffViewer";
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
