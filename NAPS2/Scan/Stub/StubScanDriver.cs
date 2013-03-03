/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

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
                new ScannedImage(bitmap, ScanBitDepth.C24BIT, ScanSettings.MaxQuality ? ImageFormat.Png : ImageFormat.Jpeg)
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
