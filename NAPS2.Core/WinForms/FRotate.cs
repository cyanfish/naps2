/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

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

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FRotate(ChangeTracker changeTracker, ThumbnailRenderer thumbnailRenderer)
        {
            this.changeTracker = changeTracker;
            this.thumbnailRenderer = thumbnailRenderer;
            InitializeComponent();

            RotationTransform = new RotationTransform();
        }

        public ScannedImage Image { get; set; }

        public List<ScannedImage> SelectedImages { get; set; }

        public RotationTransform RotationTransform { get; private set; }

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
                .Bind(tbAngle, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtAngle)
                    .RightToForm()
                .Bind(tbAngle, txtAngle, checkboxApplyToSelected, btnRevert, btnOK, btnCancel, btnAutoDeskew)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = Image.GetImage();
            pictureBox.Image = (Bitmap)workingImage.Clone();
            txtAngle.Text += '\u00B0';
            UpdatePreviewBox();
        }

        private void UpdateTransform()
        {
            RotationTransform.Angle = tbAngle.Value / 100.0;
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
                        Invoke(new MethodInvoker(() =>
                        {
                            if (pictureBox.Image != null)
                            {
                                pictureBox.Image.Dispose();
                            }
                            pictureBox.Image = result;
                        }));
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
            txtAngle.Text = (tbAngle.Value / 100.0).ToString("G");
            UpdatePreviewBox();
        }

        private void FRotate_FormClosed(object sender, FormClosedEventArgs e)
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

        private void txtAngle_TextChanged(object sender, EventArgs e)
        {
            double valueDouble;
            if (double.TryParse(txtAngle.Text.Replace('\u00B0'.ToString(CultureInfo.InvariantCulture), ""), out valueDouble))
            {
                int value = (int)Math.Round(valueDouble * 100);
                if (value >= tbAngle.Minimum && value <= tbAngle.Maximum)
                {
                    tbAngle.Value = value;
                }
                if (!txtAngle.Text.Contains('\u00B0'))
                {
                    txtAngle.Text += '\u00B0';
                }
            }
            UpdateTransform();
        }

        private void tbAngle_Scroll(object sender, EventArgs e)
        {
            txtAngle.Text = (tbAngle.Value / 100.0).ToString("G") + '\u00B0';
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
                tbAngle.Value = (int)Math.Round(newAngle * 100);
                tbAngle_Scroll(null, null);
            }
            pictureBox.Invalidate();
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            guideEnd = e.Location;
            pictureBox.Invalidate();
        }

        private void btnAutoDeskew_Click(object sender, EventArgs e)
        {
            tbAngle.Value = Convert.ToInt32(Math.Round(Image.GetImage().GetSkewAngle() * -100));
            tbAngle_Scroll(null, null);
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
