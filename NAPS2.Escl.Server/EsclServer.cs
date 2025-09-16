using System.Security.Cryptography.X509Certificates;
using EmbedIO;
using EmbedIO.WebApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Escl.Server;

public class EsclServer : IEsclServer
{
    static EsclServer()
    {
        Swan.Logging.Logger.NoLogging();
    }

    private readonly Dictionary<EsclDeviceConfig, DeviceContext> _devices = new();
    private bool _started;
    private CancellationTokenSource? _cts;

    public EsclSecurityPolicy SecurityPolicy { get; set; }

    public X509Certificate2? Certificate { get; set; }

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

    public async Task Start()
    {
        if (_started)
        {
            return;
        }
        if (SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerRequireHttps) &&
            SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerDisableHttps))
        {
            throw new EsclSecurityPolicyViolationException(
                $"EsclSecurityPolicy of {SecurityPolicy} is inconsistent");
        }
        if (SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerRequireTrustedCertificate) && Certificate == null)
        {
            throw new EsclSecurityPolicyViolationException(
                $"EsclSecurityPolicy of {SecurityPolicy} needs a certificate to be specified");
        }
        _started = true;
        _cts = new CancellationTokenSource();

        // Try to generate a self-signed certificate if the caller hasn't provided one
        if (!SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerDisableHttps) && Certificate == null)
        {
            await Task.Run(() => Certificate = CertificateHelper.GenerateSelfSignedCertificate(Logger));
        }
        if (SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerRequireHttps) && Certificate == null)
        {
            throw new EsclSecurityPolicyViolationException(
                $"EsclSecurityPolicy of {SecurityPolicy} needs a certificate to be specified");
        }

        var tasks = new List<Task>();
        foreach (var device in _devices.Keys)
        {
            var deviceCtx = _devices[device];
            deviceCtx.StartTask = Task.Run(() => StartServerAndAdvertise(deviceCtx));
            tasks.Add(deviceCtx.StartTask);
        }
        await Task.WhenAll(tasks);
    }

    private async Task StartServerAndAdvertise(DeviceContext deviceCtx)
    {
        var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_cts!.Token, deviceCtx.Cts.Token).Token;
        // Try to run the server with the port specified in the EsclDeviceConfig first. If that fails, try random ports
        // instead, and store the actually-used port back in EsclDeviceConfig so it can be advertised correctly.
        bool hasHttp = !SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerRequireHttps);
        bool hasHttps = !SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerDisableHttps) && Certificate != null;
        if (hasHttp)
        {
            await PortFinder.RunWithSpecifiedOrRandomPort(deviceCtx.Config.Port, async port =>
            {
                await StartServer(deviceCtx, port, false, cancelToken);
                deviceCtx.Config.Port = port;
            }, cancelToken);
        }
        if (hasHttps)
        {
            await PortFinder.RunWithSpecifiedOrRandomPort(deviceCtx.Config.TlsPort, async tlsPort =>
            {
                await StartServer(deviceCtx, tlsPort, true, cancelToken);
                deviceCtx.Config.TlsPort = tlsPort;
            }, cancelToken);
        }
        deviceCtx.Advertiser.AdvertiseDevice(deviceCtx.Config, hasHttp, hasHttps);
    }

    private async Task StartServer(DeviceContext deviceCtx, int port, bool tls, CancellationToken cancelToken)
    {
        var protocol = tls ? "https" : "http";
        var url = $"{protocol}://+:{port}/";
        deviceCtx.ServerState = new EsclServerState(Logger);
        var server = new WebServer(o => o
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(url)
                .WithCertificate((tls ? Certificate : null)!))
            .HandleUnhandledException(UnhandledServerException);
        if (SecurityPolicy.HasFlag(EsclSecurityPolicy.ServerAllowAnyOrigin))
        {
            server.WithCors();
        }
        server.WithWebApi("/eSCL",
            m => m.WithController(() =>
                new EsclApiController(deviceCtx.Config, deviceCtx.ServerState, SecurityPolicy, Logger)));
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