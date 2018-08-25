using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Operation;
using NAPS2.Recovery;
using NAPS2.Scan;

namespace NAPS2.Worker
{
    [ServiceContract]
    public interface IWorkerService
    {
        [OperationContract]
        void Init();

        [OperationContract]
        void SetRecoveryFolder(string path);

        [OperationContract]
        List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl);

        [OperationContract]
        List<RecoveryIndexImage> TwainScan(int recoveryFileNumber, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd);
    }
}
