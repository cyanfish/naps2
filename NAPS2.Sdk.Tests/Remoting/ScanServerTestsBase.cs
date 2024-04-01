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
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class ScanServerTestsBase : ContextualTests
{
    protected readonly ScanServer _server;
    private protected readonly MockScanBridge _bridge;
    protected readonly ScanController _client;
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
        _server.ScanController = new ScanController(ScanningContext, scanBridgeFactory);
        _server.SecurityPolicy = securityPolicy;
        _server.Certificate = certificate;

        // Initialize the server with a single device with a unique ID for the test
        var displayName = $"testName-{Guid.NewGuid()}";
        ScanningContext.Logger.LogDebug("Display name: {Name}", displayName);
        var serverDevice = new ScanDevice(ScanOptionsValidator.SystemDefaultDriver, "testID", "testName");
        _server.RegisterDevice(serverDevice, displayName);
        _server.Start().Wait();

        // Set up a client ScanController for scanning through EsclScanDriver -> network -> ScanServer
        _client = new ScanController(ScanningContext);
        var uuid = new ScanServerDevice { Device = serverDevice, Name = displayName }.GetUuid(_server.InstanceId);
        _clientDevice = new ScanDevice(Driver.Escl, uuid, displayName);
    }

    public override void Dispose()
    {
        _server.Stop().Wait();
        base.Dispose();
    }

    protected async Task<bool> TryFindClientDevice()
    {
        var cts = new CancellationTokenSource();
        // The device name is suffixed with the IP so we just check the prefix matches
        bool found = await _client.GetDevices(Driver.Escl, cts.Token)
            .AnyAsync(device => device.Name.StartsWith(_clientDevice.Name) && device.ID == _clientDevice.ID);
        cts.Cancel();
        return found;
    }
}