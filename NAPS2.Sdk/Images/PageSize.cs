using System.Globalization;

namespace NAPS2.Images;

public record PageSize
{
    public static PageSize Letter = new("8.5", "11", PageSizeUnit.Inch);

    public static PageSize Legal = new("8.5", "14", PageSizeUnit.Inch);

    public static PageSize A5 = new("148", "210", PageSizeUnit.Millimetre);

    public static PageSize A4 = new("210", "297", PageSizeUnit.Millimetre);

    public static PageSize A3 = new("297", "420", PageSizeUnit.Millimetre);

    public static PageSize B5 = new("176", "250", PageSizeUnit.Millimetre);

    public static PageSize B4 = new("250", "353", PageSizeUnit.Millimetre);

    public static PageSize? Parse(string? size)
    {
        if (size == null)
            return null;
        var parts = size.Split(' ');
        if (parts.Length != 2)
            return null;
        var dims = parts[0].Split('x');
        if (dims.Length != 2)
            return null;
        if (!decimal.TryParse(dims[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var width))
            return null;
        if (!decimal.TryParse(dims[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var height))
            return null;
        var unit = parts[1] switch
        {
            "mm" => PageSizeUnit.Millimetre,
            "cm" => PageSizeUnit.Centimetre,
            _ => PageSizeUnit.Inch
        };
        return new PageSize(width, height, unit);
    }

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

    public override string ToString()
    {
        var unit = Unit switch
        {
            PageSizeUnit.Centimetre => "cm",
            PageSizeUnit.Millimetre => "mm",
            _ => "in"
        };
        return $"{Width.ToString(CultureInfo.InvariantCulture)}x{Height.ToString(CultureInfo.InvariantCulture)} {unit}";
    }
}