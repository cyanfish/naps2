using NAPS2.EtoForms;
using NAPS2.EtoForms.WinForms;
using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe, the NAPS2 GUI.
/// </summary>
public static class WinFormsEntryPoint
{
    public static int Run(string[] args)
    {
        EtoPlatform.Current = new WinFormsEtoPlatform();

        var subArgs = args.Skip(1).ToArray();
        return args switch
        {
            ["worker", ..] => WindowsNativeWorkerEntryPoint.Run(subArgs),
            ["server", ..] => ServerEntryPoint.Run(subArgs, new GdiModule()),
            _ => GuiEntryPoint.Run(args, new GdiModule(), new WinFormsModule())
        };
    }
}