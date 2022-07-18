using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Pdf.Pdfium;

public abstract class NativePdfiumObject : IDisposable
{
    private bool _disposed;
    private IntPtr _handle;

    protected NativePdfiumObject(IntPtr handle)
    {
        Handle = handle;
    }

    protected static PdfiumNativeLibrary Native => PdfiumNativeLibrary.LazyInstance.Value;

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