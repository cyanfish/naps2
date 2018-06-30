using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    internal partial class FCrop : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage, workingImage2;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FCrop(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            CropTransform = new CropTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public CropTransform CropTransform { get; private set; }

        private bool TransformMultiple => SelectedImages != null && checkboxApplyToSelected.Checked;

        private IEnumerable<ScannedImage> ImagesToTransform => TransformMultiple ? SelectedImages : Enumerable.Repeat(Image, 1);

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            if (SelectedImages?.Count > 1)
            {
                checkboxApplyToSelected.Text = string.Format(checkboxApplyToSelected.Text, SelectedImages.Count);
            }
            else
            {
                ConditionalControls.Hide(checkboxApplyToSelected, 6);
            }

            var lm = new LayoutManager(this)
                .Bind(PictureBox)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(TbLeft, TbRight)
                    .WidthTo(() => (int)(GetImageWidthRatio() * PictureBox.Width))
                    .LeftTo(() => (int)((1 - GetImageWidthRatio()) * PictureBox.Width / 2))
                .Bind(TbTop, TbBottom)
                    .HeightTo(() => (int)(GetImageHeightRatio() * PictureBox.Height))
                    .TopTo(() => (int)((1 - GetImageHeightRatio()) * PictureBox.Height / 2))
                .Bind(TbBottom, BtnOK, BtnCancel)
                    .RightToForm()
                .Bind(TbRight, checkboxApplyToSelected, BtnRevert, BtnOK, BtnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            workingImage2 = scannedImageRenderer.Render(Image);
            UpdateCropBounds();
            UpdatePreviewBox();

            lm.UpdateLayout();
        }

        private double GetImageWidthRatio()
        {
            if (workingImage == null)
            {
                return 1;
            }
            double imageAspect = workingImage.Width / (double)workingImage.Height;
            double pboxAspect = PictureBox.Width / (double)PictureBox.Height;
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
            double imageAspect = workingImage.Width / (double)workingImage.Height;
            double pboxAspect = PictureBox.Width / (double)PictureBox.Height;
            if (pboxAspect > imageAspect)
            {
                return 1;
            }
            return pboxAspect / imageAspect;
        }

        private void UpdateCropBounds()
        {
            TbLeft.Maximum = TbRight.Maximum = workingImage.Width;
            TbTop.Maximum = TbBottom.Maximum = workingImage.Height;

            TbLeft.Value = TbTop.Value = 0;
            TbRight.Value = workingImage.Width;
            TbTop.Value = workingImage.Height;
        }

        private void UpdateTransform()
        {
            CropTransform.Left = Math.Min(TbLeft.Value, TbRight.Value);
            CropTransform.Right = workingImage.Width - Math.Max(TbLeft.Value, TbRight.Value);
            CropTransform.Bottom = Math.Min(TbTop.Value, TbBottom.Value);
            CropTransform.Top = workingImage.Height - Math.Max(TbTop.Value, TbBottom.Value);
            UpdatePreviewBox();
        }

        private void UpdatePreviewBox()
        {
            if (previewTimer == null)
            {
                previewTimer = new Timer((obj) =>
                {
                    if (previewOutOfDate && !working)
                    {
                        working = true;
                        previewOutOfDate = false;
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
                        SafeInvoke(() =>
                        {
                            PictureBox.Image?.Dispose();
                            PictureBox.Image = bitmap;
                        });
                        working = false;
                    }
                }, null, 0, 100);
            }
            previewOutOfDate = true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (!CropTransform.IsNull)
            {
                if (TransformMultiple)
                {
                    // With multiple images, we need to have the transform scaled in case they're different sizes
                    using (var referenceBitmap = scannedImageRenderer.Render(Image))
                    {
                        foreach (var img in ImagesToTransform)
                        {
                            img.AddTransform(ScaleCropTransform(img, referenceBitmap));
                            img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                        }
                    }
                }
                else
                {
                    Image.AddTransform(CropTransform);
                    Image.SetThumbnail(thumbnailRenderer.RenderThumbnail(Image));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private CropTransform ScaleCropTransform(ScannedImage img, Bitmap referenceBitmap)
        {
            using (var bitmap = scannedImageRenderer.Render(img))
            {
                double xScale = bitmap.Width / (double)referenceBitmap.Width,
                       yScale = bitmap.Height / (double)referenceBitmap.Height;
                return new CropTransform
                {
                    Left = (int)Math.Round(CropTransform.Left * xScale),
                    Right = (int)Math.Round(CropTransform.Right * xScale),
                    Top = (int)Math.Round(CropTransform.Top * yScale),
                    Bottom = (int)Math.Round(CropTransform.Bottom * yScale)
                };
            }
        }

        private void BtnRevert_Click(object sender, EventArgs e)
        {
            CropTransform = new CropTransform();
            UpdatePreviewBox();
        }

        private void TbLeft_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void TbRight_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void TbBottom_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void TbTop_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            workingImage2.Dispose();
            PictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private Point dragStartCoords;

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            dragStartCoords = TranslatePboxCoords(e.Location);
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var dragEndCoords = TranslatePboxCoords(e.Location);
                if (dragEndCoords.X > dragStartCoords.X)
                {
                    TbLeft.Value = dragStartCoords.X;
                    TbRight.Value = dragEndCoords.X;
                }
                else
                {
                    TbLeft.Value = dragEndCoords.X;
                    TbRight.Value = dragStartCoords.X;
                }
                if (dragEndCoords.Y > dragStartCoords.Y)
                {
                    TbTop.Value = workingImage.Height - dragStartCoords.Y;
                    TbBottom.Value = workingImage.Height - dragEndCoords.Y;
                }
                else
                {
                    TbTop.Value = workingImage.Height - dragEndCoords.Y;
                    TbBottom.Value = workingImage.Height - dragStartCoords.Y;
                }
                UpdateTransform();
            }
        }

        private Point TranslatePboxCoords(Point point)
        {
            double px = point.X - 1;
            double py = point.Y - 1;
            double imageAspect = workingImage.Width / (double)workingImage.Height;
            double pboxWidth = (PictureBox.Width - 2);
            double pboxHeight = (PictureBox.Height - 2);
            double pboxAspect = pboxWidth / pboxHeight;
            if (pboxAspect > imageAspect)
            {
                // Empty space on left/right
                var emptyWidth = ((1 - (imageAspect / pboxAspect)) / 2 * pboxWidth);
                px = (pboxAspect / imageAspect * (px - emptyWidth));
            }
            else
            {
                // Empty space on top/bottom
                var emptyHeight = ((1 - (pboxAspect / imageAspect)) / 2 * pboxHeight);
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