using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for the Mac NAPS2 executable.
/// </summary>
public static class MacEntryPoint
{
    public static int Run(string[] args)
    {
        if (args.Length > 0 && args[0] is "cli" or "console")
        {
            return ConsoleEntryPoint.Run(args.Skip(1).ToArray(), new MacImagesModule());
        }
        if (args.Length > 0 && args[0] == "worker")
        {
            return MacWorkerEntryPoint.Run(args.Skip(1).ToArray());
        }

        return GuiEntryPoint.Run(args, new MacImagesModule(), new MacModule());
    }
}