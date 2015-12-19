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

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FRotate(ChangeTracker changeTracker)
        {
            this.changeTracker = changeTracker;
            InitializeComponent();

            RotationTransform = new RotationTransform();
        }

        public IScannedImage Image { get; set; }

        public List<IScannedImage> SelectedImages { get; set; }

        public RotationTransform RotationTransform { get; private set; }

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

            workingImage = Image.GetImage();
            pictureBox.Image = (Bitmap)workingImage.Clone();
            txtAngle.Text += '\u00B0';
            UpdatePreviewBox();
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
                    img.SetThumbnail(img.RenderThumbnail(UserConfigManager.Config.ThumbnailSize));
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

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
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
                int value = (int)Math.Round(valueDouble * 10);
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
            txtAngle.Text = (tbAngle.Value / 10.0).ToString("G") + '\u00B0';
            UpdateTransform();
        }
    }
}
