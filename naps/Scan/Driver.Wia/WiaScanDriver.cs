using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS
{
    public class WiaScanDriver : IScanDriver
    {
        public const string DRIVER_NAME = "wia";

        public IScanSettings ScanSettings { get; set; }

        public System.Windows.Forms.IWin32Window DialogParent { get; set; }

        public IScanDevice PromptForDevice()
        {
            throw new NotImplementedException();
        }

        public List<IScannedImage> Scan()
        {
            throw new NotImplementedException();
        }
    }
}
