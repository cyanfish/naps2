#region Usings

using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Wia;
using NAPS2.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion Usings

namespace NAPS2.WinForms
{
    public partial class FDesktop : FormBase
    {
        #region Dependencies

        private readonly StringWrapper stringWrapper;
        private readonly AppConfigManager appConfigManager;
        private readonly RecoveryManager recoveryManager;
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IScannedImagePrinter scannedImagePrinter;
        private readonly ChangeTracker changeTracker;
        private readonly StillImage stillImage;
        private readonly IOperationFactory OperationFactory;
        private readonly IUserConfigManager userConfigManager;
        private readonly KeyboardShortcutManager ksm;
        private readonly WinFormsExportHelper exportHelper;
        private readonly ScannedImageRenderer scannedImageRenderer;

        #endregion Dependencies

        #region State Fields

        private readonly ScannedImageList imageList = new ScannedImageList();
        private CancellationTokenSource renderThumbnailsCts;
        private LayoutManager layoutManager;
        private bool disableSelectedIndexChangedEvent;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private NotificationManager notify;

        #endregion State Fields

        #region Initialization and Culture

        public FDesktop(StringWrapper stringWrapper, AppConfigManager appConfigManager, RecoveryManager recoveryManager, OcrDependencyManager ocrDependencyManager, IProfileManager profileManager, IScanPerformer scanPerformer, IScannedImagePrinter scannedImagePrinter, ChangeTracker changeTracker, StillImage stillImage, IOperationFactory OperationFactory, IUserConfigManager userConfigManager, KeyboardShortcutManager ksm, ThumbnailRenderer thumbnailRenderer, WinFormsExportHelper exportHelper, ScannedImageRenderer scannedImageRenderer)
        {
            this.stringWrapper = stringWrapper;
            this.appConfigManager = appConfigManager;
            this.recoveryManager = recoveryManager;
            this.ocrDependencyManager = ocrDependencyManager;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.scannedImagePrinter = scannedImagePrinter;
            this.changeTracker = changeTracker;
            this.stillImage = stillImage;
            this.OperationFactory = OperationFactory;
            this.userConfigManager = userConfigManager;
            this.ksm = ksm;
            this.thumbnailRenderer = thumbnailRenderer;
            this.exportHelper = exportHelper;
            this.scannedImageRenderer = scannedImageRenderer;
            InitializeComponent();

            Shown += FDesktop_Shown;
            FormClosing += FDesktop_FormClosing;
            Closed += FDesktop_Closed;
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            PostInitializeComponent();
        }

        /// <summary>
        /// Runs when the form is first loaded and every time the language is changed.
        /// </summary>
        private void PostInitializeComponent()
        {
            imageList.ThumbnailRenderer = thumbnailRenderer;
            ThumbnailList1.ThumbnailRenderer = thumbnailRenderer;
            int thumbnailSize = UserConfigManager.Config.ThumbnailSize;
            ThumbnailList1.ThumbnailSize = new Size(thumbnailSize, thumbnailSize);
            SetThumbnailSpacing(thumbnailSize);

            if (appConfigManager.Config.HideOcrButton)
            {
                TStrip.Items.Remove(tsOcr);
            }
            if (appConfigManager.Config.HideImportButton)
            {
                TStrip.Items.Remove(tsImport);
            }
            if (appConfigManager.Config.HideSavePdfButton)
            {
                TStrip.Items.Remove(tsdSavePDF);
            }
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                TStrip.Items.Remove(tsdSaveImages);
            }
            if (appConfigManager.Config.HideEmailButton)
            {
                TStrip.Items.Remove(tsdEmailPDF);
            }
            if (appConfigManager.Config.HidePrintButton)
            {
                TStrip.Items.Remove(tsPrint);
            }

            LoadToolStripLocation();
            RelayoutToolbar();
            InitLanguageDropdown();
            AssignKeyboardShortcuts();
            UpdateScanButton();

            layoutManager?.Deactivate();
            BtnZoomIn.Location = new Point(BtnZoomIn.Location.X, ThumbnailList1.Height - 33);
            BtnZoomOut.Location = new Point(BtnZoomOut.Location.X, ThumbnailList1.Height - 33);
            BtnZoomMouseCatcher.Location = new Point(BtnZoomMouseCatcher.Location.X, ThumbnailList1.Height - 33);
            layoutManager = new LayoutManager(this)
                   .Bind(BtnZoomIn, BtnZoomOut, BtnZoomMouseCatcher)
                       .BottomTo(() => ThumbnailList1.Height)
                   .Activate();

            ThumbnailList1.MouseWheel += ThumbnailList1_MouseWheel;
            ThumbnailList1.SizeChanged += (sender, args) => layoutManager.UpdateLayout();

            notify = new NotificationManager(this, appConfigManager);
        }

        private void InitLanguageDropdown()
        {
            // Read a list of languages from the Languages.resx file
            var resourceManager = LanguageNames.ResourceManager;
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
            foreach (var Btn in TStrip.Items.OfType<ToolStripItem>())
            {
                Btn.Text = stringWrapper.Wrap(Btn.Text, 80, CreateGraphics(), Btn.Font);
            }
            ResetToolbarMargin();
            // Recalculate visibility for the below check
            Application.DoEvents();
            // Check if toolbar buttons are overflowing
            if (TStrip.Items.OfType<ToolStripItem>().Any(Btn => !Btn.Visible)
                && (TStrip.Parent.Dock == DockStyle.Top || TStrip.Parent.Dock == DockStyle.Bottom))
            {
                ShrinkToolbarMargin();
            }
        }

