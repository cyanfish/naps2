using System.Collections.Immutable;
using Eto.Forms;
using NAPS2.ImportExport.Images;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.Ui;

public abstract class DesktopForm : EtoFormBase
{
    // private readonly KeyboardShortcutManager _ksm;
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
    private readonly IDesktopSubFormController _desktopSubFormController;

    protected readonly Command _scanCommand;
    protected readonly Command _newProfileCommand;
    protected readonly Command _batchScanCommand;
    protected readonly Command _profilesCommand;
    protected readonly Command _ocrCommand;
    protected readonly Command _importCommand;
    protected readonly Command _savePdfCommand;
    protected readonly Command _saveAllPdfCommand;
    protected readonly Command _saveSelectedPdfCommand;
    protected readonly Command _pdfSettingsCommand;
    protected readonly Command _saveImagesCommand;
    protected readonly Command _saveAllImagesCommand;
    protected readonly Command _saveSelectedImagesCommand;
    protected readonly Command _imageSettingsCommand;
    protected readonly Command _emailPdfCommand;
    protected readonly Command _emailAllPdfCommand;
    protected readonly Command _emailSelectedPdfCommand;
    protected readonly Command _emailSettingsCommand;
    protected readonly Command _printCommand;
    protected readonly Command _imageMenuCommand;
    protected readonly Command _viewImageCommand;
    protected readonly Command _cropCommand;
    protected readonly Command _brightContCommand;
    protected readonly Command _hueSatCommand;
    protected readonly Command _blackWhiteCommand;
    protected readonly Command _sharpenCommand;
    protected readonly Command _resetImageCommand;
    protected readonly Command _rotateMenuCommand;
    protected readonly Command _rotateLeftCommand;
    protected readonly Command _rotateRightCommand;
    protected readonly Command _flipCommand;
    protected readonly Command _deskewCommand;
    protected readonly Command _customRotateCommand;
    protected readonly Command _reorderMenuCommand;
    protected readonly Command _moveUpCommand;
    protected readonly Command _moveDownCommand;
    protected readonly Command _interleaveCommand;
    protected readonly Command _deinterleaveCommand;
    protected readonly Command _altInterleaveCommand;
    protected readonly Command _altDeinterleaveCommand;
    protected readonly Command _reverseMenuCommand;
    protected readonly Command _reverseAllCommand;
    protected readonly Command _reverseSelectedCommand;
    protected readonly Command _deleteCommand;
    protected readonly Command _clearAllCommand;
    protected readonly Command _languageMenuCommand;
    protected readonly Command _aboutCommand;

    private readonly ListProvider<Command> _scanMenuCommands = new();
    private readonly ListProvider<Command> _languageMenuCommands = new();

    private IListView<UiImage> _listView;
    private ImageListSyncer? _imageListSyncer;
    // private LayoutManager _layoutManager;

