/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Scan.Images;

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

        public string DriverName { get; private set; }
    }
}
