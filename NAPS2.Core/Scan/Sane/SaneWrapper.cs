using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Exceptions;
using NAPS2.Util;

namespace NAPS2.Scan.Sane
{
    public class SaneWrapper
    {
        private const string SCANIMAGE = "scanimage";

        private readonly Regex ProgressRegex = new Regex(@"^Progress: (\d+(\.\d+)?)%");

        public IEnumerable<ScanDevice> GetDeviceList()
        {
            var proc = StartProcess(SCANIMAGE, @"-f %d|%m%n");

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

        public Stream ScanOne(string deviceId, KeyValueScanOptions options, ProgressHandler progressCallback)
        {
            var profileOptions = options == null ? "" : string.Join("", options.Select(kvp => $@" {kvp.Key} ""{kvp.Value.Replace("\"", "\\\"")}"""));
            var allOptions = $@"-d ""{deviceId}"" --format=tiff --progress{profileOptions}";
            var proc = StartProcess(SCANIMAGE, allOptions);
            var errorOutput = new List<string>();
            var waitHandle = new AutoResetEvent(false);
            bool cancelled = false;
            proc.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    var match = ProgressRegex.Match(args.Data);
                    if (match.Success)
                    {
                        var result = progressCallback?.Invoke((int)float.Parse(match.Groups[1].Value) * 10, 1000);
                        if (result.HasValue && !result.Value && !cancelled)
                        {
                            cancelled = true;
                            Signal(proc, 2);
                        }
                    }
                    else
                    {
                        errorOutput.Add(args.Data);
                    }
                }
            };
            proc.Exited += (sender, args) => waitHandle.Set();
            proc.BeginErrorReadLine();
            var outputStream = new MemoryStream();
            proc.StandardOutput.BaseStream.CopyTo(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            waitHandle.WaitOne();
            if (cancelled)
            {
                return null;
            }
            if (errorOutput.Count > 0)
            {
                string errorMessage = string.Join(". ", errorOutput).Trim();
                if (errorMessage.EndsWith("Device busy", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new DeviceException(MiscResources.DeviceBusy);
                }
                if (errorMessage.EndsWith("Invalid argument", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new DeviceException(MiscResources.DeviceNotFound);
                }
                throw new ScanDriverUnknownException(new Exception(errorMessage));
            }
            return outputStream;
        }

        private static void Signal(Process proc, int signum)
        {
            var posix = Assembly.Load("Mono.Posix");
            var syscall = posix?.GetType("Mono.Unix.Native.Syscall");
            var kill = syscall?.GetMethod("kill", BindingFlags.Static | BindingFlags.Public);
            kill?.Invoke(null, new object[] { proc.Id, signum });
        }

        private static Process StartProcess(string fileName, string args)
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                proc.Start();
                return proc;
            }
            catch (Exception e)
            {
                throw new SaneNotAvailableException(e);
            }
        }
    }
}
