#region Usings

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
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport;
using NAPS2.Lang;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Platform;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Worker;
using NAPS2.Wia;
using NAPS2.Update;
using NAPS2.Util;

#endregion

namespace NAPS2.WinForms
{
    public partial class FDesktop : FormBase
    {
        #region Dependencies

        private static readonly MethodInfo ToolStripPanelSetStyle = typeof(ToolStripPanel).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ImageContext _imageContext;
        private readonly StringWrapper _stringWrapper;
        private readonly RecoveryManager _recoveryManager;
        private readonly OcrEngineManager _ocrEngineManager;
        private readonly IScanPerformer _scanPerformer;
        private readonly IScannedImagePrinter _scannedImagePrinter;
        private readonly StillImage _stillImage;
        private readonly IOperationFactory _operationFactory;
        private readonly KeyboardShortcutManager _ksm;
        private readonly ThumbnailRenderer _thumbnailRenderer;
        private readonly WinFormsExportHelper _exportHelper;
        private readonly ImageClipboard _imageClipboard;
        private readonly ImageRenderer _imageRenderer;
        private readonly NotificationManager _notify;
        private readonly CultureInitializer _cultureInitializer;
        private readonly IWorkerFactory _workerFactory;
        private readonly OperationProgress _operationProgress;
        private readonly UpdateChecker _updateChecker;
        private readonly IProfileManager _profileManager;
        private readonly ScannedImageList _imageList;

        #endregion

        #region State Fields

        private readonly UserActions _userActions;
        private readonly AutoResetEvent _renderThumbnailsWaitHandle = new AutoResetEvent(false);
        private bool _closed = false;
        private LayoutManager _layoutManager;
        private bool _disableSelectedIndexChangedEvent;
        
        public bool SkipRecoveryCleanup { get; set; }

        #endregion

        #region Initialization and Culture

        public FDesktop(ImageContext imageContext, StringWrapper stringWrapper, RecoveryManager recoveryManager, OcrEngineManager ocrEngineManager, IScanPerformer scanPerformer, IScannedImagePrinter scannedImagePrinter, StillImage stillImage, IOperationFactory operationFactory, KeyboardShortcutManager ksm, ThumbnailRenderer thumbnailRenderer, WinFormsExportHelper exportHelper, ImageClipboard imageClipboard, ImageRenderer imageRenderer, NotificationManager notify, CultureInitializer cultureInitializer, IWorkerFactory workerFactory, OperationProgress operationProgress, UpdateChecker updateChecker, IProfileManager profileManager, ScannedImageList imageList)
        {
            _imageContext = imageContext;
            _stringWrapper = stringWrapper;
            _recoveryManager = recoveryManager;
            _ocrEngineManager = ocrEngineManager;
            _scanPerformer = scanPerformer;
            _scannedImagePrinter = scannedImagePrinter;
            _stillImage = stillImage;
            _operationFactory = operationFactory;
            _ksm = ksm;
            _thumbnailRenderer = thumbnailRenderer;
            _exportHelper = exportHelper;
            _imageClipboard = imageClipboard;
            _imageRenderer = imageRenderer;
            _notify = notify;
            _cultureInitializer = cultureInitializer;
            _workerFactory = workerFactory;
            _operationProgress = operationProgress;
            _updateChecker = updateChecker;
            _profileManager = profileManager;
            _imageList = imageList;
            _userActions = new UserActions(imageContext, imageList);
            InitializeComponent();

            notify.ParentForm = this;
            Shown += FDesktop_Shown;
            FormClosing += FDesktop_FormClosing;
            Closed += FDesktop_Closed;
            thumbnailList1.ItemSelectionChanged += (sender, args) => imageList.Selection = ListSelection.From(SelectedImages); 
            // TODO: Use a delta operation (using snapshot/memento logic) rather than added/updated/deleted
            imageList.ImagesUpdated += (sender, args) =>
            {
                SelectedIndices = imageList.Selection.ToSelectedIndices(imageList.Images);
                UpdateThumbnails();
            };
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
            _imageList.ThumbnailRenderer = _thumbnailRenderer;
            thumbnailList1.ThumbnailRenderer = _thumbnailRenderer;
            int thumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize);
            thumbnailList1.ThumbnailSize = new Size(thumbnailSize, thumbnailSize);
            SetThumbnailSpacing(thumbnailSize);

            var hiddenButtons = ConfigProvider.Get(c => c.HiddenButtons);
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
            RelayoutToolbar();
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
                    btn.Text = _stringWrapper.Wrap(btn.Text ?? "", 80, g, btn.Font);
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
            ConfigScopes.User.Set(c => c.Culture = cultureId);
            _cultureInitializer.InitCulture();

