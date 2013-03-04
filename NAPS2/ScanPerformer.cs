using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using Ninject;

namespace NAPS2
{
    public class ScanPerformer : IScanPerformer
    {
        public void PerformScan(ScanSettings scanSettings, IWin32Window dialogParent, IScanReceiver scanReceiver)
        {
            var driver = KernelManager.Kernel.Get<IScanDriver>(scanSettings.Device.DriverName);
            driver.DialogParent = dialogParent;
            driver.ScanSettings = scanSettings;

            try
            {
                foreach (IScannedImage scannedImage in driver.Scan())
                {
                    scanReceiver.ReceiveScannedImage(scannedImage);
                    Application.DoEvents();
                }
            }
            catch (ScanDriverException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
