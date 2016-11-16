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
                var portableExePath = Path.Combine(portableExeDir, @"App\NAPS2.exe");
                typeof(Process).GetMethod("Start", new[] { typeof(string) }).Invoke(null, new object[] { portableExePath });
            }
        }
    }
}