            // Update localized values
            // Since all forms are opened modally and this is the root form, it should be the only one that needs to be updated live
            SaveFormState = false;
            Controls.Clear();
            UpdateRTL();
            InitializeComponent();
            PostInitializeComponent();
            UpdateThumbnails();
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
            if (!string.IsNullOrWhiteSpace(ConfigProvider.Get(c => c.StartupMessageText)))
            {
                MessageBox.Show(ConfigProvider.Get(c => c.StartupMessageText), ConfigProvider.Get(c => c.StartupMessageTitle), MessageBoxButtons.OK,
                    ConfigProvider.Get(c => c.StartupMessageIcon));
            }

            // Allow scanned images to be recovered in case of an unexpected close
            _recoveryManager.RecoverScannedImages(ReceiveScannedImage(), new RecoveryParams { ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize) });

            new Thread(RenderThumbnails).Start();

            // If NAPS2 was started by the scanner button, do the appropriate actions automatically
            await RunStillImageEvents();

            // Show a donation prompt after a month of use
            if (!ConfigProvider.Get(c => c.HasBeenRun))
            {
                ConfigScopes.User.SetAll(new CommonConfig
                {
                    HasBeenRun = true,
                    FirstRunDate = DateTime.Now
                });
            }
#if !INSTALLER_MSI
            else if (!ConfigProvider.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.Donate) &&
                !ConfigProvider.Get(c => c.HasBeenPromptedForDonation) &&
                DateTime.Now - ConfigProvider.Get(c => c.FirstRunDate) > TimeSpan.FromDays(30))
            {
                ConfigScopes.User.SetAll(new CommonConfig
                {
                    HasBeenPromptedForDonation = true,
                    LastDonatePromptDate = DateTime.Now
                });
                _notify.DonatePrompt();
            }

            if (ConfigProvider.Get(c => c.CheckForUpdates) &&
                (!ConfigProvider.Get(c => c.HasCheckedForUpdates) ||
                 ConfigProvider.Get(c => c.LastUpdateCheckDate) < DateTime.Now - _updateChecker.CheckInterval))
            {
                _updateChecker.CheckForUpdates().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.ErrorException("Error checking for updates", task.Exception);
                    }
                    else
                    {
                        ConfigScopes.User.SetAll(new CommonConfig
                        {
                            HasCheckedForUpdates = true,
                            LastUpdateCheckDate = DateTime.Now
                        });
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
                    _imageContext.Dispose();
                }
                catch (Exception ex)
                {
                    Log.ErrorException("ImageContext.Dispose failed", ex);
                }
            }
            _closed = true;
            _renderThumbnailsWaitHandle.Set();
        }

        #endregion

        #region Scanning and Still Image

        private async Task RunStillImageEvents()
        {
            if (_stillImage.ShouldScan)
            {
                await ScanWithDevice(_stillImage.DeviceID);
            }
        }

        private ScanParams DefaultScanParams() =>
            new ScanParams
            {
                NoAutoSave = ConfigProvider.Get(c => c.DisableAutoSave),
                DoOcr = ConfigProvider.Get(c => c.EnableOcr) && ConfigProvider.Get(c => c.OcrAfterScanning),
                ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize)
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
                if (ConfigProvider.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked))
                {
                    return;
                }

                // No profile for the device we're scanning with, so prompt to create one
                var editSettingsForm = FormFactory.Create<FEditProfile>();
                editSettingsForm.ScanProfile = ConfigProvider.Get(c => c.DefaultProfileSettings);
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
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile), ListSelection.Empty<ScanProfile>());
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
                var source = await _scanPerformer.PerformScan(_profileManager.DefaultProfile, DefaultScanParams(), Handle);
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
            editSettingsForm.ScanProfile = ConfigProvider.Get(c => c.DefaultProfileSettings);
            editSettingsForm.ShowDialog();
            if (!editSettingsForm.Result)
            {
                return;
            }
            _profileManager.Mutate(new ListMutation<ScanProfile>.Append(editSettingsForm.ScanProfile), ListSelection.Empty<ScanProfile>());
            _profileManager.DefaultProfile = editSettingsForm.ScanProfile;

            UpdateScanButton();

            var source = await _scanPerformer.PerformScan(editSettingsForm.ScanProfile, DefaultScanParams(), Handle);
            await source.ForEach(ReceiveScannedImage());
            Activate();
        }

        #endregion

        #region Images and Thumbnails

        private IEnumerable<int> SelectedIndices
        {
            get => thumbnailList1.SelectedIndices.Cast<int>();
            set
            {
                _disableSelectedIndexChangedEvent = true;
                if (_imageList.Images.Count == 0)
                {
                    thumbnailList1.Clear();
                }
                else
                {
                    thumbnailList1.SelectedIndices.Clear();
                    foreach (int i in value)
                    {
                        thumbnailList1.SelectedIndices.Add(i);
                    }
                }
                _disableSelectedIndexChangedEvent = false;
                thumbnailList1_SelectedIndexChanged(thumbnailList1, new EventArgs());
            }
        }

        private IEnumerable<ScannedImage> SelectedImages => _imageList.Images.ElementsAt(SelectedIndices);

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
                    lock (_imageList)
                    {
                        scannedImage.ThumbnailChanged += ImageThumbnailChanged;
                        scannedImage.ThumbnailInvalidated += ImageThumbnailInvalidated;
                        _imageList.Mutate(new ImageListMutation.InsertAfter(scannedImage, last));
                        last = scannedImage;
                    }
                });
                // Trigger thumbnail rendering just in case the received image is out of date
                _renderThumbnailsWaitHandle.Set();
            };
        }

        private void UpdateThumbnails()
        {
            thumbnailList1.UpdatedImages(_imageList.Images, out var orderingChanged);
            UpdateToolbar();

            if (orderingChanged)
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
                    lock (_imageList)
                    {
                        int index = _imageList.Images.IndexOf(image);
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
                    lock (_imageList)
                    {
                        int index = _imageList.Images.IndexOf(image);
                        if (index != -1 && image.IsThumbnailDirty)
                        {
                            thumbnailList1.ReplaceThumbnail(index, image);
                        }
                    }
                }
                _renderThumbnailsWaitHandle.Set();
            });
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
                string.Format(MiscResources.SelectedCount, SelectedIndices.Count());
            tsSavePDFSelected.Enabled = tsSaveImagesSelected.Enabled = tsEmailPDFSelected.Enabled = tsReverseSelected.Enabled =
                SelectedIndices.Any();

            // Top-level toolbar actions
            tsdImage.Enabled = tsdRotate.Enabled = tsMove.Enabled = tsDelete.Enabled = SelectedIndices.Any();
            tsdReorder.Enabled = tsdSavePDF.Enabled = tsdSaveImages.Enabled = tsdEmailPDF.Enabled = tsPrint.Enabled = tsClear.Enabled = _imageList.Images.Any();

            // Context-menu actions
            ctxView.Visible = ctxCopy.Visible = ctxDelete.Visible = ctxSeparator1.Visible = ctxSeparator2.Visible = SelectedIndices.Any();
            ctxSelectAll.Enabled = _imageList.Images.Any();

            // Other
            btnZoomIn.Enabled = _imageList.Images.Any() && ConfigProvider.Get(c => c.ThumbnailSize) < ThumbnailSizes.MAX_SIZE;
            btnZoomOut.Enabled = _imageList.Images.Any() && ConfigProvider.Get(c => c.ThumbnailSize) > ThumbnailSizes.MIN_SIZE;
            tsNewProfile.Enabled = !(ConfigProvider.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked));

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
            ConfigScopes.User.Set(c => c.DesktopToolStripDock = tStrip.Parent.Dock);
        }

        private void LoadToolStripLocation()
        {
            var dock = ConfigProvider.Get(c => c.DesktopToolStripDock);
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
                if (MessageBox.Show(string.Format(MiscResources.ConfirmClearItems, _imageList.Images.Count), MiscResources.Clear, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    _userActions.DeleteAll();
                }
            }
        }

        private void Delete()
        {
            if (SelectedIndices.Any())
            {
                if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, SelectedIndices.Count()), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    _userActions.DeleteSelected();
                }
            }
        }

        private void PreviewImage()
        {
            if (SelectedIndices.Any())
            {
                using var viewer = FormFactory.Create<FViewer>();
                viewer.ImageList = _imageList;
                viewer.ImageIndex = SelectedIndices.First();
                viewer.DeleteCallback = UpdateThumbnails;
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
                    _userActions.ResetTransforms();
                }
            }
        }

        private void OpenAbout()
        {
            FormFactory.Create<AboutForm>().ShowModal();
        }

        private void OpenSettings()
        {
            // FormFactory.Create<FSettings>().ShowDialog();
        }

        #endregion

        #region Actions - Save/Email/Import

        private async void SavePDF(List<ScannedImage> images)
        {
            if (await _exportHelper.SavePDF(images, _notify))
            {
                if (ConfigProvider.Get(c => c.DeleteAfterSaving))
                {
                    SafeInvoke(() =>
                    {
                        _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
                    });
                }
            }
        }

        private async void SaveImages(List<ScannedImage> images)
        {
            if (await _exportHelper.SaveImages(images, _notify))
            {
                if (ConfigProvider.Get(c => c.DeleteAfterSaving))
                {
                    _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(images));
                }
            }
        }

        private async void EmailPDF(List<ScannedImage> images)
        {
            await _exportHelper.EmailPDF(images);
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
            var op = _operationFactory.Create<ImportOperation>();
            if (op.Start(OrderFiles(files), ReceiveScannedImage(), new ImportParams { ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize) }))
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

        private void ImportDirect(DirectImageTransfer data, bool copy)
        {
            var op = _operationFactory.Create<DirectImportOperation>();
            if (op.Start(data, copy, ReceiveScannedImage(), new DirectImportParams { ThumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize) }))
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
            var ks = ConfigProvider.Get(c => c.KeyboardShortcuts);

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
            var ks = ConfigProvider.Get(c => c.KeyboardShortcuts);
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
            if (!_disableSelectedIndexChangedEvent)
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
            if (_ocrEngineManager.MustUpgrade && !ConfigProvider.Get(c => c.NoUpdatePrompt))
            {
                // Re-download a fixed version on Windows XP if needed
                MessageBox.Show(MiscResources.OcrUpdateAvailable, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var progressForm = FormFactory.Create<FDownloadProgress>();
                progressForm.QueueFile(_ocrEngineManager.EngineToInstall.Component);
                progressForm.ShowDialog();
            }

            if (_ocrEngineManager.MustInstallPackage)
            {
                const string packages = "\ntesseract-ocr";
                MessageBox.Show(MiscResources.TesseractNotAvailable + packages, MiscResources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (_ocrEngineManager.IsReady)
            {
                if (_ocrEngineManager.CanUpgrade && !ConfigProvider.Get(c => c.NoUpdatePrompt))
                {
                    MessageBox.Show(MiscResources.OcrUpdateAvailable, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                }
                FormFactory.Create<FOcrSetup>().ShowDialog();
            }
            else
            {
                FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
                if (_ocrEngineManager.IsReady)
                {
                    FormFactory.Create<FOcrSetup>().ShowDialog();
                }
            }
        }

        private void tsImport_Click(object sender, EventArgs e)
        {
            Import();
        }

        private void tsdSavePDF_ButtonClick(object sender, EventArgs e)
        {
            var action = ConfigProvider.Get(c => c.SaveButtonDefaultAction);

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
                SavePDF(_imageList.Images);
            }
        }

        private void tsdSaveImages_ButtonClick(object sender, EventArgs e)
        {
            var action = ConfigProvider.Get(c => c.SaveButtonDefaultAction);

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
                SaveImages(_imageList.Images);
            }
        }

        private void tsdEmailPDF_ButtonClick(object sender, EventArgs e)
        {
            var action = ConfigProvider.Get(c => c.SaveButtonDefaultAction);

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
                EmailPDF(_imageList.Images);
            }
        }

        private async void tsPrint_Click(object sender, EventArgs e)
        {
            var state = _imageList.CurrentState; 
            if (await _scannedImagePrinter.PromptToPrint(_imageList.Images, SelectedImages.ToList()))
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

        private void tsSavePDFAll_Click(object sender, EventArgs e)
        {
            SavePDF(_imageList.Images);
        }

        private void tsSavePDFSelected_Click(object sender, EventArgs e)
        {
            SavePDF(SelectedImages.ToList());
        }

        private void tsPDFSettings_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FPdfSettings>().ShowDialog();
        }

        private void tsSaveImagesAll_Click(object sender, EventArgs e)
        {
            SaveImages(_imageList.Images);
        }

        private void tsSaveImagesSelected_Click(object sender, EventArgs e)
        {
            SaveImages(SelectedImages.ToList());
        }

        private void tsImageSettings_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FImageSettings>().ShowDialog();
        }

        private void tsEmailPDFAll_Click(object sender, EventArgs e)
        {
            EmailPDF(_imageList.Images);
        }

        private void tsEmailPDFSelected_Click(object sender, EventArgs e)
        {
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

        private async void tsRotateLeft_Click(object sender, EventArgs e) => await _userActions.RotateLeft();

        private async void tsRotateRight_Click(object sender, EventArgs e) => await _userActions.RotateRight();

        private async void tsFlip_Click(object sender, EventArgs e) => await _userActions.Flip();

        private void tsDeskew_Click(object sender, EventArgs e) => _userActions.Deskew();

        private void tsCustomRotation_Click(object sender, EventArgs e)
        {
            if (SelectedIndices.Any())
            {
                var form = FormFactory.Create<FRotate>();
                form.Image = SelectedImages.First();
                form.SelectedImages = SelectedImages.ToList();
                form.ShowDialog();
                UpdateThumbnails();
            }
        }

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
            ctxPaste.Enabled = _imageClipboard.CanRead;
            if (!_imageList.Images.Any() && !ctxPaste.Enabled)
            {
                e.Cancel = true;
            }
        }

        private void ctxSelectAll_Click(object sender, EventArgs e) => _userActions.SelectAll();

        private void ctxView_Click(object sender, EventArgs e)
        {
            PreviewImage();
        }

        private async void ctxCopy_Click(object sender, EventArgs e)
        {
            await _imageClipboard.Write(SelectedImages, true);
        }

        private void ctxPaste_Click(object sender, EventArgs e)
        {
            var direct = _imageClipboard.Read();
            if (direct != null)
            {
                ImportDirect(direct, true);
            }
        }

        private void ctxDelete_Click(object sender, EventArgs e) => Delete();

        #endregion

        #region Thumbnail Resizing

        private void StepThumbnailSize(double step)
        {
            int thumbnailSize = ConfigProvider.Get(c => c.ThumbnailSize);
            thumbnailSize = (int)ThumbnailSizes.StepNumberToSize(ThumbnailSizes.SizeToStepNumber(thumbnailSize) + step);
            thumbnailSize = Math.Max(Math.Min(thumbnailSize, ThumbnailSizes.MAX_SIZE), ThumbnailSizes.MIN_SIZE);
            ConfigScopes.User.Set(c => c.ThumbnailSize = thumbnailSize);
            ResizeThumbnails(thumbnailSize);
        }

        private void ResizeThumbnails(int thumbnailSize)
        {
            if (!_imageList.Images.Any())
            {
                // Can't show visual feedback so don't do anything
                return;
            }
            if (thumbnailList1.ThumbnailSize.Height == thumbnailSize)
            {
                // Same size so no resizing needed
                return;
            }

            // Adjust the visible thumbnail display with the new size
            lock (thumbnailList1)
            {
                thumbnailList1.ThumbnailSize = new Size(thumbnailSize, thumbnailSize);
                thumbnailList1.RegenerateThumbnailList(_imageList.Images);
            }

            SetThumbnailSpacing(thumbnailSize);
            UpdateToolbar();

            // Render high-quality thumbnails at the new size in a background task
            // The existing (poorly scaled) thumbnails are used in the meantime
            _renderThumbnailsWaitHandle.Set();
        }

        private void SetThumbnailSpacing(int thumbnailSize)
        {
            thumbnailList1.Padding = new Padding(0, 20, 0, 0);
            const int MIN_PADDING = 6;
            const int MAX_PADDING = 66;
            // Linearly scale the padding with the thumbnail size
            int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) / (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
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
            var worker = useWorker ? _workerFactory.Create() : null;
            var fallback = new ExpFallback(100, 60 * 1000);
            while (!_closed)
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
                                ? _imageContext.ImageFactory.Decode(new MemoryStream(worker.Service.RenderThumbnail(_imageContext, snapshot, thumbnailList1.ThumbnailSize.Height)), ".jpg")
                                : _thumbnailRenderer.Render(snapshot, thumbnailList1.ThumbnailSize.Height).Result;

                            if (!ThumbnailStillNeedsRendering(next))
                            {
                                continue;
                            }

                            next.SetThumbnail(thumb, snapshot.Metadata.TransformState);
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
                        worker = _workerFactory.Create();
                    }
                    Thread.Sleep(fallback.Value);
                    fallback.Increase();
                    continue;
                }
                _renderThumbnailsWaitHandle.WaitOne();
            }
        }

        private bool ThumbnailStillNeedsRendering(ScannedImage next)
        {
            lock (next)
            {
                var thumb = next.GetThumbnail();
                return thumb == null || next.IsThumbnailDirty || thumb.Width != thumbnailList1.ThumbnailSize.Width || thumb.Height != thumbnailList1.ThumbnailSize.Height;
            }
        }

        private ScannedImage GetNextThumbnailToRender()
        {
            List<ScannedImage> listCopy;
            lock (_imageList)
            {
                listCopy = _imageList.Images.ToList();
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
                var thumb = img.GetThumbnail();
                if (thumb == null || thumb.Width != thumbnailList1.ThumbnailSize.Width || thumb.Height != thumbnailList1.ThumbnailSize.Height)
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

        private void thumbnailList1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Provide drag data
            if (SelectedIndices.Any())
            {
                var ido = _imageClipboard.GetDataObject(SelectedImages);
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
