using System.Collections.Immutable;
using System.Threading;
using Eto.Forms;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.Ui;

public abstract class DesktopForm : EtoFormBase
{
    // private readonly KeyboardShortcutManager _ksm;
    private readonly INotificationManager _notify;
    private readonly CultureHelper _cultureHelper;
    private readonly IProfileManager _profileManager;
    private readonly ImageTransfer _imageTransfer;
    protected readonly ThumbnailController _thumbnailController;
    private readonly UiThumbnailProvider _thumbnailProvider;
    private readonly DesktopController _desktopController;
    private readonly IDesktopScanController _desktopScanController;
    private readonly ImageListActions _imageListActions;
    private readonly DesktopFormProvider _desktopFormProvider;
    private readonly IDesktopSubFormController _desktopSubFormController;

    private readonly ListProvider<Command> _scanMenuCommands = new();
    private readonly ListProvider<Command> _languageMenuCommands = new();

    protected IListView<UiImage> _listView;
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
        ThumbnailController thumbnailController,
        UiThumbnailProvider thumbnailProvider,
        DesktopController desktopController,
        IDesktopScanController desktopScanController,
        ImageListActions imageListActions,
        DesktopFormProvider desktopFormProvider,
        IDesktopSubFormController desktopSubFormController,
        DesktopCommands commands) : base(config)
    {
        // _ksm = ksm;
        _notify = notify;
        _cultureHelper = cultureHelper;
        _profileManager = profileManager;
        ImageList = imageList;
        _imageTransfer = imageTransfer;
        _thumbnailController = thumbnailController;
        _thumbnailProvider = thumbnailProvider;
        _desktopController = desktopController;
        _desktopScanController = desktopScanController;
        _imageListActions = imageListActions;
        _desktopFormProvider = desktopFormProvider;
        _desktopSubFormController = desktopSubFormController;
        Commands = commands;

        // PostInitializeComponent();
        //
        Icon = Icons.favicon.ToEtoIcon();
        CreateToolbarsAndMenus();
        UpdateScanButton();
        InitLanguageDropdown();

        _listView = EtoPlatform.Current.CreateListView(new ImageListViewBehavior(_thumbnailProvider, _imageTransfer));
        _listView.AllowDrag = true;
        _listView.AllowDrop = true;
        _listView.Selection = ImageList.Selection;
        _listView.ItemClicked += ListViewItemClicked;
        _listView.Drop += ListViewDrop;
        _listView.SelectionChanged += ListViewSelectionChanged;
        _imageListSyncer?.Dispose();

        SetContent(_listView.Control);
        AfterLayout();

        //
        // Shown += FDesktop_Shown;
        // Closing += FDesktop_Closing;
        // Closed += FDesktop_Closed;
        imageList.SelectionChanged += (_, _) =>
        {
            Invoker.Current.SafeInvoke(() =>
            {
                UpdateToolbar();
                _listView!.Selection = ImageList.Selection;
            });
        };
        ImageList.ImagesUpdated += (_, _) => Invoker.Current.SafeInvoke(UpdateToolbar);
        _profileManager.ProfilesUpdated += (_, _) => UpdateScanButton();
        _desktopFormProvider.DesktopForm = this;
        _thumbnailController.ListView = _listView;
        _thumbnailController.ThumbnailSizeChanged += (_, _) =>
        {
            SetThumbnailSpacing(_thumbnailController.VisibleSize);
            UpdateToolbar();
        };
    }

    protected UiImageList ImageList { get; }
    protected DesktopCommands Commands { get; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _imageListSyncer = new ImageListSyncer(ImageList, _listView.ApplyDiffs, SynchronizationContext.Current!);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        UpdateToolbar();
        _desktopController.Initialize();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _desktopController.Cleanup();
    }

    protected virtual void CreateToolbarsAndMenus()
    {
        ToolBar = new ToolBar();
        ConfigureToolbar();

        var hiddenButtons = Config.Get(c => c.HiddenButtons);

        if (!hiddenButtons.HasFlag(ToolbarButtons.Scan))
            CreateToolbarButtonWithMenu(Commands.Scan, new MenuProvider()
                .Dynamic(_scanMenuCommands)
                .Separator()
                .Append(Commands.NewProfile)
                .Append(Commands.BatchScan));
        if (!hiddenButtons.HasFlag(ToolbarButtons.Profiles))
            CreateToolbarButton(Commands.Profiles);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Ocr))
            CreateToolbarButton(Commands.Ocr);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Import))
            CreateToolbarButton(Commands.Import);
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.SavePdf))
            CreateToolbarButtonWithMenu(Commands.SavePdf, new MenuProvider()
                .Append(Commands.SaveAllPdf)
                .Append(Commands.SaveSelectedPdf)
                .Separator()
                .Append(Commands.PdfSettings));
        if (!hiddenButtons.HasFlag(ToolbarButtons.SaveImages))
            CreateToolbarButtonWithMenu(Commands.SaveImages, new MenuProvider()
                .Append(Commands.SaveAllImages)
                .Append(Commands.SaveSelectedImages)
                .Separator()
                .Append(Commands.ImageSettings));
        if (!hiddenButtons.HasFlag(ToolbarButtons.EmailPdf))
            CreateToolbarButtonWithMenu(Commands.EmailPdf, new MenuProvider()
                .Append(Commands.EmailAllPdf)
                .Append(Commands.EmailSelectedPdf)
                .Separator()
                .Append(Commands.EmailSettings)
                .Append(Commands.PdfSettings));
        if (!hiddenButtons.HasFlag(ToolbarButtons.Print))
            CreateToolbarButton(Commands.Print);
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.Image))
            CreateToolbarMenu(Commands.ImageMenu, new MenuProvider()
                .Append(Commands.ViewImage)
                .Separator()
                .Append(Commands.Crop)
                .Append(Commands.BrightCont)
                .Append(Commands.HueSat)
                .Append(Commands.BlackWhite)
                .Append(Commands.Sharpen)
                .Append(Commands.DocumentCorrection)
                .Separator()
                .Append(Commands.ResetImage));
        if (!hiddenButtons.HasFlag(ToolbarButtons.Rotate))
            CreateToolbarMenu(Commands.RotateMenu, GetRotateMenuProvider());
        if (!hiddenButtons.HasFlag(ToolbarButtons.Move))
            CreateToolbarStackedButtons(Commands.MoveUp, Commands.MoveDown);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Reorder))
            CreateToolbarMenu(Commands.ReorderMenu, new MenuProvider()
                .Append(Commands.Interleave)
                .Append(Commands.Deinterleave)
                .Separator()
                .Append(Commands.AltInterleave)
                .Append(Commands.AltDeinterleave)
                .Separator()
                .SubMenu(Commands.ReverseMenu, new MenuProvider()
                    .Append(Commands.ReverseAll)
                    .Append(Commands.ReverseSelected)));
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.Delete))
            CreateToolbarButton(Commands.Delete);
        if (!hiddenButtons.HasFlag(ToolbarButtons.Clear))
            CreateToolbarButton(Commands.ClearAll);
        CreateToolbarSeparator();
        if (!hiddenButtons.HasFlag(ToolbarButtons.Language))
            CreateToolbarMenu(Commands.LanguageMenu, GetLanguageMenuProvider());
        if (!hiddenButtons.HasFlag(ToolbarButtons.About))
            CreateToolbarButton(Commands.About);
    }

    protected MenuProvider GetRotateMenuProvider() =>
        new MenuProvider()
            .Append(Commands.RotateLeft)
            .Append(Commands.RotateRight)
            .Append(Commands.Flip)
            .Append(Commands.Deskew)
            .Append(Commands.CustomRotate);

    protected MenuProvider GetLanguageMenuProvider()
    {
        return new MenuProvider().Dynamic(_languageMenuCommands);
    }

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

    // TODO: Can we generalize this kind of logic?
    protected SubMenuItem CreateSubMenu(Command menuCommand, MenuProvider menuProvider)
    {
        var menuItem = new SubMenuItem
        {
            Text = menuCommand.MenuText,
            Image = menuCommand.Image
        };
        menuProvider.Handle(subItems =>
        {
            menuItem.Items.Clear();
            foreach (var subItem in subItems)
            {
                switch (subItem)
                {
                    case MenuProvider.CommandItem { Command: var command }:
                        menuItem.Items.Add(new ButtonMenuItem(command));
                        break;
                    case MenuProvider.SeparatorItem:
                        menuItem.Items.Add(new SeparatorMenuItem());
                        break;
                    case MenuProvider.SubMenuItem:
                        throw new NotImplementedException();
                }
            }
        });
        return menuItem;
    }

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

    protected virtual void UpdateToolbar()
    {
        // Top-level toolbar items
        Commands.ImageMenu.Enabled =
            Commands.RotateMenu.Enabled = Commands.MoveUp.Enabled = Commands.MoveDown.Enabled =
                Commands.Delete.Enabled = ImageList.Selection.Any();
        Commands.SavePdf.Enabled = Commands.SaveImages.Enabled = Commands.ClearAll.Enabled =
            Commands.ReorderMenu.Enabled =
                Commands.EmailPdf.Enabled = Commands.Print.Enabled = ImageList.Images.Any();

        // "All" dropdown items
        Commands.SaveAllPdf.MenuText = Commands.SaveAllImages.MenuText = Commands.EmailAllPdf.MenuText =
            Commands.ReverseAll.MenuText = string.Format(MiscResources.AllCount, ImageList.Images.Count);
        Commands.SaveAllPdf.Enabled = Commands.SaveAllImages.Enabled = Commands.EmailAllPdf.Enabled =
            Commands.ReverseAll.Enabled = ImageList.Images.Any();

        // "Selected" dropdown items
        Commands.SaveSelectedPdf.MenuText = Commands.SaveSelectedImages.MenuText = Commands.EmailSelectedPdf.MenuText =
            Commands.ReverseSelected.MenuText = string.Format(MiscResources.SelectedCount, ImageList.Selection.Count);
        Commands.SaveSelectedPdf.Enabled = Commands.SaveSelectedImages.Enabled = Commands.EmailSelectedPdf.Enabled =
            Commands.ReverseSelected.Enabled = ImageList.Selection.Any();
        //
        // // Context-menu actions
        // ctxView.Visible = ctxCopy.Visible = ctxDelete.Visible =
        //     ctxSeparator1.Visible = ctxSeparator2.Visible = _imageList.Selection.Any();
        // ctxSelectAll.Enabled = _imageList.Images.Any();
        //
        // Other
        Commands.NewProfile.Enabled =
            !(Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked));
    }

    private void UpdateScanButton()
    {
        var defaultProfile = _profileManager.DefaultProfile;
        UpdateTitle(defaultProfile);
        _scanMenuCommands.Value = _profileManager.Profiles.Select(profile =>
                new ActionCommand(() => _desktopScanController.ScanWithProfile(profile))
                {
                    MenuText = profile.DisplayName.Replace("&", "&&"),
                    Image = profile == defaultProfile ? Icons.accept_small.ToEtoImage() : null
                })
            .ToImmutableList<Command>();
    }

    protected virtual void UpdateTitle(ScanProfile? defaultProfile)
    {
        Title = string.Format(UiStrings.Naps2TitleFormat, defaultProfile?.DisplayName ?? UiStrings.Naps2FullName);
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
    //

    protected virtual void SetThumbnailSpacing(int thumbnailSize)
    {
    }

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
        ImageList.UpdateSelection(_listView.Selection);
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
        if (!ImageList.Selection.Any())
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