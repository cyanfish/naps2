using NAPS2.DI.EntryPoints;
using System;

namespace NAPS2.Console
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            typeof(ConsoleEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}