    public DesktopForm(
        Naps2Config config,
        // KeyboardShortcutManager ksm,
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
        DesktopFormProvider desktopFormProvider,
        IDesktopSubFormController desktopSubFormController) : base(config)
    {
        // _ksm = ksm;
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

        _scanCommand = new ActionCommand(_desktopScanController.ScanDefault)
        {
            ToolBarText = UiStrings.Scan,
            Image = Icons.control_play_blue.ToEtoImage(),
            MacSymbol = "play"
        };
        _newProfileCommand = new ActionCommand(_desktopScanController.ScanWithNewProfile)
        {
            MenuText = UiStrings.NewProfile,
            Image = Icons.add_small.ToEtoImage()
        };
        _batchScanCommand = new ActionCommand(_desktopSubFormController.ShowBatchScanForm)
        {
            MenuText = UiStrings.BatchScan,
            Image = Icons.application_cascade.ToEtoImage()
        };
        _profilesCommand = new ActionCommand(_desktopSubFormController.ShowProfilesForm)
        {
            ToolBarText = UiStrings.Profiles,
            Image = Icons.blueprints.ToEtoImage(),
            MacSymbol = "list.bullet"
        };
        _ocrCommand = new ActionCommand(_desktopSubFormController.ShowOcrForm)
        {
            ToolBarText = UiStrings.Ocr,
            Image = Icons.text.ToEtoImage()
        };
        _importCommand = new ActionCommand(() => _desktopController.Import(this))
        {
            ToolBarText = UiStrings.Import,
            Image = Icons.folder_picture.ToEtoImage()
        };
        _savePdfCommand = new ActionCommand(SavePdf)
        {
            ToolBarText = UiStrings.SavePdf,
            Image = Icons.file_extension_pdf.ToEtoImage()
        };
        _saveAllPdfCommand = new ActionCommand(() => _desktopController.SavePDF(_imageList.Images));
        _saveSelectedPdfCommand = new ActionCommand(() => _desktopController.SavePDF(_imageList.Selection.ToList()));
        _pdfSettingsCommand = new ActionCommand(_desktopSubFormController.ShowPdfSettingsForm)
        {
            MenuText = UiStrings.PdfSettings
        };
        _saveImagesCommand = new ActionCommand(SaveImages)
        {
            ToolBarText = UiStrings.SaveImages,
            Image = Icons.pictures.ToEtoImage()
        };
        _saveAllImagesCommand = new ActionCommand(() => _desktopController.SaveImages(_imageList.Images));
        _saveSelectedImagesCommand =
            new ActionCommand(() => _desktopController.SaveImages(_imageList.Selection.ToList()));
        _imageSettingsCommand = new ActionCommand(_desktopSubFormController.ShowImageSettingsForm)
        {
            MenuText = UiStrings.ImageSettings
        };
        _emailPdfCommand = new ActionCommand(EmailPdf)
        {
            ToolBarText = UiStrings.EmailPdf,
            Image = Icons.email_attach.ToEtoImage()
        };
        _emailAllPdfCommand = new ActionCommand(() => _desktopController.EmailPDF(_imageList.Images));
        _emailSelectedPdfCommand = new ActionCommand(() => _desktopController.EmailPDF(_imageList.Selection.ToList()));
        _emailSettingsCommand = new ActionCommand(_desktopSubFormController.ShowEmailSettingsForm)
        {
            MenuText = UiStrings.EmailSettings
        };
        _printCommand = new ActionCommand(_desktopController.Print)
        {
            ToolBarText = UiStrings.Print,
            Image = Icons.printer.ToEtoImage()
        };
        _imageMenuCommand = new Command
        {
            ToolBarText = UiStrings.Image,
            Image = Icons.picture_edit.ToEtoImage()
        };
        _viewImageCommand = new ActionCommand(_desktopSubFormController.ShowViewerForm)
        {
            MenuText = UiStrings.View,
            MacSymbol = "viewfinder"
        };
        _cropCommand = new ActionCommand(_desktopSubFormController.ShowCropForm)
        {
            MenuText = UiStrings.Crop,
            Image = Icons.transform_crop.ToEtoImage()
        };
        _brightContCommand = new ActionCommand(_desktopSubFormController.ShowBrightnessContrastForm)
        {
            MenuText = UiStrings.BrightnessContrast,
            Image = Icons.contrast_with_sun.ToEtoImage()
        };
        _hueSatCommand = new ActionCommand(_desktopSubFormController.ShowHueSaturationForm)
        {
            MenuText = UiStrings.HueSaturation,
            Image = Icons.color_management.ToEtoImage()
        };
        _blackWhiteCommand = new ActionCommand(_desktopSubFormController.ShowBlackWhiteForm)
        {
            MenuText = UiStrings.BlackAndWhite,
            Image = Icons.contrast_high.ToEtoImage()
        };
        _sharpenCommand = new ActionCommand(_desktopSubFormController.ShowSharpenForm)
        {
            MenuText = UiStrings.Sharpen,
            Image = Icons.sharpen.ToEtoImage()
        };
        _resetImageCommand = new ActionCommand(_desktopController.ResetImage)
        {
            MenuText = UiStrings.Reset
        };
        _rotateMenuCommand = new ActionCommand
        {
            ToolBarText = UiStrings.Rotate,
            Image = Icons.arrow_rotate_anticlockwise.ToEtoImage(),
            MacSymbol = "arrow.counterclockwise"
        };
        _rotateLeftCommand = new ActionCommand(_imageListActions.RotateLeft)
        {
            MenuText = UiStrings.RotateLeft,
            Image = Icons.arrow_rotate_anticlockwise_small.ToEtoImage(),
            MacSymbol = "arrow.counterclockwise"
        };
        _rotateRightCommand = new ActionCommand(_imageListActions.RotateRight)
        {
            MenuText = UiStrings.RotateRight,
            Image = Icons.arrow_rotate_clockwise_small.ToEtoImage(),
            MacSymbol = "arrow.clockwise"
        };
        _flipCommand = new ActionCommand(_imageListActions.Flip)
        {
            MenuText = UiStrings.Flip,
            Image = Icons.arrow_switch_small.ToEtoImage(),
            MacSymbol = "arrow.2.squarepath"
        };
        _deskewCommand = new ActionCommand(_imageListActions.Deskew)
        {
            MenuText = UiStrings.Deskew
        };
        _customRotateCommand = new ActionCommand(_desktopSubFormController.ShowRotateForm)
        {
            MenuText = UiStrings.CustomRotation
        };
        _moveUpCommand = new ActionCommand(_imageListActions.MoveUp)
        {
            ToolBarText = UiStrings.MoveUp,
            Image = Icons.arrow_up_small.ToEtoImage(),
            MacSymbol = "arrow.up"
        };
        _moveDownCommand = new ActionCommand(_imageListActions.MoveDown)
        {
            ToolBarText = UiStrings.MoveDown,
            Image = Icons.arrow_down_small.ToEtoImage(),
            MacSymbol = "arrow.down"
        };
        _reorderMenuCommand = new Command
        {
            ToolBarText = UiStrings.Reorder,
            Image = Icons.arrow_refresh.ToEtoImage()
        };
        _interleaveCommand = new ActionCommand(_imageListActions.Interleave)
        {
            MenuText = UiStrings.Interleave
        };
        _deinterleaveCommand = new ActionCommand(_imageListActions.Deinterleave)
        {
            MenuText = UiStrings.Deinterleave
        };
        _altInterleaveCommand = new ActionCommand(_imageListActions.AltInterleave)
        {
            MenuText = UiStrings.AltInterleave
        };
        _altDeinterleaveCommand = new ActionCommand(_imageListActions.AltDeinterleave)
        {
            MenuText = UiStrings.AltDeinterleave
        };
        _reverseMenuCommand = new Command
        {
            MenuText = UiStrings.Reverse
        };
        _reverseAllCommand = new ActionCommand(_imageListActions.ReverseAll);
        _reverseSelectedCommand = new ActionCommand(_imageListActions.ReverseSelected);
        _deleteCommand = new ActionCommand(_imageListActions.DeleteSelected)
        {
            ToolBarText = UiStrings.Delete,
            Image = Icons.cross.ToEtoImage()
        };
        _clearAllCommand = new ActionCommand(_imageListActions.DeleteAll)
        {
            ToolBarText = UiStrings.Clear,
            MenuText = UiStrings.ClearAll,
            Image = Icons.cancel.ToEtoImage()
        };
        _languageMenuCommand = new Command
        {
            ToolBarText = UiStrings.Language,
            Image = Icons.world.ToEtoImage()
        };
        _aboutCommand = new ActionCommand(_desktopSubFormController.ShowAboutForm)
        {
            ToolBarText = UiStrings.About,
            MenuText = UiStrings.About,
            Image = Icons.information.ToEtoImage()
        };

        // PostInitializeComponent();
        //
        Icon = Icons.favicon.ToEtoIcon();
        Title = MiscResources.NAPS2;
        CreateToolbarsAndMenus();
        UpdateScanButton();
        InitLanguageDropdown();

        _listView = EtoPlatform.Current.CreateListView(new ImageListViewBehavior(_thumbnailProvider, _imageTransfer));
        _listView.AllowDrag = true;
        _listView.AllowDrop = true;
        _listView.Selection = _imageList.Selection;
        _listView.ItemClicked += ListViewItemClicked;
        _listView.Drop += ListViewDrop;
        _listView.SelectionChanged += ListViewSelectionChanged;
        // _listView.NativeControl.TabIndex = 7;
        // _listView.NativeControl.Dock = DockStyle.Fill;
        // _listView.NativeControl.ContextMenuStrip = contextMenuStrip;
        // _listView.NativeControl.KeyDown += ListViewKeyDown;
        // _listView.NativeControl.MouseWheel += ListViewMouseWheel;
        _imageListSyncer?.Dispose();

        SetContent(_listView.Control);
        AfterLayout();

        // Content = _listView.Control;
        //
        //
        // Shown += FDesktop_Shown;
        // Closing += FDesktop_Closing;
        // Closed += FDesktop_Closed;
        // imageList.SelectionChanged += (_, _) =>
        // {
        //     Invoker.Current.SafeInvoke(() =>
        //     {
        //         UpdateToolbar();
        //         _listView!.Selection = _imageList.Selection;
        //     });
        // };
        // imageList.ImagesUpdated += (_, _) => Invoker.Current.SafeInvoke(UpdateToolbar);
        _profileManager.ProfilesUpdated += (_, _) => UpdateScanButton();
        _desktopFormProvider.DesktopForm = this;
    }

