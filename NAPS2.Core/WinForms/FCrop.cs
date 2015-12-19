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
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    partial class FCrop : FormBase
    {
        private readonly ChangeTracker changeTracker;

        private Bitmap workingImage, workingImage2;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FCrop(ChangeTracker changeTracker)
        {
            this.changeTracker = changeTracker;
            InitializeComponent();

            CropTransform = new CropTransform();
        }

        public IScannedImage Image { get; set; }

        public List<IScannedImage> SelectedImages { get; set; }

        public CropTransform CropTransform { get; private set; }

        private IEnumerable<IScannedImage> ImagesToTransform
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
                .Bind(tbLeft, tbRight, pictureBox)
                    .WidthToForm()
                .Bind(tbTop, tbBottom, pictureBox)
                    .HeightToForm()
                .Bind(tbBottom, btnOK, btnCancel)
                    .RightToForm()
                .Bind(tbRight, checkboxApplyToSelected, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = Image.GetImage();
            workingImage2 = Image.GetImage();
            UpdateCropBounds();
            UpdatePreviewBox();
        }

        private void UpdateCropBounds()
        {
            tbLeft.Maximum = tbRight.Maximum = workingImage.Width;
            tbTop.Maximum = tbBottom.Maximum = workingImage.Height;

            tbLeft.Value = tbTop.Value = 0;
            tbRight.Value = workingImage.Width;
            tbTop.Value = workingImage.Height;
        }

        private void UpdateTransform()
        {
            CropTransform.Left = Math.Min(tbLeft.Value, tbRight.Value);
            CropTransform.Right = workingImage.Width - Math.Max(tbLeft.Value, tbRight.Value);
            CropTransform.Bottom = Math.Min(tbTop.Value, tbBottom.Value);
            CropTransform.Top = workingImage.Height - Math.Max(tbTop.Value, tbBottom.Value);
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
                        var bitmap = new Bitmap(workingImage2.Width, workingImage2.Height);
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.Clear(Color.Transparent);
                            var attrs = new ImageAttributes();
                            attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.5f });
                            g.DrawImage(workingImage2,
                                new Rectangle(0, 0, workingImage2.Width, workingImage2.Height),
                                0,
                                0,
                                workingImage2.Width,
                                workingImage2.Height,
                                GraphicsUnit.Pixel,
                                attrs);
                            var cropBorderRect = new Rectangle(CropTransform.Left, CropTransform.Top,
                                workingImage2.Width - CropTransform.Left - CropTransform.Right,
                                workingImage2.Height - CropTransform.Top - CropTransform.Bottom);
                            g.SetClip(cropBorderRect);
                            g.DrawImage(workingImage2, new Rectangle(0, 0, workingImage2.Width, workingImage2.Height));
                            g.ResetClip();
                            g.DrawRectangle(new Pen(Color.Black, 2.0f), cropBorderRect);
                        }
                        Invoke(new MethodInvoker(() =>
                        {
                            if (pictureBox.Image != null)
                            {
                                pictureBox.Image.Dispose();
                            }
                            pictureBox.Image = bitmap;
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
            if (!CropTransform.IsNull)
            {
                foreach (var img in ImagesToTransform)
                {
                    img.AddTransform(CropTransform);
                    img.SetThumbnail(img.RenderThumbnail(UserConfigManager.Config.ThumbnailSize));
                }
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            CropTransform = new CropTransform();
            UpdatePreviewBox();
        }

        private void tbLeft_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void tbRight_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void tbBottom_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void tbTop_Scroll(object sender, EventArgs e)
        {
            UpdateTransform();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            workingImage2.Dispose();
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            if (previewTimer != null)
            {
                previewTimer.Dispose();
            }
        }
    }
}
