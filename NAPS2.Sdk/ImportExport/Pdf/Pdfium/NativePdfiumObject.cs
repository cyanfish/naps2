using System.Runtime.InteropServices;
using System.Threading;

namespace NAPS2.ImportExport.Pdf.Pdfium;

public abstract class NativePdfiumObject : IDisposable
{
    private bool _disposed;
    private IntPtr _handle;

    protected NativePdfiumObject(IntPtr handle)
    {
        Handle = handle;
    }

    protected static PdfiumNativeLibrary Native
    {
        get
        {
            var value = PdfiumNativeLibrary.LazyInstance.Value;
            if (!Monitor.IsEntered(value))
            {
                throw new InvalidOperationException("Pdfium operations must be locked");
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
            DisposeHandle();
            _disposed = true;
        }
    }

    protected abstract void DisposeHandle();

    internal void SetAlreadyDisposed()
    {
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NativePdfiumObject()
    {
        Dispose(false);
    }
}