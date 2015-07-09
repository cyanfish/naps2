/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.WinForms
{
    partial class FBrightness : FormBase
    {
        private Bitmap workingImage;

        private object taskLock = new object();
        private Task updatePreviewTask;
        private bool previewOutOfDate;

        public FBrightness()
        {
            InitializeComponent();

            BrightnessTransform = new BrightnessTransform();
        }

        public IScannedImage Image { get; set; }

        public BrightnessTransform BrightnessTransform { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(tbBrightness, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel)
                    .RightToForm()
                .Bind(tbBrightness, txtBrightness, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();

            workingImage = Image.GetImage();
            UpdatePreviewBox();
        }

        private void UpdateTransform()
        {
            BrightnessTransform.Brightness = tbBrightness.Value;
            UpdatePreviewBox();
        }

        private void UpdatePreviewBox()
        {
            lock (taskLock)
            {
                if (updatePreviewTask != null && !updatePreviewTask.IsCompleted)
                {
                    previewOutOfDate = true;
                    return;
                }
            }
            updatePreviewTask = Task.Factory.StartNew(() =>
            {
                var result = BrightnessTransform.Perform((Bitmap)workingImage.Clone());
                Invoke(new MethodInvoker(() =>
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }
                    pictureBox.Image = result;
                }));
            }).ContinueWith(task =>
            {
                lock (taskLock)
                {
                    if (!previewOutOfDate)
                    {
                        return;
                    }
                    previewOutOfDate = false;
                }
                var result = BrightnessTransform.Perform((Bitmap)workingImage.Clone());
                Invoke(new MethodInvoker(() =>
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }
                    pictureBox.Image = result;
                }));
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Image.AddTransform(BrightnessTransform);
            Image.UpdateThumbnail();
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            BrightnessTransform = new BrightnessTransform();
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
        }

        private void txtBrightness_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtBrightness.Text, out value))
            {
                if (value >= tbBrightness.Minimum && value <= tbBrightness.Maximum)
                {
                    tbBrightness.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbBrightness_Scroll(object sender, EventArgs e)
        {
            txtBrightness.Text = tbBrightness.Value.ToString("G");
            UpdateTransform();
        }
    }
}
