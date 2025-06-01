using Autofac;
using NAPS2.Recovery;

namespace NAPS2.Modules;

/// <summary>
/// Recovery folder setup used by all entry points except the worker (which initializes based on the parent process).
/// </summary>
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