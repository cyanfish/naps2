using NAPS2.Scan.Internal.Sane.Native;

namespace NAPS2.Scan.Internal.Sane;

internal class SaneFrameController
{
    private readonly SaneOptionController _optionController;
    private readonly SaneOption? _tlx;
    private readonly SaneOption? _tly;
    private readonly SaneOption? _brx;
    private readonly SaneOption? _bry;
    private readonly double? _xres;
    private readonly double? _yres;

    public SaneFrameController(SaneOptionController optionController)
    {
        _optionController = optionController;

        _tlx = _optionController.GetOption(SaneOptionNames.TOP_LEFT_X);
        _tly = _optionController.GetOption(SaneOptionNames.TOP_LEFT_Y);
        _brx = _optionController.GetOption(SaneOptionNames.BOT_RIGHT_X);
        _bry = _optionController.GetOption(SaneOptionNames.BOT_RIGHT_Y);
        if (_brx?.Unit == SaneUnit.Pixel)
        {
            if (_optionController.TryGet(SaneOptionNames.RESOLUTION, out var res))
            {
                _xres = res;
                _yres = res;
            }
            else if (_optionController.TryGet(SaneOptionNames.X_RESOLUTION, out var xres) &&
                     _optionController.TryGet(SaneOptionNames.Y_RESOLUTION, out var yres))
            {
                _xres = xres;
                _yres = yres;
            }
        }
        CanSetFrame = _tlx != null && _tly != null && _brx != null && _bry != null &&
                      IsNumericRange(_tlx) && IsNumericRange(_tly) && IsNumericRange(_brx) && IsNumericRange(_bry) &&
                      (_brx.Unit == SaneUnit.Mm || _xres != null && _yres != null);
    }

    public bool CanSetFrame { get; }

    private double GetNumericRangeMin(SaneOption opt)
    {
        return opt.ConstraintType == SaneConstraintType.WordList
            ? opt.WordList!.Min()
            : opt.Range!.Min;
    }

    private double GetNumericRangeMax(SaneOption opt)
    {
        return opt.ConstraintType == SaneConstraintType.WordList
            ? opt.WordList!.Max()
            : opt.Range!.Max;
    }

    private bool IsNumericRange(SaneOption opt)
    {
        return opt.Type is SaneValueType.Int or SaneValueType.Fixed &&
               opt.ConstraintType is SaneConstraintType.Range or SaneConstraintType.WordList;
    }

    private double GetFrameMinMm(SaneOption opt, double? dpi) => ToMm(opt, GetNumericRangeMin(opt), dpi);

    private double GetFrameMaxMm(SaneOption opt, double? dpi) => ToMm(opt, GetNumericRangeMax(opt), dpi);

    private double ToMm(SaneOption opt, double value, double? dpi)
    {
        if (opt.Unit == SaneUnit.Pixel)
        {
            value *= 25.4 / dpi!.Value;
        }
        return value;
    }

    public (double minX, double minY, double maxX, double maxY) GetBounds()
    {
        if (!CanSetFrame) throw new InvalidOperationException();
        return (
            GetFrameMinMm(_tlx!, _xres), GetFrameMinMm(_tly!, _yres),
            GetFrameMaxMm(_brx!, _xres), GetFrameMaxMm(_bry!, _yres));
    }

    public void SetFrame(double x1, double y1, double x2, double y2)
    {
        if (!CanSetFrame) throw new InvalidOperationException();
        _optionController.TrySet(SaneOptionNames.TOP_LEFT_X, x1);
        _optionController.TrySet(SaneOptionNames.TOP_LEFT_Y, y1);
        _optionController.TrySet(SaneOptionNames.BOT_RIGHT_X, x2);
        _optionController.TrySet(SaneOptionNames.BOT_RIGHT_Y, y2);
    }
}