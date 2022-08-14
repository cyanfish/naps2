using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NAPS2.Platform.Windows;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Implementation of IScanDriver for Twain. Delegates to RemoteTwainSessionController in most cases which runs Twain
/// in a 32-bit worker process as Twain drivers are generally 32-bit only. 
/// </summary>
internal class TwainScanDriver : IScanDriver
{
    public static readonly TWIdentity TwainAppId =
        TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetEntryAssembly());

    static TwainScanDriver()
    {
        // Path to the folder containing the 64-bit twaindsm.dll relative to NAPS2.Core.dll
        if (PlatformCompat.System.CanUseWin32)
        {
            string libDir = Environment.Is64BitProcess ? "_win64" : "_win32";
            var location = Assembly.GetExecutingAssembly().Location;
            var coreDllDir = Path.GetDirectoryName(location);
            if (coreDllDir != null)
            {
                Win32.SetDllDirectory(Path.Combine(coreDllDir, libDir));
            }
        }
#if DEBUG
        PlatformInfo.Current.Log.IsDebugEnabled = true;
#endif
    }

    private readonly ScanningContext _scanningContext;

    public TwainScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        var controller = GetSessionController(options);
        return await controller.GetDeviceList(options);
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        var controller = GetSessionController(options);
        using var state = new TwainImageProcessor(_scanningContext, options, scanEvents, callback);
        await controller.StartScan(options, state, cancelToken);
    }

    private ITwainSessionController GetSessionController(ScanOptions options)
    {
        if (options.TwainOptions.Dsm != TwainDsm.NewX64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new RemoteTwainSessionController(_scanningContext);
        }
        return new LocalTwainSessionController();
    }
}