using EmbedIO;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclServer : IEsclServer
{
    private readonly Dictionary<EsclDeviceConfig, DeviceContext> _devices = new();
    private bool _started;
    private CancellationTokenSource? _cts;

    public void AddDevice(EsclDeviceConfig deviceConfig)
    {
        var advertiser = new MdnsAdvertiser();
        _devices[deviceConfig] = new DeviceContext(advertiser, new CancellationTokenSource());
        if (_started)
        {
            Task.Run(() => StartServerAndAdvertise(deviceConfig, advertiser));
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
            deviceCtx.StartTask = Task.Run(() => StartServerAndAdvertise(device, _devices[device].Advertiser));
            tasks.Add(deviceCtx.StartTask);
        }
        return Task.WhenAll(tasks);
    }

    private async Task StartServerAndAdvertise(EsclDeviceConfig deviceConfig, MdnsAdvertiser advertiser)
    {
        var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_cts!.Token, _devices[deviceConfig].Cts.Token).Token;
        // Try to run the server with the port specified in the EsclDeviceConfig first. If that fails, try random ports
        // instead, and store the actually-used port back in EsclDeviceConfig so it can be advertised correctly.
        await PortFinder.RunWithSpecifiedOrRandomPort(deviceConfig.Port, async port =>
        {
            await StartServer(deviceConfig, port, cancelToken);
            deviceConfig.Port = port;
        }, cancelToken);
        advertiser.AdvertiseDevice(deviceConfig);
    }

    private async Task StartServer(EsclDeviceConfig deviceConfig, int port, CancellationToken cancelToken)
    {
        var url = $"http://+:{port}/";
        var serverState = new EsclServerState();
        var server = new WebServer(o => o
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(url))
            .WithWebApi("/eSCL", m => m.WithController(() => new EsclApiController(deviceConfig, serverState)));
        await server.StartAsync(cancelToken);
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }
        _started = false;

        _cts!.Cancel();
        foreach (var device in _devices.Keys)
        {
            var deviceCtx = _devices[device];
            deviceCtx.StartTask?.ContinueWith(_ => deviceCtx.Advertiser.UnadvertiseDevice(device));
        }
        _cts = null;
    }

    public void Dispose()
    {
        if (_started)
        {
            Stop();
        }
    }

    private record DeviceContext(MdnsAdvertiser Advertiser, CancellationTokenSource Cts)
    {
        public Task? StartTask { get; set; }
    }
}