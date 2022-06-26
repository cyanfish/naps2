#region Usings

using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport;
using NAPS2.Ocr;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.ImportExport.Images;
using NAPS2.Platform.Windows;
using NAPS2.Remoting;
using NAPS2.Wia;
using NAPS2.Update;

#endregion

namespace NAPS2.WinForms
{
    public partial class FDesktop : FormBase
    {
        #region Dependencies

        private static readonly MethodInfo ToolStripPanelSetStyle =
            typeof(ToolStripPanel).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ToolbarFormatter _toolbarFormatter;
        private readonly TesseractLanguageManager _tesseractLanguageManager;
        private readonly IScanPerformer _scanPerformer;
        private readonly IScannedImagePrinter _scannedImagePrinter;
        private readonly StillImage _stillImage;
        private readonly IOperationFactory _operationFactory;
        private readonly KeyboardShortcutManager _ksm;
        private readonly ThumbnailRenderer _thumbnailRenderer;
        private readonly WinFormsExportHelper _exportHelper;
        private readonly ImageClipboard _imageClipboard;
        private readonly NotificationManager _notify;
        private readonly CultureHelper _cultureHelper;
        private readonly OperationProgress _operationProgress;
        private readonly UpdateChecker _updateChecker;
        private readonly IProfileManager _profileManager;
        private readonly UiImageList _imageList;
        private readonly ImageTransfer _imageTransfer;
        private readonly RecoveryStorageManager _recoveryStorageManager;
        private readonly ScanningContext _scanningContext;
        private readonly ThumbnailRenderQueue _thumbnailRenderQueue;

        #endregion

        #region State Fields

        private readonly UserActions _userActions;
        private bool _closed = false;
        private LayoutManager _layoutManager;
        private bool _disableSelectedIndexChangedEvent;

        public bool SkipRecoveryCleanup { get; set; }

        #endregion

        #region Initialization and Culture

        public FDesktop(ToolbarFormatter toolbarFormatter,
            TesseractLanguageManager tesseractLanguageManager,
            IScanPerformer scanPerformer, IScannedImagePrinter scannedImagePrinter, StillImage stillImage,
            IOperationFactory operationFactory,
            KeyboardShortcutManager ksm, ThumbnailRenderer thumbnailRenderer, WinFormsExportHelper exportHelper,
            ImageClipboard imageClipboard,
            NotificationManager notify, CultureHelper cultureHelper,
            OperationProgress operationProgress,
            UpdateChecker updateChecker, IProfileManager profileManager, UiImageList imageList,
            ImageTransfer imageTransfer,
            RecoveryStorageManager recoveryStorageManager, ScanningContext scanningContext,
            ThumbnailRenderQueue thumbnailRenderQueue)
        {
            _toolbarFormatter = toolbarFormatter;
            _tesseractLanguageManager = tesseractLanguageManager;
            _scanPerformer = scanPerformer;
            _scannedImagePrinter = scannedImagePrinter;
            _stillImage = stillImage;
            _operationFactory = operationFactory;
            _ksm = ksm;
            _thumbnailRenderer = thumbnailRenderer;
            _exportHelper = exportHelper;
            _imageClipboard = imageClipboard;
            _notify = notify;
            _cultureHelper = cultureHelper;
            _operationProgress = operationProgress;
            _updateChecker = updateChecker;
            _profileManager = profileManager;
            _imageList = imageList;
            _imageTransfer = imageTransfer;
            _recoveryStorageManager = recoveryStorageManager;
            _scanningContext = scanningContext;
            _thumbnailRenderQueue = thumbnailRenderQueue;
            _userActions = new UserActions(_scanningContext.ImageContext, imageList);
            InitializeComponent();

            notify.ParentForm = this;
            Shown += FDesktop_Shown;
            FormClosing += FDesktop_FormClosing;
            Closed += FDesktop_Closed;
            thumbnailList1.Initialize(imageList);
            imageList.ImagesUpdated += (_, _) => UpdateToolbar();
        }

        protected override void OnLoad(object sender, EventArgs args) => PostInitializeComponent();

        protected override void AfterLoad(object sender, EventArgs args) => AfterLayout();

