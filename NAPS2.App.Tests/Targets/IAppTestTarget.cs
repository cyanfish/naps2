namespace NAPS2.App.Tests.Targets;

public interface IAppTestTarget
{
    AppTestExe Console { get; }
    AppTestExe Gui { get; }
    AppTestExe Worker { get; }
    bool IsWindows { get; }
}