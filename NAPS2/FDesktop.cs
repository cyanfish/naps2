/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Email;
using NAPS2.Scan;
using Ninject;
using Ninject.Parameters;

namespace NAPS2
{
    public partial class FDesktop : Form, IScanReceiver
    {
        private readonly IEmailer emailer;
        private readonly ImageSaver imageSaver;
        private readonly ScannedImageList imageList = new ScannedImageList();

        public FDesktop(IEmailer emailer, ImageSaver imageSaver)
        {
            InitializeComponent();
            this.emailer = emailer;
            this.imageSaver = imageSaver;
        }

        private IEnumerable<int> SelectedIndices
        {
            get
            {
                return thumbnailList1.SelectedIndices.OfType<int>();
            }
            set
            {
                thumbnailList1.SelectedIndices.Clear();
                foreach (int i in value)
                {
                    thumbnailList1.SelectedIndices.Add(i);
                }
            }
        }

        public void ReceiveScannedImage(IScannedImage scannedImage)
        {
            imageList.Images.Add(scannedImage);
            UpdateThumbnails();
        }

        private void UpdateThumbnails()
        {
            thumbnailList1.UpdateImages(imageList.Images);
        }

        private void UpdateThumbnails(IEnumerable<int> selection)
        {
            UpdateThumbnails();
            SelectedIndices = selection;
        }

        private void Clear()
        {
            if (imageList.Images.Count > 0)
            {
                if (MessageBox.Show(string.Format("Are you sure you want to clear {0} item(s)?", imageList.Images.Count), "Clear", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    imageList.Delete(Enumerable.Range(0, imageList.Images.Count));
                    UpdateThumbnails();
                }
            }
        }

        private void Delete()
        {
            if (SelectedIndices.Any())
            {
                if (MessageBox.Show(string.Format("Are you sure you want to delete {0} item(s)?", SelectedIndices.Count()), "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    imageList.Delete(SelectedIndices);
                    UpdateThumbnails();
                }
            }
        }

        private void SelectAll()
        {
            UpdateThumbnails(Enumerable.Range(0, imageList.Images.Count));
        }

        private void MoveDown()
        {
            UpdateThumbnails(imageList.MoveDown(SelectedIndices));
        }

        private void MoveUp()
        {
            UpdateThumbnails(imageList.MoveUp(SelectedIndices));
        }

        private void RotateLeft()
        {
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate270FlipNone));
        }

        private void RotateRight()
        {
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate90FlipNone));
        }

        private void Flip()
        {
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.RotateNoneFlipXY));
        }

        private void exportPDF(string filename)
        {
            var pdfdialog = KernelManager.Kernel.Get<FPDFSave>();
            pdfdialog.Filename = filename;
            pdfdialog.Images = imageList.Images;
            pdfdialog.ShowDialog(this);
        }

        private void thumbnailList1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    Delete();
                    break;
                case Keys.Left:
                    if (e.Control)
                    {
                        MoveUp();
                    }
                    break;
                case Keys.Right:
                    if (e.Control)
                    {
                        MoveDown();
                    }
                    break;
                case Keys.A:
                    if (e.Control)
                    {
                        SelectAll();
                    }
                    break;
            }
        }

        private void thumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var viewer = new FViewer(imageList.Images[SelectedIndices.First()].GetImage());
                viewer.ShowDialog();
            }
        }

        private void tsScan_Click(object sender, EventArgs e)
        {
            var prof = KernelManager.Kernel.Get<FChooseProfile>(new ConstructorArgument("scanReceiver", this));
            prof.ShowDialog();
        }

        private void tsSavePDF_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count > 0)
            {
                var sd = new SaveFileDialog
                    {
                        OverwritePrompt = true,
                        AddExtension = true,
                        Filter = "PDF document (*.pdf)|*.pdf"
                    };

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    exportPDF(sd.FileName);
                }
            }
        }

        private void tsSaveImage_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count > 0)
            {
                var sd = new SaveFileDialog
                    {
                        OverwritePrompt = true,
                        AddExtension = true,
                        Filter = "Bitmap Files (*.bmp)|*.bmp" +
                                 "|Enhanced Windows MetaFile (*.emf)|*.emf" +
                                 "|Exchangeable Image File (*.exif)|*.exif" +
                                 "|GIF File (*.gif)|*.gif" +
                                 "|JPEG File (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                                 "|PNG File (*.png)|*.png" +
                                 "|TIFF File (*.tiff, *.tif)|*.tiff;*.tif",
                        DefaultExt = "jpg",
                        FilterIndex = 5
                    };

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        imageSaver.SaveImages(sd.FileName, imageList.Images, path =>
                        {
                            if (imageList.Images.Count == 1)
                            {
                                // One image, so the file name is the same and the save dialog already prompted the user to overwrite
                                return true;
                            }
                            switch (
                                MessageBox.Show(
                                    string.Format("The file {0} already exists. Do you want to overwrite it?", path),
                                    "Overwrite File", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning))
                            {
                                case DialogResult.Yes:
                                    return true;
                                case DialogResult.No:
                                    return false;
                                default:
                                    throw new InvalidOperationException("User cancelled");
                            }
                        });
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
        }

        private void tsPDFEmail_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count > 0)
            {
                string path = Paths.AppData + "\\Scan.pdf";
                exportPDF(path);
                emailer.SendEmail(path, "");
                File.Delete(path);
            }
        }

        private void tsFlip_Click(object sender, EventArgs e)
        {
            Flip();
        }

        private void tsClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void tsDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void tsMoveUp_Click(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void tsMoveDown_Click(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void tsRotateLeft_Click(object sender, EventArgs e)
        {
            RotateLeft();
        }

        private void tsRotateRight_Click(object sender, EventArgs e)
        {
            RotateRight();
        }

        private void tsProfiles_Click(object sender, EventArgs e)
        {
            KernelManager.Kernel.Get<FManageProfiles>().ShowDialog();
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            new FAbout().ShowDialog();
        }

        private void tsLangEnglish_Click(object sender, EventArgs e)
        {
            SetCulture("en-US");
        }

        private void tsLangFrench_Click(object sender, EventArgs e)
        {
            SetCulture("fr-FR");
        }

        private void tsLangSpanish_Click(object sender, EventArgs e)
        {
            SetCulture("es-ES");
        }

        private static void SetCulture(string cultureId)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureId);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureId);
        }
    }
}
