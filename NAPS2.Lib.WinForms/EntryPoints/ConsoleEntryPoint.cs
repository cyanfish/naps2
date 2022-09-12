using CommandLine;
using NAPS2.Automation;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;
using Ninject;

namespace NAPS2.EntryPoints;

// TODO: NAPS2.Console.exe should probably be Windows-only, but we should add command-line capabilities to the Mac/Linux
// TODO: executables too (e.g. "./NAPS2 console --argshere", also "./NAPS2 worker argshere").
/// <summary>
/// The entry point for NAPS2.Console.exe, the NAPS2 CLI.
/// </summary>
public static class ConsoleEntryPoint
{
    public static int Run(string[] args)
    {
        // Parse the command-line arguments (and display help text if appropriate)
        var options = Parser.Default.ParseArguments<AutomatedScanningOptions>(args).Value;
        if (options == null)
        {
            return 0;
        }

        // Initialize Ninject (the DI framework)
        var kernel = new StandardKernel(new CommonModule(), new GdiModule(), new ConsoleModule(options),
            new RecoveryModule(), new ContextModule());

        Paths.ClearTemp();

        // Start a pending worker process
        kernel.Get<IWorkerFactory>().Init();

        // Run the scan automation logic
        var scanning = kernel.Get<AutomatedScanning>();
        scanning.Execute().Wait();

        return ((ConsoleErrorOutput) kernel.Get<ErrorOutput>()).HasError ? 1 : 0;
    }
}