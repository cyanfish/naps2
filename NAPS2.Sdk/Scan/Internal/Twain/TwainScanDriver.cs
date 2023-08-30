#if !MAC
using System.Threading;
using NAPS2.Platform.Windows;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Implementation of IScanDriver for Twain. Delegates to RemoteTwainSessionController in most cases which runs Twain
/// in a 32-bit worker process as Twain drivers are generally 32-bit only. 
/// </summary>
internal class TwainScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public TwainScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        CheckArch(options);
        return Task.Run(async () =>
        {
            var controller = GetSessionController(options);
            foreach (var device in await controller.GetDeviceList(options))
            {
                callback(device);
            }
        });
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        CheckArch(options);
        return Task.Run(async () =>
        {
            var controller = GetSessionController(options);
            using var state = new TwainImageProcessor(_scanningContext, options, scanEvents, callback);
            try
            {
                await controller.StartScan(options, state, cancelToken);
            }
            finally
            {
                EnableWindow(options);
            }
            state.Flush();
        });
    }

    private void EnableWindow(ScanOptions options)
    {
        if (options.DialogParent != IntPtr.Zero && options.UseNativeUI)
        {
            // At the Windows API level, a modal window is implemented by doing two things:
            // 1. Setting the parent on the child window
            // 2. Disabling the parent window
            // The worker is supposed to re-enable the window before returning, but in case the process dies or
            // some other problem occurs, here we make sure that happens.
            Win32.EnableWindow(options.DialogParent, true);
            // We also want to make sure the main NAPS2 window is in the foreground
            Win32.SetForegroundWindow(options.DialogParent);
        }
    }

    private void CheckArch(ScanOptions options)
    {
        if (_scanningContext.WorkerFactory != null)
        {
            // Arch doesn't matter if we can run in a worker process of the correct arch.
            return;
        }
        var dsm = options.TwainOptions.Dsm;
        if (dsm is TwainDsm.New or TwainDsm.Old && Environment.Is64BitProcess)
        {
            throw new InvalidOperationException(
                "Tried to run TWAIN from a 64-bit process. " +
                "If this is intentional, set ScanOptions.TwainOptions.Dsm to TwainDsm.NewX64. " +
                "Otherwise you can set up a worker process with ScanningContext.SetUpWin32Worker().");
        }
        if (dsm == TwainDsm.NewX64 && !Environment.Is64BitProcess)
        {
            throw new InvalidOperationException("Tried to run 64-bit TWAIN from a 32-bit process.");
        }
    }

    private ITwainSessionController GetSessionController(ScanOptions options)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            // If we don't have a worker, we assume the configuration has already been validated by CheckArch and will
            // run in the current process.
            // In general we always prefer to run TWAIN in a worker though.
            return new LocalTwainSessionController(_scanningContext);
        }
        return new RemoteTwainSessionController(_scanningContext);
    }
}
#endif