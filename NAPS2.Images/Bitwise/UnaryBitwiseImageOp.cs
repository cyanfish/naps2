namespace NAPS2.Images.Bitwise;

public abstract class UnaryBitwiseImageOp : BitwiseImageOp
{
    public void Perform(IMemoryImage image)
    {
        using var srcLock = image.Lock(LockMode, out var data);
        ValidateAndPerform(data);
    }

    public unsafe void Perform(byte[] byteArray, PixelInfo pixelInfo)
    {
        fixed (byte* ptr = byteArray)
        {
            var data = new BitwiseImageData(ptr, pixelInfo);
            ValidateAndPerform(data);
        }
    }

    private void ValidateAndPerform(BitwiseImageData data)
    {
        ValidateConsistency(data);
        ValidateCore(data);
        PerformCore(data);
    }

    protected virtual LockMode LockMode => LockMode.ReadWrite;

    protected abstract void PerformCore(BitwiseImageData data);

    protected virtual void ValidateCore(BitwiseImageData data)
    {
        if (data.bytesPerPixel == 0)
        {
            throw new InvalidOperationException(
                "Can't perform this op on a black & white image; copy it to RGB(A) first.");
        }
    }
}