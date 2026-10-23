using NAPS2.ImportExport;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.ImportExport;

public class SaveSeparatorHelperTests : ContextualTests
{
    [Theory]
    [InlineData(SaveSeparator.None)]
    [InlineData(SaveSeparator.FilePerPage)]
    [InlineData(SaveSeparator.FilePerScan)]
    [InlineData(SaveSeparator.PatchT)]
    public void NoImages(SaveSeparator separator)
    {
        Assert.Empty(SaveSeparatorHelper.SeparateScans([], separator));
        Assert.Empty(SaveSeparatorHelper.SeparateScans([[], []], separator));
    }

    [Fact]
    public void None_CombinesAllScans()
    {
        var (a, b, c) = (CreateScannedImage(), CreateScannedImage(), CreateScannedImage());

        var result = SaveSeparatorHelper.SeparateScans([[a, b], [], [c]], SaveSeparator.None).ToList();

        AssertGroups(result, [a, b, c]);
    }

    [Fact]
    public void FilePerScan()
    {
        var (a, b, c) = (CreateScannedImage(), CreateScannedImage(), CreateScannedImage());

        var result = SaveSeparatorHelper.SeparateScans([[a, b], [], [c]], SaveSeparator.FilePerScan).ToList();

        AssertGroups(result, [a, b], [c]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void FilePerPage(int splitSize)
    {
        var (a, b, c) = (CreateScannedImage(), CreateScannedImage(), CreateScannedImage());

        var result = SaveSeparatorHelper.SeparateScans([[a, b], [], [c]], SaveSeparator.FilePerPage, splitSize)
            .ToList();

        AssertGroups(result, [a], [b], [c]);
    }

    [Fact]
    public void FilePerPage_SplitSizeDoesntSpanScans()
    {
        var (a, b, c) = (CreateScannedImage(), CreateScannedImage(), CreateScannedImage());
        var (d, e) = (CreateScannedImage(), CreateScannedImage());

        var result = SaveSeparatorHelper.SeparateScans([[a, b, c], [d, e]], SaveSeparator.FilePerPage, 2).ToList();

        AssertGroups(result, [a, b], [c], [d, e]);
    }

    [Fact]
    public void PatchT_SplitsOnPatchSheetsAcrossScans()
    {
        var (a, b, c, d) = (CreateScannedImage(), CreateScannedImage(), CreateScannedImage(), CreateScannedImage());
        var (p1, p2) = (CreatePatchT(), CreatePatchT());

        var result = SaveSeparatorHelper.SeparateScans([[a, p1, b], [c, p2], [d]], SaveSeparator.PatchT).ToList();

        AssertGroups(result, [a], [b, c], [d]);
        Assert.True(IsDisposed(p1));
        Assert.True(IsDisposed(p2));
        Assert.False(IsDisposed(a));
    }

    [Fact]
    public void PatchT_NoEmptyGroups()
    {
        var (a, b) = (CreateScannedImage(), CreateScannedImage());
        var (p1, p2, p3, p4) = (CreatePatchT(), CreatePatchT(), CreatePatchT(), CreatePatchT());

        var result = SaveSeparatorHelper.SeparateScans([[p1, a, p2, p3, b, p4]], SaveSeparator.PatchT).ToList();

        AssertGroups(result, [a], [b]);
        Assert.True(new[] { p1, p2, p3, p4 }.All(IsDisposed));
    }

    [Fact]
    public void PatchT_OnlyPatchSheets()
    {
        var p1 = CreatePatchT();

        Assert.Empty(SaveSeparatorHelper.SeparateScans([[p1]], SaveSeparator.PatchT));
        Assert.True(IsDisposed(p1));
    }

    [Fact]
    public void PatchT_OtherBarcodesDontSplit()
    {
        var a = CreateScannedImage();
        var b = CreateScannedImage().WithPostProcessingData(
            new PostProcessingData() with { Barcode = new Barcode(true, true, "OTHER") }, true);

        var result = SaveSeparatorHelper.SeparateScans([[a, b]], SaveSeparator.PatchT).ToList();

        AssertGroups(result, [a, b]);
    }

    private ProcessedImage CreatePatchT()
    {
        var image = CreateScannedImage().WithPostProcessingData(
            new PostProcessingData() with { Barcode = new Barcode(true, true, "PATCHT") }, true);
        Assert.True(image.PostProcessingData.Barcode.IsPatchT);
        return image;
    }

    private static void AssertGroups(List<List<ProcessedImage>> actual, params List<ProcessedImage>[] expected)
    {
        Assert.Equal(expected.Length, actual.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }
}
