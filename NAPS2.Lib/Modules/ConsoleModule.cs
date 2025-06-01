using Autofac;
using NAPS2.Automation;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Modules;

/// <summary>
/// Console-specific module used by ConsoleEntryPoint.
/// </summary>
public class ConsoleModule : Module
{
    private readonly AutomatedScanningOptions _options;

    public ConsoleModule(AutomatedScanningOptions options)
    {
        _options = options;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(_options);

        builder.RegisterType<ConsolePdfPasswordProvider>().As<IPdfPasswordProvider>();
        builder.RegisterType<ConsoleErrorOutput>().As<ErrorOutput>().SingleInstance();
        builder.RegisterType<ConsoleOverwritePrompt>().As<IOverwritePrompt>();
        if (_options.Progress)
        {
            builder.RegisterType<EtoOperationProgress>().As<OperationProgress>();
        }
        else
        {
            builder.RegisterType<ConsoleOperationProgress>().As<OperationProgress>();
        }
        builder.RegisterType<StubNotify>().As<INotify>();
        // TODO: We might want an eto-based dialog helper, or at least handle dialogs in a more user-friendly way than just silently doing nothing
        builder.RegisterType<StubDialogHelper>().As<DialogHelper>();
        builder.RegisterType<ConsoleOutput>().AsSelf().WithParameter("writer", Console.Out);
        builder.RegisterType<SaveNotifyStub>().As<ISaveNotify>();
        builder.RegisterType<ConsoleDevicePrompt>().As<IDevicePrompt>();

        builder.RegisterBuildCallback(ctx =>
        {
            var scanningContext = ctx.Resolve<ScanningContext>();
            scanningContext.FileStorageManager = ctx.Resolve<FileStorageManager>();
            scanningContext.OcrEngine = ctx.Resolve<IOcrEngine>();
        });
    }
}