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
    partial class FHueSaturation : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FHueSaturation(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            HueTransform = new HueTransform();
            SaturationTransform = new SaturationTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public HueTransform HueTransform { get; private set; }

        public SaturationTransform SaturationTransform { get; private set; }

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
                .Bind(tbHue, tbSaturation, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtHue, txtSaturation)
                    .RightToForm()
                .Bind(tbHue, txtHue, tbSaturation, txtSaturation, pictureBox1, pictureBox2,
                      checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = txtHue;
        }

        private void UpdateTransform()
        {
            HueTransform.HueShift = tbHue.Value;
            SaturationTransform.Saturation = tbSaturation.Value;
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
                        var result = HueTransform.Perform((Bitmap)workingImage.Clone());
                        result = SaturationTransform.Perform(result);
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
            if (!HueTransform.IsNull || !SaturationTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(HueTransform);
                    img.AddTransform(SaturationTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            HueTransform = new HueTransform();
            SaturationTransform = new SaturationTransform();
            tbHue.Value = 0;
            tbSaturation.Value = 0;
            txtHue.Text = tbHue.Value.ToString("G");
            txtSaturation.Text = tbSaturation.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            pictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void txtHue_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtHue.Text, out value))
            {
                if (value >= tbHue.Minimum && value <= tbHue.Maximum)
                {
                    tbHue.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbHue_Scroll(object sender, EventArgs e)
        {
            txtHue.Text = tbHue.Value.ToString("G");
            UpdateTransform();
        }

        private void txtSaturation_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtSaturation.Text, out value))
            {
                if (value >= tbSaturation.Minimum && value <= tbSaturation.Maximum)
                {
                    tbSaturation.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbSaturation_Scroll(object sender, EventArgs e)
        {
            txtSaturation.Text = tbSaturation.Value.ToString("G");
            UpdateTransform();
        }
    }
}
