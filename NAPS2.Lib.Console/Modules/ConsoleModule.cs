using NAPS2.Automation;
using NAPS2.Dependencies;
using NAPS2.ImportExport.Pdf;
using NAPS2.WinForms;
using Ninject.Modules;

namespace NAPS2.Modules;

public class ConsoleModule : NinjectModule
{
    public override void Load()
    {
        Bind<IPdfPasswordProvider>().To<ConsolePdfPasswordProvider>();
        Bind<ErrorOutput>().To<ConsoleErrorOutput>();
        Bind<OverwritePrompt>().To<ConsoleOverwritePrompt>();
        Bind<OperationProgress>().To<ConsoleOperationProgress>();
        Bind<IComponentInstallPrompt>().To<ConsoleComponentInstallPrompt>();
        Bind<DialogHelper>().To<WinFormsDialogHelper>(); // TODO: We don't really want this, but it is an explicit option, so it's okay for now...
        Bind<ConsoleOutput>().ToSelf().WithConstructorArgument("writer", Console.Out);
    }
}