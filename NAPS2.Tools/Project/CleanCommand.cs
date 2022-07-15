namespace NAPS2.Tools.Project;

public static class CleanCommand
{
    public static int Run(CleanOptions opts)
    {
        Console.WriteLine("Starting clean");
        foreach (var projectDir in new DirectoryInfo(Paths.SolutionRoot).EnumerateDirectories("NAPS2.*")
                     .Where(x => x.Name.ToLower() != "naps2.tools"))
        {
            foreach (var cleanDir in projectDir.EnumerateDirectories()
                         .Where(x => x.Name.ToLower() == "bin" || x.Name.ToLower() == "obj"))
            {
                foreach (var subDir in cleanDir.EnumerateDirectories())
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Could not delete {projectDir.Name}/{cleanDir.Name}/{subDir.Name}: {ex.Message}");
                    }
                }
            }
            if (opts.Verbose)
            {
                Console.WriteLine($"Cleaned {projectDir.Name}");
            }
        }
        Console.WriteLine("Cleaned.");
        return 0;
    }
}