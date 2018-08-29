using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<ScannedImage> Scan()
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
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan().");
            }
            // Deconstruct a foreach loop so we can use a try-catch block and work around the limitations of "yield return"
            using (var enumerator = ScanInternal().GetEnumerator())
            {
                while (true)
                {
                    try
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                    }
                    catch (ScanDriverException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new ScanDriverUnknownException(e);
                    }

                    yield return enumerator.Current;
                }
            }
        }

        protected abstract IEnumerable<ScannedImage> ScanInternal();
    }
}
