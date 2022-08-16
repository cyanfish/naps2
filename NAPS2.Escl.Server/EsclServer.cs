using EmbedIO;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclServer : IDisposable
{
    private readonly EsclServerConfig _serverConfig;
    private WebServer? _server;

    public EsclServer(EsclServerConfig serverConfig)
    {
        _serverConfig = serverConfig;
    }

    public void Start()
    {
        if (_server != null)
        {
            throw new InvalidOperationException();
        }
        var url = "http://localhost:9898/";
        _server = new WebServer(o => o
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(url))
            .WithWebApi("/escl", m => m.WithController(() => new EsclApiController(_serverConfig)));
        _server.StateChanged += ServerOnStateChanged;
        // TODO: This might block on tasks, maybe copy impl but async
        _server.Start();
    }

    private void ServerOnStateChanged(object sender, WebServerStateChangedEventArgs e)
    {
        
    }

    public void Dispose()
    {
        _server?.Dispose();
    }
}