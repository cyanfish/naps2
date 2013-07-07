using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Parameters;

namespace NAPS2.Console
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var options = new AutomatedScanningOptions();
                if (!CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    return;
                }
                var scanning = KernelManager.Kernel.Get<AutomatedScanning>(new ConstructorArgument("options", options));
                scanning.Execute();
                return;
            }
        }
    }
}
