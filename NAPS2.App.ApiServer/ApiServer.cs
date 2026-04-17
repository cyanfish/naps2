using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using NAPS2.App.ApiServer.Controllers;
using NAPS2.Lib.ApiServer;
using System.Security.Cryptography.X509Certificates;

namespace NAPS2.App.ApiServer;

public sealed class ApiServer : IApiServer
{
    private readonly ApiServerConfiguration _config;
    private readonly ScanJobManager _scanJobManager = new();
    private WebServer? _server;
    private bool _isRunning;

    public ApiServer(ApiServerConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public bool IsRunning => _isRunning;

    public int Port => _config.Port;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return;
        }

        _config.Validate();
        _server = CreateWebServer(_config);
        await _server.StartAsync(cancellationToken);
        _isRunning = true;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning || _server == null)
        {
            return Task.CompletedTask;
        }

        _server.Dispose();
        _server = null;
        _isRunning = false;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_server != null)
        {
            _server.Dispose();
            _server = null;
        }

        _scanJobManager.Dispose();
    }

    private WebServer CreateWebServer(ApiServerConfiguration config)
    {
        var certificate = config.LoadCertificate();
        var serverOptions = new WebServerOptions()
            .WithMode(HttpListenerMode.EmbedIO)
            .WithUrlPrefix(config.UrlPrefix);

        if (certificate != null)
        {
            serverOptions = serverOptions.WithCertificate(certificate);
        }

        var server = new WebServer(serverOptions)
            .HandleUnhandledException((context, exception) =>
            {
                Console.Error.WriteLine($"Unhandled API server error: {exception}");
                return Task.CompletedTask;
            });

        if (config.EnableCors)
        {
            server.WithCors();
        }

        server.WithWebApi("/api", m => m
            .WithController(() => new ApiRootController(config))
            .WithController(() => new Controllers.ScanController(config, _scanJobManager))
            .WithController(() => new SettingsController(config)));

        server.WithAction("/", HttpVerbs.Get, ctx => ctx.SendStringAsync("NAPS2 API Server is running", "text/plain", System.Text.Encoding.UTF8));
        return server;
    }
}
