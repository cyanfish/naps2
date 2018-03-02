using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI.EntryPoints;
using NAPS2.Worker;

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
            typeof(WinFormsEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