        /// <summary>
        /// Runs when the form is first loaded and every time the language is changed.
        /// </summary>
        private void PostInitializeComponent()
        {
            foreach (var panel in toolStripContainer1.Controls.OfType<ToolStripPanel>())
            {
                ToolStripPanelSetStyle.Invoke(panel, new object[] { ControlStyles.Selectable, true });
            }
            _imageList.ThumbnailRenderer = _thumbnailRenderer;
            int thumbnailSize = Config.Get(c => c.ThumbnailSize);
            thumbnailList1.ThumbnailSize = thumbnailSize;
            SetThumbnailSpacing(thumbnailSize);

            var hiddenButtons = Config.Get(c => c.HiddenButtons);
            var buttonMap = new List<(ToolbarButtons, ToolStripItem)>
            {
                (ToolbarButtons.Scan, tsScan),
                (ToolbarButtons.Profiles, tsProfiles),
                (ToolbarButtons.Ocr, tsOcr),
                (ToolbarButtons.Import, tsImport),
                (ToolbarButtons.SavePdf, tsdSavePDF),
                (ToolbarButtons.SaveImages, tsdSaveImages),
                (ToolbarButtons.EmailPdf, tsdEmailPDF),
                (ToolbarButtons.Print, tsPrint),
                (ToolbarButtons.Image, tsdImage),
                (ToolbarButtons.Rotate, tsdRotate),
                (ToolbarButtons.Move, tsMove),
                (ToolbarButtons.Reorder, tsdReorder),
                (ToolbarButtons.Delete, tsDelete),
                (ToolbarButtons.Clear, tsClear),
                (ToolbarButtons.Language, toolStripDropDownButton1),
                (ToolbarButtons.Settings, tsSettingsAbout),
                (ToolbarButtons.About, tsSettingsAbout),
            };
            foreach (var (flag, button) in buttonMap)
            {
                if (hiddenButtons.HasFlag(flag))
                {
                    tStrip.Items.Remove(button);
                }
            }

            LoadToolStripLocation();
            InitLanguageDropdown();
            AssignKeyboardShortcuts();
            UpdateScanButton();

            _layoutManager?.Deactivate();
            btnZoomIn.Location = new Point(btnZoomIn.Location.X, thumbnailList1.Height - 33);
            btnZoomOut.Location = new Point(btnZoomOut.Location.X, thumbnailList1.Height - 33);
            btnZoomMouseCatcher.Location = new Point(btnZoomMouseCatcher.Location.X, thumbnailList1.Height - 33);
            _layoutManager = new LayoutManager(this)
                .Bind(btnZoomIn, btnZoomOut, btnZoomMouseCatcher)
                .BottomTo(() => thumbnailList1.Height)
                .Activate();

            thumbnailList1.MouseWheel += thumbnailList1_MouseWheel;
            thumbnailList1.SizeChanged += (sender, args) => _layoutManager.UpdateLayout();
        }

        private void AfterLayout()
        {
            _toolbarFormatter.RelayoutToolbar(tStrip);
        }

        private void InitLanguageDropdown()
        {
            foreach (var (langCode, langName) in _cultureHelper.GetAvailableCultures())
            {
                var button = new ToolStripMenuItem(langName, null, (_, _) => SetCulture(langCode));
                toolStripDropDownButton1.DropDownItems.Add(button);
            }
        }

        private void SetCulture(string cultureId)
        {
            SaveToolStripLocation();
            Config.User.Set(c => c.Culture, cultureId);
            _cultureHelper.SetCulturesFromConfig();

            // Update localized values
            // Since all forms are opened modally and this is the root form, it should be the only one that needs to be updated live
            SaveFormState = false;
            Controls.Clear();
            UpdateRTL();
            InitializeComponent();
            PostInitializeComponent();
            AfterLayout();
            _notify.Rebuild();
            Focus();
            WindowState = FormWindowState.Normal;
            DoRestoreFormState();
            SaveFormState = true;
        }

        private async void FDesktop_Shown(object sender, EventArgs e)
        {
            // TODO: Start the Eto application in the entry point once all forms (or at least FDesktop?) are migrated
            new Eto.Forms.Application(Eto.Platforms.WinForms).Attach();

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
            if (!string.IsNullOrWhiteSpace(Config.Get(c => c.StartupMessageText)))
            {
                MessageBox.Show(Config.Get(c => c.StartupMessageText), Config.Get(c => c.StartupMessageTitle),
                    MessageBoxButtons.OK,
                    Config.Get(c => c.StartupMessageIcon));
            }

            // Allow scanned images to be recovered in case of an unexpected close
            var op = _operationFactory.Create<RecoveryOperation>();
            if (op.Start(ReceiveScannedImage(),
                    new RecoveryParams { ThumbnailSize = Config.Get(c => c.ThumbnailSize) }))
            {
                _operationProgress.ShowProgress(op);
            }

            _thumbnailRenderQueue.SetThumbnailSize(Config.Get(c => c.ThumbnailSize));
            _thumbnailRenderQueue.StartRendering(_imageList);

            // If NAPS2 was started by the scanner button, do the appropriate actions automatically
            await RunStillImageEvents();

            // Show a donation prompt after a month of use
            if (!Config.Get(c => c.HasBeenRun))
            {
                var transact = Config.User.BeginTransaction();
                transact.Set(c => c.HasBeenRun, true);
                transact.Set(c => c.FirstRunDate, DateTime.Now);
                transact.Commit();
            }
#if !INSTALLER_MSI
            else if (!Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.Donate) &&
                     !Config.Get(c => c.HasBeenPromptedForDonation) &&
                     DateTime.Now - Config.Get(c => c.FirstRunDate) > TimeSpan.FromDays(30))
            {
                var transact = Config.User.BeginTransaction();
                transact.Set(c => c.HasBeenPromptedForDonation, true);
                transact.Set(c => c.LastDonatePromptDate, DateTime.Now);
                transact.Commit();
                _notify.DonatePrompt();
            }

