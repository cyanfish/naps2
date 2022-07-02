#region Usings

using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport;
using NAPS2.Ocr;
using NAPS2.ImportExport.Images;

#endregion

namespace NAPS2.WinForms;

public partial class FDesktop : FormBase
{
    private readonly ToolbarFormatter _toolbarFormatter;
    private readonly TesseractLanguageManager _tesseractLanguageManager;
    private readonly IScannedImagePrinter _scannedImagePrinter;
    private readonly KeyboardShortcutManager _ksm;
    private readonly ThumbnailRenderer _thumbnailRenderer;
    private readonly INotificationManager _notify;
    private readonly CultureHelper _cultureHelper;
    private readonly IProfileManager _profileManager;
    private readonly UiImageList _imageList;
    private readonly ImageTransfer _imageTransfer;
    private readonly ThumbnailRenderQueue _thumbnailRenderQueue;
    private readonly UiThumbnailProvider _thumbnailProvider;
    private readonly DesktopController _desktopController;
    private readonly IDesktopScanController _desktopScanController;
    private readonly ImageListActions _imageListActions;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly DesktopSubFormController _desktopSubFormController;

    private WinFormsListView<UiImage> _listView;
    private ImageListSyncer? _imageListSyncer;
    private LayoutManager _layoutManager;

    #region Initialization and Culture

    public FDesktop(
        ToolbarFormatter toolbarFormatter,
        TesseractLanguageManager tesseractLanguageManager,
        IScannedImagePrinter scannedImagePrinter,
        KeyboardShortcutManager ksm,
        ThumbnailRenderer thumbnailRenderer,
        INotificationManager notify,
        CultureHelper cultureHelper,
        IProfileManager profileManager,
        UiImageList imageList,
        ImageTransfer imageTransfer,
        ThumbnailRenderQueue thumbnailRenderQueue,
        UiThumbnailProvider thumbnailProvider,
        DesktopController desktopController,
        IDesktopScanController desktopScanController,
        ImageListActions imageListActions,
        DesktopFormProvider desktopFormProvider, DesktopSubFormController desktopSubFormController)
    {
        _toolbarFormatter = toolbarFormatter;
        _tesseractLanguageManager = tesseractLanguageManager;
        _scannedImagePrinter = scannedImagePrinter;
        _ksm = ksm;
        _thumbnailRenderer = thumbnailRenderer;
        _notify = notify;
        _cultureHelper = cultureHelper;
        _profileManager = profileManager;
        _imageList = imageList;
        _imageTransfer = imageTransfer;
        _thumbnailRenderQueue = thumbnailRenderQueue;
        _thumbnailProvider = thumbnailProvider;
        _desktopController = desktopController;
        _desktopScanController = desktopScanController;
        _imageListActions = imageListActions;
        _desktopFormProvider = desktopFormProvider;
        _desktopSubFormController = desktopSubFormController;
        InitializeComponent();

        notify.ParentForm = this;
        Shown += FDesktop_Shown;
        FormClosing += FDesktop_FormClosing;
        Closed += FDesktop_Closed;
        imageList.SelectionChanged += (_, _) =>
        {
            SafeInvoke(() =>
            {
                UpdateToolbar();
                _listView!.Selection = _imageList.Selection;
            });
        };
        imageList.ImagesUpdated += (_, _) => SafeInvoke(UpdateToolbar);
        _profileManager.ProfilesUpdated += (_, _) => UpdateScanButton();
        _desktopFormProvider.DesktopForm = this;
    }

    protected override void OnLoad(object sender, EventArgs args) => PostInitializeComponent();

    protected override void AfterLoad(object sender, EventArgs args) => AfterLayout();

