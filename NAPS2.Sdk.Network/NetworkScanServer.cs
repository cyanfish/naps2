using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Network.Internal;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Internal;

namespace NAPS2.Remoting.Network;

public class NetworkScanServer : IDisposable
{
    private readonly ImageContext _imageContext;
    private readonly NetworkScanServerOptions _options;
    private readonly IScanBridgeFactory _scanBridgeFactory;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private Server? _server;

    public NetworkScanServer()
        : this(ImageContext.Default, new ScanBridgeFactory(ImageContext.Default, WorkerFactory.Default), new NetworkScanServerOptions())
    {
    }

    public NetworkScanServer(NetworkScanServerOptions options)
        : this(ImageContext.Default, new ScanBridgeFactory(ImageContext.Default, WorkerFactory.Default), options)
    {
    }

    public NetworkScanServer(ImageContext imageContext, WorkerFactory workerFactory, NetworkScanServerOptions options)
        : this(imageContext, new ScanBridgeFactory(imageContext, workerFactory), options)
    {
    }

    internal NetworkScanServer(ImageContext imageContext, IScanBridgeFactory scanBridgeFactory, NetworkScanServerOptions options)
    {
        _imageContext = imageContext;
        _options = options;
        _scanBridgeFactory = scanBridgeFactory;
    }

    public void Start()
    {
        if (_server != null)
        {
            throw new InvalidOperationException("Already started");
        }

        // Start server
        // TODO: Secure
        _server = new Server
        {
            Services = { NetworkScanService.BindService(new NetworkScanServiceImpl(_imageContext, _scanBridgeFactory)) },
            Ports = { new ServerPort("0.0.0.0", _options.Port ?? ServerPort.PickUnused, ServerCredentials.Insecure) }
        };
        _server.Start();

        // Start discovery
        if (_options.AllowDiscovery)
        {
            int discoveryPort = _options.DiscoveryPort ?? Discovery.DEFAULT_DISCOVERY_PORT;
            int serverPort = _server.Ports.Single().BoundPort;
            string serverName = _options.ServerName ?? Environment.MachineName;
            Task.Run(() => Discovery.ListenForBroadcast(discoveryPort, serverPort, serverName, _cts.Token));
        }
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