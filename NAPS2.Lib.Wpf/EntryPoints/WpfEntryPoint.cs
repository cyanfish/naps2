using NAPS2.EtoForms;
using NAPS2.EtoForms.Wpf;
using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe, the NAPS2 GUI, using WPF.
/// </summary>
public static class WpfEntryPoint
{
    public static int Run(string[] args)
    {
        EtoPlatform.Current = new WpfEtoPlatform();

        var subArgs = args.Skip(1).ToArray();
        return args switch
        {
            ["worker", ..] => WpfNativeWorkerEntryPoint.Run(subArgs),
            ["server", ..] => ServerEntryPoint.Run(subArgs, new WpfImagesModule(), new WpfModule()),
            _ => GuiEntryPoint.Run(args, new WpfImagesModule(), new WpfModule())
        };
    }
}
