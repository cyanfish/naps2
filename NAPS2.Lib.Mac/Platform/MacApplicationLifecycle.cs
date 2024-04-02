using NAPS2.Remoting;

namespace NAPS2.Platform;

public class MacApplicationLifecycle(ProcessCoordinator processCoordinator, Naps2Config config)
    : ApplicationLifecycle(processCoordinator, config)
{
    protected override void HandleSingleInstance()
    {
        // Mac is single-instance by default and doesn't need any special handling.
        // "Open With" also uses NSApplicationDelegate so we don't need to communicate that cross-process.
    }
}