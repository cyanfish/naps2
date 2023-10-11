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

        if (args.Length > 0 && args[0] == "worker")
        {
            return WindowsWorkerEntryPoint.Run(args.Skip(1).ToArray());
        }

        return GuiEntryPoint.Run(args, new GdiModule(), new WinFormsModule());
    }
}