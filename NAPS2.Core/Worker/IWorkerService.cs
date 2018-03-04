using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.ImportExport.Pdf;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.Worker
{
    [ServiceContract(CallbackContract = typeof(IWorkerCallback))]
    public interface IWorkerService : IDisposable
    {
        [OperationContract]
        void Init();

        [OperationContract]
        void SetRecoveryFolder(string path);

        [OperationContract]
        List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl);

        [OperationContract]
        List<RecoveryIndexImage> TwainScan(int recoveryFileNumber, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams);

        [OperationContract(IsOneWay = true)]
        void ExportPdf(string subFileName, List<ScannedImage.SnapshotExport> snapshots, PdfSettings pdfSettings, string ocrLanguageCode);
    }
}
