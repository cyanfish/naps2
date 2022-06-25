using NAPS2.Scan;
using Ninject;
using Ninject.Modules;
using NLog;

namespace NAPS2.Modules;

public class ContextModule : NinjectModule
{
    public override void Load()
    {
        Kernel.Get<ScanningContext>().TempFolderPath = Paths.Temp;
        Kernel.Get<ScanningContext>().RecoveryPath = Paths.Recovery;

        var config = Kernel.Get<Naps2Config>();

        Log.Logger = new NLogLogger();
        if (PlatformCompat.System.CanUseWin32)
        {
            Log.EventLogger = new WindowsEventLogger(config);
        }
#if DEBUG
        Trace.Listeners.Add(new NLogTraceListener());
#endif
    }
}