using NAPS2.EtoForms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Update;
using NAPS2.WinForms;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class WinFormsModule : NinjectModule
{
    public override void Load()
    {
        Bind<IBatchScanPerformer>().To<BatchScanPerformer>();
        Bind<IPdfPasswordProvider>().To<WinFormsPdfPasswordProvider>();
        Bind<ErrorOutput>().To<MessageBoxErrorOutput>();
        Bind<IOverwritePrompt>().To<EtoOverwritePrompt>();
        Bind<OperationProgress>().To<EtoOperationProgress>().InSingletonScope();
        Bind<DialogHelper>().To<EtoDialogHelper>();
        Bind<IDevicePrompt>().To<EtoDevicePrompt>();
        Bind<INotificationManager>().To<NotificationManager>().InSingletonScope();
        Bind<ISaveNotify>().ToMethod(ctx => ctx.Kernel.Get<INotificationManager>());
        Bind<IScannedImagePrinter>().To<PrintDocumentPrinter>();
        Bind<DesktopController>().ToSelf().InSingletonScope();
        Bind<IUpdateChecker>().To<UpdateChecker>();
        Bind<IExportController>().To<ExportController>();
        Bind<IDesktopScanController>().To<DesktopScanController>();
        Bind<IDesktopSubFormController>().To<DesktopSubFormController>();
        Bind<DesktopFormProvider>().ToSelf().InSingletonScope();

        Bind<DesktopForm>().To<WinFormsDesktopForm>();

        EtoPlatform.Current = new WinFormsEtoPlatform();
        // TODO: Can we add a test for this?
        Log.EventLogger = new WindowsEventLogger(Kernel!.Get<Naps2Config>());
    }
}