/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Text;

namespace NAPS2
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

        public ScanBitDepth Depth { get; set; }

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
            Depth = ScanBitDepth.C24BIT;
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
