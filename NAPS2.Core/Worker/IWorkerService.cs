using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.Worker
{
    /// <summary>
    /// The WCF service interface for NAPS2.Worker.exe.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IWorkerCallback))]
    public interface IWorkerService
    {
        [OperationContract]
        void Init(string recoveryFolderPath);

        [OperationContract]
        List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl);

        [OperationContract(IsOneWay = true)]
        void TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd);

        [OperationContract]
        byte[] RenderThumbnail(ScannedImage.Snapshot snapshot, int size);
    }
}
