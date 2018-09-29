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
        private LayoutManager lm;

        private int originalWidth, originalHeight;

        public FCrop(ChangeTracker changeTracker, ScannedImageRenderer scannedImageRenderer)
            : base(changeTracker, scannedImageRenderer)
        {
            InitializeComponent();

            lm = new LayoutManager(this)
                .Bind(tbLeft, tbRight)
                    .WidthTo(() => (int)(GetImageWidthRatio() * pictureBox.Width))
                    .LeftTo(() => (int)((1 - GetImageWidthRatio()) * pictureBox.Width / 2))
                .Bind(tbTop, tbBottom)
                    .HeightTo(() => (int)(GetImageHeightRatio() * pictureBox.Height))
                    .TopTo(() => (int)((1 - GetImageHeightRatio()) * pictureBox.Height / 2))
                .Activate();

            originalWidth = originalHeight = 1000;
        }

        public CropTransform CropTransform { get; private set; }

        protected override IEnumerable<Transform> Transforms => new[] { CropTransform };

        protected override PictureBox PictureBox => pictureBox;

        protected override Bitmap RenderPreview()
        {
            var bitmap = new Bitmap(originalWidth, originalHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.5f });
                g.DrawImage(workingImage2,
                    new Rectangle(0, 0, originalWidth, originalHeight),
                    0,
                    0,
                    originalWidth,
                    originalHeight,
                    GraphicsUnit.Pixel,
                    attrs);
                var cropBorderRect = new Rectangle(CropTransform.Left, CropTransform.Top,
                    originalWidth - CropTransform.Left - CropTransform.Right,
                    originalHeight - CropTransform.Top - CropTransform.Bottom);
                g.SetClip(cropBorderRect);
                g.DrawImage(workingImage2, new Rectangle(0, 0, originalWidth, originalHeight));
                g.ResetClip();
                g.DrawRectangle(new Pen(Color.Black, 2.0f), cropBorderRect);
            }

            return bitmap;
        }

        protected override void InitTransform()
        {
            originalWidth = workingImage.Width;
            originalHeight = workingImage.Height;
            if (_lastTransform != null && _lastSize == workingImage.Size)
            {
                CropTransform = _lastTransform;
            }
            else
            {
                ResetTransform();
            }
            UpdateCropBounds();
            lm.UpdateLayout();
        }

        protected override void ResetTransform()
        {
            CropTransform = new CropTransform
            {
                OriginalHeight = originalHeight,
                OriginalWidth = originalWidth
            };
        }

        protected override void TransformSaved()
        {
            _lastTransform = CropTransform;
            _lastSize = workingImage.Size;
        }

        private double GetImageWidthRatio()
        {
            if (workingImage == null)
            {
                return 1;
            }
            double imageAspect = originalWidth / (double)originalHeight;
            double pboxAspect = pictureBox.Width / (double)pictureBox.Height;
            if (imageAspect > pboxAspect)
            {
                return 1;
            }
            return imageAspect / pboxAspect;
        }

        private double GetImageHeightRatio()
        {
            if (workingImage == null)
            {
                return 1;
            }
            double imageAspect = originalWidth / (double)originalHeight;
            double pboxAspect = pictureBox.Width / (double)pictureBox.Height;
            if (pboxAspect > imageAspect)
            {
                return 1;
            }
            return pboxAspect / imageAspect;
        }

        private void UpdateCropBounds()
        {
            tbLeft.Maximum = tbRight.Maximum = originalWidth;
            tbTop.Maximum = tbBottom.Maximum = originalHeight;

            tbLeft.Value = CropTransform.Left;
            tbBottom.Value = CropTransform.Bottom;
            tbRight.Value = originalWidth - CropTransform.Right;
            tbTop.Value = originalHeight - CropTransform.Top;
        }

        private void UpdateTransform()
        {
            CropTransform = new CropTransform
            {
                Left = Math.Min(tbLeft.Value, tbRight.Value),
                Right = originalWidth - Math.Max(tbLeft.Value, tbRight.Value),
                Bottom = Math.Min(tbTop.Value, tbBottom.Value),
                Top = originalHeight - Math.Max(tbTop.Value, tbBottom.Value),
                OriginalHeight = originalHeight,
                OriginalWidth = originalWidth
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
                    tbTop.Value = originalHeight - dragStartCoords.Y;
                    tbBottom.Value = originalHeight - dragEndCoords.Y;
                }
                else
                {
                    tbTop.Value = originalHeight - dragEndCoords.Y;
                    tbBottom.Value = originalHeight - dragStartCoords.Y;
                }
                UpdateTransform();
            }
        }

        private Point TranslatePboxCoords(Point point)
        {
            double px = point.X - 1;
            double py = point.Y - 1;
            double imageAspect = originalWidth / (double)originalHeight;
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
            double x = px / pboxWidth * originalWidth;
            double y = py / pboxHeight * originalHeight;
            x = Math.Max(Math.Min(x, originalWidth), 0);
            y = Math.Max(Math.Min(y, originalHeight), 0);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}
