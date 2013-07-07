/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
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
using System.Collections.Generic;
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

        public ScanDpi Resolution { get; set; }

        public ScanSource PaperSource { get; set; }
    }

    public enum ScanSource
    {
        [Description("Glass")]
        Glass,
        [Description("Feeder")]
        Feeder,
        [Description("Duplex")]
        Duplex
    }

    public enum ScanBitDepth
    {
        [Description("24-bit Color")]
        C24Bit,
        [Description("Grayscale")]
        Grayscale,
        [Description("Black & White")]
        BlackWhite
    }

    public enum ScanDpi
    {
        [Description("100 dpi")]
        Dpi100,
        [Description("200 dpi")]
        Dpi200,
        [Description("300 dpi")]
        Dpi300,
        [Description("600 dpi")]
        Dpi600,
        [Description("1200 dpi")]
        Dpi1200
    }

    public enum ScanHorizontalAlign
    {
        [Description("Left")]
        Left,
        [Description("Center")]
        Center,
        [Description("Right")]
        Right
    }

    public enum ScanScale
    {
        [Description("1:1")]
        OneToOne,
        [Description("1:2")]
        OneToTwo,
        [Description("1:4")]
        OneToFour,
        [Description("1:8")]
        OneToEight
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
        Legal,
        [Description("US Letter (8.5x11 in)")]
        Letter
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
                case ScanPageSize.Legal:
                    return new Size(8500, 14000);
                case ScanPageSize.Letter:
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
