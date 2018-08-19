using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NAPS2.Scan.Sane
{
    public class SaneWrapper
    {
        private const string SCANIMAGE = "scanimage";
        private const string DEVICE_LIST_ARGS = @"-f %d|%m%n";

        public IEnumerable<ScanDevice> GetDeviceList()
        {
            var proc = StartProcess(SCANIMAGE, DEVICE_LIST_ARGS);

            string line;
            while ((line = proc.StandardOutput.ReadLine()?.Trim()) != null)
            {
                string[] parts = line.Split('|');
                if (parts.Length == 2)
                {
                    yield return new ScanDevice(parts[0], parts[1]);
                }
            }
        }

        private static Process StartProcess(string fileName, string args)
        {
            Process proc;
            try
            {
                proc = Process.Start(new ProcessStartInfo
                {
                    FileName = SCANIMAGE,
                    Arguments = DEVICE_LIST_ARGS,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });
            }
            catch (Exception e)
            {
                throw new SaneNotAvailableException(e);
            }
            if (proc == null)
            {
                throw new SaneNotAvailableException();
            }
            return proc;
        }
    }
}
