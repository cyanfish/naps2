using NAPS2.Images.Bitwise;
using Xunit;
using static NAPS2.Sdk.Tests.Images.SyntheticImageGenerator;

namespace NAPS2.Sdk.Tests.Images;

/// <summary>
/// Tests for EdgeDetectionImageOp using real scans, where the expected crop was measured by hand.
/// </summary>
public class EdgeDetectionRealWorldTests : ContextualTests
{
    // The allowed error in pixels (at 300 dpi, ~1.7mm)
    private const int TOLERANCE = 20;

    private const int PAD_X = 250;
    private const int PAD_Y = 300;
    // The expected bounds include some blur between the page and the background, so the padding tests crop a bit
    // further in to avoid leaving a thin line between the page and the padding.
    private const int INSET = 20;

    private record ScanCase(byte[] Resource, Crop Expected);

    private record Crop(int Left, int Top, int Right, int Bottom);

    private static readonly Dictionary<string, ScanCase> Cases = new()
    {
        // Letter-size page on a longer flatbed. The page fills the width and top of the scan; the bottom edge is a
        // faint highlight line just above the slightly darker lid.
        ["doc"] = new(ImageResources.autocrop_doc, new Crop(0, 0, 0, 183)),
        // Same as "doc", but with a table. The table is content on the page; the crop should be at the actual page
        // edge, even though the table's long ruled lines look a lot like edges.
        ["table"] = new(ImageResources.autocrop_table, new Crop(0, 0, 0, 176)),
        // A photo in the top-left corner of the scan.
        ["photo"] = new(ImageResources.autocrop_photo, new Crop(0, 0, 873, 2186)),
        // A white receipt in the top-left corner of the scan, on a white lid.
        ["receipt"] = new(ImageResources.autocrop_receipt, new Crop(0, 0, 1608, 2038)),
    };

    private static readonly Dictionary<string, Rgb> FillColors = new()
    {
        ["white"] = WHITE,
        ["grey"] = GRAY,
        ["black"] = BLACK,
    };

    public static IEnumerable<object[]> CaseNames => Cases.Keys.Select(x => new object[] { x });

    public static IEnumerable<object[]> PaddingCases =>
        from name in Cases.Keys
        from color in FillColors.Keys
        from axes in new[] { "x", "y", "xy" }
        // The bottom of the photo has a similar luma to grey, which is too ambiguous to be worth handling
        where !(name == "photo" && color == "grey")
        select new object[] { name, color, axes };

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void DetectsPage(string name)
    {
        var scanCase = Cases[name];
        var image = LoadImage(scanCase.Resource);

        AssertCrop(scanCase.Expected, Detect(image));
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Idempotent(string name)
    {
        // Once cropped, there shouldn't be anything else to crop
        var scanCase = Cases[name];
        var image = LoadImage(scanCase.Resource);
        var first = Detect(image);
        var cropped = image.PerformTransform(new CropTransform(first.Left, first.Right, first.Top, first.Bottom));

        AssertCrop(new Crop(0, 0, 0, 0), Detect(cropped));
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Grayscale(string name)
    {
        var scanCase = Cases[name];
        var image = LoadImage(scanCase.Resource).CopyWithPixelFormat(ImagePixelFormat.Gray8);

        AssertCrop(scanCase.Expected, Detect(image));
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void HalfResolution(string name)
    {
        // Detection should work for lower resolution scans too (here 150 dpi)
        var scanCase = Cases[name];
        var image = LoadImage(scanCase.Resource).PerformTransform(new ScaleTransform(0.5));
        var actual = Detect(image);

        AssertCrop(scanCase.Expected, new Crop(actual.Left * 2, actual.Top * 2, actual.Right * 2, actual.Bottom * 2));
    }

    [Theory]
    [MemberData(nameof(DeskewCases))]
    public void Deskewed(string name, double angle)
    {
        // Simulate deskewing a crooked scan: rotate the scan, crop off the corners (as a real scan would only have
        // scanner background there), then rotate back. The page ends up straight, but there are angled white wedges
        // along the sides of the image where the scan area doesn't cover.
        var scanCase = Cases[name];
        var image = LoadImage(scanCase.Resource);
        int margin = (int) Math.Ceiling(Math.Max(image.Width, image.Height) * Math.Sin(Math.Abs(angle) * Math.PI / 180));
        var deskewed = image
            .PerformTransform(new RotationTransform(angle))
            .PerformTransform(new CropTransform(margin, margin, margin, margin))
            .PerformTransform(new RotationTransform(-angle));

        var expected = scanCase.Expected;
        int Adjust(int crop) => Math.Max(crop - margin, 0);
        AssertCrop(
            new Crop(Adjust(expected.Left), Adjust(expected.Top), Adjust(expected.Right), Adjust(expected.Bottom)),
            Detect(deskewed));
    }

    public static IEnumerable<object[]> DeskewCases =>
        from name in Cases.Keys
        from angle in new[] { 1.0, -2.0 }
        select new object[] { name, angle };

    [Theory]
    [MemberData(nameof(PaddingCases))]
    public void CroppedAndPadded(string name, string color, string axes)
    {
        // Crop to the page, then add padding to the right and/or bottom as if the page were scanned in the corner of
        // a larger scan area with a uniform background
        var scanCase = Cases[name];
        var expected = scanCase.Expected;
        int Inset(int crop) => crop > 0 ? crop + INSET : 0;
        var page = LoadImage(scanCase.Resource).PerformTransform(new CropTransform(
            Inset(expected.Left), Inset(expected.Right), Inset(expected.Top), Inset(expected.Bottom)));
        int padX = axes.Contains('x') ? PAD_X : 0;
        int padY = axes.Contains('y') ? PAD_Y : 0;
        var image = Pad(page, padX, padY, FillColors[color]);

        var actual = Detect(image);
        AssertCrop(new Crop(0, 0, padX, padY), actual);
        // The padding has a sharp edge, so it should be detected exactly
        if (padX > 0) Assert.Equal(padX, actual.Right);
        if (padY > 0) Assert.Equal(padY, actual.Bottom);
    }

    private static Crop Detect(IMemoryImage image)
    {
        var op = new EdgeDetectionImageOp();
        op.Perform(image);
        return new Crop(op.Left, op.Top, op.Right, op.Bottom);
    }

    private static void AssertCrop(Crop expected, Crop actual)
    {
        bool Close(int a, int b) => Math.Abs(a - b) <= TOLERANCE;
        Assert.True(
            Close(expected.Left, actual.Left) && Close(expected.Top, actual.Top) &&
            Close(expected.Right, actual.Right) && Close(expected.Bottom, actual.Bottom),
            $"Expected {expected} (±{TOLERANCE}), got {actual}");
    }
}
