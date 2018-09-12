using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;

namespace NAPS2.Scan
{
    /// <summary>
    /// A base class for IScanDriver implementing common error handling.
    /// </summary>
    public abstract class ScanDriverBase : IScanDriver
    {
        public abstract string DriverName { get; }

        public abstract bool IsSupported { get; }

        public ScanProfile ScanProfile { get; set; }

        public ScanParams ScanParams { get; set; }

        public ScanDevice ScanDevice { get; set; }

        public IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
            if (!IsSupported)
            {
                throw new DriverNotSupportedException();
            }
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling PromptForDevice().");
            }
            try
            {
                return PromptForDeviceInternal();
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        protected abstract ScanDevice PromptForDeviceInternal();

        public List<ScanDevice> GetDeviceList()
        {
            if (!IsSupported)
            {
                throw new DriverNotSupportedException();
            }
            try
            {
                return GetDeviceListInternal();
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        protected abstract List<ScanDevice> GetDeviceListInternal();

        public ScannedImageSource Scan()
        {
            if (!IsSupported)
            {
                throw new DriverNotSupportedException();
            }
            if (ScanProfile == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanProfile must be specified before calling Scan().");
            }
            if (ScanParams == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanParams must be specified before calling Scan().");
            }
            if (ScanDevice == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanDevice must be specified before calling Scan().");
            }
            if (DialogParent == null && !ScanParams.NoUI)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan() without NoUI.");
            }
            
            var source = new ScannedImageSource.Concrete();
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await ScanInternal(source);
                    source.Done();
                }
                catch (ScanDriverException e)
                {
                    source.Error(e);
                }
                catch (Exception e)
                {
                    source.Error(new ScanDriverUnknownException(e));
                }
            }, TaskCreationOptions.LongRunning);
            return source;
        }

        protected abstract Task ScanInternal(ScannedImageSource.Concrete source);
    }
}
