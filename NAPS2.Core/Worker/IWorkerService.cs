using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Wia;

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

        [FaultContract(typeof(ScanDriverExceptionDetail))]
        [OperationContract]
        WiaConfiguration Wia10NativeUI(string scanDevice, IntPtr hwnd);

        [OperationContract]
        List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl);

        [FaultContract(typeof(ScanDriverExceptionDetail))]
        [OperationContract]
        Task TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd);

        [OperationContract]
        void CancelTwainScan();

        [OperationContract]
        MapiSendMailReturnCode SendMapiEmail(string clientName, EmailMessage message);

        [OperationContract]
        byte[] RenderThumbnail(ScannedImage.Snapshot snapshot, int size);
    }
}
