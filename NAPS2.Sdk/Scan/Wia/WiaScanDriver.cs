using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Operation;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";
        
        private readonly OperationProgress operationProgress;
        private readonly ScannedImageHelper scannedImageHelper;

        public WiaScanDriver()
        {
            operationProgress = OperationProgress.Default;
            scannedImageHelper = new ScannedImageHelper();
        }

        public WiaScanDriver(OperationProgress operationProgress, ScannedImageHelper scannedImageHelper)
        {
            this.operationProgress = operationProgress;
            this.scannedImageHelper = scannedImageHelper;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsWiaDriverSupported;

        protected override ScanDevice PromptForDeviceInternal(ScanProfile scanProfile, IntPtr dialogParent)
        {
            try
            {
                using (var deviceManager = new WiaDeviceManager(scanProfile.WiaVersion))
                {
                    using (var device = deviceManager.PromptForDevice(dialogParent))
                    {
                        if (device == null)
                        {
                            return null;
                        }

                        return new ScanDevice(device.Id(), device.Name());
                    }
                }
            }
            catch (WiaException e) when (e.ErrorCode == WiaErrorCodes.NO_DEVICE_AVAILABLE)
            {
                throw new NoDevicesFoundException();
            }
        }

        protected override List<ScanDevice> GetDeviceListInternal(ScanProfile scanProfile)
        {
            using (var deviceManager = new WiaDeviceManager(scanProfile?.WiaVersion ?? WiaVersion.Default))
            {
                return deviceManager.GetDeviceInfos().Select(deviceInfo =>
                {
                    using (deviceInfo)
                    {
                        return new ScanDevice(deviceInfo.Id(), deviceInfo.Name());
                    }
                }).ToList();
            }
        }

        protected override async Task ScanInternal(ScannedImageSink sink, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            var op = new WiaScanOperation(scannedImageHelper);
            using (cancelToken.Register(op.Cancel))
            {
                op.Start(scanProfile, scanDevice, scanParams, dialogParent, sink);
                Invoker.Current.SafeInvoke(() =>
                {
                    if (scanParams.Modal)
                    {
                        operationProgress.ShowModalProgress(op);
                    }
                    else
                    {
                        operationProgress.ShowBackgroundProgress(op);
                    }
                });
                await op.Success;
            }

            if (op.ScanException != null)
            {
                op.ScanException.PreserveStackTrace();
                throw op.ScanException;
            }
        }
    }
}
