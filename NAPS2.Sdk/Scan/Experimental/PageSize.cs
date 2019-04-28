using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public class PageSize
    {
        public static PageSize Letter = new PageSize("8.5", "11", PageSizeUnit.Inch);

        public static PageSize Legal = new PageSize("8.5", "14", PageSizeUnit.Inch);

        public static PageSize A5 = new PageSize("148", "210", PageSizeUnit.Millimetre);

        public static PageSize A4 = new PageSize("210", "297", PageSizeUnit.Millimetre);

        public static PageSize A3 = new PageSize("297", "420", PageSizeUnit.Millimetre);

        public static PageSize B5 = new PageSize("176", "250", PageSizeUnit.Millimetre);

        public static PageSize B4 = new PageSize("250", "353", PageSizeUnit.Millimetre);

        public PageSize(string width, string height, PageSizeUnit unit)
        {
            Width = decimal.Parse(width, CultureInfo.InvariantCulture);
            Height = decimal.Parse(height, CultureInfo.InvariantCulture);
            Unit = unit;
        }

        public PageSize(decimal width, decimal height, PageSizeUnit unit)
        {
            Width = width;
            Height = height;
            Unit = unit;
        }

        public decimal Width { get; }

        public decimal Height { get; }

        public PageSizeUnit Unit { get; }
    }
}
