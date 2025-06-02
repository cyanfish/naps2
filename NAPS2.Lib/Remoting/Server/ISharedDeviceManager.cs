using System.Collections.Immutable;

namespace NAPS2.Remoting.Server;

public interface ISharedDeviceManager
{
    // TODO: Maybe have Start/Stop return a task so we ensure that deregistration occurs before the app closes?
    void StartSharing();
    void StopSharing();
    void AddSharedDevice(SharedDevice device);
    void RemoveSharedDevice(SharedDevice device);
    void ReplaceSharedDevice(SharedDevice original, SharedDevice replacement);
    ImmutableList<SharedDevice> SharedDevices { get; }
    event EventHandler SharingServerStopped;
    void InvokeSharingServerStopped();
}