using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Mocks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class ScanServerTestsBase : ContextualTests, IAsyncLifetime
{
    protected const int TIMEOUT = 60_000;

    protected readonly ScanServer _server;
    protected readonly ScanDevice _serverDevice;
    protected readonly string _serverDisplayName;
    private protected readonly MockScanBridge _bridge;
    protected readonly ScanController _client;
    protected readonly string _uuid;
    protected readonly ScanDevice _clientDevice;

    public ScanServerTestsBase(ITestOutputHelper testOutputHelper,
        EsclSecurityPolicy securityPolicy = EsclSecurityPolicy.None,
        X509Certificate2 certificate = null) : base(testOutputHelper)
    {
        _server = new ScanServer(ScanningContext, new EsclServer());

        // Set up a server connecting to a mock scan backend
        _bridge = new MockScanBridge();
        var scanBridgeFactory = Substitute.For<IScanBridgeFactory>();
        scanBridgeFactory.Create(Arg.Any<ScanOptions>()).Returns(_bridge);
        _server.ScanControllerFactory = () => new ScanController(ScanningContext, scanBridgeFactory);
        _server.SecurityPolicy = securityPolicy;
        _server.Certificate = certificate;

        // Initialize the server with a single device with a unique ID for the test
        _serverDisplayName = $"testName-{Guid.NewGuid()}";
        ScanningContext.Logger.LogDebug("Display name: {Name}", _serverDisplayName);
        _serverDevice = new ScanDevice(ScanOptionsValidator.SystemDefaultDriver, "testID", "testName");
        _server.RegisterDevice(_serverDevice, _serverDisplayName);

        // Set up a client ScanController for scanning through EsclScanDriver -> network -> ScanServer
        _client = new ScanController(ScanningContext);
        _uuid = new ScanServerDevice { Device = _serverDevice, Name = _serverDisplayName }.GetUuid(_server.InstanceId);
        _clientDevice = new ScanDevice(Driver.Escl, _uuid, _serverDisplayName);
    }

    public Task InitializeAsync() => _server.Start();

    public Task DisposeAsync() => _server.Stop();

    protected async Task<bool> TryFindClientDevice()
    {
        var cts = new CancellationTokenSource();
        // The device name is suffixed with the IP so we just check the prefix matches
        bool found = await _client.GetDevices(Driver.Escl, cts.Token)
            .AnyAsync(device => device.Name.StartsWith(_clientDevice.Name) && device.ID == _clientDevice.ID);
        cts.Cancel();
        return found;
    }

    protected void UseServerPort(int port)
    {
        _server.UnregisterDevice(_serverDevice, _serverDisplayName);
        _server.RegisterDevice(_serverDevice, _serverDisplayName, port);
    }
}