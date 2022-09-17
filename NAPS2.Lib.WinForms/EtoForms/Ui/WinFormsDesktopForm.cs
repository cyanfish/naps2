using System.Drawing;
using Eto.Forms;
using Eto.WinForms;
using Eto.WinForms.Forms.ToolBar;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport.Images;
using NAPS2.WinForms;
using wf = System.Windows.Forms;

namespace NAPS2.EtoForms.Ui;

public class WinFormsDesktopForm : DesktopForm
{
    private readonly ToolbarFormatter _toolbarFormatter = new(new StringWrapper());
    private readonly wf.Form _form;
    private wf.ToolStrip _toolStrip = null!;
    private wf.ToolStripContainer _container = null!;
    private LayoutManager _layoutManager;
    private wf.Button btnZoomIn, btnZoomOut, btnZoomMouseCatcher;

    public WinFormsDesktopForm(
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
        DesktopSubFormController desktopSubFormController)
        : base(config, /*ksm,*/ notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailController, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController)
    {
        _form = this.ToNative();
        _form.ClientSize = new Size(1204, 526);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ConfigureZoomButtons();

        NativeListView.TabIndex = 7;
        NativeListView.Dock = wf.DockStyle.Fill;
        // NativeListView.ContextMenuStrip = contextMenuStrip;
        // NativeListView.KeyDown += ListViewKeyDown;
        // NativeListView.MouseWheel += ListViewMouseWheel;
        NativeListView.SizeChanged += (_, _) => _layoutManager.UpdateLayout();
        NativeListView.Focus();
    }

    private void ConfigureZoomButtons()
    {
        _layoutManager?.Deactivate();
        btnZoomIn = new wf.Button
        {
            Image = Icons.zoom_in.ToBitmap(),
            AccessibleName = UiStrings.ZoomIn,
            BackColor = Color.White,
            Size = new Size(23, 23),
            FlatStyle = wf.FlatStyle.Flat,
            TabIndex = 8
        };
        btnZoomIn.Click += (_, _) => StepThumbnailSize(1);
        _container.ContentPanel.Controls.Add(btnZoomIn);
        btnZoomOut = new wf.Button
        {
            Image = Icons.zoom_out.ToBitmap(),
            AccessibleName = UiStrings.ZoomOut,
            BackColor = Color.White,
            Size = new Size(23, 23),
            FlatStyle = wf.FlatStyle.Flat,
            TabIndex = 9
        };
        btnZoomOut.Click += (_, _) => StepThumbnailSize(-1);
        _container.ContentPanel.Controls.Add(btnZoomOut);
        btnZoomMouseCatcher = new wf.Button
        {
            BackColor = Color.White,
            Size = new Size(45, 23),
            FlatStyle = wf.FlatStyle.Flat
        };
        _container.ContentPanel.Controls.Add(btnZoomMouseCatcher);
        btnZoomMouseCatcher.BringToFront();
        btnZoomIn.BringToFront();
        btnZoomOut.BringToFront();

        btnZoomIn.Location = new Point(32, NativeListView.Height - 33);
        btnZoomOut.Location = new Point(10, NativeListView.Height - 33);
        btnZoomMouseCatcher.Location = new Point(10, NativeListView.Height - 33);
        _layoutManager = new LayoutManager(_form)
            .Bind(btnZoomIn, btnZoomOut, btnZoomMouseCatcher)
            .BottomTo(() => NativeListView.Height)
            .Activate();
    }

    private void StepThumbnailSize(double step)
    {
        int thumbnailSize = _thumbnailController.VisibleSize;
        thumbnailSize =
            (int) ThumbnailSizes.StepNumberToSize(ThumbnailSizes.SizeToStepNumber(thumbnailSize) + step);
        _thumbnailController.VisibleSize = thumbnailSize;
    }

    protected override void UpdateToolbar()
    {
        base.UpdateToolbar();
        btnZoomIn.Enabled = ImageList.Images.Any() && _thumbnailController.VisibleSize < ThumbnailSizes.MAX_SIZE;
        btnZoomOut.Enabled = ImageList.Images.Any() && _thumbnailController.VisibleSize > ThumbnailSizes.MIN_SIZE;
    }

    private wf.ListView NativeListView => ((WinFormsListView<UiImage>) _listView).NativeControl;

    protected override void ConfigureToolbar()
    {
        _toolStrip = ((ToolBarHandler) ToolBar.Handler).Control;
        _toolStrip.ShowItemToolTips = false;
        _toolStrip.TabStop = true;
        _toolStrip.ImageScalingSize = new Size(32, 32);
        _toolStrip.ParentChanged += (_, _) => _toolbarFormatter.RelayoutToolbar(_toolStrip);
    }

    protected override void SetContent(Control content)
    {
        _container = new wf.ToolStripContainer();
        _container.TopToolStripPanel.Controls.Add(_toolStrip);
        foreach (var panel in _container.Controls.OfType<wf.ToolStripPanel>())
        {
            // Allow tabbing through the toolbar for accessibility
            WinFormsHacks.SetControlStyle(panel, wf.ControlStyles.Selectable, true);
        }

        var wfContent = content.ToNative();
        wfContent.Dock = wf.DockStyle.Fill;
        _container.ContentPanel.Controls.Add(wfContent);

        Content = _container.ToEto();
    }

