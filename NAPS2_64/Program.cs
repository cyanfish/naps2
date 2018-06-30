using NAPS2.DI.EntryPoints;
using System;

namespace NAPS2_64
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
            typeof(WinFormsEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}