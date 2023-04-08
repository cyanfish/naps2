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
}