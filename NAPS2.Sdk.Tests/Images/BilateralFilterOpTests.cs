using NAPS2.Images.Bitwise;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;
using static NAPS2.Sdk.Tests.Images.SyntheticImageGenerator;

namespace NAPS2.Sdk.Tests.Images;

/// <summary>
/// Verifies that the vectorized bilateral filter gives identical results to the non-vectorized implementation.
/// </summary>
public class BilateralFilterOpTests : ContextualTests
{
    [Theory]
    [MemberData(nameof(FormatsAndSizes))]
    public void VectorizedMatchesScalar(ImagePixelFormat pixelFormat, int width, int height)
    {
        using var image = CreateNoisyPage(ImageContext, pixelFormat, width, height);

        AssertVectorizedMatchesScalar(image);
    }

    [Theory]
    [MemberData(nameof(Formats))]
    public void VectorizedMatchesScalarForRealImage(ImagePixelFormat pixelFormat)
    {
        using var original = LoadImage(ImageResources.ocr_test);
        using var image = original.CopyWithPixelFormat(pixelFormat);

        AssertVectorizedMatchesScalar(image);
    }

    private static void AssertVectorizedMatchesScalar(IMemoryImage image)
    {
        using var vectorized = image.CopyBlank();
        using var scalar = image.CopyBlank();

        new BilateralFilterOp().Perform(image, vectorized);
        new BilateralFilterOp { AllowVectorization = false }.Perform(image, scalar);

        ImageAsserts.Similar(scalar, vectorized, 0);
    }

    public static IEnumerable<object[]> Formats =
    [
        new object[] { ImagePixelFormat.ARGB32 },
        new object[] { ImagePixelFormat.RGB24 },
        new object[] { ImagePixelFormat.Gray8 }
    ];

    // Sizes cover: too narrow to vectorize (22), exactly one 8-wide vector of filtered pixels (23), widths where the
    // last vector is shifted to overlap the previous one, and heights near the filter size.
    public static IEnumerable<object[]> FormatsAndSizes =>
        from format in Formats
        from size in new[] { (22, 20), (23, 16), (24, 17), (37, 30), (100, 80), (257, 41) }
        select new object[] { format[0], size.Item1, size.Item2 };
}
