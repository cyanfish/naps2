using System.Threading;
using System.Windows.Forms;
using NAPS2.EtoForms.WinForms;
using NAPS2.Modules;
using NAPS2.Scan.Internal.Twain;
using NAPS2.WinForms;

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

        // Set up a form for the worker process
        // A parent form is needed for some operations, namely 64-bit TWAIN scanning
        // TODO: We don't currently do TWAIN scanning in the native worker, so maybe this can be cleaned up
        var form = new BackgroundForm();
        Invoker.Current = new WinFormsInvoker(() => form);
        TwainHandleManager.Factory = () => new WinFormsTwainHandleManager(form);

        return WorkerEntryPoint.Run(args, new GdiModule(), () => Application.Run(form), () => form.Close());
    }

    private static void UnhandledException(object? sender, ThreadExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the worker to close.", e.Exception);
    }
}