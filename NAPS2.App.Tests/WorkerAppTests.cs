using GrpcDotNetNamedPipes;
using NAPS2.Remoting.Worker;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.App.Tests;

public class WorkerAppTests : ContextualTexts
{
    [Fact]
    public void CreatesPipeServer()
    {
        var process = AppTestHelper.StartProcess("NAPS2.Worker.exe", FolderPath, Process.GetCurrentProcess().Id.ToString());
        try
        {
            Assert.Equal("ready", process.StandardOutput.ReadLine());
            string pipeName = $"NAPS2.Worker/{process.Id}";
            var client = new WorkerServiceAdapter(new NamedPipeChannel(".", pipeName));
            client.Init(FolderPath);
            AppTestHelper.AssertNoErrorLog(FolderPath);
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }
}