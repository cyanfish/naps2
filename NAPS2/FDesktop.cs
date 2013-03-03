/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using PdfSharp.Pdf;
using PdfSharp.Drawing;

using NAPS2.Email;
using NAPS2.Scan;

using Ninject;

using WIA;
using System.Drawing.Imaging;

namespace NAPS2
{
    public partial class FDesktop : Form
    {
        private ScannedImageList imageList;
        private readonly IEmailer emailer;

        public FDesktop(IEmailer emailer)
        {
            InitializeComponent();
            this.emailer = emailer;
            imageList = new ScannedImageList();
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

        private void UpdateThumbnails()
        {
            thumbnailList1.UpdateImages(imageList.Images);
        }

        private void UpdateThumbnails(IEnumerable<int> selection)
        {
            UpdateThumbnails();
            SelectedIndices = selection;
        }

        private void Scan(ScanSettings Profile)
        {
            IScanDriver driver = Dependencies.Kernel.Get<IScanDriver>(Profile.Device.DriverName);
            driver.DialogParent = this;
            driver.ScanSettings = Profile;

            try
            {
                var newImages = driver.Scan();
                imageList.Images.AddRange(newImages);
                UpdateThumbnails();
            }
            catch (ScanDriverException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.RotateNoneFlipX));
        }

        private void exportPDF(string filename)
        {
            FPDFSave pdfdialog = Dependencies.Kernel.Get<FPDFSave>();
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
            }
        }

        private void thumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                FViewer viewer = new FViewer(imageList.Images[SelectedIndices.First()].GetImage());
                viewer.ShowDialog();
            }
        }

        private void tsScan_Click(object sender, EventArgs e)
        {
            //demoScan();
            //return;

            FChooseProfile prof = new FChooseProfile();
            prof.ShowDialog();

            if (prof.Profile == null)
                return;

            Scan(prof.Profile);
        }

        private void tsSavePDF_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count > 0)
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.OverwritePrompt = true;
                sd.AddExtension = true;
                sd.Filter = "PDF document (*.pdf)|*.pdf";

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
                SaveFileDialog sd = new SaveFileDialog();
                sd.OverwritePrompt = true;
                sd.AddExtension = true;
                sd.Filter = "Bitmap Files (*.bmp)|*.bmp" +
                "|Enhanced Windows MetaFile (*.emf)|*.emf" +
                "|Exchangeable Image File (*.exif)|*.exif" +
                "|Gif Files (*.gif)|*.gif" +
                "|JPEG Files (*.(*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                "|PNG Files (*.png)|*.png" +
                "|TIFF Files (*.tiff, *.tif)|*.tiff;*.tif";
                sd.DefaultExt = "jpg";
                sd.FilterIndex = 4;

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat format = GetImageFormat(sd.FileName);

                    int i = 0;

                    if (imageList.Images.Count == 1)
                    {
                        using (Bitmap baseImage = imageList.Images[0].GetImage())
                        {
                            baseImage.Save(sd.FileName, format);
                        }
                        return;
                    }

                    if (format == ImageFormat.Tiff)
                    {
                        var bitmaps = imageList.Images.Select(x => x.GetImage()).ToArray();
                        TiffHelper.SaveMultipage(bitmaps, sd.FileName);
                        foreach (Bitmap bitmap in bitmaps)
                        {
                            bitmap.Dispose();
                        }
                        return;
                    }

                    foreach (ScannedImage img in imageList.Images)
                    {
                        string filename = Path.GetDirectoryName(sd.FileName) + "\\" + Path.GetFileNameWithoutExtension(sd.FileName) + i.ToString().PadLeft(3, '0') + Path.GetExtension(sd.FileName);
                        using (Bitmap baseImage = img.GetImage())
                        {
                            baseImage.Save(filename, format);
                        }
                        i++;
                    }
                }
            }
        }

        private ImageFormat GetImageFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            switch (extension.ToLower())
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".emf":
                    return ImageFormat.Emf;
                case ".gif":
                    return ImageFormat.Gif;
                case ".ico":
                    return ImageFormat.Icon;
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".tif":
                case ".tiff":
                    return ImageFormat.Tiff;
                case ".wmf":
                    return ImageFormat.Wmf;
                default:
                    return ImageFormat.Jpeg;
            }
        }

        private void tsPDFEmail_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count > 0)
            {
                string path = Application.StartupPath + "\\Scan.pdf";
                exportPDF(path);
                emailer.SendEmail(path, "");
                File.Delete(path);
            }
        }

        private void tsFlip_Click(object sender, EventArgs e)
        {
            Flip();
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
            new FManageProfiles().ShowDialog();
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            new FAbout().ShowDialog();
        }
    }
}
