using Autofac;
using NAPS2.Automation;
using NAPS2.EtoForms;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Modules;

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
        builder.RegisterType<ConsoleOperationProgress>().As<OperationProgress>();
        // TODO: We might want an eto-based dialog helper, or at least handle dialogs in a more user-friendly way than just silently doing nothing
        builder.RegisterType<StubDialogHelper>().As<DialogHelper>();
        builder.RegisterType<ConsoleOutput>().AsSelf().WithParameter("writer", Console.Out);
        builder.RegisterType<SaveNotifyStub>().As<ISaveNotify>();
        builder.RegisterType<ConsoleDevicePrompt>().As<IDevicePrompt>();
    }
}