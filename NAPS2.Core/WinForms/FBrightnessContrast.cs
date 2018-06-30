using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    internal partial class FBrightnessContrast : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FBrightnessContrast(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            BrightnessTransform = new BrightnessTransform();
            TrueContrastTransform = new TrueContrastTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public BrightnessTransform BrightnessTransform { get; private set; }

        public TrueContrastTransform TrueContrastTransform { get; private set; }

        private IEnumerable<ScannedImage> ImagesToTransform => SelectedImages != null && checkboxApplyToSelected.Checked ? SelectedImages : Enumerable.Repeat(Image, 1);

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

            new LayoutManager(this)
                .Bind(TbBrightness, TbContrast, PictureBox)
                    .WidthToForm()
                .Bind(PictureBox)
                    .HeightToForm()
                .Bind(BtnOK, BtnCancel, TxtBrightness, TxtContrast)
                    .RightToForm()
                .Bind(TbBrightness, TxtBrightness, TbContrast, TxtContrast, PictureBox1, PictureBox2,
                      checkboxApplyToSelected, BtnRevert, BtnOK, BtnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            PictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = TxtBrightness;
        }

        private void UpdateTransform()
        {
            BrightnessTransform.Brightness = TbBrightness.Value;
            TrueContrastTransform.Contrast = TbContrast.Value;
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
                        var result = (Bitmap)workingImage.Clone();
                        if (!BrightnessTransform.IsNull)
                        {
                            result = BrightnessTransform.Perform(result);
                        }
                        if (!TrueContrastTransform.IsNull)
                        {
                            result = TrueContrastTransform.Perform(result);
                        }
                        SafeInvoke(() =>
                        {
                            PictureBox.Image?.Dispose();
                            PictureBox.Image = result;
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
            if (!BrightnessTransform.IsNull || !TrueContrastTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(BrightnessTransform);
                    img.AddTransform(TrueContrastTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void BtnRevert_Click(object sender, EventArgs e)
        {
            BrightnessTransform = new BrightnessTransform();
            TrueContrastTransform = new TrueContrastTransform();
            TbBrightness.Value = 0;
            TbContrast.Value = 0;
            TxtBrightness.Text = TbBrightness.Value.ToString("G");
            TxtContrast.Text = TbContrast.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            PictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void TxtBrightness_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtBrightness.Text, out int value))
            {
                if (value >= TbBrightness.Minimum && value <= TbBrightness.Maximum)
                {
                    TbBrightness.Value = value;
                }
            }
            UpdateTransform();
        }

        private void TbBrightness_Scroll(object sender, EventArgs e)
        {
            TxtBrightness.Text = TbBrightness.Value.ToString("G");
            UpdateTransform();
        }

        private void TxtContrast_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtContrast.Text, out int value))
            {
                if (value >= TbContrast.Minimum && value <= TbContrast.Maximum)
                {
                    TbContrast.Value = value;
                }
            }
            UpdateTransform();
        }

        private void TbContrast_Scroll(object sender, EventArgs e)
        {
            TxtContrast.Text = TbContrast.Value.ToString("G");
            UpdateTransform();
        }
    }
}