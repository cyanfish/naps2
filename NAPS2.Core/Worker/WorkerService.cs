using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.ImportExport.Pdf;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Scan.Twain;

namespace NAPS2.Worker
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkerService : IWorkerService
    {
        private readonly TwainWrapper twainWrapper;
        private readonly IPdfExporter pdfExporter;

        public Form ParentForm { get; set; }

        public WorkerService(TwainWrapper twainWrapper, IPdfExporter pdfExporter)
        {
            this.twainWrapper = twainWrapper;
            this.pdfExporter = pdfExporter;
        }

        public void Init()
        {
            OperationContext.Current.Channel.Closed += (sender, args) => Application.Exit();
            Callback = OperationContext.Current.GetCallbackChannel<IWorkerCallback>();
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

        public void Dispose()
        {
        }

        public void ExportPdf(string subFileName, List<ScannedImage.SnapshotExport> snapshots, PdfSettings pdfSettings, string ocrLanguageCode)
        {
            // TODO: Make a type for a serializable snapshot
            // TODO: Other operations. Import. Recovery. Save images. Password and ghostscript callbacks.
            // Figure out ghostscript operation in general.
            // Also - consider off-process thumbnail rendering. That's probably IO bound though, right?
            // So parellization doesn't help. The only benefit would be memory. Which is not a bad benefit.
            WrapOperation(() => pdfExporter.Export(subFileName, snapshots.Import(), pdfSettings, ocrLanguageCode, Callback.Progress));
        }

        private void WrapOperation(Func<bool> op)
        {
            bool success = false;
            try
            {
                success = op();
            }
            catch (Exception e)
            {
                Callback.Error(e);
            }
            finally
            {
                Callback.Finish(success);
            }
        }

        public IWorkerCallback Callback { get; set; }
    }
}
