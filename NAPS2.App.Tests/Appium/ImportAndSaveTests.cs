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
        CopyResourceToFile(PdfResources.word_generated_pdf, "word.pdf");
        CopyResourceToFile(PdfResources.word_patcht_pdf, "patcht.pdf");
        CopyResourceToFile(PdfResources.image_pdf, "image.pdf");
        CopyResourceToFile(BinaryResources.ocr_test, "text.jpg");
        var tessdata = Path.Combine(FolderPath, "components", "tesseract4", "fast");
        Directory.CreateDirectory(tessdata);
        CopyResourceToFile(BinaryResources.eng_traineddata, tessdata, "eng.traineddata");
        
        ImportFile("word.pdf");
        ImportFile("patcht.pdf");
        ImportFile("image.pdf");
        ImportFile("text.jpg");
        
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
        Thread.Sleep(100);
        WaitUntilGone("Cancel", 10_000);

        var path = Path.Combine(FolderPath, "test.pdf");
        PdfAsserts.AssertImages(path, 
            PdfResources.word_p1,
            PdfResources.word_p2,
            PdfResources.word_patcht_p1,
            ImageResources.dog,
            ImageResources.ocr_test);
        PdfAsserts.AssertContainsTextOnce("Page one.", path);
        PdfAsserts.AssertContainsTextOnce("Page two.", path);
        PdfAsserts.AssertContainsTextOnce("ADVERTISEMENT.", path);
        PdfAsserts.AssertContainsTextOnce("Sized for printing unscaled", path);
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }

    private void ImportFile(string fileName)
    {
        ClickAtName("Import");
        DoubleClickAtName(fileName);
        ResetMainWindow();
        Thread.Sleep(100);
    }
}