namespace NAPS2.Remoting.Server;

public interface ISharedDeviceManager
{
    void StartSharing();
    void StopSharing();
    void AddSharedDevice(SharedDevice device);
    void RemoveSharedDevice(SharedDevice device);
    void ReplaceSharedDevice(SharedDevice original, SharedDevice replacement);
    IEnumerable<SharedDevice> SharedDevices { get; }
}