using NAPS2.Automation;
using NAPS2.Dependencies;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.WinForms;
using Ninject.Modules;

namespace NAPS2.Modules;

public class ConsoleModule : NinjectModule
{
    private readonly AutomatedScanningOptions _options;

    public ConsoleModule(AutomatedScanningOptions options)
    {
        _options = options;
    }

    public override void Load()
    {
        Bind<AutomatedScanningOptions>().ToConstant(_options);

        Bind<IPdfPasswordProvider>().To<ConsolePdfPasswordProvider>();
        Bind<ErrorOutput>().To<ConsoleErrorOutput>().InSingletonScope();
        Bind<IOverwritePrompt>().To<ConsoleOverwritePrompt>();
        Bind<OperationProgress>().To<ConsoleOperationProgress>();
        // TODO: We might want an eto-based dialog helper, or at least handle dialogs in a more user-friendly way than just silently doing nothing
        Bind<DialogHelper>().To<StubDialogHelper>();
        Bind<ConsoleOutput>().ToSelf().WithConstructorArgument("writer", Console.Out);
        Bind<ISaveNotify>().To<SaveNotifyStub>();
        Bind<IDevicePrompt>().To<ConsoleDevicePrompt>();
    }
}