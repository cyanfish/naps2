namespace NAPS2.Images.Bitwise;

public abstract class BinaryBitwiseImageOp : BitwiseImageOp
{
    public void Perform(IMemoryImage src, IMemoryImage dst)
    {
        using var srcLock = src.Lock(SrcLockMode, out var srcPix);
        using var dstLock = dst.Lock(DstLockMode, out var dstPix);
        Validate(srcPix, dstPix);
        PerformCore(srcPix, dstPix);
    }
    
    public void Perform(IMemoryImage src, PixelInfo dst)
    {
        using var srcLock = src.Lock(SrcLockMode, out var srcPix);
        Validate(srcPix, dst);
        PerformCore(srcPix, dst);
    }
    
    public void Perform(PixelInfo src, IMemoryImage dst)
    {
        using var dstLock = dst.Lock(DstLockMode, out var dstPix);
        Validate(src, dstPix);
        PerformCore(src, dstPix);
    }
    
    public void Perform(PixelInfo src, PixelInfo dst)
    {
        Validate(src, dst);
        PerformCore(src, dst);
    }

    private void Validate(PixelInfo src, PixelInfo dst)
    {
        ValidateConsistency(src);
        ValidateConsistency(dst);
        ValidateCore(src, dst);
    }

    protected virtual void ValidateCore(PixelInfo src, PixelInfo dst)
    {
        if (src.h != dst.h || src.w != dst.w)
        {
            throw new ArgumentException("Source and destination dimensions must match");
        }
    }

    protected abstract LockMode SrcLockMode { get; }

    protected abstract LockMode DstLockMode { get; }

    protected abstract void PerformCore(PixelInfo src, PixelInfo dst);
}