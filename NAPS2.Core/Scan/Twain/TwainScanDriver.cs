using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;
using NAPS2.Worker;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";
        
        private readonly IWorkerServiceFactory workerServiceFactory;
        private readonly TwainWrapper twainWrapper;
        private readonly ScannedImageHelper scannedImageHelper;

        public TwainScanDriver(IWorkerServiceFactory workerServiceFactory, TwainWrapper twainWrapper, IFormFactory formFactory, ScannedImageHelper scannedImageHelper)
            : base(formFactory)
        {
            this.workerServiceFactory = workerServiceFactory;
            this.twainWrapper = twainWrapper;
            this.scannedImageHelper = scannedImageHelper;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsTwainDriverSupported;
        
        private bool UseWorker => !(ScanProfile.TwainImpl == TwainImpl.X64 && Environment.Is64BitProcess) && PlatformCompat.Runtime.UseWorker;
        
        protected override List<ScanDevice> GetDeviceListInternal()
        {
            // Exclude WIA proxy devices since NAPS2 already supports WIA
            return GetFullDeviceList().Where(x => !x.ID.StartsWith("WIA-", StringComparison.InvariantCulture)).ToList();
        }

        private IEnumerable<ScanDevice> GetFullDeviceList()
        {
            var twainImpl = ScanProfile?.TwainImpl ?? TwainImpl.Default;
            if (UseWorker)
            {
                using(var worker = workerServiceFactory.Create())
                {
                    return worker.Service.TwainGetDeviceList(twainImpl);
                }
            }
            return twainWrapper.GetDeviceList(twainImpl);
        }

        protected override async Task ScanInternal(ScannedImageSource.Concrete source)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (UseWorker)
                {
                    var parentHandle = DialogParent?.SafeHandle() ?? IntPtr.Zero;
                    try
                    {
                        using (var worker = workerServiceFactory.Create())
                        {
                            worker.Callback.ImageCallback += (img, tempPath) =>
                            {
                                if (tempPath != null) scannedImageHelper.RunBackgroundOcr(img, ScanParams, tempPath);
                                source.Put(img);
                            };
                            CancelToken.Register(worker.Service.CancelTwainScan);
                            await worker.Service.TwainScan(ScanDevice, ScanProfile, ScanParams, parentHandle);
                        }
                    }
                    finally
                    {
                        if (parentHandle != IntPtr.Zero)
                        {
                            // If the worker process hard crashes while a modal window is open, it may leave the parent
                            // window in a state where it can't be interacted with. This fixes that interaction.
                            //
                            // At the Windows API level, a modal window is implemented by doing two things:
                            // 1. Setting the parent on the child window
                            // 2. Disabling the parent window
                            // The first is implicitly undone when the worker process dies. The second is undone here.
                            Win32.EnableWindow(parentHandle, true);
                        }
                    }
                }
                else
                {
                    twainWrapper.Scan(DialogParent, ScanDevice, ScanProfile, ScanParams, CancelToken, source, scannedImageHelper.RunBackgroundOcr);
                }
            }, TaskCreationOptions.LongRunning).Unwrap();
        }
    }
}
