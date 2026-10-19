using NAPS2.EtoForms;
using NAPS2.EtoForms.Wpf;
using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Console.exe, the NAPS2 CLI, using WPF.
/// </summary>
public static class WpfConsoleEntryPoint
{
    public static int Run(string[] args)
    {
        EtoPlatform.Current = new WpfEtoPlatform();
        return ConsoleEntryPoint.Run(args, new WpfImagesModule(), new WpfModule());
    }
}
