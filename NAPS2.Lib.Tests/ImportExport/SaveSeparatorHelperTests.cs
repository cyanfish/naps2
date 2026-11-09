using NAPS2.ImportExport;
using NAPS2.Images;
using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using ZXing;
using Xunit;

namespace NAPS2.Lib.Tests.ImportExport;

public class SaveSeparatorHelperTests : ContextualTests
{
    [Fact]
    public void BarcodeSeparatorKeepsMarkerAtStartOfNextDocument()
    {
        var images = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray,
            ImageResources.patcht,
            ImageResources.dog_h_n300);
        images[2] = images[2].WithPostProcessingData(images[2].PostProcessingData with
        {
            Barcode = new Barcode(true, true, "AB1234") { DetectedFormat = BarcodeFormat.QR_CODE }
        }, true);

        var scans = SaveSeparatorHelper.SeparateScans(new[] { images.AsEnumerable() }, SaveSeparator.Barcode,
            barcodeSeparatorOptions: new BarcodeSeparatorOptions
            {
                BarcodeType = BarcodeFormat.QR_CODE,
                MinimumLength = 6,
                MaximumLength = 6,
                Prefix = "AB"
            }).ToList();

        Assert.Equal(2, scans.Count);
        Assert.Equal(new[] { images[0], images[1] }, scans[0]);
        Assert.Equal(new[] { images[2], images[3] }, scans[1]);
        foreach (var image in scans.SelectMany(x => x))
        {
            image.Dispose();
        }
    }

    [Fact]
    public void BarcodeSeparatorCanDiscardMarkerPage()
    {
        var images = CreateScannedImages(ImageResources.dog, ImageResources.patcht, ImageResources.dog_gray);
        images[1] = images[1].WithPostProcessingData(images[1].PostProcessingData with
        {
            Barcode = new Barcode(true, true, "123") { DetectedFormat = BarcodeFormat.CODE_128 }
        }, true);

        var scans = SaveSeparatorHelper.SeparateScans(new[] { images.AsEnumerable() }, SaveSeparator.Barcode,
            barcodeSeparatorOptions: new BarcodeSeparatorOptions
            {
                BarcodeType = BarcodeFormat.CODE_128,
                RemoveBarcodePage = true
            }).ToList();

        Assert.True(IsDisposed(images[1]));
        Assert.Equal(2, scans.Count);
        Assert.Equal(new[] { images[0] }, scans[0]);
        Assert.Equal(new[] { images[2] }, scans[1]);
        foreach (var image in scans.SelectMany(x => x))
        {
            image.Dispose();
        }
    }

    [Fact]
    public void BarcodeSeparatorCanMatchSelectedPatchCodes()
    {
        var patchTexts = new[] { "PATCH1", "PATCH2", "PATCH3", "PATCH4", "PATCH6", "PATCHT" };
        var images = CreateScannedImages(Enumerable.Repeat(ImageResources.dog, patchTexts.Length).ToArray());
        for (int i = 0; i < images.Count; i++)
        {
            images[i] = images[i].WithPostProcessingData(images[i].PostProcessingData with
            {
                Barcode = new Barcode(true, true, patchTexts[i]) { DetectedFormat = BarcodeFormat.CODE_39 }
            }, true);
        }

        var scans = SaveSeparatorHelper.SeparateScans(new[] { images.AsEnumerable() }, SaveSeparator.Barcode,
            barcodeSeparatorOptions: new BarcodeSeparatorOptions
            {
                PatchCodes = [PatchCodeType.PatchIII, PatchCodeType.PatchVI]
            }).ToList();

        Assert.Equal(new PatchCodeType?[] { PatchCodeType.PatchI, PatchCodeType.PatchII, PatchCodeType.PatchIII,
                PatchCodeType.PatchIV, PatchCodeType.PatchVI, PatchCodeType.PatchT },
            images.Select(x => x.PostProcessingData.Barcode.PatchCode).ToArray());
        Assert.Equal(3, scans.Count);
        Assert.Equal(images.Take(2), scans[0]);
        Assert.Equal(images.Skip(2).Take(2), scans[1]);
        Assert.Equal(images.Skip(4), scans[2]);
        foreach (var image in scans.SelectMany(x => x))
        {
            image.Dispose();
        }
    }

    [Theory]
    [InlineData(BarcodeFormat.CODE_128, 1, int.MaxValue, "", "ABC-abc-1234")]
    [InlineData(BarcodeFormat.CODE_39, 1, int.MaxValue, "", "ABC-1234")]
    [InlineData(BarcodeFormat.CODE_128, 12, 12, "", "ABC-abc-1234")]
    [InlineData(null, 8, 8, "ABC-", "ABC-1234")]
    [InlineData(null, 9, 11, "", "")]
    [InlineData(null, 1, int.MaxValue, "ABC-abc", "ABC-abc-1234")]
    [InlineData(null, 1, int.MaxValue, "ABC-xyz", "")]
    public void BarcodeSeparatorFiltersSampleCodes(BarcodeFormat? barcodeType, int minimumLength, int maximumLength,
        string prefix, string expectedSeparator)
    {
        var images = CreateSampleBarcodeImages();
        var scans = SaveSeparatorHelper.SeparateScansWithBarcodeValues(new[] { images.AsEnumerable() },
            SaveSeparator.Barcode, barcodeSeparatorOptions: new BarcodeSeparatorOptions
            {
                BarcodeType = barcodeType,
                MinimumLength = minimumLength,
                MaximumLength = maximumLength,
                Prefix = prefix
            }).ToList();

        Assert.Equal(expectedSeparator == "" ? 1 : 2, scans.Count);
        Assert.Equal(expectedSeparator == "" ? Array.Empty<string>() : new[] { expectedSeparator },
            scans.Skip(1).Select(x => x.BarcodeValue));
        foreach (var image in scans.SelectMany(x => x.Images))
        {
            image.Dispose();
        }
    }

    [Fact]
    public void BarcodeSeparatorNamesFilesUsingInitialBarcodeAndPatchCode()
    {
        var code39 = DetectSampleBarcode(ImageResources.barcode_code39);
        var code128 = DetectSampleBarcode(ImageResources.barcode_code128);
        var images = CreateScannedImages(
            ImageResources.dog,
            ImageResources.dog_gray,
            ImageResources.dog_h_n300,
            ImageResources.dog_h_p300);
        images[0] = images[0].WithPostProcessingData(images[0].PostProcessingData with { Barcode = code39 }, true);
        images[1] = images[1].WithPostProcessingData(images[1].PostProcessingData with { Barcode = code128 }, true);
        images[2] = images[2].WithPostProcessingData(images[2].PostProcessingData with
        {
            Barcode = new Barcode(true, true, "PATCH1") { DetectedFormat = BarcodeFormat.CODE_39 }
        }, true);
        images[3] = images[3].WithPostProcessingData(images[3].PostProcessingData with { Barcode = code128 }, true);

        var scans = SaveSeparatorHelper.SeparateScansWithBarcodeValues(new[] { images.AsEnumerable() },
            SaveSeparator.Barcode, barcodeSeparatorOptions: new BarcodeSeparatorOptions
            {
                PatchCodes = [PatchCodeType.PatchI]
            }, removePatchCodePages: true).ToList();

        Assert.Equal(2, scans.Count);
        Assert.Equal(new[] { images[0], images[1] }, scans[0].Images);
        Assert.Equal("ABC-1234", scans[0].BarcodeValue);
        Assert.Equal(new[] { images[3] }, scans[1].Images);
        Assert.Equal("PATCH1", scans[1].BarcodeValue);
        Assert.True(IsDisposed(images[2]));

        var nameManager = new NameManager();
        var context = new NameContext { DateTime = DateTime.Now, IsFilePath = false };
        Assert.Equal("scanorder-1-ABC-1234.pdf",
            nameManager.Get("scanorder-$(n)-$(barcode).pdf",
                context with { BarcodeValue = scans[0].BarcodeValue }));
        Assert.Equal("scanorder-2-PATCH1.pdf",
            nameManager.Get("scanorder-$(n)-$(barcode).pdf",
                context with { BatchIndex = 1, BarcodeValue = scans[1].BarcodeValue }));

        foreach (var image in scans.SelectMany(x => x.Images))
        {
            image.Dispose();
        }
    }

    private List<ProcessedImage> CreateSampleBarcodeImages()
    {
        var barcodes = new[]
        {
            DetectSampleBarcode(ImageResources.barcode_code128),
            DetectSampleBarcode(ImageResources.barcode_code39)
        };
        var images = CreateScannedImages(ImageResources.dog, ImageResources.dog_gray, ImageResources.dog_h_n300,
            ImageResources.dog_h_p300, ImageResources.dog_s_n300);
        images[1] = images[1].WithPostProcessingData(images[1].PostProcessingData with { Barcode = barcodes[0] }, true);
        images[3] = images[3].WithPostProcessingData(images[3].PostProcessingData with { Barcode = barcodes[1] }, true);
        return images;
    }

    private Barcode DetectSampleBarcode(byte[] imageData)
    {
        using var image = LoadImage(imageData);
        return BarcodeDetector.Detect(image, new BarcodeDetectionOptions
        {
            DetectBarcodes = true,
            PatchTOnly = false
        });
    }

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
