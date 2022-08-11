namespace NAPS2.Images.Bitwise;

public class BitwiseImageOp
{
    protected unsafe void Validate(PixelInfo pix)
    {
        if (pix.data == (byte*)IntPtr.Zero)
        {
            throw new ArgumentException("Null data");
        }
        if (pix.bytesPerPixel > 0 && pix.bitsPerPixel != pix.bytesPerPixel * 8)
        {
            throw new ArgumentException("Invalid bits per pixel");
        }
        if (!(pix.bytesPerPixel is 1 or 3 or 4 || pix.bytesPerPixel == 0 && pix.bitsPerPixel == 1))
        {
            throw new ArgumentException("Invalid bytes per pixel");
        }
        if (pix.bytesPerPixel > 0 && pix.stride < pix.w * pix.bytesPerPixel)
        {
            throw new ArgumentException("Invalid stride");
        }
        if (pix.bytesPerPixel == 4)
        {
            var offsets = new[] { pix.rOff, pix.gOff, pix.bOff, pix.aOff };
            if (offsets.Distinct().Count() < 4 || offsets.Any(x => x is < 0 or > 3))
            {
                throw new ArgumentException("Invalid offsets");
            }
        }
        if (pix.bytesPerPixel == 3)
        {
            var offsets = new[] { pix.rOff, pix.gOff, pix.bOff };
            if (offsets.Distinct().Count() < 3 || offsets.Any(x => x is < 0 or > 2))
            {
                throw new ArgumentException("Invalid offsets");
            }
        }
    }
}