namespace NAPS2.Util;

public static class ProcessHelper
{
    public static void OpenUrl(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}