using NAPS2.Images.Bitwise;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class CopyBitwiseImageOpTests : ContextualTests
{
    [Fact]
    public void DestChannel_ColorToColor()
    {
        var original = LoadImage(ImageResources.color_image);

        var destR = ImageContext.Create(original.Width, original.Height, ImagePixelFormat.ARGB32);
        var destB = ImageContext.Create(original.Width, original.Height, ImagePixelFormat.ARGB32);
        var destG = ImageContext.Create(original.Width, original.Height, ImagePixelFormat.ARGB32);
        var destRbg = ImageContext.Create(original.Width, original.Height, ImagePixelFormat.ARGB32);

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
        var color  = LoadImage(ImageResources.color_image);
        var original = ImageContext.Create(color.Width, color.Height, ImagePixelFormat.Gray8);
        color.CopyTo(original);

        var dest = ImageContext.Create(original.Width, original.Height, ImagePixelFormat.ARGB32);

        new CopyBitwiseImageOp { DestChannel = ColorChannel.Red }.Perform(original, dest);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Green }.Perform(original, dest);
        new CopyBitwiseImageOp { DestChannel = ColorChannel.Blue }.Perform(original, dest);

        ImageAsserts.Similar(original, dest, 0);
    }
}