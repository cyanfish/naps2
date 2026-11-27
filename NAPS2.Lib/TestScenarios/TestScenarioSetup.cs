#if TEST_SCENARIOS
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NAPS2.Config.Model;
using NAPS2.Scan;
using NAPS2.Update;

namespace NAPS2.TestScenarios;

/// <summary>
/// State that a test scenario sets up before the application starts (config, fake devices, etc.).
/// </summary>
public class TestScenarioSetup
{
    private readonly List<Action<Naps2Config>> _configActions = [];

    public TestScenarioSetup()
    {
        // Avoid network access and anything the user might notice outside the test instance
        Config(c => c.CheckForUpdates, false);
        Config(c => c.DisableScannerSharing, true);
    }

    /// <summary>
    /// If true, real scanner drivers are used instead of fake devices. Be careful as this can scan with real devices.
    /// </summary>
    public bool RealScanners { get; set; }

    /// <summary>
    /// The devices shown for each driver (unless RealScanners is set).
    /// </summary>
    public List<ScanDevice> FakeDevices { get; set; } =
    [
        ..new[] { Driver.Wia, Driver.Twain, Driver.Apple, Driver.Sane, Driver.Escl }
            .Select(driver => new ScanDevice(driver, "test-scanner", "Test Scanner"))
    ];

    public int FakeDeviceDelay { get; set; }

    /// <summary>
    /// The pages returned by each fake scan, as image file paths (null for a blank page).
    /// </summary>
    public List<string?> FakeScanImages { get; set; } = [null];

    public int FakeScanPageDelay { get; set; }

    /// <summary>
    /// The result of update checks (null for no update).
    /// </summary>
    public UpdateInfo? FakeUpdate { get; set; }

    public bool FakeUpdateCheckNeverCompletes { get; set; }

    /// <summary>
    /// The applications available for "open with" (null to use the real ones).
    /// </summary>
    public List<OpenWithEntry>? FakeOpenWith { get; set; }

    /// <summary>
    /// Sets a config value for the duration of the test (the user config file isn't changed).
    /// </summary>
    public void Config<T>(Expression<Func<CommonConfig, T>> accessor, T value)
    {
        _configActions.Add(config => config.Run.Set(accessor, value));
    }

    /// <summary>
    /// Sets a config value by its property path (e.g. "ThumbnailSize" or "PdfSettings.Metadata.Title").
    /// </summary>
    /// <param name="path">The property path, case-insensitive.</param>
    /// <param name="getValue">Gets the value given the property type.</param>
    public void Config(string path, Func<Type, object?> getValue)
    {
        _configActions.Add(config => SetConfig(config.Run, path, getValue));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "Config types are in NAPS2 assemblies, which aren't trimmed (TrimMode=partial)")]
    internal static void SetConfig(ConfigScope<CommonConfig> scope, string path, Func<Type, object?> getValue)
    {
        var param = Expression.Parameter(typeof(CommonConfig), "c");
        Expression body = param;
        foreach (var part in path.Split('.'))
        {
            var prop = body.Type.GetProperty(part,
                           BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                       ?? throw new ArgumentException($"Unknown config property: {path}");
            body = Expression.Property(body, prop);
        }
        var accessor = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(CommonConfig), body.Type), body,
            param);
        typeof(ConfigScope<CommonConfig>).GetMethod(nameof(ConfigScope<CommonConfig>.Set))!
            .MakeGenericMethod(body.Type)
            .Invoke(scope, [accessor, getValue(body.Type)]);
    }

    internal void ApplyConfig(Naps2Config config)
    {
        foreach (var action in _configActions)
        {
            action(config);
        }
    }
}
#endif
