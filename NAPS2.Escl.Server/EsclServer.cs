using EmbedIO;
using EmbedIO.WebApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Escl.Server;

public class EsclServer : IEsclServer
{
    private readonly Dictionary<EsclDeviceConfig, DeviceContext> _devices = new();
    private bool _started;
    private CancellationTokenSource? _cts;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public void AddDevice(EsclDeviceConfig deviceConfig)
    {
        var deviceCtx = new DeviceContext(deviceConfig);
        _devices[deviceConfig] = deviceCtx;
        if (_started)
        {
            Task.Run(() => StartServerAndAdvertise(deviceCtx));
        }
    }

    public void RemoveDevice(EsclDeviceConfig deviceConfig)
    {
        var deviceCtx = _devices[deviceConfig];
        if (_started)
        {
            deviceCtx.StartTask?.ContinueWith(_ => deviceCtx.Advertiser.Dispose());
        }
        deviceCtx.Cts.Cancel();
        _devices.Remove(deviceConfig);
    }

    public Task Start()
    {
        if (_started)
        {
            return Task.CompletedTask;
        }
        _started = true;
        _cts = new CancellationTokenSource();

        var tasks = new List<Task>();
        foreach (var device in _devices.Keys)
        {
            var deviceCtx = _devices[device];
            deviceCtx.StartTask = Task.Run(() => StartServerAndAdvertise(deviceCtx));
            tasks.Add(deviceCtx.StartTask);
        }
        return Task.WhenAll(tasks);
    }

    private async Task StartServerAndAdvertise(DeviceContext deviceCtx)
    {
        var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_cts!.Token, deviceCtx.Cts.Token).Token;
        // Try to run the server with the port specified in the EsclDeviceConfig first. If that fails, try random ports
        // instead, and store the actually-used port back in EsclDeviceConfig so it can be advertised correctly.
        await PortFinder.RunWithSpecifiedOrRandomPort(deviceCtx.Config.Port, async port =>
        {
            await StartServer(deviceCtx, port, cancelToken);
            deviceCtx.Config.Port = port;
        }, cancelToken);
        deviceCtx.Advertiser.AdvertiseDevice(deviceCtx.Config);
    }

    private async Task StartServer(DeviceContext deviceCtx, int port, CancellationToken cancelToken)
    {
        var url = $"http://+:{port}/";
        deviceCtx.ServerState = new EsclServerState();
        var server = new WebServer(o => o
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(url))
            .HandleUnhandledException(UnhandledServerException)
            .WithWebApi("/eSCL",
                m => m.WithController(() => new EsclApiController(deviceCtx.Config, deviceCtx.ServerState, Logger)));
        await server.StartAsync(cancelToken);
    }

    private Task UnhandledServerException(IHttpContext ctx, Exception ex)
    {
        Logger.LogError(ex, "Unhandled ESCL server error");
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        if (!_started)
        {
            return Task.CompletedTask;
        }
        _started = false;

        _cts!.Cancel();
        var tasks = new List<Task>();
        foreach (var device in _devices.Keys)
        {
            var deviceCtx = _devices[device];
            if (deviceCtx.StartTask != null)
            {
                tasks.Add(deviceCtx.StartTask.ContinueWith(_ => deviceCtx.Advertiser.UnadvertiseDevice(device)));
            }
        }
        _cts = null;
        return Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        if (_started)
        {
            Stop();
        }
    }

    private record DeviceContext(EsclDeviceConfig Config)
    {
        public MdnsAdvertiser Advertiser { get; } = new();
        public CancellationTokenSource Cts { get; } = new();
        public Task? StartTask { get; set; }
        public EsclServerState? ServerState { get; set; }
    }
}