using System.Threading;
using NAPS2.Ocr;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Ocr;

public class TesseractOcrEngineTests : ContextualTests
{
    private readonly TesseractOcrEngine _engine;
    private readonly string _testImagePath;
    private readonly string _testImagePathHebrew;

    public TesseractOcrEngineTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        SetUpOcr();
        _testImagePath = CopyResourceToFile(BinaryResources.ocr_test, "ocr_test.jpg");
        _testImagePathHebrew = CopyResourceToFile(BinaryResources.ocr_test_hebrew, "ocr_test_hebrew.jpg");
        _engine = (TesseractOcrEngine) ScanningContext.OcrEngine;
    }

    [Fact]
    public async Task ProcessEnglishImage()
    {
        var ocrParams = new OcrParams("eng", OcrMode.Fast, 0);
        var result = await _engine.ProcessImage(ScanningContext, _testImagePath, ocrParams, CancellationToken.None);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Elements);
        foreach (var element in result.Elements)
        {
            Assert.Equal("eng", element.LanguageCode);
            Assert.False(element.RightToLeft);
        }
        Assert.Equal("ADVERTISEMENT.", result.Elements[0].Text);
        Assert.InRange(result.Elements[0].Bounds.x, 139, 149);
        Assert.InRange(result.Elements[0].Bounds.y, 26, 36);
        Assert.InRange(result.Elements[0].Bounds.w, 237, 247);
        Assert.InRange(result.Elements[0].Bounds.h, 17, 27);
    }

    [Fact]
    public async Task ProcessHebrewImage()
    {
        var result = await _engine.ProcessImage(ScanningContext, _testImagePathHebrew, new OcrParams("heb", OcrMode.Fast, 0), CancellationToken.None);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Elements);
        foreach (var element in result.Elements)
        {
            Assert.Equal("heb", element.LanguageCode);
            Assert.True(element.RightToLeft);
        }
        Assert.Equal("הקדמת", result.Elements[0].Text);
    }

    [Fact(Skip = "flaky")]
    public async Task ImmediateCancel()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();
        var result = await _engine.ProcessImage(ScanningContext, _testImagePath, new OcrParams("eng", OcrMode.Fast, 0), cts.Token);
        Assert.Null(result);
    }

    [Fact(Skip = "flaky")]
    public async Task CancelWhileProcessing()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(20);
        var result = await _engine.ProcessImage(ScanningContext, _testImagePath, new OcrParams("eng", OcrMode.Fast, 0), cts.Token);
        Assert.Null(result);
    }

    [Fact(Skip = "flaky")]
    public async Task Timeout()
    {
        var timeout = 0.1;
        var result = await _engine.ProcessImage(ScanningContext, _testImagePath, new OcrParams("eng", OcrMode.Fast, timeout), CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task NoTimeout()
    {
        var timeout = 60;
        var result = await _engine.ProcessImage(ScanningContext, _testImagePath, new OcrParams("eng", OcrMode.Fast, timeout), CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Mode()
    {
        CopyResourceToFile(BinaryResources.eng_traineddata, Path.Combine(FolderPath, "best"), "eng.traineddata");
        // Bad data for unused mode
        CopyResourceToFile(BinaryResources.heb_traineddata, Path.Combine(FolderPath, "fast"), "eng.traineddata");

        var mode = OcrMode.Best;
        var result = await _engine.ProcessImage(ScanningContext, _testImagePath, new OcrParams("eng", mode, 0), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("ADVERTISEMENT.", result.Elements[0].Text);
    }
}