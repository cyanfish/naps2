using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NAPS2.Portable
{
    class Program
    {
        static void Main(string[] args)
        {
            var portableExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (portableExeDir != null)
            {
                Process.Start(Path.Combine(portableExeDir, @"App\NAPS2.exe"));
            }
        }
    }
}
