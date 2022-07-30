using System.Threading;
using NAPS2.App.Tests.Verification;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.App.Tests.Appium;

[Collection("appium")]
public class ScanAndSaveTests : AppiumTests
{
    [VerifyFact(AllowDebug = true)]
    public void ScanWiaSavePdf()
    {
        // Clicking Scan without a profile opens the profile settings window
        ClickAtName("Scan");
        // WIA driver is selected by default, so we open the WIA device dialog
        ClickAtName("Choose device");
        Thread.Sleep(100);
        // Click OK in the wia device dialog (selecting the first available device by default)
        ClickAt(_session.FindElementsByName("OK")[1]);
        // Click OK in the profile settings window
        ClickAtName("OK");
        // Wait for scanning to finish
        WaitUntilGone("Cancel");
        ResetMainWindow();
        // Save "test.pdf" in the default location (which will be the test data path as NAPS2 knows we're in a test)^
        ClickAtName("Save PDF");
        ResetMainWindow();
        var fileNameElements = _session.FindElementsByName("File name:");
        var fileTextBox = fileNameElements.Last();
        ClickAt(fileTextBox);
        fileTextBox.SendKeys("test.pdf");
        ClickAtName("Save");
        // Wait for the save to finish, it should be almost instant
        Thread.Sleep(200);

        PdfAsserts.AssertPageCount(1, Path.Combine(FolderPath, "test.pdf"));
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }

    [VerifyFact(AllowDebug = true)]
    public void ScanTwainSaveImage()
    {
        // Clicking Scan without a profile opens the profile settings window
        ClickAtName("Scan");
        ClickAtName("TWAIN Driver");
        // Open the TWAIN device dialog
        ClickAtName("Choose device");
        Thread.Sleep(100);
        // Click Select in the twain device dialog (selecting the first available device by default)
        ClickAtName("Select");
        // Click OK in the profile settings window
        ClickAtName("OK");
        // Wait for scanning to finish
        WaitUntilGone("Cancel");
        ResetMainWindow();
        // Save "test.pdf" in the default location (which will be the test data path as NAPS2 knows we're in a test)^
        ClickAtName("Save Images");
        ResetMainWindow();
        var fileNameElements = _session.FindElementsByName("File name:");
        var fileTextBox = fileNameElements.Last();
        ClickAt(fileTextBox);
        fileTextBox.SendKeys("test.jpg");
        ClickAtName("Save");
        // Wait for the save to finish, it should be almost instant
        Thread.Sleep(200);

        ImageAsserts.Inches(Path.Combine(FolderPath, "test.jpg"), 8.5, 11);
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }
}