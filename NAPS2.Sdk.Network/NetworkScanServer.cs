using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Network.Internal;
using NAPS2.Scan.Internal;

namespace NAPS2.Remoting.Network
{
    public class NetworkScanServer : IDisposable
    {
        private readonly ImageContext imageContext;
        private readonly NetworkScanServerOptions options;
        private readonly IRemoteScanController remoteScanController;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Server server;

        public NetworkScanServer()
            : this(ImageContext.Default, new NetworkScanServerOptions(), new RemoteScanController())
        {
        }

        public NetworkScanServer(NetworkScanServerOptions options)
            : this(ImageContext.Default, options, new RemoteScanController())
        {
        }

        public NetworkScanServer(ImageContext imageContext, NetworkScanServerOptions options)
            : this(imageContext, options, new RemoteScanController(new ScanDriverFactory(imageContext), new RemotePostProcessor(imageContext, new ThresholdBlankDetector())))
        {
        }

        internal NetworkScanServer(ImageContext imageContext, NetworkScanServerOptions options, IRemoteScanController remoteScanController)
        {
            this.imageContext = imageContext;
            this.options = options;
            this.remoteScanController = remoteScanController;
        }

        public void Start()
        {
            if (server != null)
            {
                throw new InvalidOperationException("Already started");
            }

            // Start server
            // TODO: Secure
            server = new Server
            {
                Services = { NetworkScanService.BindService(new NetworkScanServiceImpl(imageContext, remoteScanController)) },
                Ports = { new ServerPort("0.0.0.0", options.Port ?? ServerPort.PickUnused, ServerCredentials.Insecure) }
            };
            server.Start();

            // Start discovery
            if (options.AllowDiscovery)
            {
                int discoveryPort = options.DiscoveryPort ?? Discovery.DEFAULT_DISCOVERY_PORT;
                int serverPort = server.Ports.Single().BoundPort;
                string serverName = options.ServerName ?? Environment.MachineName;
                Task.Run(() => Discovery.ListenForBroadcast(discoveryPort, serverPort, serverName, cts.Token));
            }
        }

        public void Kill()
        {
            Dispose();
        }

        public void Dispose()
        {
            server?.ShutdownAsync().Wait();
        }
    }
}