namespace NAPS2.Tools.Project;

public static class CleanCommand
{
    public static int Run()
    {
        foreach (var projectDir in new DirectoryInfo(Paths.Root).EnumerateDirectories("NAPS2.*")
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
            Console.WriteLine($"Cleaned {projectDir.Name}");
        }
        return 0;
    }
}