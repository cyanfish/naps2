using System.Threading;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Mocks;
using NSubstitute;
using Xunit;
using IScanDriver = NAPS2.Scan.Internal.IScanDriver;
using IScanDriverFactory = NAPS2.Scan.Internal.IScanDriverFactory;

namespace NAPS2.Sdk.Tests.Scan;

public class RemoteScanControllerTests : ContextualTests
{
    [Fact]
    public async Task GetDevices()
    {
        var scanDriver = Substitute.For<IScanDriver>();
        scanDriver.GetDevices(Arg.Any<ScanOptions>(), Arg.Any<CancellationToken>(), Arg.Any<Action<ScanDevice>>())
            .Returns(x =>
            {
                var options = (ScanOptions) x[0];
                var callback = (Action<ScanDevice>) x[2];
                callback(new ScanDevice(options.Driver, "test_id1", "test_name1"));
                callback(new ScanDevice(options.Driver, "WIA-test_id2", "test_name2"));
                return Task.CompletedTask;
            });
        var controller = CreateControllerWithMockDriver(scanDriver);

        var deviceList = new List<ScanDevice>();
        await controller.GetDevices(new ScanOptions { Driver = Driver.Wia }, CancellationToken.None, deviceList.Add);
        Assert.Equal(2, deviceList.Count);
        Assert.Equal("test_id1", deviceList[0].ID);
        Assert.Equal("WIA-test_id2", deviceList[1].ID);

        deviceList.Clear();
        await controller.GetDevices(new ScanOptions { Driver = Driver.Twain }, CancellationToken.None, deviceList.Add);
        Assert.Single(deviceList);
        Assert.Equal("test_id1", deviceList[0].ID);

        deviceList.Clear();
        await controller.GetDevices(
            new ScanOptions { Driver = Driver.Twain, TwainOptions = { IncludeWiaDevices = true } },
            CancellationToken.None,
            deviceList.Add);
        Assert.Equal(2, deviceList.Count);
    }

    [Fact]
    public async Task ScanAndDeskew()
    {
        var scanDriver = Substitute.For<IScanDriver>();
        scanDriver.Scan(Arg.Any<ScanOptions>(), Arg.Any<CancellationToken>(), Arg.Any<IScanEvents>(),
            Arg.Any<Action<IMemoryImage>>()).ReturnsForAnyArgs(
            x =>
            {
                var callback = (Action<IMemoryImage>) x[3];
                var image = LoadImage(ImageResources.skewed);
                callback(image);
                return Task.FromResult(true);
            });
        var controller = CreateControllerWithMockDriver(scanDriver);

        var images = new List<ProcessedImage>();

        void Callback(ProcessedImage image, PostProcessingContext context)
        {
            images.Add(image);
        }

        var options = new ScanOptions
        {
            AutoDeskew = true
        };
        await controller.Scan(options, CancellationToken.None, ScanEvents.Stub, Callback);

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.deskewed, images[0], ImageAsserts.XL_RMSE_THRESHOLD);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BackgroundOcrAfterDuplexFlip(bool fileStorage)
    {
        if (fileStorage)
        {
            SetUpFileStorage();
        }
        SetUpOcr();
        using var backSide = LoadImage(ImageResources.ocr_test).PerformTransform(new RotationTransform(180));
        using var backSideData = backSide.SaveToMemoryStream(ImageFileFormat.Png);
        var factory = new ScanDriverFactoryBuilder()
            .WithScannedImages(ImageResources.blank1, backSideData.ToArray())
            .Build();
        var remoteController = new RemoteScanController(factory, new RemotePostProcessor(ScanningContext));
        var ocrController = new OcrController(ScanningContext);
        var ocrTasks = new List<Task<OcrResult>>();
        ocrController.OcrStarted += (_, args) => ocrTasks.Add(args.ResultTask);
        var controller = new ScanController(ScanningContext,
            new LocalPostProcessor(ScanningContext, ocrController), new ScanOptionsValidator(),
            new InProcScanBridgeFactory(new InProcScanBridge(ScanningContext, remoteController)));
        var options = new ScanOptions
        {
            Device = new ScanDevice(Driver.Escl, "test_id", "test_name"),
            PaperSource = PaperSource.Duplex,
            FlipDuplexedPages = true,
            ExcludeBlankPages = true,
            OcrParams = new OcrParams("eng", OcrMode.Fast, 60)
        };

        var images = await controller.Scan(options).ToListAsync();

        using var image = Assert.Single(images);
        Assert.Equal(2, image.PostProcessingData.PageNumber);
        Assert.Equal(PageSide.Back, image.PostProcessingData.PageSide);
        var ocrResult = await Assert.Single(ocrTasks);
        Assert.NotNull(ocrResult);
        Assert.Equal("ADVERTISEMENT.", ocrResult.Words[0].Text);
        Assert.True(ScanningContext.OcrRequestQueue.HasCachedResult(ScanningContext.OcrEngine!, image, options.OcrParams));
        var pdfPath = P("duplex.pdf");
        await new PdfExporter(ScanningContext).Export(pdfPath, [image], ocrParams: options.OcrParams);
        PdfAsserts.AssertContainsTextOnce("ADVERTISEMENT.", pdfPath);
    }

    private RemoteScanController CreateControllerWithMockDriver(IScanDriver scanDriver)
    {
        var scanDriverFactory = Substitute.For<IScanDriverFactory>();
        scanDriverFactory.Create(Arg.Any<ScanOptions>()).Returns(scanDriver);
        var controller = new RemoteScanController(scanDriverFactory, new RemotePostProcessor(ScanningContext));
        return controller;
    }
}
