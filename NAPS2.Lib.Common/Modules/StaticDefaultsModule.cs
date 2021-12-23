using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class StaticDefaultsModule : NinjectModule
{
    public override void Load()
    {
        OperationProgress.Default = Kernel.Get<OperationProgress>();
    }
}