using Autofac;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Mocks;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

internal class AutomationHelper
{
    private readonly ContextualTests _testClass;
    private readonly ITestOutputHelper _testOutputHelper;

    public AutomationHelper(ContextualTests testClass, ITestOutputHelper testOutputHelper)
    {
        _testClass = testClass;
        _testOutputHelper = testOutputHelper;
    }

    public Task RunCommand(AutomatedScanningOptions options, params byte[][] imagesToScan)
    {
        return RunCommand(options, null, imagesToScan);
    }

    public Task RunCommand(AutomatedScanningOptions options, Action<IContainer> setup, params byte[][] imagesToScan)
    {
        return RunCommand(options, setup, new ScanDriverFactoryBuilder().WithScannedImages(imagesToScan).Build());
    }

    public Task RunCommand(AutomatedScanningOptions options, IScanDriverFactory scanDriverFactory)
    {
        return RunCommand(options, null, scanDriverFactory);
    }

    public async Task RunCommand(AutomatedScanningOptions options, Action<IContainer> setup, IScanDriverFactory scanDriverFactory)
    {
        var container = AutoFacHelper.FromModules(new CommonModule(), new ConsoleModule(options),
            new TestModule(_testClass.ScanningContext, _testClass.ImageContext, scanDriverFactory, _testOutputHelper,
                _testClass.FolderPath));
        setup?.Invoke(container);
        var automatedScanning = container.Resolve<AutomatedScanning>();
        await automatedScanning.Execute();
    }
}