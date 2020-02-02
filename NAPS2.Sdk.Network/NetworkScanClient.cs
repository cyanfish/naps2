using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NAPS2.Remoting.Network
{
    public class NetworkScanClient
    {
        public async Task<IEnumerable<DiscoveredServer>> DiscoverServers(int timeout = 5000, CancellationToken cancellationToken = default)
        {
            var servers = new List<DiscoveredServer>();
            await DiscoverServers(servers.Add, timeout, cancellationToken);
            return servers;
        }
        
        public async Task DiscoverServers(Action<DiscoveredServer> callback, int timeout = 5000, CancellationToken cancellationToken = default)
        {
            var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(timeout);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            // TODO
        }
    }

    public class DiscoveredServer
    {
        internal DiscoveredServer(string name, IPEndPoint ipEndPoint)
        {
            Name = name;
            IPEndPoint = ipEndPoint;
        }

        public string Name { get; }
        
        public IPEndPoint IPEndPoint { get; }
    }
}
