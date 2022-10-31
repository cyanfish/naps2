using Autofac;
using NAPS2.Recovery;

namespace NAPS2.Modules;

public class RecoveryModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        string recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
        builder.Register(ctx => RecoveryStorageManager.CreateFolder(recoveryFolderPath, ctx.Resolve<UiImageList>()))
            .SingleInstance();
        builder.RegisterInstance(new FileStorageManager(recoveryFolderPath));
    }
}