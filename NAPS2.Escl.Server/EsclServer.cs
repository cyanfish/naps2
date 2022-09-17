using EmbedIO;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclServer : IDisposable
{
    private readonly EsclServerConfig _serverConfig;
    private readonly EsclServerState _serverState = new();
    private readonly CancellationTokenSource _cts = new();
    private WebServer? _server;

    public EsclServer(EsclServerConfig serverConfig)
    {
        _serverConfig = serverConfig;
    }

    public int Port { get; set; } = 9898;

    public void Start()
    {
        if (_server != null)
        {
            throw new InvalidOperationException();
        }
        var url = $"http://+:{Port}/";
        _server = new WebServer(o => o
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(url))
            .WithWebApi("/escl", m => m.WithController(() => new EsclApiController(_serverConfig, _serverState)));
        _server.HandleHttpException(async (ctx, ex) =>
        {
            
        });
        _server.StateChanged += ServerOnStateChanged;
        // TODO: This might block on tasks, maybe copy impl but async
        _server.Start(_cts.Token);
    }

    private void ServerOnStateChanged(object sender, WebServerStateChangedEventArgs e)
    {
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}