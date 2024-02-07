namespace NAPS2.Images.Bitwise;

public class FillColorImageOp : UnaryBitwiseImageOp
{
    private readonly byte _r, _g, _b, _a;

    public FillColorImageOp(byte r, byte g, byte b, byte a)
    {
        _r = r;
        _g = g;
        _b = b;
        _a = a;
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
        bool gray = data.bytesPerPixel == 1;
        byte luma = (byte) ((_r * R_MULT + _g * G_MULT + _b * B_MULT) / 1000);
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                if (gray)
                {
                    *pixel = luma;
                }
                else
                {
                    *(pixel + data.rOff) = _r;
                    *(pixel + data.gOff) = _g;
                    *(pixel + data.bOff) = _b;
                }
                if (data.hasAlpha)
                {
                    *(pixel + data.aOff) = _a;
                }
            }
        }
    }
}