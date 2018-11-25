using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.ImportExport;

namespace NAPS2.Scan.Batch
{
    public class BatchSettings
    {
        public BatchSettings()
        {
            SaveSeparator = SaveSeparator.FilePerPage;
        }

        public string ProfileDisplayName { get; set; }

        public BatchScanType ScanType { get; set; }

        public int ScanCount { get; set; }

        public double ScanIntervalSeconds { get; set; }

        public BatchOutputType OutputType { get; set;  }

        public SaveSeparator SaveSeparator { get; set; }

        public string SavePath { get; set; }
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
