using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using CommandLine;

namespace NAPS2
{
    public class AutomatedScanningOptions
    {
        [Option('o', "output", Required = true, HelpText = "The path to the file to save.")]
        public string OutputPath { get; set; }

        [Option('p', "profile", HelpText = "The name of the profile to use for scanning. If not specified, the most-recently-used profile is selected.")]
        public string ProfileName { get; set; }

        [Option('v', "verbose", HelpText = "Display progress information. If not specified, no output is displayed if the scan is successful.")]
        public bool Verbose { get; set; }

        [Option('n', "number", DefaultValue = 1, HelpText = "The number of pages to scan.")]
        public int Number { get; set; }

        [Option('d', "delay", DefaultValue = 0, HelpText = "The delay (in milliseconds) before each page is scanned.")]
        public int Delay { get; set; }

        // TODO: PDF/Image/ImageFormat
    }
}
