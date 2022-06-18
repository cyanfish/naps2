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
        var tessdataPath = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(tessdataPath);
        
        var exePath = CopyResourceToFile(TesseractResources.tesseract_x64, "tesseract.exe");
        CopyResourceToFile(TesseractResources.eng_traineddata, tessdataPath, "eng.traineddata");
        CopyResourceToFile(TesseractResources.heb_traineddata, tessdataPath, "heb.traineddata");
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
    public async Task RunTesseract()
    {
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, 0), CancellationToken.None);
        Assert.NotNull(result);
        foreach (var element in result.Elements)
        {
            Assert.Equal("eng", element.LanguageCode);
            Assert.False(element.RightToLeft);
        }
    }

    [Fact]
    public async Task RunTesseractHebrew()
    {
        var result = await _engine.ProcessImage(_testImagePathHebrew, new OcrParams("heb", OcrMode.Fast, 0), CancellationToken.None);
        Assert.NotNull(result);
        foreach (var element in result.Elements)
        {
            Assert.Equal("heb", element.LanguageCode);
            Assert.True(element.RightToLeft);
        }
    } 
}