using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Mac;
using NAPS2.EtoForms.Ui;
using NAPS2.Images.Mac;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Update;

namespace NAPS2.Modules;

public class MacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // builder.RegisterType<BatchScanPerformer>().As<IBatchScanPerformer>();
        builder.RegisterType<StubPdfPasswordProvider>().As<IPdfPasswordProvider>();
        builder.RegisterType<MessageBoxErrorOutput>().As<ErrorOutput>();
        builder.RegisterType<EtoOverwritePrompt>().As<IOverwritePrompt>();
        builder.RegisterType<EtoOperationProgress>().As<OperationProgress>().SingleInstance();
        builder.RegisterType<EtoDialogHelper>().As<DialogHelper>();
        builder.RegisterType<EtoDevicePrompt>().As<IDevicePrompt>();
        builder.RegisterType<StubNotificationManager>().As<INotificationManager>().SingleInstance();
        builder.Register<ISaveNotify>(ctx => ctx.Resolve<INotificationManager>());
        builder.RegisterType<StubScannedImagePrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<DesktopController>().AsSelf().SingleInstance();
        builder.RegisterType<DesktopScanController>().As<IDesktopScanController>();
        builder.RegisterType<UpdateChecker>().As<IUpdateChecker>();
        builder.RegisterType<ExportController>().As<IExportController>();
        builder.RegisterType<StubDesktopSubFormController>().As<IDesktopSubFormController>();
        builder.RegisterType<DesktopFormProvider>().AsSelf().SingleInstance();
        builder.RegisterType<MacImageContext>().As<ImageContext>();
        builder.RegisterType<MacImageContext>().AsSelf();
        builder.RegisterType<MacIconProvider>().As<IIconProvider>();

        builder.RegisterType<MacDesktopForm>().As<DesktopForm>();
        builder.RegisterType<MacPreviewForm>().As<PreviewForm>();

        EtoPlatform.Current = new MacEtoPlatform();
        // Log.EventLogger = new WindowsEventLogger(Kernel!.Get<Naps2Config>());
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