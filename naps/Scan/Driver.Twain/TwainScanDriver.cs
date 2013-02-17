using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS.Scan.Driver.Twain
{
    public class TwainScanDriver : IScanDriver
    {
        public const string DRIVER_NAME = "twain";

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
