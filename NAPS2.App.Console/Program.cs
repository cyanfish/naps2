using System;
using NAPS2.EntryPoints;

namespace NAPS2.Console
{
    static class Program
    {
        /// <summary>
        /// The NAPS2.Console.exe main method.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Use reflection to avoid antivirus false positives (yes, really)
            typeof(ConsoleEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
