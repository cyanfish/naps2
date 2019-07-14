using System;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;

namespace NAPS2.Scan
{
    /// <summary>
    /// A high-level interface used for scanning.
    /// This abstracts away the logic of obtaining and using an instance of IScanDriver.
    /// </summary>
    public class ScanPerformer : IScanPerformer
    {
        private readonly IScanDriverFactory driverFactory;

        public ScanPerformer(IScanDriverFactory driverFactory)
        {
            this.driverFactory = driverFactory;
        }

        // TODO: Move additional logic (auto save, event logging, device prompting) to the driver base class
        // TODO: Probably ISaveNotify should follow the static default/injected pattern so it doesn't have to be a parameter.

        public Task<ScannedImageSource> PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default)
        {
            return Task.Run(() =>
            {
                var driver = driverFactory.Create(scanProfile.DriverName);
                return driver.Scan(scanProfile, scanParams, dialogParent, cancelToken);
            });
        }
    }
}
