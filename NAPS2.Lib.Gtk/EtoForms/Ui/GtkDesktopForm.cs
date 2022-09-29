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
        IDesktopSubFormController desktopSubFormController)
        : base(config, /*ksm,*/ notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailController, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController)
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
        _moveDownCommand.ToolBarText = "";
        _moveUpCommand.ToolBarText = "";
        _saveAllPdfCommand.Shortcut = Application.Instance.CommonModifier | Keys.S;
        _saveSelectedPdfCommand.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S;
        _saveAllImagesCommand.Shortcut = Application.Instance.CommonModifier | Keys.M;
        _saveSelectedImagesCommand.Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.M;

        Menu = new MenuBar
        {
            AboutItem = _aboutCommand,
            ApplicationItems =
            {
                CreateSubMenu(_languageMenuCommand, GetLanguageMenuProvider())
            },
            Items =
            {
                new SubMenuItem
                {
                    Text = "File",
                    Items =
                    {
                        _importCommand,
                        new SeparatorMenuItem(),
                        _saveAllPdfCommand,
                        _saveSelectedPdfCommand,
                        _saveAllImagesCommand,
                        _saveSelectedImagesCommand,
                        new SeparatorMenuItem(),
                        _emailAllPdfCommand,
                        _emailSelectedPdfCommand,
                        _printCommand,
                        new SeparatorMenuItem(),
                        _clearAllCommand
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
                        _scanCommand,
                        _newProfileCommand
                    }
                },
                new SubMenuItem
                {
                    Text = "Image",
                    Items =
                    {
                        _viewImageCommand,
                        new SeparatorMenuItem(),
                        _cropCommand,
                        _brightContCommand,
                        _hueSatCommand,
                        _blackWhiteCommand,
                        _sharpenCommand,
                        new SeparatorMenuItem(),
                        _resetImageCommand
                    }
                },
                new SubMenuItem
                {
                    Text = "Tools",
                    Items =
                    {
                        _batchScanCommand,
                        _ocrCommand
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