            if (Config.Get(c => c.CheckForUpdates) &&
                (!Config.Get(c => c.HasCheckedForUpdates) ||
                 Config.Get(c => c.LastUpdateCheckDate) < DateTime.Now - _updateChecker.CheckInterval))
            {
                _updateChecker.CheckForUpdates().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.ErrorException("Error checking for updates", task.Exception);
                    }
                    else
                    {
                        var transact = Config.User.BeginTransaction();
                        transact.Set(c => c.HasCheckedForUpdates, true);
                        transact.Set(c => c.LastUpdateCheckDate, DateTime.Now);
                        transact.Commit();
                    }
                    var update = task.Result;
                    if (update != null)
                    {
                        SafeInvoke(() => _notify.UpdateAvailable(_updateChecker, update));
                    }
                }).AssertNoAwait();
            }
#endif
        }

        #endregion

        #region Cleanup

        private void FDesktop_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_closed) return;

            if (_operationProgress.ActiveOperations.Any())
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (_operationProgress.ActiveOperations.Any(x => !x.SkipExitPrompt))
                    {
                        var result = MessageBox.Show(MiscResources.ExitWithActiveOperations,
                            MiscResources.ActiveOperations,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                        {
                            e.Cancel = true;
                        }
                    }
                }
                else
                {
                    SkipRecoveryCleanup = true;
                }
            }
            else if (_imageList.Images.Any() && _imageList.SavedState != _imageList.CurrentState)
            {
                if (e.CloseReason == CloseReason.UserClosing && !SkipRecoveryCleanup)
                {
                    var result = MessageBox.Show(MiscResources.ExitWithUnsavedChanges, MiscResources.UnsavedChanges,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        _imageList.SavedState = _imageList.CurrentState;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    SkipRecoveryCleanup = true;
                }
            }

            if (!e.Cancel && _operationProgress.ActiveOperations.Any())
            {
                _operationProgress.ActiveOperations.ForEach(op => op.Cancel());
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
                Task.Run(() =>
                {
                    var timeoutCts = new CancellationTokenSource();
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));
                    try
                    {
                        _operationProgress.ActiveOperations.ForEach(op => op.Wait(timeoutCts.Token));
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    _closed = true;
                    SafeInvoke(Close);
                });
            }
        }

        private void FDesktop_Closed(object sender, EventArgs e)
        {
            SaveToolStripLocation();
            Pipes.KillServer();
            if (!SkipRecoveryCleanup)
            {
                try
                {
                    // TODO: Figure out and fix undisposed processed images
                    _scanningContext.Dispose();
                    _recoveryStorageManager.Dispose();
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Recovery cleanup failed", ex);
                }
            }
            _closed = true;
            _thumbnailRenderQueue.Dispose();
        }

        #endregion

        #region Scanning and Still Image

        private async Task RunStillImageEvents()
        {
            if (_stillImage.ShouldScan)
            {
                await ScanWithDevice(_stillImage.DeviceID!);
            }
        }

        private ScanParams DefaultScanParams() =>
            new ScanParams
            {
                NoAutoSave = Config.Get(c => c.DisableAutoSave),
                DoOcr = Config.Get(c => c.EnableOcr) && Config.Get(c => c.OcrAfterScanning),
                ThumbnailSize = Config.Get(c => c.ThumbnailSize)
            };

        private async Task ScanWithDevice(string deviceID)
        {
            Activate();
            ScanProfile profile;
            if (_profileManager.DefaultProfile?.Device?.ID == deviceID)
            {
                // Try to use the default profile if it has the right device
                profile = _profileManager.DefaultProfile;
            }
            else
            {
                // Otherwise just pick any old profile with the right device
                // Not sure if this is the best way to do it, but it's hard to prioritize profiles
                profile = _profileManager.Profiles.FirstOrDefault(x => x.Device != null && x.Device.ID == deviceID);
            }
            if (profile == null)
            {
                if (Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked))
                {
                    return;
                }

                // No profile for the device we're scanning with, so prompt to create one
                var editSettingsForm = FormFactory.Create<FEditProfile>();
                editSettingsForm.ScanProfile = Config.Get(c => c.DefaultProfileSettings);
                try
                {
                    // Populate the device field automatically (because we can do that!)
                    using var deviceManager = new WiaDeviceManager();
                    using var device = deviceManager.FindDevice(deviceID);
                    editSettingsForm.CurrentDevice = new ScanDevice(deviceID, device.Name());
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
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile),
                    ListSelection.Empty<ScanProfile>());
                _profileManager.DefaultProfile = profile;

                UpdateScanButton();
            }
            if (profile != null)
            {
                // We got a profile, yay, so we can actually do the scan now
                var source = await _scanPerformer.PerformScan(profile, DefaultScanParams(), Handle);
                await source.ForEach(ReceiveScannedImage());
                Activate();
            }
        }

        private async Task ScanDefault()
        {
            if (_profileManager.DefaultProfile != null)
            {
                var source =
                    await _scanPerformer.PerformScan(_profileManager.DefaultProfile, DefaultScanParams(), Handle);
                await source.ForEach(ReceiveScannedImage());
                Activate();
            }
            else if (_profileManager.Profiles.Count == 0)
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
            editSettingsForm.ScanProfile = Config.Get(c => c.DefaultProfileSettings);
            editSettingsForm.ShowDialog();
            if (!editSettingsForm.Result)
            {
                return;
            }
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile),
                ListSelection.Empty<ScanProfile>());
            _profileManager.DefaultProfile = editSettingsForm.ScanProfile;

            UpdateScanButton();

            var source = await _scanPerformer.PerformScan(editSettingsForm.ScanProfile, DefaultScanParams(), Handle);
            await source.ForEach(ReceiveScannedImage());
            Activate();
        }

        #endregion

        #region Images and Thumbnails

        /// <summary>
        /// Constructs a receiver for scanned images.
        /// This keeps images from the same source together, even if multiple sources are providing images at the same time.
        /// </summary>
        /// <returns></returns>
        public Action<ProcessedImage> ReceiveScannedImage()
        {
            UiImage? last = null;
            return scannedImage =>
            {
                SafeInvoke(() =>
                {
                    lock (_imageList)
                    {
                        var uiImage = new UiImage(scannedImage);
                        _imageList.Mutate(new ImageListMutation.InsertAfter(uiImage, last));
                        last = uiImage;
                    }
                });
            };
        }

        #endregion

        #region Toolbar

        private void UpdateToolbar()
        {
            // "All" dropdown items
            tsSavePDFAll.Text = tsSaveImagesAll.Text = tsEmailPDFAll.Text = tsReverseAll.Text =
                string.Format(MiscResources.AllCount, _imageList.Images.Count);
            tsSavePDFAll.Enabled = tsSaveImagesAll.Enabled = tsEmailPDFAll.Enabled = tsReverseAll.Enabled =
                _imageList.Images.Any();

            // "Selected" dropdown items
            tsSavePDFSelected.Text = tsSaveImagesSelected.Text = tsEmailPDFSelected.Text = tsReverseSelected.Text =
                string.Format(MiscResources.SelectedCount, _imageList.Selection.Count);
            tsSavePDFSelected.Enabled = tsSaveImagesSelected.Enabled = tsEmailPDFSelected.Enabled =
                tsReverseSelected.Enabled =
                    _imageList.Selection.Any();

            // Top-level toolbar actions
            tsdImage.Enabled = tsdRotate.Enabled = tsMove.Enabled = tsDelete.Enabled = _imageList.Selection.Any();
            tsdReorder.Enabled = tsdSavePDF.Enabled = tsdSaveImages.Enabled =
                tsdEmailPDF.Enabled = tsPrint.Enabled = tsClear.Enabled = _imageList.Images.Any();

            // Context-menu actions
            ctxView.Visible = ctxCopy.Visible = ctxDelete.Visible =
                ctxSeparator1.Visible = ctxSeparator2.Visible = _imageList.Selection.Any();
            ctxSelectAll.Enabled = _imageList.Images.Any();

            // Other
            btnZoomIn.Enabled = _imageList.Images.Any() && Config.Get(c => c.ThumbnailSize) < ThumbnailSizes.MAX_SIZE;
            btnZoomOut.Enabled = _imageList.Images.Any() && Config.Get(c => c.ThumbnailSize) > ThumbnailSizes.MIN_SIZE;
            tsNewProfile.Enabled =
                !(Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked));

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
            var defaultProfile = _profileManager.DefaultProfile;
            int i = 1;
            foreach (var profile in _profileManager.Profiles)
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
                    _profileManager.DefaultProfile = profile;

                    UpdateScanButton();

                    var source = await _scanPerformer.PerformScan(profile, DefaultScanParams(), Handle);
                    await source.ForEach(ReceiveScannedImage());
                    Activate();
                };
                tsScan.DropDownItems.Insert(tsScan.DropDownItems.Count - staticButtonCount, item);

                i++;
            }

            if (_profileManager.Profiles.Any())
            {
                tsScan.DropDownItems.Insert(tsScan.DropDownItems.Count - staticButtonCount, new ToolStripSeparator());
            }
        }

        private void SaveToolStripLocation()
        {
            Config.User.Set(c => c.DesktopToolStripDock, tStrip.Parent.Dock);
        }

        private void LoadToolStripLocation()
        {
            var dock = Config.Get(c => c.DesktopToolStripDock);
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
            if (_imageList.Images.Count > 0)
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmClearItems, _imageList.Images.Count),
                        MiscResources.Clear, MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question) == DialogResult.OK)
                {
                    _userActions.DeleteAll();
                }
            }
        }

        private void Delete()
        {
            if (_imageList.Selection.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, _imageList.Selection.Count),
                        MiscResources.Delete, MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question) == DialogResult.OK)
                {
                    _userActions.DeleteSelected();
                }
            }
        }

        private void PreviewImage()
        {
            if (_imageList.Selection.Any())
            {
                using var viewer = FormFactory.Create<FViewer>();
                viewer.ImageList = _imageList;
                // TODO: Fix this 
                // viewer.ImageIndex = SelectedIndices.First();
                // viewer.DeleteCallback = UpdateThumbnails;
                viewer.SelectCallback = i =>
                {
                    if (_imageList.Selection.Count <= 1)
                    {
                        // TODO: Fix this
                        // SelectedIndices = new[] { i };
                        thumbnailList1.Items[i].EnsureVisible();
                    }
                };
                viewer.ShowDialog();
            }
        }

        private void ShowProfilesForm()
        {
            var form = FormFactory.Create<ProfilesForm>();
            form.ImageCallback = ReceiveScannedImage();
            form.ShowModal();
            UpdateScanButton();
        }

        private void ResetImage()
        {
            if (_imageList.Selection.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmResetImages, _imageList.Selection.Count),
                        MiscResources.ResetImage,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    _userActions.ResetTransforms();
                }
            }
        }

        private void OpenAbout()
        {
            _toolbarFormatter.RelayoutToolbar(tStrip);
            FormFactory.Create<AboutForm>().ShowModal();
        }

        private void OpenSettings()
        {
            // FormFactory.Create<FSettings>().ShowDialog();
        }

        #endregion

        #region Actions - Save/Email/Import

        private async void SavePDF(List<UiImage> images)
        {
            using var imagesToSave = images.Select(x => x.GetClonedImage()).ToDisposableList();
            if (await _exportHelper.SavePDF(imagesToSave.InnerList, _notify))
            {
                if (Config.Get(c => c.DeleteAfterSaving))
                {
                    SafeInvoke(() =>
                    {
                        _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
                    });
                }
            }
        }

        private async void SaveImages(List<UiImage> images)
        {
            using var imagesToSave = images.Select(x => x.GetClonedImage()).ToDisposableList();
            if (await _exportHelper.SaveImages(imagesToSave.InnerList, _notify))
            {
                if (Config.Get(c => c.DeleteAfterSaving))
                {
                    _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
                }
            }
        }

        private async void EmailPDF(List<UiImage> images)
        {
            using var imagesToEmail = images.Select(x => x.GetClonedImage()).ToDisposableList();
            await _exportHelper.EmailPDF(imagesToEmail.InnerList);
        }

        private void Import()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true,
                Filter = MiscResources.FileTypeAllFiles + @"|*.*|" +
                         MiscResources.FileTypePdf + @"|*.pdf|" +
                         MiscResources.FileTypeImageFiles +
                         @"|*.bmp;*.emf;*.exif;*.gif;*.jpg;*.jpeg;*.png;*.tiff;*.tif|" +
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
            var op = _operationFactory.Create<ImportOperation>();
            if (op.Start(OrderFiles(files), ReceiveScannedImage(),
                    new ImportParams { ThumbnailSize = Config.Get(c => c.ThumbnailSize) }))
            {
                _operationProgress.ShowProgress(op);
            }
        }

        private List<string> OrderFiles(IEnumerable<string> files)
        {
            // Custom ordering to account for numbers so that e.g. "10" comes after "2"
            var filesList = files.ToList();
            filesList.Sort(new NaturalStringComparer());
            return filesList;
        }

        private void ImportDirect(ImageTransferData data, bool copy)
        {
            var op = _operationFactory.Create<DirectImportOperation>();
            if (op.Start(data, copy, ReceiveScannedImage(),
                    new DirectImportParams { ThumbnailSize = Config.Get(c => c.ThumbnailSize) }))
            {
                _operationProgress.ShowProgress(op);
            }
        }

        #endregion

        #region Keyboard Shortcuts

        private void AssignKeyboardShortcuts()
        {
            // Defaults

            _ksm.Assign("Ctrl+Enter", tsScan);
            _ksm.Assign("Ctrl+B", tsBatchScan);
            _ksm.Assign("Ctrl+O", tsImport);
            _ksm.Assign("Ctrl+S", tsdSavePDF);
            _ksm.Assign("Ctrl+P", tsPrint);
            _ksm.Assign("Ctrl+Up", _userActions.MoveUp);
            _ksm.Assign("Ctrl+Left", _userActions.MoveUp);
            _ksm.Assign("Ctrl+Down", _userActions.MoveDown);
            _ksm.Assign("Ctrl+Right", _userActions.MoveDown);
            _ksm.Assign("Ctrl+Shift+Del", tsClear);
            _ksm.Assign("F1", OpenAbout);
            _ksm.Assign("Ctrl+OemMinus", btnZoomOut);
            _ksm.Assign("Ctrl+Oemplus", btnZoomIn);
            _ksm.Assign("Del", ctxDelete);
            _ksm.Assign("Ctrl+A", ctxSelectAll);
            _ksm.Assign("Ctrl+C", ctxCopy);
            _ksm.Assign("Ctrl+V", ctxPaste);

            // Configured

            // TODO: Granular
            var ks = Config.Get(c => c.KeyboardShortcuts);

            _ksm.Assign(ks.About, OpenAbout);
            _ksm.Assign(ks.BatchScan, tsBatchScan);
            _ksm.Assign(ks.Clear, tsClear);
            _ksm.Assign(ks.Delete, tsDelete);
            _ksm.Assign(ks.EmailPDF, tsdEmailPDF);
            _ksm.Assign(ks.EmailPDFAll, tsEmailPDFAll);
            _ksm.Assign(ks.EmailPDFSelected, tsEmailPDFSelected);
            _ksm.Assign(ks.ImageBlackWhite, tsBlackWhite);
            _ksm.Assign(ks.ImageBrightness, tsBrightnessContrast);
            _ksm.Assign(ks.ImageContrast, tsBrightnessContrast);
            _ksm.Assign(ks.ImageCrop, tsCrop);
            _ksm.Assign(ks.ImageHue, tsHueSaturation);
            _ksm.Assign(ks.ImageSaturation, tsHueSaturation);
            _ksm.Assign(ks.ImageSharpen, tsSharpen);
            _ksm.Assign(ks.ImageReset, tsReset);
            _ksm.Assign(ks.ImageView, tsView);
            _ksm.Assign(ks.Import, tsImport);
            _ksm.Assign(ks.MoveDown, _userActions.MoveDown);
            _ksm.Assign(ks.MoveUp, _userActions.MoveUp);
            _ksm.Assign(ks.NewProfile, tsNewProfile);
            _ksm.Assign(ks.Ocr, tsOcr);
            _ksm.Assign(ks.Print, tsPrint);
            _ksm.Assign(ks.Profiles, ShowProfilesForm);

            _ksm.Assign(ks.ReorderAltDeinterleave, tsAltDeinterleave);
            _ksm.Assign(ks.ReorderAltInterleave, tsAltInterleave);
            _ksm.Assign(ks.ReorderDeinterleave, tsDeinterleave);
            _ksm.Assign(ks.ReorderInterleave, tsInterleave);
            _ksm.Assign(ks.ReorderReverseAll, tsReverseAll);
            _ksm.Assign(ks.ReorderReverseSelected, tsReverseSelected);
            _ksm.Assign(ks.RotateCustom, tsCustomRotation);
            _ksm.Assign(ks.RotateFlip, tsFlip);
            _ksm.Assign(ks.RotateLeft, tsRotateLeft);
            _ksm.Assign(ks.RotateRight, tsRotateRight);
            _ksm.Assign(ks.SaveImages, tsdSaveImages);
            _ksm.Assign(ks.SaveImagesAll, tsSaveImagesAll);
            _ksm.Assign(ks.SaveImagesSelected, tsSaveImagesSelected);
            _ksm.Assign(ks.SavePDF, tsdSavePDF);
            _ksm.Assign(ks.SavePDFAll, tsSavePDFAll);
            _ksm.Assign(ks.SavePDFSelected, tsSavePDFSelected);
            _ksm.Assign(ks.ScanDefault, tsScan);

            _ksm.Assign(ks.ZoomIn, btnZoomIn);
            _ksm.Assign(ks.ZoomOut, btnZoomOut);
        }

        private void AssignProfileShortcut(int i, ToolStripMenuItem item)
        {
            var sh = GetProfileShortcut(i);
            if (string.IsNullOrWhiteSpace(sh) && i <= 11)
            {
                sh = "F" + (i + 1);
            }
            _ksm.Assign(sh, item);
        }

        private string? GetProfileShortcut(int i)
        {
            // TODO: Granular
            var ks = Config.Get(c => c.KeyboardShortcuts);
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
            e.Handled = _ksm.Perform(e.KeyData);
        }

        private void thumbnailList1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                StepThumbnailSize(e.Delta / (double) SystemInformation.MouseWheelScrollDelta);
            }
        }

        #endregion

        #region Event Handlers - Misc

        private void thumbnailList1_ItemActivate(object sender, EventArgs e) => PreviewImage();

        private void thumbnailList1_MouseMove(object sender, MouseEventArgs e) =>
            Cursor = thumbnailList1.GetItemAt(e.X, e.Y) == null ? Cursors.Default : Cursors.Hand;

        private void thumbnailList1_MouseLeave(object sender, EventArgs e) => Cursor = Cursors.Default;

        private void tStrip_ParentChanged(object sender, EventArgs e) => _toolbarFormatter.RelayoutToolbar(tStrip);

        #endregion

        #region Event Handlers - Toolbar

        private async void tsScan_ButtonClick(object sender, EventArgs e) => await ScanDefault();

        private async void tsNewProfile_Click(object sender, EventArgs e) => await ScanWithNewProfile();

        private void tsBatchScan_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBatchScan>();
            form.ImageCallback = ReceiveScannedImage();
            form.ShowDialog();
            UpdateScanButton();
        }

        private void tsProfiles_Click(object sender, EventArgs e) => ShowProfilesForm();

        private void tsOcr_Click(object sender, EventArgs e)
        {
            if (_tesseractLanguageManager.InstalledLanguages.Any())
            {
                FormFactory.Create<FOcrSetup>().ShowDialog();
            }
            else
            {
                FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                if (_tesseractLanguageManager.InstalledLanguages.Any())
                {
                    FormFactory.Create<FOcrSetup>().ShowDialog();
                }
            }
        }

        private void tsImport_Click(object sender, EventArgs e) => Import();

        private void tsdSavePDF_ButtonClick(object sender, EventArgs e)
        {
            var action = Config.Get(c => c.SaveButtonDefaultAction);

            if (action == SaveButtonDefaultAction.AlwaysPrompt
                || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
            {
                tsdSavePDF.ShowDropDown();
            }
            else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
            {
                SavePDF(_imageList.Selection.ToList());
            }
            else
            {
                SavePDF(_imageList.Images);
            }
        }

        private void tsdSaveImages_ButtonClick(object sender, EventArgs e)
        {
            var action = Config.Get(c => c.SaveButtonDefaultAction);

            if (action == SaveButtonDefaultAction.AlwaysPrompt
                || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
            {
                tsdSaveImages.ShowDropDown();
            }
            else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
            {
                SaveImages(_imageList.Selection.ToList());
            }
            else
            {
                SaveImages(_imageList.Images);
            }
        }

        private void tsdEmailPDF_ButtonClick(object sender, EventArgs e)
        {
            var action = Config.Get(c => c.SaveButtonDefaultAction);

            if (action == SaveButtonDefaultAction.AlwaysPrompt
                || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
            {
                tsdEmailPDF.ShowDropDown();
            }
            else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
            {
                EmailPDF(_imageList.Selection.ToList());
            }
            else
            {
                EmailPDF(_imageList.Images);
            }
        }

        private async void tsPrint_Click(object sender, EventArgs e)
        {
            var state = _imageList.CurrentState;
            using var allImages = _imageList.Images.Select(x => x.GetClonedImage()).ToDisposableList();
            using var selectedImages = _imageList.Selection.Select(x => x.GetClonedImage()).ToDisposableList();
            if (await _scannedImagePrinter.PromptToPrint(allImages.InnerList, selectedImages.InnerList))
            {
                _imageList.SavedState = state;
            }
        }

        private void tsMove_FirstClick(object sender, EventArgs e) => _userActions.MoveUp();

        private void tsMove_SecondClick(object sender, EventArgs e) => _userActions.MoveDown();

        private void tsDelete_Click(object sender, EventArgs e) => Delete();

        private void tsClear_Click(object sender, EventArgs e) => Clear();

        private void tsAbout_Click(object sender, EventArgs e) => OpenAbout();

        private void tsSettings_Click(object sender, EventArgs e) => OpenSettings();

        #endregion

        #region Event Handlers - Save/Email Menus

        private void tsSavePDFAll_Click(object sender, EventArgs e) => SavePDF(_imageList.Images);
        private void tsSavePDFSelected_Click(object sender, EventArgs e) => SavePDF(_imageList.Selection.ToList());
        private void tsPDFSettings_Click(object sender, EventArgs e) => FormFactory.Create<FPdfSettings>().ShowDialog();

        private void tsSaveImagesAll_Click(object sender, EventArgs e) => SaveImages(_imageList.Images);

        private void tsSaveImagesSelected_Click(object sender, EventArgs e) =>
            SaveImages(_imageList.Selection.ToList());

        private void tsImageSettings_Click(object sender, EventArgs e) =>
            FormFactory.Create<FImageSettings>().ShowDialog();

        private void tsEmailPDFAll_Click(object sender, EventArgs e) => EmailPDF(_imageList.Images);
        private void tsEmailPDFSelected_Click(object sender, EventArgs e) => EmailPDF(_imageList.Selection.ToList());

        private void tsPdfSettings2_Click(object sender, EventArgs e) =>
            FormFactory.Create<FPdfSettings>().ShowDialog();

        private void tsEmailSettings_Click(object sender, EventArgs e) =>
            FormFactory.Create<FEmailSettings>().ShowDialog();

        #endregion

        #region Event Handlers - Image Menu

        private void tsView_Click(object sender, EventArgs e) => PreviewImage();

        private void ShowImageForm<T>() where T : ImageForm
        {
            var selection = _imageList.Selection.ToList();
            if (selection.Any())
            {
                var form = FormFactory.Create<T>();
                form.Image = selection.First();
                form.SelectedImages = selection.ToList();
                form.ShowDialog();
            }
        }

        private void tsCrop_Click(object sender, EventArgs e) => ShowImageForm<FCrop>();
        private void tsBrightnessContrast_Click(object sender, EventArgs e) => ShowImageForm<FBrightnessContrast>();
        private void tsHueSaturation_Click(object sender, EventArgs e) => ShowImageForm<FHueSaturation>();
        private void tsBlackWhite_Click(object sender, EventArgs e) => ShowImageForm<FBlackWhite>();
        private void tsSharpen_Click(object sender, EventArgs e) => ShowImageForm<FSharpen>();
        private void tsReset_Click(object sender, EventArgs e) => ResetImage();

        #endregion

        #region Event Handlers - Rotate Menu

        private async void tsRotateLeft_Click(object sender, EventArgs e) => await _userActions.RotateLeft();
        private async void tsRotateRight_Click(object sender, EventArgs e) => await _userActions.RotateRight();
        private async void tsFlip_Click(object sender, EventArgs e) => await _userActions.Flip();
        private void tsDeskew_Click(object sender, EventArgs e) => _userActions.Deskew();
        private void tsCustomRotation_Click(object sender, EventArgs e) => ShowImageForm<FRotate>();

        #endregion

        #region Event Handlers - Reorder Menu

        private void tsInterleave_Click(object sender, EventArgs e) => _userActions.Interleave();
        private void tsDeinterleave_Click(object sender, EventArgs e) => _userActions.Deinterleave();
        private void tsAltInterleave_Click(object sender, EventArgs e) => _userActions.AltInterleave();
        private void tsAltDeinterleave_Click(object sender, EventArgs e) => _userActions.AltDeinterleave();
        private void tsReverseAll_Click(object sender, EventArgs e) => _userActions.ReverseAll();
        private void tsReverseSelected_Click(object sender, EventArgs e) => _userActions.ReverseSelected();

        #endregion

        #region Context Menu

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ctxPaste.Enabled = _imageTransfer.IsInClipboard();
            if (!_imageList.Images.Any() && !ctxPaste.Enabled)
            {
                e.Cancel = true;
            }
        }

        private void ctxSelectAll_Click(object sender, EventArgs e) => _userActions.SelectAll();
        private void ctxView_Click(object sender, EventArgs e) => PreviewImage();
        private void ctxDelete_Click(object sender, EventArgs e) => Delete();

        private async void ctxCopy_Click(object sender, EventArgs e)
        {
            using var imagesToCopy = _imageList.Selection.Select(x => x.GetClonedImage()).ToDisposableList();
            await _imageClipboard.Write(imagesToCopy.InnerList, true);
        }

        private void ctxPaste_Click(object sender, EventArgs e)
        {
            if (_imageTransfer.IsInClipboard())
            {
                ImportDirect(_imageTransfer.GetFromClipboard(), true);
            }
        }

        #endregion

        #region Thumbnail Resizing

        private void StepThumbnailSize(double step)
        {
            int thumbnailSize = Config.Get(c => c.ThumbnailSize);
            thumbnailSize =
                (int) ThumbnailSizes.StepNumberToSize(ThumbnailSizes.SizeToStepNumber(thumbnailSize) + step);
            thumbnailSize = Math.Max(Math.Min(thumbnailSize, ThumbnailSizes.MAX_SIZE), ThumbnailSizes.MIN_SIZE);
            Config.User.Set(c => c.ThumbnailSize, thumbnailSize);
            ResizeThumbnails(thumbnailSize);
        }

        private void ResizeThumbnails(int thumbnailSize)
        {
            if (!_imageList.Images.Any())
            {
                // Can't show visual feedback so don't do anything
                // TODO: This is wrong?
                return;
            }
            if (thumbnailList1.ThumbnailSize == thumbnailSize)
            {
                // Same size so no resizing needed
                return;
            }

            // Adjust the visible thumbnail display with the new size
            lock (thumbnailList1)
            {
                thumbnailList1.ThumbnailSize = thumbnailSize;
                thumbnailList1.RegenerateThumbnailList(_imageList.Images);
            }

            SetThumbnailSpacing(thumbnailSize);
            UpdateToolbar(); // TODO: Do we need this?

            // Render high-quality thumbnails at the new size in a background task
            // The existing (poorly scaled) thumbnails are used in the meantime
            _thumbnailRenderQueue.SetThumbnailSize(thumbnailSize);
        }

        private void SetThumbnailSpacing(int thumbnailSize)
        {
            thumbnailList1.Padding = new Padding(0, 20, 0, 0);
            const int MIN_PADDING = 6;
            const int MAX_PADDING = 66;
            // Linearly scale the padding with the thumbnail size
            int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) /
                (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
            int spacing = thumbnailSize + padding * 2;
            ListViewNative.SetListSpacing(thumbnailList1, spacing, spacing);
        }

        private void btnZoomOut_Click(object sender, EventArgs e) => StepThumbnailSize(-1);
        private void btnZoomIn_Click(object sender, EventArgs e) => StepThumbnailSize(1);

        #endregion

        #region Drag/Drop

        // TODO: Get rid of shared drag/drop code by using WinFormsListView instead of ThumbnailList
        private void thumbnailList1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Provide drag data
            var selection = _imageList.Selection.ToList();
            if (selection.Any())
            {
                var ido = new DataObject();
                using var selectedImages = selection.Select(x => x.GetClonedImage()).ToDisposableList();
                _imageTransfer.AddTo(ido.ToEto(), selectedImages.InnerList);
                DoDragDrop(ido, DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void thumbnailList1_DragEnter(object sender, DragEventArgs e)
        {
            // Determine if drop data is compatible
            try
            {
                if (_imageTransfer.IsIn(e.Data.ToEto()))
                {
                    var data = _imageTransfer.GetFrom(e.Data.ToEto());
                    e.Effect = data.ProcessId == Process.GetCurrentProcess().Id
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
            if (_imageTransfer.IsIn(e.Data.ToEto()))
            {
                var data = _imageTransfer.GetFrom(e.Data.ToEto());
                if (data.ProcessId == Process.GetCurrentProcess().Id)
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
                var data = (string[]) e.Data.GetData(DataFormats.FileDrop);
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
            if (!_imageList.Selection.Any())
            {
                return;
            }
            int index = GetDragIndex(e);
            if (index != -1)
            {
                _userActions.MoveTo(index);
            }
        }

        private void thumbnailList1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                var index = GetDragIndex(e);
                if (index == _imageList.Images.Count)
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
                var row = items.Where(x => x.Bounds.Top <= cp.Y && x.Bounds.Bottom >= cp.Y).OrderBy(x => x.Bounds.X)
                    .ToList();
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