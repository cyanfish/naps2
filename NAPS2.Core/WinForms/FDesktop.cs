#region Usings

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
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Platform;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Wia;
using NAPS2.Scan.Wia.Native;
using NAPS2.Update;
using NAPS2.Util;
using NAPS2.Worker;

#endregion

namespace NAPS2.WinForms
{
    public partial class FDesktop : FormBase
    {
        #region Dependencies

        private static readonly MethodInfo ToolStripPanelSetStyle = typeof(ToolStripPanel).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly StringWrapper stringWrapper;
        private readonly AppConfigManager appConfigManager;
        private readonly RecoveryManager recoveryManager;
        private readonly OcrManager ocrManager;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IScannedImagePrinter scannedImagePrinter;
        private readonly ChangeTracker changeTracker;
        private readonly StillImage stillImage;
        private readonly IOperationFactory operationFactory;
        private readonly IUserConfigManager userConfigManager;
        private readonly KeyboardShortcutManager ksm;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly WinFormsExportHelper exportHelper;
        private readonly ScannedImageRenderer scannedImageRenderer;
        private readonly NotificationManager notify;
        private readonly CultureInitializer cultureInitializer;
        private readonly IWorkerServiceFactory workerServiceFactory;
        private readonly IOperationProgress operationProgress;
        private readonly UpdateChecker updateChecker;

        #endregion

        #region State Fields

        private readonly ScannedImageList imageList = new ScannedImageList();
        private readonly AutoResetEvent renderThumbnailsWaitHandle = new AutoResetEvent(false);
        private bool closed = false;
        private LayoutManager layoutManager;
        private bool disableSelectedIndexChangedEvent;

        #endregion

        #region Initialization and Culture

        public FDesktop(StringWrapper stringWrapper, AppConfigManager appConfigManager, RecoveryManager recoveryManager, OcrManager ocrManager, IProfileManager profileManager, IScanPerformer scanPerformer, IScannedImagePrinter scannedImagePrinter, ChangeTracker changeTracker, StillImage stillImage, IOperationFactory operationFactory, IUserConfigManager userConfigManager, KeyboardShortcutManager ksm, ThumbnailRenderer thumbnailRenderer, WinFormsExportHelper exportHelper, ScannedImageRenderer scannedImageRenderer, NotificationManager notify, CultureInitializer cultureInitializer, IWorkerServiceFactory workerServiceFactory, IOperationProgress operationProgress, UpdateChecker updateChecker)
        {
            this.stringWrapper = stringWrapper;
            this.appConfigManager = appConfigManager;
            this.recoveryManager = recoveryManager;
            this.ocrManager = ocrManager;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.scannedImagePrinter = scannedImagePrinter;
            this.changeTracker = changeTracker;
            this.stillImage = stillImage;
            this.operationFactory = operationFactory;
            this.userConfigManager = userConfigManager;
            this.ksm = ksm;
            this.thumbnailRenderer = thumbnailRenderer;
            this.exportHelper = exportHelper;
            this.scannedImageRenderer = scannedImageRenderer;
            this.notify = notify;
            this.cultureInitializer = cultureInitializer;
            this.workerServiceFactory = workerServiceFactory;
            this.operationProgress = operationProgress;
            this.updateChecker = updateChecker;
            InitializeComponent();

            notify.ParentForm = this;
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
            foreach (var panel in toolStripContainer1.Controls.OfType<ToolStripPanel>())
            {
                ToolStripPanelSetStyle.Invoke(panel, new object[] { ControlStyles.Selectable, true });
            }
            imageList.ThumbnailRenderer = thumbnailRenderer;
            thumbnailList1.ThumbnailRenderer = thumbnailRenderer;
            int thumbnailSize = UserConfigManager.Config.ThumbnailSize;
            thumbnailList1.ThumbnailSize = new Size(thumbnailSize, thumbnailSize);
            SetThumbnailSpacing(thumbnailSize);

            if (appConfigManager.Config.HideOcrButton)
            {
                tStrip.Items.Remove(tsOcr);
            }
            if (appConfigManager.Config.HideImportButton)
            {
                tStrip.Items.Remove(tsImport);
            }
            if (appConfigManager.Config.HideSavePdfButton)
            {
                tStrip.Items.Remove(tsdSavePDF);
            }
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                tStrip.Items.Remove(tsdSaveImages);
            }
            if (appConfigManager.Config.HideEmailButton)
            {
                tStrip.Items.Remove(tsdEmailPDF);
            }
            if (appConfigManager.Config.HidePrintButton)
            {
                tStrip.Items.Remove(tsPrint);
            }

            LoadToolStripLocation();
            RelayoutToolbar();
            InitLanguageDropdown();
            AssignKeyboardShortcuts();
            UpdateScanButton();

            layoutManager?.Deactivate();
            btnZoomIn.Location = new Point(btnZoomIn.Location.X, thumbnailList1.Height - 33);
            btnZoomOut.Location = new Point(btnZoomOut.Location.X, thumbnailList1.Height - 33);
            btnZoomMouseCatcher.Location = new Point(btnZoomMouseCatcher.Location.X, thumbnailList1.Height - 33);
            layoutManager = new LayoutManager(this)
                   .Bind(btnZoomIn, btnZoomOut, btnZoomMouseCatcher)
                       .BottomTo(() => thumbnailList1.Height)
                   .Activate();

