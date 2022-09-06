using NAPS2.EtoForms;
using NAPS2.EtoForms.Mac;
using NAPS2.EtoForms.Ui;
using NAPS2.Images.Mac;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Update;
using NAPS2.WinForms;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class MacModule : NinjectModule
{
    public override void Load()
    {
        // Bind<IBatchScanPerformer>().To<BatchScanPerformer>();
        Bind<IPdfPasswordProvider>().To<StubPdfPasswordProvider>();
        Bind<ErrorOutput>().To<StubErrorOutput>();
        Bind<IOverwritePrompt>().To<StubOverwritePrompt>();
        Bind<OperationProgress>().To<EtoOperationProgress>().InSingletonScope();
        Bind<DialogHelper>().To<StubDialogHelper>();
        Bind<INotificationManager>().To<StubNotificationManager>().InSingletonScope();
        Bind<ISaveNotify>().ToMethod(ctx => ctx.Kernel.Get<INotificationManager>());
        Bind<IScannedImagePrinter>().To<StubScannedImagePrinter>();
        Bind<DesktopController>().ToSelf().InSingletonScope();
        Bind<IUpdateChecker>().To<UpdateChecker>();
        Bind<IWinFormsExportHelper>().To<StubExportHelper>();
        Bind<IDesktopScanController>().To<StubDesktopScanController>();
        Bind<IDesktopSubFormController>().To<StubDesktopSubFormController>();
        Bind<DesktopFormProvider>().ToSelf().InSingletonScope();
        Bind<ImageContext>().To<MacImageContext>();
        Bind<MacImageContext>().ToSelf();

        Bind<DesktopForm>().To<MacDesktopForm>();

        EtoPlatform.Current = new MacEtoPlatform();
        // Log.EventLogger = new WindowsEventLogger(Kernel!.Get<Naps2Config>());
    }
}

public class StubDesktopSubFormController : IDesktopSubFormController
{
    private IFormFactory _formFactory;
    private DesktopImagesController _desktopImagesController;

    public StubDesktopSubFormController(IFormFactory formFactory, DesktopImagesController desktopImagesController)
    {
        _formFactory = formFactory;
        _desktopImagesController = desktopImagesController;
    }

    public void ShowCropForm()
    {
    }

    public void ShowBrightnessContrastForm()
    {
    }

    public void ShowHueSaturationForm()
    {
    }

    public void ShowBlackWhiteForm()
    {
    }

    public void ShowSharpenForm()
    {
    }

    public void ShowRotateForm()
    {
    }

    public void ShowProfilesForm()
    {
        var form = _formFactory.Create<ProfilesForm>();
        form.ImageCallback = _desktopImagesController.ReceiveScannedImage();
        form.ShowModal();
    }

    public void ShowOcrForm()
    {
    }

    public void ShowBatchScanForm()
    {
    }

    public void ShowViewerForm()
    {
    }

    public void ShowPdfSettingsForm()
    {
    }

    public void ShowImageSettingsForm()
    {
    }

    public void ShowEmailSettingsForm()
    {
    }

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
    }
}

public class StubDesktopScanController : IDesktopScanController
{
    public Task ScanWithDevice(string deviceID)
    {
        return Task.CompletedTask;
    }

    public Task ScanDefault()
    {
        return Task.CompletedTask;
    }

    public Task ScanWithNewProfile()
    {
        return Task.CompletedTask;
    }

    public Task ScanWithProfile(ScanProfile profile)
    {
        return Task.CompletedTask;
    }
}

public class StubExportHelper : IWinFormsExportHelper
{
    public Task<bool> SavePDF(IList<ProcessedImage> images, ISaveNotify notify)
    {
        return Task.FromResult(false);
    }

    public Task<bool> ExportPDF(string filename, IList<ProcessedImage> images, bool email, EmailMessage emailMessage)
    {
        return Task.FromResult(false);
    }

    public Task<bool> SaveImages(IList<ProcessedImage> images, ISaveNotify notify)
    {
        return Task.FromResult(false);
    }

    public Task<bool> EmailPDF(IList<ProcessedImage> images)
    {
        return Task.FromResult(false);
    }
}

public class StubScannedImagePrinter : IScannedImagePrinter
{
    public Task<bool> PromptToPrint(IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        return Task.FromResult(false);
    }
}

public class StubNotificationManager : INotificationManager
{
    public void PdfSaved(string path)
    {
    }

    public void ImagesSaved(int imageCount, string path)
    {
    }

    public void DonatePrompt()
    {
    }

    public void OperationProgress(OperationProgress opModalProgress, IOperation op)
    {
    }

    public void UpdateAvailable(IUpdateChecker updateChecker, UpdateInfo update)
    {
    }

    public void Rebuild()
    {
    }
}

public class StubPdfPasswordProvider : IPdfPasswordProvider
{
    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        password = null!;
        return false;
    }
}