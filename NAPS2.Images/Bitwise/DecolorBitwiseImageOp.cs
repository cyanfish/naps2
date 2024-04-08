namespace NAPS2.Images.Bitwise;

internal class DecolorBitwiseImageOp : UnaryBitwiseImageOp
{
    private readonly bool _blackAndWhite;

    public DecolorBitwiseImageOp(bool blackAndWhite)
    {
        _blackAndWhite = blackAndWhite;
    }

    public float BlackWhiteThreshold { get; init; }

    protected override unsafe void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        bool fromGray = data.bytesPerPixel == 1;
        bool toGray = !_blackAndWhite;
        if (fromGray && toGray) return;
        var thresholdAdjusted = (int) (BlackWhiteThreshold * 1000 * 255);
        for (int i = partStart; i < partEnd; i++)
        {
            byte* row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                byte* pixel = row + data.bytesPerPixel * j;
                byte r, g, b;
                if (fromGray)
                {
                    r = g = b = *pixel;
                }
                else
                {
                    r = *(pixel + data.rOff);
                    g = *(pixel + data.gOff);
                    b = *(pixel + data.bOff);
                }
                var luma = r * R_MULT + g * G_MULT + b * B_MULT;
                if (toGray)
                {
                    r = g = b = (byte) (luma / 1000);
                }
                else
                {
                    r = g = b = luma >= thresholdAdjusted ? (byte) 255 : (byte) 0;
                }
                if (fromGray)
                {
                    *pixel = r;
                }
                else
                {
                    *(pixel + data.rOff) = r;
                    *(pixel + data.gOff) = g;
                    *(pixel + data.bOff) = b;
                }
            }
        }
    }
}