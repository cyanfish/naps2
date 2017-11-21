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
    partial class FBlackWhite : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FBlackWhite(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            BlackWhiteTransform = new BlackWhiteTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public BlackWhiteTransform BlackWhiteTransform { get; private set; }

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
                .Bind(tbThreshold, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtThreshold)
                    .RightToForm()
                .Bind(tbThreshold, txtThreshold, checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();

            ActiveControl = txtThreshold;
        }

        private void UpdateTransform()
        {
            BlackWhiteTransform.Threshold = tbThreshold.Value;
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
                        var result = BlackWhiteTransform.Perform((Bitmap)workingImage.Clone());
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
            if (!BlackWhiteTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(BlackWhiteTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            BlackWhiteTransform = new BlackWhiteTransform();
            tbThreshold.Value = 0;
            txtThreshold.Text = tbThreshold.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FBlackWhite_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            pictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void txtBlackWhite_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtThreshold.Text, out value))
            {
                if (value >= tbThreshold.Minimum && value <= tbThreshold.Maximum)
                {
                    tbThreshold.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbBlackWhite_Scroll(object sender, EventArgs e)
        {
            txtThreshold.Text = tbThreshold.Value.ToString("G");
            UpdateTransform();
        }
    }
}
