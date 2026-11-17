using System.Net;
using System.Net.Http;
using System.Text;
using NAPS2.Escl.Server;
using NSubstitute;
using Xunit;

namespace NAPS2.Escl.Tests;

public class ScanJobTests
{
    // As sent by eSCL clients that follow the specification (hyphens in the scan namespace).
    private const string StandardSettings =
        """
        <?xml version="1.0" encoding="utf-8"?><scan:ScanSettings xmlns:scan="http://schemas.hp.com/imaging/escl/2011-05-03" xmlns:pwg="http://www.pwg.org/schemas/2010/12/sm"><pwg:Version>2.0</pwg:Version><pwg:ScanRegions><pwg:ScanRegion><pwg:Height>3507</pwg:Height><pwg:ContentRegionUnits>escl:ThreeHundredthsOfInches</pwg:ContentRegionUnits><pwg:Width>2481</pwg:Width><pwg:XOffset>0</pwg:XOffset><pwg:YOffset>0</pwg:YOffset></pwg:ScanRegion></pwg:ScanRegions><pwg:InputSource>Feeder</pwg:InputSource><scan:Duplex>true</scan:Duplex><scan:ColorMode>Grayscale8</scan:ColorMode><scan:XResolution>300</scan:XResolution><scan:YResolution>300</scan:YResolution><pwg:DocumentFormat>image/jpeg</pwg:DocumentFormat></scan:ScanSettings>
        """;

    // As sent by EsclClient, i.e. by other NAPS2 instances (slashes in the scan namespace).
    private const string Naps2Settings =
        """
        <?xml version="1.0" encoding="utf-8"?><scan:ScanSettings xmlns:scan="http://schemas.hp.com/imaging/escl/2011/05/03" xmlns:pwg="http://www.pwg.org/schemas/2010/12/sm"><pwg:Version>2.0</pwg:Version><scan:Duplex>true</scan:Duplex><scan:ColorMode>Grayscale8</scan:ColorMode><scan:XResolution>300</scan:XResolution><scan:YResolution>300</scan:YResolution><pwg:DocumentFormat>image/jpeg</pwg:DocumentFormat></scan:ScanSettings>
        """;

    [Theory]
    [InlineData(StandardSettings)]
    [InlineData(Naps2Settings)]
    public async Task CreateScanJobAccepted(string settings)
    {
        var (server, deviceConfig, _) = Setup();
        using (server)
        {
            await server.Start();
            var response = await Post(deviceConfig, settings);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }
    }

    [Fact]
    public async Task CreateScanJobParsesStandardNamespaceSettings()
    {
        var (server, deviceConfig, captured) = Setup();
        using (server)
        {
            await server.Start();
            await Post(deviceConfig, StandardSettings);
            Assert.NotNull(captured());
            var settings = captured()!;
            Assert.Equal(EsclColorMode.Grayscale8, settings.ColorMode);
            Assert.Equal(300, settings.XResolution);
            Assert.Equal(300, settings.YResolution);
            Assert.True(settings.Duplex);
            Assert.Equal(EsclInputSource.Feeder, settings.InputSource);
            Assert.Equal("image/jpeg", settings.DocumentFormat);
            Assert.Equal(2481, settings.Width);
            Assert.Equal(3507, settings.Height);
        }
    }

    [Fact]
    public async Task CreateScanJobRejectsInvalidSettings()
    {
        var (server, deviceConfig, _) = Setup();
        using (server)
        {
            await server.Start();
            var response = await Post(deviceConfig,
                "<scan:ScanSettings xmlns:scan=\"http://example.com/unrelated\"/>");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    private static (EsclServer server, EsclDeviceConfig deviceConfig, Func<EsclScanSettings?> captured) Setup()
    {
        EsclScanSettings? settings = null;
        var job = Substitute.For<IEsclScanJob>();
        var server = new EsclServer();
        var deviceConfig = new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.0",
                MakeAndModel = "HP Blah",
                SerialNumber = "123abc",
                Uuid = Guid.NewGuid().ToString("D")
            },
            CreateJob = received =>
            {
                settings = received;
                return job;
            }
        };
        server.AddDevice(deviceConfig);
        return (server, deviceConfig, () => settings);
    }

    private static async Task<HttpResponseMessage> Post(EsclDeviceConfig deviceConfig, string settings)
    {
        using var httpClient = new HttpClient();
        return await httpClient.PostAsync(
            $"http://{IPAddress.Loopback}:{deviceConfig.Port}/eSCL/ScanJobs",
            new StringContent(settings, Encoding.UTF8, "text/xml"));
    }
}
