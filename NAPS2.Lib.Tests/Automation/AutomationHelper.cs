using Autofac;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Scan.Internal;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Mocks;

namespace NAPS2.Lib.Tests.Automation;

internal class AutomationHelper
{
    private readonly ContextualTests _testClass;
    private readonly TestOutputTextWriter _outputWriter;
    private Action<ContainerBuilder> _containerBuilderSetup;
    private Action<IContainer> _containerSetup;

    public AutomationHelper(ContextualTests testClass, TestOutputTextWriter outputWriter)
    {
        _testClass = testClass;
        _outputWriter = outputWriter;
    }

    public string Output => _outputWriter.Output;

    public AutomationHelper WithContainer(Action<IContainer> setup)
    {
        return new AutomationHelper(_testClass, _outputWriter)
        {
            _containerSetup = setup
        };
    }

    public AutomationHelper WithContainerBuilder(Action<ContainerBuilder> setup)
    {
        return new AutomationHelper(_testClass, _outputWriter)
        {
            _containerBuilderSetup = setup
        };
    }

    public Task RunCommand(AutomatedScanningOptions options, params byte[][] imagesToScan)
    {
        return RunCommand(options, new ScanDriverFactoryBuilder().WithScannedImages(imagesToScan).Build());
    }

    public async Task RunCommand(AutomatedScanningOptions options, IScanDriverFactory scanDriverFactory)
    {
        var testModule = new TestModule(_testClass.ScanningContext, _testClass.ImageContext, scanDriverFactory,
            _outputWriter, _testClass.FolderPath, _containerBuilderSetup);
        var container = AutoFacHelper.FromModules(new CommonModule(), new ConsoleModule(options), testModule);
        _containerSetup?.Invoke(container);
        var automatedScanning = container.Resolve<AutomatedScanning>();
        await automatedScanning.Execute();
    }
}