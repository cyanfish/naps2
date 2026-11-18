#if TEST_SCENARIOS
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan;

namespace NAPS2.TestScenarios;

/// <summary>
/// Provides named test scenarios. Register implementations with Autofac (e.g. in a platform module for
/// platform-specific scenarios). Most scenarios show a particular UI state and end with Ready.
/// </summary>
internal abstract class TestScenarioProvider
{
    private readonly List<(string, TestScenario, bool)> _scenarios = [];

    protected TestScenarioContext Ctx { get; private set; } = null!;

    protected DesktopForm Desktop => Ctx.Desktop;

    protected Naps2Config Config => Get<Naps2Config>();

    protected string DesktopTitle => Ctx.Ui(() => Desktop.Title);

    public IEnumerable<(string Name, TestScenario Scenario, bool Variant)> GetScenarios()
    {
        Define();
        return _scenarios;
    }

    protected abstract void Define();

    /// <param name="name">The scenario name.</param>
    /// <param name="run">Sets up the UI state and calls Ready.</param>
    /// <param name="setup">Configures the app before it starts (e.g. config values or fake devices).</param>
    /// <param name="variant">Whether the scenario has the same layout as another one, with only different strings or
    /// state (e.g. another error message). These are only listed with "list-all".</param>
    protected void Add(string name, Func<Task> run, Action<TestScenarioSetup>? setup = null, bool variant = false)
    {
        _scenarios.Add((name, new TestScenario(setup, async ctx =>
        {
            Ctx = ctx;
            await run();
        }), variant));
    }

    protected T Get<T>() where T : notnull => Ctx.Get<T>();

    /// <summary>
    /// Gets the resx key of a resource string (e.g. "UiStrings.Scan") from the expression.
    /// </summary>
    protected static string K(string value, [CallerArgumentExpression(nameof(value))] string expr = "") => expr;

    /// <summary>
    /// Gets the resx key and text of a resource string.
    /// </summary>
    protected static (string Key, string Text) P(string value,
        [CallerArgumentExpression(nameof(value))] string expr = "") => (expr, value);

    /// <summary>
    /// Waits for the UI state to be ready and captures the window as screenshot.png. The parameters are also saved as
    /// the result data, for external tools that capture the final state (with NAPS2_TEST_SCENARIO_KEEP_OPEN).
    /// </summary>
    /// <param name="strings">Resx keys of the strings that are shown.</param>
    /// <param name="title">Title of the window to capture (default: the frontmost titled window).</param>
    /// <param name="rect">For external tools, an explicit screen rect to capture.</param>
    /// <param name="before">UI action to run right before capture (e.g. opening a dropdown).</param>
    /// <param name="delay">Extra time to wait before capture.</param>
    /// <param name="expand">For external tools, a native combo box to expand (by name or value prefix).</param>
    /// <param name="popupCrop">For external tools, to crop to popups (menus/dropdowns) plus this margin.</param>
    /// <param name="positions">For external tools, explicit screen rects of strings (e.g. icon-only buttons).</param>
    /// <param name="waitForOperations">Whether to wait for active operations to finish.</param>
    protected async Task Ready(string[] strings, string? title = null, Func<Rectangle>? rect = null,
        Action? before = null, int delay = 0, string? expand = null, int? popupCrop = null,
        Func<Dictionary<string, Rectangle>>? positions = null, bool waitForOperations = true)
    {
        await Ctx.WaitIdle(waitForOperations: waitForOperations);
        if (before != null)
        {
            Ctx.Ui(before);
            await Ctx.WaitIdle(waitForOperations: waitForOperations);
        }
        await Task.Delay(delay);
        Ctx.Data = Ctx.Ui(() =>
        {
            var obj = new JsonObject
            {
                ["strings"] = new JsonArray(strings.Select(x => (JsonNode) x).ToArray()),
                ["title"] = title,
                ["expand"] = expand,
                ["popupCrop"] = popupCrop
            };
            if (rect != null)
            {
                obj["rect"] = ToJson(rect());
            }
            if (positions != null)
            {
                var posObj = new JsonObject();
                foreach (var (key, value) in positions())
                {
                    posObj[key] = ToJson(value);
                }
                obj["positions"] = posObj;
            }
            return obj;
        });
        await Ctx.Capture("screenshot.png", title, waitForOperations);
    }

    private static JsonArray ToJson(Rectangle r) => new(r.Left, r.Top, r.Right, r.Bottom);

    /// <summary>
    /// Shows a form (configured by setup) and calls Ready. The before and positions callbacks get the form.
    /// </summary>
    protected async Task ShowForm<T>(string[] strings, Action<T>? setup = null, Action<T>? before = null,
        int? popupCrop = null, int delay = 0, Func<T, Dictionary<string, Rectangle>>? positions = null)
        where T : IFormBase
    {
        var (form, window) = await Ctx.ShowForm(setup);
        await Ready(strings, window.Title, before: before == null ? null : () => before(form), delay: delay,
            popupCrop: popupCrop, positions: positions == null ? null : () => positions(form));
    }

