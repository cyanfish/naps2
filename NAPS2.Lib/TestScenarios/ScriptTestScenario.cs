#if TEST_SCENARIOS
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using NAPS2.Scan;

namespace NAPS2.TestScenarios;

/// <summary>
/// A test scenario defined by a JSON script (NAPS2_TEST_SCENARIO=path/to/script.json). Example:
/// <code>
/// {
///   "config": { "ThumbnailSize": 224 },
///   "fakeDevices": [{ "driver": "wia", "id": "x", "name": "Canon MP495" }],
///   "steps": [
///     { "action": "addProfile", "name": "My Scanner", "driver": "wia", "device": { "id": "x", "name": "Canon MP495" } },
///     { "action": "import", "path": "animals.tiff" },
///     { "action": "waitIdle" },
///     { "action": "capture", "path": "main.png" },
///     { "action": "command", "name": "NewProfile" },
///     { "action": "waitForWindow", "title": "Profile Settings" },
///     { "action": "dumpUi", "path": "ui.json" }
///   ]
/// }
/// </code>
/// Input paths are relative to the script's folder; output paths are relative to the output folder.
/// See RunStep for the full list of actions.
/// </summary>
internal class ScriptTestScenario
{
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Deserialized types are in NAPS2 assemblies, which aren't trimmed (TrimMode=partial)")]
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        // Some apps disable reflection-based serialization by default
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private readonly JsonElement _root;
    private readonly string _scriptFolder;

    private ScriptTestScenario(string path)
    {
        var options = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        using var doc = JsonDocument.Parse(File.ReadAllText(path), options);
        _root = doc.RootElement.Clone();
        _scriptFolder = Path.GetDirectoryName(Path.GetFullPath(path))!;
    }

    public static TestScenario Load(string path)
    {
        var script = new ScriptTestScenario(path);
        return new TestScenario(script.Setup, script.Run);
    }

