using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Xunit;

namespace NAPS2.App.Tests;

public static class AppTestHelper
{
    public static Process StartGuiProcess(string exeName, string appData, string args = null)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(dir, exeName),
            Arguments = args ?? "",
            UseShellExecute = false
        };
        startInfo.EnvironmentVariables["APPDATA"] = appData;
        var process = Process.Start(startInfo);
        return process;
    }

    public static Process StartProcess(string exeName, string appData, string args = null)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(dir, exeName),
            Arguments = args ?? "",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.EnvironmentVariables["APPDATA"] = appData;
        var process = Process.Start(startInfo);
        return process;
    }

    public static void Cleanup(Process process)
    {
        try
        {
            process.Kill();
        }
        catch (Exception)
        {
            // Kill if possible
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    public static void WaitForVisibleWindow(Process process)
    {
        var startTime = DateTime.Now;
        while ((process.MainWindowHandle == IntPtr.Zero || !IsWindowVisible(process.MainWindowHandle)) &&
               DateTime.Now - startTime < TimeSpan.FromSeconds(5))
        {
            Thread.Sleep(100);
            process.WaitForInputIdle();
        }
        Assert.NotEqual(IntPtr.Zero, process.MainWindowHandle);
        Assert.True(IsWindowVisible(process.MainWindowHandle));
    }
}