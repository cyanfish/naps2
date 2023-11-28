using EmbedIO;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclServer : IEsclServer
{
    private readonly Dictionary<EsclDeviceConfig, CancellationTokenSource> _devices = new();
    private bool _started;
    private CancellationTokenSource? _cts;
    private MdnsAdvertiser? _advertiser;

    public void AddDevice(EsclDeviceConfig deviceConfig)
    {
        if (deviceConfig.Port == 0)
        {
            deviceConfig.Port = Port++;
        }
        _devices[deviceConfig] = new CancellationTokenSource();
        if (_started)
        {
            StartServer(deviceConfig);
            var advertiser = _advertiser!;
            Task.Run(() => advertiser.AdvertiseDevice(deviceConfig));
        }
    }

    public void RemoveDevice(EsclDeviceConfig deviceConfig)
    {
        if (_started)
        {
            // TODO: Maybe enforce ordering to ensure we don't unadvertise before advertising?
            var advertiser = _advertiser!;
            Task.Run(() => advertiser.UnadvertiseDevice(deviceConfig));
        }
        _devices[deviceConfig].Cancel();
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
        _advertiser = new MdnsAdvertiser();

        foreach (var device in _devices.Keys)
        {
            StartServer(device);
            Task.Run(() => _advertiser!.AdvertiseDevice(device));
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
        server.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(_cts!.Token, _devices[deviceConfig].Token).Token);
    }

    public void Stop()
    {
        if (!_started)
        {
            throw new InvalidOperationException();
        }
        _started = false;

        _cts!.Cancel();
        _advertiser!.Dispose();
        _cts = null;
        _advertiser = null;
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