using System.Runtime.InteropServices;
using System.Threading;
using NAPS2.App.Tests.Targets;
using Xunit;

namespace NAPS2.App.Tests;

public static class AppTestHelper
{
    public static Process StartGuiProcess(AppTestExe exe, string appData, string args = null)
    {
        var startInfo = GetProcessStartInfo(exe, appData, args);
        return Process.Start(startInfo);
    }

    public static Process StartProcess(AppTestExe exe, string appData, string args = null)
    {
        var startInfo = GetProcessStartInfo(exe, appData, args);
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        return Process.Start(startInfo);
    }

    private static ProcessStartInfo GetProcessStartInfo(AppTestExe exe, string appData, string args) =>
        new()
        {
            FileName = GetExePath(exe),
            Arguments = exe.ArgPrefix != null && args != null
                ? $"{exe.ArgPrefix} {args}"
                : exe.ArgPrefix ?? args ?? "",
            UseShellExecute = false,
            EnvironmentVariables =
            {
                ["NAPS2_TEST_DATA"] = appData
            }
        };

    public static string GetBaseDirectory(AppTestExe exe)
    {
        var envDirectory = Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT");
        var testDirectory = exe.DefaultRootPath;
        return string.IsNullOrEmpty(envDirectory) ? testDirectory : envDirectory;
    }

    public static string GetExePath(AppTestExe exe)
    {
        var dir = GetBaseDirectory(exe);
        if (dir != exe.DefaultRootPath && exe.TestRootSubPath != null)
        {
            dir = Path.Combine(dir, exe.TestRootSubPath);
        }
        var file = Path.Combine(dir, exe.ExeSubPath);
        if (!File.Exists(file))
        {
            throw new Exception($"Could not find {exe.ExeSubPath} in {dir}");
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
        var path = Path.Combine(appData, "errorlog.txt");
        if (File.Exists(path))
        {
            Assert.False(File.Exists(path), File.ReadAllText(path));
        }
    }

    public static void AssertErrorLog(string appData)
    {
        var path = Path.Combine(appData, "errorlog.txt");
        Assert.True(File.Exists(path), path);
    }

    public static string SolutionRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}