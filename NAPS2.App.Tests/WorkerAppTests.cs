using Xunit;

namespace NAPS2.App.Tests;

public class WorkerAppTests
{
    [Fact]
    public void CreatesWindow()
    {
        var process = AppTestHelper.StartProcess("NAPS2.Worker.exe", Process.GetCurrentProcess().Id.ToString());
        try
        {
            Assert.Equal("ready", process.StandardOutput.ReadLine());
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }
}