        private void ResetToolbarMargin()
        {
            foreach (var Btn in TStrip.Items.OfType<ToolStripItem>())
            {
                if (Btn is ToolStripSplitButton)
                {
                    if (TStrip.Parent.Dock == DockStyle.Left || TStrip.Parent.Dock == DockStyle.Right)
                    {
                        Btn.Margin = new Padding(10, 1, 5, 2);
                    }
                    else
                    {
                        Btn.Margin = new Padding(5, 1, 5, 2);
                    }
                }
                else if (Btn is ToolStripDoubleButton)
                {
                    Btn.Padding = new Padding(5, 0, 5, 0);
                }
                else if (TStrip.Parent.Dock == DockStyle.Left || TStrip.Parent.Dock == DockStyle.Right)
                {
                    Btn.Margin = new Padding(0, 1, 5, 2);
                }
                else
                {
                    Btn.Padding = new Padding(10, 0, 10, 0);
                }
            }
        }

        private void ShrinkToolbarMargin()
        {
            foreach (var Btn in TStrip.Items.OfType<ToolStripItem>())
            {
                if (Btn is ToolStripSplitButton)
                {
                    Btn.Margin = new Padding(0, 1, 0, 2);
                }
                else if (Btn is ToolStripDoubleButton)
                {
                    Btn.Padding = new Padding(0, 0, 0, 0);
                }
                else
                {
                    Btn.Padding = new Padding(5, 0, 5, 0);
                }
            }
        }

        private void SetCulture(string cultureId)
        {
            SaveToolStripLocation();
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

            // Receive messages from other processes
            Pipes.StartServer(msg =>
            {
                if (msg.StartsWith(Pipes.MSG_SCAN_WITH_DEVICE, StringComparison.CurrentCulture))
                {
                    SafeInvoke(() => ScanWithDevice(msg.Substring(Pipes.MSG_SCAN_WITH_DEVICE.Length)));
                }
                if (msg.Equals(Pipes.MSG_ACTIVATE))
                {
                    SafeInvoke(() =>
                    {
                        var form = Application.OpenForms.Cast<Form>().Last();
                        if (form.WindowState == FormWindowState.Minimized)
                        {
                            Win32.ShowWindow(form.Handle, Win32.ShowWindowCommands.Restore);
                        }
                        form.Activate();
                    });
                }
            });

            // If configured (e.g. by a business), show a customizable message box on application startup.
            var appConfig = appConfigManager.Config;
            if (!string.IsNullOrWhiteSpace(appConfig.StartupMessageText))
            {
                MessageBox.Show(appConfig.StartupMessageText, appConfig.StartupMessageTitle, MessageBoxButtons.OK,
                    appConfig.StartupMessageIcon);
            }

            // Allow scanned images to be recovered in case of an unexpected close
            recoveryManager.RecoverScannedImages(ReceiveScannedImage);

            // If NAPS2 was started by the scanner button, do the appropriate actions automatically
            RunStillImageEvents();

            // Show a donation prompt after a month of use
            if (userConfigManager.Config.FirstRunDate == null)
            {
                userConfigManager.Config.FirstRunDate = DateTime.Now;
                userConfigManager.Save();
            }
#if !INSTALLER_MSI
            else if (!appConfigManager.Config.HideDonateButton
                && userConfigManager.Config.LastDonatePromptDate == null
                && DateTime.Now - userConfigManager.Config.FirstRunDate > TimeSpan.FromDays(30))
            {
                userConfigManager.Config.LastDonatePromptDate = DateTime.Now;
                userConfigManager.Save();
                notify.DonatePrompt();
            }
#endif
        }

        #endregion Initialization and Culture

