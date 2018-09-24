using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NAPS2.Util;

namespace NAPS2.ClientServer
{
    public class ServerDiscovery
    {
        private const int DISCOVERY_PORT = 33277;
        private static readonly byte[] MagicBroadcastBytes = { 0x7f, 0x87, 0x00, 0x8b, 0x08, 0x87, 0x5d, 0xd3, 0x64, 0x1a };
        private static readonly byte[] MagicResponseBytes = { 0xf4, 0x38, 0xb9, 0xa3, 0xf7, 0x37, 0xaf, 0x35, 0x41, 0xc7 };

        public static void ListenForBroadcast(int serverPort)
        {
            var fallback = new ExpFallback(100, 60 * 1000);
            var udpClient = new UdpClient(DISCOVERY_PORT) { Client = { ReceiveTimeout = 0 } };
            IPEndPoint remoteEndpoint = null;
            while (true)
            {
                try
                {
                    var receivedBytes = udpClient.Receive(ref remoteEndpoint);
                    if (receivedBytes.SequenceEqual(MagicBroadcastBytes))
                    {
                        var responseBytes = MagicResponseBytes
                            .Concat(BitConverter.GetBytes(serverPort))
                            .Concat(Encoding.UTF8.GetBytes(Environment.MachineName))
                            .ToArray();
                        udpClient.Send(responseBytes, responseBytes.Length, remoteEndpoint);
                    }
                }
                catch (SocketException)
                {
                    Thread.Sleep(fallback.Value);
                    fallback.Increase();
                }
            }
        }

        public static void SendBroadcast(Action<string, IPEndPoint> callback)
        {
            var udpClient = new UdpClient { Client = { ReceiveTimeout = 1000 } };
            udpClient.Send(MagicBroadcastBytes, MagicBroadcastBytes.Length, new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT));

            var fallback = new ExpFallback(100, 60 * 1000);
            IPEndPoint remoteEndpoint = null;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                try
                {
                    var receivedBytes = udpClient.Receive(ref remoteEndpoint);
                    int portIndex = MagicResponseBytes.Length;
                    int nameIndex = portIndex + 4;
                    if (receivedBytes.Length >= nameIndex &&
                        receivedBytes.Take(portIndex).SequenceEqual(MagicResponseBytes))
                    {
                        int port = BitConverter.ToInt32(receivedBytes, portIndex);
                        string computerName = "";
                        try
                        {
                            computerName = Encoding.UTF8.GetString(receivedBytes, nameIndex, receivedBytes.Length - nameIndex);
                        }
                        catch (ArgumentException)
                        {
                        }
                        callback(computerName, new IPEndPoint(remoteEndpoint.Address, port));
                    }
                }
                catch (SocketException)
                {
                    Thread.Sleep(fallback.Value);
                    fallback.Increase();
                }
                if (stopwatch.ElapsedMilliseconds > 10000)
                {
                    return;
                }
            }
        }
    }
}
