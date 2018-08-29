using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Util;

namespace NAPS2.Worker
{
    /// <summary>
    /// The WCF service implementation for NAPS2.Worker.exe.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkerService : IWorkerService
    {
        private readonly TwainWrapper twainWrapper;

        public Form ParentForm { get; set; }

        public WorkerService(TwainWrapper twainWrapper)
        {
            this.twainWrapper = twainWrapper;
        }

        public void Init()
        {
            OperationContext.Current.Channel.Closed += (sender, args) => Application.Exit();
        }

        public void SetRecoveryFolder(string path)
        {
            RecoveryImage.RecoveryFolder = new DirectoryInfo(path);
        }

        public List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl)
        {
            return twainWrapper.GetDeviceList(twainImpl);
        }

        public List<RecoveryIndexImage> TwainScan(int recoveryFileNumber, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd)
        {
            RecoveryImage.RecoveryFileNumber = recoveryFileNumber;
            return twainWrapper.Scan(new Win32Window(hwnd), true, scanDevice, scanProfile, scanParams).Select(x => x.RecoveryIndexImage).ToList();
        }

        public void Dispose()
        {
        }
    }
}
