using System.Threading;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Sane.Native;

public abstract class SaneNativeObject : IDisposable
{
    private bool _disposed;
    private IntPtr _handle;

    protected SaneNativeObject(IntPtr handle)
    {
        Handle = handle;
    }

    protected static SaneNativeLibrary Native
    {
        get
        {
            var value = SaneNativeLibrary.Instance;
            if (!Monitor.IsEntered(value))
            {
                throw new InvalidOperationException("Sane operations must be locked");
            }
            return value;
        }
    }

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
        // TODO: This isn't necessarily going to work as we don't have a lock, not sure the best way to handle it
        // Though this does provide a way to give some kind of error when running tests
        Dispose(false);
    }
}