using System.Threading;
using NAPS2.App.Tests.Targets;
using NAPS2.App.Tests.Verification;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.App.Tests.Appium;

#pragma warning disable CS0162
[Collection("appium")]
public class ScanAndSaveTests : AppiumTests
{
    [VerifyTheory(AllowDebug = true, WindowsAppium = true)]
    [ClassData(typeof(AppiumTestData))]
    public void ScanWiaSavePdf(IAppTestTarget target)
    {
        Init(target);
        // Clicking Scan without a profile opens the profile settings window
        ClickAtName("Scan");
        ClickAtName("Choose device");
        WaitFor(() => HasElementWithName("Always Ask"));
        var deviceName = AppTestHelper.GetDeviceName(Driver.Wia);
        // WIA driver is selected by default, so we just click the device
        if (!string.IsNullOrEmpty(deviceName)) ClickAtName(deviceName);
        ClickAt(_session.FindElementByName("Select"));
        WaitFor(() => !HasElementWithName("Select"));
        // Click OK in the profile settings window
        ClickAtName("OK");
        WaitFor(() => HasElementWithName("Cancel"));
        // Wait for scanning to finish
        WaitFor(() => !HasElementWithName("Cancel"), 30_000);
        ResetMainWindow();
        // Save "test.pdf" in the default location (which will be the test data path as NAPS2 knows we're in a test)^
        ClickAtName("Save PDF");
        ResetMainWindow();
        var fileTextBox = WaitFor(() => _session.FindElementsByName("File name:").Last());
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
        ClickAtName("Choose device");
        WaitFor(() => HasElementWithName("Always Ask"));
        var deviceName = AppTestHelper.GetDeviceName(Driver.Twain);
        ClickAtName("TWAIN Driver");
        Thread.Sleep(100);
        WaitFor(() => HasElementWithName("Always Ask"));
        if (!string.IsNullOrEmpty(deviceName)) ClickAtName(deviceName);
        ClickAtName("Select");
        WaitFor(() => !HasElementWithName("Select"));
        // Click OK in the profile settings window
        ClickAtName("OK");
        WaitFor(() => HasElementWithName("Cancel"));
        // Wait for scanning to finish
        WaitFor(() => !HasElementWithName("Cancel"), 30_000);
        ResetMainWindow();
        // Save "test.pdf" in the default location (which will be the test data path as NAPS2 knows we're in a test)^
        ClickAtName("Save Images");
        ResetMainWindow();
        var fileTextBox = WaitFor(() => _session.FindElementsByName("File name:").Last());
        ClickAt(fileTextBox);
        fileTextBox.SendKeys("test.jpg");
        ClickAtName("Save");
        // Wait for the save to finish, it should be almost instant
        Thread.Sleep(1000);

        ImageAsserts.Inches(Path.Combine(FolderPath, "test.jpg"), 8.5, 11);
        AppTestHelper.AssertNoErrorLog(FolderPath);
    }
}