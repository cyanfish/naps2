using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using Xunit;
using IScanDriver = NAPS2.Scan.Internal.IScanDriver;
using IScanDriverFactory = NAPS2.Scan.Internal.IScanDriverFactory;

namespace NAPS2.Sdk.Tests.Scan;

public class RemoteScanControllerTests : ContextualTexts
{
    [Fact]
    public async Task GetDeviceList()
    {
        var device = new ScanDevice("test_id1", "test_name1");
        var wiaDevice = new ScanDevice("WIA-test_id2", "test_name2");
        var scanDriver = new Mock<IScanDriver>();
        scanDriver.Setup(x => x.GetDeviceList(It.IsAny<ScanOptions>())).ReturnsAsync(new List<ScanDevice> { device, wiaDevice });
        var scanDriverFactory = new Mock<IScanDriverFactory>();
        scanDriverFactory.Setup(x => x.Create(It.IsAny<ScanOptions>())).Returns(scanDriver.Object);
        var controller = new RemoteScanController(scanDriverFactory.Object, new RemotePostProcessor(ImageContext));

        var deviceList = await controller.GetDeviceList(new ScanOptions { Driver = Driver.Wia });
        Assert.Equal(2, deviceList.Count);
        Assert.Equal("test_id1", deviceList[0].ID);
        Assert.Equal("WIA-test_id2", deviceList[1].ID);

        deviceList = await controller.GetDeviceList(new ScanOptions { Driver = Driver.Twain });
        Assert.Single(deviceList);
        Assert.Equal("test_id1", deviceList[0].ID);

        deviceList = await controller.GetDeviceList(new ScanOptions { Driver = Driver.Twain, TwainOptions = { IncludeWiaDevices = true } });
        Assert.Equal(2, deviceList.Count);
    }
}