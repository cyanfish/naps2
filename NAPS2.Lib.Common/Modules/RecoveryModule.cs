using NAPS2.Scan;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class RecoveryModule : NinjectModule
{
    public override void Load()
    {
        // TODO: We still need to handle recovery metadata
        string recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
        Kernel.Get<ScanningContext>().FileStorageManager = new FileStorageManager(recoveryFolderPath);
    }
}