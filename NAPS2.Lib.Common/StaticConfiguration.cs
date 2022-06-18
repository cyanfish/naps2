using NAPS2.Ocr;
using Ninject;
using NLog;

namespace NAPS2;

public static class StaticConfiguration
{
    public static void Initialize(IKernel kernel)
    {
        var config = kernel.Get<ScopedConfig>();

        Log.Logger = new NLogLogger();
        if (PlatformCompat.System.CanUseWin32)
        {
            Log.EventLogger = new WindowsEventLogger(config);
        }
#if DEBUG
        Debug.Listeners.Add(new NLogTraceListener());
#endif
    }
}