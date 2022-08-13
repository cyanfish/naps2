using NAPS2.Images.Bitwise;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

// TODO: Make real tests instead of just perf comparison
public class BitwiseTests : ContextualTests
{
    private const int SIZE = 10000;

    [Fact]
    public void CopyFast()
    {
        var image1 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);
        var image2 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);

        new CopyBitwiseImageOp().Perform(image1, image2);
    }
    
    [Fact]
    public void CopyColor()
    {
        var image1 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.RGB24);
        var image2 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);

        new CopyBitwiseImageOp().Perform(image1, image2);
    }
    
    [Fact]
    public void CopyToGray()
    {
        var image1 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);
        var image2 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.Gray8);

        new CopyBitwiseImageOp().Perform(image1, image2);
    }
    
    [Fact]
    public void CopyFromGray()
    {
        var image1 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.Gray8);
        var image2 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);

        new CopyBitwiseImageOp().Perform(image1, image2);
    }
    
    [Fact]
    public void CopyToBit()
    {
        var image1 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);
        var image2 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.BW1);

        new CopyBitwiseImageOp().Perform(image1, image2);
    }
    
    [Fact]
    public void CopyFromBit()
    {
        var image1 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.BW1);
        var image2 = ImageContext.Create(SIZE, SIZE, ImagePixelFormat.ARGB32);

        new CopyBitwiseImageOp().Perform(image1, image2);
    }
}