using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Worker;
using Xunit;

namespace NAPS2.Remoting.Network.Tests
{
    public class ServerDiscoveryTests
    {
        [Fact]
        public async Task ServerDiscovery()
        {
            using var server = CreateServer(new NetworkScanServerOptions
            {
                ServerName = "NetworkScanTests.ServerDiscovery"
            });
            var client = new NetworkScanClient();
            server.Start();
            var discovered = await client.DiscoverServers(100);
            Assert.Contains(discovered, x => x.Name == "NetworkScanTests.ServerDiscovery");
        }
        
        [Fact]
        public async Task ServerDiscoveryCustomPort()
        {
            using var server = CreateServer(new NetworkScanServerOptions
            {
                ServerName = "NetworkScanTests.ServerDiscoveryCustomPort",
                DiscoveryPort = 33433
            });
            var client = new NetworkScanClient(new NetworkScanClientOptions
            {
                DiscoveryPort = 33433
            });
            server.Start();
            var discovered = await client.DiscoverServers(100);
            Assert.Contains(discovered, x => x.Name == "NetworkScanTests.ServerDiscoveryCustomPort");
        }
        
        [Fact]
        public async Task ServerDiscoveryMismatchPort()
        {
            using var server = CreateServer(new NetworkScanServerOptions
            {
                ServerName = "NetworkScanTests.ServerDiscoveryMismatchPort",
                DiscoveryPort = 33444
            });
            var client = new NetworkScanClient(new NetworkScanClientOptions
            {
                DiscoveryPort = 33555
            });
            server.Start();
            var discovered = await client.DiscoverServers(100);
            Assert.DoesNotContain(discovered, x => x.Name == "NetworkScanTests.ServerDiscoveryMismatchPort");
        }
        
        [Fact]
        public async Task ServerDiscoveryOff()
        {
            using var server = CreateServer(new NetworkScanServerOptions
            {
                ServerName = "NetworkScanTests.ServerDiscoveryOff",
                AllowDiscovery = false
            });
            var client = new NetworkScanClient();
            server.Start();
            var discovered = await client.DiscoverServers(100);
            Assert.DoesNotContain(discovered, x => x.Name == "NetworkScanTests.ServerDiscoveryOff");
        }

        private NetworkScanServer CreateServer(NetworkScanServerOptions options)
        {
            var imageContext = new GdiImageContext();
            return new NetworkScanServer(imageContext, new WorkerFactory(imageContext), options);
        }
    }
}