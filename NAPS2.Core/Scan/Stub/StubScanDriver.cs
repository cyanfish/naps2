using NAPS2.Scan.Images;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace NAPS2.Scan.Stub
{
    public class StubScanDriver : IScanDriver
    {
        private static int _number = 1;

        public StubScanDriver(string driverName)
        {
            DriverName = driverName;
        }

        public ScanProfile ScanProfile { get; set; }

        public ScanParams ScanParams { get; set; }

        public ScanDevice ScanDevice { get; set; }

        public IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
            return new ScanDevice("test", "Test Scanner");
        }

        public List<ScanDevice> GetDeviceList()
        {
            return new List<ScanDevice>
            {
                new ScanDevice("test", "Test Scanner")
            };
        }

        public IEnumerable<ScannedImage> Scan()
        {
            for (int i = 0; i < ImageCount; i++)
            {
                Thread.Sleep(500);
                yield return MakeImage();
            }
        }

        private int ImageCount
        {
            get
            {
                switch (ScanProfile.PaperSource)
                {
                    case ScanSource.Glass:
                        return 1;

                    case ScanSource.Feeder:
                        return 3;

                    case ScanSource.Duplex:
                        return 4;
                }
                return 0;
            }
        }

        private ScannedImage MakeImage()
        {
            var bitmap = new Bitmap(600, 800);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.LightGray, 0, 0, bitmap.Width, bitmap.Height);
                g.DrawString((_number++).ToString("G"), new Font("Times New Roman", 80), Brushes.Black, 0, 350);
            }
            var image = new ScannedImage(bitmap, ScanBitDepth.C24Bit, ScanProfile.MaxQuality, ScanProfile.Quality);
            return image;
        }

        public string DriverName { get; }
    }
}