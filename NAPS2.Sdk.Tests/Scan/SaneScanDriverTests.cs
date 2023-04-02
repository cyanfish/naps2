using Moq;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Sane;
using NAPS2.Scan.Internal.Sane.Native;
using NAPS2.Sdk.Tests.Asserts;
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
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock);

        var result = _driver.ScanFrame(deviceMock.Object, eventsMock.Object, 0, out var p);

        Assert.NotNull(result);
        Assert.Equal(ExpectedData.Length, result.Length);
        Assert.Equal(ExpectedData, result.ToArray().Select(x => (int) x));
        deviceMock.Verify(d => d.Start());
        eventsMock.Verify(e => e.PageStart());
        eventsMock.Verify(e => e.PageProgress(0));
        eventsMock.Verify(e => e.PageProgress(It.Is<double>(x => x - 64.0 / 80.0 < 0.01)));
        eventsMock.Verify(e => e.PageProgress(1));
        eventsMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ScanFrame_UnknownFrameSize()
    {
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock, -1);

        var result = _driver.ScanFrame(deviceMock.Object, eventsMock.Object, 0, out var p);

        Assert.NotNull(result);
        Assert.Equal(ExpectedData.Length, result.Length);
        Assert.Equal(ExpectedData, result.ToArray().Select(x => (int) x));
        Assert.Equal(p.Lines, 8);
        deviceMock.Verify(d => d.Start());
        eventsMock.Verify(e => e.PageStart());
        eventsMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ScanFrame_NoRead_ReturnsNull()
    {
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock);
        deviceMock.Setup(d => d.Read(It.IsAny<byte[]>(), out It.Ref<int>.IsAny))
            .Returns((byte[] buffer, out int len) =>
            {
                len = 0;
                return false;
            });

        var result = _driver.ScanFrame(deviceMock.Object, eventsMock.Object, 0, out var p);

        Assert.Null(result);
        deviceMock.Verify(d => d.Start());
        eventsMock.Verify(e => e.PageStart());
        eventsMock.Verify(e => e.PageProgress(0));
        eventsMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ScanPage_KnownFrameSize()
    {
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock);

        var result = _driver.ScanPage(deviceMock.Object, eventsMock.Object);

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
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock, -1);

        var result = _driver.ScanPage(deviceMock.Object, eventsMock.Object);

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
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock, 6);

        var result = _driver.ScanPage(deviceMock.Object, eventsMock.Object);

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
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock, 10);

        var result = _driver.ScanPage(deviceMock.Object, eventsMock.Object);

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
        var deviceMock = new Mock<ISaneDevice>();
        var eventsMock = new Mock<IScanEvents>();
        SetupDeviceMock(deviceMock);
        deviceMock.Setup(d => d.Read(It.IsAny<byte[]>(), out It.Ref<int>.IsAny))
            .Returns((byte[] buffer, out int len) =>
            {
                len = 0;
                return false;
            });

        var result = _driver.ScanPage(deviceMock.Object, eventsMock.Object);

        Assert.Null(result);
    }

    private static void SetupDeviceMock(Mock<ISaneDevice> deviceMock, int? lines = null)
    {
        deviceMock.Setup(d => d.GetParameters()).Returns(new SaneReadParameters
        {
            Depth = 8,
            Frame = SaneFrameType.Rgb,
            Lines = lines ?? 8,
            BytesPerLine = 10,
            PixelsPerLine = 3
        });
        var sequence = new MockSequence();
        deviceMock.InSequence(sequence).Setup(d => d.Read(It.IsAny<byte[]>(), out It.Ref<int>.IsAny))
            .Returns((byte[] buffer, out int len) =>
            {
                for (int i = 0; i < 64; i++)
                {
                    buffer[i] = (byte) i;
                }
                len = 64;
                return true;
            });
        deviceMock.InSequence(sequence).Setup(d => d.Read(It.IsAny<byte[]>(), out It.Ref<int>.IsAny))
            .Returns((byte[] buffer, out int len) =>
            {
                for (int i = 0; i < 16; i++)
                {
                    buffer[i] = (byte) i;
                }
                len = 16;
                return true;
            });
        deviceMock.InSequence(sequence).Setup(d => d.Read(It.IsAny<byte[]>(), out It.Ref<int>.IsAny))
            .Returns((byte[] buffer, out int len) =>
            {
                len = 0;
                return false;
            });
    }
}