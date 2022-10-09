namespace NAPS2.Images.Bitwise;

/// <summary>
/// Performs pre-processing for the ColumnColorOp.
/// </summary>
public class ColumnColorPreOp : UnaryBitwiseImageOp
{
    private const double COL_IGNORE_TOP_AND_BOTTOM = 0.02;

    public int[] ColHighR { get; private set; } = null!;
    public int[] ColHighG { get; private set; } = null!;
    public int[] ColHighB { get; private set; } = null!;

    // TODO: Do we need to allow for inverted orientation (i.e. rows)?
    protected override void StartCore(BitwiseImageData data)
    {
        ColHighR = new int[data.w];
        ColHighG = new int[data.w];
        ColHighB = new int[data.w];
    }

    protected override void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (data.bytesPerPixel is 1 or 3 or 4)
        {
            PerformRgba(data, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData data, int partStart, int partEnd)
    {
        var colHighR = new int[data.w];
        var colHighG = new int[data.w];
        var colHighB = new int[data.w];
        var colMinY = (int) (data.h * COL_IGNORE_TOP_AND_BOTTOM);
        var colMaxY = (int) (data.h * (1 - COL_IGNORE_TOP_AND_BOTTOM));
        bool rgb = data.bytesPerPixel is 3 or 4;
        for (int i = partStart; i < partEnd; i++)
        {
            if (i <= colMinY || i >= colMaxY) continue;
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                if (rgb)
                {
                    byte r = *(pixel + data.rOff);
                    byte g = *(pixel + data.gOff);
                    byte b = *(pixel + data.bOff);
                    colHighR[j] = Math.Max(r, colHighR[j]);
                    colHighG[j] = Math.Max(g, colHighG[j]);
                    colHighB[j] = Math.Max(b, colHighB[j]);
                }
                else
                {
                    byte v = *pixel;
                    colHighR[j] = Math.Max(v, colHighR[j]);
                }
            }
        }
        lock (this)
        {
            for (int i = 0; i < data.w; i++)
            {
                ColHighR[i] = Math.Max(ColHighR[i], colHighR[i]);
                ColHighG[i] = Math.Max(ColHighG[i], colHighG[i]);
                ColHighB[i] = Math.Max(ColHighB[i], colHighB[i]);
            }
        }
    }

    protected override void FinishCore()
    {
        // TODO: Outlier detection and removal? (e.g. if there are some black columns at the side of the page)
    }
}