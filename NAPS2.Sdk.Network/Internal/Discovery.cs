using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Remoting.Network.Internal;
using NAPS2.Util;

namespace NAPS2.Remoting.Network.Internal
{
    internal static class Discovery
    {
        public const int DEFAULT_DISCOVERY_PORT = 33277;
        
        private static readonly byte[] MagicBroadcastBytes = { 0x7f, 0x87, 0x00, 0x8b, 0x08, 0x87, 0x5d, 0xd3, 0x64, 0x1a };
        private static readonly byte[] MagicResponseBytes = { 0xf4, 0x38, 0xb9, 0xa3, 0xf7, 0x37, 0xaf, 0x35, 0x41, 0xc7 };

        public static async Task ListenForBroadcast(int discoveryPort, int serverPort, string serverName,
            CancellationToken cancellationToken)
        {
            var udpClient = new UdpClient(discoveryPort);
            using var cancelReg = cancellationToken.Register(() => udpClient.Dispose());
            
            var fallback = new ExpFallback(100, 60 * 1000);
            while (true)
            {
                try
                {
                    var response = await udpClient.ReceiveAsync();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    if (response.Buffer.SequenceEqual(MagicBroadcastBytes))
                    {
                        var responseBytes = MagicResponseBytes
                            .Concat(BitConverter.GetBytes(serverPort))
                            .Concat(Encoding.UTF8.GetBytes(serverName))
                            .ToArray();
                        udpClient.Send(responseBytes, responseBytes.Length, response.RemoteEndPoint);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    await fallback.DelayTask(cancellationToken);
                    fallback.Increase();
                }
            }
        }

        public static async Task SendBroadcast(int discoveryPort, Action<DiscoveredServer> callback,
            int timeout, CancellationToken cancellationToken)
        {
            var udpClient = new UdpClient();
            using var cancelReg = cancellationToken.Register(() => udpClient.Dispose());

            try
            {
                var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                await udpClient.SendAsync(MagicBroadcastBytes, MagicBroadcastBytes.Length, broadcastEndpoint);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            var fallback = new ExpFallback(50, timeout / 2);
            while (true)
            {
                try
                {
                    var response = await udpClient.ReceiveAsync();
                    int portIndex = MagicResponseBytes.Length;
                    int nameIndex = portIndex + 4;
                    if (response.Buffer.Length >= nameIndex &&
                        response.Buffer.Take(portIndex).SequenceEqual(MagicResponseBytes))
                    {
                        int port = BitConverter.ToInt32(response.Buffer, portIndex);
                        string serverName = "";
                        try
                        {
                            serverName = Encoding.UTF8.GetString(response.Buffer, nameIndex,
                                response.Buffer.Length - nameIndex);
                        }
                        catch (ArgumentException)
                        {
                        }

                        var endpoint = new IPEndPoint(response.RemoteEndPoint.Address, port);
                        callback(new DiscoveredServer(serverName, endpoint));
                    }
                    fallback.Reset();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    await fallback.DelayTask(cancellationToken);
                    fallback.Increase();
                }
            }
        }
    }
}
