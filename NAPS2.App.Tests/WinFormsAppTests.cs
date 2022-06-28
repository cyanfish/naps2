using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.App.Tests;

public class WinFormsAppTests : ContextualTexts
{
    [Fact]
    public void CreatesWindow()
    {
        var process = AppTestHelper.StartGuiProcess("NAPS2.exe", FolderPath);
        try
        {
            AppTestHelper.WaitForVisibleWindow(process);
            Assert.Equal("Not Another PDF Scanner 2", process.MainWindowTitle);
            Assert.True(process.CloseMainWindow());
            Assert.True(process.WaitForExit(1000));
            AppTestHelper.AssertNoErrorLog(FolderPath);
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }
    
}