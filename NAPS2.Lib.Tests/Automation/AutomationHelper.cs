using System.Drawing;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Sdk.Tests;
using Ninject;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

public class AutomationHelper
{
    private readonly ContextualTests _testClass;
    private readonly ITestOutputHelper _testOutputHelper;

    public AutomationHelper(ContextualTests testClass, ITestOutputHelper testOutputHelper)
    {
        _testClass = testClass;
        _testOutputHelper = testOutputHelper;
    }

    public Task RunCommand(AutomatedScanningOptions options, params Bitmap[] imagesToScan)
    {
        return RunCommand(options, null, imagesToScan);
    }

    public async Task RunCommand(AutomatedScanningOptions options, Action<IKernel> setup, params Bitmap[] imagesToScan)
    {
        var scanDriverFactory = new ScanDriverFactoryBuilder().WithScannedImages(imagesToScan).Build();
        var kernel = new StandardKernel(new CommonModule(), new ConsoleModule(options),
            new TestModule(_testClass.ScanningContext, _testClass.ImageContext, scanDriverFactory, _testOutputHelper,
                _testClass.FolderPath));
        setup?.Invoke(kernel);
        var automatedScanning = kernel.Get<AutomatedScanning>();
        await automatedScanning.Execute();
    }
}