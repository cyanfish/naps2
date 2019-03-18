using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Images;
using NAPS2.Images.Storage;

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

        public CancellationToken CancelToken { get; set; }

        public ScanDevice PromptForDevice(ScanProfile scanProfile, IntPtr dialogParent) => new ScanDevice("test", "Test Scanner");

        public List<ScanDevice> GetDeviceList(ScanProfile scanProfile) => new List<ScanDevice>
        {
            new ScanDevice("test", "Test Scanner")
        };

        public ScannedImageSource Scan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            var sink = new ScannedImageSink();
            Task.Run(() =>
            {
                for (int i = 0; i < ImageCount; i++)
                {
                    Thread.Sleep(500);
                    sink.PutImage(MakeImage());
                }
            });
            return sink.AsSource();
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
            var image = new ScannedImage(new GdiImage(bitmap), ScanBitDepth.C24Bit, ScanProfile.MaxQuality, ScanProfile.Quality);
            return image;
        }

        public string DriverName { get; }

        public bool IsSupported => true;
    }
}
