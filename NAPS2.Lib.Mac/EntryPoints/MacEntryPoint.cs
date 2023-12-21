using NAPS2.EtoForms;
using NAPS2.EtoForms.Mac;
using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for the Mac NAPS2 executable.
/// </summary>
public static class MacEntryPoint
{
    public static int Run(string[] args)
    {
        Runtime.MarshalManagedException += (_, eventArgs) =>
        {
            Log.ErrorException("Marshalling managed exception", eventArgs.Exception);
            eventArgs.ExceptionMode = MarshalManagedExceptionMode.ThrowObjectiveCException;
        };
        Runtime.MarshalObjectiveCException += (_, eventArgs) =>
        {
            Log.Error($"Marshalling ObjC exception: {eventArgs.Exception.Description}");
        };

        EtoPlatform.Current = new MacEtoPlatform();

        var subArgs = args.Skip(1).ToArray();
        return args switch
        {
            ["cli" or "console", ..] => ConsoleEntryPoint.Run(subArgs, new MacImagesModule()),
            ["worker", ..] => MacWorkerEntryPoint.Run(subArgs),
            ["server", ..] => ServerEntryPoint.Run(subArgs, new MacImagesModule()),
            _ => GuiEntryPoint.Run(args, new MacImagesModule(), new MacModule())
        };
    }
}