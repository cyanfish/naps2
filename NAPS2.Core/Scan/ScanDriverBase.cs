using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;

namespace NAPS2.Scan
{
    public abstract class ScanDriverBase : IScanDriver
    {
        public abstract string DriverName { get; }

        public ScanProfile ScanProfile { get; set; }

        public ScanParams ScanParams { get; set; }

        public ScanDevice ScanDevice { get; set; }

        public IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
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

        public IEnumerable<ScannedImage> Scan()
        {
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
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan().");
            }
            try
            {
                return ScanInternal();
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

        protected abstract IEnumerable<ScannedImage> ScanInternal();
    }
}
