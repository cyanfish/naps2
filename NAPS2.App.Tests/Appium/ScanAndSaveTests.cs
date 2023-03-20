using System.Threading;
using NAPS2.App.Tests.Targets;
using NAPS2.App.Tests.Verification;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.App.Tests.Appium;

#pragma warning disable CS0162
[Collection("appium")]
public class ScanAndSaveTests : AppiumTests
{
    private const string WIA_DEVICE_NAME = "";
    private const string TWAIN_DEVICE_NAME = "";

    [VerifyTheory(AllowDebug = true, WindowsAppium = true)]
    [ClassData(typeof(AppiumTestData))]
    public void ScanWiaSavePdf(IAppTestTarget target)
    {
        Init(target);
        // Clicking Scan without a profile opens the profile settings window
        ClickAtName("Scan");
        // WIA driver is selected by default, so we open the WIA device dialog
        ClickAtName("Choose device");
        Thread.Sleep(100);
        if (WIA_DEVICE_NAME != "") ClickAtName(WIA_DEVICE_NAME);
        // Click OK in the wia device dialog (selecting the first available device by default)
        // TODO: More consistent way to pick the right OK button
        ClickAt(_session.FindElementsByName("OK")[0]);
        WaitUntilGone("Properties", 1_000);
        // Click OK in the profile settings window
        ClickAtName("OK");
        // Wait for scanning to finish
        WaitUntilGone("Cancel", 30_000);
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
        Thread.Sleep(1000);

        PdfAsserts.AssertPageCount(1, Path.Combine(FolderPath, "test.pdf"));
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }

    [VerifyTheory(AllowDebug = true, WindowsAppium = true)]
    [ClassData(typeof(AppiumTestData))]
    public void ScanTwainSaveImage(IAppTestTarget target)
    {
        Init(target);
        // Clicking Scan without a profile opens the profile settings window
        ClickAtName("Scan");
        ClickAtName("TWAIN Driver");
        // Open the TWAIN device dialog
        ClickAtName("Choose device");
        Thread.Sleep(100);
        if (TWAIN_DEVICE_NAME != "") ClickAtName(TWAIN_DEVICE_NAME);
        // Click Select in the twain device dialog (selecting the first available device by default)
        ClickAtName("Select");
        WaitUntilGone("Select", 1_000);
        // Click OK in the profile settings window
        ClickAtName("OK");
        // Wait for scanning to finish
        WaitUntilGone("Cancel", 30_000);
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
        Thread.Sleep(1000);

        ImageAsserts.Inches(Path.Combine(FolderPath, "test.jpg"), 8.5, 11);
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }
}