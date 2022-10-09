namespace NAPS2.Images.Bitwise;

// TODO: experimental
public class ColumnColorOp : UnaryBitwiseImageOp
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