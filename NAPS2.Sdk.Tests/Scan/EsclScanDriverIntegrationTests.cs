using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Remoting;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Escl;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Scan;

public class EsclScanDriverIntegrationTests(ITestOutputHelper testOutputHelper) : ContextualTests(testOutputHelper),
    IAsyncLifetime
{
    private EsclServer _server = null!;
    private EsclDeviceConfig _deviceConfig = null!;
    private EsclScanDriver _driver = null!;
    private ScanDevice _device = null!;
    private ScanOptions _options = null!;

    private EsclScanSettings? _lastScanSettings;
    private string _contentType = ContentTypes.JPEG;
    private byte[] _contentToReturn = [];

    public async Task InitializeAsync()
    {
        _server = new EsclServer();
        _deviceConfig = new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                Version = "2.6",
                MakeAndModel = "Test Scanner",
                Manufacturer = "Test Manufacturer",
                SerialNumber = "SN-12345",
                Uuid = Guid.NewGuid().ToString("D"),
                PlatenCaps = new EsclInputCaps()
            },
            CreateJob = settings =>
            {
                _lastScanSettings = settings;
                var job = Substitute.For<IEsclScanJob>();
                job.ContentType.Returns(_contentType);
                job.WaitForNextDocument(Arg.Any<CancellationToken>()).Returns(true, false);
                job.WriteDocumentTo(Arg.Any<Stream>())
                    .Returns(x => x.Arg<Stream>().WriteAsync(_contentToReturn, 0, _contentToReturn.Length));
                return job;
            }
        };
        _server.AddDevice(_deviceConfig);
        await _server.Start();

        _driver = new EsclScanDriver(ScanningContext);
        _device = new ScanDevice(Driver.Escl, $"http://127.0.0.1:{_deviceConfig.Port}/eSCL", "Test Scanner");
        _options = new ScanOptions
        {
            Device = _device,
            PageSize = PageSize.Letter
        };
    }

    public Task DisposeAsync()
    {
        _server.Dispose();
        return Task.CompletedTask;
    }

    [Fact(Timeout = 60_000)]
    public async Task GetCaps_ReturnsCapsFromRealServer()
    {
        var caps = await _driver.GetCaps(_options, CancellationToken.None);

        Assert.Equal("Test Scanner", caps.MetadataCaps!.Model);
        Assert.Equal("Test Manufacturer", caps.MetadataCaps!.Manufacturer);
        Assert.Equal("SN-12345", caps.MetadataCaps!.SerialNumber);
        Assert.Equal("2.6", caps.MetadataCaps!.ProtocolVersion);
    }

    [Theory(Timeout = 60_000)]
    [InlineData(JpegDpiHeader.Jfif, 72)]
    [InlineData(JpegDpiHeader.Exif, 72)]
    [InlineData(JpegDpiHeader.None, 300)]
    public async Task Scan_JpegResolution(JpegDpiHeader header, int expectedDpi)
    {
        _contentType = ContentTypes.JPEG;
        _contentToReturn = header switch
        {
            JpegDpiHeader.Jfif => ImageResources.dog_jfif,
            JpegDpiHeader.Exif => ImageResources.dog_exif,
            _ => ImageResources.dog_headerless
        };
        _options.Dpi = 300;

        var images = new List<IMemoryImage>();
        await _driver.Scan(_options, CancellationToken.None, ScanEvents.Stub, images.Add);

        var image = Assert.Single(images);
        Assert.Equal(expectedDpi, image.HorizontalResolution, 2);
        Assert.Equal(expectedDpi, image.VerticalResolution, 2);
    }

    public enum JpegDpiHeader
    {
        None,
        Jfif,
        Exif
    }
}
