using NAPS2.Recovery;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class RecoveryModule : NinjectModule
{
    public override void Load()
    {
        string recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
        var recoveryStorageManager = RecoveryStorageManager.CreateFolder(recoveryFolderPath, Kernel.Get<UiImageList>());
        var fileStorageManager = new FileStorageManager(recoveryFolderPath);
        Kernel.Bind<RecoveryStorageManager>().ToConstant(recoveryStorageManager);
        Kernel.Bind<FileStorageManager>().ToConstant(fileStorageManager);
    }
}