using System.Security.Cryptography.X509Certificates;
using NAPS2.Escl;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class ManualIpScanServerTests(ITestOutputHelper testOutputHelper) : ScanServerTestsBase(testOutputHelper,
    EsclSecurityPolicy.None, X509CertificateLoader.LoadPkcs12(BinaryResources.testcert, null))
{
    [Fact(Timeout = TIMEOUT)]
    public async Task ScanHttpIpv4()
    {
        var httpPort = _server.GetDevicePorts(_serverDevice, _serverDisplayName).port;
        var httpDevice = new ScanDevice(Driver.Escl, $"http://127.0.0.1:{httpPort}/eSCL", _serverDisplayName);

        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        var images = await _client.Scan(new ScanOptions
        {
            Device = httpDevice
        }).ToListAsync();
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact(Timeout = TIMEOUT)]
    public async Task ScanHttpsIpv4()
    {
        var httpsPort = _server.GetDevicePorts(_serverDevice, _serverDisplayName).tlsPort;
        var httpsDevice = new ScanDevice(Driver.Escl, $"https://127.0.0.1:{httpsPort}/eSCL", _serverDisplayName);

        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        var images = await _client.Scan(new ScanOptions
        {
            Device = httpsDevice
        }).ToListAsync();
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }
}