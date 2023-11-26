using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class ScanServer : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<(Driver, string), EsclDeviceConfig> _currentDevices = new();
    private EsclServer? _esclServer;

    public ScanServer(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        ScanController = new ScanController(scanningContext);
    }

    internal ScanController ScanController { get; set; }

    public void RegisterDevice(Driver driver, ScanDevice device, string? name = null)
    {
        var key = (driver, device.ID);
        var esclDeviceConfig = MakeEsclDeviceConfig(driver, device, name ?? device.Name);
        _currentDevices.Add(key, esclDeviceConfig);
        _esclServer?.AddDevice(esclDeviceConfig);
    }

    public void UnregisterDevice(Driver driver, ScanDevice device)
    {
        var key = (driver, device.ID);
        var esclDeviceConfig = _currentDevices[key];
        _currentDevices.Remove(key);
        _esclServer?.RemoveDevice(esclDeviceConfig);
    }

    private EsclDeviceConfig MakeEsclDeviceConfig(Driver driver, ScanDevice device, string name)
    {
        var uniqueHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(device.ID));
        return new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                MakeAndModel = name,
                Uuid = new Guid(uniqueHash.Take(16).ToArray()).ToString("D"),
                // TODO: Ideally we want to get the actual device capabilities
                PlatenCaps = new EsclInputCaps
                {
                    SettingProfiles =
                    {
                        new EsclSettingProfile
                        {
                            ColorModes =
                                { EsclColorMode.RGB24, EsclColorMode.Grayscale8, EsclColorMode.BlackAndWhite1 },
                            XResolutionRange = new ResolutionRange(100, 4800, 300),
                            YResolutionRange = new ResolutionRange(100, 4800, 300),
                            DocumentFormats = { "application/pdf", "image/jpeg" }
                        }
                    }
                }
            },
            CreateJob = () => new ScanJob(ScanController, driver, device)
        };
    }

    public void Start()
    {
        if (_esclServer != null) throw new InvalidOperationException("Already started");
        _esclServer = new EsclServer();
        foreach (var device in _currentDevices.Values)
        {
            _esclServer.AddDevice(device);
        }
        _esclServer.Start();
    }

    public void Stop()
    {
        if (_esclServer == null) throw new InvalidOperationException("Not started");
        _esclServer.Dispose();
        _esclServer = null;
    }

    public void Dispose()
    {
        _esclServer?.Dispose();
        _esclServer = null;
    }

    private class ScanJob : IEsclScanJob
    {
        private readonly ScanController _controller;
        private readonly CancellationTokenSource _cts = new();
        private readonly IAsyncEnumerator<ProcessedImage> _enumerable;

        public ScanJob(ScanController controller, Driver driver, ScanDevice device)
        {
            _controller = controller;
            _enumerable = controller.Scan(new ScanOptions { Driver = driver, Device = device }, _cts.Token).GetAsyncEnumerator();
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        // TODO: Handle errors
        public async Task<bool> WaitForNextDocument() => await _enumerable.MoveNextAsync();

        public void WriteDocumentTo(Stream stream)
        {
            // TODO: PDF etc
            _enumerable.Current.Save(stream, ImageFileFormat.Jpeg);
        }

        public async Task WriteProgressTo(Stream stream)
        {
            var tcs = new TaskCompletionSource<bool>();
            var streamWriter = new StreamWriter(stream);

            void OnPageProgress(object? sender, PageProgressEventArgs e)
            {
                // TODO: Match page number?
                // TODO: Throttle?
                streamWriter.WriteLine(e.Progress.ToString(CultureInfo.InvariantCulture));
                streamWriter.Flush();
            }
            void OnPageEnd(object? sender, PageEndEventArgs e)
            {
                tcs.TrySetResult(true);
            }

            _controller.PageProgress += OnPageProgress;
            _controller.PageEnd += OnPageEnd;
            // TODO: Terminate in case of errors or if we called too late
            await tcs.Task;
            _controller.PageProgress -= OnPageProgress;
            _controller.PageEnd -= OnPageEnd;
        }
    }
}