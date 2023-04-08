namespace NAPS2.Images.Bitwise;

public abstract class BinaryBitwiseImageOp : BitwiseImageOp
{
    public void Perform(IMemoryImage src, IMemoryImage dst)
    {
        using var srcLock = src.Lock(SrcLockMode, out var srcData);
        using var dstLock = dst.Lock(DstLockMode, out var dstData);
        ValidateAndPerform(srcData, dstData);
    }

    public unsafe void Perform(IMemoryImage src, byte[] dst, PixelInfo dstPixelInfo)
    {
        if (dst.Length < dstPixelInfo.Length)
        {
            throw new ArgumentException(
                $"Destination byte array length {dst.Length} is less than expected for height {dstPixelInfo.Height} and stride {dstPixelInfo.Stride}");
        }
        using var srcLock = src.Lock(SrcLockMode, out var srcData);
        fixed (byte* dstPtr = dst)
        {
            var dstData = new BitwiseImageData(dstPtr, dstPixelInfo);
            ValidateAndPerform(srcData, dstData);
        }
    }

    public unsafe void Perform(byte[] src, PixelInfo srcPixelInfo, IMemoryImage dst)
    {
        if (src.Length < srcPixelInfo.Length)
        {
            throw new ArgumentException("Source byte array length is less than expected");
        }
        using var dstLock = dst.Lock(DstLockMode, out var dstData);
        fixed (byte* srcPtr = src)
        {
            var srcData = new BitwiseImageData(srcPtr, srcPixelInfo);
            ValidateAndPerform(srcData, dstData);
        }
    }

    public unsafe void Perform(byte[] src, PixelInfo srcPixelInfo, byte[] dst, PixelInfo dstPixelInfo)
    {
        if (src.Length < srcPixelInfo.Length)
        {
            throw new ArgumentException("Source byte array length is less than expected");
        }
        if (dst.Length < dstPixelInfo.Length)
        {
            throw new ArgumentException(
                $"Destination byte array length {dst.Length} is less than expected for height {dstPixelInfo.Height} and stride {dstPixelInfo.Stride}");
        }
        fixed (byte* srcPtr = src)
        fixed (byte* dstPtr = dst)
        {
            var srcData = new BitwiseImageData(srcPtr, srcPixelInfo);
            var dstData = new BitwiseImageData(dstPtr, dstPixelInfo);
            ValidateAndPerform(srcData, dstData);
        }
    }

    public void Perform(IntPtr src, PixelInfo srcPixelInfo, IMemoryImage dst)
    {
        using var dstLock = dst.Lock(DstLockMode, out var dstData);
        var srcData = new BitwiseImageData(src, srcPixelInfo);
        ValidateAndPerform(srcData, dstData);
    }

    public void Perform(IMemoryImage src, IntPtr dst, PixelInfo dstPixelInfo)
    {
        using var srcLock = src.Lock(SrcLockMode, out var srcData);
        var dstData = new BitwiseImageData(dst, dstPixelInfo);
        ValidateAndPerform(srcData, dstData);
    }

    public void Perform(IntPtr src, PixelInfo srcPixelInfo, IntPtr dst, PixelInfo dstPixelInfo)
    {
        var srcData = new BitwiseImageData(src, srcPixelInfo);
        var dstData = new BitwiseImageData(dst, dstPixelInfo);
        ValidateAndPerform(srcData, dstData);
    }

    private void ValidateAndPerform(BitwiseImageData src, BitwiseImageData dst)
    {
        ValidateConsistency(src);
        ValidateConsistency(dst);
        ValidateCore(src, dst);

        StartCore(src, dst);

        var partitionSize = GetPartitionSize(src, dst);
        var partitionCount = GetPartitionCount(src, dst);
        if (partitionCount == 1)
        {
            PerformCore(src, dst, 0, partitionSize);
        }
        else
        {
            int div = (partitionSize + partitionCount - 1) / partitionCount;
            Parallel.For(0, partitionCount, i =>
            {
                int start = div * i, end = Math.Min(div * (i + 1), partitionSize);
                PerformCore(src, dst, start, end);
            });
        }

        FinishCore();
    }

    protected virtual int GetPartitionSize(BitwiseImageData src, BitwiseImageData dst) => src.h;

    protected virtual int GetPartitionCount(BitwiseImageData src, BitwiseImageData dst) => DefaultPartitionCount;

    protected virtual void ValidateCore(BitwiseImageData src, BitwiseImageData dst)
    {
        if (src.h != dst.h || src.w != dst.w)
        {
            throw new ArgumentException("Source and destination dimensions must match");
        }
        if (src.invertY || dst.invertY)
        {
            throw new ArgumentException("InvertY is not supported");
        }
        if (src.invertColorSpace || dst.invertColorSpace)
        {
            throw new ArgumentException("InvertBit is not supported");
        }
    }

    protected abstract LockMode SrcLockMode { get; }

    protected abstract LockMode DstLockMode { get; }

    protected virtual void StartCore(BitwiseImageData src, BitwiseImageData dst)
    {
    }

    protected abstract void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd);
}