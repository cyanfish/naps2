using Moq;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Mocks;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class ScanErrorHandling : ContextualTexts
{
    [Fact]
    public void Scan_InvalidOptions()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);

        var invalidOptions = new ScanOptions { Dpi = -1 };
        Assert.Throws<ArgumentException>(() => controller.Scan(invalidOptions));
    }
        
    [Fact]
    public async Task GetDeviceList_InvalidOptions()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);

        var invalidOptions = new ScanOptions { Dpi = -1 };
        await Assert.ThrowsAsync<ArgumentException>(() => controller.GetDeviceList(invalidOptions));
    }

    [Fact]
    public async void Scan_CreateScanBridge()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
        bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Throws<InvalidOperationException>();
        var source = controller.Scan(new ScanOptions());
        await Assert.ThrowsAsync<InvalidOperationException>(source.ToList);
    }

    [Fact]
    public async Task GetDeviceList_CreateScanBridge()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
        bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Throws<InvalidOperationException>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetDeviceList(new ScanOptions()));
    }

    [Fact]
    public async Task GetDeviceList_BridgeGetDeviceList()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridge = new StubScanBridge { Error = new InvalidOperationException() };
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
        bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Returns(bridge);
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetDeviceList(new ScanOptions()));
    }

    [Fact]
    public async void Scan_LocalPostProcess()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridge = new StubScanBridge { MockOutput = new List<ScannedImage> { CreateScannedImage() } };
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
        bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Returns(bridge);
        localPostProcessor.Setup(pp => pp.PostProcess(It.IsAny<ScannedImage>(), It.IsAny<ScanOptions>(), It.IsAny<PostProcessingContext>()))
            .Throws<InvalidOperationException>();
        var source = controller.Scan(new ScanOptions());
        await Assert.ThrowsAsync<InvalidOperationException>(source.ToList);
    }

    [Fact]
    public async void Scan_BridgeScan()
    {
        var localPostProcessor = new Mock<ILocalPostProcessor>();
        var bridge = new StubScanBridge { Error = new InvalidOperationException() };
        var bridgeFactory = new Mock<IScanBridgeFactory>();
        var controller = new ScanController(localPostProcessor.Object, new ScanOptionsValidator(), bridgeFactory.Object);
            
        bridgeFactory.Setup(factory => factory.Create(It.IsAny<ScanOptions>())).Returns(bridge);
        var source = controller.Scan(new ScanOptions());
        await Assert.ThrowsAsync<InvalidOperationException>(source.ToList);
    }
        
    // TODO: Add some testing that exceptions are wrapped up in ScanDriverUnknownException where appropriate (always? only from driver itself? is it really needed?)
    // TODO: I guess the point is that you get the nice-ish "An error occurred with the scanning driver" instead of some inscrutable message. But is that more of a UI thing?
}