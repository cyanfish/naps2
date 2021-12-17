using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class RecoveryModule : NinjectModule
{
    public override void Load()
    {
        string recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
        Kernel.Get<ImageContext>().UseRecovery(recoveryFolderPath);
    }
}