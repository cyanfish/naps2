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
    partial class FContrast : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FContrast(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            ContrastTransform = new TrueContrastTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public TrueContrastTransform ContrastTransform { get; private set; }

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
                .Bind(tbContrast, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtContrast)
                    .RightToForm()
                .Bind(tbContrast, txtContrast, checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();
        }

        private void UpdateTransform()
        {
            ContrastTransform.Contrast = tbContrast.Value;
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
                        var result = ContrastTransform.Perform((Bitmap)workingImage.Clone());
                        try
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                pictureBox.Image?.Dispose();
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
            if (!ContrastTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(ContrastTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            ContrastTransform = new TrueContrastTransform();
            tbContrast.Value = 0;
            txtContrast.Text = tbContrast.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            pictureBox.Image?.Dispose();
            previewTimer?.Dispose();
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
