using System.Threading;
using Grpc.Core;
using NAPS2.Remoting.Network.Internal;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Remoting.Network;

public class NetworkScanServer : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly NetworkScanServerOptions _options;
    private readonly IScanBridgeFactory _scanBridgeFactory;
    private readonly CancellationTokenSource _cts = new();
    private Server? _server;

    public NetworkScanServer(ScanningContext scanningContext)
        : this(scanningContext, new NetworkScanServerOptions())
    {
    }

    public NetworkScanServer(ScanningContext scanningContext, NetworkScanServerOptions options)
        : this(scanningContext, new ScanBridgeFactory(scanningContext), options)
    {
    }

    internal NetworkScanServer(ScanningContext scanningContext, IScanBridgeFactory scanBridgeFactory,
        NetworkScanServerOptions options)
    {
        _scanningContext = scanningContext;
        _options = options;
        _scanBridgeFactory = scanBridgeFactory;
    }

    public Task Start()
    {
        if (_server != null)
        {
            throw new InvalidOperationException("Already started");
        }

        // Start server
        // TODO: Secure
        _server = new Server
        {
            Services =
            {
                NetworkScanService.BindService(new NetworkScanServiceImpl(_scanningContext.ImageContext,
                    _scanBridgeFactory))
            },
            Ports = { new ServerPort("0.0.0.0", _options.Port ?? ServerPort.PickUnused, ServerCredentials.Insecure) }
        };
        _server.Start();

        // Start discovery
        if (_options.AllowDiscovery)
        {
            int discoveryPort = _options.DiscoveryPort ?? Discovery.DEFAULT_DISCOVERY_PORT;
            int serverPort = _server.Ports.Single().BoundPort;
            string serverName = _options.ServerName ?? Environment.MachineName;

            var tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                // We only want Start to resolve once this task starts running. Otherwise discovery might not yet be
                // active which could cause test failures. Of course there's still a few ms until the actual UDP socket
                // receive starts, but that shouldn't generally be an issue. We could also consider tying this to
                // UdpClient.BeginReceive resolving though that's more complicated.
                tcs.TrySetResult(true);
                return Discovery.ListenForBroadcast(discoveryPort, serverPort, serverName, _cts.Token);
            });
            return tcs.Task;
        }
        return Task.CompletedTask;
    }

    public void Kill()
    {
        Dispose();
    }

    public void Dispose()
    {
        _server?.ShutdownAsync().Wait();
    }
}