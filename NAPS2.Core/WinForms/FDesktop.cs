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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Update;

namespace NAPS2.WinForms
{
    public partial class FDesktop : FormBase, IScanReceiver, IAutoUpdaterClient
    {
        private readonly IEmailer emailer;
        private readonly IScannedImageImporter scannedImageImporter;
        private readonly ImageSaver imageSaver;
        private readonly StringWrapper stringWrapper;
        private readonly AppConfigManager appConfigManager;
        private readonly IErrorOutput errorOutput;
        private readonly IScannedImageFactory scannedImageFactory;
        private readonly RecoveryManager recoveryManager;
        private readonly ScannedImageList imageList = new ScannedImageList();
        private readonly AutoUpdaterUI autoUpdaterUI;
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IImagePrinter imagePrinter;
        private readonly ChangeTracker changeTracker;

        public FDesktop(IEmailer emailer, ImageSaver imageSaver, StringWrapper stringWrapper, AppConfigManager appConfigManager, IErrorOutput errorOutput, IScannedImageFactory scannedImageFactory, RecoveryManager recoveryManager, IScannedImageImporter scannedImageImporter, AutoUpdaterUI autoUpdaterUI, OcrDependencyManager ocrDependencyManager, IProfileManager profileManager, IScanPerformer scanPerformer, IImagePrinter imagePrinter, ChangeTracker changeTracker)
        {
            this.emailer = emailer;
            this.imageSaver = imageSaver;
            this.stringWrapper = stringWrapper;
            this.appConfigManager = appConfigManager;
            this.errorOutput = errorOutput;
            this.scannedImageFactory = scannedImageFactory;
            this.recoveryManager = recoveryManager;
            this.scannedImageImporter = scannedImageImporter;
            this.autoUpdaterUI = autoUpdaterUI;
            this.ocrDependencyManager = ocrDependencyManager;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.imagePrinter = imagePrinter;
            this.changeTracker = changeTracker;
            InitializeComponent();
        }

        private void FDesktop_Load(object sender, EventArgs e)
        {
            PostInitializeComponent();
        }

        private void PostInitializeComponent()
        {
            RelayoutToolbar();
            InitLanguageDropdown();
            UpdateScanButton();
        }

