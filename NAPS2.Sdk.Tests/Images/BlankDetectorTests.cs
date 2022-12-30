using NAPS2.Images.Bitwise;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class BlankDetectorTests : ContextualTests
{
    private const int WHITE_THRESHOLD = 70;
    private const int COVERAGE_THRESHOLD = 15;

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Blank1(ImagePixelFormat pixelFormat)
    {
        var image = GetTestImage(ImageResources.blank1, pixelFormat);
        var op = new BlankDetectionImageOp(WHITE_THRESHOLD, COVERAGE_THRESHOLD);
        op.Perform(image);
        Assert.True(op.IsBlank);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Blank2(ImagePixelFormat pixelFormat)
    {
        var image = GetTestImage(ImageResources.blank2, pixelFormat);
        var op = new BlankDetectionImageOp(WHITE_THRESHOLD, COVERAGE_THRESHOLD);
        op.Perform(image);
        Assert.True(op.IsBlank);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void NotBlank(ImagePixelFormat pixelFormat)
    {
        var image = GetTestImage(ImageResources.notblank, pixelFormat);
        var op = new BlankDetectionImageOp(WHITE_THRESHOLD, COVERAGE_THRESHOLD);
        op.Perform(image);
        Assert.False(op.IsBlank);
    }

    private IMemoryImage GetTestImage(byte[] resource, ImagePixelFormat pixelFormat)
    {
        var image = LoadImage(resource);
        if (pixelFormat == ImagePixelFormat.BW1)
        {
            return image.PerformTransform(new BlackWhiteTransform(WHITE_THRESHOLD * 20 - 1000));
        }
        return image.CopyWithPixelFormat(pixelFormat);
    }

    public static IEnumerable<object[]> TestCases = new List<object[]>
    {
        new object[] { ImagePixelFormat.ARGB32 },
        new object[] { ImagePixelFormat.RGB24 },
        new object[] { ImagePixelFormat.Gray8 },
        new object[] { ImagePixelFormat.BW1 }
    };
}