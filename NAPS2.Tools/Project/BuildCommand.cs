namespace NAPS2.Tools.Project;

public static class BuildCommand
{
    public static int Run(BuildOptions opts)
    {
        foreach (var config in GetConfigs(opts.What))
        {
            Console.WriteLine($"---------- BUILDING CONFIGURATION: {config} ----------");
            Cli.Run("dotnet", $"build -c {config}");
        }
        return 0;
    }

    private static IEnumerable<string> GetConfigs(string? what)
    {
        switch (what?.ToLower())
        {
            case "debug":
                return new[] { "Debug" };
            case "exe":
                return new[] { "InstallerEXE" };
            case "msi":
                return new[] { "InstallerMSI" };
            case "zip":
                return new[] { "Standalone" };
            case "all":
                return new[] { "Debug", "InstallerEXE", "InstallerMSI", "Standalone" };
        }
        return new string[] { };
    }
}