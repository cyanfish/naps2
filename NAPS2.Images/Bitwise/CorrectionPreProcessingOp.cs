namespace NAPS2.Images.Bitwise;

// TODO: experimental
public class CorrectionPreProcessingOp : UnaryBitwiseImageOp
{
    private const int R_MULT = 299;
    private const int G_MULT = 587;
    private const int B_MULT = 114;

    public CorrectionPreProcessingOp(CorrectionMode mode)
    {
        Mode = mode;
    }

    public CorrectionMode Mode { get; }
    public long TotalCount { get; private set; }
    public int[] Counts { get; } = new int[256];

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
            for (int i = 0; i < Counts.Length; i++)
            {
                Counts[i] += partitionCounts[i];
                TotalCount += partitionCounts[i];
            }
        }
    }
}