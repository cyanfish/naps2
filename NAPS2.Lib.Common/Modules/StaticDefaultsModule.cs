using NAPS2.Images.Storage;
using NAPS2.Operation;
using NAPS2.Remoting.Worker;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules
{
    public class StaticDefaultsModule : NinjectModule
    {
        public override void Load()
        {
            OperationProgress.Default = Kernel.Get<OperationProgress>();
            ErrorOutput.Default = Kernel.Get<MessageBoxErrorOutput>();
            DialogHelper.Default = Kernel.Get<DialogHelper>();
            OverwritePrompt.Default = Kernel.Get<OverwritePrompt>();
            ImageContext.Default = Kernel.Get<ImageContext>();
            WorkerFactory.Default = Kernel.Get<IWorkerFactory>();
        }
    }
}
