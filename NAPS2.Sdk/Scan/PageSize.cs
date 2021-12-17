using System;
using System.Globalization;

namespace NAPS2.Scan;

public class PageSize
{
    public static PageSize Letter = new PageSize("8.5", "11", PageSizeUnit.Inch);

    public static PageSize Legal = new PageSize("8.5", "14", PageSizeUnit.Inch);

    public static PageSize A5 = new PageSize("148", "210", PageSizeUnit.Millimetre);

    public static PageSize A4 = new PageSize("210", "297", PageSizeUnit.Millimetre);

    public static PageSize A3 = new PageSize("297", "420", PageSizeUnit.Millimetre);

    public static PageSize B5 = new PageSize("176", "250", PageSizeUnit.Millimetre);

    public static PageSize B4 = new PageSize("250", "353", PageSizeUnit.Millimetre);

    protected PageSize()
    {
    }

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

    public decimal Width { get; protected set; }

    public decimal Height { get; protected set; }

    public PageSizeUnit Unit { get; protected set; }

    public decimal WidthInMm
    {
        get
        {
            switch (Unit)
            {
                case PageSizeUnit.Inch:
                    return Width * 25.4m;
                case PageSizeUnit.Centimetre:
                    return Width * 10;
                case PageSizeUnit.Millimetre:
                    return Width;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public decimal WidthInInches
    {
        get
        {
            switch (Unit)
            {
                case PageSizeUnit.Inch:
                    return Width;
                case PageSizeUnit.Centimetre:
                    return Width * 0.393701m;
                case PageSizeUnit.Millimetre:
                    return Width * 0.0393701m;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public int WidthInThousandthsOfAnInch => (int)(WidthInInches * 1000);

    public decimal HeightInMm
    {
        get
        {
            switch (Unit)
            {
                case PageSizeUnit.Inch:
                    return Height * 25.4m;
                case PageSizeUnit.Centimetre:
                    return Height * 10;
                case PageSizeUnit.Millimetre:
                    return Height;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public decimal HeightInInches
    {
        get
        {
            switch (Unit)
            {
                case PageSizeUnit.Inch:
                    return Height;
                case PageSizeUnit.Centimetre:
                    return Height * 0.393701m;
                case PageSizeUnit.Millimetre:
                    return Height * 0.0393701m;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public int HeightInThousandthsOfAnInch => (int)(HeightInInches * 1000);
}