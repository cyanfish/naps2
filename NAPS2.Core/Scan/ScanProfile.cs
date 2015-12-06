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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan
{
    public class ScanProfile
    {
        public const int CURRENT_VERSION = 2;

        public ScanProfile()
        {
            // Set defaults
            BitDepth = ScanBitDepth.C24Bit;
            PageAlign = ScanHorizontalAlign.Left;
            PageSize = ScanPageSize.Letter;
            Resolution = ScanDpi.Dpi200;
            PaperSource = ScanSource.Glass;
        }

        public ScanDevice Device { get; set; }

        public string DriverName { get; set; }

        public string DisplayName { get; set; }

        public int IconID { get; set; }

        public bool MaxQuality { get; set; }

        public bool IsDefault { get; set; }

        public int Version { get; set; }

        public bool UseNativeUI { get; set; }

        public ScanScale AfterScanScale { get; set; }

        public int Brightness { get; set; }

        public int Contrast { get; set; }

        public ScanBitDepth BitDepth { get; set; }

        public ScanHorizontalAlign PageAlign { get; set; }

        public ScanPageSize PageSize { get; set; }

        public PageDimensions CustomPageSize { get; set; }

        public ScanDpi Resolution { get; set; }

        public ScanSource PaperSource { get; set; }
    }

    public enum ScanSource
    {
        [LocalizedDescription(typeof(SettingsResources), "Source_Glass")]
        Glass,
        [LocalizedDescription(typeof(SettingsResources), "Source_Feeder")]
        Feeder,
        [LocalizedDescription(typeof(SettingsResources), "Source_Duplex")]
        Duplex
    }

    public enum ScanBitDepth
    {
        [LocalizedDescription(typeof(SettingsResources), "BitDepth_24Color")]
        C24Bit,
        [LocalizedDescription(typeof(SettingsResources), "BitDepth_8Grayscale")]
        Grayscale,
        [LocalizedDescription(typeof(SettingsResources), "BitDepth_1BlackAndWhite")]
        BlackWhite
    }

    public enum ScanDpi
    {
        [LocalizedDescription(typeof(SettingsResources), "Dpi_100")]
        Dpi100,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_150")]
        Dpi150,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_200")]
        Dpi200,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_300")]
        Dpi300,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_400")]
        Dpi400,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_600")]
        Dpi600,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_800")]
        Dpi800,
        [LocalizedDescription(typeof(SettingsResources), "Dpi_1200")]
        Dpi1200
    }

    public enum ScanHorizontalAlign
    {
        [LocalizedDescription(typeof(SettingsResources), "HorizontalAlign_Left")]
        Left,
        [LocalizedDescription(typeof(SettingsResources), "HorizontalAlign_Center")]
        Center,
        [LocalizedDescription(typeof(SettingsResources), "HorizontalAlign_Right")]
        Right
    }

    public enum ScanScale
    {
        [LocalizedDescription(typeof(SettingsResources), "Scale_1_1")]
        OneToOne,
        [LocalizedDescription(typeof(SettingsResources), "Scale_1_2")]
        OneToTwo,
        [LocalizedDescription(typeof(SettingsResources), "Scale_1_4")]
        OneToFour,
        [LocalizedDescription(typeof(SettingsResources), "Scale_1_8")]
        OneToEight
    }

    public enum ScanPageSize
    {
        [LocalizedDescription(typeof(SettingsResources), "PageSize_Letter")]
        [PageDimensions("8.5", "11", PageSizeUnit.Inch)]
        Letter,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_Legal")]
        [PageDimensions("8.5", "14", PageSizeUnit.Inch)]
        Legal,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_A5")]
        [PageDimensions("148", "210", PageSizeUnit.Millimetre)]
        A5,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_A4")]
        [PageDimensions("210", "297", PageSizeUnit.Millimetre)]
        A4,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_A3")]
        [PageDimensions("297", "420", PageSizeUnit.Millimetre)]
        A3,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_B5")]
        [PageDimensions("176", "250", PageSizeUnit.Millimetre)]
        B5,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_B4")]
        [PageDimensions("250", "353", PageSizeUnit.Millimetre)]
        B4,
        [LocalizedDescription(typeof(SettingsResources), "PageSize_Custom")]
        Custom
    }

    public class PageDimensions
    {
        public decimal Width { get; set; }

        public decimal Height { get; set; }

        public PageSizeUnit Unit { get; set; }
    }

    public class PageDimensionsAttribute : Attribute
    {
        public PageDimensionsAttribute(string width, string height, PageSizeUnit unit)
        {
            this.PageDimensions = new PageDimensions
            {
                Width = decimal.Parse(width, CultureInfo.InvariantCulture),
                Height = decimal.Parse(height, CultureInfo.InvariantCulture),
                Unit = unit
            };
        }

        public PageDimensions PageDimensions { get; private set; }
    }

    public enum PageSizeUnit
    {
        [LocalizedDescription(typeof(SettingsResources), "PageSizeUnit_Inch")]
        Inch,
        [LocalizedDescription(typeof(SettingsResources), "PageSizeUnit_Centimetre")]
        Centimetre,
        [LocalizedDescription(typeof(SettingsResources), "PageSizeUnit_Millimetre")]
        Millimetre
    }

    public static class ScanEnumExtensions
    {
        public static int WidthInThousandthsOfAnInch(this PageDimensions pageDimensions)
        {
            switch (pageDimensions.Unit)
            {
                case PageSizeUnit.Inch:
                    return (int)(pageDimensions.Width * 1000);
                case PageSizeUnit.Centimetre:
                    return (int)(pageDimensions.Width * 0.393701m * 1000);
                case PageSizeUnit.Millimetre:
                    return (int)(pageDimensions.Width * 0.0393701m * 1000);
                default:
                    throw new ArgumentException();
            }
        }

        public static int HeightInThousandthsOfAnInch(this PageDimensions pageDimensions)
        {
            switch (pageDimensions.Unit)
            {
                case PageSizeUnit.Inch:
                    return (int)(pageDimensions.Height * 1000);
                case PageSizeUnit.Centimetre:
                    return (int)(pageDimensions.Height * 0.393701m * 1000);
                case PageSizeUnit.Millimetre:
                    return (int)(pageDimensions.Height * 0.0393701m * 1000);
                default:
                    throw new ArgumentException();
            }
        }

        public static PageDimensions PageDimensions(this Enum enumValue)
        {
            object[] attrs = enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(PageDimensionsAttribute), false);
            return attrs.Cast<PageDimensionsAttribute>().Select(x => x.PageDimensions).SingleOrDefault();
        }

        public static int ToIntDpi(this ScanDpi enumValue)
        {
            switch (enumValue)
            {
                case ScanDpi.Dpi100:
                    return 100;
                case ScanDpi.Dpi150:
                    return 150;
                case ScanDpi.Dpi200:
                    return 200;
                case ScanDpi.Dpi300:
                    return 300;
                case ScanDpi.Dpi400:
                    return 400;
                case ScanDpi.Dpi600:
                    return 600;
                case ScanDpi.Dpi800:
                    return 800;
                case ScanDpi.Dpi1200:
                    return 1200;
                default:
                    throw new ArgumentException();
            }
        }

        public static int ToIntScaleFactor(this ScanScale enumValue)
        {
            switch (enumValue)
            {
                case ScanScale.OneToOne:
                    return 1;
                case ScanScale.OneToTwo:
                    return 2;
                case ScanScale.OneToFour:
                    return 4;
                case ScanScale.OneToEight:
                    return 8;
                default:
                    throw new ArgumentException();
            }
        }

        public static string Description(this Enum enumValue)
        {
            object[] attrs = enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attrs.Cast<DescriptionAttribute>().Select(x => x.Description).SingleOrDefault();
        }
    }
}
