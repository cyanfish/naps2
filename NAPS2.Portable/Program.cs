using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NAPS2.Portable
{
    internal static class Program
    {
        private static void Main()
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