            thumbnailList1.MouseWheel += thumbnailList1_MouseWheel;
            thumbnailList1.SizeChanged += (sender, args) => layoutManager.UpdateLayout();
        }

        private void InitLanguageDropdown()
        {
            // Read a list of languages from the Languages.resx file
            var resourceManager = LanguageNames.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
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
            // Resize and wrap text as necessary
            using (var g = CreateGraphics())
            {
                foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
                {
                    if (PlatformCompat.Runtime.SetToolbarFont)
                    {
                        btn.Font = new Font("Segoe UI", 9);
                    }
                    btn.Text = stringWrapper.Wrap(btn.Text ?? "", 80, g, btn.Font);
                }
            }
            ResetToolbarMargin();
            // Recalculate visibility for the below check
            Application.DoEvents();
            // Check if toolbar buttons are overflowing
            if (tStrip.Items.OfType<ToolStripItem>().Any(btn => !btn.Visible)
                && (tStrip.Parent.Dock == DockStyle.Top || tStrip.Parent.Dock == DockStyle.Bottom))
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
                    if (tStrip.Parent.Dock == DockStyle.Left || tStrip.Parent.Dock == DockStyle.Right)
                    {
                        btn.Margin = new Padding(10, 1, 5, 2);
                    }
                    else
                    {
                        btn.Margin = new Padding(5, 1, 5, 2);
                    }
                }
                else if (btn is ToolStripDoubleButton)
                {
                    btn.Padding = new Padding(5, 0, 5, 0);
                }
                else if (tStrip.Parent.Dock == DockStyle.Left || tStrip.Parent.Dock == DockStyle.Right)
                {
                    btn.Margin = new Padding(0, 1, 5, 2);
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

        private void SetCulture(string cultureId)
        {
            SaveToolStripLocation();
            UserConfigManager.Config.Culture = cultureId;
            UserConfigManager.Save();
            cultureInitializer.InitCulture(Thread.CurrentThread);

            // Update localized values
            // Since all forms are opened modally and this is the root form, it should be the only one that needs to be updated live
            SaveFormState = false;
            Controls.Clear();
            UpdateRTL();
            InitializeComponent();
            PostInitializeComponent();
            AddThumbnails();
            notify.Rebuild();
            Focus();
            WindowState = FormWindowState.Normal;
            DoRestoreFormState();
            SaveFormState = true;
        }

        private async void FDesktop_Shown(object sender, EventArgs e)
        {
            UpdateToolbar();

            // Receive messages from other processes
            Pipes.StartServer(msg =>
            {
                if (msg.StartsWith(Pipes.MSG_SCAN_WITH_DEVICE, StringComparison.InvariantCulture))
                {
                    SafeInvoke(async () => await ScanWithDevice(msg.Substring(Pipes.MSG_SCAN_WITH_DEVICE.Length)));
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
            recoveryManager.RecoverScannedImages(ReceiveScannedImage());

            new Thread(RenderThumbnails).Start();

            // If NAPS2 was started by the scanner button, do the appropriate actions automatically
            await RunStillImageEvents();

            // Show a donation prompt after a month of use
            if (userConfigManager.Config.FirstRunDate == null)
            {
                userConfigManager.Config.FirstRunDate = DateTime.Now;
                userConfigManager.Save();
            }
#if !INSTALLER_MSI
            else if (!appConfigManager.Config.HideDonateButton &&
                userConfigManager.Config.LastDonatePromptDate == null &&
                DateTime.Now - userConfigManager.Config.FirstRunDate > TimeSpan.FromDays(30))
            {
                userConfigManager.Config.LastDonatePromptDate = DateTime.Now;
                userConfigManager.Save();
                notify.DonatePrompt();
            }

            if (userConfigManager.Config.CheckForUpdates &&
                (userConfigManager.Config.LastUpdateCheckDate == null ||
                 userConfigManager.Config.LastUpdateCheckDate < DateTime.Now - updateChecker.CheckInterval))
            {
                updateChecker.CheckForUpdates().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.ErrorException("Error checking for updates", task.Exception);
                    }
                    else
                    {
                        userConfigManager.Config.LastUpdateCheckDate = DateTime.Now;
                        userConfigManager.Save();
                    }
                    var update = task.Result;
                    if (update != null)
                    {
                        SafeInvoke(() => notify.UpdateAvailable(updateChecker, update));
                    }
                }).AssertNoAwait();
            }
#endif
        }

        #endregion

        #region Cleanup

        private void FDesktop_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closed) return;

