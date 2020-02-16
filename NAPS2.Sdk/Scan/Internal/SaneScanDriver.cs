using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Sane;
using NAPS2.Util;

namespace NAPS2.Scan.Internal
{
    internal class SaneScanDriver : IScanDriver
    {
        private const string SCANIMAGE = "scanimage";
        private const int SIGINT = 2;
        private const int SIGTERM = 15;
        private static readonly Regex ProgressRegex = new Regex(@"^Progress: (\d+(\.\d+)?)%");

        private static readonly Dictionary<string, SaneOptionCollection> SaneOptionCache = new Dictionary<string, SaneOptionCollection>();

        private readonly ImageContext _imageContext;

        public SaneScanDriver(ImageContext imageContext)
        {
            _imageContext = imageContext;
        }

        public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
        {
            return Task.Run(() =>
            {
                var deviceList = new List<ScanDevice>();
                var proc = StartProcess(SCANIMAGE, @"--formatted-device-list=%d|%m%n");

                string line;
                while ((line = proc.StandardOutput.ReadLine()?.Trim()) != null)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        deviceList.Add(new ScanDevice(parts[0], parts[1]));
                    }
                }

                return deviceList;
            });
        }

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
        {
            return Task.Run(() =>
            {
                // TODO: Test ADF
                var keyValueOptions = new Lazy<KeyValueScanOptions>(() => GetKeyValueOptions(options));
                scanEvents.PageStart();
                bool result = Transfer(keyValueOptions, options, cancelToken, scanEvents, callback);

                if (result && options.PaperSource != PaperSource.Flatbed)
                {
                    try
                    {
                        do
                        {
                            scanEvents.PageStart();
                        } while (Transfer(keyValueOptions, options, cancelToken, scanEvents, callback));
                    }
                    catch (Exception e)
                    {
                        Log.ErrorException("Error in SANE. This may be a normal ADF termination.", e);
                    }
                }
            });
        }

        private Stream ScanOne(string deviceId, KeyValueScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents)
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

            // Set up events
            proc.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    var match = ProgressRegex.Match(args.Data);
                    if (match.Success)
                    {
                        scanEvents.PageProgress(double.Parse(match.Groups[1].Value) / 100);
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
            Task.Run(() =>
            {
                proc.StandardOutput.BaseStream.CopyTo(outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);
                outputFinishedWaitHandle.Set();
            });

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

        private SaneOptionCollection GetAvailableOptions(string deviceId)
        {
            var proc = StartProcess(SCANIMAGE, $@"--help --device-name={deviceId}");
            return new SaneOptionParser().Parse(proc.StandardOutput);
        }

        private KeyValueScanOptions GetKeyValueOptions(ScanOptions options)
        {
            var availableOptions = SaneOptionCache.GetOrSet(options.Device.ID, () => GetAvailableOptions(options.Device.ID));
            var keyValueOptions = new KeyValueScanOptions(options.SaneOptions.KeyValueOptions ?? new KeyValueScanOptions());

            bool ChooseStringOption(string name, Func<string, bool> match)
            {
                var opt = availableOptions.Get(name);
                var choice = opt?.StringList?.FirstOrDefault(match);
                if (choice != null)
                {
                    keyValueOptions[name] = choice;
                    return true;
                }
                return false;
            }

            bool ChooseNumericOption(string name, decimal value)
            {
                var opt = availableOptions.Get(name);
                if (opt?.ConstraintType == SaneConstraintType.WordList)
                {
                    var choice = opt.WordList?.OrderBy(x => Math.Abs(x - value)).FirstOrDefault();
                    if (choice != null)
                    {
                        keyValueOptions[name] = choice.Value.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                }
                else if (opt?.ConstraintType == SaneConstraintType.Range)
                {
                    if (value < opt.Range.Min)
                    {
                        value = opt.Range.Min;
                    }
                    if (value > opt.Range.Max)
                    {
                        value = opt.Range.Max;
                    }
                    if (opt.Range.Quant != 0)
                    {
                        var mod = (value - opt.Range.Min) % opt.Range.Quant;
                        if (mod != 0)
                        {
                            value = mod < opt.Range.Quant / 2 ? value - mod : value + opt.Range.Quant - mod;
                        }
                    }
                    keyValueOptions[name] = value.ToString("0.#####", CultureInfo.InvariantCulture);
                    return true;
                }
                return false;
            }

            bool IsFlatbedChoice(string choice) => choice.IndexOf("flatbed", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool IsFeederChoice(string choice) => new[] { "adf", "feeder", "simplex" }.Any(x => choice.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
            bool IsDuplexChoice(string choice) => choice.IndexOf("duplex", StringComparison.InvariantCultureIgnoreCase) >= 0;

            if (options.PaperSource == PaperSource.Flatbed)
            {
                ChooseStringOption("--source", IsFlatbedChoice);
            }
            else if (options.PaperSource == PaperSource.Feeder)
            {
                if (!ChooseStringOption("--source", x => IsFeederChoice(x) && !IsDuplexChoice(x)) &&
                    !ChooseStringOption("--source", IsFeederChoice) &&
                    !ChooseStringOption("--source", IsDuplexChoice))
                {
                    throw new NoFeederSupportException();
                }
            }
            else if (options.PaperSource == PaperSource.Duplex)
            {
                if (!ChooseStringOption("--source", IsDuplexChoice))
                {
                    throw new NoDuplexSupportException();
                }
            }

            if (options.BitDepth == BitDepth.Color)
            {
                ChooseStringOption("--mode", x => x == "Color");
                ChooseNumericOption("--depth", 8);
            }
            else if (options.BitDepth == BitDepth.Grayscale)
            {
                ChooseStringOption("--mode", x => x == "Gray");
                ChooseNumericOption("--depth", 8);
            }
            else if (options.BitDepth == BitDepth.BlackAndWhite)
            {
                if (!ChooseStringOption("--mode", x => x == "Lineart"))
                {
                    ChooseStringOption("--mode", x => x == "Halftone");
                }
                ChooseNumericOption("--depth", 1);
                ChooseNumericOption("--threshold", (-options.Brightness + 1000) / 20m);
            }

            var width = options.PageSize.WidthInMm;
            var height = options.PageSize.HeightInMm;
            ChooseNumericOption("-x", width);
            ChooseNumericOption("-y", height);
            var maxWidth = availableOptions.Get("-l")?.Range?.Max;
            var maxHeight = availableOptions.Get("-t")?.Range?.Max;
            if (maxWidth != null)
            {
                if (options.PageAlign == HorizontalAlign.Center)
                {
                    ChooseNumericOption("-l", (maxWidth.Value - width) / 2);
                }
                else if (options.PageAlign == HorizontalAlign.Right)
                {
                    ChooseNumericOption("-l", maxWidth.Value - width);
                }
                else
                {
                    ChooseNumericOption("-l", 0);
                }
            }
            if (maxHeight != null)
            {
                ChooseNumericOption("-t", 0);
            }

            if (!ChooseNumericOption("--resolution", options.Dpi))
            {
                ChooseNumericOption("--x-resolution", options.Dpi);
                ChooseNumericOption("--y-resolution", options.Dpi);
            }

            return keyValueOptions;
        }

        private bool Transfer(Lazy<KeyValueScanOptions> keyValueOptions, ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
        {
            Stream stream = ScanOne(options.Device.ID, keyValueOptions.Value, cancelToken, scanEvents);
            if (stream == null)
            {
                return false;
            }
            using (stream)
            using (var image = _imageContext.ImageFactory.Decode(stream, ".bmp"))
            {
                callback(image);
                return true;
            }
        }
    }
}