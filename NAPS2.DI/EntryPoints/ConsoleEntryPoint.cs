using CommandLine;
using NAPS2.Automation;
using NAPS2.DI.Modules;
using Ninject;
using Ninject.Parameters;

namespace NAPS2.DI.EntryPoints
{
    public static class ConsoleEntryPoint
    {
        public static void Run(string[] args)
        {
            var kernel = new StandardKernel(new CommonModule(), new ConsoleModule());
            CommandLine.Parser.Default.ParseArguments<AutomatedScanningOptions>(args)
                .WithParsed<AutomatedScanningOptions>(opts => kernel.Get<AutomatedScanning>(new ConstructorArgument("options", opts)).Execute());
        }
    }
}