    /// <summary>
    /// Runs when the form is first loaded and every time the language is changed.
    /// </summary>
    private void PostInitializeComponent()
    {
        // TODO: Migrate the whole FDesktop to Eto
        // For now, as a partial migration, we're using our Eto ListView abstraction directly. 
        _listView = new WinFormsListView<UiImage>(new ImageListViewBehavior(_thumbnailProvider, _imageTransfer))
        {
            AllowDrag = true,
            AllowDrop = true
        };
        _listView.Selection = _imageList.Selection;
        _listView.ItemClicked += ListViewItemClicked;
        _listView.Drop += ListViewDrop;
        _listView.SelectionChanged += ListViewSelectionChanged;
        _listView.NativeControl.TabIndex = 7;
        _listView.NativeControl.Dock = DockStyle.Fill;
        _listView.NativeControl.ContextMenuStrip = contextMenuStrip;
        _listView.NativeControl.KeyDown += ListViewKeyDown;
        _listView.NativeControl.MouseWheel += ListViewMouseWheel;
        toolStripContainer1.ContentPanel.Controls.Add(_listView.NativeControl);
        _imageListSyncer?.Dispose();
        _imageListSyncer = new ImageListSyncer(_imageList, _listView.ApplyDiffs, SynchronizationContext.Current);

        foreach (var panel in toolStripContainer1.Controls.OfType<ToolStripPanel>())
        {
            // Allow tabbing through the toolbar for accessibility
            WinFormsHacks.SetControlStyle(panel, ControlStyles.Selectable, true);
        }
        _imageList.ThumbnailRenderer = _thumbnailRenderer;
        int thumbnailSize = Config.Get(c => c.ThumbnailSize);
        _listView.ImageSize = thumbnailSize;
        SetThumbnailSpacing(thumbnailSize);

        // TODO: Verify that hidden buttons can't be accessed via keyboard shortcut
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
        btnZoomIn.Location = new Point(btnZoomIn.Location.X, _listView.NativeControl.Height - 33);
        btnZoomOut.Location = new Point(btnZoomOut.Location.X, _listView.NativeControl.Height - 33);
        btnZoomMouseCatcher.Location =
            new Point(btnZoomMouseCatcher.Location.X, _listView.NativeControl.Height - 33);
        _layoutManager = new LayoutManager(this)
            .Bind(btnZoomIn, btnZoomOut, btnZoomMouseCatcher)
            .BottomTo(() => _listView.NativeControl.Height)
            .Activate();
        _listView.NativeControl.SizeChanged += (_, _) => _layoutManager.UpdateLayout();

        _imageListSyncer.SyncNow();
        _listView.NativeControl.Focus();
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
        await _desktopController.Initialize();
    }

    #endregion

    #region Cleanup

