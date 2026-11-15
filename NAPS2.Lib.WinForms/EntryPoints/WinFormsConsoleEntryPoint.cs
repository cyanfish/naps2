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
        AttachParentConsole();
        EtoPlatform.Current = new WinFormsEtoPlatform();
        return ConsoleEntryPoint.Run(args, new GdiModule(), new WinFormsModule());
    }

    private static void AttachParentConsole()
    {
        // NAPS2.exe has all the actual NAPS2 code, but since Windows differentiates between GUI and CLI executables,
        // users are expected to use the console through NAPS2.Console.exe, which acts as a lightweight intermediary.
        // For that to work properly, we need to call AttachConsole and set up the stdin/out pipes.
        int[] stdHandleIds = [Win32.STD_INPUT_HANDLE, Win32.STD_OUTPUT_HANDLE, Win32.STD_ERROR_HANDLE];
        var stdHandles = stdHandleIds.Select(Win32.GetStdHandle).ToArray();
        if (!Win32.AttachConsole(Win32.ATTACH_PARENT_PROCESS))
        {
            return;
        }
        for (int i = 0; i < stdHandleIds.Length; i++)
        {
            if (stdHandles[i] != IntPtr.Zero && stdHandles[i] != new IntPtr(-1))
            {
                Win32.SetStdHandle(stdHandleIds[i], stdHandles[i]);
            }
        }
    }
}
