using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI.EntryPoints;
using NAPS2.Host;

namespace NAPS2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains(X86HostManager.HOST_ARG))
            {
                typeof(X86HostEntryPoint).GetMethod("Run").Invoke(null, new object[] {args});
            }
            else
            {
                typeof(WinFormsEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
            }
        }
    }
}
