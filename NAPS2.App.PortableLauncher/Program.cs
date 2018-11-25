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
                try
                {
                    if (args.Length == 3 && args[0] == "/Update")
                    {
                        UpdatePortableApp(portableExeDir, args[1], args[2]);
                    }
                }
                finally
                {
                    var portableExePath = Path.Combine(portableExeDir, "App", "NAPS2.exe");
                    typeof(Process).GetMethod("Start", new[] {typeof(string)}).Invoke(null, new object[] {portableExePath});
                }
            }
        }

        private static void UpdatePortableApp(string portableExeDir, string procId, string newAppFolderPath)
        {
            // Wait for the starting process to finish so we don't try to mess with files in use
            try
            {
                var proc = Process.GetProcessById(int.Parse(procId));
                proc.WaitForExit();
            }
            catch (ArgumentException)
            {
            }

            // Safely replace the old App folder
            AtomicReplaceDirectory(newAppFolderPath, Path.Combine(portableExeDir, "App"));
        }

        private static void AtomicReplaceDirectory(string source, string dest)
        {
            string temp = dest + ".old";
            Directory.Move(dest, temp);
            try
            {
                Directory.Move(source, dest);
                Directory.Delete(temp, true);
            }
            catch (Exception)
            {
                Directory.Move(temp, dest);
                throw;
            }
        }
    }
}