            if (operationProgress.ActiveOperations.Any())
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (operationProgress.ActiveOperations.Any(x => !x.SkipExitPrompt))
                    {
                        var result = MessageBox.Show(MiscResources.ExitWithActiveOperations, MiscResources.ActiveOperations,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                        {
                            e.Cancel = true;
                        }
                    }
                }
                else
                {
                    RecoveryImage.DisableRecoveryCleanup = true;
                }
            }
            else if (changeTracker.HasUnsavedChanges)
            {
                if (e.CloseReason == CloseReason.UserClosing && !RecoveryImage.DisableRecoveryCleanup)
                {
                    var result = MessageBox.Show(MiscResources.ExitWithUnsavedChanges, MiscResources.UnsavedChanges,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        changeTracker.Clear();
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

            if (!e.Cancel && operationProgress.ActiveOperations.Any())
            {
                operationProgress.ActiveOperations.ForEach(op => op.Cancel());
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
                Task.Factory.StartNew(() =>
                {
                    var timeoutCts = new CancellationTokenSource();
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));
                    try
                    {
                        operationProgress.ActiveOperations.ForEach(op => op.Wait(timeoutCts.Token));
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    closed = true;
                    SafeInvoke(Close);
                });
            }
        }

        private void FDesktop_Closed(object sender, EventArgs e)
        {
            SaveToolStripLocation();
            Pipes.KillServer();
            imageList.Delete(Enumerable.Range(0, imageList.Images.Count));
            closed = true;
            renderThumbnailsWaitHandle.Set();
        }

        #endregion

        #region Scanning and Still Image

        private async Task RunStillImageEvents()
        {
            if (stillImage.ShouldScan)
            {
                await ScanWithDevice(stillImage.DeviceID);
            }
        }

        private async Task ScanWithDevice(string deviceID)
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
                profile = profileManager.Profiles.FirstOrDefault(x => x.Device != null && x.Device.ID == deviceID);
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
                    using (var deviceManager = new WiaDeviceManager())
                    using (var device = deviceManager.FindDevice(deviceID))
                    {
                        editSettingsForm.CurrentDevice = new ScanDevice(deviceID, device.Name());
                    }
                }
                catch (WiaException)
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
                await scanPerformer.PerformScan(profile, new ScanParams(), this, notify, ReceiveScannedImage());
                Activate();
            }
        }

        private async Task ScanDefault()
        {
            if (profileManager.DefaultProfile != null)
            {
                await scanPerformer.PerformScan(profileManager.DefaultProfile, new ScanParams(), this, notify, ReceiveScannedImage());
                Activate();
            }
            else if (profileManager.Profiles.Count == 0)
            {
                await ScanWithNewProfile();
            }
            else
            {
                ShowProfilesForm();
            }
        }

        private async Task ScanWithNewProfile()
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

            await scanPerformer.PerformScan(editSettingsForm.ScanProfile, new ScanParams(), this, notify, ReceiveScannedImage());
            Activate();
        }

        #endregion

        #region Images and Thumbnails

        private IEnumerable<int> SelectedIndices
        {
            get => thumbnailList1.SelectedIndices.Cast<int>();
            set
            {
                disableSelectedIndexChangedEvent = true;
                thumbnailList1.SelectedIndices.Clear();
                foreach (int i in value)
                {
                    thumbnailList1.SelectedIndices.Add(i);
                }
                disableSelectedIndexChangedEvent = false;
                thumbnailList1_SelectedIndexChanged(thumbnailList1, new EventArgs());
            }
        }

        private IEnumerable<ScannedImage> SelectedImages => imageList.Images.ElementsAt(SelectedIndices);

        /// <summary>
        /// Constructs a receiver for scanned images.
        /// This keeps images from the same source together, even if multiple sources are providing images at the same time.
        /// </summary>
        /// <returns></returns>
        public Action<ScannedImage> ReceiveScannedImage()
        {
            ScannedImage last = null;
            return scannedImage =>
            {
                SafeInvoke(() =>
                {
                    lock (imageList)
                    {
                        // Default to the end of the list
                        int index = imageList.Images.Count;
                        // Use the index after the last image from the same source (if it exists)
                        if (last != null)
                        {
                            int lastIndex = imageList.Images.IndexOf(last);
                            if (lastIndex != -1)
                            {
                                index = lastIndex + 1;
                            }
                        }
                        imageList.Images.Insert(index, scannedImage);
                        scannedImage.MovedTo(index);
                        scannedImage.ThumbnailChanged += ImageThumbnailChanged;
                        scannedImage.ThumbnailInvalidated += ImageThumbnailInvalidated;
                        AddThumbnails();
                        last = scannedImage;
                    }
                    changeTracker.Made();
                });
                // Trigger thumbnail rendering just in case the received image is out of date
                renderThumbnailsWaitHandle.Set();
            };
        }

        private void AddThumbnails()
        {
            thumbnailList1.AddedImages(imageList.Images);
            UpdateToolbar();
        }

        private void DeleteThumbnails()
        {
            thumbnailList1.DeletedImages(imageList.Images);
            UpdateToolbar();
        }

        private void UpdateThumbnails(IEnumerable<int> selection, bool scrollToSelection, bool optimizeForSelection)
        {
            thumbnailList1.UpdatedImages(imageList.Images, optimizeForSelection ? SelectedIndices.Concat(selection).ToList() : null);
            SelectedIndices = selection;
            UpdateToolbar();

            if (scrollToSelection)
            {
                // Scroll to selection
                // If selection is empty (e.g. after interleave), this scrolls to top
                thumbnailList1.EnsureVisible(SelectedIndices.LastOrDefault());
                thumbnailList1.EnsureVisible(SelectedIndices.FirstOrDefault());
            }
        }

        private void ImageThumbnailChanged(object sender, EventArgs e)
        {
            SafeInvokeAsync(() =>
            {
                var image = (ScannedImage)sender;
                lock (image)
                {
                    lock (imageList)
                    {
                        int index = imageList.Images.IndexOf(image);
                        if (index != -1)
                        {
                            thumbnailList1.ReplaceThumbnail(index, image);
                        }
                    }
                }
            });
        }

        private void ImageThumbnailInvalidated(object sender, EventArgs e)
        {
            SafeInvokeAsync(() =>
            {
                var image = (ScannedImage)sender;
                lock (image)
                {
                    lock (imageList)
                    {
                        int index = imageList.Images.IndexOf(image);
                        if (index != -1 && image.IsThumbnailDirty)
                        {
                            thumbnailList1.ReplaceThumbnail(index, image);
                        }
                    }
                }
                renderThumbnailsWaitHandle.Set();
            });
        }

        #endregion

        #region Toolbar

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
            tsdReorder.Enabled = tsdSavePDF.Enabled = tsdSaveImages.Enabled = tsdEmailPDF.Enabled = tsPrint.Enabled = tsClear.Enabled = imageList.Images.Any();

            // Context-menu actions
            ctxView.Visible = ctxCopy.Visible = ctxDelete.Visible = ctxSeparator1.Visible = ctxSeparator2.Visible = SelectedIndices.Any();
            ctxSelectAll.Enabled = imageList.Images.Any();

            // Other
            btnZoomIn.Enabled = imageList.Images.Any() && UserConfigManager.Config.ThumbnailSize < ThumbnailRenderer.MAX_SIZE;
            btnZoomOut.Enabled = imageList.Images.Any() && UserConfigManager.Config.ThumbnailSize > ThumbnailRenderer.MIN_SIZE;
            tsNewProfile.Enabled = !(appConfigManager.Config.NoUserProfiles && profileManager.Profiles.Any(x => x.IsLocked));

            if (PlatformCompat.Runtime.RefreshListViewAfterChange)
            {
                thumbnailList1.Size = new Size(thumbnailList1.Width - 1, thumbnailList1.Height - 1);
                thumbnailList1.Size = new Size(thumbnailList1.Width + 1, thumbnailList1.Height + 1);
            }
        }

        private void UpdateScanButton()
        {
            const int staticButtonCount = 2;

            // Clean up the dropdown
            while (tsScan.DropDownItems.Count > staticButtonCount)
            {
                tsScan.DropDownItems.RemoveAt(0);
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
                item.Click += async (sender, args) =>
                {
                    profileManager.DefaultProfile = profile;
                    profileManager.Save();

                    UpdateScanButton();

                    await scanPerformer.PerformScan(profile, new ScanParams(), this, notify, ReceiveScannedImage());
                    Activate();
                };
                tsScan.DropDownItems.Insert(tsScan.DropDownItems.Count - staticButtonCount, item);

                i++;
            }

            if (profileManager.Profiles.Any())
            {
                tsScan.DropDownItems.Insert(tsScan.DropDownItems.Count - staticButtonCount, new ToolStripSeparator());
            }
        }

        private void SaveToolStripLocation()
        {
            UserConfigManager.Config.DesktopToolStripDock = tStrip.Parent.Dock;
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
                    tStrip.Parent = panel;
                }
            }
            tStrip.Parent.TabStop = true;
        }

        #endregion

        #region Actions

        private void Clear()
        {
            if (imageList.Images.Count > 0)
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmClearItems, imageList.Images.Count), MiscResources.Clear, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    imageList.Delete(Enumerable.Range(0, imageList.Images.Count));
                    DeleteThumbnails();
                    changeTracker.Clear();
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
                    DeleteThumbnails();
                    if (imageList.Images.Any())
                    {
                        changeTracker.Made();
                    }
                    else
                    {
                        changeTracker.Clear();
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
            UpdateThumbnails(imageList.MoveDown(SelectedIndices), true, true);
            changeTracker.Made();
        }

        private void MoveUp()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            UpdateThumbnails(imageList.MoveUp(SelectedIndices), true, true);
            changeTracker.Made();
        }

        private async Task RotateLeft()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            changeTracker.Made();
            await imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate270FlipNone);
            changeTracker.Made();
        }

        private async Task RotateRight()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            changeTracker.Made();
            await imageList.RotateFlip(SelectedIndices, RotateFlipType.Rotate90FlipNone);
            changeTracker.Made();
        }

        private async Task Flip()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }
            changeTracker.Made();
            await imageList.RotateFlip(SelectedIndices, RotateFlipType.RotateNoneFlipXY);
            changeTracker.Made();
        }

        private void Deskew()
        {
            if (!SelectedIndices.Any())
            {
                return;
            }

            var op = operationFactory.Create<DeskewOperation>();
            if (op.Start(SelectedImages.ToList()))
            {
                operationProgress.ShowProgress(op);
                changeTracker.Made();
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
                    viewer.DeleteCallback = DeleteThumbnails;
                    viewer.SelectCallback = i =>
                    {
                        if (SelectedIndices.Count() <= 1)
                        {
                            SelectedIndices = new[] { i };
                            thumbnailList1.Items[i].EnsureVisible();
                        }
                    };
                    viewer.ShowDialog();
                }
            }
        }

        private void ShowProfilesForm()
        {
            var form = FormFactory.Create<FProfiles>();
            form.ImageCallback = ReceiveScannedImage();
            form.ShowDialog();
            UpdateScanButton();
        }

        private void ResetImage()
        {
            if (SelectedIndices.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmResetImages, SelectedIndices.Count()), MiscResources.ResetImage, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    imageList.ResetTransforms(SelectedIndices);
                    changeTracker.Made();
                }
            }
        }

        #endregion

        #region Actions - Save/Email/Import

        private async void SavePDF(List<ScannedImage> images)
        {
            if (await exportHelper.SavePDF(images, notify))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    SafeInvoke(() =>
                    {
                        imageList.Delete(imageList.Images.IndiciesOf(images));
                        DeleteThumbnails();
                    });
                }
            }
        }

        private async void SaveImages(List<ScannedImage> images)
        {
            if (await exportHelper.SaveImages(images, notify))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    imageList.Delete(imageList.Images.IndiciesOf(images));
                    DeleteThumbnails();
                }
            }
        }

        private async void EmailPDF(List<ScannedImage> images)
        {
            await exportHelper.EmailPDF(images);
        }

        private void Import()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true,
                Filter = MiscResources.FileTypeAllFiles + @"|*.*|" +
                         MiscResources.FileTypePdf + @"|*.pdf|" +
                         MiscResources.FileTypeImageFiles + @"|*.bmp;*.emf;*.exif;*.gif;*.jpg;*.jpeg;*.png;*.tiff;*.tif|" +
                         MiscResources.FileTypeBmp + @"|*.bmp|" +
                         MiscResources.FileTypeEmf + @"|*.emf|" +
                         MiscResources.FileTypeExif + @"|*.exif|" +
                         MiscResources.FileTypeGif + @"|*.gif|" +
                         MiscResources.FileTypeJpeg + @"|*.jpg;*.jpeg|" +
                         MiscResources.FileTypePng + @"|*.png|" +
                         MiscResources.FileTypeTiff + @"|*.tiff;*.tif"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ImportFiles(ofd.FileNames);
            }
        }

        private void ImportFiles(IEnumerable<string> files)
        {
            var op = operationFactory.Create<ImportOperation>();
            if (op.Start(OrderFiles(files), ReceiveScannedImage()))
            {
                operationProgress.ShowProgress(op);
            }
        }

        private List<string> OrderFiles(IEnumerable<string> files)
        {
            // Custom ordering to account for numbers so that e.g. "10" comes after "2"
            var filesList = files.ToList();
            filesList.Sort(new NaturalStringComparer());
            return filesList;
        }

        private void ImportDirect(DirectImageTransfer data, bool copy)
        {
            var op = operationFactory.Create<DirectImportOperation>();
            if (op.Start(data, copy, ReceiveScannedImage()))
            {
                operationProgress.ShowProgress(op);
            }
        }

        #endregion

        #region Keyboard Shortcuts

        private void AssignKeyboardShortcuts()
        {
            // Defaults

            ksm.Assign("Ctrl+Enter", tsScan);
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
            ksm.Assign("Ctrl+OemMinus", btnZoomOut);
            ksm.Assign("Ctrl+Oemplus", btnZoomIn);
            ksm.Assign("Del", ctxDelete);
            ksm.Assign("Ctrl+A", ctxSelectAll);
            ksm.Assign("Ctrl+C", ctxCopy);
            ksm.Assign("Ctrl+V", ctxPaste);

            // Configured

            var ks = userConfigManager.Config.KeyboardShortcuts ?? appConfigManager.Config.KeyboardShortcuts ?? new KeyboardShortcuts();

            ksm.Assign(ks.About, tsAbout);
            ksm.Assign(ks.BatchScan, tsBatchScan);
            ksm.Assign(ks.Clear, tsClear);
            ksm.Assign(ks.Delete, tsDelete);
            ksm.Assign(ks.EmailPDF, tsdEmailPDF);
            ksm.Assign(ks.EmailPDFAll, tsEmailPDFAll);
            ksm.Assign(ks.EmailPDFSelected, tsEmailPDFSelected);
            ksm.Assign(ks.ImageBlackWhite, tsBlackWhite);
            ksm.Assign(ks.ImageBrightness, tsBrightnessContrast);
            ksm.Assign(ks.ImageContrast, tsBrightnessContrast);
            ksm.Assign(ks.ImageCrop, tsCrop);
            ksm.Assign(ks.ImageHue, tsHueSaturation);
            ksm.Assign(ks.ImageSaturation, tsHueSaturation);
            ksm.Assign(ks.ImageSharpen, tsSharpen);
            ksm.Assign(ks.ImageReset, tsReset);
            ksm.Assign(ks.ImageView, tsView);
            ksm.Assign(ks.Import, tsImport);
            ksm.Assign(ks.MoveDown, MoveDown); // TODO
            ksm.Assign(ks.MoveUp, MoveUp); // TODO
            ksm.Assign(ks.NewProfile, tsNewProfile);
            ksm.Assign(ks.Ocr, tsOcr);
            ksm.Assign(ks.Print, tsPrint);
            ksm.Assign(ks.Profiles, ShowProfilesForm);

            ksm.Assign(ks.ReorderAltDeinterleave, tsAltDeinterleave);
            ksm.Assign(ks.ReorderAltInterleave, tsAltInterleave);
            ksm.Assign(ks.ReorderDeinterleave, tsDeinterleave);
            ksm.Assign(ks.ReorderInterleave, tsInterleave);
            ksm.Assign(ks.ReorderReverseAll, tsReverseAll);
            ksm.Assign(ks.ReorderReverseSelected, tsReverseSelected);
            ksm.Assign(ks.RotateCustom, tsCustomRotation);
            ksm.Assign(ks.RotateFlip, tsFlip);
            ksm.Assign(ks.RotateLeft, tsRotateLeft);
            ksm.Assign(ks.RotateRight, tsRotateRight);
            ksm.Assign(ks.SaveImages, tsdSaveImages);
            ksm.Assign(ks.SaveImagesAll, tsSaveImagesAll);
            ksm.Assign(ks.SaveImagesSelected, tsSaveImagesSelected);
            ksm.Assign(ks.SavePDF, tsdSavePDF);
            ksm.Assign(ks.SavePDFAll, tsSavePDFAll);
            ksm.Assign(ks.SavePDFSelected, tsSavePDFSelected);
            ksm.Assign(ks.ScanDefault, tsScan);

            ksm.Assign(ks.ZoomIn, btnZoomIn);
            ksm.Assign(ks.ZoomOut, btnZoomOut);
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

        private void thumbnailList1_KeyDown(object sender, KeyEventArgs e)
        {
            ksm.Perform(e.KeyData);
        }

        private void thumbnailList1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                StepThumbnailSize(e.Delta / (double)SystemInformation.MouseWheelScrollDelta);
            }
        }

        #endregion

        #region Event Handlers - Misc

        private void thumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void thumbnailList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!disableSelectedIndexChangedEvent)
            {
                UpdateToolbar();
            }
        }

        private void thumbnailList1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = thumbnailList1.GetItemAt(e.X, e.Y) == null ? Cursors.Default : Cursors.Hand;
        }

        private void thumbnailList1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void tStrip_DockChanged(object sender, EventArgs e)
        {
            RelayoutToolbar();
        }

        #endregion

        #region Event Handlers - Toolbar

        private async void tsScan_ButtonClick(object sender, EventArgs e)
        {
            await ScanDefault();
        }

        private async void tsNewProfile_Click(object sender, EventArgs e)
        {
            await ScanWithNewProfile();
        }

        private void tsBatchScan_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBatchScan>();
            form.ImageCallback = ReceiveScannedImage();
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

            if (ocrManager.MustUpgrade && !appConfigManager.Config.NoUpdatePrompt)
            {
                // Re-download a fixed version on Windows XP if needed
                MessageBox.Show(MiscResources.OcrUpdateAvailable, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var progressForm = FormFactory.Create<FDownloadProgress>();
                progressForm.QueueFile(ocrManager.EngineToInstall.Component);
                progressForm.ShowDialog();
            }

            if (ocrManager.MustInstallPackage)
            {
                const string packages = "\ntesseract-ocr";
                MessageBox.Show(MiscResources.TesseractNotAvailable + packages, MiscResources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (ocrManager.IsReady)
            {
                if (ocrManager.CanUpgrade && !appConfigManager.Config.NoUpdatePrompt)
                {
                    MessageBox.Show(MiscResources.OcrUpdateAvailable, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                }
                FormFactory.Create<FOcrSetup>().ShowDialog();
            }
            else
            {
                FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                if (ocrManager.IsReady)
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
                || action == SaveButtonDefaultAction.PromptIfSelected && SelectedIndices.Any())
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
                || action == SaveButtonDefaultAction.PromptIfSelected && SelectedIndices.Any())
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
                || action == SaveButtonDefaultAction.PromptIfSelected && SelectedIndices.Any())
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

        private async void tsPrint_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HidePrintButton)
            {
                return;
            }

            var changeToken = changeTracker.State;
            if (await scannedImagePrinter.PromptToPrint(imageList.Images, SelectedImages.ToList()))
            {
                changeTracker.Saved(changeToken);
            }
        }

        private void tsMove_FirstClick(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void tsMove_SecondClick(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void tsDelete_Click(object sender, EventArgs e)
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

        #endregion

        #region Event Handlers - Save/Email Menus

        private void tsSavePDFAll_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSavePdfButton)
            {
                return;
            }

            SavePDF(imageList.Images);
        }

        private void tsSavePDFSelected_Click(object sender, EventArgs e)
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

        private void tsSaveImagesAll_Click(object sender, EventArgs e)
        {
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                return;
            }

            SaveImages(imageList.Images);
        }

        private void tsSaveImagesSelected_Click(object sender, EventArgs e)
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

        #endregion

        #region Event Handlers - Image Menu

        private void tsView_Click(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private void tsCrop_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FCrop>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
            }
        }

        private void tsBrightnessContrast_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FBrightnessContrast>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
            }
        }

        private void tsHueSaturation_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FHueSaturation>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
            }
        }

        private void tsBlackWhite_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FBlackWhite>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
            }
        }

        private void tsSharpen_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FSharpen>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
            }
        }

        private void tsReset_Click(object sender, EventArgs e)
        {
            ResetImage();
        }

        #endregion

        #region Event Handlers - Rotate Menu

        private async void tsRotateLeft_Click(object sender, EventArgs e)
        {
            await RotateLeft();
        }

        private async void tsRotateRight_Click(object sender, EventArgs e)
        {
            await RotateRight();
        }

        private async void tsFlip_Click(object sender, EventArgs e)
        {
            await Flip();
        }

        private void tsDeskew_Click(object sender, EventArgs e)
        {
            Deskew();
        }

        private void tsCustomRotation_Click(object sender, EventArgs e)
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

        #endregion

        #region Event Handlers - Reorder Menu

        private void tsInterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.Interleave(SelectedIndices), true, false);
            changeTracker.Made();
        }

        private void tsDeinterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.Deinterleave(SelectedIndices), true, false);
            changeTracker.Made();
        }

        private void tsAltInterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.AltInterleave(SelectedIndices), true, false);
            changeTracker.Made();
        }

        private void tsAltDeinterleave_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 3)
            {
                return;
            }
            UpdateThumbnails(imageList.AltDeinterleave(SelectedIndices), true, false);
            changeTracker.Made();
        }

        private void tsReverseAll_Click(object sender, EventArgs e)
        {
            if (imageList.Images.Count < 2)
            {
                return;
            }
            UpdateThumbnails(imageList.Reverse(), true, false);
            changeTracker.Made();
        }

        private void tsReverseSelected_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Count() < 2)
            {
                return;
            }
            UpdateThumbnails(imageList.Reverse(SelectedIndices), true, true);
            changeTracker.Made();
        }

        #endregion

        #region Context Menu

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ctxPaste.Enabled = CanPaste;
            if (!imageList.Images.Any() && !ctxPaste.Enabled)
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

        private void ctxPaste_Click(object sender, EventArgs e)
        {
            PasteImages();
        }

        private void ctxDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        #endregion

        #region Clipboard

        private async void CopyImages()
        {
            if (SelectedIndices.Any())
            {
                // TODO: Make copy an operation
                var ido = await GetDataObjectForImages(SelectedImages, true);
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
                return ido != null && ido.GetDataPresent(typeof(DirectImageTransfer).FullName);
            }
        }

        private async Task<IDataObject> GetDataObjectForImages(IEnumerable<ScannedImage> images, bool includeBitmap)
        {
            var imageList = images.ToList();
            IDataObject ido = new DataObject();
            if (imageList.Count == 0)
            {
                return ido;
            }
            if (includeBitmap)
            {
                using (var firstBitmap = await scannedImageRenderer.Render(imageList[0]))
                {
                    ido.SetData(DataFormats.Bitmap, true, new Bitmap(firstBitmap));
                    ido.SetData(DataFormats.Rtf, true, await RtfEncodeImages(firstBitmap, imageList));
                }
            }
            ido.SetData(typeof(DirectImageTransfer), new DirectImageTransfer(imageList));
            return ido;
        }

        private async Task<string> RtfEncodeImages(Bitmap firstBitmap, List<ScannedImage> images)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            if (!AppendRtfEncodedImage(firstBitmap, images[0].FileFormat, sb, false))
            {
                return null;
            }
            foreach (var img in images.Skip(1))
            {
                using (var bitmap = await scannedImageRenderer.Render(img))
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
                if (sb.Length + stream.Length * 2 > maxRtfSize)
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

        #endregion

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
            if (!imageList.Images.Any())
            {
                // Can't show visual feedback so don't do anything
                return;
            }
            if (thumbnailList1.ThumbnailSize.Height == thumbnailSize)
            {
                // Same size so no resizing needed
                return;
            }

            // Save the new size to config
            UserConfigManager.Config.ThumbnailSize = thumbnailSize;
            UserConfigManager.Save();
            // Adjust the visible thumbnail display with the new size
            lock (thumbnailList1)
            {
                thumbnailList1.ThumbnailSize = new Size(thumbnailSize, thumbnailSize);
                thumbnailList1.RegenerateThumbnailList(imageList.Images);
            }

            SetThumbnailSpacing(thumbnailSize);
            UpdateToolbar();

            // Render high-quality thumbnails at the new size in a background task
            // The existing (poorly scaled) thumbnails are used in the meantime
            renderThumbnailsWaitHandle.Set();
        }

        private void SetThumbnailSpacing(int thumbnailSize)
        {
            thumbnailList1.Padding = new Padding(0, 20, 0, 0);
            const int MIN_PADDING = 6;
            const int MAX_PADDING = 66;
            // Linearly scale the padding with the thumbnail size
            int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailRenderer.MIN_SIZE) / (ThumbnailRenderer.MAX_SIZE - ThumbnailRenderer.MIN_SIZE);
            int spacing = thumbnailSize + padding * 2;
            SetListSpacing(thumbnailList1, spacing, spacing);
        }

        private void SetListSpacing(ListView list, int hspacing, int vspacing)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            Win32.SendMessage(list.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)(int)(((ushort)hspacing) | (uint)(vspacing << 16)));
        }

        private void RenderThumbnails()
        {
            bool useWorker = PlatformCompat.Runtime.UseWorker;
            var worker = useWorker ? workerServiceFactory.Create() : null;
            var fallback = new ExpFallback(100, 60 * 1000);
            while (!closed)
            {
                try
                {
                    ScannedImage next;
                    while ((next = GetNextThumbnailToRender()) != null)
                    {
                        if (!ThumbnailStillNeedsRendering(next))
                        {
                            continue;
                        }
                        using (var snapshot = next.Preserve())
                        {
                            var thumb = worker != null
                                ? new Bitmap(new MemoryStream(worker.Service.RenderThumbnail(snapshot, thumbnailList1.ThumbnailSize.Height)))
                                : thumbnailRenderer.RenderThumbnail(snapshot, thumbnailList1.ThumbnailSize.Height).Result;

                            if (!ThumbnailStillNeedsRendering(next))
                            {
                                continue;
                            }

                            next.SetThumbnail(thumb, snapshot.TransformState);
                        }
                        fallback.Reset();
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error rendering thumbnails", e);
                    if (worker != null)
                    {
                        worker.Dispose();
                        worker = workerServiceFactory.Create();
                    }
                    Thread.Sleep(fallback.Value);
                    fallback.Increase();
                    continue;
                }
                renderThumbnailsWaitHandle.WaitOne();
            }
        }

        private bool ThumbnailStillNeedsRendering(ScannedImage next)
        {
            lock (next)
            {
                var thumb = next.GetThumbnail();
                return thumb == null || next.IsThumbnailDirty || thumb.Size != thumbnailList1.ThumbnailSize;
            }
        }

        private ScannedImage GetNextThumbnailToRender()
        {
            List<ScannedImage> listCopy;
            lock (imageList)
            {
                listCopy = imageList.Images.ToList();
            }
            // Look for images without thumbnails
            foreach (var img in listCopy)
            {
                if (img.GetThumbnail() == null)
                {
                    return img;
                }
            }
            // Look for images with dirty thumbnails
            foreach (var img in listCopy)
            {
                if (img.IsThumbnailDirty)
                {
                    return img;
                }
            }
            // Look for images with mis-sized thumbnails
            foreach (var img in listCopy)
            {
                if (img.GetThumbnail()?.Size != thumbnailList1.ThumbnailSize)
                {
                    return img;
                }
            }
            // Nothing to render
            return null;
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            StepThumbnailSize(-1);
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            StepThumbnailSize(1);
        }

        #endregion

        #region Drag/Drop

        private async void thumbnailList1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Provide drag data
            if (SelectedIndices.Any())
            {
                var ido = await GetDataObjectForImages(SelectedImages, false);
                DoDragDrop(ido, DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void thumbnailList1_DragEnter(object sender, DragEventArgs e)
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

        private void thumbnailList1_DragDrop(object sender, DragEventArgs e)
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
            thumbnailList1.InsertionMark.Index = -1;
        }

        private void thumbnailList1_DragLeave(object sender, EventArgs e)
        {
            thumbnailList1.InsertionMark.Index = -1;
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
                changeTracker.Made();
            }
        }

        private void thumbnailList1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                var index = GetDragIndex(e);
                if (index == imageList.Images.Count)
                {
                    thumbnailList1.InsertionMark.Index = index - 1;
                    thumbnailList1.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    thumbnailList1.InsertionMark.Index = index;
                    thumbnailList1.InsertionMark.AppearsAfterItem = false;
                }
            }
        }

        private int GetDragIndex(DragEventArgs e)
        {
            Point cp = thumbnailList1.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = thumbnailList1.GetItemAt(cp.X, cp.Y);
            if (dragToItem == null)
            {
                var items = thumbnailList1.Items.Cast<ListViewItem>().ToList();
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
                dragToItem = row.FirstOrDefault(x => x.Bounds.Right >= cp.X) ?? row.LastOrDefault();
            }
            if (dragToItem == null)
            {
                return -1;
            }
            int dragToIndex = dragToItem.ImageIndex;
            if (cp.X > (dragToItem.Bounds.X + dragToItem.Bounds.Width / 2))
            {
                dragToIndex++;
            }
            return dragToIndex;
        }

        #endregion
    }
}
