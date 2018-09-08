using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    partial class FCrop : ImageForm
    {
        private static CropTransform _lastTransform;
        private static Size _lastSize;

        private Point dragStartCoords;

        public FCrop(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
            : base(changeTracker, thumbnailRenderer, scannedImageRenderer)
        {
            InitializeComponent();
        }

        public CropTransform CropTransform { get; private set; }

        protected override IEnumerable<Transform> Transforms => new[] { CropTransform };

        protected override PictureBox PictureBox => pictureBox;

        protected override Bitmap RenderPreview()
        {
            var bitmap = new Bitmap(workingImage2.Width, workingImage2.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.5f });
                g.DrawImage(workingImage2,
                    new Rectangle(0, 0, workingImage2.Width, workingImage2.Height),
                    0,
                    0,
                    workingImage2.Width,
                    workingImage2.Height,
                    GraphicsUnit.Pixel,
                    attrs);
                var cropBorderRect = new Rectangle(CropTransform.Left, CropTransform.Top,
                    workingImage2.Width - CropTransform.Left - CropTransform.Right,
                    workingImage2.Height - CropTransform.Top - CropTransform.Bottom);
                g.SetClip(cropBorderRect);
                g.DrawImage(workingImage2, new Rectangle(0, 0, workingImage2.Width, workingImage2.Height));
                g.ResetClip();
                g.DrawRectangle(new Pen(Color.Black, 2.0f), cropBorderRect);
            }

            return bitmap;
        }

        protected override void InitTransform()
        {
            if (_lastTransform != null && _lastSize == workingImage.Size)
            {
                CropTransform = _lastTransform;
            }
            else
            {
                ResetTransform();
            }
            UpdateCropBounds();
        }

        protected override void ResetTransform()
        {
            CropTransform = new CropTransform
            {
                OriginalHeight = workingImage.Height,
                OriginalWidth = workingImage.Width
            };
        }

        protected override void TransformSaved()
        {
            _lastTransform = CropTransform;
            _lastSize = workingImage.Size;
        }

        private void UpdateCropBounds()
        {
            tbLeft.Maximum = tbRight.Maximum = workingImage.Width;
            tbTop.Maximum = tbBottom.Maximum = workingImage.Height;

            tbLeft.Value = tbTop.Value = 0;
            tbRight.Value = workingImage.Width;
            tbTop.Value = workingImage.Height;
        }

        private void UpdateTransform()
        {
            CropTransform = new CropTransform
            {
                Left = Math.Min(tbLeft.Value, tbRight.Value),
                Right = workingImage.Width - Math.Max(tbLeft.Value, tbRight.Value),
                Bottom = Math.Min(tbTop.Value, tbBottom.Value),
                Top = workingImage.Height - Math.Max(tbTop.Value, tbBottom.Value),
                OriginalHeight = workingImage.Height,
                OriginalWidth = workingImage.Width
            };
            UpdatePreviewBox();
        }

        private void tbLeft_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void tbRight_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void tbBottom_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void tbTop_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            dragStartCoords = TranslatePboxCoords(e.Location);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var dragEndCoords = TranslatePboxCoords(e.Location);
                if (dragEndCoords.X > dragStartCoords.X)
                {
                    tbLeft.Value = dragStartCoords.X;
                    tbRight.Value = dragEndCoords.X;
                }
                else
                {
                    tbLeft.Value = dragEndCoords.X;
                    tbRight.Value = dragStartCoords.X;
                }
                if (dragEndCoords.Y > dragStartCoords.Y)
                {
                    tbTop.Value = workingImage.Height - dragStartCoords.Y;
                    tbBottom.Value = workingImage.Height - dragEndCoords.Y;
                }
                else
                {
                    tbTop.Value = workingImage.Height - dragEndCoords.Y;
                    tbBottom.Value = workingImage.Height - dragStartCoords.Y;
                }
                UpdateTransform();
            }
        }

        private Point TranslatePboxCoords(Point point)
        {
            double px = point.X - 1;
            double py = point.Y - 1;
            double imageAspect = workingImage.Width / (double)workingImage.Height;
            double pboxWidth = (pictureBox.Width - 2);
            double pboxHeight = (pictureBox.Height - 2);
            double pboxAspect = pboxWidth / pboxHeight;
            if (pboxAspect > imageAspect)
            {
                // Empty space on left/right
                var emptyWidth = ((1 - imageAspect / pboxAspect) / 2 * pboxWidth);
                px = (pboxAspect / imageAspect * (px - emptyWidth));
            }
            else
            {
                // Empty space on top/bottom
                var emptyHeight = ((1 - pboxAspect / imageAspect) / 2 * pboxHeight);
                py = (imageAspect / pboxAspect * (py - emptyHeight));
            }
            double x = px / pboxWidth * workingImage.Width;
            double y = py / pboxHeight * workingImage.Height;
            x = Math.Max(Math.Min(x, workingImage.Width), 0);
            y = Math.Max(Math.Min(y, workingImage.Height), 0);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}
