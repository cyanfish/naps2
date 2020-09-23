using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private static readonly Regex ProgressRegex = new Regex(@"^Progress: (\d+(\.\d+)?)%");

        public IEnumerable<ScanDevice> GetDeviceList()
        {
            var proc = StartProcess(SCANIMAGE, @"--formatted-device-list=%d|%m%n");

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

        public SaneOptionCollection GetOptions(string deviceId)
        {
            var proc = StartProcess(SCANIMAGE, $@"--help --device-name={deviceId}");
            return new SaneOptionParser().Parse(proc.StandardOutput);
        }

        public Stream ScanOne(string deviceId, KeyValueScanOptions options, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            // Start the scanning process
            var profileOptions = options == null ? "" : string.Join("", options.Select(kvp => $@" {kvp.Key} ""{kvp.Value.Replace("\"", "\\\"")}"""));
            var allOptions = $@"--device-name=""{deviceId}"" --format=tiff --progress{profileOptions}";
            var proc = StartProcess(SCANIMAGE, allOptions);

            // Set up state
            var procExitWaitHandle = new ManualResetEvent(false);
            var outputFinishedWaitHandle = new ManualResetEvent(false);
            var errorOutput = new List<string>();
            bool cancelled = false;
            const int maxProgress = 1000;

            // Set up events
            proc.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    var match = ProgressRegex.Match(args.Data);
                    if (match.Success)
                    {
                        progressCallback?.Invoke((int)float.Parse(match.Groups[1].Value) * 10, maxProgress);
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
            Task.Factory.StartNew(() =>
            {
                proc.StandardOutput.BaseStream.CopyTo(outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);
                outputFinishedWaitHandle.Set();
            }, TaskCreationOptions.LongRunning);

            // Wait for the process to stop (or for the user to cancel)
            WaitHandle.WaitAny(new[] { procExitWaitHandle, cancelToken.WaitHandle });
            if (cancelToken.IsCancellationRequested)
            {
                cancelled = true;
                SafeStopProcess(proc, procExitWaitHandle);
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

        private static void SafeStopProcess(Process proc, WaitHandle procExitWaitHandle)
        {
            Signal(proc, SIGINT);
            if (!procExitWaitHandle.WaitOne(5000))
            {
                Signal(proc, SIGTERM);
                if (!procExitWaitHandle.WaitOne(1000))
                {
                    proc.Kill();
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
