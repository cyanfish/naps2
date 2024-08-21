namespace NAPS2.App.Tests.Targets;

public class WindowsAppTestTarget : IAppTestTarget
{
    public AppTestExe Console => GetAppTestExe("NAPS2.App.Console", "NAPS2.Console.exe", "win-x64", null);
    public AppTestExe Gui => GetAppTestExe("NAPS2.App.WinForms", "NAPS2.exe", "win-x64", null);
    public AppTestExe Worker => GetAppTestExe("NAPS2.App.Worker", "NAPS2.Worker.exe", "win-x86", "lib");
    public bool IsWindows => true;

    private AppTestExe GetAppTestExe(string project, string exeName, string arch, string testRootSubPath)
    {
        return new AppTestExe(
            Path.Combine(AppTestHelper.SolutionRoot, project, "bin", "Debug", "net9-windows", arch),
            exeName,
            TestRootSubPath: testRootSubPath);
    }

    public override string ToString() => "Windows";
}