using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

class TwainEvents : ITwainEvents
{
    private readonly Action<TwainPageStart> _pageStartCallback;
    private readonly Action<TwainNativeImage> _nativeImageCallback;
    private readonly Action<TwainMemoryBuffer> _memoryBufferCallback;

    public TwainEvents(Action<TwainPageStart> pageStartCallback, Action<TwainNativeImage> nativeImageCallback,
        Action<TwainMemoryBuffer> memoryBufferCallback)
    {
        _pageStartCallback = pageStartCallback;
        _nativeImageCallback = nativeImageCallback;
        _memoryBufferCallback = memoryBufferCallback;
    }

    public void PageStart(TwainPageStart pageStart)
    {
        _pageStartCallback(pageStart);
    }

    public void NativeImageTransferred(TwainNativeImage nativeImage)
    {
        _nativeImageCallback(nativeImage);
    }

    public void MemoryBufferTransferred(TwainMemoryBuffer memoryBuffer)
    {
        _memoryBufferCallback(memoryBuffer);
    }
}