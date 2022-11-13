namespace NAPS2.Tools.Project;

public class TestCommand : ICommand<TestOptions>
{
    public int Run(TestOptions opts)
    {
        // TODO: Framework options (e.g. "-f net462")
        Output.Info("Running tests");
        Cli.Run("dotnet", "test", new()
        {
            {"NAPS2_TEST_ROOT", Path.Combine(Paths.SolutionRoot, "NAPS2.App.Tests", "bin", "Debug", "net462")}
        });
        Output.Info("Tests passed.");
        return 0;
    }
}