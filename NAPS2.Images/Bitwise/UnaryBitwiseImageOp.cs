namespace NAPS2.Images.Bitwise;

public abstract class UnaryBitwiseImageOp : BitwiseImageOp
{
    public void Perform(IMemoryImage image)
    {
        using var srcLock = image.Lock(LockMode, out var pix);
        Validate(pix);
        PerformCore(pix);
    }

    public void Perform(PixelInfo pix)
    {
        Validate(pix);
        PerformCore(pix);
    }

    private void Validate(PixelInfo pix)
    {
        ValidateConsistency(pix);
        ValidateCore(pix);
    }

    protected virtual LockMode LockMode => LockMode.ReadWrite;

    protected abstract void PerformCore(PixelInfo pix);

    protected virtual void ValidateCore(PixelInfo pix)
    {
        if (pix.bytesPerPixel == 0)
        {
            throw new InvalidOperationException(
                "Can't perform this op on a black & white image; copy it to RGB(A) first.");
        }
    }
}