using System.Runtime.InteropServices;

namespace NAPS2.Tools.Project;

public class TestCommand : ICommand<TestOptions>
{
    public int Run(TestOptions opts)
    {
        var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        var depsRootPath = OperatingSystem.IsMacOS()
            ? $"NAPS2.App.Mac/bin/Debug/net11.0-macos/osx-{arch}"
            : OperatingSystem.IsLinux()
                ? $"NAPS2.App.Gtk/bin/Debug/net10.0/linux-{arch}"
                : $"NAPS2.App.WinForms/bin/Debug/net10.0-windows/win-{arch}";

        var scopes = string.IsNullOrEmpty(opts.Scope) ? ["sdk", "lib", "app"] : opts.Scope.Split('+');
        var projects = new List<string>();
        if (scopes.Contains("sdk")) projects.Add("NAPS2.Sdk.Tests");
        if (scopes.Contains("lib")) projects.Add("NAPS2.Lib.Tests");
        if (scopes.Contains("app")) projects.Add("NAPS2.App.Tests");
        if (projects.Count == 0)
        {
            Output.Info("Invalid scopes, no tests run.");
            return 0;
        }

        Output.Info($"Building test projects ({string.Join(", ", scopes)})");
        foreach (var project in projects)
        {
            Cli.Run("dotnet", $"build {project}");
        }

        // Run all the test modules in a single invocation so they can execute in parallel
        Output.Info($"Running tests ({string.Join(", ", scopes)})");
        var testModules = string.Join(";", projects.Select(project => GetTestModulePath(project, arch)));
        Cli.Run("dotnet",
            $"test --test-modules \"{testModules}\" --root-directory \"{Paths.SolutionRoot}\" --output Detailed --retry-failed-tests 1 -- --parallel-algorithm aggressive",
            new()
        {
            { "NAPS2_TEST_DEPS", Path.Combine(Paths.SolutionRoot, depsRootPath) },
            { "NAPS2_TEST_NOGUI", opts.NoGui ? "1" : "0" },
            { "NAPS2_TEST_NONETWORK", opts.NoNetwork ? "1" : "0" },
            { "NAPS2_TEST_IMAGES", opts.Images ?? "" }
        });
        Output.Info("Tests passed.");
        return 0;
    }

    // Module paths are globs relative to the root directory
    private static string GetTestModulePath(string project, string arch)
    {
        if (OperatingSystem.IsWindows())
        {
            return $"{project}/bin/Debug/net10.0-windows/{project}.dll";
        }
        if (OperatingSystem.IsMacOS() && project != "NAPS2.App.Tests")
        {
            // On Mac, the Sdk/Lib test projects are also built as app bundles (to test native images etc.), so we run
            // the bundle executable
            return $"{project}/bin/Debug/net11.0-macos/osx-{arch}/{project}.app/Contents/MacOS/{project}";
        }
        return $"{project}/bin/Debug/net10.0/{project}.dll";
    }
}
