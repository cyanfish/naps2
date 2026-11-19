using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Threading;

namespace NAPS2.Tools.Project;

/// <summary>
/// Runs a test scenario (see NAPS2.Lib/TestScenarios) in a Debug build of the app with a fresh data folder, waits for it
/// to finish, and prints the result.
/// </summary>
public class ScenarioCommand : ICommand<ScenarioOptions>
{
    public int Run(ScenarioOptions opts)
    {
        if (new[] { opts.Name != null, opts.ScriptPath != null, opts.List }.Count(x => x) != 1)
        {
            throw new InvalidOperationException("Specify exactly one of --name, --path or --list");
        }
        if (opts.All && !opts.List)
        {
            throw new InvalidOperationException("--all only applies to --list");
        }
        var scriptPath = opts.ScriptPath != null ? Path.GetFullPath(opts.ScriptPath) : null;
        if (scriptPath != null && (!File.Exists(scriptPath) || Path.GetExtension(scriptPath) != ".json"))
        {
            throw new InvalidOperationException($"Script not found or not a .json file: {scriptPath}");
        }

        var (project, exePath) = GetApp(opts.App);
        if (opts.Build)
        {
            Output.Info($"Building {project}");
            Cli.Run("dotnet", $"build {project} -c Debug");
        }
        if (!File.Exists(exePath))
        {
            throw new InvalidOperationException($"App not found (use --build): {exePath}");
        }

        var scenarioName = opts.List ? opts.All ? "list-all" : "list" : opts.Name ?? Path.GetFileNameWithoutExtension(scriptPath!);
        // The app's data folder, with outputs in its "output" subfolder
        var dataPath = Path.GetFullPath(opts.OutputPath ??
                                        Path.Combine(Path.GetTempPath(), "naps2-scenarios", scenarioName));
        if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
        Directory.CreateDirectory(dataPath);
        if (opts.DataPath != null)
        {
            ProjectHelper.CopyDirectory(opts.DataPath, dataPath);
        }
        var outputPath = Path.Combine(dataPath, "output");
        var resultPath = Path.Combine(outputPath, "result.json");
        var logPath = Path.Combine(outputPath, "log.txt");

        var startInfo = new ProcessStartInfo(exePath)
        {
            UseShellExecute = false,
            WorkingDirectory = Paths.SolutionRoot
        };
        startInfo.Environment["NAPS2_TEST_DATA"] = dataPath;
        startInfo.Environment["NAPS2_TEST_SCENARIO"] = scriptPath ?? scenarioName;
        if (opts.KeepOpen)
        {
            startInfo.Environment["NAPS2_TEST_SCENARIO_KEEP_OPEN"] = "1";
        }

        Output.Info($"Running scenario {scenarioName} ({exePath})");
        Output.Verbose($"Output: {outputPath}");
        using var proc = Process.Start(startInfo) ?? throw new Exception($"Could not start {exePath}");
        var stopwatch = Stopwatch.StartNew();
        // With --keep-open we only wait for the result, otherwise we wait for the app to exit too
        while (opts.KeepOpen ? !File.Exists(resultPath) && !proc.HasExited : !proc.WaitForExit(100))
        {
            if (stopwatch.Elapsed.TotalSeconds > opts.Timeout)
            {
                proc.Kill(true);
                if (File.Exists(logPath))
                {
                    Output.Info(File.ReadAllText(logPath).TrimEnd());
                }
                Output.Info($"Timed out after {opts.Timeout}s; output: {outputPath}");
                Environment.Exit(1);
            }
            if (opts.KeepOpen) Thread.Sleep(100);
        }

        if (!File.Exists(resultPath))
        {
            Output.Info($"The app exited (code {proc.ExitCode}) without a result; output: {outputPath}");
            Environment.Exit(1);
        }
        var result = JsonNode.Parse(File.ReadAllText(resultPath))!;
        foreach (var line in result["log"]?.AsArray() ?? [])
        {
            Output.Verbose(line!.GetValue<string>());
        }
        foreach (var scenario in result["scenarios"]?.AsArray() ?? [])
        {
            Output.Info(scenario!.GetValue<string>());
        }
        foreach (var output in result["outputs"]?.AsArray() ?? [])
        {
            Output.Info($"Output: {output!.GetValue<string>()}");
        }
        if (opts.KeepOpen && !proc.HasExited)
        {
            Output.Info($"The app is still running (PID {proc.Id})");
        }
        if (result["success"]?.GetValue<bool>() != true)
        {
            var error = result["error"]?.GetValue<string>() ?? "";
            Output.Info($"Scenario failed: {error.Split('\n')[0].Trim()}");
            Output.Info($"Result: {resultPath}");
            Environment.Exit(1);
        }
        Output.Info($"Scenario succeeded; result: {resultPath}");
        return 0;
    }

    private static (string Project, string ExePath) GetApp(string? app)
    {
        if (app != null && !OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("--app is only supported on Windows");
        }
        var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        string project;
        string exePath;
        if (OperatingSystem.IsWindows())
        {
            project = ProjectHelper.GetWindowsAppProject(app);
            exePath = Path.Combine(Paths.SolutionRoot, project, "bin", "Debug", "net11.0-windows", $"win-{arch}",
                "NAPS2.exe");
        }
        else if (OperatingSystem.IsMacOS())
        {
            project = "NAPS2.App.Mac";
            exePath = Path.Combine(Paths.SolutionRoot, project, "bin", "Debug", "net11.0-macos", "NAPS2.app",
                "Contents", "MacOS", "NAPS2");
        }
        else
        {
            project = "NAPS2.App.Gtk";
            exePath = Path.Combine(Paths.SolutionRoot, project, "bin", "Debug", "net11.0", $"linux-{arch}", "naps2");
        }
        return (project, exePath);
    }
}
