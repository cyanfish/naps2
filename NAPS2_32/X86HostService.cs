using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.Host;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Twain;

namespace NAPS2_32
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class X86HostService : IX86HostService
    {
        private readonly TwainWrapper twainWrapper;

        public X86HostService(TwainWrapper twainWrapper)
        {
            this.twainWrapper = twainWrapper;
        }

        public void SetRecoveryFolder(string path)
        {
            RecoveryImage.RecoveryFolder = new DirectoryInfo(path);
        }

        public List<ScanDevice> TwainGetDeviceList()
        {
            var parentForm = new BackgroundForm();
            parentForm.Show();
            try
            {
                return twainWrapper.GetDeviceList();
            }
            finally
            {
                parentForm.Close();
            }
        }

        public List<RecoveryIndexImage> TwainScan(int recoveryFileNumber, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams)
        {
            RecoveryImage.RecoveryFileNumber = recoveryFileNumber;
            var parentForm = new BackgroundForm();
            parentForm.Show();
            try
            {
                return twainWrapper.Scan(parentForm, false, scanDevice, scanProfile, scanParams).Select(x => x.RecoveryIndexImage).ToList();
            }
            finally
            {
                parentForm.Close();
            }
        }
    }
}
