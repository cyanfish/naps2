using System.Runtime.InteropServices;

namespace NAPS2.App.Tests.Targets;

public class LinuxAppTestTarget : IAppTestTarget
{
    public AppTestExe Console => GetAppTestExe("console");
    public AppTestExe Gui => GetAppTestExe(null);
    public AppTestExe Worker => GetAppTestExe("worker");
    public bool IsWindows => false;

    private AppTestExe GetAppTestExe(string argPrefix)
    {
        var runtime = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
        return new AppTestExe(
            Path.Combine(AppTestHelper.SolutionRoot, "NAPS2.App.Gtk", "bin", "Debug", "net8", runtime),
            "naps2",
            argPrefix);
    }

    public override string ToString() => "Linux";
}