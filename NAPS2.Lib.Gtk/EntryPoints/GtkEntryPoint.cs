using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for the Gtk NAPS2 executable.
/// </summary>
public static class GtkEntryPoint
{
    public static int Run(string[] args)
    {
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
}