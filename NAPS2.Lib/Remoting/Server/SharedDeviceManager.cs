using System.Collections.Immutable;
using System.Threading;
using NAPS2.Config.Model;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class SharedDeviceManager : ISharedDeviceManager
{
    private const int LOCK_CHECK_INTERVAL = 10_000;

    private readonly Naps2Config _config;
    private readonly FileConfigScope<ImmutableList<SharedDevice>> _scope;
    private readonly ScanServer _server;
    private FileStream? _lockFile;
    private Timer? _lockCheckTimer;

    public SharedDeviceManager(ScanningContext scanningContext, Naps2Config config, string sharedDevicesConfigPath)
    {
        _config = config;
        _scope = ConfigScope.File(sharedDevicesConfigPath, new ConfigStorageSerializer<ImmutableList<SharedDevice>>(),
            ConfigScopeMode.ReadWrite);
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
        if (TakeLock())
        {
            ResetLockTimer();
            _server.Start();
        }
        else if (_lockCheckTimer == null)
        {
            _lockCheckTimer = new Timer(_ => StartSharing(), null, LOCK_CHECK_INTERVAL, LOCK_CHECK_INTERVAL);
        }
    }

    public void StopSharing()
    {
        if (_config.Get(c => c.DisableScannerSharing))
        {
            return;
        }
        ResetLockTimer();
        _server.Stop();
        ReleaseLock();
    }

    private bool TakeLock()
    {
        try
        {
            var path = Path.Combine(Paths.AppData, "sharing.lock");
            _lockFile = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private void ResetLockTimer()
    {
        _lockCheckTimer?.Dispose();
        _lockCheckTimer = null;
    }

    private void ReleaseLock()
    {
        _lockFile?.Dispose();
        _lockFile = null;
    }

    public void AddSharedDevice(SharedDevice device)
    {
        var devices = SharedDevices;
        if (devices.Contains(device))
        {
            // Ignore adding duplicates
            return;
        }
        devices = devices.Add(device);
        SharedDevices = devices;
        _server.RegisterDevice(device);
    }

    public void RemoveSharedDevice(SharedDevice device)
    {
        var devices = SharedDevices;
        devices = devices.Remove(device);
        SharedDevices = devices;
        _server.UnregisterDevice(device);
    }

    public void ReplaceSharedDevice(SharedDevice original, SharedDevice replacement)
    {
        var devices = SharedDevices;
        if (original != replacement && devices.Contains(replacement))
        {
            // Delete if the new config is a duplicate
            RemoveSharedDevice(original);
            return;
        }
        devices = devices.Replace(original, replacement);
        SharedDevices = devices;
        _server.UnregisterDevice(original);
        _server.RegisterDevice(replacement);
    }

    public ImmutableList<SharedDevice> SharedDevices
    {
        get => _scope.GetOr(c => c, ImmutableList<SharedDevice>.Empty);
        private set => _scope.Set(c => c, value);
    }

    private void RegisterDevicesFromConfig()
    {
        foreach (var device in SharedDevices)
        {
            _server.RegisterDevice(device);
        }
    }
}