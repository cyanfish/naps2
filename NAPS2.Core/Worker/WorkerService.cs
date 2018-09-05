using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
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

        public void TwainScan(int recoveryFileNumber, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd)
        {
            try
            {
                RecoveryImage.RecoveryFileNumber = recoveryFileNumber;
                twainWrapper.Scan(new Win32Window(hwnd), true, scanDevice, scanProfile, scanParams, new WorkerImageSource(Callback));
            }
            catch (Exception e)
            {
                var stream = new MemoryStream();
                new NetDataContractSerializer().Serialize(stream, e);
                Callback.Error(stream.ToArray());
            }
            finally
            {
                Callback.Finish();
            }
        }

        public void Dispose()
        {
        }

        public IWorkerCallback Callback { get; set; }

        private class WorkerImageSource : ScannedImageSource.Concrete
        {
            private readonly IWorkerCallback callback;

            public WorkerImageSource(IWorkerCallback callback)
            {
                this.callback = callback;
            }

            public override void Put(ScannedImage image)
            {
                callback.TwainImageReceived(image.RecoveryIndexImage);
            }
        }
    }
}
