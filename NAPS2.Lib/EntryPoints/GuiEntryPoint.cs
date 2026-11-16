using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
#if TEST_SCENARIOS
using NAPS2.TestScenarios;
#endif

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for the main NAPS2 executable.
/// </summary>
public static class GuiEntryPoint
{
    public static int Run(string[] args, Module imageModule, Module platformModule)
    {
#if TEST_SCENARIOS
        var testScenarioModule = TestScenarioModule.CreateFromEnvironment();
#endif

        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(
            new CommonModule(), imageModule, platformModule, new GuiModule(), new RecoveryModule(), new StaticInitModule()
#if TEST_SCENARIOS
            , testScenarioModule
#endif
        );

        Paths.ClearTemp();

        // Parse the command-line arguments and see if we're doing something other than displaying the main form
        var lifecycle = container.Resolve<ApplicationLifecycle>();
        lifecycle.ParseArgs(args);
        lifecycle.ExitIfRedundant();

        EtoPlatform.Current.InitializeForegroundApp();

        // Set up basic application configuration
        container.Resolve<CultureHelper>().SetCulturesFromConfig();
        Trace.Listeners.Add(new ConsoleTraceListener());

        // Spare worker processes are started once the main form is shown, as they compete with it for CPU
        var workerFactory = container.Resolve<IWorkerFactory>();
        var scanningContext = container.Resolve<ScanningContext>();
        workerFactory.Init(scanningContext, new WorkerFactoryInitOptions { StartSpareWorkers = false });

        // Show the main form
        var application = EtoPlatform.Current.CreateApplication();
        Invoker.Current = new EtoInvoker(application);
        var formFactory = container.Resolve<IFormFactory>();
        var desktop = formFactory.Create<DesktopForm>();
        desktop.Shown += (_, _) => workerFactory.StartSpareWorkers(scanningContext);
#if TEST_SCENARIOS
        testScenarioModule?.Attach(container, desktop);
#endif

        application.Run(desktop);
        return 0;
    }
}