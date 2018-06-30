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
    internal partial class FHueSaturation : FormBase
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
            if (SelectedImages?.Count > 1)
            {
                checkboxApplyToSelected.Text = string.Format(checkboxApplyToSelected.Text, SelectedImages.Count);
            }
            else
            {
                ConditionalControls.Hide(checkboxApplyToSelected, 6);
            }

            new LayoutManager(this)
                .Bind(TbHue, TbSaturation, PictureBox)
                    .WidthToForm()
                .Bind(PictureBox)
                    .HeightToForm()
                .Bind(BtnOK, BtnCancel, TxtHue, TxtSaturation)
                    .RightToForm()
                .Bind(TbHue, TxtHue, TbSaturation, TxtSaturation, PictureBox1, PictureBox2,
                      checkboxApplyToSelected, BtnRevert, BtnOK, BtnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            PictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = TxtHue;
        }

        private void UpdateTransform()
        {
            HueTransform.HueShift = TbHue.Value;
            SaturationTransform.Saturation = TbSaturation.Value;
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
                        if (!HueTransform.IsNull)
                        {
                            result = HueTransform.Perform(result);
                        }
                        if (!SaturationTransform.IsNull)
                        {
                            result = SaturationTransform.Perform(result);
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

        private void BtnRevert_Click(object sender, EventArgs e)
        {
            HueTransform = new HueTransform();
            SaturationTransform = new SaturationTransform();
            TbHue.Value = 0;
            TbSaturation.Value = 0;
            TxtHue.Text = TbHue.Value.ToString("G");
            TxtSaturation.Text = TbSaturation.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            PictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void TxtHue_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtHue.Text, out int value))
            {
                if (value >= TbHue.Minimum && value <= TbHue.Maximum)
                {
                    TbHue.Value = value;
                }
            }
            UpdateTransform();
        }

        private void TbHue_Scroll(object sender, EventArgs e)
        {
            TxtHue.Text = TbHue.Value.ToString("G");
            UpdateTransform();
        }

        private void TxtSaturation_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtSaturation.Text, out int value))
            {
                if (value >= TbSaturation.Minimum && value <= TbSaturation.Maximum)
                {
                    TbSaturation.Value = value;
                }
            }
            UpdateTransform();
        }

        private void TbSaturation_Scroll(object sender, EventArgs e)
        {
            TxtSaturation.Text = TbSaturation.Value.ToString("G");
            UpdateTransform();
        }
    }
}