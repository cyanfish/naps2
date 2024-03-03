using NAPS2.Images.Bitwise;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Images;

public class BitwisePerfTests : ContextualTests
{
    private const int SIZE = 7000;

    private readonly ITestOutputHelper _output;

    public BitwisePerfTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CopyFast()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.ARGB32);
        using var image2 = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new CopyBitwiseImageOp().Perform(image1, image2);
    }

    [Fact]
    public void CopyColor()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.RGB24);
        using var image2 = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new CopyBitwiseImageOp().Perform(image1, image2);
    }

    [Fact]
    public void CopyToGray()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.ARGB32);
        using var image2 = CreateAndFill(ImagePixelFormat.Gray8);

        using var _ = Timer();
        new CopyBitwiseImageOp().Perform(image1, image2);
    }

    [Fact]
    public void CopyFromGray()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.Gray8);
        using var image2 = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new CopyBitwiseImageOp().Perform(image1, image2);
    }

    [Fact]
    public void CopyToBit()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.ARGB32);
        using var image2 = CreateAndFill(ImagePixelFormat.BW1);

        using var _ = Timer();
        new CopyBitwiseImageOp().Perform(image1, image2);
    }

    [Fact]
    public void CopyFromBit()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.BW1);
        using var image2 = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new CopyBitwiseImageOp().Perform(image1, image2);
    }

    [Fact]
    public void CopyAlignedBit()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.BW1);
        using var image2 = CreateAndFill(ImagePixelFormat.BW1);

        using var _ = Timer();
        new CopyBitwiseImageOp
        {
            SourceXOffset = 8,
            DestXOffset = 16,
            Columns = SIZE - 16
        }.Perform(image1, image2);
    }

    [Fact]
    public void CopyUnalignedBit()
    {
        using var image1 = CreateAndFill(ImagePixelFormat.BW1);
        using var image2 = CreateAndFill(ImagePixelFormat.BW1);

        using var _ = Timer();
        new CopyBitwiseImageOp
        {
            SourceXOffset = 1,
            DestXOffset = 2,
            Columns = SIZE - 2
        }.Perform(image1, image2);
    }

    [Fact]
    public void Brightness()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new BrightnessBitwiseImageOp(0.5f).Perform(image);
    }

    [Fact]
    public void Contrast()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new ContrastBitwiseImageOp(0.5f).Perform(image);
    }

    [Fact]
    public void HueShift()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new HueShiftBitwiseImageOp(0.5f).Perform(image);
    }

    [Fact]
    public void Saturation()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new SaturationBitwiseImageOp(0.5f).Perform(image);
    }

    [Fact]
    public void Sharpness()
    {
        // Using a smaller size as sharpening is super slow
        using var image = CreateAndFill(ImagePixelFormat.ARGB32, SIZE / 4);
        using var image2 = CreateAndFill(ImagePixelFormat.ARGB32, SIZE / 4);

        using var _ = Timer();
        new SharpenBitwiseImageOp(0.5f).Perform(image, image2);
    }

    [Fact]
    public void BilateralFilter()
    {
        // Using a smaller size as sharpening is super slow
        using var image = CreateAndFill(ImagePixelFormat.ARGB32, SIZE / 4);
        using var image2 = CreateAndFill(ImagePixelFormat.ARGB32, SIZE / 4);

        using var _ = Timer();
        new BilateralFilterOp().Perform(image, image2);
    }

    [Fact]
    public void BilateralFilterGray()
    {
        // Using a smaller size as sharpening is super slow
        using var image = CreateAndFill(ImagePixelFormat.Gray8, SIZE / 4);
        using var image2 = CreateAndFill(ImagePixelFormat.Gray8, SIZE / 4);

        using var _ = Timer();
        new BilateralFilterOp().Perform(image, image2);
    }

    [Fact]
    public void LogicalPixelFormat()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);

        using var _ = Timer();
        new LogicalPixelFormatOp().Perform(image);
    }

    [Fact]
    public void Fill()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);
        using var imageLock = image.Lock(LockMode.ReadWrite, out var data);

        using var _ = Timer();
        BitwisePrimitives.Fill(data, 0x83);
    }

    [Fact]
    public void Invert()
    {
        using var image = CreateAndFill(ImagePixelFormat.ARGB32);
        using var imageLock = image.Lock(LockMode.ReadWrite, out var data);

        using var _ = Timer();
        BitwisePrimitives.Invert(data);
    }

    private IMemoryImage CreateAndFill(ImagePixelFormat pixelFormat, int size = SIZE)
    {
        var image = ImageContext.Create(size, size, pixelFormat);
        using var _ = image.Lock(LockMode.ReadWrite, out var data);
        // Ensure memory is actually materialized and not just committed
        BitwisePrimitives.Fill(data, 0);
        return image;
    }

    private IDisposable Timer()
    {
        return new TimingRecorder(_output);
    }

    private class TimingRecorder : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Stopwatch _stopwatch;

        public TimingRecorder(ITestOutputHelper output)
        {
            _output = output;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _output.WriteLine($"Execution time: {_stopwatch.ElapsedMilliseconds}");
        }
    }
}