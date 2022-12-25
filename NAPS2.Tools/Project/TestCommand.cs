using System.Runtime.InteropServices;

namespace NAPS2.Tools.Project;

public class TestCommand : ICommand<TestOptions>
{
    public int Run(TestOptions opts)
    {
        // TODO: Framework options (e.g. "-f net462")
        var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        var depsRootPath = OperatingSystem.IsMacOS()
            ? $"NAPS2.App.Mac/bin/Debug/net7-macos10.15/osx-{arch}"
            : OperatingSystem.IsLinux()
                ? $"NAPS2.App.Gtk/bin/Debug/net6/linux-{arch}"
                : "NAPS2.App.WinForms/bin/Debug/net462";
        var frameworkArg = OperatingSystem.IsWindows() ? "" : "-f net6";

        void RunTests(string project)
        {
            Cli.Run("dotnet", $"test -l \"console;verbosity=normal\" {frameworkArg} {project}", new()
            {
                { "NAPS2_TEST_DEPS", Path.Combine(Paths.SolutionRoot, depsRootPath) },
                { "NAPS2_TEST_NOGUI", opts.NoGui ? "1" : "0" }
            });
        }

        Output.Info("Running tests");
        RunTests("NAPS2.Sdk.Tests");
        RunTests("NAPS2.Lib.Tests");
        RunTests("NAPS2.App.Tests");
        Output.Info("Tests passed.");
        return 0;
    }
}