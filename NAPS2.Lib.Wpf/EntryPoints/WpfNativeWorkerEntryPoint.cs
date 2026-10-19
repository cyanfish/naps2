using NAPS2.Modules;
using NAPS2.Platform.Windows;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe when running in worker mode.
/// </summary>
public static class WpfNativeWorkerEntryPoint
{
    public static int Run(string[] args)
    {
        // TODO: We don't currently do TWAIN scanning in the native worker, so maybe this can be cleaned up
        var messagePump = Win32MessagePump.Create();
        Invoker.Current = messagePump;
        TwainHandleManager.Factory = () => new Win32TwainHandleManager(messagePump);

        return WorkerEntryPoint.Run(args, new WpfImagesModule(), messagePump.RunMessageLoop, messagePump.Dispose);
    }
}
