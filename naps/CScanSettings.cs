using System;
using System.Collections.Generic;
using System.Text;

namespace NAPS
{
    public class CScanSettings
    {
        public enum ScanSource
        {
            GLASS,
            FEEDER,
            DUPLEX
        }

        public enum BitDepth
        {
            C24BIT,
            GRAYSCALE,
            BLACKWHITE
        }

        public enum DPI
        {
            DPI100,
            DPI200,
            DPI300,
            DPI600,
            DPI1200
        }

        public enum HorizontalAlign
        {
            LEFT,
            CENTER,
            RIGHT
        }

        public enum Scale
        {
            ONETOONE,
            ONETOTWO,
            ONETOFOUR,
            ONETOEIGHT
        }

        public enum Driver
        {
            WIA,
            TWAIN
        }

        public HorizontalAlign PageAlign { get; set; }

        public Scale AfterScanScale { get; set; }

        public CPageSizes.PageSize PageSize { get; set; }

        public bool ShowScanUI { get; set; }

        public string DisplayName { get; set; }

        public int IconID { get; set; }

        public BitDepth Depth { get; set; }

        public string DeviceID { get; set; }

        public Driver DeviceDriver { get; set; }

        public ScanSource Source { get; set; }

        public DPI Resolution { get; set; }

        public int Contrast { get; set; }

        public int Brightness { get; set; }

        public bool HighQuality { get; set; }

        public CScanSettings()
        {
            ShowScanUI = false;
            DisplayName = "";
            IconID = 0;
            Depth = BitDepth.C24BIT;
            DeviceID = "";
            Source = ScanSource.GLASS;
            Resolution = DPI.DPI200;
            Contrast = 0;
            Brightness = 0;
            PageSize = CPageSizes.PageSize.A4;
            PageAlign = HorizontalAlign.CENTER;
            AfterScanScale = Scale.ONETOONE;
            DeviceDriver = Driver.WIA;
            HighQuality = false;
        }
    }
}
