using NAPS2.Images.Bitwise;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class CopyBitwiseImageOpTests : ContextualTests
{
    [Fact]
    public void DestChannel_ColorToColor()
    {
        var original = LoadImage(ImageResources.dog);

        var destR = original.CopyBlank();
        var destB = original.CopyBlank();
        var destG = original.CopyBlank();
        var destRbg = original.CopyBlank();

        new CopyBitwiseImageOp { DestChannel = ColorChannel.Red }.Perform(original, destR);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Green }.Perform(original, destG);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Blue }.Perform(original, destB);

        new CopyBitwiseImageOp { DestChannel = ColorChannel.Red }.Perform(original, destRbg);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Green }.Perform(original, destRbg);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Blue }.Perform(original, destRbg);

        ImageAsserts.NotSimilar(original, destR);
        ImageAsserts.NotSimilar(original, destB);
        ImageAsserts.NotSimilar(original, destG);
        ImageAsserts.Similar(original, destRbg, 0);
    }

    [Fact]
    public void DestChannel_GrayscaleToColor()
    {
        var color  = LoadImage(ImageResources.dog);
        var original = color.CopyWithPixelFormat(ImagePixelFormat.Gray8);

        var dest = color.CopyBlank();

        new CopyBitwiseImageOp { DestChannel = ColorChannel.Red }.Perform(original, dest);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Green }.Perform(original, dest);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Blue }.Perform(original, dest);

        ImageAsserts.Similar(original, dest, 0);
    }

    [Fact]
    public void RgbToRgba()
    {
        var color  = LoadImage(ImageResources.dog);
        var original = color.CopyWithPixelFormat(ImagePixelFormat.RGB24);
        var dest = original.CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        ImageAsserts.Similar(original, dest, 0);
    }

    [Fact]
    public void GrayToRgba()
    {
        var color  = LoadImage(ImageResources.dog);
        var original = color.CopyWithPixelFormat(ImagePixelFormat.Gray8);
        var dest = original.CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        ImageAsserts.Similar(original, dest, 0);
    }

    [Fact]
    public void BlackWhiteToRgba()
    {
        var color  = LoadImage(ImageResources.dog);
        var original = color.CopyWithPixelFormat(ImagePixelFormat.BW1);
        var dest = original.CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        ImageAsserts.Similar(original, dest, 0);
    }

    [Fact]
    public void BlackWhiteInvertSource()
    {
        var srcBuffer = new byte[] { 0xFA, 0x03 };
        var srcInfo = new PixelInfo(8, 2, SubPixelType.InvertedBit);

        var dstBuffer = new byte[] { 0, 0 };
        var dstInfo = new PixelInfo(8, 2, SubPixelType.Bit);

        new CopyBitwiseImageOp().Perform(srcBuffer, srcInfo, dstBuffer, dstInfo);

        var expectedBuffer = new byte[] { 0x05, 0xFC };
        Assert.Equal(expectedBuffer, dstBuffer);
    }

    [Fact]
    public void BlackWhiteInvertDest()
    {
        var srcBuffer = new byte[] { 0xFA, 0x03 };
        var srcInfo = new PixelInfo(8, 2, SubPixelType.Bit);

        var dstBuffer = new byte[2];
        var dstInfo = new PixelInfo(8, 2, SubPixelType.InvertedBit);

        new CopyBitwiseImageOp().Perform(srcBuffer, srcInfo, dstBuffer, dstInfo);

        var expectedBuffer = new byte[] { 0x05, 0xFC };
        Assert.Equal(expectedBuffer, dstBuffer);
    }

    [Fact]
    public void BlackWhiteInvertSourceAndDest()
    {
        var srcBuffer = new byte[] { 0xFA, 0x03 };
        var srcInfo = new PixelInfo(8, 2, SubPixelType.InvertedBit);

        var dstBuffer = new byte[] { 0, 0 };
        var dstInfo = new PixelInfo(8, 2, SubPixelType.InvertedBit);

        new CopyBitwiseImageOp().Perform(srcBuffer, srcInfo, dstBuffer, dstInfo);

        var expectedBuffer = new byte[] { 0xFA, 0x03 };
        Assert.Equal(expectedBuffer, dstBuffer);
    }

    [Fact]
    public void BlackWhiteInvertSourceToGray()
    {
        var srcBuffer = new byte[] { 0xFA, 0x03 };
        var srcInfo = new PixelInfo(8, 2, SubPixelType.InvertedBit);

        var dstBuffer = new byte[16];
        var dstInfo = new PixelInfo(8, 2, SubPixelType.Gray);

        new CopyBitwiseImageOp().Perform(srcBuffer, srcInfo, dstBuffer, dstInfo);

        var expectedBuffer = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0xFF, // 0x05 expanded
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00 // 0xFC expanded
        };
        Assert.Equal(expectedBuffer, dstBuffer);
    }

    [Fact]
    public void BlackWhiteInvertDestFromGray()
    {
        var srcBuffer = new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0x00, // 0xFA expanded
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF // 0x03 expanded
        };
        var srcInfo = new PixelInfo(8, 2, SubPixelType.Gray);

        var dstBuffer = new byte[2];
        var dstInfo = new PixelInfo(8, 2, SubPixelType.InvertedBit);

        new CopyBitwiseImageOp().Perform(srcBuffer, srcInfo, dstBuffer, dstInfo);

        var expectedBuffer = new byte[] { 0x05, 0xFC };
        Assert.Equal(expectedBuffer, dstBuffer);
    }

    // Pixels with partial, zero and full alpha
    private static readonly byte[] Straight = [200, 100, 50, 128, 10, 20, 30, 0, 40, 50, 60, 255];
    private static readonly byte[] Premultiplied = [100, 50, 25, 128, 0, 0, 0, 0, 40, 50, 60, 255];
    private static readonly PixelInfo StraightInfo = new(3, 1, SubPixelType.Rgba);
    private static readonly PixelInfo PremultipliedInfo = new(3, 1, SubPixelType.Rgba) { PremultipliedAlpha = true };

    [Fact]
    public void StraightToPremultiplied()
    {
        var dst = new byte[12];
        new CopyBitwiseImageOp().Perform(Straight, StraightInfo, dst, PremultipliedInfo);
        Assert.Equal(Premultiplied, dst);
    }

    [Fact]
    public void PremultipliedToStraight()
    {
        var dst = new byte[12];
        new CopyBitwiseImageOp().Perform(Premultiplied, PremultipliedInfo, dst, StraightInfo);
        // Some precision (and the transparent pixel's color) is lost
        Assert.Equal([199, 100, 50, 128, 0, 0, 0, 0, 40, 50, 60, 255], dst);
    }

    [Fact]
    public void PremultipliedToRgb()
    {
        var dst = new byte[9];
        new CopyBitwiseImageOp().Perform(Premultiplied, PremultipliedInfo, dst, new PixelInfo(3, 1, SubPixelType.Rgb));
        Assert.Equal([199, 100, 50, 0, 0, 0, 40, 50, 60], dst);
    }

    [Fact]
    public void StraightAlphaRoundTripsThroughImage()
    {
        // The image may be premultiplied depending on the platform
        using var image = ImageContext.Create(3, 1, ImagePixelFormat.ARGB32);
        new CopyBitwiseImageOp().Perform(Straight, StraightInfo, image);
        var dst = new byte[12];
        new CopyBitwiseImageOp().Perform(image, dst, StraightInfo);
        for (int i = 0; i < 4; i++)
        {
            Assert.InRange(dst[i], Straight[i] - 1, Straight[i]);
        }
    }
}