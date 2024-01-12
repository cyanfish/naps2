using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests.Mocks;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class ScanErrorHandling : ContextualTests
{
    [Fact]
    public void Scan_InvalidOptions()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        var invalidOptions = new ScanOptions { Dpi = -1 };
        Assert.Throws<ArgumentException>(() => controller.Scan(invalidOptions));
    }

    [Fact]
    public async Task GetDeviceList_InvalidOptions()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        var invalidOptions = new ScanOptions { Dpi = -1 };
        await Assert.ThrowsAsync<ArgumentException>(() => controller.GetDeviceList(invalidOptions));
    }

    [Fact]
    public async void Scan_CreateScanBridge()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        bridgeFactory.When(factory => factory.Create(Arg.Any<ScanOptions>()))
            .Do(_ => throw new InvalidOperationException());
        var source = controller.Scan(new ScanOptions { Device = new ScanDevice(Driver.Default, "foo", "bar") });
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.ToListAsync());
    }

    [Fact]
    public async Task GetDeviceList_CreateScanBridge()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        bridgeFactory.When(factory => factory.Create(Arg.Any<ScanOptions>()))
            .Do(_ => throw new InvalidOperationException());
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetDeviceList(new ScanOptions()));
    }

    [Fact]
    public async Task GetDeviceList_BridgeGetDeviceList()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridge = new MockScanBridge { Error = new InvalidOperationException() };
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        bridgeFactory.Create(Arg.Any<ScanOptions>()).Returns(bridge);
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetDeviceList(new ScanOptions()));
    }

    [Fact]
    public async void Scan_LocalPostProcess()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridge = new MockScanBridge { MockOutput = [CreateScannedImage()] };
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        bridgeFactory.Create(Arg.Any<ScanOptions>()).Returns(bridge);
        localPostProcessor.When(pp =>
                pp.PostProcess(Arg.Any<ProcessedImage>(), Arg.Any<ScanOptions>(), Arg.Any<PostProcessingContext>()))
            .Do(_ => throw new InvalidOperationException());
        var source = controller.Scan(new ScanOptions { Device = new ScanDevice(Driver.Default, "foo", "bar") });
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.ToListAsync());
    }

    [Fact]
    public async void Scan_BridgeScan()
    {
        var localPostProcessor = Substitute.For<ILocalPostProcessor>();
        var bridge = new MockScanBridge { Error = new InvalidOperationException() };
        var bridgeFactory = Substitute.For<IScanBridgeFactory>();
        var controller =
            new ScanController(ScanningContext, localPostProcessor, new ScanOptionsValidator(),
                bridgeFactory);

        bridgeFactory.Create(Arg.Any<ScanOptions>()).Returns(bridge);
        var source = controller.Scan(new ScanOptions { Device = new ScanDevice(Driver.Default, "foo", "bar") });
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.ToListAsync());
    }

    // TODO: Add some testing that exceptions are wrapped up in ScanDriverUnknownException where appropriate (always? only from driver itself? is it really needed?)
    // TODO: I guess the point is that you get the nice-ish "An error occurred with the scanning driver" instead of some inscrutable message. But is that more of a UI thing?
}