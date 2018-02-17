using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    partial class FBrightnessContrast : FormBase
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
            if (SelectedImages != null && SelectedImages.Count > 1)
            {
                checkboxApplyToSelected.Text = string.Format(checkboxApplyToSelected.Text, SelectedImages.Count);
            }
            else
            {
                ConditionalControls.Hide(checkboxApplyToSelected, 6);
            }

            new LayoutManager(this)
                .Bind(tbBrightness, tbContrast, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtBrightness, txtContrast)
                    .RightToForm()
                .Bind(tbBrightness, txtBrightness, tbContrast, txtContrast, pictureBox1, pictureBox2,
                      checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = txtBrightness;
        }

        private void UpdateTransform()
        {
            BrightnessTransform.Brightness = tbBrightness.Value;
            TrueContrastTransform.Contrast = tbContrast.Value;
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
                        var result = (Bitmap) workingImage.Clone();
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
                            pictureBox.Image?.Dispose();
                            pictureBox.Image = result;
                        });
                        working = false;
                    }
                }, null, 0, 100);
            }
            previewOutOfDate = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
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

        private void btnRevert_Click(object sender, EventArgs e)
        {
            BrightnessTransform = new BrightnessTransform();
            TrueContrastTransform = new TrueContrastTransform();
            tbBrightness.Value = 0;
            tbContrast.Value = 0;
            txtBrightness.Text = tbBrightness.Value.ToString("G");
            txtContrast.Text = tbContrast.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            pictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void txtBrightness_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtBrightness.Text, out value))
            {
                if (value >= tbBrightness.Minimum && value <= tbBrightness.Maximum)
                {
                    tbBrightness.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbBrightness_Scroll(object sender, EventArgs e)
        {
            txtBrightness.Text = tbBrightness.Value.ToString("G");
            UpdateTransform();
        }

        private void txtContrast_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtContrast.Text, out value))
            {
                if (value >= tbContrast.Minimum && value <= tbContrast.Maximum)
                {
                    tbContrast.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbContrast_Scroll(object sender, EventArgs e)
        {
            txtContrast.Text = tbContrast.Value.ToString("G");
            UpdateTransform();
        }
    }
}
