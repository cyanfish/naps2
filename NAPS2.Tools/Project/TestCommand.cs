namespace NAPS2.Tools.Project;

public static class TestCommand
{
    public static int Run(TestOptions opts)
    {
        // TODO: Framework options (e.g. "-f net462")
        Console.WriteLine("Running tests");
        Cli.Run("dotnet", "test", opts.Verbose, new()
        {
            {"NAPS2_TEST_ROOT", Path.Combine(Paths.SolutionRoot, "NAPS2.App.Tests", "bin", "Debug", "net462")}
        });
        Console.WriteLine("Tests passed.");
        return 0;
    }
}