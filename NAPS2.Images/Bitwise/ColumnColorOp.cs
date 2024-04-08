namespace NAPS2.Images.Bitwise;

/// <summary>
/// Performs a per-color-channel per-column correction based on the assumption that each column should have at least a
/// few white segments, and if the R/G/B values in each column have a max less than 255, we should scale all values up
/// accordingly.
///
/// This is per-column as in feeder (and other) scanners, there can be a separate sensor for each column that needs to
/// be calibrated independently. Of course that means this correction must happen before deskew or anything else that
/// can combine values across columns.
/// </summary>
internal class ColumnColorOp : UnaryBitwiseImageOp
{
    /// <summary>
    /// Performs this operation including pre-processing steps.
    /// </summary>
    /// <param name="image"></param>
    public static void PerformFullOp(IMemoryImage image)
    {
        var columnColorPreOp = new ColumnColorPreOp();
        columnColorPreOp.Perform(image);
        new ColumnColorOp(columnColorPreOp).Perform(image);
    }

    private readonly ColumnColorPreOp _preOp;

    public ColumnColorOp(ColumnColorPreOp preOp)
    {
        _preOp = preOp;
    }

    protected override unsafe void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        bool rgb = data.bytesPerPixel is 3 or 4;
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                if (rgb)
                {
                    int r = *(pixel + data.rOff);
                    int g = *(pixel + data.gOff);
                    int b = *(pixel + data.bOff);
                    // TODO: Do we want to do a gamma correction here too?
                    r = r * 255 / _preOp.ColHighR[j];
                    g = g * 255 / _preOp.ColHighG[j];
                    b = b * 255 / _preOp.ColHighB[j];
                    if (r > 255) r = 255;
                    if (g > 255) g = 255;
                    if (b > 255) b = 255;
                    *(pixel + data.rOff) = (byte) r;
                    *(pixel + data.gOff) = (byte) g;
                    *(pixel + data.bOff) = (byte) b;
                }
                else
                {
                    int v = *pixel;
                    v = v * 255 / _preOp.ColHighR[j];
                    if (v > 255) v = 255;
                    *pixel = (byte) v;
                }
            }
        }
    }
}