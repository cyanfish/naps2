using NAPS2.Images.Storage;
using NAPS2.Operation;
using NAPS2.Remoting.Worker;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules
{
    public class StaticDefaultsModule : NinjectModule
    {
        public override void Load()
        {
            OperationProgress.Default = Kernel.Get<OperationProgress>();
            ImageContext.Default = Kernel.Get<ImageContext>();
            WorkerFactory.Default = Kernel.Get<IWorkerFactory>();
        }
    }
}
