using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Config.Model;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class SharedDeviceManager : ISharedDeviceManager
{
    private const int STARTUP_RETRY_INTERVAL = 10_000;

    private readonly ILogger _logger;
    private readonly Naps2Config _config;
    private readonly ScanningContext _scanningContext;
    private readonly FileConfigScope<SharingConfig> _scope;
    private readonly string _lockFilePath;
    private readonly Func<IEsclServer> _esclServerFactory;
    private ScanServer? _server;
    private FileStream? _lockFile;
    private Timer? _startTimer;
    private bool _userStarted;

    public SharedDeviceManager(ScanningContext scanningContext, Naps2Config config, string sharedDevicesConfigPath)
        : this(scanningContext, config, sharedDevicesConfigPath, Path.Combine(Paths.AppData, "sharing.lock"),
            () => new EsclServer())
    {
    }

    internal SharedDeviceManager(ScanningContext scanningContext, Naps2Config config, string sharedDevicesConfigPath,
        string lockFilePath, Func<IEsclServer> esclServerFactory)
    {
        _logger = scanningContext.Logger;
        _config = config;
        _scope = ConfigScope.File(sharedDevicesConfigPath, new ConfigStorageSerializer<SharingConfig>(),
            ConfigScopeMode.ReadWrite);
        _scanningContext = scanningContext;
        _lockFilePath = lockFilePath;
        _esclServerFactory = esclServerFactory;
    }

    private ScanServer Server
    {
        get
        {
            lock (this)
            {
                return _server ??= CreateServer();
            }
        }
    }

    private ScanServer CreateServer()
    {
        var server = new ScanServer(_scanningContext, _esclServerFactory());
        server.SetDefaultIcon(Icons.scanner_128);
        server.SecurityPolicy = _config.Get(c => c.EsclSecurityPolicy);
        if (_config.Get(c => c.EsclServerCertificatePath) is { } certPath && !string.IsNullOrWhiteSpace(certPath))
        {
            try
            {
                server.Certificate = Path.GetExtension(certPath) == ".pfx"
                    ? X509CertificateLoader.LoadPkcs12FromFile(certPath, null)
                    : X509CertificateLoader.LoadCertificateFromFile(certPath);
                if (!server.Certificate.HasPrivateKey)
                {
                    _logger.LogDebug(
                        $"Certificate has no private key. Make sure it's installed in the local computer's certificate store. \"{certPath}\"");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"Could not read X509 certificate from EsclServerCertificatePath \"{certPath}\"");
            }
        }
        server.InstanceId = _scope.GetOrDefault(c => c.InstanceId) ?? Guid.NewGuid();
        foreach (var device in SharedDevices)
        {
            RegisterOnServer(server, device);
        }
        return server;
    }

    public void StartSharing()
    {
        if (_config.Get(c => c.DisableScannerSharing))
        {
            return;
        }
        lock (this)
        {
            if (_userStarted) return;
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
                Server.Start().ContinueWith(t =>
                    _logger.LogError(t.Exception, "Error starting ScanServer"), TaskContinuationOptions.OnlyOnFaulted);
                return true;
            }
            return false;
        }
    }

    public void StopSharing()
    {
        lock (this)
        {
            if (!_userStarted) return;
            _userStarted = false;
            ResetStartTimer();
            _server?.Stop().ContinueWith(t =>
                _logger.LogError(t.Exception, "Error stopping ScanServer"), TaskContinuationOptions.OnlyOnFaulted);
            ReleaseLock();
        }
    }

    private bool TakeLock()
    {
        try
        {
            _lockFile = new FileStream(_lockFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
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
        lock (this)
        {
            var devices = SharedDevices;
            if (devices.Contains(device))
            {
                // Ignore adding duplicates
                return;
            }
            devices = devices.Add(device);
            SharedDevices = devices;
            if (_server != null) RegisterOnServer(_server, device);
            if (_startTimer != null)
            {
                // If startup was deferred, don't wait for the timer before retrying since adding a device might allow
                // us to start
                TryStart();
            }
        }
    }

    public void RemoveSharedDevice(SharedDevice device)
    {
        lock (this)
        {
            var devices = SharedDevices;
            devices = devices.Remove(device);
            SharedDevices = devices;
            if (_server != null) UnregisterOnServer(_server, device);
        }
    }

    public void ReplaceSharedDevice(SharedDevice original, SharedDevice replacement)
    {
        lock (this)
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
            if (_server != null)
            {
                UnregisterOnServer(_server, original);
                RegisterOnServer(_server, replacement);
            }
        }
    }

    public ImmutableList<SharedDevice> SharedDevices
    {
        get => _scope.GetOr(c => c.SharedDevices, ImmutableList<SharedDevice>.Empty);
        private set
        {
            if (!_scope.Has(c => c.InstanceId))
            {
                _scope.Set(c => c.InstanceId, _server?.InstanceId ?? Guid.NewGuid());
            }
            _scope.Set(c => c.SharedDevices, value);
        }
    }

    public event EventHandler? SharingServerStopped;
    public void InvokeSharingServerStopped() => SharingServerStopped?.Invoke(this, EventArgs.Empty);

    private static void RegisterOnServer(ScanServer server, SharedDevice device) =>
        server.RegisterDevice(device.Device, device.Name, device.Port, device.TlsPort);

    private static void UnregisterOnServer(ScanServer server, SharedDevice device) =>
        server.UnregisterDevice(device.Device, device.Name);
}