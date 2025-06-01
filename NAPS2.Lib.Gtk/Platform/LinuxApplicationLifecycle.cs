using NAPS2.Remoting;

namespace NAPS2.Platform;

public class LinuxApplicationLifecycle(
    ProcessCoordinator processCoordinator,
    IOsServiceManager serviceManager,
    Naps2Config config)
    : ApplicationLifecycle(processCoordinator, serviceManager, config)
{
}