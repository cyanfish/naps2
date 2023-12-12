using System.Collections.Immutable;
using System.Threading;
using NAPS2.Config.Model;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class SharedDeviceManager : ISharedDeviceManager
{
    private const int STARTUP_RETRY_INTERVAL = 10_000;

    private readonly Naps2Config _config;
    private readonly FileConfigScope<SharingConfig> _scope;
    private readonly ScanServer _server;
    private FileStream? _lockFile;
    private Timer? _startTimer;
    private bool _userStarted;

    public SharedDeviceManager(ScanningContext scanningContext, Naps2Config config, string sharedDevicesConfigPath)
    {
        _config = config;
        _scope = ConfigScope.File(sharedDevicesConfigPath, new ConfigStorageSerializer<SharingConfig>(),
            ConfigScopeMode.ReadWrite);
        _server = new ScanServer(scanningContext, new EsclServer());
        _server.SetDefaultIcon(Icons.scanner_128);
        _server.InstanceId = _scope.GetOrDefault(c => c.InstanceId) ?? Guid.NewGuid();
        RegisterDevicesFromConfig();
    }

    public void StartSharing()
    {
        if (_config.Get(c => c.DisableScannerSharing))
        {
            return;
        }
        lock (this)
        {
            _userStarted = true;
            if (!TryStart())
            {
                // Retry after some interval in case the shared devices changed on disk or the sharing lock frees up
                _startTimer ??= new Timer(_ => TryStart(), null, STARTUP_RETRY_INTERVAL, STARTUP_RETRY_INTERVAL);
            }
        }
    }

    private bool TryStart()
    {
        lock (this)
        {
            // Only start if (1) we haven't stopped, (2) we have devices to share, and (3) we can take the exclusive
            // sharing lock (so multiple NAPS2 instances don't try to share duplicates of the same devices)
            if (_userStarted && SharedDevices.Any() && TakeLock())
            {
                ResetStartTimer();
                _server.Start();
                return true;
            }
            return false;
        }
    }

    public void StopSharing()
    {
        lock (this)
        {
            _userStarted = false;
            ResetStartTimer();
            _server.Stop();
            ReleaseLock();
        }
    }

    private bool TakeLock()
    {
        try
        {
            var path = Path.Combine(Paths.AppData, "sharing.lock");
            _lockFile = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private void ResetStartTimer()
    {
        _startTimer?.Dispose();
        _startTimer = null;
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
        if (_startTimer != null)
        {
            // If startup was deferred, don't wait for the timer before retrying since we adding a device might allow
            // us to start
            TryStart();
        }
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
        get => _scope.GetOr(c => c.SharedDevices, ImmutableList<SharedDevice>.Empty);
        private set
        {
            if (!_scope.Has(c => c.InstanceId))
            {
                _scope.Set(c => c.InstanceId, _server.InstanceId);
            }
            _scope.Set(c => c.SharedDevices, value);
        }
    }

    private void RegisterDevicesFromConfig()
    {
        foreach (var device in SharedDevices)
        {
            _server.RegisterDevice(device);
        }
    }
}