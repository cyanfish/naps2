using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.App.Tests;

public class WinFormsAppTests : ContextualTests
{
    [Fact]
    public void CreatesWindow()
    {
        var process = AppTestHelper.StartGuiProcess("NAPS2.exe", FolderPath);
        try
        {
            AppTestHelper.WaitForVisibleWindow(process);
            Assert.Equal("NAPS2 - Not Another PDF Scanner", process.MainWindowTitle);
            Assert.True(process.CloseMainWindow());
            Assert.True(process.WaitForExit(5000));
            AppTestHelper.AssertNoErrorLog(FolderPath);
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }
}