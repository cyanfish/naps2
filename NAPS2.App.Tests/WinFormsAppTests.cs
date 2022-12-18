using NAPS2.App.Tests.Targets;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.App.Tests;

public class WinFormsAppTests : ContextualTests
{
    [Theory]
    [ClassData(typeof(AppTestData))]
    public void CreatesWindow(IAppTestTarget target)
    {
        var process = AppTestHelper.StartGuiProcess(target.Gui, FolderPath);
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