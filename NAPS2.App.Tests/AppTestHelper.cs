using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Xunit;

namespace NAPS2.App.Tests;

public static class AppTestHelper
{
    public static Process StartGuiProcess(string exeName, string appData, string args = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = GetExePath(exeName),
            Arguments = args ?? "",
            UseShellExecute = false,
            EnvironmentVariables =
            {
                ["APPDATA"] = appData
            }
        };
        var process = Process.Start(startInfo);
        return process;
    }

    public static Process StartProcess(string exeName, string appData, string args = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = GetExePath(exeName),
            Arguments = args ?? "",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            EnvironmentVariables =
            {
                ["APPDATA"] = appData
            }
        };
        var process = Process.Start(startInfo);
        return process;
    }

    public static string GetBaseDirectory()
    {
        var envDirectory = Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT");
        var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return string.IsNullOrEmpty(envDirectory) ? testDirectory : envDirectory;
    }

    private static string GetExePath(string exeName)
    {
        var dir = GetBaseDirectory();
        var file = Path.Combine(dir, exeName);
        if (!File.Exists(file))
        {
            file = Path.Combine(dir, "lib", exeName);
        }
        if (!File.Exists(file))
        {
            throw new Exception($"Could not find {exeName} in {dir}");
        }
        return file;
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

    public static void AssertNoErrorLog(string appData)
    {
        var path = Path.Combine(appData, "NAPS2", "errorlog.txt");
        Assert.False(File.Exists(path), File.ReadAllText(path));
    }

    public static void AssertErrorLog(string appData)
    {
        var path = Path.Combine(appData, "NAPS2", "errorlog.txt");
        Assert.True(File.Exists(path));
    }
}