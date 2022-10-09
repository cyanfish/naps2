namespace NAPS2.Images.Bitwise;

/// <summary>
/// Performs pre-processing for the WhiteBlackPointOp.
/// </summary>
public class WhiteBlackPointPreOp : UnaryBitwiseImageOp
{
    public WhiteBlackPointPreOp(CorrectionMode mode)
    {
        Mode = mode;
    }

    public CorrectionMode Mode { get; }
    public long PixelTotalCount { get; private set; }
    public int[] PixelLumCounts { get; } = new int[256];
    public double HorizontalNoise { get; private set; }
    public double VerticalNoise { get; private set; }

    protected override unsafe void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        var partitionCounts = new int[256];
        bool rgb = data.bytesPerPixel is 3 or 4;
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                int value;
                if (rgb)
                {
                    var r = *(pixel + data.rOff);
                    var g = *(pixel + data.gOff);
                    var b = *(pixel + data.bOff);
                    // TODO: Do this?
                    // var max = Math.Max(Math.Max(r, g), b);
                    // var min = Math.Min(Math.Min(r, g), b);
                    // if (max - min > 16)
                    // {
                    //     continue;
                    // }
                    // TODO: Not sure whether it's best to use this or just an average, though it probably doesn't matter
                    // much as the lows/highs should be black/white.
                    value = (r * R_MULT + g * G_MULT + b * B_MULT) / 1000;
                }
                else
                {
                    value = *pixel;
                }
                partitionCounts[value]++;
            }
        }
        lock (this)
        {
            for (int i = 0; i < PixelLumCounts.Length; i++)
            {
                PixelLumCounts[i] += partitionCounts[i];
                PixelTotalCount += partitionCounts[i];
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
        HorizontalNoise = horNoiseTotal / (double) horNoiseCount;
        VerticalNoise = verNoiseTotal / (double) verNoiseCount;
    }
}