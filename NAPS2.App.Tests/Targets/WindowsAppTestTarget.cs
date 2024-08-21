namespace NAPS2.App.Tests.Targets;

public class WindowsAppTestTarget : IAppTestTarget
{
    public AppTestExe Console => GetAppTestExe("NAPS2.App.Console", "NAPS2.Console.exe", null);
    public AppTestExe Gui => GetAppTestExe("NAPS2.App.WinForms", "NAPS2.exe", null);
    public AppTestExe Worker => GetAppTestExe("NAPS2.App.Worker", "NAPS2.Worker.exe", "lib");
    public bool IsWindows => true;

    private AppTestExe GetAppTestExe(string project, string exeName, string testRootSubPath)
    {
        return new AppTestExe(
            Path.Combine(AppTestHelper.SolutionRoot, project, "bin", "Debug", "net9-windows", "win-x64"),
            exeName,
            TestRootSubPath: testRootSubPath);
    }

    public override string ToString() => "Windows";
}