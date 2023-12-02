using System.Threading;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Asserts;
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
        ImageAsserts.Similar(ImageResources.deskewed, images[0], ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    private RemoteScanController CreateControllerWithMockDriver(IScanDriver scanDriver)
    {
        var scanDriverFactory = Substitute.For<IScanDriverFactory>();
        scanDriverFactory.Create(Arg.Any<ScanOptions>()).Returns(scanDriver);
        var controller = new RemoteScanController(scanDriverFactory, new RemotePostProcessor(ScanningContext));
        return controller;
    }
}