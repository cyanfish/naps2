using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI.EntryPoints;

namespace NAPS2.Worker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            typeof(WorkerEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