    protected async Task ShowMessage(string[] strings, string message, string caption,
        MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxType type = MessageBoxType.Information)
    {
        var window = await Ctx.WaitForNewWindow(() => MessageBox.Show(Desktop, message, caption, buttons, type));
        await Ready(strings, window.Title);
    }

    protected async Task ShowError(string[] strings, string message, string? details = null)
    {
        var window = await Ctx.WaitForNewWindow(() =>
        {
            var errorOutput = Get<ErrorOutput>();
            if (details != null) errorOutput.DisplayError(message, details);
            else errorOutput.DisplayError(message);
        });
        await Ready(strings, window.Title);
    }

    protected async Task ShowProgress(string[] strings, string title, string status, int current = 0, int max = 0,
        OperationProgressType type = OperationProgressType.Default, bool allowBackground = true)
    {
        var op = new FakeOperation(title, status, current, max, type, allowBackground);
        var window = await Ctx.WaitForNewWindow(() => Get<OperationProgress>().ShowModalProgress(op),
            waitForOperations: false);
        await Ready(strings, window.Title, waitForOperations: false);
    }

    protected async Task ShowFileDialog(string[] strings, FileFilterGroup groups, bool save = true)
    {
        var dir = Path.Combine(Paths.AppData, "empty-dir");
        Directory.CreateDirectory(dir);
        var window = await Ctx.WaitForNewWindow(() =>
        {
            FileDialog dialog = save ? new SaveFileDialog() : new OpenFileDialog();
            Get<FileFilters>().Set(dialog, groups);
            dialog.Directory = new Uri(dir);
            dialog.ShowDialog(Desktop);
        });
        await Ready(strings, window.Title, expand: save ? "Save as type:" : "*", popupCrop: 30, delay: 1000);
    }

    /// <summary>
    /// Adds images to the image list, selecting the first ones.
    /// </summary>
    protected List<UiImage> LoadImages(int count = 4, int selected = 1)
    {
        var resources = new[] { "ocr_test.jpg", "autocrop_table.jpg", "deskewed.jpg", "autocrop_doc.jpg" };
        var scanningContext = Get<ScanningContext>();
        var uiImages = Enumerable.Range(0, count)
            .Select(i => new UiImage(scanningContext.CreateProcessedImage(
                scanningContext.ImageContext.Load(Ctx.GetTestResource(resources[i % resources.Length])))))
            .ToList();
        Ctx.Ui(() =>
        {
            var imageList = Get<UiImageList>();
            imageList.Mutate(new ImageListMutation.Append(uiImages));
            imageList.UpdateSelection(ListSelection.From(uiImages.Take(selected)));
        });
        return uiImages;
    }

    /// <summary>
    /// The driver used for fake devices and profiles, as other drivers fail validation.
    /// </summary>
    protected static readonly Driver PlatformDriver =
        OperatingSystem.IsWindows() ? Driver.Wia : OperatingSystem.IsMacOS() ? Driver.Apple : Driver.Sane;

    protected static ScanProfile NewProfile(string name = "My Scanner", string? driver = null,
        string deviceName = "Canon CanoScan LiDE 400") => new()
    {
        DisplayName = name,
        DriverName = driver ?? PlatformDriver.ToString().ToLowerInvariant(),
        Device = new ScanProfileDevice("fake-device", deviceName),
        Version = ScanProfile.CURRENT_VERSION
    };

    protected ScanProfile AddProfile(string name = "My Scanner", string? driver = null,
        string deviceName = "Canon CanoScan LiDE 400")
    {
        var profile = NewProfile(name, driver, deviceName);
        Ctx.AddProfile(profile, isDefault: Ctx.Ui(() => Get<IProfileManager>().DefaultProfile == null));
        return profile;
    }

    protected static void SetImage(ImageFormBase form, UiImage image)
    {
        form.Image = image;
        form.SelectedImages = [image];
    }

    protected static readonly List<ScanDevice> ExampleDevices =
    [
        new(PlatformDriver, "canon", "Canon PIXMA MG3600"),
        new(PlatformDriver, "epson", "Epson Perfection V39"),
        new(PlatformDriver, "hp", "HP OfficeJet 8020")
    ];

    private class FakeOperation : OperationBase
    {
        public FakeOperation(string title, string status, int current, int max, OperationProgressType type,
            bool allowBackground)
        {
            ProgressTitle = title;
            AllowCancel = true;
            AllowBackground = allowBackground;
            Status = new OperationStatus
            {
                StatusText = status,
                CurrentProgress = current,
                MaxProgress = max,
                ProgressType = type
            };
            RunAsync(async () =>
            {
                await Task.Delay(Timeout.Infinite, CancelToken);
                return true;
            });
        }
    }
}
#endif
