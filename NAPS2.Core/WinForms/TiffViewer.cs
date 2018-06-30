using System;
using System.Drawing;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class TiffViewer : UserControl
    {
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

        public int ImageWidth
        {
            get
            {
                if (image != null)
                {
                    return image.Width;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int ImageHeight
        {
            get
            {
                if (image != null)
                {
                    return image.Height;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double Zoom
        {
            set
            {
                if (image != null)
                {
                    xzoom = Math.Max(Math.Min(value, 1000), 10);
                    double displayWidth = image.Width * ((double)xzoom / 100);
                    double displayHeight = image.Height * ((double)xzoom / 100) * (image.HorizontalResolution / (double)image.VerticalResolution);
                    pbox.Image = image;
                    pbox.Width = (int)displayWidth;
                    pbox.Height = (int)displayHeight;
                    if (ZoomChanged != null)
                    {
                        pbox.Cursor = HorizontalScroll.Visible || VerticalScroll.Visible ? Cursors.Hand : Cursors.Default;
                        ZoomChanged.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            get => xzoom;
        }

        public event EventHandler<EventArgs> ZoomChanged;

        private void ClearImage()
        {
            pbox.Image = null;
            pbox.Width = 1;
            pbox.Height = 1;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
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

        private void Pbox_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
        }

        private void Pbox_MouseMove(object sender, MouseEventArgs e)
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
            pbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(pbox)).BeginInit();
            SuspendLayout();
            //
            // pbox
            //
            resources.ApplyResources(pbox, "pbox");
            pbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pbox.Name = "pbox";
            pbox.TabStop = false;
            pbox.SizeMode = PictureBoxSizeMode.Zoom;
            pbox.MouseDown += Pbox_MouseDown;
            pbox.MouseMove += Pbox_MouseMove;
            //
            // TiffViewer
            //
            resources.ApplyResources(this, "$this");
            BackColor = System.Drawing.Color.LightGray;
            Controls.Add(pbox);
            Name = "TiffViewer";
            ((System.ComponentModel.ISupportInitialize)(pbox)).EndInit();
            ResumeLayout(false);
        }

        #endregion Component Designer generated code
    }
}