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
    partial class FSharpen : FormBase
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
            if (SelectedImages != null && SelectedImages.Count > 1)
            {
                checkboxApplyToSelected.Text = string.Format(checkboxApplyToSelected.Text, SelectedImages.Count);
            }
            else
            {
                ConditionalControls.Hide(checkboxApplyToSelected, 6);
            }

            new LayoutManager(this)
                .Bind(tbSharpen, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtSharpen)
                    .RightToForm()
                .Bind(tbSharpen, txtSharpen, checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = txtSharpen;
        }

        private void UpdateTransform()
        {
            SharpenTransform.Sharpness = tbSharpen.Value;
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

        private void btnRevert_Click(object sender, EventArgs e)
        {
            SharpenTransform = new SharpenTransform();
            tbSharpen.Value = 0;
            txtSharpen.Text = tbSharpen.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FSharpen_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            pictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void txtSharpen_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtSharpen.Text, out value))
            {
                if (value >= tbSharpen.Minimum && value <= tbSharpen.Maximum)
                {
                    tbSharpen.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbSharpen_Scroll(object sender, EventArgs e)
        {
            txtSharpen.Text = tbSharpen.Value.ToString("G");
            UpdateTransform();
        }
    }
}
