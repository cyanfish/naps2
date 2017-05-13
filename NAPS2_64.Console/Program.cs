using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.DI.EntryPoints;

namespace NAPS2_64.Console
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            typeof(ConsoleEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
        }
    }
}
