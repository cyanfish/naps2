namespace NAPS2.App.Tests.Targets;

public class WindowsAppTestTarget : IAppTestTarget
{
    public AppTestExe Console => GetAppTestExe("NAPS2.App.Console", "NAPS2.Console.exe", "win-x64");
    public AppTestExe Gui => GetAppTestExe("NAPS2.App.WinForms", "NAPS2.exe", "win-x64");
    public AppTestExe Worker => GetAppTestExe("NAPS2.App.Worker", "NAPS2.Worker.exe", "win-x86", null, "lib");
    public AppTestExe Server => GetAppTestExe("NAPS2.App.WinForms", "NAPS2.exe", "win-x64", "server");
    public bool IsWindows => true;

    private AppTestExe GetAppTestExe(string project, string exeName, string arch, string argPrefix = null,
        string testRootSubPath = null)
    {
        return new AppTestExe(
            Path.Combine(AppTestHelper.SolutionRoot, project, "bin", "Debug", "net9-windows", arch),
            exeName,
            argPrefix,
            testRootSubPath);
    }

    public override string ToString() => "Windows";
}