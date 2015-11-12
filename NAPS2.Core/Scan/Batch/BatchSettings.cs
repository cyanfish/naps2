using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Batch
{
    public class BatchSettings
    {
        public string ProfileDisplayName { get; set; }

        public BatchScanType ScanType { get; set; }

        public int ScanCount { get; set; }

        public double ScanIntervalSeconds { get; set; }

        public BatchOutputType OutputType { get; set;  }

        public BatchSaveSeparator SaveSeparator { get; set; }

        public string SavePath { get; set; }
    }

    public enum BatchSaveSeparator
    {
        FilePerPage,
        FilePerScan,
        PatchT
    }

    public enum BatchOutputType
    {
        Load,
        SingleFile,
        MultipleFiles
    }

    public enum BatchScanType
    {
        Single,
        MultipleWithPrompt,
        MultipleWithDelay
    }
}
