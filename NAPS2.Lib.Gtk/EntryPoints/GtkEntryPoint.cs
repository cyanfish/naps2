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

        var subArgs = args.Skip(1).ToArray();
        return args switch
        {
            ["cli" or "console", ..] => ConsoleEntryPoint.Run(subArgs, new GtkImagesModule()),
            ["worker", ..] => WorkerEntryPoint.Run(subArgs, new GtkImagesModule()),
            ["server", ..] => ServerEntryPoint.Run(subArgs, new GtkImagesModule()),
            _ => GuiEntryPoint.Run(args, new GtkImagesModule(), new GtkModule())
        };
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