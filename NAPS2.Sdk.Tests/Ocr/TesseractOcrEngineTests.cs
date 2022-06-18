using System.Threading;
using NAPS2.Ocr;
using Xunit;

namespace NAPS2.Sdk.Tests.Ocr;

public class TesseractOcrEngineTests : ContextualTexts
{
    private readonly TesseractOcrEngine _engine;
    private readonly string _testImagePath;

    public TesseractOcrEngineTests()
    {
        var exePath = Path.Combine(FolderPath, "tesseract.exe");
        File.WriteAllBytes(exePath, TesseractResources.tesseract_x64);
        
        var tessdataPath = Path.Combine(FolderPath, "fast");
        Directory.CreateDirectory(tessdataPath);
        var engDataPath = Path.Combine(tessdataPath, "eng.traineddata");
        File.WriteAllBytes(engDataPath, TesseractResources.eng_traineddata);

        _testImagePath = Path.Combine(FolderPath, "ocr_test.jpg");
        File.WriteAllBytes(_testImagePath, TesseractResources.ocr_test);
        
        _engine = new TesseractOcrEngine(exePath, FolderPath);
    }

    [Fact]
    public async Task RunTesseract()
    {
        var result = await _engine.ProcessImage(_testImagePath, new OcrParams("eng", OcrMode.Fast, 0), CancellationToken.None);
        Assert.NotNull(result);
    } 
}