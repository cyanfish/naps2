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
        switch (status)
        {
            case SaneStatus.Good:
                return;
            case SaneStatus.Cancelled:
                throw new OperationCanceledException();
            case SaneStatus.NoDocs:
                throw new NoPagesException();
            case SaneStatus.DeviceBusy:
                throw new DeviceException(SdkResources.DeviceBusy);
            case SaneStatus.Invalid:
                // TODO: Maybe not always correct? e.g. when setting options
                throw new DeviceException(SdkResources.DeviceOffline);
            case SaneStatus.Jammed:
                throw new DeviceException(SdkResources.DevicePaperJam);
            case SaneStatus.CoverOpen:
                throw new DeviceException(SdkResources.DeviceCoverOpen);
            default:
                throw new DeviceException($"SANE error: {status}");
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
        if (!_disposed)
        {
            if (Handle != IntPtr.Zero)
            {
                DisposeHandle();
            }
            _disposed = true;
        }
    }

    protected abstract void DisposeHandle();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SaneNativeObject()
    {
        // TODO: This isn't necessarily going to work as we don't have a lock, not sure the best way to handle it
        // Though this does provide a way to give some kind of error when running tests
        Dispose(false);
    }
}