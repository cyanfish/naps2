using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Sane;
using NAPS2.Scan.Internal.Sane.Native;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class SaneScanDriverTests : ContextualTests
{
    private static readonly int[] ExpectedData = Enumerable.Range(0, 64).Concat(Enumerable.Range(0, 16)).ToArray();

    private readonly SaneScanDriver _driver;

    public SaneScanDriverTests()
    {
        _driver = new SaneScanDriver(ScanningContext);
    }

    [Fact]
    public void ScanFrame_KnownFrameSize()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock);

        var result = _driver.ScanFrame(deviceMock, eventsMock, 0, out var p);

        Assert.NotNull(result);
        Assert.Equal(ExpectedData.Length, result.Length);
        Assert.Equal(ExpectedData, result.ToArray().Select(x => (int) x));
        deviceMock.Received().Start();
        eventsMock.Received().PageStart();
        eventsMock.Received().PageProgress(0);
        eventsMock.Received().PageProgress(Arg.Is<double>(x => x - 64.0 / 80.0 < 0.01));
        eventsMock.Received().PageProgress(1);
        eventsMock.ReceivedCallsCount(4);
    }

    [Fact]
    public void ScanFrame_UnknownFrameSize()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock, -1);

        var result = _driver.ScanFrame(deviceMock, eventsMock, 0, out var p);

        Assert.NotNull(result);
        Assert.Equal(ExpectedData.Length, result.Length);
        Assert.Equal(ExpectedData, result.ToArray().Select(x => (int) x));
        Assert.Equal(p.Lines, 8);
        deviceMock.Received().Start();
        eventsMock.Received().PageStart();
        eventsMock.ReceivedCallsCount(1);
    }

    [Fact]
    public void ScanFrame_NoRead_ReturnsNull()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock);
        deviceMock.Read(Arg.Any<byte[]>(), out Arg.Any<int>())
            .Returns(x =>
            {
                x[1] = 0;
                return false;
            });

        var result = _driver.ScanFrame(deviceMock, eventsMock, 0, out var p);

        Assert.Null(result);
        deviceMock.Received().Start();
        eventsMock.Received().PageStart();
        eventsMock.Received().PageProgress(0);
        eventsMock.ReceivedCallsCount(2);
    }

    [Fact]
    public void ScanPage_KnownFrameSize()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock);

        var result = _driver.ScanPage(deviceMock, eventsMock, new SaneScanDriver.OptionData());

        Assert.NotNull(result);
        Assert.Equal(3, result.Width);
        Assert.Equal(8, result.Height);
        ImageAsserts.PixelColors(result, new()
        {
            { (0, 0), (0, 1, 2) },
            { (2, 2), (26, 27, 28) }
        });
    }

    [Fact]
    public void ScanPage_UnknownFrameSize()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock, -1);

        var result = _driver.ScanPage(deviceMock, eventsMock, new SaneScanDriver.OptionData());

        Assert.NotNull(result);
        Assert.Equal(3, result.Width);
        Assert.Equal(8, result.Height);
        ImageAsserts.PixelColors(result, new()
        {
            { (0, 0), (0, 1, 2) },
            { (2, 2), (26, 27, 28) }
        });
    }

    [Fact]
    public void ScanPage_TooSmallFrameSize()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock, 6);

        var result = _driver.ScanPage(deviceMock, eventsMock, new SaneScanDriver.OptionData());

        Assert.NotNull(result);
        Assert.Equal(3, result.Width);
        Assert.Equal(8, result.Height);
        ImageAsserts.PixelColors(result, new()
        {
            { (0, 0), (0, 1, 2) },
            { (2, 2), (26, 27, 28) }
        });
    }

    [Fact]
    public void ScanPage_TooBigFrameSize()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock, 10);

        var result = _driver.ScanPage(deviceMock, eventsMock, new SaneScanDriver.OptionData());

        Assert.NotNull(result);
        Assert.Equal(3, result.Width);
        Assert.Equal(8, result.Height);
        ImageAsserts.PixelColors(result, new()
        {
            { (0, 0), (0, 1, 2) },
            { (2, 2), (26, 27, 28) }
        });
    }

    [Fact]
    public void ScanPage_NoRead_ReturnsNull()
    {
        var deviceMock = Substitute.For<ISaneDevice>();
        var eventsMock = Substitute.For<IScanEvents>();
        SetupDeviceMock(deviceMock);
        deviceMock.Read(Arg.Any<byte[]>(), out Arg.Any<int>())
            .Returns(x =>
            {
                x[1] = 0;
                return false;
            });

        var result = _driver.ScanPage(deviceMock, eventsMock, new SaneScanDriver.OptionData());

        Assert.Null(result);
    }

    private static void SetupDeviceMock(ISaneDevice deviceMock, int? lines = null)
    {
        deviceMock.GetParameters().Returns(new SaneReadParameters
        {
            Depth = 8,
            Frame = SaneFrameType.Rgb,
            Lines = lines ?? 8,
            BytesPerLine = 10,
            PixelsPerLine = 3
        });
        deviceMock.Read(Arg.Any<byte[]>(), out Arg.Any<int>())
            .Returns(x =>
                {
                    var buffer = (byte[]) x[0];
                    for (int i = 0; i < 64; i++)
                    {
                        buffer[i] = (byte) i;
                    }
                    x[1] = 64;
                    return true;
                },
                x =>
                {
                    var buffer = (byte[]) x[0];
                    for (int i = 0; i < 16; i++)
                    {
                        buffer[i] = (byte) i;
                    }
                    x[1] = 16;
                    return true;
                },
                x =>
                {
                    x[1] = 0;
                    return false;
                });
    }
}