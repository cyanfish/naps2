using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Scan
{
    /// <summary>
    /// A high-level interface used for scanning.
    /// This abstracts away the logic of obtaining and using an instance of IScanDriver.
    /// </summary>
    public class ScanPerformer : IScanPerformer
    {
        private readonly IScanDriverFactory driverFactory;
        private readonly IErrorOutput errorOutput;
        private readonly IAutoSave autoSave;

        public ScanPerformer(IScanDriverFactory driverFactory, IErrorOutput errorOutput, IAutoSave autoSave)
        {
            this.driverFactory = driverFactory;
            this.errorOutput = errorOutput;
            this.autoSave = autoSave;
        }
        
        // TODO: Move additional logic (auto save, event logging, device prompting) to the driver base class
        // TODO: Not sure what to do with the error handling still. Maybe make that a separate overload/option in ScanPerformer.
        // TODO: Make ScanPerformer return a ScannedImageSource (adding more impls if necessary), and have consumers do "await source.ForEach(callback)"
        // TODO: Bascially the only thing ScanPerformer should do is delegate to the correct driver.
        // TODO: Probably ISaveNotify should follow the static default/injected pattern so it doesn't have to be a parameter.
        
        public ScannedImageSource PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default)
        {
            var driver = driverFactory.Create(scanProfile.DriverName);
            try
            {
                return driver.Scan(scanProfile, scanParams, dialogParent, cancelToken);
            }
            catch (ScanDriverException e)
            {
                if (e is ScanDriverUnknownException)
                {
                    Log.ErrorException(e.Message, e.InnerException);
                    errorOutput.DisplayError(e.Message, e);
                }
                else
                {
                    errorOutput.DisplayError(e.Message);
                }
                // TODO: How does this error handling work with returned ScannedImageSource?
                throw new NotImplementedException();
            }
        }
    }
}
