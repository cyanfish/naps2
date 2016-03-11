using System;
using System.Collections.Generic;
using System.Linq;
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

            var options = new AutomatedScanningOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }
            var scanning = kernel.Get<AutomatedScanning>(new ConstructorArgument("options", options));
            scanning.Execute();
        }
    }
}
