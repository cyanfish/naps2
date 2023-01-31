using System.Threading;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Sane.Native;

public abstract class SaneNativeObject : IDisposable
{
    private bool _disposed;
    private IntPtr _handle;

    protected SaneNativeObject(SaneNativeLibrary native, IntPtr handle)
    {
        Native = native;
        Handle = handle;
    }

    protected SaneNativeLibrary Native { get; }

    protected internal IntPtr Handle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
        set => _handle = value;
    }

    protected void HandleStatus(SaneStatus status)
    {
        if (status != SaneStatus.Good)
        {
            throw new SaneException(status);
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Dispose(true);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~SaneNativeObject()
    {
        Dispose(false);
    }
}