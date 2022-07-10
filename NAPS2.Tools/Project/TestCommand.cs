namespace NAPS2.Tools.Project;

public static class TestCommand
{
    public static int Run(TestOptions opts)
    {
        // TODO: Do we want to test on .net core too?
        Cli.Run("dotnet", "test -f net462", opts.Verbose, new()
        {
            {"NAPS2_TEST_ROOT", Path.Combine(Paths.SolutionRoot, "NAPS2.App.Tests", "bin", "Debug", "net462")}
        });
        return 0;
    }
}