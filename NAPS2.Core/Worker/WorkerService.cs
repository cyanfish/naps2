using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.ImportExport.Pdf;
using NAPS2.Operation;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Scan.Twain;
using NAPS2.Util;

namespace NAPS2.Worker
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkerService : IWorkerService
    {
        private readonly TwainWrapper twainWrapper;
        private readonly IPdfExporter pdfExporter;
        private readonly IOperationFactory operationFactory;

        public Form ParentForm { get; set; }

        public WorkerService(TwainWrapper twainWrapper, IPdfExporter pdfExporter, IOperationFactory operationFactory)
        {
            this.twainWrapper = twainWrapper;
            this.pdfExporter = pdfExporter;
            this.operationFactory = operationFactory;
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
        
        public void DoOperationWork(string operationTypeName, WorkerOperation.WorkArgs args)
        {
            // TODO: Make a type for a serializable snapshot
            // TODO: Other operations. Import. Recovery. Save images. Password and ghostscript callbacks.
            // Figure out ghostscript operation in general.
            // Also - consider off-process thumbnail rendering. That's probably IO bound though, right?
            // So parellization doesn't help. The only benefit would be memory. Which is not a bad benefit.
            var operationType = Type.GetType(operationTypeName);
            if (operationType == null)
            {
                Log.Error($"Operation type not available: {operationTypeName}");
                return;
            }
            var op = (WorkerOperation)typeof(IOperationFactory).GetMethod("Create")?.MakeGenericMethod(operationType).Invoke(operationFactory, new object[0]);
            if (op == null)
            {
                Log.Error($"Could not create operation: {operationTypeName}");
                return;
            }

            bool success = false;
            try
            {
                op.ProgressProxy = Callback.Progress;
                success = op.DoWorkInternal(args);
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
