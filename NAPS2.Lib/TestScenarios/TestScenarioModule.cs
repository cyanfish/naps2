#if TEST_SCENARIOS
using System.Text.Json;
using System.Text.Json.Nodes;
using Autofac;
using NAPS2.EtoForms.Ui;
using NAPS2.Platform;
using NAPS2.Scan.Internal;
using NAPS2.Update;

namespace NAPS2.TestScenarios;

/// <summary>
/// Runs a test scenario if NAPS2_TEST_SCENARIO is set to a scenario name (from a TestScenarioProvider, or "list" to
/// list them without variants, or "list-all" to list all of them) or the path of a JSON script (see ScriptTestScenario). NAPS2_TEST_DATA must also be set, and output files
/// (including result.json) are written to its "output" subfolder. If NAPS2_TEST_SCENARIO_KEEP_OPEN is "1", the app
/// stays open when the scenario finishes (e.g. so an external tool can capture the final state).
/// </summary>
public class TestScenarioModule : Module
{
    private readonly string _nameOrPath;
    private readonly string _outputFolder = Path.Combine(Paths.AppData, "output");
    private readonly bool _keepOpen = Environment.GetEnvironmentVariable("NAPS2_TEST_SCENARIO_KEEP_OPEN") == "1";
    private readonly TestScenarioSetup _setup = new();
    private TestScenario? _scenario;

    public static TestScenarioModule? CreateFromEnvironment() =>
        Environment.GetEnvironmentVariable("NAPS2_TEST_SCENARIO") is { Length: > 0 } nameOrPath
            ? new TestScenarioModule(nameOrPath)
            : null;

    private TestScenarioModule(string nameOrPath)
    {
        if (!Paths.IsTestAppDataPath)
        {
            throw new InvalidOperationException("Test scenarios require NAPS2_TEST_DATA to be set");
        }
        _nameOrPath = nameOrPath;
        Directory.CreateDirectory(_outputFolder);
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(_setup);
        builder.RegisterType<CommonTestScenarios>().As<TestScenarioProvider>();
        builder.RegisterDecorator<FakeScanBridgeFactory, IScanBridgeFactory>();
        builder.RegisterDecorator<FakeUpdateChecker, IUpdateChecker>();
        builder.RegisterDecorator<FakeOpenWith, IOpenWith>();
        builder.RegisterBuildCallback(ctx =>
        {
            try
            {
                _scenario = _nameOrPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    ? ScriptTestScenario.Load(_nameOrPath)
                    : GetNamedScenario(ctx.Resolve<IEnumerable<TestScenarioProvider>>());
                _scenario.Setup?.Invoke(_setup);
                _setup.ApplyConfig(ctx.Resolve<Naps2Config>());
            }
            catch (Exception ex)
            {
                WriteResult(new JsonObject { ["success"] = false, ["error"] = ex.ToString() });
                Environment.Exit(1);
            }
        });
    }

    private TestScenario GetNamedScenario(IEnumerable<TestScenarioProvider> providers)
    {
        var all = providers.SelectMany(p => p.GetScenarios()).ToList();
        var scenarios = all.ToDictionary(x => x.Name, x => x.Scenario);
        if (_nameOrPath is "list" or "list-all")
        {
            var names = all.Where(x => _nameOrPath == "list-all" || !x.Variant).Select(x => (JsonNode) x.Name);
            WriteResult(new JsonObject
            {
                ["success"] = true,
                ["scenarios"] = new JsonArray(names.ToArray())
            });
            Environment.Exit(0);
        }
        return scenarios.GetValueOrDefault(_nameOrPath) ?? throw new TestScenarioException(
            $"Unknown test scenario \"{_nameOrPath}\"; scenarios: {string.Join(", ", scenarios.Keys)}");
    }

    public void Attach(IComponentContext container, DesktopForm desktop)
    {
        desktop.Shown += (_, _) => Task.Run(() => Run(container, desktop));
    }

    private async Task Run(IComponentContext container, DesktopForm desktop)
    {
        var ctx = new TestScenarioContext(container, desktop, container.Resolve<ITestScenarioPlatform>(),
            _outputFolder);
        var result = new JsonObject();
        try
        {
            ctx.Log("Started");
            await ctx.WaitIdle();
            await _scenario!.Run(ctx);
            ctx.ThrowIfDispatchError();
            ctx.Log("Finished");
            result["success"] = true;
        }
        catch (Exception ex)
        {
            ctx.Log($"Error: {ex.Message}");
            result["success"] = false;
            result["error"] = ex.ToString();
            try
            {
                ctx.DumpUi("error-ui.json");
                ctx.CaptureAll("error-window-");
            }
            catch (Exception ex2)
            {
                ctx.Log($"Error capturing error state: {ex2.Message}");
            }
        }
        result["data"] = ctx.Data;
        result["log"] = new JsonArray(ctx.LogLines.Select(x => (JsonNode) x).ToArray());
        result["outputs"] = new JsonArray(ctx.Outputs.Select(x => (JsonNode) x).ToArray());
        WriteResult(result);
        if (!_keepOpen)
        {
            // The data folder is throwaway, so there's no need to close windows (which modal native windows like
            // message boxes would block anyway)
            Environment.Exit(0);
        }
    }

    private void WriteResult(JsonObject result) => WriteJson(Path.Combine(_outputFolder, "result.json"), result);

    /// <summary>
    /// Writes a JSON file atomically, so external tools never read a partial file.
    /// </summary>
    internal static void WriteJson(string path, JsonNode json)
    {
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(tempPath, path, true);
    }
}
#endif
