namespace NAPS2.Tools.Project;

public class CleanCommand : ICommand<CleanOptions>
{
    public int Run(CleanOptions opts)
    {
        Output.Info("Starting clean");
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
                        Output.Info($"Could not delete {projectDir.Name}/{cleanDir.Name}/{subDir.Name}: {ex.Message}");
                    }
                }
            }
            Output.Verbose($"Cleaned {projectDir.Name}");
        }
        Output.Info("Cleaned.");
        return 0;
    }
}