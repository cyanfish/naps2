using NAPS2.Dependencies;
using NAPS2.EtoForms;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject.Modules;

namespace NAPS2.Modules;

public class WinFormsModule : NinjectModule
{
    public override void Load()
    {
        Bind<IPdfPasswordProvider>().To<WinFormsPdfPasswordProvider>();
        Bind<ErrorOutput>().To<MessageBoxErrorOutput>();
        Bind<OverwritePrompt>().To<WinFormsOverwritePrompt>();
        Bind<OperationProgress>().To<WinFormsOperationProgress>().InSingletonScope();
        Bind<IComponentInstallPrompt>().To<WinFormsComponentInstallPrompt>();
        Bind<DialogHelper>().To<WinFormsDialogHelper>();
        Bind<IEtoPlatform>().To<WinFormsEtoPlatform>();
    }
}