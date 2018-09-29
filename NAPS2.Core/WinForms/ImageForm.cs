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
    partial class ImageForm : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ScannedImageRenderer scannedImageRenderer;

        protected Bitmap workingImage, workingImage2;
        private bool initComplete;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;
        private bool closed;

        private ImageForm()
        {
            // For the designer only
            InitializeComponent();
        }

        protected ImageForm(ChangeTracker changeTracker, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        protected virtual IEnumerable<Transform> Transforms => throw new NotImplementedException();

        protected virtual PictureBox PictureBox => throw new NotImplementedException();

        private bool TransformMultiple => SelectedImages != null && checkboxApplyToSelected.Checked;

        private IEnumerable<ScannedImage> ImagesToTransform => TransformMultiple ? SelectedImages : Enumerable.Repeat(Image, 1);

        protected virtual Bitmap RenderPreview()
        {
            var result = (Bitmap)workingImage.Clone();
            foreach (var transform in Transforms)
            {
                if (!transform.IsNull)
                {
                    result = transform.Perform(result);
                }
            }
            return result;
        }

        protected virtual void InitTransform()
        {
        }

        protected virtual void ResetTransform()
        {
        }

        protected virtual void TransformSaved()
        {
        }

        private async void ImageForm_Load(object sender, EventArgs e)
        {
            checkboxApplyToSelected.BringToFront();
            btnRevert.BringToFront();
            btnCancel.BringToFront();
            btnOK.BringToFront();
            if (SelectedImages != null && SelectedImages.Count > 1)
            {
                checkboxApplyToSelected.Text = string.Format(checkboxApplyToSelected.Text, SelectedImages.Count);
            }
            else
            {
                ConditionalControls.Hide(checkboxApplyToSelected, 6);
            }

            Size = new Size(600, 600);

            var maxDimen = Screen.AllScreens.Max(s => Math.Max(s.WorkingArea.Height, s.WorkingArea.Width));
            workingImage = await scannedImageRenderer.Render(Image, maxDimen * 2);
            if (closed)
            {
                workingImage?.Dispose();
                return;
            }
            workingImage2 = (Bitmap)workingImage.Clone();

            InitTransform();
            lock (this)
            {
                initComplete = true;
            }

            UpdatePreviewBox();
        }

        protected void UpdatePreviewBox()
        {
            if (previewTimer == null)
            {
                previewTimer = new Timer(_ =>
                {
                    lock (this)
                    {
                        if (!initComplete || !IsHandleCreated || !previewOutOfDate || working) return;
                        working = true;
                        previewOutOfDate = false;
                    }
                    var bitmap = RenderPreview();
                    SafeInvoke(() =>
                    {
                        PictureBox.Image?.Dispose();
                        PictureBox.Image = bitmap;
                    });
                    lock (this)
                    {
                        working = false;
                    }
                }, null, 0, 100);
            }
            lock (this)
            {
                previewOutOfDate = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Transforms.Any(x => !x.IsNull))
            {
                foreach (var img in ImagesToTransform)
                {
                    lock (img)
                    {
                        foreach (var t in Transforms)
                        {
                            img.AddTransform(t);
                        }
                    }
                }
                changeTracker.Made();
            }
            TransformSaved();
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            ResetTransform();
            UpdatePreviewBox();
        }

        private void ImageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage?.Dispose();
            workingImage2?.Dispose();
            PictureBox.Image?.Dispose();
            previewTimer?.Dispose();
            closed = true;
        }
    }
}
