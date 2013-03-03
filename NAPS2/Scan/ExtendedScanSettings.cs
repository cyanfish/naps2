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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan
{
    public class ExtendedScanSettings : ScanSettings
    {
        public ScanScale AfterScanScale { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public ScanHorizontalAlign PageAlign { get; set; }

        public ScanPageSize PageSize { get; set; }

        public ScanDPI Resolution { get; set; }

        public ScanSource PaperSource { get; set; }
    }

    public enum ScanSource
    {
        [Description("Glass")]
        GLASS,
        [Description("Feeder")]
        FEEDER,
        [Description("Duplex")]
        DUPLEX
    }

    public enum ScanBitDepth
    {
        [Description("24-bit Color")]
        C24BIT,
        [Description("Grayscale")]
        GRAYSCALE,
        [Description("Black & White")]
        BLACKWHITE
    }

    public enum ScanDPI
    {
        [Description("100 dpi")]
        DPI100,
        [Description("200 dpi")]
        DPI200,
        [Description("300 dpi")]
        DPI300,
        [Description("600 dpi")]
        DPI600,
        [Description("1200 dpi")]
        DPI1200
    }

    public enum ScanHorizontalAlign
    {
        [Description("Left")]
        LEFT,
        [Description("Center")]
        CENTER,
        [Description("Right")]
        RIGHT
    }

    public enum ScanScale
    {
        [Description("1:1")]
        ONETOONE,
        [Description("1:2")]
        ONETOTWO,
        [Description("1:4")]
        ONETOFOUR,
        [Description("1:8")]
        ONETOEIGHT
    }

    public enum ScanPageSize
    {
        [Description("A5 (148x210 mm)")]
        A5,
        [Description("A4 (210x297 mm)")]
        A4,
        [Description("A3 (297x420 mm)")]
        A3,
        [Description("US Legal (8.5x14 in)")]
        LEGAL,
        [Description("US Letter (8.5x11 in)")]
        LETTER
    }

    public static class ScanEnumExtensions
    {
        public static Size ToSize(this ScanPageSize pageSize)
        {
            switch (pageSize)
            {
                case ScanPageSize.A5:
                    return new Size(5826, 8267);
                case ScanPageSize.A4:
                    return new Size(8267, 11692);
                case ScanPageSize.A3:
                    return new Size(11692, 16535);
                case ScanPageSize.LEGAL:
                    return new Size(8500, 14000);
                case ScanPageSize.LETTER:
                    return new Size(8500, 11000);
                default:
                    throw new ArgumentException();
            }
        }

        public static string Description(this Enum enumValue)
        {
            object[] attrs = enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length == 0)
            {
                return null;
            }
            return attrs.OfType<DescriptionAttribute>().Single().Description;
        }
    }
}
