using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    partial class FCrop : ImageForm
    {
        private static CropTransform _lastTransform;
        private static Size _lastSize;

        private Point _dragStartCoords;
        private LayoutManager _lm;

        private int _originalWidth, _originalHeight;

        public FCrop(ImageContext imageContext)
            : base(imageContext)
        {
            InitializeComponent();

            _lm = new LayoutManager(this)
                .Bind(tbLeft, tbRight)
                    .WidthTo(() => (int)(GetImageWidthRatio() * pictureBox.Width))
                    .LeftTo(() => (int)((1 - GetImageWidthRatio()) * pictureBox.Width / 2))
                .Bind(tbTop, tbBottom)
                    .HeightTo(() => (int)(GetImageHeightRatio() * pictureBox.Height))
                    .TopTo(() => (int)((1 - GetImageHeightRatio()) * pictureBox.Height / 2))
                .Activate();

            _originalWidth = _originalHeight = 1000;
        }

        public CropTransform CropTransform { get; private set; }

        protected override IEnumerable<Transform> Transforms => new[] { CropTransform };

        protected override PictureBox PictureBox => pictureBox;

        protected override Bitmap RenderPreview()
        {
            var bitmap = new Bitmap(_originalWidth, _originalHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.5f });
                g.DrawImage(workingImage2,
                    new Rectangle(0, 0, _originalWidth, _originalHeight),
                    0,
                    0,
                    _originalWidth,
                    _originalHeight,
                    GraphicsUnit.Pixel,
                    attrs);
                var cropBorderRect = new Rectangle(CropTransform.Left, CropTransform.Top,
                    _originalWidth - CropTransform.Left - CropTransform.Right,
                    _originalHeight - CropTransform.Top - CropTransform.Bottom);
                g.SetClip(cropBorderRect);
                g.DrawImage(workingImage2, new Rectangle(0, 0, _originalWidth, _originalHeight));
                g.ResetClip();
                g.DrawRectangle(new Pen(Color.Black, 2.0f), cropBorderRect);
            }

            return bitmap;
        }

        protected override void InitTransform()
        {
            _originalWidth = workingImage.Width;
            _originalHeight = workingImage.Height;
            if (_lastTransform != null && _lastSize == workingImage.Size)
            {
                CropTransform = _lastTransform;
            }
            else
            {
                ResetTransform();
            }
            UpdateCropBounds();
            _lm.UpdateLayout();
        }

        protected override void ResetTransform()
        {
            CropTransform = new CropTransform(0, 0, 0, 0, _originalWidth, _originalHeight);
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
            double imageAspect = _originalWidth / (double)_originalHeight;
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
            double imageAspect = _originalWidth / (double)_originalHeight;
            double pboxAspect = pictureBox.Width / (double)pictureBox.Height;
            if (pboxAspect > imageAspect)
            {
                return 1;
            }
            return pboxAspect / imageAspect;
        }

        private void UpdateCropBounds()
        {
            tbLeft.Maximum = tbRight.Maximum = _originalWidth;
            tbTop.Maximum = tbBottom.Maximum = _originalHeight;

            tbLeft.Value = CropTransform.Left;
            tbRight.Value = _originalWidth - CropTransform.Right;
            tbTop.Value = _originalHeight - CropTransform.Top;
            tbBottom.Value = CropTransform.Bottom;
        }

        private void UpdateTransform()
        {
            CropTransform = new CropTransform
            (
                Math.Min(tbLeft.Value, tbRight.Value),
                _originalWidth - Math.Max(tbLeft.Value, tbRight.Value),
                _originalHeight - Math.Max(tbTop.Value, tbBottom.Value),
                Math.Min(tbTop.Value, tbBottom.Value),
                _originalWidth,
                _originalHeight
            );
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
            _dragStartCoords = TranslatePboxCoords(e.Location);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var dragEndCoords = TranslatePboxCoords(e.Location);
                if (dragEndCoords.X > _dragStartCoords.X)
                {
                    tbLeft.Value = _dragStartCoords.X;
                    tbRight.Value = dragEndCoords.X;
                }
                else
                {
                    tbLeft.Value = dragEndCoords.X;
                    tbRight.Value = _dragStartCoords.X;
                }
                if (dragEndCoords.Y > _dragStartCoords.Y)
                {
                    tbTop.Value = _originalHeight - _dragStartCoords.Y;
                    tbBottom.Value = _originalHeight - dragEndCoords.Y;
                }
                else
                {
                    tbTop.Value = _originalHeight - dragEndCoords.Y;
                    tbBottom.Value = _originalHeight - _dragStartCoords.Y;
                }
                UpdateTransform();
            }
        }

        private Point TranslatePboxCoords(Point point)
        {
            double px = point.X - 1;
            double py = point.Y - 1;
            double imageAspect = _originalWidth / (double)_originalHeight;
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
            double x = px / pboxWidth * _originalWidth;
            double y = py / pboxHeight * _originalHeight;
            x = Math.Max(Math.Min(x, _originalWidth), 0);
            y = Math.Max(Math.Min(y, _originalHeight), 0);
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }
    }
}
