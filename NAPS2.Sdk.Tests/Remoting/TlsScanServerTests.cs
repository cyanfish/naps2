using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NAPS2.Escl;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class TlsScanServerTests(ITestOutputHelper testOutputHelper) : ScanServerTestsBase(testOutputHelper,
    EsclSecurityPolicy.RequireHttps, new X509Certificate2(BinaryResources.testcert))
{
    [Fact]
    public async Task FindDevice()
    {
        var devices = await _client.GetDeviceList(Driver.Escl);
        // The device name is suffixed with the IP so we just check the prefix matches
        Assert.Contains(devices,
            device => device.Name.StartsWith(_clientDevice.Name) && device.ID == _clientDevice.ID);
    }

    [Fact]
    public async Task Scan()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        var images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            EsclOptions =
            {
                SecurityPolicy = EsclSecurityPolicy.RequireHttps
            }
        }).ToListAsync();
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public async Task ScanPreventedByTrustedCertificateSecurityPolicy()
    {
        var scanResult = _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            EsclOptions =
            {
                SecurityPolicy = EsclSecurityPolicy.RequireTrustedCertificate
            }
        });
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () => await scanResult.ToListAsync());
        Assert.True(exception.InnerException is AuthenticationException ||
                    exception.InnerException?.InnerException is AuthenticationException);
    }
}