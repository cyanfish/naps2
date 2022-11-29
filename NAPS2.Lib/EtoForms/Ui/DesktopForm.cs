using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public abstract class DesktopForm : EtoFormBase
{
    private readonly DesktopKeyboardShortcuts _keyboardShortcuts;
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
    private readonly ContextMenu _contextMenu = new();

    protected IListView<UiImage> _listView;
    private ImageListSyncer? _imageListSyncer;

    public DesktopForm(
        Naps2Config config,
        DesktopKeyboardShortcuts keyboardShortcuts,
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
        _keyboardShortcuts = keyboardShortcuts;
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
        _listView.ImageSize = _thumbnailController.VisibleSize;
        _listView.ContextMenu = _contextMenu;

        // TODO: Fix Eto so that we don't need to set an item here (otherwise the first time we right click nothing happens)
        _contextMenu.Items.Add(Commands.SelectAll);
        _contextMenu.Opening += OpeningContextMenu;
        _keyboardShortcuts.Assign(Commands);
        KeyDown += OnKeyDown;

        LayoutController.RootPadding = 0;
        FormStateController.AutoLayoutSize = false;
        FormStateController.DefaultClientSize = new Size(1210, 600);
        LayoutController.Content = L.Overlay(
            GetMainContent(),
            L.Column(
                C.Filler(),
                L.Row(GetZoomButtons(), C.Filler())
            ).Padding(10)
        );

        //
        // Shown += FDesktop_Shown;
        // Closing += FDesktop_Closing;
        // Closed += FDesktop_Closed;
        _desktopFormProvider.DesktopForm = this;
        _thumbnailController.ListView = _listView;
        _thumbnailController.ThumbnailSizeChanged += ThumbnailController_ThumbnailSizeChanged;
        SetThumbnailSpacing(_thumbnailController.VisibleSize);
        ImageList.SelectionChanged += ImageList_SelectionChanged;
        ImageList.ImagesUpdated += ImageList_ImagesUpdated;
        _profileManager.ProfilesUpdated += ProfileManager_ProfilesUpdated;
    }

    private void OpeningContextMenu(object sender, EventArgs e)
    {
        _contextMenu.Items.Clear();
        Commands.Paste.Enabled = _imageTransfer.IsInClipboard();
        if (ImageList.Selection.Any())
        {
            // TODO: Remove icon from delete command somehow
            // TODO: Is this memory leaking (because of event handlers) when commands are converted to menuitems?
            _contextMenu.Items.AddRange(new List<MenuItem>
            {
                Commands.ViewImage,
                new SeparatorMenuItem(),
                Commands.SelectAll,
                Commands.Copy,
                Commands.Paste,
                new SeparatorMenuItem(),
                Commands.Delete
            });
        }
        else
        {
            _contextMenu.Items.AddRange(new List<MenuItem>
            {
                Commands.SelectAll,
                Commands.Paste
            });
        }
    }

    private void ImageList_SelectionChanged(object? sender, EventArgs e)
    {
        Invoker.Current.SafeInvoke(() =>
        {
            UpdateToolbar();
            _listView!.Selection = ImageList.Selection;
        });
    }

    private void ImageList_ImagesUpdated(object? sender, ImageListEventArgs e)
    {
        Invoker.Current.SafeInvoke(UpdateToolbar);
    }

    private void ProfileManager_ProfilesUpdated(object? sender, EventArgs e)
    {
        UpdateScanButton();
    }

    private void ThumbnailController_ThumbnailSizeChanged(object? sender, EventArgs e)
    {
        SetThumbnailSpacing(_thumbnailController.VisibleSize);
        UpdateToolbar();
    }

    protected UiImageList ImageList { get; }
    protected DesktopCommands Commands { get; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _imageListSyncer = new ImageListSyncer(ImageList, _listView.ApplyDiffs, SynchronizationContext.Current!);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        UpdateToolbar();
        await _desktopController.Initialize();
    }

    // protected override void OnClosing(CancelEventArgs e)
    // {
    //     if (!_desktopController.PrepareForClosing(e.CloseReason == CloseReason.UserClosing))
    //     {
    //         e.Cancel = true;
    //     }
    // }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _desktopController.Cleanup();

        // TODO: Make sure we don't have any remaining memory leaks (toolbars? commands?)
        _thumbnailController.ThumbnailSizeChanged -= ThumbnailController_ThumbnailSizeChanged;
        ImageList.SelectionChanged -= ImageList_SelectionChanged;
        ImageList.ImagesUpdated -= ImageList_ImagesUpdated;
        _profileManager.ProfilesUpdated -= ProfileManager_ProfilesUpdated;
        _imageListSyncer?.Dispose();
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

    protected virtual LayoutElement GetMainContent() => _listView.Control;

    protected virtual LayoutElement GetZoomButtons()
    {
        var zoomIn = C.ImageButton(Commands.ZoomIn);
        EtoPlatform.Current.ConfigureZoomButton(zoomIn);
        var zoomOut = C.ImageButton(Commands.ZoomOut);
        EtoPlatform.Current.ConfigureZoomButton(zoomOut);
        return L.Row(zoomOut, zoomIn).Spacing(-1);
    }

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
        _desktopController.Suspend();
        try
        {
            Config.User.Set(c => c.Culture, cultureId);
            _cultureHelper.SetCulturesFromConfig();
            FormStateController.DoSaveFormState();
            var newDesktop = FormFactory.Create<DesktopForm>();
            newDesktop.Show();
            SetMainForm(newDesktop);
            Close();
        }
        finally
        {
            _desktopController.Resume();
        }
        // TODO: If we make any other forms non-modal, we will need to refresh them too
    }

    protected virtual void SetMainForm(Form newMainForm)
    {
        Application.Instance.MainForm = newMainForm;
    }

    protected virtual void UpdateToolbar()
    {
        // Top-level toolbar items
        Commands.ImageMenu.Enabled =
            Commands.RotateMenu.Enabled = Commands.MoveUp.Enabled = Commands.MoveDown.Enabled =
                Commands.Delete.Enabled = ImageList.Selection.Any();
        Commands.SavePdf.Enabled = Commands.SaveImages.Enabled = Commands.ClearAll.Enabled =
            Commands.ReorderMenu.Enabled =
                Commands.EmailPdf.Enabled = Commands.Print.Enabled = ImageList.Images.Any();

        // TODO: Changing the text on the command doesn't actually propagate to the widget
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

        // Other
        Commands.SelectAll.Enabled = ImageList.Images.Any();
        Commands.ZoomIn.Enabled = ImageList.Images.Any() && _thumbnailController.VisibleSize < ThumbnailSizes.MAX_SIZE;
        Commands.ZoomOut.Enabled = ImageList.Images.Any() && _thumbnailController.VisibleSize > ThumbnailSizes.MIN_SIZE;
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
        for (int i = 0; i < _scanMenuCommands.Value.Count; i++)
        {
            _keyboardShortcuts.AssignProfileShortcut(i, _scanMenuCommands.Value[i]);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = _keyboardShortcuts.Perform(e.KeyData);
    }

    protected virtual void UpdateTitle(ScanProfile? defaultProfile)
    {
        Title = string.Format(UiStrings.Naps2TitleFormat, defaultProfile?.DisplayName ?? UiStrings.Naps2FullName);
    }

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

    protected virtual void SetThumbnailSpacing(int thumbnailSize)
    {
    }

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
}