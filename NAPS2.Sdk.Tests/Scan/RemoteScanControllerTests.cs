using System.Threading;
using Moq;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Images;
using Xunit;
using IScanDriver = NAPS2.Scan.Internal.IScanDriver;
using IScanDriverFactory = NAPS2.Scan.Internal.IScanDriverFactory;

namespace NAPS2.Sdk.Tests.Scan;

public class RemoteScanControllerTests : ContextualTests
{
    [Fact]
    public async Task GetDeviceList()
    {
        var device = new ScanDevice("test_id1", "test_name1");
        var wiaDevice = new ScanDevice("WIA-test_id2", "test_name2");
        var scanDriver = new Mock<IScanDriver>();
        scanDriver.Setup(x => x.GetDeviceList(It.IsAny<ScanOptions>()))
            .ReturnsAsync(new List<ScanDevice> { device, wiaDevice });
        var controller = CreateControllerWithMockDriver(scanDriver.Object);

        var deviceList = await controller.GetDeviceList(new ScanOptions { Driver = Driver.Wia });
        Assert.Equal(2, deviceList.Count);
        Assert.Equal("test_id1", deviceList[0].ID);
        Assert.Equal("WIA-test_id2", deviceList[1].ID);

        deviceList = await controller.GetDeviceList(new ScanOptions { Driver = Driver.Twain });
        Assert.Single(deviceList);
        Assert.Equal("test_id1", deviceList[0].ID);

        deviceList = await controller.GetDeviceList(new ScanOptions
            { Driver = Driver.Twain, TwainOptions = { IncludeWiaDevices = true } });
        Assert.Equal(2, deviceList.Count);
    }

    [Fact]
    public async Task ScanAndDeskew()
    {
        var scanDriver = new Mock<IScanDriver>();
        scanDriver.Setup(x => x.Scan(It.IsAny<ScanOptions>(), It.IsAny<CancellationToken>(), It.IsAny<IScanEvents>(),
            It.IsAny<Action<IMemoryImage>>())).Returns(new InvocationFunc(
            ctx =>
            {
                var callback = (Action<IMemoryImage>) ctx.Arguments[3];
                var image = new GdiImage(ImageResources.skewed);
                callback(image);
                return Task.FromResult(true);
            }));
        var controller = CreateControllerWithMockDriver(scanDriver.Object);

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
        ImageAsserts.Similar(ImageResources.deskewed, images[0]);
    }

    private RemoteScanController CreateControllerWithMockDriver(IScanDriver scanDriver)
    {
        var scanDriverFactory = new Mock<IScanDriverFactory>();
        scanDriverFactory.Setup(x => x.Create(It.IsAny<ScanOptions>())).Returns(scanDriver);
        var controller = new RemoteScanController(scanDriverFactory.Object, new RemotePostProcessor(ScanningContext));
        return controller;
    }
}