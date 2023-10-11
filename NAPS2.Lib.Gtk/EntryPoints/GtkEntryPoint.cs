using NAPS2.EtoForms;
using NAPS2.EtoForms.Gtk;
using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for the Gtk NAPS2 executable.
/// </summary>
public static class GtkEntryPoint
{
    public static int Run(string[] args)
    {
        GLib.ExceptionManager.UnhandledException += UnhandledGtkException;
        EtoPlatform.Current = new GtkEtoPlatform();

        if (args.Length > 0 && args[0] is "cli" or "console")
        {
            return ConsoleEntryPoint.Run(args.Skip(1).ToArray(), new GtkImagesModule());
        }
        if (args.Length > 0 && args[0] == "worker")
        {
            return WorkerEntryPoint.Run(args.Skip(1).ToArray(), new GtkImagesModule());
        }

        return GuiEntryPoint.Run(args, new GtkImagesModule(), new GtkModule());
    }

    private static void UnhandledGtkException(GLib.UnhandledExceptionArgs e)
    {
        if (e.IsTerminating)
        {
            Log.FatalException("An error occurred that caused the task to terminate.", e.ExceptionObject as Exception ?? new Exception());
        }
        else
        {
            Log.ErrorException("An unhandled error occurred.", e.ExceptionObject as Exception ?? new Exception());
        }
    }
}