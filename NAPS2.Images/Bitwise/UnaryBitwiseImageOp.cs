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

        var partitionSize = GetPartitionSize(data);
        var partitionCount = GetPartitionCount(data);
        if (partitionCount == 1)
        {
            PerformCore(data, 0, partitionSize);
        }
        else
        {
            int div = (partitionSize + partitionCount - 1) / partitionCount;
            Parallel.For(0, partitionCount, i =>
            {
                int start = div * i, end = Math.Min(div * (i + 1), partitionSize);
                PerformCore(data, start, end);
            });
        }
    }

    protected virtual int GetPartitionSize(BitwiseImageData data) => data.h;
    
    protected virtual int GetPartitionCount(BitwiseImageData data) => DefaultPartitionCount;

    protected virtual LockMode LockMode => LockMode.ReadWrite;

    protected abstract void PerformCore(BitwiseImageData data, int partStart, int partEnd);

    protected virtual void ValidateCore(BitwiseImageData data)
    {
        if (data.bytesPerPixel == 0)
        {
            throw new InvalidOperationException(
                "Can't perform this op on a black & white image; copy it to RGB(A) first.");
        }
    }
}