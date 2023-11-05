using NAPS2.Remoting.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting;

public class SharedDeviceManager
{
    private readonly Naps2Config _config;
    private readonly ScanServer _server;

    public SharedDeviceManager(ScanningContext scanningContext, Naps2Config config)
    {
        _config = config;
        _server = new ScanServer(scanningContext);
        RegisterDevicesFromConfig();
    }

    public void StartSharing()
    {
        // TODO: We should only actually start the web server if we have devices to share (depends on whether we have a
        // separate server instance per device)
        _server.Start();
    }

    public void StopSharing()
    {
        _server.Stop();
    }

    public void AddSharedDevice(SharedDevice device)
    {
        var devices = _config.Get(c => c.SharedDevices);
        devices = devices.Add(device);
        _config.User.Set(c => c.SharedDevices, devices);
        _server.RegisterDevice(device.Driver, device.Device, device.Name);
    }

    public void RemoveSharedDevice(SharedDevice device)
    {
        var devices = _config.Get(c => c.SharedDevices);
        devices = devices.Remove(device);
        _config.User.Set(c => c.SharedDevices, devices);
        _server.UnregisterDevice(device.Driver, device.Device);
    }

    public void ReplaceSharedDevice(SharedDevice original, SharedDevice replacement)
    {
        var devices = _config.Get(c => c.SharedDevices);
        devices = devices.Replace(original, replacement);
        _config.User.Set(c => c.SharedDevices, devices);
        _server.UnregisterDevice(original.Driver, original.Device);
        _server.RegisterDevice(replacement.Driver, replacement.Device, replacement.Name);
    }

    public IEnumerable<SharedDevice> SharedDevices => _config.Get(c => c.SharedDevices);

    private void RegisterDevicesFromConfig()
    {
        foreach (var device in _config.Get(c => c.SharedDevices))
        {
            _server.RegisterDevice(device.Driver, device.Device, device.Name);
        }
    }
}