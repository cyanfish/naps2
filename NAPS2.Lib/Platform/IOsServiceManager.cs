namespace NAPS2.Platform;

/// <summary>
/// Abstraction for OS-specific "run on startup" registration logic.
/// </summary>
public interface IOsServiceManager
{
    bool IsRegistered { get; }

    void Register();

    void Unregister();
}