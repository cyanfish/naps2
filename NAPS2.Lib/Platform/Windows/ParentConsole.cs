namespace NAPS2.Platform.Windows;

internal static class ParentConsole
{
    public static void Attach()
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
