using EmbedIO;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclServer : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly MdnsAdvertiser _advertiser;
    private readonly Dictionary<EsclDeviceConfig, CancellationTokenSource> _devices = new();
    private bool _started;

    public EsclServer()
    {
        _advertiser = new MdnsAdvertiser();
    }

    public void AddDevice(EsclDeviceConfig deviceConfig)
    {
        if (deviceConfig.Port == 0)
        {
            deviceConfig.Port = Port++;
        }
        _advertiser.AdvertiseDevice(deviceConfig);
        _devices[deviceConfig] = new CancellationTokenSource();
        if (_started)
        {
            StartServer(deviceConfig);
        }
    }

    public void RemoveDevice(EsclDeviceConfig deviceConfig)
    {
        _advertiser.UnadvertiseDevice(deviceConfig);
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

        foreach (var device in _devices.Keys)
        {
            StartServer(device);
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
            .WithWebApi("/escl", m => m.WithController(() => new EsclApiController(deviceConfig, serverState)));
        server.HandleHttpException(async (_, _) => { });
        server.StateChanged += ServerOnStateChanged;
        // TODO: This might block on tasks, maybe copy impl but async
        server.Start(CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, _devices[deviceConfig].Token).Token);
    }

    private void ServerOnStateChanged(object sender, WebServerStateChangedEventArgs e)
    {
    }

    public void Dispose()
    {
        _cts.Cancel();
        _advertiser.Dispose();
    }
}