using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI;
using NAPS2.Host;
using Ninject;

namespace NAPS2_32
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            X86HostEntry.Run(args, KernelManager.Kernel.Get<X86HostService>());
        }
    }
}
