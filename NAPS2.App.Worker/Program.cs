using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI.EntryPoints;

namespace NAPS2.Worker
{
    static class Program
    {
        /// <summary>
        /// The NAPS2.Worker.exe main method.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Use reflection to avoid antivirus false positives (yes, really)
            typeof(WorkerEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
