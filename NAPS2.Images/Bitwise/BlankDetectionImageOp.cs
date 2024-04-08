namespace NAPS2.Images.Bitwise;

internal class BlankDetectionImageOp : UnaryBitwiseImageOp
{
    // If the pixel value (0-255) >= white_threshold, then it counts as a white pixel.
    private const int WHITE_THRESHOLD_MIN = 1;
    private const int WHITE_THRESHOLD_MAX = 255;
    // If the fraction of non-white pixels > coverage_threshold, then it counts as a non-blank page.
    private const double COVERAGE_THRESHOLD_MIN = 0.00;
    private const double COVERAGE_THRESHOLD_MAX = 0.01;
    private const double IGNORE_EDGE_FRACTION = 0.01;

    private readonly int _whiteThresholdAdjusted;
    private readonly double _coverageThresholdAdjusted;

    private int _startX;
    private int _startY;
    private int _endX;
    private int _endY;
    private long _totalMatch;
    private long _totalPixels;

    public BlankDetectionImageOp(int whiteThreshold, int coverageThreshold)
    {
        _whiteThresholdAdjusted = (int) Math.Round(WHITE_THRESHOLD_MIN +
                                                   (whiteThreshold / 100.0) *
                                                   (WHITE_THRESHOLD_MAX - WHITE_THRESHOLD_MIN));
        _coverageThresholdAdjusted = COVERAGE_THRESHOLD_MIN +
                                     (coverageThreshold / 100.0) * (COVERAGE_THRESHOLD_MAX - COVERAGE_THRESHOLD_MIN);
    }

    protected override LockMode LockMode => LockMode.ReadOnly;

    public double Coverage { get; private set; }

    public bool IsBlank { get; private set; }

    protected override void ValidateCore(BitwiseImageData data)
    {
    }

    protected override void StartCore(BitwiseImageData data)
    {
        _totalPixels = data.w * data.h;
        _startX = (int) (data.w * IGNORE_EDGE_FRACTION);
        _startY = (int) (data.h * IGNORE_EDGE_FRACTION);
        _endX = (int) (data.w * (1 - IGNORE_EDGE_FRACTION));
        _endY = (int) (data.h * (1 - IGNORE_EDGE_FRACTION));
    }

    protected override void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (data.bytesPerPixel is 3 or 4)
        {
            PerformRgba(data, partStart, partEnd);
        }
        else if (data.bytesPerPixel == 1)
        {
            PerformGray(data, partStart, partEnd);
        }
        else if (data.bitsPerPixel == 1)
        {
            PerformBit(data, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData data, int partStart, int partEnd)
    {
        int match = 0;
        for (int i = partStart; i < partEnd; i++)
        {
            if (i < _startY || i > _endY) continue;
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                if (j < _startX || j > _endX) continue;
                var pixel = row + j * data.bytesPerPixel;
                var r = *(pixel + data.rOff);
                var g = *(pixel + data.gOff);
                var b = *(pixel + data.bOff);

                int luma = r * 299 + g * 587 + b * 114;
                if (luma < _whiteThresholdAdjusted * 1000)
                {
                    match++;
                }
            }
        }
        lock (this)
        {
            _totalMatch += match;
        }
    }

    private unsafe void PerformGray(BitwiseImageData data, int partStart, int partEnd)
    {
        int match = 0;
        for (int i = partStart; i < partEnd; i++)
        {
            if (i < _startY || i > _endY) continue;
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                if (j < _startX || j > _endX) continue;
                var pixel = row + j * data.bytesPerPixel;
                var luma = *pixel;

                if (luma < _whiteThresholdAdjusted)
                {
                    match++;
                }
            }
        }
        lock (this)
        {
            _totalMatch += match;
        }
    }

    private unsafe void PerformBit(BitwiseImageData data, int partStart, int partEnd)
    {
        int match = 0;
        for (int i = partStart; i < partEnd; i++)
        {
            if (i < _startY || i > _endY) continue;
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j += 8)
            {
                if (j < _startX || j > _endX) continue;
                byte fullByte = *(row + j / 8);
                for (int k = 7; k >= 0; k--)
                {
                    var bit = fullByte & 1;
                    fullByte >>= 1;
                    if (j + k < data.w)
                    {
                        if (bit == 0)
                        {
                            match++;
                        }
                    }
                }
            }
        }
        lock (this)
        {
            _totalMatch += match;
        }
    }

    protected override void FinishCore()
    {
        Coverage = _totalMatch / (double) _totalPixels;
        IsBlank = Coverage < _coverageThresholdAdjusted;
    }
}