    private void FDesktop_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_desktopController.PrepareForClosing(e.CloseReason == CloseReason.UserClosing))
        {
            e.Cancel = true;
        }
    }

    private void FDesktop_Closed(object sender, EventArgs e)
    {
        SaveToolStripLocation();
        _desktopController.Cleanup();
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
            // TODO: Eventually, once we have a native linux UI, we can get rid of mono hacks
            // TODO: But in the meantime we should verify mono behavior
            _listView.NativeControl.Size =
                new Size(_listView.NativeControl.Width - 1, _listView.NativeControl.Height - 1);
            _listView.NativeControl.Size =
                new Size(_listView.NativeControl.Width + 1, _listView.NativeControl.Height + 1);
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
            item.Click += async (_, _) => await _desktopScanController.ScanWithProfile(profile);
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

    #region Keyboard Shortcuts

    private void AssignKeyboardShortcuts()
    {
        // Defaults

        _ksm.Assign("Ctrl+Enter", tsScan);
        _ksm.Assign("Ctrl+B", tsBatchScan);
        _ksm.Assign("Ctrl+O", tsImport);
        _ksm.Assign("Ctrl+S", tsdSavePDF);
        _ksm.Assign("Ctrl+P", tsPrint);
        _ksm.Assign("Ctrl+Up", _imageListActions.MoveUp);
        _ksm.Assign("Ctrl+Left", _imageListActions.MoveUp);
        _ksm.Assign("Ctrl+Down", _imageListActions.MoveDown);
        _ksm.Assign("Ctrl+Right", _imageListActions.MoveDown);
        _ksm.Assign("Ctrl+Shift+Del", tsClear);
        _ksm.Assign("F1", _desktopSubFormController.ShowAboutForm);
        _ksm.Assign("Ctrl+OemMinus", btnZoomOut);
        _ksm.Assign("Ctrl+Oemplus", btnZoomIn);
        _ksm.Assign("Del", ctxDelete);
        _ksm.Assign("Ctrl+A", ctxSelectAll);
        _ksm.Assign("Ctrl+C", ctxCopy);
        _ksm.Assign("Ctrl+V", ctxPaste);

        // Configured

        var ks = Config.Get(c => c.KeyboardShortcuts);

        _ksm.Assign(ks.About, _desktopSubFormController.ShowAboutForm);
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
        _ksm.Assign(ks.MoveDown, _imageListActions.MoveDown);
        _ksm.Assign(ks.MoveUp, _imageListActions.MoveUp);
        _ksm.Assign(ks.NewProfile, tsNewProfile);
        _ksm.Assign(ks.Ocr, tsOcr);
        _ksm.Assign(ks.Print, tsPrint);
        _ksm.Assign(ks.Profiles, tsProfiles);

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

    private void ListViewKeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = _ksm.Perform(e.KeyData);
    }

    private void ListViewMouseWheel(object? sender, MouseEventArgs e)
    {
        if (ModifierKeys.HasFlag(Keys.Control))
        {
            StepThumbnailSize(e.Delta / (double) SystemInformation.MouseWheelScrollDelta);
        }
    }

    #endregion

    #region Event Handlers - Misc

    private void tStrip_ParentChanged(object sender, EventArgs e) => _toolbarFormatter.RelayoutToolbar(tStrip);

    #endregion

    #region Event Handlers - Toolbar

    private async void tsScan_ButtonClick(object sender, EventArgs e) => await _desktopScanController.ScanDefault();

    private async void tsNewProfile_Click(object sender, EventArgs e) =>
        await _desktopScanController.ScanWithNewProfile();

    private void tsBatchScan_Click(object sender, EventArgs e) => _desktopSubFormController.ShowBatchScanForm();
    private void tsProfiles_Click(object sender, EventArgs e) => _desktopSubFormController.ShowProfilesForm();

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

    private void tsImport_Click(object sender, EventArgs e) => _desktopController.Import();

    private async void tsdSavePDF_ButtonClick(object sender, EventArgs e)
    {
        var action = Config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            tsdSavePDF.ShowDropDown();
        }
        else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
        {
            await _desktopController.SavePDF(_imageList.Selection.ToList());
        }
        else
        {
            await _desktopController.SavePDF(_imageList.Images);
        }
    }

    private async void tsdSaveImages_ButtonClick(object sender, EventArgs e)
    {
        var action = Config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            tsdSaveImages.ShowDropDown();
        }
        else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
        {
            await _desktopController.SaveImages(_imageList.Selection.ToList());
        }
        else
        {
            await _desktopController.SaveImages(_imageList.Images);
        }
    }

    private async void tsdEmailPDF_ButtonClick(object sender, EventArgs e)
    {
        var action = Config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            tsdEmailPDF.ShowDropDown();
        }
        else if (action == SaveButtonDefaultAction.SaveSelected && _imageList.Selection.Any())
        {
            await _desktopController.EmailPDF(_imageList.Selection.ToList());
        }
        else
        {
            await _desktopController.EmailPDF(_imageList.Images);
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

    private void tsMove_FirstClick(object sender, EventArgs e) => _imageListActions.MoveUp();

    private void tsMove_SecondClick(object sender, EventArgs e) => _imageListActions.MoveDown();

    private void tsDelete_Click(object sender, EventArgs e) => _desktopController.Delete();

    private void tsClear_Click(object sender, EventArgs e) => _desktopController.Clear();

    private void tsAbout_Click(object sender, EventArgs e) => _desktopSubFormController.ShowAboutForm();

    private void tsSettings_Click(object sender, EventArgs e) => _desktopSubFormController.ShowSettingsForm();

    #endregion

    #region Event Handlers - Save/Email Menus

    private async void tsSavePDFAll_Click(object sender, EventArgs e) =>
        await _desktopController.SavePDF(_imageList.Images);

    private async void tsSavePDFSelected_Click(object sender, EventArgs e) =>
        await _desktopController.SavePDF(_imageList.Selection.ToList());

    private async void tsPDFSettings_Click(object sender, EventArgs e) =>
        FormFactory.Create<FPdfSettings>().ShowDialog();

    private async void tsSaveImagesAll_Click(object sender, EventArgs e) =>
        await _desktopController.SaveImages(_imageList.Images);

    private async void tsSaveImagesSelected_Click(object sender, EventArgs e) =>
        await _desktopController.SaveImages(_imageList.Selection.ToList());

    private void tsImageSettings_Click(object sender, EventArgs e) =>
        FormFactory.Create<FImageSettings>().ShowDialog();

    private async void tsEmailPDFAll_Click(object sender, EventArgs e) =>
        await _desktopController.EmailPDF(_imageList.Images);

    private async void tsEmailPDFSelected_Click(object sender, EventArgs e) =>
        await _desktopController.EmailPDF(_imageList.Selection.ToList());

    private void tsPdfSettings2_Click(object sender, EventArgs e) =>
        FormFactory.Create<FPdfSettings>().ShowDialog();

    private void tsEmailSettings_Click(object sender, EventArgs e) =>
        FormFactory.Create<FEmailSettings>().ShowDialog();

    #endregion

    #region Event Handlers - Image Menu

    private void tsView_Click(object sender, EventArgs e) => _desktopSubFormController.ShowViewerForm();
    private void tsCrop_Click(object sender, EventArgs e) => _desktopSubFormController.ShowImageForm<FCrop>();

    private void tsBrightnessContrast_Click(object sender, EventArgs e) =>
        _desktopSubFormController.ShowImageForm<FBrightnessContrast>();

    private void tsHueSaturation_Click(object sender, EventArgs e) =>
        _desktopSubFormController.ShowImageForm<FHueSaturation>();

    private void tsBlackWhite_Click(object sender, EventArgs e) =>
        _desktopSubFormController.ShowImageForm<FBlackWhite>();

    private void tsSharpen_Click(object sender, EventArgs e) => _desktopSubFormController.ShowImageForm<FSharpen>();
    private void tsReset_Click(object sender, EventArgs e) => _desktopController.ResetImage();

    #endregion

    #region Event Handlers - Rotate Menu

    private async void tsRotateLeft_Click(object sender, EventArgs e) => await _imageListActions.RotateLeft();
    private async void tsRotateRight_Click(object sender, EventArgs e) => await _imageListActions.RotateRight();
    private async void tsFlip_Click(object sender, EventArgs e) => await _imageListActions.Flip();
    private void tsDeskew_Click(object sender, EventArgs e) => _imageListActions.Deskew();
    private void tsCustomRotation_Click(object sender, EventArgs e) => _desktopSubFormController.ShowImageForm<FRotate>();

    #endregion

    #region Event Handlers - Reorder Menu

    private void tsInterleave_Click(object sender, EventArgs e) => _imageListActions.Interleave();
    private void tsDeinterleave_Click(object sender, EventArgs e) => _imageListActions.Deinterleave();
    private void tsAltInterleave_Click(object sender, EventArgs e) => _imageListActions.AltInterleave();
    private void tsAltDeinterleave_Click(object sender, EventArgs e) => _imageListActions.AltDeinterleave();
    private void tsReverseAll_Click(object sender, EventArgs e) => _imageListActions.ReverseAll();
    private void tsReverseSelected_Click(object sender, EventArgs e) => _imageListActions.ReverseSelected();

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

    private void ctxSelectAll_Click(object sender, EventArgs e) => _imageListActions.SelectAll();
    private void ctxView_Click(object sender, EventArgs e) => _desktopSubFormController.ShowViewerForm();
    private void ctxDelete_Click(object sender, EventArgs e) => _desktopController.Delete();

    private async void ctxCopy_Click(object sender, EventArgs e) => await _desktopController.Copy();

    private void ctxPaste_Click(object sender, EventArgs e) => _desktopController.Paste();

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
        if (_listView.ImageSize == thumbnailSize)
        {
            // Same size so no resizing needed
            return;
        }

        // Adjust the visible thumbnail display with the new size
        _listView.ImageSize = thumbnailSize;
        _listView.RegenerateImages();

        SetThumbnailSpacing(thumbnailSize);
        UpdateToolbar(); // TODO: Do we need this?

        // Render high-quality thumbnails at the new size in a background task
        // The existing (poorly scaled) thumbnails are used in the meantime
        _thumbnailRenderQueue.SetThumbnailSize(thumbnailSize);
    }

    private void SetThumbnailSpacing(int thumbnailSize)
    {
        _listView.NativeControl.Padding = new Padding(0, 20, 0, 0);
        const int MIN_PADDING = 6;
        const int MAX_PADDING = 66;
        // Linearly scale the padding with the thumbnail size
        int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) /
            (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
        int spacing = thumbnailSize + padding * 2;
        WinFormsHacks.SetListSpacing(_listView.NativeControl, spacing, spacing);
    }

    private void btnZoomOut_Click(object sender, EventArgs e) => StepThumbnailSize(-1);
    private void btnZoomIn_Click(object sender, EventArgs e) => StepThumbnailSize(1);

    #endregion

    #region Drag/Drop

    private void ListViewItemClicked(object? sender, EventArgs e) => _desktopSubFormController.ShowViewerForm();

    private void ListViewSelectionChanged(object? sender, EventArgs e)
    {
        _imageList.UpdateSelection(_listView.Selection);
        UpdateToolbar();
    }

    private void ListViewDrop(object? sender, DropEventArgs args)
    {
        if (_imageTransfer.IsIn(args.Data.ToEto()))
        {
            var data = _imageTransfer.GetFrom(args.Data.ToEto());
            if (data.ProcessId == Process.GetCurrentProcess().Id)
            {
                DragMoveImages(args.Position);
            }
            else
            {
                _desktopController.ImportDirect(data, false);
            }
        }
        else if (args.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var data = (string[]) args.Data.GetData(DataFormats.FileDrop);
            _desktopController.ImportFiles(data);
        }
    }

    private void DragMoveImages(int position)
    {
        if (!_imageList.Selection.Any())
        {
            return;
        }
        if (position != -1)
        {
            _imageListActions.MoveTo(position);
        }
    }

    #endregion
}