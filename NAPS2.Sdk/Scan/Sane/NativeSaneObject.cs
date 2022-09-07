using System.Threading;
using NAPS2.Scan.Exceptions;
using static NAPS2.Scan.Sane.SaneNativeLibrary;

namespace NAPS2.Scan.Sane;

public abstract class NativeSaneObject : IDisposable
{
    private bool _disposed;
    private IntPtr _handle;

    protected NativeSaneObject(IntPtr handle)
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

    protected void HandleStatus(SANE_Status status)
    {
        switch (status)
        {
            case SANE_Status.Good:
                return;
            case SANE_Status.Cancelled:
                throw new OperationCanceledException();
            case SANE_Status.NoDocs:
                throw new NoPagesException();
            case SANE_Status.DeviceBusy:
                throw new DeviceException(SdkResources.DeviceBusy);
            case SANE_Status.Invalid:
                // TODO: Maybe not always correct? e.g. when setting options
                throw new DeviceException(SdkResources.DeviceOffline);
            case SANE_Status.Jammed:
                throw new DeviceException(SdkResources.DevicePaperJam);
            case SANE_Status.CoverOpen:
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

    ~NativeSaneObject()
    {
        // TODO: This isn't necessarily going to work as we don't have a lock, not sure the best way to handle it
        // Though this does provide a way to give some kind of error when running tests
        Dispose(false);
    }
}