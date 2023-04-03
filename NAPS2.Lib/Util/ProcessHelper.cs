namespace NAPS2.Util;

public static class ProcessHelper
{
    public static void OpenUrl(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    public static void OpenFile(string file)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = file,
            Verb = "open"
        });
    }

    public static void OpenFolder(string folder) => OpenFile(folder);

    public static bool IsSuccessful(string command, string args, int timeoutMs)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo(command, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (process != null)
            {
                process.WaitForExit(timeoutMs);
                bool result = process.HasExited && process.ExitCode == 0;
                if (!process.HasExited)
                {
                    process.Kill();
                }
                return result;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}