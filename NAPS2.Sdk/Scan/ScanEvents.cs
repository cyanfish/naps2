using System;
using NAPS2.Scan.Internal;

namespace NAPS2.Scan
{
    internal class ScanEvents : IScanEvents
    {
        private readonly Action pageStartCallback;
        private readonly Action<double> pageProgressCallback;

        public ScanEvents(Action pageStartCallback, Action<double> pageProgressCallback)
        {
            this.pageStartCallback = pageStartCallback;
            this.pageProgressCallback = pageProgressCallback;
        }

        public void PageStart()
        {
            pageStartCallback();
        }

        public void PageProgress(double progress)
        {
            pageProgressCallback(progress);
        }
    }
}