/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using NAPS2.Scan;

using Ninject;

using WIA;

namespace NAPS2
{
    public partial class FDesktop : Form
    {
        private List<IScannedImage> images;
        private readonly IEmailer emailer;

        public FDesktop(IEmailer emailer)
        {
            InitializeComponent();
            this.emailer = emailer;
            images = new List<IScannedImage>();
        }

        private void thumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            if (thumbnailList1.SelectedIndices.Count > 0)
            {
                FViewer viewer = new FViewer(images[thumbnailList1.SelectedIndices[0]].GetImage());
                viewer.ShowDialog();
            }
        }

        private void updateView()
        {
            thumbnailList1.UpdateView(images);
        }

        private void scan(ScanSettings Profile)
        {
            IScanDriver driver = Dependencies.Kernel.Get<IScanDriver>(Profile.Device.DriverName);
            driver.DialogParent = this;
            driver.ScanSettings = Profile;

            var newImages = driver.Scan();
            images.AddRange(newImages);
            thumbnailList1.AddImages(newImages);

            Application.DoEvents();
        }

        private void deleteItems()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                if (MessageBox.Show(string.Format("Do you really want to delete {0} items?", thumbnailList1.SelectedItems.Count), "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    foreach (int index in thumbnailList1.SelectedIndices)
                    {
                        images[index].Dispose();
                        images.RemoveAt(index);
                    }
                    thumbnailList1.UpdateImages(images);
                }
            }
        }

        private int getImageBefore(int id)
        {
            var before = id - 1;
            if (before < 0)
            {
                before = images.Count - 1;
            }
            return before;
        }

        private int getImageAfter(int id)
        {
            var after = id + 1;
            if (after >= images.Count)
            {
                after = 0;
            }
            return after;
        }

        private void moveUp()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                foreach (int i in thumbnailList1.SelectedIndices)
                {
                    int before = getImageBefore(i);
                    var img = images[i];
                    images.RemoveAt(i);
                    images.Insert(before, img);
                }
                thumbnailList1.UpdateImages(images);
            }
        }

        private void moveDown()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                var selected = thumbnailList1.SelectedItems.OfType<ListViewItem>().Select(x => x.ImageList.Images[x.ImageIndex]).ToList();
                for (int j = thumbnailList1.SelectedIndices.Count - 1; j >= 0; j--)
                {
                    int i = thumbnailList1.SelectedIndices[j];
                    int after = getImageAfter(i);
                    var img = images[i];
                    images.RemoveAt(i);
                    images.Insert(after, img);
                }
                thumbnailList1.UpdateImages(images);
                thumbnailList1.SelectedIndices.Clear();
                foreach (ListViewItem item in thumbnailList1.Items)
                {
                    if (selected.Contains(item.ImageList.Images[item.ImageIndex]))
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        private void rotateLeft()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem it in thumbnailList1.SelectedItems)
                {
                    images[(int)it.Tag].RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                updateView();
            }
        }

        private void rotateRight()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem it in thumbnailList1.SelectedItems)
                {
                    images[(int)it.Tag].RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                updateView();
            }
        }

        private void thumbnailList1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    deleteItems();
                    break;
                case Keys.Left:
                    if (e.Control)
                        moveUp();
                    break;
                case Keys.Right:
                    if (e.Control)
                        moveDown();
                    break;
            }
        }

        private void exportPDF(string filename)
        {
            FPDFSave pdfdialog = Dependencies.Kernel.Get<FPDFSave>();
            pdfdialog.Filename = filename;
            pdfdialog.Images = images;
            pdfdialog.ShowDialog(this);
        }

        private void tsScan_Click(object sender, EventArgs e)
        {
            //demoScan();
            //return;

            FChooseProfile prof = new FChooseProfile();
            prof.ShowDialog();

            if (prof.Profile == null)
                return;

            scan(prof.Profile);
        }

        private void tsSavePDF_Click(object sender, EventArgs e)
        {
            if (images.Count > 0)
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
            if (images.Count > 0)
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.OverwritePrompt = true;
                sd.AddExtension = true;
                sd.Filter = "Bitmap Files (*.bmp)|*.bmp" +
                "|Enhanced Windows MetaFile (*.emf)|*.emf" +
                "|Exchangeable Image File (*.exif)|*.exif" +
                "|Gif Files (*.gif)|*.gif|JPEG Files (*.jpg)|*.jpg" +
                "|PNG Files (*.png)|*.png|TIFF Files (*.tif)|*.tif";
                sd.DefaultExt = "png";
                sd.FilterIndex = 6;

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    int i = 0;

                    if (images.Count == 1)
                    {
                        using (Bitmap baseImage = images[0].GetImage())
                        {
                            baseImage.Save(sd.FileName);
                        }
                        return;
                    }

                    if (sd.FilterIndex == 7)
                    {
                        var bitmaps = images.Select(x => x.GetImage()).ToArray();
                        CTiffHelper.SaveMultipage(bitmaps, sd.FileName);
                        foreach (Bitmap bitmap in bitmaps)
                        {
                            bitmap.Dispose();
                        }
                        return;
                    }

                    foreach (ScannedImage img in images)
                    {
                        string filename = Path.GetDirectoryName(sd.FileName) + "\\" + Path.GetFileNameWithoutExtension(sd.FileName) + i.ToString().PadLeft(3, '0') + Path.GetExtension(sd.FileName);
                        using (Bitmap baseImage = img.GetImage())
                        {
                            baseImage.Save(filename);
                        }
                        i++;
                    }
                }
            }
        }

        private void tsPDFEmail_Click(object sender, EventArgs e)
        {
            if (images.Count > 0)
            {
                string path = Application.StartupPath + "\\Scan.pdf";
                exportPDF(path);
                emailer.SendEmail(path, "");
                File.Delete(path);
            }
        }

        private void tsMoveUp_Click(object sender, EventArgs e)
        {
            moveUp();
        }

        private void tsMoveDown_Click(object sender, EventArgs e)
        {
            moveDown();
        }

        private void tsRotateLeft_Click(object sender, EventArgs e)
        {
            rotateLeft();
        }

        private void tsRotateRight_Click(object sender, EventArgs e)
        {
            rotateRight();
        }

        private void tsProfiles_Click(object sender, EventArgs e)
        {
            FManageProfiles pmanager = new FManageProfiles();
            pmanager.ShowDialog();
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            new FAbout().ShowDialog();
        }

        private void tsExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
