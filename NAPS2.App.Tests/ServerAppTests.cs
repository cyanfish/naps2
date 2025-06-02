using NAPS2.App.Tests.Targets;
using NAPS2.Remoting;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.App.Tests;

public class ServerAppTests : ContextualTests
{
    [GuiTheory]
    [ClassData(typeof(AppTestData))]
    public void StartAndStopServer(IAppTestTarget target)
    {
        var process = AppTestHelper.StartProcess(target.Server, FolderPath);
        try
        {
            var helper = ProcessCoordinator.CreateDefault();
            Assert.True(helper.StopSharingServer(process, 5000));
            Assert.True(process.WaitForExit(5000));
            AppTestHelper.AssertNoErrorLog(FolderPath);
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }
}