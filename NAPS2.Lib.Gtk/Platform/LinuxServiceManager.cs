namespace NAPS2.Platform;

/// <summary>
/// Manages a user-level systemd service on Linux.
/// </summary>
public class LinuxServiceManager : IOsServiceManager
{
    // At the moment we only support systemd on Linux
    public bool CanRegister => Directory.Exists(" /run/systemd/system/");

    public bool IsRegistered => false;

    public void Register()
    {
    }

    public void Unregister()
    {
    }
}