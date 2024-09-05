using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NAPS2.Escl;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Remoting;

public class FallbackScanServerTests(ITestOutputHelper testOutputHelper) : ScanServerTestsBase(testOutputHelper,
    EsclSecurityPolicy.None, X509CertificateLoader.LoadPkcs12(BinaryResources.testcert, null))
{
    [Fact(Timeout = TIMEOUT)]
    public async Task ScanFallbackFromHttpsToHttp()
    {
        _bridge.MockOutput = CreateScannedImages(ImageResources.dog);
        var images = await _client.Scan(new ScanOptions
        {
            Device = _clientDevice,
            EsclOptions =
            {
                // This policy makes sure HTTPS will fail due to an untrusted certificate, which simulates the case
                // where we're failing due to the server only supporting obsolete TLS versions.
                SecurityPolicy = EsclSecurityPolicy.ClientRequireTrustedCertificate
            }
        }).ToListAsync();
        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact(Timeout = TIMEOUT)]
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