    protected virtual void CreateToolbarsAndMenus()
    {
        ToolBar = new ToolBar();
        ConfigureToolbar();

        var hiddenButtons = Config.Get(c => c.HiddenButtons);

        if (!hiddenButtons.HasFlag(ToolbarButtons.Scan))
            CreateToolbarButtonWithMenu(_scanCommand, new MenuProvider()
                .Dynamic(_scanMenuCommands)
                .Separator()
                .Append(_newProfileCommand)
                .Append(_batchScanCommand));
        if (!hiddenButtons.HasFlag(ToolbarButtons.Profiles))
            CreateToolbarButton(_profilesCommand);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Ocr))
            CreateToolbarButton(_ocrCommand);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Import))
            CreateToolbarButton(_importCommand);
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.SavePdf))
            CreateToolbarButtonWithMenu(_savePdfCommand, new MenuProvider()
                .Append(_saveAllPdfCommand)
                .Append(_saveSelectedPdfCommand)
                .Separator()
                .Append(_pdfSettingsCommand));
        if (!hiddenButtons.HasFlag(ToolbarButtons.SaveImages))
            CreateToolbarButtonWithMenu(_saveImagesCommand, new MenuProvider()
                .Append(_saveAllImagesCommand)
                .Append(_saveSelectedImagesCommand)
                .Separator()
                .Append(_imageSettingsCommand));
        if (!hiddenButtons.HasFlag(ToolbarButtons.EmailPdf))
            CreateToolbarButtonWithMenu(_emailPdfCommand, new MenuProvider()
                .Append(_emailAllPdfCommand)
                .Append(_emailSelectedPdfCommand)
                .Separator()
                .Append(_emailSettingsCommand)
                .Append(_pdfSettingsCommand));
        if (!hiddenButtons.HasFlag(ToolbarButtons.Print))
            CreateToolbarButton(_printCommand);
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.Image))
            CreateToolbarMenu(_imageMenuCommand, new MenuProvider()
                .Append(_viewImageCommand)
                .Separator()
                .Append(_cropCommand)
                .Append(_brightContCommand)
                .Append(_hueSatCommand)
                .Append(_blackWhiteCommand)
                .Append(_sharpenCommand)
                .Separator()
                .Append(_resetImageCommand));
        if (!hiddenButtons.HasFlag(ToolbarButtons.Rotate))
            CreateToolbarMenu(_rotateMenuCommand, GetRotateMenuProvider());
        if (!hiddenButtons.HasFlag(ToolbarButtons.Move))
            CreateToolbarStackedButtons(_moveUpCommand, _moveDownCommand);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Reorder))
            CreateToolbarMenu(_reorderMenuCommand, new MenuProvider()
                .Append(_interleaveCommand)
                .Append(_deinterleaveCommand)
                .Separator()
                .Append(_altInterleaveCommand)
                .Append(_altDeinterleaveCommand)
                .Separator()
                .SubMenu(_reverseMenuCommand, new MenuProvider()
                    .Append(_reverseAllCommand)
                    .Append(_reverseSelectedCommand)));
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.Delete))
            CreateToolbarButton(_deleteCommand);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Clear))
            CreateToolbarButton(_clearAllCommand);
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.Language))
            CreateToolbarMenu(_languageMenuCommand, new MenuProvider().Dynamic(_languageMenuCommands));
        if (!hiddenButtons.HasFlag(ToolbarButtons.About))
            CreateToolbarButton(_aboutCommand);
    }

    protected MenuProvider GetRotateMenuProvider() =>
        new MenuProvider()
            .Append(_rotateLeftCommand)
            .Append(_rotateRightCommand)
            .Append(_flipCommand)
            .Append(_deskewCommand)
            .Append(_customRotateCommand);

    protected virtual void AfterLayout()
    {
    }

    protected virtual void ConfigureToolbar()
    {
    }

    protected virtual void CreateToolbarButton(Command command) => throw new InvalidOperationException();

    protected virtual void CreateToolbarButtonWithMenu(Command command, MenuProvider menu) =>
        throw new InvalidOperationException();

    protected virtual void CreateToolbarMenu(Command command, MenuProvider menu) =>
        throw new InvalidOperationException();

    protected virtual void CreateToolbarStackedButtons(Command command1, Command command2) =>
        throw new InvalidOperationException();

    protected virtual void CreateToolbarSeparator() => throw new InvalidOperationException();

    protected virtual void SetContent(Control content)
    {
        Content = content;
    }

    // // protected override void OnLoad(EventArgs args) => PostInitializeComponent();
    //
    // protected override void OnLoadComplete(EventArgs args) => AfterLayout();

    // /// <summary>
    // /// Runs when the form is first loaded and every time the language is changed.
    // /// </summary>
    // private void PostInitializeComponent()
    // {
    //
    //     int thumbnailSize = Config.ThumbnailSize();
    //     _listView.ImageSize = thumbnailSize;
    //     SetThumbnailSpacing(thumbnailSize);
    //
    //     LoadToolStripLocation();
    //     InitLanguageDropdown();
    //     AssignKeyboardShortcuts();
    //     UpdateScanButton();
    //
    //     _layoutManager?.Deactivate();
    //     btnZoomIn.Location = new Point(btnZoomIn.Location.X, _listView.NativeControl.Height - 33);
    //     btnZoomOut.Location = new Point(btnZoomOut.Location.X, _listView.NativeControl.Height - 33);
    //     btnZoomMouseCatcher.Location =
    //         new Point(btnZoomMouseCatcher.Location.X, _listView.NativeControl.Height - 33);
    //     _layoutManager = new LayoutManager(this)
    //         .Bind(btnZoomIn, btnZoomOut, btnZoomMouseCatcher)
    //         .BottomTo(() => _listView.NativeControl.Height)
    //         .Activate();
    //     _listView.NativeControl.SizeChanged += (_, _) => _layoutManager.UpdateLayout();
    //
    //     _imageListSyncer = new ImageListSyncer(_imageList, _listView.ApplyDiffs, SynchronizationContext.Current!);
    //     _listView.NativeControl.Focus();
    // }
    //
    private void InitLanguageDropdown()
    {
        _languageMenuCommands.Value = _cultureHelper.GetAvailableCultures().Select(x =>
            new ActionCommand(() => SetCulture(x.langCode))
            {
                MenuText = x.langName
            }).ToImmutableList<Command>();
    }

    private void SetCulture(string cultureId)
    {
        // SaveToolStripLocation();
        // Config.User.Set(c => c.Culture, cultureId);
        // _cultureHelper.SetCulturesFromConfig();
        //
        // // Update localized values
        // // Since all forms are opened modally and this is the root form, it should be the only one that needs to be updated live
        // SaveFormState = false;
        // Controls.Clear();
        // UpdateRTL();
        // InitializeComponent();
        // PostInitializeComponent();
        // AfterLayout();
        // _notify.Rebuild();
        // Focus();
        // WindowState = FormWindowState.Normal;
        // DoRestoreFormState();
        // SaveFormState = true;
    }
    //
    // private async void FDesktop_Shown(object sender, EventArgs e)
    // {
    //     // TODO: Start the Eto application in the entry point once all forms (or at least FDesktop?) are migrated
    //     new Eto.Forms.Application(Eto.Platforms.WinForms).Attach();
    //
    //     UpdateToolbar();
    //     await _desktopController.Initialize();
    // }
    //
    // #endregion
    //
    // #region Cleanup
    //
    // private void FDesktop_Closing(object? sender, CancelEventArgs e)
    // {
    //     // if (!_desktopController.PrepareForClosing(e.CloseReason == CloseReason.UserClosing))
    //     // {
    //     //     e.Cancel = true;
    //     // }
    // }
    //
    // private void FDesktop_Closed(object sender, EventArgs e)
    // {
    //     SaveToolStripLocation();
    //     _desktopController.Cleanup();
    // }
    //
    // #endregion
    //

    #region Toolbar

    private void UpdateToolbar()
    {
        // "All" dropdown items
        _saveAllPdfCommand.MenuText = _saveAllImagesCommand.MenuText = _emailAllPdfCommand.MenuText =
            _reverseAllCommand.MenuText = string.Format(MiscResources.AllCount, _imageList.Images.Count);
        _saveAllPdfCommand.Enabled = _saveAllImagesCommand.Enabled = _emailAllPdfCommand.Enabled =
            _reverseAllCommand.Enabled = _imageList.Images.Any();

        // "Selected" dropdown items
        _saveSelectedPdfCommand.MenuText = _saveSelectedImagesCommand.MenuText = _emailSelectedPdfCommand.MenuText =
            _reverseSelectedCommand.MenuText = string.Format(MiscResources.SelectedCount, _imageList.Selection.Count);
        _saveSelectedPdfCommand.Enabled = _saveSelectedImagesCommand.Enabled = _emailSelectedPdfCommand.Enabled =
            _reverseSelectedCommand.Enabled = _imageList.Selection.Any();
        //
        // // Context-menu actions
        // ctxView.Visible = ctxCopy.Visible = ctxDelete.Visible =
        //     ctxSeparator1.Visible = ctxSeparator2.Visible = _imageList.Selection.Any();
        // ctxSelectAll.Enabled = _imageList.Images.Any();
        //
        // // Other
        // btnZoomIn.Enabled = _imageList.Images.Any() && Config.ThumbnailSize() < ThumbnailSizes.MAX_SIZE;
        // btnZoomOut.Enabled = _imageList.Images.Any() && Config.ThumbnailSize() > ThumbnailSizes.MIN_SIZE;
        _newProfileCommand.Enabled =
            !(Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked));
    }

    private void UpdateScanButton()
    {
        var defaultProfile = _profileManager.DefaultProfile;
        _scanMenuCommands.Value = _profileManager.Profiles.Select(profile =>
                new ActionCommand(() => _desktopScanController.ScanWithProfile(profile))
                {
                    MenuText = profile.DisplayName.Replace("&", "&&"),
                    Image = profile == defaultProfile ? Icons.accept_small.ToEtoImage() : null
                })
            .ToImmutableList<Command>();
    }

    #endregion

    //
    // #region Keyboard Shortcuts
    //
    // private void AssignKeyboardShortcuts()
    // {
    //     // Defaults
    //
    //     _ksm.Assign("Ctrl+Enter", tsScan);
    //     _ksm.Assign("Ctrl+B", tsBatchScan);
    //     _ksm.Assign("Ctrl+O", tsImport);
    //     _ksm.Assign("Ctrl+S", tsdSavePDF);
    //     _ksm.Assign("Ctrl+P", tsPrint);
    //     _ksm.Assign("Ctrl+Up", _imageListActions.MoveUp);
    //     _ksm.Assign("Ctrl+Left", _imageListActions.MoveUp);
    //     _ksm.Assign("Ctrl+Down", _imageListActions.MoveDown);
    //     _ksm.Assign("Ctrl+Right", _imageListActions.MoveDown);
    //     _ksm.Assign("Ctrl+Shift+Del", tsClear);
    //     _ksm.Assign("F1", _desktopSubFormController.ShowAboutForm);
    //     _ksm.Assign("Ctrl+OemMinus", btnZoomOut);
    //     _ksm.Assign("Ctrl+Oemplus", btnZoomIn);
    //     _ksm.Assign("Del", ctxDelete);
    //     _ksm.Assign("Ctrl+A", ctxSelectAll);
    //     _ksm.Assign("Ctrl+C", ctxCopy);
    //     _ksm.Assign("Ctrl+V", ctxPaste);
    //
    //     // Configured
    //
    //     var ks = Config.Get(c => c.KeyboardShortcuts);
    //
    //     _ksm.Assign(ks.About, _desktopSubFormController.ShowAboutForm);
    //     _ksm.Assign(ks.BatchScan, tsBatchScan);
    //     _ksm.Assign(ks.Clear, tsClear);
    //     _ksm.Assign(ks.Delete, tsDelete);
    //     _ksm.Assign(ks.EmailPDF, tsdEmailPDF);
    //     _ksm.Assign(ks.EmailPDFAll, tsEmailPDFAll);
    //     _ksm.Assign(ks.EmailPDFSelected, tsEmailPDFSelected);
    //     _ksm.Assign(ks.ImageBlackWhite, tsBlackWhite);
    //     _ksm.Assign(ks.ImageBrightness, tsBrightnessContrast);
    //     _ksm.Assign(ks.ImageContrast, tsBrightnessContrast);
    //     _ksm.Assign(ks.ImageCrop, tsCrop);
    //     _ksm.Assign(ks.ImageHue, tsHueSaturation);
    //     _ksm.Assign(ks.ImageSaturation, tsHueSaturation);
    //     _ksm.Assign(ks.ImageSharpen, tsSharpen);
    //     _ksm.Assign(ks.ImageReset, tsReset);
    //     _ksm.Assign(ks.ImageView, tsView);
    //     _ksm.Assign(ks.Import, tsImport);
    //     _ksm.Assign(ks.MoveDown, _imageListActions.MoveDown);
    //     _ksm.Assign(ks.MoveUp, _imageListActions.MoveUp);
    //     _ksm.Assign(ks.NewProfile, tsNewProfile);
    //     _ksm.Assign(ks.Ocr, tsOcr);
    //     _ksm.Assign(ks.Print, tsPrint);
    //     _ksm.Assign(ks.Profiles, tsProfiles);
    //
    //     _ksm.Assign(ks.ReorderAltDeinterleave, tsAltDeinterleave);
    //     _ksm.Assign(ks.ReorderAltInterleave, tsAltInterleave);
    //     _ksm.Assign(ks.ReorderDeinterleave, tsDeinterleave);
    //     _ksm.Assign(ks.ReorderInterleave, tsInterleave);
    //     _ksm.Assign(ks.ReorderReverseAll, tsReverseAll);
    //     _ksm.Assign(ks.ReorderReverseSelected, tsReverseSelected);
    //     _ksm.Assign(ks.RotateCustom, tsCustomRotation);
    //     _ksm.Assign(ks.RotateFlip, tsFlip);
    //     _ksm.Assign(ks.RotateLeft, tsRotateLeft);
    //     _ksm.Assign(ks.RotateRight, tsRotateRight);
    //     _ksm.Assign(ks.SaveImages, tsdSaveImages);
    //     _ksm.Assign(ks.SaveImagesAll, tsSaveImagesAll);
    //     _ksm.Assign(ks.SaveImagesSelected, tsSaveImagesSelected);
    //     _ksm.Assign(ks.SavePDF, tsdSavePDF);
    //     _ksm.Assign(ks.SavePDFAll, tsSavePDFAll);
    //     _ksm.Assign(ks.SavePDFSelected, tsSavePDFSelected);
    //     _ksm.Assign(ks.ScanDefault, tsScan);
    //
    //     _ksm.Assign(ks.ZoomIn, btnZoomIn);
    //     _ksm.Assign(ks.ZoomOut, btnZoomOut);
    // }
    //
    // private void AssignProfileShortcut(int i, ToolStripMenuItem item)
    // {
    //     var sh = GetProfileShortcut(i);
    //     if (string.IsNullOrWhiteSpace(sh) && i <= 11)
    //     {
    //         sh = "F" + (i + 1);
    //     }
    //     _ksm.Assign(sh, item);
    // }
    //
    // private string? GetProfileShortcut(int i)
    // {
    //     // TODO: Granular
    //     var ks = Config.Get(c => c.KeyboardShortcuts);
    //     switch (i)
    //     {
    //         case 1:
    //             return ks.ScanProfile1;
    //         case 2:
    //             return ks.ScanProfile2;
    //         case 3:
    //             return ks.ScanProfile3;
    //         case 4:
    //             return ks.ScanProfile4;
    //         case 5:
    //             return ks.ScanProfile5;
    //         case 6:
    //             return ks.ScanProfile6;
    //         case 7:
    //             return ks.ScanProfile7;
    //         case 8:
    //             return ks.ScanProfile8;
    //         case 9:
    //             return ks.ScanProfile9;
    //         case 10:
    //             return ks.ScanProfile10;
    //         case 11:
    //             return ks.ScanProfile11;
    //         case 12:
    //             return ks.ScanProfile12;
    //     }
    //     return null;
    // }
    //
    // private void ListViewKeyDown(object? sender, KeyEventArgs e)
    // {
    //     e.Handled = _ksm.Perform(e.KeyData);
    // }
    //
    // private void ListViewMouseWheel(object? sender, MouseEventArgs e)
    // {
    //     if (ModifierKeys.HasFlag(Keys.Control))
    //     {
    //         StepThumbnailSize(e.Delta / (double) SystemInformation.MouseWheelScrollDelta);
    //     }
    // }
    //
    // #endregion
    //
    //

    private async void SavePdf()
    {
        var action = Config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            // TODO
            // tsdSavePDF.ShowDropDown();
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

    private async void SaveImages()
    {
        var action = Config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            // TODO
            // tsdSaveImages.ShowDropDown();
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

    private async void EmailPdf()
    {
        var action = Config.Get(c => c.SaveButtonDefaultAction);

        if (action == SaveButtonDefaultAction.AlwaysPrompt
            || action == SaveButtonDefaultAction.PromptIfSelected && _imageList.Selection.Any())
        {
            // TODO
            // tsdEmailPDF.ShowDropDown();
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

    // #region Context Menu
    //
    // private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    // {
    //     ctxPaste.Enabled = _imageTransfer.IsInClipboard();
    //     if (!_imageList.Images.Any() && !ctxPaste.Enabled)
    //     {
    //         e.Cancel = true;
    //     }
    // }
    //
    // private void ctxSelectAll_Click(object sender, EventArgs e) => _imageListActions.SelectAll();
    // private void ctxView_Click(object sender, EventArgs e) => _desktopSubFormController.ShowViewerForm();
    // private void ctxDelete_Click(object sender, EventArgs e) => _desktopController.Delete();
    //
    // private async void ctxCopy_Click(object sender, EventArgs e) => await _desktopController.Copy();
    //
    // private void ctxPaste_Click(object sender, EventArgs e) => _desktopController.Paste();
    //
    // #endregion
    //
    // #region Thumbnail Resizing
    //
    // private void StepThumbnailSize(double step)
    // {
    //     int thumbnailSize = Config.ThumbnailSize();
    //     thumbnailSize =
    //         (int) ThumbnailSizes.StepNumberToSize(ThumbnailSizes.SizeToStepNumber(thumbnailSize) + step);
    //     thumbnailSize = ThumbnailSizes.Validate(thumbnailSize);
    //     Config.User.Set(c => c.ThumbnailSize, thumbnailSize);
    //     ResizeThumbnails(thumbnailSize);
    // }
    //
    // private void ResizeThumbnails(int thumbnailSize)
    // {
    //     if (!_imageList.Images.Any())
    //     {
    //         // Can't show visual feedback so don't do anything
    //         // TODO: This is wrong?
    //         return;
    //     }
    //     if (_listView.ImageSize == thumbnailSize)
    //     {
    //         // Same size so no resizing needed
    //         return;
    //     }
    //
    //     // Adjust the visible thumbnail display with the new size
    //     _listView.ImageSize = thumbnailSize;
    //     _listView.RegenerateImages();
    //
    //     SetThumbnailSpacing(thumbnailSize);
    //     UpdateToolbar(); // TODO: Do we need this?
    //
    //     // Render high-quality thumbnails at the new size in a background task
    //     // The existing (poorly scaled) thumbnails are used in the meantime
    //     _thumbnailRenderQueue.SetThumbnailSize(thumbnailSize);
    // }
    //
    // private void SetThumbnailSpacing(int thumbnailSize)
    // {
    //     _listView.NativeControl.Padding = new Padding(0, 20, 0, 0);
    //     const int MIN_PADDING = 6;
    //     const int MAX_PADDING = 66;
    //     // Linearly scale the padding with the thumbnail size
    //     int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) /
    //         (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
    //     int spacing = thumbnailSize + padding * 2;
    //     WinFormsHacks.SetListSpacing(_listView.NativeControl, spacing, spacing);
    // }
    //
    // private void btnZoomOut_Click(object sender, EventArgs e) => StepThumbnailSize(-1);
    // private void btnZoomIn_Click(object sender, EventArgs e) => StepThumbnailSize(1);
    //
    // #endregion
    //

    #region Drag/Drop

    private void ListViewItemClicked(object? sender, EventArgs e) => _desktopSubFormController.ShowViewerForm();

    private void ListViewSelectionChanged(object? sender, EventArgs e)
    {
        _imageList.UpdateSelection(_listView.Selection);
        UpdateToolbar();
    }

    private void ListViewDrop(object? sender, DropEventArgs args)
    {
        if (_imageTransfer.IsIn(args.Data))
        {
            var data = _imageTransfer.GetFrom(args.Data);
            if (data.ProcessId == Process.GetCurrentProcess().Id)
            {
                DragMoveImages(args.Position);
            }
            else
            {
                _desktopController.ImportDirect(data, false);
            }
        }
        else if (args.Data.Contains("FileDrop"))
        {
            // TODO: Is this xplat-compatible?
            var data = args.Data.GetObject<string[]>("FileDrop");
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