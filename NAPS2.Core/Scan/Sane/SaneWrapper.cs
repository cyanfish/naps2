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
        private const int SIGINT = 2;
        private const int SIGTERM = 15;
        private const int SIGKILL = 9;
        private static readonly Regex ProgressRegex = new Regex(@"^Progress: (\d+(\.\d+)?)%");

        private readonly ThreadFactory threadFactory;

        public SaneWrapper(ThreadFactory threadFactory)
        {
            this.threadFactory = threadFactory;
        }

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
            // Start the scanning process
            var profileOptions = options == null ? "" : string.Join("", options.Select(kvp => $@" {kvp.Key} ""{kvp.Value.Replace("\"", "\\\"")}"""));
            var allOptions = $@"-d ""{deviceId}"" --format=tiff --progress{profileOptions}";
            var proc = StartProcess(SCANIMAGE, allOptions);

            // Set up state
            var procExitWaitHandle = new AutoResetEvent(false);
            var outputFinishedWaitHandle = new AutoResetEvent(false);
            var errorOutput = new List<string>();
            bool cancelled = false;
            int currentProgress = 0;
            const int maxProgress = 1000;

            // Set up events
            proc.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    var match = ProgressRegex.Match(args.Data);
                    if (match.Success)
                    {
                        currentProgress = (int) float.Parse(match.Groups[1].Value) * 10;
                    }
                    else
                    {
                        errorOutput.Add(args.Data);
                    }
                }
            };
            proc.Exited += (sender, args) => procExitWaitHandle.Set();
            proc.BeginErrorReadLine();

            // Read the image output into a MemoryStream off-thread
            var outputStream = new MemoryStream();
            threadFactory.StartThread(() =>
            {
                proc.StandardOutput.BaseStream.CopyTo(outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);
                outputFinishedWaitHandle.Set();
            });

            // Wait for the process to stop (or for the user to cancel)
            while (!procExitWaitHandle.WaitOne(200))
            {
                if (progressCallback?.Invoke(currentProgress, maxProgress) == false)
                {
                    cancelled = true;
                    SafeStopProcess(proc, procExitWaitHandle);
                    break;
                }
            }
            // Ensure the image output thread has finished so we don't return an incomplete MemoryStream
            outputFinishedWaitHandle.WaitOne();

            if (cancelled)
            {
                // The user has cancelled, so we can ignore everything else
                return null;
            }
            if (errorOutput.Count > 0)
            {
                // Non-progress output to stderr indicates that the scan was not successful
                string stderr = string.Join(". ", errorOutput).Trim();
                ThrowDeviceError(stderr);
            }
            // No unexpected stderr output, so we can assume that the output stream is complete and valid
            return outputStream;
        }

        private static void ThrowDeviceError(string stderr)
        {
            if (stderr.EndsWith("Device busy", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new DeviceException(MiscResources.DeviceBusy);
            }
            if (stderr.EndsWith("Invalid argument", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new DeviceException(MiscResources.DeviceNotFound);
            }
            throw new ScanDriverUnknownException(new Exception(stderr));
        }

        private static void SafeStopProcess(Process proc, AutoResetEvent procExitWaitHandle)
        {
            Signal(proc, SIGINT);
            if (!procExitWaitHandle.WaitOne(5000))
            {
                Signal(proc, SIGTERM);
                if (!procExitWaitHandle.WaitOne(1000))
                {
                    Signal(proc, SIGKILL);
                }
            }
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
