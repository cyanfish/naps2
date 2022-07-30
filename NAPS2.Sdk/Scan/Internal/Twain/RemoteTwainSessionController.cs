using System.Threading;
using NAPS2.Platform.Windows;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Proxy implementation of ITwainSessionController that interacts with a Twain session in a worker process.
/// </summary>
public class RemoteTwainSessionController : ITwainSessionController
{
    private readonly ScanningContext _scanningContext;

    public RemoteTwainSessionController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException(
                "ScanningContext.WorkerFactory must be set to use TWAIN from a 64-bit process.");
        }
        using var workerContext = _scanningContext.WorkerFactory.Create();
        return await workerContext.Service.TwainGetDeviceList(options);
    }

    public async Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException(
                "ScanningContext.WorkerFactory must be set to use TWAIN from a 64-bit process.");
        }
        using var workerContext = _scanningContext.WorkerFactory.Create();
        try
        {
            await workerContext.Service.TwainScan(options, cancelToken, twainEvents);
        }
        finally
        {
            EnableWindow(options.DialogParent);
        }
    }

    private void EnableWindow(IntPtr dialogParent)
    {
        if (dialogParent != IntPtr.Zero)
        {
            // At the Windows API level, a modal window is implemented by doing two things:
            // 1. Setting the parent on the child window
            // 2. Disabling the parent window
            // The worker is supposed to re-enable the window before returning, but in case the process dies or
            // some other problem occurs, here we make sure that happens.
            Win32.EnableWindow(dialogParent, true);
        }
    }
}