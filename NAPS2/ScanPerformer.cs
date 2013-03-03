using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ninject;

namespace NAPS2
{
    public class ScanPerformer : IScanPerformer
    {
        public void PerformScan(ScanSettings scanSettings, IWin32Window dialogParent, IScanReceiver scanReceiver)
        {
            IScanDriver driver = KernelManager.Kernel.Get<IScanDriver>(scanSettings.Device.DriverName);
            driver.DialogParent = dialogParent;
            driver.ScanSettings = scanSettings;

            try
            {
                scanReceiver.ReceiveScan(driver.Scan());
            }
            catch (ScanDriverException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
