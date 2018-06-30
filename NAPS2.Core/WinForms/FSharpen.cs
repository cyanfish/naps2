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
    internal partial class FSharpen : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FSharpen(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            SharpenTransform = new SharpenTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public SharpenTransform SharpenTransform { get; private set; }

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
                .Bind(TbSharpen, PictureBox)
                    .WidthToForm()
                .Bind(PictureBox)
                    .HeightToForm()
                .Bind(BtnOK, BtnCancel, TxtSharpen)
                    .RightToForm()
                .Bind(TbSharpen, TxtSharpen, checkboxApplyToSelected, BtnRevert, BtnOK, BtnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            PictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = TxtSharpen;
        }

        private void UpdateTransform()
        {
            SharpenTransform.Sharpness = TbSharpen.Value;
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
                        var result = SharpenTransform.Perform((Bitmap)workingImage.Clone());
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
            if (!SharpenTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(SharpenTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void BtnRevert_Click(object sender, EventArgs e)
        {
            SharpenTransform = new SharpenTransform();
            TbSharpen.Value = 0;
            TxtSharpen.Text = TbSharpen.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FSharpen_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            PictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void TxtSharpen_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtSharpen.Text, out int value))
            {
                if (value >= TbSharpen.Minimum && value <= TbSharpen.Maximum)
                {
                    TbSharpen.Value = value;
                }
            }
            UpdateTransform();
        }

        private void TbSharpen_Scroll(object sender, EventArgs e)
        {
            TxtSharpen.Text = TbSharpen.Value.ToString("G");
            UpdateTransform();
        }
    }
}