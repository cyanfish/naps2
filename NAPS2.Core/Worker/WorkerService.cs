using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Twain;

namespace NAPS2.Worker
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class WorkerService : IWorkerService
    {
        private readonly TwainWrapper twainWrapper;

        public Form ParentForm { get; set; }

        public WorkerService(TwainWrapper twainWrapper)
        {
            this.twainWrapper = twainWrapper;
        }

        public void SetRecoveryFolder(string path)
        {
            RecoveryImage.RecoveryFolder = new DirectoryInfo(path);
        }

        public List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl)
        {
            return twainWrapper.GetDeviceList(twainImpl);
        }

        public List<RecoveryIndexImage> TwainScan(int recoveryFileNumber, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams)
        {
            RecoveryImage.RecoveryFileNumber = recoveryFileNumber;
            return twainWrapper.Scan(ParentForm, true, scanDevice, scanProfile, scanParams).Select(x => x.RecoveryIndexImage).ToList();
        }
    }
}
