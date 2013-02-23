using NAPS2.Scan.Wia;
using NAPS2.Scan.Twain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Stub
{
    class StubScanDriver : IScanDriver
    {
        protected StubScanDriver(string driverName)
        {
            this.DriverName = driverName;
        }

        public ScanSettings ScanSettings { get; set; }

        public System.Windows.Forms.IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
            return new ScanDevice("test", "Test Scanner", DriverName);
        }

        public List<IScannedImage> Scan()
        {
            Bitmap bitmap = new Bitmap(600, 800);
            Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(Brushes.LightGray, 0, 0, bitmap.Width, bitmap.Height);
            g.DrawString(new Random().Next().ToString(), new Font("Times New Roman", 80), Brushes.Black, 0, 350);
            return new List<IScannedImage>
            {
                new ScannedImage(bitmap, ScanBitDepth.C24BIT, ImageFormat.Jpeg)
            };
        }

        public string DriverName { get; private set; }
    }

    class StubWiaScanDriver : StubScanDriver
    {
        public StubWiaScanDriver()
            : base(WiaScanDriver.DRIVER_NAME)
        {
        }
    }

    class StubTwainScanDriver : StubScanDriver
    {
        public StubTwainScanDriver()
            : base(TwainScanDriver.DRIVER_NAME)
        {
        }
    }
}
