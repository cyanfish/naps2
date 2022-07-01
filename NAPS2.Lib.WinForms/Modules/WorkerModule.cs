using NAPS2.Scan.Internal.Twain;
using Ninject.Modules;

namespace NAPS2.Modules;

public class WorkerModule : NinjectModule
{
    public override void Load()
    {
        Bind<ITwainSessionController>().To<LocalTwainSessionController>();
    }
}