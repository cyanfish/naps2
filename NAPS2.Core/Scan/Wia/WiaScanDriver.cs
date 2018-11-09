using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";
        
        private readonly IOperationFactory operationFactory;
        private readonly IOperationProgress operationProgress;

        public WiaScanDriver(IFormFactory formFactory, IOperationFactory operationFactory, IOperationProgress operationProgress)
            : base(formFactory)
        {
            this.operationFactory = operationFactory;
            this.operationProgress = operationProgress;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsWiaDriverSupported;

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            using (var deviceManager = new WiaDeviceManager(ScanProfile.WiaVersion))
            {
                return deviceManager.GetDeviceInfos().Select(x =>
                {
                    using (x)
                    {
                        return new ScanDevice(x.Id(), x.Name());
                    }
                }).ToList();
            }
        }

        protected override async Task ScanInternal(ScannedImageSource.Concrete source)
        {
            var op = operationFactory.Create<WiaScanOperation>();
            using (CancelToken.Register(op.Cancel))
            {
                op.Start(ScanProfile, ScanParams, source);
                Invoker.Current.SafeInvoke(() => operationProgress.ShowProgress(op));
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