        #region Cleanup

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
                    RecoveryImage.DisableRecoveryCleanup = true;
                }
            }
        }

        private void FDesktop_Closed(object sender, EventArgs e)
        {
            SaveToolStripLocation();
            Pipes.KillServer();
            imageList.Delete(Enumerable.Range(0, imageList.Images.Count));
        }

        #endregion Cleanup

        #region Scanning and Still Image

        private void RunStillImageEvents()
        {
            if (stillImage.DoScan)
            {
                ScanWithDevice(stillImage.DeviceID);
            }
        }

        private void ScanWithDevice(string deviceID)
        {
            Activate();
            ScanProfile profile;
            if (profileManager.DefaultProfile?.Device?.ID == deviceID)
            {
                // Try to use the default profile if it has the right device
                profile = profileManager.DefaultProfile;
            }
            else
            {
                // Otherwise just pick any old profile with the right device
                // Not sure if this is the best way to do it, but it's hard to prioritize profiles
                profile = profileManager.Profiles.Find(x => x.Device != null && x.Device.ID == deviceID);
            }
            if (profile == null)
            {
                if (appConfigManager.Config.NoUserProfiles && profileManager.Profiles.Any(x => x.IsLocked))
                {
                    return;
                }

                // No profile for the device we're scanning with, so prompt to create one
                var editSettingsForm = FormFactory.Create<FEditProfile>();
                editSettingsForm.ScanProfile = appConfigManager.Config.DefaultProfileSettings ??
                                               new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
                try
                {
                    // Populate the device field automatically (because we can do that!)
                    string deviceName = WiaApi.GetDeviceName(deviceID);
                    editSettingsForm.CurrentDevice = new ScanDevice(deviceID, deviceName);
                }
                catch (DeviceNotFoundException)
                {
                }
                editSettingsForm.ShowDialog();
                if (!editSettingsForm.Result)
                {
                    return;
                }
                profile = editSettingsForm.ScanProfile;
                profileManager.Profiles.Add(profile);
                profileManager.DefaultProfile = profile;
                profileManager.Save();

                UpdateScanButton();
            }
            if (profile != null)
            {
                // We got a profile, yay, so we can actually do the scan now
                scanPerformer.PerformScan(profile, new ScanParams(), this, notify, ReceiveScannedImage);
                Activate();
            }
        }

        private void ScanDefault()
        {
            if (profileManager.DefaultProfile != null)
            {
                scanPerformer.PerformScan(profileManager.DefaultProfile, new ScanParams(), this, notify, ReceiveScannedImage);
                Activate();
            }
            else if (profileManager.Profiles.Count == 0)
            {
                ScanWithNewProfile();
            }
            else
            {
                ShowProfilesForm();
            }
        }

        private void ScanWithNewProfile()
        {
            var editSettingsForm = FormFactory.Create<FEditProfile>();
            editSettingsForm.ScanProfile = appConfigManager.Config.DefaultProfileSettings ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
            editSettingsForm.ShowDialog();
            if (!editSettingsForm.Result)
            {
                return;
            }
            profileManager.Profiles.Add(editSettingsForm.ScanProfile);
            profileManager.DefaultProfile = editSettingsForm.ScanProfile;
            profileManager.Save();

            UpdateScanButton();

            scanPerformer.PerformScan(editSettingsForm.ScanProfile, new ScanParams(), this, notify, ReceiveScannedImage);
            Activate();
        }

        #endregion Scanning and Still Image

        #region Images and Thumbnails

        private IEnumerable<int> SelectedIndices
        {
            get => ThumbnailList1.SelectedIndices.Cast<int>();
            set
            {
                disableSelectedIndexChangedEvent = true;
                ThumbnailList1.SelectedIndices.Clear();
                foreach (int i in value)
                {
                    ThumbnailList1.SelectedIndices.Add(i);
                }
                disableSelectedIndexChangedEvent = false;
                ThumbnailList1_SelectedIndexChanged(ThumbnailList1, EventArgs.Empty);
            }
        }

        private IEnumerable<ScannedImage> SelectedImages => imageList.Images.ElementsAt(SelectedIndices);

        public void ReceiveScannedImage(ScannedImage scannedImage)
        {
            SafeInvoke(() =>
            {
                imageList.Images.Add(scannedImage);
                AppendThumbnail(scannedImage);
                changeTracker.HasUnsavedChanges = true;
                Application.DoEvents();
            });
        }

        private void UpdateThumbnails()
        {
            ThumbnailList1.UpdateImages(imageList.Images);
            UpdateToolbar();
        }

        private void AppendThumbnail(ScannedImage scannedImage)
        {
            ThumbnailList1.AppendImage(scannedImage);
            UpdateToolbar();
        }

        private void UpdateThumbnails(IEnumerable<int> selection, bool scrollToSelection, bool optimizeForSelection)
        {
            ThumbnailList1.UpdateImages(imageList.Images, optimizeForSelection ? SelectedIndices.Concat(selection).ToList() : null);
            SelectedIndices = selection;
            UpdateToolbar();

            if (scrollToSelection)
            {
                // Scroll to selection
                // If selection is empty (e.g. after interleave), this scrolls to top
                ThumbnailList1.EnsureVisible(SelectedIndices.LastOrDefault());
                ThumbnailList1.EnsureVisible(SelectedIndices.FirstOrDefault());
            }
        }

        #endregion Images and Thumbnails

        #region Toolbar

        private void UpdateToolbar()
        {
            // "All" dropdown items
            TsSavePDFAll.Text = TsSaveImagesAll.Text = tsEmailPDFAll.Text = tsReverseAll.Text =
                string.Format(MiscResources.AllCount, imageList.Images.Count);
            TsSavePDFAll.Enabled = TsSaveImagesAll.Enabled = tsEmailPDFAll.Enabled = tsReverseAll.Enabled =
                imageList.Images.Count > 0;

            // "Selected" dropdown items
            TsSavePDFSelected.Text = TsSaveImagesSelected.Text = tsEmailPDFSelected.Text = tsReverseSelected.Text =
                string.Format(MiscResources.SelectedCount, SelectedIndices.Count());
            TsSavePDFSelected.Enabled = TsSaveImagesSelected.Enabled = tsEmailPDFSelected.Enabled = tsReverseSelected.Enabled =
                SelectedIndices.Any();

            // Top-level toolbar actions
            tsdImage.Enabled = tsdRotate.Enabled = tsMove.Enabled = TsDelete.Enabled = SelectedIndices.Any();
            tsdReorder.Enabled = tsdSavePDF.Enabled = tsdSaveImages.Enabled = tsdEmailPDF.Enabled = tsPrint.Enabled = tsClear.Enabled = imageList.Images.Count > 0;

            // Context-menu actions
            ctxView.Visible = ctxCopy.Visible = ctxDelete.Visible = ctxSeparator1.Visible = ctxSeparator2.Visible = SelectedIndices.Any();
            ctxSelectAll.Enabled = imageList.Images.Count > 0;

            // Other
            BtnZoomIn.Enabled = imageList.Images.Count > 0 && UserConfigManager.Config.ThumbnailSize < ThumbnailRenderer.MAX_SIZE;
            BtnZoomOut.Enabled = imageList.Images.Count > 0 && UserConfigManager.Config.ThumbnailSize > ThumbnailRenderer.MIN_SIZE;
            TsNewProfile.Enabled = !(appConfigManager.Config.NoUserProfiles && profileManager.Profiles.Any(x => x.IsLocked));
        }

        private void UpdateScanButton()
        {
            const int staticButtonCount = 2;

            // Clean up the dropdown
            while (TsScan.DropDownItems.Count > staticButtonCount)
            {
                TsScan.DropDownItems.RemoveAt(0);
            }

            // Populate the dropdown
            var defaultProfile = profileManager.DefaultProfile;
            int i = 1;
            foreach (var profile in profileManager.Profiles)
            {
                var item = new ToolStripMenuItem
                {
                    Text = profile.DisplayName.Replace("&", "&&"),
                    Image = profile == defaultProfile ? Icons.accept_small : null,
                    ImageScaling = ToolStripItemImageScaling.None
                };
                AssignProfileShortcut(i, item);
                item.Click += (sender, args) =>
                {
                    profileManager.DefaultProfile = profile;
                    profileManager.Save();

                    UpdateScanButton();

                    scanPerformer.PerformScan(profile, new ScanParams(), this, notify, ReceiveScannedImage);
                    Activate();
                };
                TsScan.DropDownItems.Insert(TsScan.DropDownItems.Count - staticButtonCount, item);

                i++;
            }

            if (profileManager.Profiles.Count > 0)
            {
                TsScan.DropDownItems.Insert(TsScan.DropDownItems.Count - staticButtonCount, new ToolStripSeparator());
            }
        }

        private void SaveToolStripLocation()
        {
            UserConfigManager.Config.DesktopToolStripDock = TStrip.Parent.Dock;
            UserConfigManager.Save();
        }

        private void LoadToolStripLocation()
        {
            var dock = UserConfigManager.Config.DesktopToolStripDock;
            if (dock != DockStyle.None)
            {
                var panel = toolStripContainer1.Controls.OfType<ToolStripPanel>().FirstOrDefault(x => x.Dock == dock);
                if (panel != null)
                {
                    TStrip.Parent = panel;
                }
            }
        }

        #endregion Toolbar

        #region Actions

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
                    UpdateThumbnails(Enumerable.Empty<int>(), false, false);
                    changeTracker.HasUnsavedChanges = imageList.Images.Count > 0;
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
            UpdateThumbnails(imageList.MoveDown(SelectedIndices), true, true);
            changeTracker.HasUnsavedChanges = true;
        }

        private void MoveUp()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.MoveUp(SelectedIndices), true, true);
            changeTracker.HasUnsavedChanges = true;
        }

        private void RotateLeft()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate270FlipNone), false, true);
            changeTracker.HasUnsavedChanges = true;
        }

        private void RotateRight()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate90FlipNone), false, true);
            changeTracker.HasUnsavedChanges = true;
        }

        private void Flip()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.RotateFlip(SelectedIndices, RotateFlipType.RotateNoneFlipXY), false, true);
            changeTracker.HasUnsavedChanges = true;
        }

        private void Deskew()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }

            var op = OperationFactory.Create<DeskewOperation>();
            var progressForm = FormFactory.Create<FProgress>();
            progressForm.Operation = op;

            if (op.Start(SelectedImages.ToList()))
            {
                progressForm.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
                changeTracker.HasUnsavedChanges = true;
            }
        }

        private void PreviewImage()
        {
            if (SelectedIndices.Any())
            {
                using (var viewer = FormFactory.Create<FViewer>())
                {
                    viewer.ImageList = imageList;
                    viewer.ImageIndex = SelectedIndices.First();
                    viewer.DeleteCallback = UpdateThumbnails;
                    viewer.UpdateCallback = x => UpdateThumbnails(x, false, true);
                    viewer.SelectCallback = i =>
                    {
                        if (SelectedIndices.Count() <= 1)
                        {
                            SelectedIndices = new[] { i };
                            ThumbnailList1.Items[i].EnsureVisible();
                        }
                    };
                    viewer.ShowDialog();
                }
            }
        }

        private void ShowProfilesForm()
        {
            var form = FormFactory.Create<FProfiles>();
            form.ImageCallback = ReceiveScannedImage;
            form.ShowDialog();
            UpdateScanButton();
        }

        private void ResetImage()
        {
            if (SelectedIndices.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmResetImages, SelectedIndices.Count()), MiscResources.ResetImage, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    UpdateThumbnails(imageList.ResetTransforms(SelectedIndices), false, true);
                    changeTracker.HasUnsavedChanges = true;
                }
            }
        }

        #endregion Actions

        #region Actions - Save/Email/Import

        private void SavePDF(List<ScannedImage> images)
        {
            if (exportHelper.SavePDF(images, notify))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    imageList.Delete(imageList.Images.IndiciesOf(images));
                    UpdateThumbnails(Enumerable.Empty<int>(), false, false);
                }
            }
        }

        private void SaveImages(List<ScannedImage> images)
        {
            if (exportHelper.SaveImages(images, notify))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    imageList.Delete(imageList.Images.IndiciesOf(images));
                    UpdateThumbnails(Enumerable.Empty<int>(), false, false);
                }
            }
        }

        private void EmailPDF(List<ScannedImage> images)
        {
            exportHelper.EmailPDF(images);
        }

        private void Import()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true,
                Filter = MiscResources.FileTypeAllFiles + "|*.*|" +
                         MiscResources.FileTypePdf + "|*.pdf|" +
                         MiscResources.FileTypeImageFiles + "|*.bmp;*.emf;*.exif;*.gif;*.jpg;*.jpeg;*.png;*.tiff;*.tif|" +
                         MiscResources.FileTypeBmp + "|*.bmp|" +
                         MiscResources.FileTypeEmf + "|*.emf|" +
                         MiscResources.FileTypeExif + "|*.exif|" +
                         MiscResources.FileTypeGif + "|*.gif|" +
                         MiscResources.FileTypeJpeg + "|*.jpg;*.jpeg|" +
                         MiscResources.FileTypePng + "|*.png|" +
                         MiscResources.FileTypeTiff + "|*.tiff;*.tif"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ImportFiles(ofd.FileNames);
            }
        }

        private void ImportFiles(IEnumerable<string> files)
        {
            var op = OperationFactory.Create<ImportOperation>();
            var progressForm = FormFactory.Create<FProgress>();
            progressForm.Operation = op;
            if (op.Start(OrderFiles(files), ReceiveScannedImage))
            {
                progressForm.ShowDialog();
            }
        }

        private List<string> OrderFiles(IEnumerable<string> files)
        {
            // Custom ordering to account for numbers so that e.g. "10" comes after "2"
            var filesList = files.ToList();
            filesList.Sort(Win32.StrCmpLogicalW);
            return filesList;
        }

        private void ImportDirect(DirectImageTransfer data, bool copy)
        {
            var op = OperationFactory.Create<DirectImportOperation>();
            var progressForm = FormFactory.Create<FProgress>();
            progressForm.Operation = op;
            if (op.Start(data, copy, ReceiveScannedImage))
            {
                progressForm.ShowDialog();
            }
        }

        #endregion Actions - Save/Email/Import

        #region Keyboard Shortcuts

        private void AssignKeyboardShortcuts()
        {
            // Defaults

            ksm.Assign("Ctrl+Enter", TsScan);
            ksm.Assign("Ctrl+B", tsBatchScan);
            ksm.Assign("Ctrl+O", tsImport);
            ksm.Assign("Ctrl+S", tsdSavePDF);
            ksm.Assign("Ctrl+P", tsPrint);
            ksm.Assign("Ctrl+Up", MoveUp);
            ksm.Assign("Ctrl+Left", MoveUp);
            ksm.Assign("Ctrl+Down", MoveDown);
            ksm.Assign("Ctrl+Right", MoveDown);
            ksm.Assign("Ctrl+Shift+Del", tsClear);
            ksm.Assign("F1", tsAbout);
            ksm.Assign("Ctrl+OemMinus", BtnZoomOut);
            ksm.Assign("Ctrl+Oemplus", BtnZoomIn);
            ksm.Assign("Del", ctxDelete);

            // Configured

            var ks = userConfigManager.Config.KeyboardShortcuts ?? appConfigManager.Config.KeyboardShortcuts ?? new KeyboardShortcuts();

            ksm.Assign(ks.About, tsAbout);
            ksm.Assign(ks.BatchScan, tsBatchScan);
            ksm.Assign(ks.Clear, tsClear);
            ksm.Assign(ks.Delete, TsDelete);
            ksm.Assign(ks.EmailPDF, tsdEmailPDF);
            ksm.Assign(ks.EmailPDFAll, tsEmailPDFAll);
            ksm.Assign(ks.EmailPDFSelected, tsEmailPDFSelected);
            ksm.Assign(ks.ImageBlackWhite, TsBlackWhite);
            ksm.Assign(ks.ImageBrightness, TsBrightnessContrast);
            ksm.Assign(ks.ImageContrast, TsBrightnessContrast);
            ksm.Assign(ks.ImageCrop, TsCrop);
            ksm.Assign(ks.ImageHue, TsHueSaturation);
            ksm.Assign(ks.ImageSaturation, TsHueSaturation);
            ksm.Assign(ks.ImageSharpen, TsSharpen);
            ksm.Assign(ks.ImageReset, tsReset);
            ksm.Assign(ks.ImageView, tsView);
            ksm.Assign(ks.Import, tsImport);
            ksm.Assign(ks.MoveDown, MoveDown); // TODO
            ksm.Assign(ks.MoveUp, MoveUp); // TODO
            ksm.Assign(ks.NewProfile, TsNewProfile);
            ksm.Assign(ks.Ocr, tsOcr);
            ksm.Assign(ks.Print, tsPrint);
            ksm.Assign(ks.Profiles, ShowProfilesForm);

            ksm.Assign(ks.ReorderAltDeinterleave, tsAltDeinterleave);
            ksm.Assign(ks.ReorderAltInterleave, tsAltInterleave);
            ksm.Assign(ks.ReorderDeinterleave, tsDeinterleave);
            ksm.Assign(ks.ReorderInterleave, tsInterleave);
            ksm.Assign(ks.ReorderReverseAll, tsReverseAll);
            ksm.Assign(ks.ReorderReverseSelected, tsReverseSelected);
            ksm.Assign(ks.RotateCustom, TsCustomRotation);
            ksm.Assign(ks.RotateFlip, TsFlip);
            ksm.Assign(ks.RotateLeft, TsRotateLeft);
            ksm.Assign(ks.RotateRight, TsRotateRight);
            ksm.Assign(ks.SaveImages, tsdSaveImages);
            ksm.Assign(ks.SaveImagesAll, TsSaveImagesAll);
            ksm.Assign(ks.SaveImagesSelected, TsSaveImagesSelected);
            ksm.Assign(ks.SavePDF, tsdSavePDF);
            ksm.Assign(ks.SavePDFAll, TsSavePDFAll);
            ksm.Assign(ks.SavePDFSelected, TsSavePDFSelected);
            ksm.Assign(ks.ScanDefault, TsScan);

            ksm.Assign(ks.ZoomIn, BtnZoomIn);
            ksm.Assign(ks.ZoomOut, BtnZoomOut);
        }

        private void AssignProfileShortcut(int i, ToolStripMenuItem item)
        {
            var sh = GetProfileShortcut(i);
            if (string.IsNullOrWhiteSpace(sh) && i <= 11)
            {
                sh = "F" + (i + 1);
            }
            ksm.Assign(sh, item);
        }

        private string GetProfileShortcut(int i)
        {
            var ks = userConfigManager.Config.KeyboardShortcuts ?? appConfigManager.Config.KeyboardShortcuts ?? new KeyboardShortcuts();
            switch (i)
            {
                case 1:
                    return ks.ScanProfile1;

                case 2:
                    return ks.ScanProfile2;

                case 3:
                    return ks.ScanProfile3;

                case 4:
                    return ks.ScanProfile4;

                case 5:
                    return ks.ScanProfile5;

                case 6:
                    return ks.ScanProfile6;

                case 7:
                    return ks.ScanProfile7;

                case 8:
                    return ks.ScanProfile8;

                case 9:
                    return ks.ScanProfile9;

                case 10:
                    return ks.ScanProfile10;

                case 11:
                    return ks.ScanProfile11;

                case 12:
                    return ks.ScanProfile12;
            }
            return null;
        }

        private void ThumbnailList1_KeyDown(object sender, KeyEventArgs e)
        {
            ksm.Perform(e.KeyData);
        }

        private void ThumbnailList1_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
            {
                StepThumbnailSize(e.Delta / (double)SystemInformation.MouseWheelScrollDelta);
            }
        }

        #endregion Keyboard Shortcuts

        #region Event Handlers - Misc

        private void ThumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void ThumbnailList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!disableSelectedIndexChangedEvent)
            {
                UpdateToolbar();
            }
        }

        private void ThumbnailList1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = ThumbnailList1.GetItemAt(e.X, e.Y) == null ? Cursors.Default : Cursors.Hand;
        }

        private void ThumbnailList1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void TStrip_DockChanged(object sender, EventArgs e)
        {
            RelayoutToolbar();
        }

        #endregion Event Handlers - Misc

        #region Event Handlers - Toolbar

        private void TsScan_ButtonClick(object sender, EventArgs e)
        {
            ScanDefault();
        }

        private void TsNewProfile_Click(object sender, EventArgs e)
        {
            ScanWithNewProfile();
        }

        private void tsBatchScan_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBatchScan>();
            form.ImageCallback = ReceiveScannedImage;
            form.ShowDialog();
            UpdateScanButton();
        }

        private void tsProfiles_Click(object sender, EventArgs e)
        {
            ShowProfilesForm();
        }

        private void tsOcr_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideOcrButton)
            {
                return;
            }

            if (ocrDependencyManager.TesseractExeRequiresFix && !appConfigManager.Config.NoUpdatePrompt)
            {
                // Re-download a fixed version on Windows XP if needed
                MessageBox.Show(MiscResources.OcrUpdateAvailable, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var progressForm = FormFactory.Create<FDownloadProgress>();
                progressForm.QueueFile(ocrDependencyManager.Downloads.Tesseract304Xp,
                    ocrDependencyManager.Components.Tesseract304Xp.Install);
                progressForm.ShowDialog();
            }

            if (ocrDependencyManager.InstalledTesseractExe != null && ocrDependencyManager.InstalledTesseractLanguages.Any())
            {
                if (!ocrDependencyManager.HasNewTesseractExe && !appConfigManager.Config.NoUpdatePrompt)
                {
                    MessageBox.Show(MiscResources.OcrUpdateAvailable, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                }
                FormFactory.Create<FOcrSetup>().ShowDialog();
            }
            else
            {
                FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                if (ocrDependencyManager.InstalledTesseractExe != null && ocrDependencyManager.InstalledTesseractLanguages.Any())
                {
                    FormFactory.Create<FOcrSetup>().ShowDialog();
                }
            }
        }

        private void tsImport_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideImportButton)
            {
                return;
            }

            Import();
        }

        private void tsdSavePDF_ButtonClick(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSavePdfButton)
            {
                return;
            }

            var action = appConfigManager.Config.SaveButtonDefaultAction;

            if (action == SaveButtonDefaultAction.AlwaysPrompt
                || (action == SaveButtonDefaultAction.PromptIfSelected && SelectedIndices.Any()))
            {
                tsdSavePDF.ShowDropDown();
            }
            else if (action == SaveButtonDefaultAction.SaveSelected && SelectedIndices.Any())
            {
                SavePDF(SelectedImages.ToList());
            }
            else
            {
                SavePDF(imageList.Images);
            }
        }

        private void tsdSaveImages_ButtonClick(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                return;
            }

            var action = appConfigManager.Config.SaveButtonDefaultAction;

            if (action == SaveButtonDefaultAction.AlwaysPrompt
                || (action == SaveButtonDefaultAction.PromptIfSelected && SelectedIndices.Any()))
            {
                tsdSaveImages.ShowDropDown();
            }
            else if (action == SaveButtonDefaultAction.SaveSelected && SelectedIndices.Any())
            {
                SaveImages(SelectedImages.ToList());
            }
            else
            {
                SaveImages(imageList.Images);
            }
        }

        private void tsdEmailPDF_ButtonClick(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideEmailButton)
            {
                return;
            }

            var action = appConfigManager.Config.SaveButtonDefaultAction;

            if (action == SaveButtonDefaultAction.AlwaysPrompt
                || (action == SaveButtonDefaultAction.PromptIfSelected && SelectedIndices.Any()))
            {
                tsdEmailPDF.ShowDropDown();
            }
            else if (action == SaveButtonDefaultAction.SaveSelected && SelectedIndices.Any())
            {
                EmailPDF(SelectedImages.ToList());
            }
            else
            {
                EmailPDF(imageList.Images);
            }
        }

        private void tsPrint_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HidePrintButton)
            {
                return;
            }

            changeTracker.HasUnsavedChanges &= !scannedImagePrinter.PromptToPrint(imageList.Images, SelectedImages.ToList());
        }

        private void tsMove_ClickFirst(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void tsMove_ClickSecond(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void TsDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void tsClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FAbout>().ShowDialog();
        }

        #endregion Event Handlers - Toolbar

        #region Event Handlers - Save/Email Menus

        private void TsSavePDFAll_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSavePdfButton)
            {
                return;
            }

            SavePDF(imageList.Images);
        }

        private void TsSavePDFSelected_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSavePdfButton)
            {
                return;
            }

            SavePDF(SelectedImages.ToList());
        }

        private void tsPDFSettings_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FPdfSettings>().ShowDialog();
        }

        private void TsSaveImagesAll_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                return;
            }

            SaveImages(imageList.Images);
        }

        private void TsSaveImagesSelected_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                return;
            }

            SaveImages(SelectedImages.ToList());
        }

        private void tsImageSettings_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FImageSettings>().ShowDialog();
        }

        private void tsEmailPDFAll_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideEmailButton)
            {
                return;
            }

            EmailPDF(imageList.Images);
        }

        private void tsEmailPDFSelected_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideEmailButton)
            {
                return;
            }

            EmailPDF(SelectedImages.ToList());
        }

        private void tsPdfSettings2_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FPdfSettings>().ShowDialog();
        }

        private void tsEmailSettings_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FEmailSettings>().ShowDialog();
        }

        #endregion Event Handlers - Save/Email Menus

        #region Event Handlers - Image Menu

        private void tsView_Click(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void TsCrop_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FCrop>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
            }
        }

        private void TsBrightnessContrast_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FBrightnessContrast>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
            }
        }

        private void TsHueSaturation_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FHueSaturation>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
            }
        }

        private void TsBlackWhite_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FBlackWhite>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
            }
        }

        private void TsSharpen_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FSharpen>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
            }
        }

        private void tsReset_Click(object sender, EventArgs e)
        {
            ResetImage();
        }

        #endregion Event Handlers - Image Menu

        #region Event Handlers - Rotate Menu

        private void TsRotateLeft_Click(object sender, EventArgs e)
        {
            RotateLeft();
        }

        private void TsRotateRight_Click(object sender, EventArgs e)
        {
            RotateRight();
        }

        private void TsFlip_Click(object sender, EventArgs e)
        {
            Flip();
        }

        private void TsDeskew_Click(object sender, EventArgs e)
        {
            Deskew();
        }

        private void TsCustomRotation_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FRotate>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails(SelectedIndices.ToList(), false, true);
            }
        }

        #endregion Event Handlers - Rotate Menu

        #region Event Handlers - Reorder Menu

        private void tsInterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.Interleave(SelectedIndices), true, false);
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsDeinterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.Deinterleave(SelectedIndices), true, false);
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsAltInterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.AltInterleave(SelectedIndices), true, false);
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsAltDeinterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.AltDeinterleave(SelectedIndices), true, false);
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsReverseAll_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 2)
            {
                return;
            }
            UpdateThumbnails(imageList.Reverse(), true, false);
            changeTracker.HasUnsavedChanges = true;
        }

        private void tsReverseSelected_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Count() < 2)
            {
                return;
            }
            UpdateThumbnails(imageList.Reverse(SelectedIndices), true, true);
            changeTracker.HasUnsavedChanges = true;
        }

        #endregion Event Handlers - Reorder Menu

        #region Context Menu

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ctxPaste.Enabled = CanPaste;
            e.Cancel |= (imageList.Images.Count == 0 && !ctxPaste.Enabled);
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

        private void ctxPaste_Click(object sender, EventArgs e)
        {
            PasteImages();
        }

        private void ctxDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        #endregion Context Menu

        #region Clipboard

        private void CopyImages()
        {
            if (SelectedIndices.Any())
            {
                var ido = GetDataObjectForImages(SelectedImages, true);
                Clipboard.SetDataObject(ido);
            }
        }

        private void PasteImages()
        {
            var ido = Clipboard.GetDataObject();
            if (ido == null)
            {
                return;
            }
            if (ido.GetDataPresent(typeof(DirectImageTransfer).FullName))
            {
                var data = (DirectImageTransfer)ido.GetData(typeof(DirectImageTransfer).FullName);
                ImportDirect(data, true);
            }
        }

        private bool CanPaste
        {
            get
            {
                var ido = Clipboard.GetDataObject();
                return ido?.GetDataPresent(typeof(DirectImageTransfer).FullName) == true;
            }
        }

        private IDataObject GetDataObjectForImages(IEnumerable<ScannedImage> images, bool includeBitmap)
        {
            var scannedImages = images.ToList();
            IDataObject ido = new DataObject();
            if (scannedImages.Count == 0)
            {
                return ido;
            }
            if (includeBitmap)
            {
                using (var firstBitmap = scannedImageRenderer.Render(scannedImages[0]))
                {
                    ido.SetData(DataFormats.Bitmap, true, new Bitmap(firstBitmap));
                    ido.SetData(DataFormats.Rtf, true, RtfEncodeImages(firstBitmap, scannedImages));
                }
            }
            ido.SetData(typeof(DirectImageTransfer), new DirectImageTransfer(scannedImages));
            return ido;
        }

        private string RtfEncodeImages(Bitmap firstBitmap, List<ScannedImage> images)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            if (!AppendRtfEncodedImage(firstBitmap, images[0].FileFormat, sb, false))
            {
                return null;
            }
            foreach (var img in images.Skip(1))
            {
                using (var bitmap = scannedImageRenderer.Render(img))
                {
                    if (!AppendRtfEncodedImage(bitmap, img.FileFormat, sb, true))
                    {
                        break;
                    }
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static bool AppendRtfEncodedImage(Image image, ImageFormat format, StringBuilder sb, bool par)
        {
            const int maxRtfSize = 20 * 1000 * 1000;
            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                if (sb.Length + (stream.Length * 2) > maxRtfSize)
                {
                    return false;
                }

                if (par)
                {
                    sb.Append(@"\par");
                }
                sb.Append(@"{\pict\pngblip\picw");
                sb.Append(image.Width);
                sb.Append(@"\pich");
                sb.Append(image.Height);
                sb.Append(@"\picwgoa");
                sb.Append(image.Width);
                sb.Append(@"\pichgoa");
                sb.Append(image.Height);
                sb.Append(@"\hex ");
                // Do a "low-level" conversion to save on memory by avoiding intermediate representations
                stream.Seek(0, SeekOrigin.Begin);
                int value;
                while ((value = stream.ReadByte()) != -1)
                {
                    int hi = value / 16, lo = value % 16;
                    sb.Append(GetHexChar(hi));
                    sb.Append(GetHexChar(lo));
                }
                sb.Append("}");
            }
            return true;
        }

        private static char GetHexChar(int n)
        {
            return (char)(n < 10 ? '0' + n : 'A' + (n - 10));
        }

        #endregion Clipboard

        #region Thumbnail Resizing

        private void StepThumbnailSize(double step)
        {
            int thumbnailSize = UserConfigManager.Config.ThumbnailSize;
            thumbnailSize = (int)ThumbnailRenderer.StepNumberToSize(ThumbnailRenderer.SizeToStepNumber(thumbnailSize) + step);
            thumbnailSize = Math.Max(Math.Min(thumbnailSize, ThumbnailRenderer.MAX_SIZE), ThumbnailRenderer.MIN_SIZE);
            ResizeThumbnails(thumbnailSize);
        }

        private void ResizeThumbnails(int thumbnailSize)
        {
            if (imageList.Images.Count == 0)
            {
                // Can't show visual feedback so don't do anything
                return;
            }
            if (ThumbnailList1.ThumbnailSize.Height == thumbnailSize)
            {
                // Same size so no resizing needed
                return;
            }

            // Save the new size to config
            UserConfigManager.Config.ThumbnailSize = thumbnailSize;
            UserConfigManager.Save();
            UpdateToolbar();
            // Adjust the visible thumbnail display with the new size
            ThumbnailList1.ThumbnailSize = new Size(thumbnailSize, thumbnailSize);
            ThumbnailList1.RegenerateThumbnailList(imageList.Images);

            SetThumbnailSpacing(thumbnailSize);

            // Render high-quality thumbnails at the new size in a background task
            // The existing (poorly scaled) thumbnails are used in the meantime
            RenderThumbnails(thumbnailSize, imageList.Images.ToList());
        }

        private void SetThumbnailSpacing(int thumbnailSize)
        {
            ThumbnailList1.Padding = new Padding(0, 20, 0, 0);
            const int MIN_PADDING = 6;
            const int MAX_PADDING = 66;
            // Linearly scale the padding with the thumbnail size
            int padding = MIN_PADDING + ((MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailRenderer.MIN_SIZE) / (ThumbnailRenderer.MAX_SIZE - ThumbnailRenderer.MIN_SIZE));
            int spacing = thumbnailSize + (padding * 2);
            SetListSpacing(ThumbnailList1, spacing, spacing);
        }

        private void SetListSpacing(ListView list, int hspacing, int vspacing)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            Win32.SendMessage(list.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)(int)(((ushort)hspacing) | (uint)(vspacing << 16)));
        }

        private void RenderThumbnails(int thumbnailSize, IEnumerable<ScannedImage> imagesToRenderThumbnailsFor)
        {
            // Cancel any previous task so that no two run at the same time
            renderThumbnailsCts?.Cancel();
            renderThumbnailsCts = new CancellationTokenSource();
            var ct = renderThumbnailsCts.Token;
            Task.Factory.StartNew(() =>
            {
                foreach (var img in imagesToRenderThumbnailsFor)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    object oldState;
                    Bitmap thumbnail;
                    // Lock the image to prevent it from being disposed mid-render
                    lock (img)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        // Save the state to check later for concurrent changes
                        oldState = img.GetThumbnailState();
                        // Render the thumbnail
                        try
                        {
                            thumbnail = thumbnailRenderer.RenderThumbnail(img, thumbnailSize);
                        }
                        catch
                        {
                            // An error occurred, which could mean the image was deleted
                            // In any case we don't need to worry too much about it and can move on to the next
                            continue;
                        }
                    }
                    // Do the rest of the stuff on the UI thread to help with synchronization
                    ScannedImage img1 = img;
                    SafeInvoke(() =>
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        // Check for concurrent transformations
                        if (oldState != img1.GetThumbnailState())
                        {
                            // The thumbnail has been concurrently updated
                            return;
                        }
                        // Checks passed, so use the newly rendered thumbnail at the appropriate index
                        img1.SetThumbnail(thumbnail);
                        int index = imageList.Images.IndexOf(img1);
                        if (index != -1)
                        {
                            ThumbnailList1.ReplaceThumbnail(index, img1);
                        }
                    });
                }
            }, ct);
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            StepThumbnailSize(-1);
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            StepThumbnailSize(1);
        }

        #endregion Thumbnail Resizing

        #region Drag/Drop

        private void ThumbnailList1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Provide drag data
            if (SelectedIndices.Any())
            {
                var ido = GetDataObjectForImages(SelectedImages, false);
                DoDragDrop(ido, DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void ThumbnailList1_DragEnter(object sender, DragEventArgs e)
        {
            // Determine if drop data is compatible
            try
            {
                if (e.Data.GetDataPresent(typeof(DirectImageTransfer).FullName))
                {
                    var data = (DirectImageTransfer)e.Data.GetData(typeof(DirectImageTransfer).FullName);
                    e.Effect = data.ProcessID == Process.GetCurrentProcess().Id
                        ? DragDropEffects.Move
                        : DragDropEffects.Copy;
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error receiving drag/drop", ex);
            }
        }

        private void ThumbnailList1_DragDrop(object sender, DragEventArgs e)
        {
            // Receive drop data
            if (e.Data.GetDataPresent(typeof(DirectImageTransfer).FullName))
            {
                var data = (DirectImageTransfer)e.Data.GetData(typeof(DirectImageTransfer).FullName);
                if (data.ProcessID == Process.GetCurrentProcess().Id)
                {
                    DragMoveImages(e);
                }
                else
                {
                    ImportDirect(data, false);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = (string[])e.Data.GetData(DataFormats.FileDrop);
                ImportFiles(data);
            }
            ThumbnailList1.InsertionMark.Index = -1;
        }

        private void ThumbnailList1_DragLeave(object sender, EventArgs e)
        {
            ThumbnailList1.InsertionMark.Index = -1;
        }

        private void DragMoveImages(DragEventArgs e)
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            int index = GetDragIndex(e);
            if (index != -1)
            {
                UpdateThumbnails(imageList.MoveTo(SelectedIndices, index), true, true);
                changeTracker.HasUnsavedChanges = true;
            }
        }

        private void ThumbnailList1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                var index = GetDragIndex(e);
                if (index == imageList.Images.Count)
                {
                    ThumbnailList1.InsertionMark.Index = index - 1;
                    ThumbnailList1.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    ThumbnailList1.InsertionMark.Index = index;
                    ThumbnailList1.InsertionMark.AppearsAfterItem = false;
                }
            }
        }

        private int GetDragIndex(DragEventArgs e)
        {
            Point cp = ThumbnailList1.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = ThumbnailList1.GetItemAt(cp.X, cp.Y);
            if (dragToItem == null)
            {
                var items = ThumbnailList1.Items.Cast<ListViewItem>().ToList();
                var minY = items.Select(x => x.Bounds.Top).Min();
                var maxY = items.Select(x => x.Bounds.Bottom).Max();
                if (cp.Y < minY)
                {
                    cp.Y = minY;
                }
                if (cp.Y > maxY)
                {
                    cp.Y = maxY;
                }
                var row = items.Where(x => x.Bounds.Top <= cp.Y && x.Bounds.Bottom >= cp.Y).OrderBy(x => x.Bounds.X).ToList();
                dragToItem = row.Find(x => x.Bounds.Right >= cp.X) ?? row.LastOrDefault();
            }
            if (dragToItem == null)
            {
                return -1;
            }
            int dragToIndex = dragToItem.ImageIndex;
            if (cp.X > (dragToItem.Bounds.X + (dragToItem.Bounds.Width / 2)))
            {
                dragToIndex++;
            }
            return dragToIndex;
        }

        #endregion Drag/Drop
    }
}