    private void Setup(TestScenarioSetup setup)
    {
        if (_root.TryGetProperty("config", out var config))
        {
            foreach (var prop in config.EnumerateObject())
            {
                setup.Config(prop.Name, type => Deserialize(prop.Value, type));
            }
        }
        if (_root.TryGetProperty("realScanners", out var realScanners))
        {
            setup.RealScanners = realScanners.GetBoolean();
        }
        if (_root.TryGetProperty("fakeDevices", out var devices))
        {
            setup.FakeDevices = devices.EnumerateArray().Select(ParseDevice).ToList();
        }
        if (_root.TryGetProperty("fakeDeviceDelay", out var deviceDelay))
        {
            setup.FakeDeviceDelay = deviceDelay.GetInt32();
        }
        if (_root.TryGetProperty("fakeScan", out var fakeScan))
        {
            if (fakeScan.TryGetProperty("images", out var images))
            {
                setup.FakeScanImages = images.EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? null : GetInputPath(x.GetString()!))
                    .ToList();
            }
            if (fakeScan.TryGetProperty("pageDelay", out var pageDelay))
            {
                setup.FakeScanPageDelay = pageDelay.GetInt32();
            }
        }
    }

    private async Task Run(TestScenarioContext ctx)
    {
        if (!_root.TryGetProperty("steps", out var steps)) return;
        int i = 0;
        foreach (var step in steps.EnumerateArray())
        {
            i++;
            var action = Str(step, "action") ?? throw new TestScenarioException($"Step {i} has no action");
            ctx.Log($"Step {i}: {step.GetRawText()}");
            try
            {
                await RunStep(ctx, action, step);
            }
            catch (Exception ex)
            {
                throw new TestScenarioException($"Step {i} ({action}) failed: {ex.Message}", ex);
            }
        }
    }

    private async Task RunStep(TestScenarioContext ctx, string action, JsonElement step)
    {
        switch (action)
        {
            case "delay":
                await Task.Delay(Int(step, "ms") ?? 1000);
                break;
            case "waitIdle":
                await ctx.WaitIdle(Int(step, "timeout") ?? 30000);
                break;
            case "waitForWindow":
                await ctx.WaitForWindow(Required(step, "title"), Int(step, "timeout") ?? 10000);
                break;
            case "waitForWindowClosed":
                await ctx.WaitForWindowClosed(Required(step, "title"), Int(step, "timeout") ?? 10000);
                break;
            case "capture":
                await ctx.Capture(Required(step, "path"), Str(step, "window"));
                break;
            case "dumpUi":
                ctx.DumpUi(Str(step, "path") ?? "ui.json");
                break;
            case "command":
                ctx.RunCommand(Required(step, "name"));
                break;
            case "previewCommand":
                ctx.RunPreviewCommand(Required(step, "name"));
                break;
            case "showForm":
                ctx.ShowForm(Required(step, "type"));
                break;
            case "click":
                ctx.Click(Str(step, "text"), Str(step, "label"), Int(step, "index"), Str(step, "window"));
                break;
            case "setValue":
                ctx.SetValue(RequiredValue(step, "value"), Str(step, "text"), Str(step, "label"), Int(step, "index"),
                    Str(step, "window"));
                break;
            case "closeWindow":
                ctx.CloseWindow(Str(step, "window"));
                break;
            case "resizeWindow":
                ctx.ResizeWindow(Int(step, "width") ?? throw Missing("width"),
                    Int(step, "height") ?? throw Missing("height"),
                    Str(step, "window"), step.TryGetProperty("client", out var client) && client.GetBoolean());
                break;
            case "config":
                foreach (var prop in step.EnumerateObject().Where(p => p.Name != "action"))
                {
                    ctx.SetConfig(prop.Name, type => Deserialize(prop.Value, type));
                }
                break;
            case "import":
                var paths = step.TryGetProperty("paths", out var pathsElement)
                    ? pathsElement.EnumerateArray().Select(x => x.GetString()!).ToArray()
                    : [Required(step, "path")];
                ctx.Import(paths.Select(GetInputPath).ToArray());
                break;
            case "selectImages":
                ctx.SelectImages(step.GetProperty("indices").EnumerateArray().Select(x => x.GetInt32()).ToArray());
                break;
            case "addProfile":
                ctx.AddProfile(ParseProfile(step),
                    !step.TryGetProperty("default", out var isDefault) || isDefault.GetBoolean());
                break;
            default:
                throw new TestScenarioException($"Unknown action: {action}");
        }
    }

    private string GetInputPath(string path) => Path.GetFullPath(Path.Combine(_scriptFolder, path));

    private static ScanDevice ParseDevice(JsonElement element) =>
        new(ParseDriver(Required(element, "driver")), Required(element, "id"), Str(element, "name") ?? "");

    private static Driver ParseDriver(string driver) => Enum.Parse<Driver>(driver, true);

    /// <summary>
    /// Parses a profile from "name", "driver", "device" ({id, name}), and any other ScanProfile property names.
    /// </summary>
    private static ScanProfile ParseProfile(JsonElement element)
    {
        var profile = new ScanProfile
        {
            DisplayName = Str(element, "name") ?? "Test Profile",
            DriverName = Str(element, "driver") ?? "wia",
            Version = ScanProfile.CURRENT_VERSION
        };
        if (element.TryGetProperty("device", out var device))
        {
            profile.Device = new ScanProfileDevice(Required(device, "id"), Str(device, "name") ?? "");
        }
        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Name is "action" or "name" or "driver" or "device" or "default") continue;
            var propInfo = typeof(ScanProfile).GetProperty(prop.Name,
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? throw new TestScenarioException($"Unknown ScanProfile property: {prop.Name}");
            propInfo.SetValue(profile, Deserialize(prop.Value, propInfo.PropertyType));
        }
        return profile;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Deserialized types are in NAPS2 assemblies, which aren't trimmed (TrimMode=partial)")]
    private static object? Deserialize(JsonElement element, Type type) => element.Deserialize(type, JsonOptions);

    private static string? Str(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString()
            : null;

    private static int? Int(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetInt32()
            : null;

    private static string Required(JsonElement element, string name) => Str(element, name) ?? throw Missing(name);

    private static string RequiredValue(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText()
            : throw Missing(name);

    private static TestScenarioException Missing(string name) => new($"Missing property \"{name}\"");
}
#endif