    protected override void AfterLayout()
    {
        _toolbarFormatter.RelayoutToolbar(_toolStrip);
    }

    protected override void SetThumbnailSpacing(int thumbnailSize)
    {
        NativeListView.Padding = new wf.Padding(0, 20, 0, 0);
        const int MIN_PADDING = 6;
        const int MAX_PADDING = 66;
        // Linearly scale the padding with the thumbnail size
        int padding = MIN_PADDING + (MAX_PADDING - MIN_PADDING) * (thumbnailSize - ThumbnailSizes.MIN_SIZE) /
            (ThumbnailSizes.MAX_SIZE - ThumbnailSizes.MIN_SIZE);
        int spacing = thumbnailSize + padding * 2;
        WinFormsHacks.SetListSpacing(NativeListView, spacing, spacing);
    }

    protected override void CreateToolbarButton(Command command)
    {
        var item = new wf.ToolStripButton
        {
            TextImageRelation = wf.TextImageRelation.ImageAboveText
        };
        ApplyCommand(item, command);
        _toolStrip.Items.Add(item);
    }

    protected override void CreateToolbarButtonWithMenu(Command command, MenuProvider menu)
    {
        var item = new wf.ToolStripSplitButton
        {
            TextImageRelation = wf.TextImageRelation.ImageAboveText
        };
        ApplyCommand(item, command);
        _toolStrip.Items.Add(item);
        menu.Handle(subItems => SetUpMenu(item, subItems));
    }

    private void SetUpMenu(wf.ToolStripDropDownItem item, List<MenuProvider.Item> subItems)
    {
        item.DropDownItems.Clear();
        foreach (var subItem in subItems)
        {
            switch (subItem)
            {
                case MenuProvider.SeparatorItem:
                    item.DropDownItems.Add(new wf.ToolStripSeparator());
                    break;
                case MenuProvider.CommandItem commandSubItem:
                    item.DropDownItems.Add(ApplyCommand(new wf.ToolStripMenuItem
                    {
                        ImageScaling = wf.ToolStripItemImageScaling.None
                    }, commandSubItem.Command));
                    break;
                case MenuProvider.SubMenuItem subMenuSubItem:
                    var subMenu = new wf.ToolStripMenuItem();
                    ApplyCommand(subMenu, subMenuSubItem.Command);
                    // TODO: If submenus are dynamic this will memory leak or something
                    subMenuSubItem.MenuProvider.Handle(subSubItems => SetUpMenu(subMenu, subSubItems));
                    item.DropDownItems.Add(subMenu);
                    break;
            }
        }
    }

    protected override void CreateToolbarMenu(Command command, MenuProvider menu)
    {
        var item = new wf.ToolStripDropDownButton
        {
            TextImageRelation = wf.TextImageRelation.ImageAboveText,
            ShowDropDownArrow = false
        };
        ApplyCommand(item, command);
        _toolStrip.Items.Add(item);
        menu.Handle(subItems => SetUpMenu(item, subItems));
    }

    protected override void CreateToolbarStackedButtons(Command command1, Command command2)
    {
        var item = new ToolStripDoubleButton
        {
            FirstImage = command1.Image.ToSD(),
            FirstText = command1.ToolBarText,
            SecondImage = command2.Image.ToSD(),
            SecondText = command2.ToolBarText
        };
        command1.EnabledChanged += (_, _) => item.Enabled = command1.Enabled;
        item.FirstClick += (_, _) => command1.Execute();
        item.SecondClick += (_, _) => command2.Execute();
        _toolStrip.Items.Add(item);
    }

    private wf.ToolStripItem ApplyCommand(wf.ToolStripItem item, Command command)
    {
        item.Image = command.Image.ToSD();
        item.Text = item is wf.ToolStripMenuItem ? command.MenuText : command.ToolBarText;
        command.EnabledChanged += (_, _) => item.Enabled = command.Enabled;
        if (item is wf.ToolStripSplitButton button)
        {
            button.ButtonClick += (_, _) => command.Execute();
        }
        else
        {
            item.Click += (_, _) => command.Execute();
        }
        return item;
    }

    protected override void CreateToolbarSeparator()
    {
        _toolStrip.Items.Add(new wf.ToolStripSeparator());
    }


    // TODO: Call these

    private void SaveToolStripLocation()
    {
        Config.User.Set(c => c.DesktopToolStripDock, _toolStrip.Parent.Dock.ToConfig());
    }

    private void LoadToolStripLocation()
    {
        var dock = Config.Get(c => c.DesktopToolStripDock).ToWinForms();
        if (dock != wf.DockStyle.None)
        {
            var panel = _container.Controls.OfType<wf.ToolStripPanel>().FirstOrDefault(x => x.Dock == dock);
            if (panel != null)
            {
                _toolStrip.Parent = panel;
            }
        }
        _toolStrip.Parent.TabStop = true;
    }
}