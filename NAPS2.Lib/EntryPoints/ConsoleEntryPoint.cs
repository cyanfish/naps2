using Autofac;
using CommandLine;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Console.exe (on Windows) and "naps2 cli" (on Mac/Linux), the NAPS2 CLI.
/// </summary>
public static class ConsoleEntryPoint
{
    public static int Run(string[] args, Module imageModule)
    {
        // Parse the command-line arguments (and display help text if appropriate)
        var options = Parser.Default.ParseArguments<AutomatedScanningOptions>(args).Value;
        if (options == null)
        {
            return 0;
        }

        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(
            new CommonModule(), imageModule, new ConsoleModule(options), new RecoveryModule(), new ContextModule());

        Paths.ClearTemp();

        // Start a pending worker process
        container.Resolve<IWorkerFactory>().Init(new WorkerFactoryInitOptions { StartSpareWorkers = false });

        // Run the scan automation logic
        var scanning = container.Resolve<AutomatedScanning>();
        scanning.Execute().Wait();

        return ((ConsoleErrorOutput) container.Resolve<ErrorOutput>()).HasError ? 1 : 0;
    }
}