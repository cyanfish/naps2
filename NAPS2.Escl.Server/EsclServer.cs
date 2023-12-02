using EmbedIO;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclServer : IEsclServer
{
    private readonly Dictionary<EsclDeviceConfig, (MdnsAdvertiser advertiser, CancellationTokenSource cts)> _devices = new();
    private bool _started;
    private CancellationTokenSource? _cts;

    public void AddDevice(EsclDeviceConfig deviceConfig)
    {
        if (deviceConfig.Port == 0)
        {
            deviceConfig.Port = Port++;
        }
        var advertiser = new MdnsAdvertiser();
        _devices[deviceConfig] = (advertiser, new CancellationTokenSource());
        if (_started)
        {
            StartServer(deviceConfig);
            Task.Run(() => advertiser.AdvertiseDevice(deviceConfig));
        }
    }

    public void RemoveDevice(EsclDeviceConfig deviceConfig)
    {
        var (advertiser, cts) = _devices[deviceConfig];
        if (_started)
        {
            // TODO: Maybe enforce ordering to ensure we don't unadvertise before advertising?
            Task.Run(() => advertiser.Dispose());
        }
        cts.Cancel();
        _devices.Remove(deviceConfig);
    }

    // TODO: Better port handling
    public int Port { get; set; } = 9898;

    public void Start()
    {
        if (_started)
        {
            throw new InvalidOperationException();
        }
        _started = true;
        _cts = new CancellationTokenSource();

        foreach (var device in _devices.Keys)
        {
            StartServer(device);
            Task.Run(() => _devices[device].advertiser.AdvertiseDevice(device));
        }
    }

    private void StartServer(EsclDeviceConfig deviceConfig)
    {
        // TODO: Auto free port?
        var url = $"http://+:{deviceConfig.Port}/";
        var serverState = new EsclServerState();
        var server = new WebServer(o => o
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(url))
            .WithWebApi("/eSCL", m => m.WithController(() => new EsclApiController(deviceConfig, serverState)));
        server.StateChanged += ServerOnStateChanged;
        server.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(_cts!.Token, _devices[deviceConfig].cts.Token).Token);
    }

    public void Stop()
    {
        if (!_started)
        {
            throw new InvalidOperationException();
        }
        _started = false;

        _cts!.Cancel();
        foreach (var device in _devices.Keys)
        {
            _devices[device].advertiser.UnadvertiseDevice(device);
        }
        _cts = null;
    }

    private void ServerOnStateChanged(object sender, WebServerStateChangedEventArgs e)
    {
    }

    public void Dispose()
    {
        if (_started)
        {
            Stop();
        }
    }
}