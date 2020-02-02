using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Remoting.Network.Internal;

namespace NAPS2.Remoting.Network
{
    public class NetworkScanServer : IDisposable
    {
        private readonly NetworkScanServerOptions options;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Server server;

        public NetworkScanServer()
            : this(new NetworkScanServerOptions())
        {
        }

        public NetworkScanServer(NetworkScanServerOptions options)
        {
            this.options = options;
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
                Services = { NetworkScanService.BindService(new NetworkScanServiceImpl()) },
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