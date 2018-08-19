using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI.EntryPoints;

namespace NAPS2
{
    static class Program
    {
        /// <summary>
        /// The NAPS2.exe main method.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Use reflection to avoid antivirus false positives (yes, really)
            typeof(WinFormsEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
