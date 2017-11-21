using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    partial class FRotate : FormBase
    {
        private readonly ChangeTracker changeTracker;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FRotate(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            RotationTransform = new RotationTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public RotationTransform RotationTransform { get; private set; }

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
                .Bind(tbAngle, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtAngle)
                    .RightToForm()
                .Bind(tbAngle, txtAngle, checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = scannedImageRenderer.Render(Image);
            pictureBox.Image = (Bitmap)workingImage.Clone();
            txtAngle.Text += '\u00B0';
            UpdatePreviewBox();

            ActiveControl = txtAngle;
        }

        private void UpdateTransform()
        {
            RotationTransform.Angle = tbAngle.Value / 10.0;
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
                        var result = RotationTransform.Perform((Bitmap)workingImage.Clone());
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
            if (!RotationTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(RotationTransform);
                    img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            RotationTransform = new RotationTransform();
            tbAngle.Value = 0;
            txtAngle.Text = (tbAngle.Value / 10.0).ToString("G");
            UpdatePreviewBox();
        }

        private void FRotate_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            pictureBox.Image?.Dispose();
            previewTimer?.Dispose();
        }

        private void txtAngle_TextChanged(object sender, EventArgs e)
        {
            double valueDouble;
            if (double.TryParse(txtAngle.Text.Replace('\u00B0'.ToString(CultureInfo.InvariantCulture), ""), out valueDouble))
            {
                int value = (int)Math.Round(valueDouble * 10);
                if (value >= tbAngle.Minimum && value <= tbAngle.Maximum)
                {
                    tbAngle.Value = value;
                }
                if (!txtAngle.Text.Contains('\u00B0'))
                {
                    var (ss, sl) = (txtAngle.SelectionStart, txtAngle.SelectionLength);
                    txtAngle.Text += '\u00B0';
                    (txtAngle.SelectionStart, txtAngle.SelectionLength) = (ss, sl);
                }
            }
            UpdateTransform();
        }

        private void tbAngle_Scroll(object sender, EventArgs e)
        {
            txtAngle.Text = (tbAngle.Value / 10.0).ToString("G") + '\u00B0';
            UpdateTransform();
        }

        private bool guideExists;
        private Point guideStart, guideEnd;

        private const int MIN_LINE_DISTANCE = 50;
        private const float LINE_PEN_SIZE = 1;

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            guideExists = true;
            guideStart = guideEnd = e.Location;
            pictureBox.Invalidate();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            guideExists = false;
            var dx = guideEnd.X - guideStart.X;
            var dy = guideEnd.Y - guideStart.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance > MIN_LINE_DISTANCE)
            {
                var angle = -Math.Atan2(dy, dx) * 180.0 / Math.PI;
                while (angle > 45.0)
                {
                    angle -= 90.0;
                }
                while (angle < -45.0)
                {
                    angle += 90.0;
                }
                var oldAngle = tbAngle.Value / 10.0;
                var newAngle = angle + oldAngle;
                while (newAngle > 180.0)
                {
                    newAngle -= 360.0;
                }
                while (newAngle < -180.0)
                {
                    newAngle += 360.0;
                }
                tbAngle.Value = (int)Math.Round(newAngle * 10);
                tbAngle_Scroll(null, null);
            }
            pictureBox.Invalidate();
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            guideEnd = e.Location;
            pictureBox.Invalidate();
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (guideExists)
            {
                var old = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawLine(new Pen(Color.Black, LINE_PEN_SIZE), guideStart, guideEnd);
                e.Graphics.SmoothingMode = old;
            }
        }
    }
}
