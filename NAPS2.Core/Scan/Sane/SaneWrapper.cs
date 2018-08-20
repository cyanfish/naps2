using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            proc.ErrorDataReceived += (sender, args) =>
            {
				if (args.Data != null)
				{
					var match = ProgressRegex.Match(args.Data);
					if (match.Success)
					{
						progressCallback?.Invoke((int)float.Parse(match.Groups[1].Value) * 10, 1000);
					}
				}
            };
            proc.BeginErrorReadLine();
            var outputStream = new MemoryStream();
            proc.StandardOutput.BaseStream.CopyTo(outputStream);
            return outputStream;
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
