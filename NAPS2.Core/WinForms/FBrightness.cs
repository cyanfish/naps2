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
    partial class FBrightness : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FBrightness(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            InitializeComponent();

            BrightnessTransform = new BrightnessTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public BrightnessTransform BrightnessTransform { get; private set; }

        private IEnumerable<ScannedImage> ImagesToTransform
        {
            get
            {
                return SelectedImages != null && checkboxApplyToSelected.Checked ? SelectedImages : Enumerable.Repeat(Image, 1);
            }
        }

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
                .Bind(tbBrightness, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtBrightness)
                    .RightToForm()
                .Bind(tbBrightness, txtBrightness, checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = Image.GetImage();
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();
        }

        private void UpdateTransform()
        {
            BrightnessTransform.Brightness = tbBrightness.Value;
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
                        var result = BrightnessTransform.Perform((Bitmap)workingImage.Clone());
                        try
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                if (pictureBox.Image != null)
                                {
                                    pictureBox.Image.Dispose();
                                }
                                pictureBox.Image = result;
                            }));
                        }
                        catch (ObjectDisposedException)
                        {
                        }
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
            if (!BrightnessTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(BrightnessTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            BrightnessTransform = new BrightnessTransform();
            tbBrightness.Value = 0;
            txtBrightness.Text = tbBrightness.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            if (previewTimer != null)
            {
                previewTimer.Dispose();
            }
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
    }
}
