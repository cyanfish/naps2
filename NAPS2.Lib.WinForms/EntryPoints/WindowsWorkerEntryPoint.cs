using System.Threading;
using System.Windows.Forms;
using NAPS2.EtoForms.WinForms;
using NAPS2.Modules;
using NAPS2.Scan.Internal.Twain;
using NAPS2.WinForms;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Worker.exe, an off-process worker.
///
/// NAPS2.Worker.exe runs in 32-bit mode for compatibility with 32-bit TWAIN drivers.
/// </summary>
public static class WindowsWorkerEntryPoint
{
    public static int Run(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += UnhandledException;

        // Set up a form for the worker process
        // A parent form is needed for some operations, namely 64-bit TWAIN scanning
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