        private void InitLanguageDropdown()
        {
            // Read a list of languages from the Languages.resx file
            var resourceManager = LanguageResources.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet.Cast<DictionaryEntry>().OrderBy(x => x.Value))
            {
                var langCode = ((string)entry.Key).Replace("_", "-");
                var langName = (string)entry.Value;

                // Only include those languages for which localized resources exist
                string localizedResourcesPath =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", langCode,
                        "NAPS2.Core.resources.dll");
                if (langCode == "en" || File.Exists(localizedResourcesPath))
                {
                    var button = new ToolStripMenuItem(langName, null, (sender, args) => SetCulture(langCode));
                    toolStripDropDownButton1.DropDownItems.Add(button);
                }
            }
        }

        private void RelayoutToolbar()
        {
            // Wrap text as necessary
            foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
            {
                btn.Text = stringWrapper.Wrap(btn.Text, 80, CreateGraphics(), btn.Font);
            }
            ResetToolbarMargin();
            // Recalculate visibility for the below check
            Application.DoEvents();
            // Check if toolbar buttons are overflowing
            if (tStrip.Items.OfType<ToolStripItem>().Any(btn => !btn.Visible))
            {
                ShrinkToolbarMargin();
            }
        }

        private void ResetToolbarMargin()
        {
            foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
            {
                if (btn is ToolStripSplitButton)
                {
                    btn.Margin = new Padding(5, 1, 5, 2);
                }
                else if (btn is ToolStripDoubleButton)
                {
                    btn.Padding = new Padding(5, 0, 5, 0);
                }
                else
                {
                    btn.Padding = new Padding(10, 0, 10, 0);
                }
            }
        }

        private void ShrinkToolbarMargin()
        {
            foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
            {
                if (btn is ToolStripSplitButton)
                {
                    btn.Margin = new Padding(0, 1, 0, 2);
                }
                else if (btn is ToolStripDoubleButton)
                {
                    btn.Padding = new Padding(0, 0, 0, 0);
                }
                else
                {
                    btn.Padding = new Padding(5, 0, 5, 0);
                }
            }
        }

        private IEnumerable<int> SelectedIndices
        {
            get
            {
                return thumbnailList1.SelectedIndices.Cast<int>();
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

        private IEnumerable<IScannedImage> SelectedImages
        {
            get { return imageList.Images.ElementsAt(SelectedIndices); }
        }

        public void ReceiveScannedImage(IScannedImage scannedImage)
        {
            imageList.Images.Add(scannedImage);
            AppendThumbnail(scannedImage);
            changeTracker.HasUnsavedChanges = true;
        }

        private void UpdateThumbnails()
        {
            thumbnailList1.UpdateImages(imageList.Images);
            UpdateToolbar();
        }

        private void AppendThumbnail(IScannedImage scannedImage)
        {
            thumbnailList1.AppendImage(scannedImage);
            UpdateToolbar();
        }

        private void UpdateThumbnails(IEnumerable<int> selection)
        {
            UpdateThumbnails();
            SelectedIndices = selection;
        }

        private void UpdateToolbar()
        {
            // "All" dropdown items
            tsSavePDFAll.Text = tsSaveImagesAll.Text = tsEmailPDFAll.Text = tsReverseAll.Text =
                string.Format(MiscResources.AllCount, imageList.Images.Count);
            tsSavePDFAll.Enabled = tsSaveImagesAll.Enabled = tsEmailPDFAll.Enabled = tsReverseAll.Enabled =
                imageList.Images.Any();

            // "Selected" dropdown items
            tsSavePDFSelected.Text = tsSaveImagesSelected.Text = tsEmailPDFSelected.Text = tsReverseSelected.Text =
                string.Format(MiscResources.SelectedCount, SelectedIndices.Count());
            tsSavePDFSelected.Enabled = tsSaveImagesSelected.Enabled = tsEmailPDFSelected.Enabled = tsReverseSelected.Enabled =
                SelectedIndices.Any();

            // Top-level toolbar actions
            tsdImage.Enabled = tsdRotate.Enabled = tsMove.Enabled = tsDelete.Enabled = SelectedIndices.Any();
            tsdReorder.Enabled = tsdSavePDF.Enabled = tsdSaveImages.Enabled = tsdEmailPDF.Enabled = tsdPrint.Enabled = tsClear.Enabled = imageList.Images.Any();

            // Context-menu actions
            ctxView.Visible = ctxCopy.Visible = SelectedIndices.Any();
            ctxSelectAll.Enabled = imageList.Images.Any();
        }

        private void Clear()
        {
            if (imageList.Images.Count > 0)
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmClearItems, imageList.Images.Count), MiscResources.Clear, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    imageList.Delete(Enumerable.Range(0, imageList.Images.Count));
                    UpdateThumbnails();
                    changeTracker.HasUnsavedChanges = false;
                }
            }
        }

        private void Delete()
        {
            if (SelectedIndices.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, SelectedIndices.Count()), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    imageList.Delete(SelectedIndices);
                    UpdateThumbnails();
                    if (imageList.Images.Any())
                    {
                        changeTracker.HasUnsavedChanges = true;
                    }
                    else
                    {
                        changeTracker.HasUnsavedChanges = false;
                    }
                }
            }
        }

        private void SelectAll()
        {
            SelectedIndices = Enumerable.Range(0, imageList.Images.Count);
        }

        private void MoveDown()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.MoveDown(SelectedIndices));
            changeTracker.HasUnsavedChanges = true;
        }

        private void MoveUp()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.MoveUp(SelectedIndices));
            changeTracker.HasUnsavedChanges = true;
        }

        private void RotateLeft()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate270FlipNone));
            changeTracker.HasUnsavedChanges = true;
        }

        private void RotateRight()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate90FlipNone));
            changeTracker.HasUnsavedChanges = true;
        }

        private void Flip()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.RotateNoneFlipXY));
            changeTracker.HasUnsavedChanges = true;
        }

        private void ExportPDF(string filename, List<IScannedImage> images)
        {
            var pdfdialog = FormFactory.Create<FPDFSave>();
            pdfdialog.Filename = filename;
            pdfdialog.Images = images;
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
                case Keys.Up:
                    if (e.Control)
                    {
                        MoveUp();
                    }
                    break;
                case Keys.Right:
                case Keys.Down:
                    if (e.Control)
                    {
                        MoveDown();
                    }
                    break;
                case Keys.O:
                    if (e.Control)
                    {
                        Import();
                    }
                    break;
                case Keys.Enter:
                    if (e.Control)
                    {
                        ScanDefault();
                    }
                    break;
                case Keys.S:
                    if (e.Control)
                    {
                        SavePDF(imageList.Images);
                    }
                    break;
            }
        }

        private void thumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void PreviewImage()
        {
            if (SelectedIndices.Any())
            {
                var viewer = FormFactory.Create<FViewer>();
                viewer.ImageList = imageList;
                viewer.ImageIndex = SelectedIndices.First();
                viewer.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList());
            }
        }

        private void UpdateScanButton()
        {
            // Clean up the dropdown
            while (tsScan.DropDownItems.Count > 1)
            {
                tsScan.DropDownItems.RemoveAt(0);
            }

            // Populate the dropdown
            var defaultProfile = profileManager.DefaultProfile;
            foreach (var profile in profileManager.Profiles)
            {
                var item = new ToolStripMenuItem
                {
                    Text = profile.DisplayName.Replace("&", "&&"),
                    Image = profile == defaultProfile ? Icons.accept_small : null,
                    ImageScaling = ToolStripItemImageScaling.None
                };
                item.Click += (sender, args) =>
                {
                    profileManager.DefaultProfile = profile;
                    profileManager.Save();

                    UpdateScanButton();

                    scanPerformer.PerformScan(profile, this, this);
                };
                tsScan.DropDownItems.Insert(tsScan.DropDownItems.Count - 1, item);
            }

            if (profileManager.Profiles.Any())
            {
                tsScan.DropDownItems.Insert(tsScan.DropDownItems.Count - 1, new ToolStripSeparator());
            }
        }

        private void tsScan_ButtonClick(object sender, EventArgs e)
        {
            ScanDefault();
        }

        private void ScanDefault()
        {
            if (profileManager.DefaultProfile != null)
            {
                scanPerformer.PerformScan(profileManager.DefaultProfile, this, this);
            }
            else
            {
                ScanWithNewProfile();
            }
        }

        private void tsNewProfile_Click(object sender, EventArgs e)
        {
            ScanWithNewProfile();
        }

        private void ScanWithNewProfile()
        {
            var editSettingsForm = FormFactory.Create<FEditScanSettings>();
            editSettingsForm.ScanSettings = new ExtendedScanSettings { Version = ExtendedScanSettings.CURRENT_VERSION };
            editSettingsForm.ShowDialog();
            if (!editSettingsForm.Result)
            {
                return;
            }
            profileManager.Profiles.Add(editSettingsForm.ScanSettings);
            profileManager.DefaultProfile = editSettingsForm.ScanSettings;
            profileManager.Save();

            UpdateScanButton();

            scanPerformer.PerformScan(editSettingsForm.ScanSettings, this, this);
        }

        private void tsdSavePDF_ButtonClick(object sender, EventArgs e)
        {
            SavePDF(imageList.Images);
        }

        private void tsSavePDFAll_Click(object sender, EventArgs e)
        {
            SavePDF(imageList.Images);
        }

        private void tsSavePDFSelected_Click(object sender, EventArgs e)
        {
            SavePDF(SelectedImages.ToList());
        }

        private void SavePDF(List<IScannedImage> images)
        {
            if (images.Any())
            {
                var sd = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    AddExtension = true,
                    Filter = MiscResources.FileTypePdf + "|*.pdf"
                };

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    ExportPDF(sd.FileName, images);
                    changeTracker.HasUnsavedChanges = false;
                }
            }
        }

        private void tsdSaveImages_ButtonClick(object sender, EventArgs e)
        {
            SaveImages(imageList.Images);
        }

        private void tsSaveImagesAll_Click(object sender, EventArgs e)
        {
            SaveImages(imageList.Images);
        }

        private void tsSaveImagesSelected_Click(object sender, EventArgs e)
        {
            SaveImages(SelectedImages.ToList());
        }

        private void SaveImages(List<IScannedImage> images)
        {
            if (images.Any())
            {
                var sd = new SaveFileDialog
                    {
                        OverwritePrompt = true,
                        AddExtension = true,
                        Filter = MiscResources.FileTypeBmp + "|*.bmp|" +
                                 MiscResources.FileTypeEmf + "|*.emf|" +
                                 MiscResources.FileTypeExif + "|*.exif|" +
                                 MiscResources.FileTypeGif + "|*.gif|" +
                                 MiscResources.FileTypeJpeg + "|*.jpg;*.jpeg|" +
                                 MiscResources.FileTypePng + "|*.png|" +
                                 MiscResources.FileTypeTiff + "|*.tiff;*.tif"
                    };
                switch ((UserConfigManager.Config.LastImageExt ?? "").ToLowerInvariant())
                {
                    case "bmp":
                        sd.FilterIndex = 1;
                        break;
                    case "emf":
                        sd.FilterIndex = 2;
                        break;
                    case "exif":
                        sd.FilterIndex = 3;
                        break;
                    case "gif":
                        sd.FilterIndex = 4;
                        break;
                    case "png":
                        sd.FilterIndex = 6;
                        break;
                    case "tif":
                    case "tiff":
                        sd.FilterIndex = 7;
                        break;
                    default:
                        sd.FilterIndex = 5;
                        break;
                }

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    UserConfigManager.Config.LastImageExt = (Path.GetExtension(sd.FileName) ?? "").Replace(".", "");
                    UserConfigManager.Save();
                    try
                    {
                        imageSaver.SaveImages(sd.FileName, images, path =>
                        {
                            if (images.Count() == 1)
                            {
                                // One image, so the file name is the same and the save dialog already prompted the user to overwrite
                                return true;
                            }
                            switch (
                                MessageBox.Show(
                                    string.Format(MiscResources.ConfirmOverwriteFile, path),
                                    MiscResources.OverwriteFile, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning))
                            {
                                case DialogResult.Yes:
                                    return true;
                                case DialogResult.No:
                                    return false;
                                default:
                                    throw new InvalidOperationException("User cancelled");
                            }
                        });
                        changeTracker.HasUnsavedChanges = false;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
        }

        private void tsdEmailPDF_ButtonClick(object sender, EventArgs e)
        {
            EmailPDF(imageList.Images);
        }

        private void tsEmailPDFAll_Click(object sender, EventArgs e)
        {
            EmailPDF(imageList.Images);
        }

        private void tsEmailPDFSelected_Click(object sender, EventArgs e)
        {
            EmailPDF(SelectedImages.ToList());
        }

        private void EmailPDF(List<IScannedImage> images)
        {
            if (images.Any())
            {
                string path = Paths.AppData + "\\Scan.pdf";
                ExportPDF(path, images);
                if (emailer.SendEmail(new EmailMessage
                {
                    Attachments = new List<EmailAttachment>
                    {
                        new EmailAttachment
                        {
                            FilePath = path,
                            AttachmentName = Path.GetFileName(path)
                        }
                    }
                }))
                {
                    changeTracker.HasUnsavedChanges = false;
                }
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
            ShowProfilesForm();
        }

        private void ShowProfilesForm()
        {
            var form = FormFactory.Create<FProfiles>();
            form.ScanReceiver = this;
            form.ShowDialog();
            UpdateScanButton();
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FAbout>().ShowDialog();
        }

        private void SetCulture(string cultureId)
        {
            UserConfigManager.Config.Culture = cultureId;
            UserConfigManager.Save();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureId);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureId);

            // Update localized values
            // Since all forms are opened modally and this is the root form, it should be the only one that needs to be updated live
            SaveFormState = false;
            Controls.RemoveAll();
            UpdateRTL();
            InitializeComponent();
            PostInitializeComponent();
            UpdateThumbnails();
            Focus();
            WindowState = FormWindowState.Normal;
            DoRestoreFormState();
            SaveFormState = true;
        }

        private void FDesktop_Shown(object sender, EventArgs e)
        {
            UpdateToolbar();

            // If configured (e.g. by a business), show a customizable message box on application startup.
            var appConfig = appConfigManager.Config;
            if (!string.IsNullOrWhiteSpace(appConfig.StartupMessageText))
            {
                MessageBox.Show(appConfig.StartupMessageText, appConfig.StartupMessageTitle, MessageBoxButtons.OK,
                    appConfig.StartupMessageIcon);
            }

            // Allow scanned images to be recovered in case of an unexpected close
            foreach (IScannedImage scannedImage in recoveryManager.RecoverScannedImages())
            {
                imageList.Images.Add(scannedImage);
                changeTracker.HasUnsavedChanges = true;
            }
            UpdateThumbnails();

            // Automatic updates
            // Not yet enabled
            // autoUpdaterUI.OnApplicationStart(this);
        }

        private void FDesktop_Closed(object sender, EventArgs e)
        {
            // TODO: Add a closing confirmation
            imageList.Delete(Enumerable.Range(0, imageList.Images.Count));
        }

        private void tsImport_Click(object sender, EventArgs e)
        {
            Import();
        }

        private void Import()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in ofd.FileNames)
                {
                    // TODO: Run in thread, and show a dialog (just like exporting)
                    // Need to provide count somehow (progress callback). count = # files or # pages
                    var images = scannedImageImporter.Import(fileName);
                    foreach (var img in images)
                    {
                        imageList.Images.Add(img);
                        AppendThumbnail(img);
                        thumbnailList1.Refresh();
                        changeTracker.HasUnsavedChanges = true;
                    }
                }
            }
        }

        public void UpdateAvailable(VersionInfo versionInfo)
        {
            Invoke(new Action(() => autoUpdaterUI.PerformUpdate(this, versionInfo)));
        }

        public void InstallComplete()
        {
            Invoke(new Action(() =>
            {
                switch (MessageBox.Show(MiscResources.InstallCompletePromptRestart, MiscResources.InstallComplete, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        Close(); // TODO: This close might be canceled. Handle that.
                        Process.Start(Application.ExecutablePath);
                        break;
                }
            }));
        }

        private void thumbnailList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateToolbar();
        }

        private void tsInterleave_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Count() < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.Interleave(SelectedIndices));
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsDeinterleave_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Count() < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.Deinterleave(SelectedIndices));
            changeTracker.HasUnsavedChanges = true;
        }

        private void thumbnailList1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = thumbnailList1.GetItemAt(e.X, e.Y) == null ? Cursors.Default : Cursors.Hand;
        }

        private void thumbnailList1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void tsOcr_Click(object sender, EventArgs e)
        {
            if (ocrDependencyManager.IsExecutableDownloaded && ocrDependencyManager.GetDownloadedLanguages().Any())
            {
                FormFactory.Create<FOcrSetup>().ShowDialog();
            }
            else
            {
                FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                if (ocrDependencyManager.IsExecutableDownloaded && ocrDependencyManager.GetDownloadedLanguages().Any())
                {
                    FormFactory.Create<FOcrSetup>().ShowDialog();
                }
            }
        }

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!imageList.Images.Any())
            {
                e.Cancel = true;
            }
        }

        private void ctxSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void ctxView_Click(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void ctxCopy_Click(object sender, EventArgs e)
        {
            CopyImages();
        }

        private void CopyImages()
        {
            if (SelectedIndices.Any())
            {
                var images = SelectedImages.Select(x => x.GetImage()).ToList();
                IDataObject ido = new DataObject();
                ido.SetData(DataFormats.Bitmap, true, images.First());
                var rtfEncodedImages = "{" + string.Join(@"\par", images.Select(GetRtfEncodedImage)) + "}";
                ido.SetData(DataFormats.Rtf, true, rtfEncodedImages);
                Clipboard.SetDataObject(ido);
            }
        }

        private static string GetRtfEncodedImage(Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                string hexString = BitConverter.ToString(stream.ToArray(), 0).Replace("-", string.Empty);

                return @"{\pict\pngblip\picw" +
                       image.Width + @"\pich" + image.Height +
                       @"\picwgoa" + image.Width + @"\pichgoa" + image.Height +
                       @"\hex " + hexString + "}";
            }
        }

        private void tsReverseAll_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 2)
            {
                return;
            }
            UpdateThumbnails(imageList.Reverse());
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsReverseSelected_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Count() < 2)
            {
                return;
            }
            UpdateThumbnails(imageList.Reverse(SelectedIndices));
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsMove_ClickFirst(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void tsMove_ClickSecond(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void tsView_Click(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void tsReset_Click(object sender, EventArgs e)
        {
            ResetImage();
        }

        private void ResetImage()
        {
            if (SelectedIndices.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmResetImages, SelectedIndices.Count()), MiscResources.ResetImage, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    UpdateThumbnails(imageList.ResetTransforms(SelectedIndices));
                    changeTracker.HasUnsavedChanges = true;
                }
            }
        }

        private void tsCrop_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FCrop>();
                form.Image = SelectedImages.First();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList());
            }
        }

        private void tsBrightness_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FBrightness>();
                form.Image = SelectedImages.First();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList());
            }
        }

        private void tsContrast_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FContrast>();
                form.Image = SelectedImages.First();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList());
            }
        }

        private void customRotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FRotate>();
                form.Image = SelectedImages.First();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList());
            }
        }

        private void tsdPrint_Click(object sender, EventArgs e)
        {
            if (imagePrinter.PromptToPrint(imageList.Images, SelectedImages.ToList()))
            {
                changeTracker.HasUnsavedChanges = false;
            }
        }

        private void FDesktop_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (changeTracker.HasUnsavedChanges)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    var result = MessageBox.Show(MiscResources.ExitWithUnsavedChanges, MiscResources.UnsavedChanges,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        changeTracker.HasUnsavedChanges = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    FileBasedScannedImage.DisableRecoveryCleanup = true;
                }
            }
        }
    }
}
