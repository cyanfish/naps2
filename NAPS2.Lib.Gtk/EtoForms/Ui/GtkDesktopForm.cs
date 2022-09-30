using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Gtk;
using NAPS2.ImportExport.Images;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.Ui;

public class GtkDesktopForm : DesktopForm
{
    public GtkDesktopForm(
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
        DesktopCommands commands)
        : base(config, /*ksm,*/ notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailController, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController, commands)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        // TODO: What's the best place to initialize this? It needs to happen from the UI event loop.
        Invoker.Current = new SyncContextInvoker(SynchronizationContext.Current);
        base.OnLoad(e);
        ClientSize = new Size(1000, 600);
        // TODO: This is a bit of a hack as for some reason the view doesn't update unless we do this
        ((GtkListView<UiImage>)_listView).Updated += (_, _) => Content = _listView.Control;
    }

    protected override void CreateToolbarsAndMenus()
    {
        Commands.MoveDown.ToolBarText = "";
        Commands.MoveUp.ToolBarText = "";
        Commands.SaveAllPdf.Shortcut = Application.Instance.CommonModifier | Keys.S;
        Commands.SaveSelectedPdf.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S;
        Commands.SaveAllImages.Shortcut = Application.Instance.CommonModifier | Keys.M;
        Commands.SaveSelectedImages.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.M;

        Menu = new MenuBar
        {
            AboutItem = Commands.About,
            ApplicationItems =
            {
                CreateSubMenu(Commands.LanguageMenu, GetLanguageMenuProvider())
            },
            Items =
            {
                new SubMenuItem
                {
                    Text = "File",
                    Items =
                    {
                        Commands.Import,
                        new SeparatorMenuItem(),
                        Commands.SaveAllPdf,
                        Commands.SaveSelectedPdf,
                        Commands.SaveAllImages,
                        Commands.SaveSelectedImages,
                        new SeparatorMenuItem(),
                        Commands.EmailAllPdf,
                        Commands.EmailSelectedPdf,
                        Commands.Print,
                        new SeparatorMenuItem(),
                        Commands.ClearAll
                    }
                },
                new SubMenuItem
                {
                    Text = "Edit"
                },
                new SubMenuItem
                {
                    Text = "Scan",
                    Items =
                    {
                        Commands.Scan,
                        Commands.NewProfile
                    }
                },
                new SubMenuItem
                {
                    Text = "Image",
                    Items =
                    {
                        Commands.ViewImage,
                        new SeparatorMenuItem(),
                        Commands.Crop,
                        Commands.BrightCont,
                        Commands.HueSat,
                        Commands.BlackWhite,
                        Commands.Sharpen,
                        new SeparatorMenuItem(),
                        Commands.ResetImage
                    }
                },
                new SubMenuItem
                {
                    Text = "Tools",
                    Items =
                    {
                        Commands.BatchScan,
                        Commands.Ocr
                    }
                }
            }
        };
        //
        // var toolbar = new NSToolbar("naps2.desktop.toolbar");
        // toolbar.Delegate = new ToolbarDelegate(this);
        // toolbar.AllowsUserCustomization = true;
        // // toolbar.AutosavesConfiguration = true;
        // toolbar.DisplayMode = NSToolbarDisplayMode.Icon;
        //
        // var window = this.ToNative();
        // window.Toolbar = toolbar;
        // window.ToolbarStyle = NSWindowToolbarStyle.Unified;
        // // TODO: Subtitle based on active profile?
        // window.Subtitle = "Not Another PDF Scanner";
        // // TODO: Do we want full size content?
        // window.StyleMask |= NSWindowStyle.FullSizeContentView;
        // window.StyleMask |= NSWindowStyle.UnifiedTitleAndToolbar;
    }
}