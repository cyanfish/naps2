using System.Threading;
using NAPS2.Ocr;
using Xunit;

namespace NAPS2.Sdk.Tests.Ocr;

public class TesseractOcrEngineTests : ContextualTexts
{
    private readonly TesseractOcrEngine _engine;
    private readonly string _testImagePath;
    private readonly string _testImagePathHebrew;

    public TesseractOcrEngineTests()
    {
        var best = Path.Combine(FolderPath, "best");
        Directory.CreateDirectory(best);
        var fast = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(fast);
        
        var exePath = CopyResourceToFile(TesseractResources.tesseract_x64, "tesseract.exe");
        CopyResourceToFile(TesseractResources.eng_traineddata, fast, "eng.traineddata");
        CopyResourceToFile(TesseractResources.heb_traineddata, fast, "heb.traineddata");
        _testImagePath = CopyResourceToFile(TesseractResources.ocr_test, "ocr_test.jpg");
        _testImagePathHebrew = CopyResourceToFile(TesseractResources.ocr_test_hebrew, "ocr_test_hebrew.jpg");

        _engine = new TesseractOcrEngine(exePath, FolderPath);
    }

    private string CopyResourceToFile(byte[] resource, string folder, string fileName)
    {
        string path = Path.Combine(folder, fileName);
        File.WriteAllBytes(path, resource);
        return path;
    }

    private string CopyResourceToFile(byte[] resource, string fileName)
    {
        return CopyResourceToFile(resource, FolderPath, fileName);
    }

    [Fact]
    public async Task ProcessEnglishImage()
    {
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, 0), CancellationToken.None);
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
        var result = await _engine.ProcessImage(_testImagePathHebrew, new OcrParams("heb", OcrMode.Fast, 0), CancellationToken.None);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Elements);
        foreach (var element in result.Elements)
        {
            Assert.Equal("heb", element.LanguageCode);
            Assert.True(element.RightToLeft);
        }
        Assert.Equal("הקדמת", result.Elements[0].Text);
    }

    [Fact]
    public async Task ImmediateCancel()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, 0), cts.Token);
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelWhileProcessing()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(50);
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, 0), cts.Token);
        Assert.Null(result);
    }

    [Fact]
    public async Task Timeout()
    {
        var timeout = 0.1;
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, timeout), CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task NoTimeout()
    {
        var timeout = 10;
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, timeout), CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Mode()
    {
        CopyResourceToFile(TesseractResources.eng_traineddata, Path.Combine(FolderPath, "best"), "eng.traineddata");
        // Bad data for unused mode
        CopyResourceToFile(TesseractResources.heb_traineddata, Path.Combine(FolderPath, "fast"), "eng.traineddata");

        var mode = OcrMode.Best;
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", mode, 0), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("ADVERTISEMENT.", result.Elements[0].Text);
    }
}