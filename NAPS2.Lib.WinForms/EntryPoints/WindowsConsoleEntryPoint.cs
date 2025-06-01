using NAPS2.EtoForms;
using NAPS2.EtoForms.WinForms;
using NAPS2.Modules;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Console.exe, the NAPS2 CLI.
/// </summary>
public static class WindowsConsoleEntryPoint
{
    public static int Run(string[] args)
    {
        EtoPlatform.Current = new WinFormsEtoPlatform();
        return ConsoleEntryPoint.Run(args, new GdiModule(), new WinFormsModule());
    }
}