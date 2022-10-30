using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Installation;

public static class FlatpakInstaller
{
    public static void Install(Platform platform, string version, bool run, bool verbose)
    {
        var flatpakPath = ProjectHelper.GetPackagePath("flatpak", platform, version);
        Console.WriteLine($"Starting flatpak install for: {flatpakPath}");

        try
        {
            Cli.Run("flatpak", $"uninstall --user --noninteractive com.naps2.Naps2", verbose);
        }
        catch (Exception)
        {
            // Ok if uninstall fails
        }

        Cli.Run("flatpak", $"install --user --noninteractive {flatpakPath}", verbose);
        if (run)
        {
            Cli.Run("flatpak", $"run com.naps2.Naps2", verbose);
        }

        Console.WriteLine("Installed.");
    }
}