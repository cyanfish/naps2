namespace NAPS2.Images.Bitwise;

public class LogicalPixelFormatOp : UnaryBitwiseImageOp
{
    public ImagePixelFormat LogicalPixelFormat { get; private set; }

    protected override LockMode LockMode => LockMode.ReadOnly;

    protected override void ValidateCore(BitwiseImageData data)
    {
    }

    protected override unsafe void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        lock (this)
        {
            if (data.bitsPerPixel == 1)
            {
                LogicalPixelFormat = ImagePixelFormat.BW1;
                return;
            }
        }

        bool readGray = data.bytesPerPixel == 1;
        bool isBinary = true;
        bool isGray = true;
        bool isOpaque = true;
        bool checkBinary = data.bytesPerPixel >= 1;
        bool checkGray = data.bytesPerPixel >= 3;
        bool checkOpacity = data.bytesPerPixel == 4;
        for (int i = partStart; i < partEnd; i++)
        {
            byte* row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                byte* pixel = row + data.bytesPerPixel * j;
                if (readGray)
                {
                    if (checkBinary)
                    {
                        byte val = *pixel;
                        if (val != 0 && val != 255)
                        {
                            isBinary = false;
                            break;
                        }
                    }
                }
                else
                {

                    var r = *(pixel + data.rOff);
                    var g = *(pixel + data.gOff);
                    var b = *(pixel + data.bOff);
                    // TODO: Uh oh. Big problem in copy, as well as other places: when copying to rgba, the alpha should be set to 255.
                    if (checkOpacity)
                    {
                        var a = *(pixel + data.bOff);
                        if (a != 255)
                        {
                            isOpaque = false;
                            checkOpacity = false;
                            break;
                        }
                    }
                    if (checkGray)
                    {
                        if (r != g || g != b)
                        {
                            isGray = false;
                            isBinary = false;
                            checkGray = false;
                            checkBinary = false;
                            if (!checkOpacity) break;
                        }
                    }
                    if (checkBinary)
                    {
                        if ((r != 0 && r != 255) || (g != 0 && g != 255) || (b != 0 && b != 255))
                        {
                            isBinary = false;
                            checkBinary = false;
                        }
                    }
                }
            }
        }
        var pixelFormat = isBinary && isOpaque ? ImagePixelFormat.BW1 :
            isGray && isOpaque ? ImagePixelFormat.Gray8 :
            isOpaque ? ImagePixelFormat.RGB24 : ImagePixelFormat.ARGB32;
        lock (this)
        {
            if (pixelFormat > LogicalPixelFormat)
            {
                LogicalPixelFormat = pixelFormat;
            }
        }
    }
}