using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class SharedDeviceManager : ISharedDeviceManager
{
    private readonly Naps2Config _config;
    private readonly ScanServer _server;

    public SharedDeviceManager(ScanningContext scanningContext, Naps2Config config)
    {
        _config = config;
        _server = new ScanServer(scanningContext, new EsclServer());
        _server.SetDefaultIcon(Icons.scanner_128);
        RegisterDevicesFromConfig();
    }

    public void StartSharing()
    {
        if (_config.Get(c => c.DisableScannerSharing))
        {
            return;
        }
        _server.Start();
    }

    public void StopSharing()
    {
        if (_config.Get(c => c.DisableScannerSharing))
        {
            return;
        }
        _server.Stop();
    }

    public void AddSharedDevice(SharedDevice device)
    {
        var devices = _config.Get(c => c.SharedDevices);
        if (devices.Contains(device))
        {
            // Ignore adding duplicates
            return;
        }
        devices = devices.Add(device);
        _config.User.Set(c => c.SharedDevices, devices);
        _server.RegisterDevice(device);
    }

    public void RemoveSharedDevice(SharedDevice device)
    {
        var devices = _config.Get(c => c.SharedDevices);
        devices = devices.Remove(device);
        _config.User.Set(c => c.SharedDevices, devices);
        _server.UnregisterDevice(device);
    }

    public void ReplaceSharedDevice(SharedDevice original, SharedDevice replacement)
    {
        var devices = _config.Get(c => c.SharedDevices);
        if (original != replacement && devices.Contains(replacement))
        {
            // Delete if the new config is a duplicate
            RemoveSharedDevice(original);
            return;
        }
        devices = devices.Replace(original, replacement);
        _config.User.Set(c => c.SharedDevices, devices);
        _server.UnregisterDevice(original);
        _server.RegisterDevice(replacement);
    }

    public IEnumerable<SharedDevice> SharedDevices => _config.Get(c => c.SharedDevices);

    private void RegisterDevicesFromConfig()
    {
        foreach (var device in _config.Get(c => c.SharedDevices))
        {
            _server.RegisterDevice(device);
        }
    }
}