using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.Scan.Sane
{
    public class SaneScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "sane";
        
        public override string DriverName => DRIVER_NAME;

        protected override ScanDevice PromptForDeviceInternal()
        {
            throw new NotImplementedException();
        }

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ScannedImage> ScanInternal()
        {
            throw new NotImplementedException();
        }
    }
}
