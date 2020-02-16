using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Remoting.Network.Internal;

namespace NAPS2.Remoting.Network
{
    public class NetworkScanClient
    {
        private readonly NetworkScanClientOptions _options;

        public NetworkScanClient()
            : this(new NetworkScanClientOptions())
        {
        }
        
        public NetworkScanClient(NetworkScanClientOptions options)
        {
            _options = options;
        }
        
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
            
            var discoveryPort = _options.DiscoveryPort ?? Discovery.DEFAULT_DISCOVERY_PORT;
            await Discovery.SendBroadcast(discoveryPort, callback, timeout, cts.Token);
        }
    }
}
