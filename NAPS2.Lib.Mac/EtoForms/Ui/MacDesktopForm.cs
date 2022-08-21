using NAPS2.Config;
using NAPS2.Images;
using NAPS2.ImportExport.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.EtoForms.Ui;

public class MacDesktopForm : DesktopForm
{
    public MacDesktopForm(
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
        IDesktopSubFormController desktopSubFormController)
        : base(config, /*ksm,*/ notify, cultureHelper, profileManager,
            imageList, imageTransfer, thumbnailRenderQueue, thumbnailProvider, desktopController, desktopScanController,
            imageListActions, desktopFormProvider, desktopSubFormController)
    {
    }

    protected override void CreateToolbarsAndMenus()
    {
    }

    protected override void ConfigureToolbar()
    {
    }

    protected override void AfterLayout()
    {
    }
}