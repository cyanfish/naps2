namespace NAPS2.Images.Bitwise;

public struct BitwiseImageData
{
    public unsafe BitwiseImageData(IntPtr ptr, PixelInfo pix)
        : this((byte*) ptr, pix)
    {
    }

    public unsafe BitwiseImageData(byte* ptr, PixelInfo pix)
    {
        this.ptr = ptr;
        safePtr = (IntPtr) ptr;
        stride = pix.Stride;
        w = pix.Width;
        h = pix.Height;
        var sub = pix.SubPixelType;
        bitsPerPixel = sub.BitsPerPixel;
        bytesPerPixel = sub.BytesPerPixel;
        rOff = sub.RedOffset;
        gOff = sub.GreenOffset;
        bOff = sub.BlueOffset;
        aOff = sub.AlphaOffset;
        invertY = pix.InvertY;
        invertColorSpace = sub.InvertColorSpace;
    }

    public unsafe byte* ptr;
    public IntPtr safePtr;
    public int stride;
    public int w;
    public int h;
    public int bitsPerPixel;
    public int bytesPerPixel;
    public int rOff;
    public int gOff;
    public int bOff;
    public int aOff;
    public bool invertY;
    public bool invertColorSpace;

    public (int, int, int, int, int) BitLayout => (bitsPerPixel, rOff, gOff, bOff, aOff);
}