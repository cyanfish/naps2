using System.Threading;
using System.Windows.Forms;
using NAPS2.Modules;
using NAPS2.Platform.Windows;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe when running in worker mode.
/// </summary>
public static class WindowsNativeWorkerEntryPoint
{
    public static int Run(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += UnhandledException;

        // TODO: We don't currently do TWAIN scanning in the native worker, so maybe this can be cleaned up
        var messagePump = Win32MessagePump.Create();
        // TODO: Set a logger on the message pump?
        Invoker.Current = messagePump;
        TwainHandleManager.Factory = () => new Win32TwainHandleManager(messagePump);

        return WorkerEntryPoint.Run(args, new GdiModule(), messagePump.RunMessageLoop, messagePump.Dispose);
    }

    private static void UnhandledException(object? sender, ThreadExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the worker to close.", e.Exception);
    }
}