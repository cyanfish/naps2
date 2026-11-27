using NAPS2.EtoForms;
using NAPS2.EtoForms.WinForms;
using NAPS2.Modules;
using NAPS2.Platform.Windows;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Console.exe, the NAPS2 CLI.
/// </summary>
public static class WinFormsConsoleEntryPoint
{
    public static int Run(string[] args)
    {
        ParentConsole.Attach();
        EtoPlatform.Current = new WinFormsEtoPlatform();
        return ConsoleEntryPoint.Run(args, new GdiModule(), new WinFormsModule());
    }
}