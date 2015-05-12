/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using NAPS2.Lang.Resources;

namespace NAPS2.Scan
{
    public class ExtendedScanSettings : ScanSettings
    {
        public const int CURRENT_VERSION = 1;

        public ExtendedScanSettings()
        {
            // Set defaults
            BitDepth = ScanBitDepth.C24Bit;
            PageAlign = ScanHorizontalAlign.Left;
            PageSize = ScanPageSize.Letter;
            Resolution = ScanDpi.Dpi200;
            PaperSource = ScanSource.Glass;
        }

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
        [LocalizedDescription(typeof(SettingsResources), "Dpi_600")]
        Dpi600,
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
                Width = decimal.Parse(width),
                Height = decimal.Parse(height),
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

        public static string Description(this Enum enumValue)
        {
            object[] attrs = enumValue.GetType().GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attrs.Cast<DescriptionAttribute>().Select(x => x.Description).SingleOrDefault();
        }
    }
}
