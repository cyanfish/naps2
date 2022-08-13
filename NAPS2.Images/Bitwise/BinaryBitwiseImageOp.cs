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
        using var srcLock = src.Lock(SrcLockMode, out var srcData);
        fixed (byte* dstPtr = dst)
        {
            var dstData = new BitwiseImageData(dstPtr, dstPixelInfo);
            ValidateAndPerform(srcData, dstData);
        }
    }

    public unsafe void Perform(byte[] src, PixelInfo srcPixelInfo, IMemoryImage dst)
    {
        using var dstLock = dst.Lock(DstLockMode, out var dstData);
        fixed (byte* srcPtr = src)
        {
            var srcData = new BitwiseImageData(srcPtr, srcPixelInfo);
            ValidateAndPerform(srcData, dstData);
        }
    }
    
    public unsafe void Perform(IntPtr src, PixelInfo srcPixelInfo, IMemoryImage dst)
    {
        using var dstLock = dst.Lock(DstLockMode, out var dstData);
        var srcData = new BitwiseImageData((byte*) src, srcPixelInfo);
        ValidateAndPerform(srcData, dstData);
    }
    
    public unsafe void Perform(IMemoryImage src, IntPtr dst, PixelInfo dstPixelInfo)
    {
        using var srcLock = src.Lock(SrcLockMode, out var srcData);
        var dstData = new BitwiseImageData((byte*) dst, dstPixelInfo);
        ValidateAndPerform(srcData, dstData);
    }

    private void ValidateAndPerform(BitwiseImageData src, BitwiseImageData dst)
    {
        ValidateConsistency(src);
        ValidateConsistency(dst);
        ValidateCore(src, dst);
        PerformCore(src, dst);
    }

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
    }

    protected abstract LockMode SrcLockMode { get; }

    protected abstract LockMode DstLockMode { get; }

    protected abstract void PerformCore(BitwiseImageData src, BitwiseImageData dst);
}