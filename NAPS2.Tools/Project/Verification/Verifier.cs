namespace NAPS2.Tools.Project.Verification;

public static class Verifier
{
    public static void RunVerificationTests(string testRoot, bool verbose)
    {
        Console.WriteLine($"Running verification tests in: {testRoot}");
        Cli.Run("dotnet", "test NAPS2.App.Tests -f net462", verbose, new()
        {
            { "NAPS2_TEST_ROOT", testRoot }
        });
        if (verbose)
        {
            Console.WriteLine($"Ran verification tests in: {testRoot}");
        }
    }
}