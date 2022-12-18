namespace NAPS2.App.Tests.Targets;

public class LinuxAppTestTarget : IAppTestTarget
{
    public AppTestExe Console => GetAppTestExe("console");
    public AppTestExe Gui => GetAppTestExe(null);
    public AppTestExe Worker => GetAppTestExe("worker");

    private AppTestExe GetAppTestExe(string argPrefix)
    {
        return new AppTestExe(
            Path.Combine(AppTestHelper.SolutionRoot, "NAPS2.App.Gtk", "bin", "Debug", "net6"),
            "naps2",
            argPrefix);
    }

    public override string ToString() => "Linux";
}