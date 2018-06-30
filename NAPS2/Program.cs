using NAPS2.DI.EntryPoints;
using NAPS2.Host;
using System;
using System.Linq;

namespace NAPS2
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Contains(X86HostManager.HOST_ARG))
            {
                typeof(X86HostEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
            }
            else
            {
                typeof(WinFormsEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
            }
        }
    }
}