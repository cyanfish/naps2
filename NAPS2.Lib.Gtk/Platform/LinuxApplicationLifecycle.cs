using NAPS2.Remoting;

namespace NAPS2.Platform;

public class LinuxApplicationLifecycle(ProcessCoordinator processCoordinator, Naps2Config config)
    : ApplicationLifecycle(processCoordinator, config)
{
}