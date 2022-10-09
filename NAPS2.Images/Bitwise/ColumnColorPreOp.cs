namespace NAPS2.Images.Bitwise;

// TODO: experimental
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

    private unsafe void CalculateNoiseOrientation(BitwiseImageData data)
    {
        // TODO: Not sure if we actually need this, but it does seem to pick out much greater horizontal noise (2x)
        // vs control (1.1x) when there are vertical bands.
        if (data.w < 3 || data.h < 3) return;
        const int maxDelta = 10;
        int midX = data.w / 2;
        int midY = data.h / 2;
        long horNoiseCount = 0, horNoiseTotal = 0;
        long verNoiseCount = 0, verNoiseTotal = 0;
        for (int i = 0; i < data.h; i++)
        {
            var midPixel = data.ptr + data.stride * i + midX * data.bytesPerPixel;
            var prePixel = midPixel - data.bytesPerPixel;
            var postPixel = midPixel + data.bytesPerPixel;

            var deltaR = Math.Abs((*(prePixel + data.rOff) + *(postPixel + data.rOff)) / 2 - *(midPixel + data.rOff));
            var deltaG = Math.Abs((*(prePixel + data.gOff) + *(postPixel + data.gOff)) / 2 - *(midPixel + data.gOff));
            var deltaB = Math.Abs((*(prePixel + data.bOff) + *(postPixel + data.bOff)) / 2 - *(midPixel + data.bOff));
            var delta = deltaR + deltaG + deltaB;
            if (delta < maxDelta)
            {
                horNoiseTotal += delta;
                horNoiseCount++;
            }
        }
        for (int i = 0; i < data.w; i++)
        {
            var midPixel = data.ptr + data.stride * midY + i * data.bytesPerPixel;
            var prePixel = midPixel - data.stride;
            var postPixel = midPixel + data.stride;

            var deltaR = Math.Abs((*(prePixel + data.rOff) + *(postPixel + data.rOff)) / 2 - *(midPixel + data.rOff));
            var deltaG = Math.Abs((*(prePixel + data.gOff) + *(postPixel + data.gOff)) / 2 - *(midPixel + data.gOff));
            var deltaB = Math.Abs((*(prePixel + data.bOff) + *(postPixel + data.bOff)) / 2 - *(midPixel + data.bOff));
            var delta = deltaR + deltaG + deltaB;
            if (delta < maxDelta)
            {
                verNoiseTotal += delta;
                verNoiseCount++;
            }
        }
    }
}