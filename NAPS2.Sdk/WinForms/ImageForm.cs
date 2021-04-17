using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    partial class ImageForm : FormBase
    {
        private readonly ImageContext _imageContext;
        private readonly BitmapRenderer _bitmapRenderer;

        protected Bitmap workingImage, workingImage2;
        private bool _initComplete;
        private bool _previewOutOfDate;
        private bool _working;
        private Timer _previewTimer;
        private bool _closed;

        private ImageForm()
        {
            // For the designer only
            InitializeComponent();
        }

        protected ImageForm(ImageContext imageContext, BitmapRenderer bitmapRenderer)
        {
            _imageContext = imageContext;
            _bitmapRenderer = bitmapRenderer;
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
                    // TODO: Maybe the working images etc. should be storage
                    result = ((GdiImage)_imageContext.PerformTransform(new GdiImage(result), transform)).Bitmap;
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
            workingImage = await _bitmapRenderer.Render(Image, maxDimen * 2);
            if (_closed)
            {
                workingImage?.Dispose();
                return;
            }
            workingImage2 = (Bitmap)workingImage.Clone();

            InitTransform();
            lock (this)
            {
                _initComplete = true;
            }

            UpdatePreviewBox();
        }

        protected void UpdatePreviewBox()
        {
            if (_previewTimer == null)
            {
                _previewTimer = new Timer(_ =>
                {
                    lock (this)
                    {
                        if (!_initComplete || !IsHandleCreated || !_previewOutOfDate || _working) return;
                        _working = true;
                        _previewOutOfDate = false;
                    }
                    var bitmap = RenderPreview();
                    SafeInvoke(() =>
                    {
                        PictureBox.Image?.Dispose();
                        PictureBox.Image = bitmap;
                    });
                    lock (this)
                    {
                        _working = false;
                    }
                }, null, 0, 100);
            }
            lock (this)
            {
                _previewOutOfDate = true;
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
                        // Optimize thumbnail rendering for the first (or only) image since we already have it loaded into memory
                        if (img == Image)
                        {
                            var transformed = _imageContext.PerformAllTransforms(new GdiImage(workingImage).Clone(), Transforms);
                            img.SetThumbnail(_imageContext.PerformTransform(transformed, new ThumbnailTransform(Config.Get(c => c.ThumbnailSize))));
                        }
                    }
                }
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
            _previewTimer?.Dispose();
            _closed = true;
        }
    }
}
