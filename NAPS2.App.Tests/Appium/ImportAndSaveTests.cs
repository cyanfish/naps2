using System.Threading;
using NAPS2.App.Tests.Verification;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.App.Tests.Appium;

[Collection("appium")]
public class ImportAndSaveTests : AppiumTests
{
    [VerifyFact(AllowDebug = true)]
    public void ImportVariousAndSavePdfWithOcr()
    {
        File.WriteAllBytes(Path.Combine(FolderPath, "word.pdf"), PdfResources.word_generated_pdf);
        File.WriteAllBytes(Path.Combine(FolderPath, "image.pdf"), PdfResources.image_pdf);
        File.WriteAllBytes(Path.Combine(FolderPath, "text.jpg"), BinaryResources.ocr_test);
        var tessdata = Path.Combine(FolderPath, "components", "tesseract-4.0.0b4", "fast");
        Directory.CreateDirectory(tessdata);
        File.WriteAllBytes(Path.Combine(tessdata, "eng.traineddata"), BinaryResources.eng_traineddata);
        
        ClickAtName("Import");
        DoubleClickAtName("word.pdf");
        ResetMainWindow();
        Thread.Sleep(100);
        ClickAtName("Import");
        DoubleClickAtName("image.pdf");
        ResetMainWindow();
        Thread.Sleep(100);
        ClickAtName("Import");
        DoubleClickAtName("text.jpg");
        ResetMainWindow();
        Thread.Sleep(100);
        
        ClickAtName("OCR");
        ClickAtName("Make PDFs searchable using OCR");
        ClickAtName("OK");
        
        ClickAtName("Save PDF");
        ResetMainWindow();
        var fileNameElements = _session.FindElementsByName("File name:");
        var fileTextBox = fileNameElements.Last();
        ClickAt(fileTextBox);
        fileTextBox.SendKeys("test.pdf");
        ClickAtName("Save");
        // Wait for the save to finish
        WaitUntilGone("Cancel");

        var path = Path.Combine(FolderPath, "test.pdf");
        PdfAsserts.AssertImages(path, 
            PdfResources.word_p1,
            PdfResources.word_p2,
            ImageResources.color_image,
            ImageResources.ocr_test);
        PdfAsserts.AssertContainsTextOnce("ADVERTISEMENT.", path);
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }
}