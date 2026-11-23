#if TEST_SCENARIOS
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Text.Json.Nodes;
using Autofac;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Ui;
using NAPS2.Scan;

namespace NAPS2.TestScenarios;

/// <summary>
/// Actions available to test scenarios. Methods are called from a background thread and marshal to the UI thread as
/// needed. Actions that can open a modal window (commands, clicks, showing forms) are dispatched without waiting, so
/// use WaitForWindow/WaitIdle afterwards.
/// </summary>
public class TestScenarioContext
{
    private readonly IComponentContext _container;
    private readonly ITestScenarioPlatform _platform;
    private readonly List<string> _log = [];
    private readonly List<string> _outputs = [];
    private Exception? _dispatchError;
    private int _pendingDispatches;

    [ThreadStatic]
    private static bool _onUiThread;

    internal TestScenarioContext(IComponentContext container, DesktopForm desktop, ITestScenarioPlatform platform,
        string outputFolder)
    {
        _container = container;
        _platform = platform;
        Desktop = desktop;
        OutputFolder = outputFolder;
    }

    public DesktopForm Desktop { get; }

    public string OutputFolder { get; }

    /// <summary>
    /// Data saved in result.json (e.g. for external tools).
    /// </summary>
    public JsonObject? Data { get; set; }

    internal IReadOnlyList<string> LogLines => _log;

    internal IReadOnlyList<string> Outputs => _outputs;

    public T Get<T>() where T : notnull => _container.Resolve<T>();

    /// <summary>
    /// Adds a line to the result log, and also to log.txt in the output folder immediately (in case of a hang).
    /// </summary>
    public void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        lock (_log)
        {
            _log.Add(line);
            File.AppendAllText(Path.Combine(OutputFolder, "log.txt"), line + Environment.NewLine);
        }
    }

    /// <summary>
    /// Runs the action on the UI thread and waits for it to complete.
    /// </summary>
    public void Ui(Action action) => Ui(() =>
    {
        action();
        return 0;
    });

    /// <summary>
    /// Runs the function on the UI thread and waits for it to complete.
    /// </summary>
    public T Ui<T>(Func<T> func)
    {
        if (_onUiThread)
        {
            // Nested calls (e.g. from an action already running on the UI thread) run directly
            return func();
        }
        WaitForDispatches();
        ThrowIfDispatchError();
        Exception? error = null;
        var result = Invoker.Current.InvokeGet(() =>
        {
            _onUiThread = true;
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                error = ex;
                return default!;
            }
            finally
            {
                _onUiThread = false;
            }
        });
        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
        return result;
    }

    /// <summary>
    /// Queues the action on the UI thread without waiting for it to complete (e.g. because it shows a modal window).
    /// Errors are reported by the next action.
    /// </summary>
    public void Dispatch(Action action)
    {
        WaitForDispatches();
        ThrowIfDispatchError();
        Interlocked.Increment(ref _pendingDispatches);
        Invoker.Current.InvokeDispatch(() =>
        {
            Interlocked.Decrement(ref _pendingDispatches);
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _dispatchError ??= ex;
            }
        });
    }

    /// <summary>
    /// Waits for dispatched actions to start, so later actions are ordered after them (synchronous invocations can
    /// otherwise run first, e.g. with WPF's dispatcher priorities).
    /// </summary>
    private void WaitForDispatches()
    {
        var stopwatch = Stopwatch.StartNew();
        while (Volatile.Read(ref _pendingDispatches) > 0)
        {
            if (stopwatch.ElapsedMilliseconds > 30000)
            {
                throw new TestScenarioException("Timed out waiting for a dispatched action to run");
            }
            Thread.Sleep(10);
        }
    }

    internal void ThrowIfDispatchError()
    {
        if (_dispatchError is { } error)
        {
            _dispatchError = null;
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    /// <summary>
    /// Waits until the function returns a non-null value, checking every 100ms.
    /// </summary>
    public async Task<T> WaitUntil<T>(Func<T?> get, string description, int timeout = 10000) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            if (get() is { } result) return result;
            if (stopwatch.ElapsedMilliseconds > timeout)
            {
                throw new TestScenarioException($"Timed out after {timeout}ms waiting for {description}");
            }
            await Task.Delay(100);
        }
    }

    public Task WaitUntil(Func<bool> condition, string description, int timeout = 10000) =>
        WaitUntil(() => condition() ? "" : null, description, timeout);

    /// <summary>
    /// Waits until there are no active operations (unless waitForOperations is false) and all thumbnails are rendered.
    /// </summary>
    public async Task WaitIdle(int timeout = 30000, bool waitForOperations = true)
    {
        var operationProgress = Get<OperationProgress>();
        var imageList = Get<UiImageList>();
        bool IsIdle() => Ui(() => !(waitForOperations && operationProgress.ActiveOperations.Count > 0) &&
                                  !imageList.Images.Any(image => image.IsThumbnailDirty));

        do
        {
            await WaitUntil(IsIdle, "the app to be idle", timeout);
            // Give the UI time to settle (layout, painting, etc.)
            await Task.Delay(300);
        } while (!IsIdle());
    }

    /// <summary>
    /// Gets the visible windows of the application, front to back.
    /// </summary>
    public List<TestScenarioWindow> GetWindows() => Ui(_platform.GetWindows);

    /// <summary>
    /// Finds a window by title (exact match, or else a unique case-insensitive substring). If no title is specified,
    /// gets the frontmost titled window.
    /// </summary>
    public TestScenarioWindow FindWindow(string? title) => Ui(() => FindWindowInternal(title));

    private TestScenarioWindow FindWindowInternal(string? title)
    {
        var windows = _platform.GetWindows();
        if (title == null)
        {
            return windows.FirstOrDefault(w => w.Title != "") ??
                   throw new TestScenarioException("No visible windows");
        }
        var exact = windows.FirstOrDefault(w => w.Title == title);
        if (exact != null) return exact;
        var partial = windows.Where(w => w.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
        if (partial.Count == 1) return partial[0];
        var titles = string.Join(", ", windows.Where(w => w.Title != "").Select(w => $"\"{w.Title}\""));
        throw new TestScenarioException(partial.Count == 0
            ? $"No window with title \"{title}\"; windows: {titles}"
            : $"Multiple windows match \"{title}\"; windows: {titles}");
    }

    /// <summary>
    /// Finds the Eto window with the given title (or the frontmost window). Native windows (e.g. message boxes) don't
    /// have an Eto window.
    /// </summary>
    private Window FindEtoWindow(string? title)
    {
        var native = FindWindowInternal(title);
        return GetEtoWindow(native) ?? throw new TestScenarioException($"Window \"{native.Title}\" isn't an Eto window");
    }

    private static Window? GetEtoWindow(TestScenarioWindow window) =>
        Application.Instance.Windows.FirstOrDefault(w => w.Title == window.Title && w.Visible);

    /// <summary>
    /// Waits for a window with the given title (see FindWindow) to be visible, and then for the app to be idle.
    /// </summary>
    public async Task<TestScenarioWindow> WaitForWindow(string title, int timeout = 10000)
    {
        TestScenarioWindow? TryFindWindow()
        {
            try
            {
                return FindWindow(title);
            }
            catch (TestScenarioException)
            {
                return null;
            }
        }
        var window = await WaitUntil(TryFindWindow, $"window \"{title}\"", timeout);
        await WaitIdle();
        return window;
    }

    public Task WaitForWindowClosed(string title, int timeout = 10000) =>
        WaitUntil(() => GetWindows().All(w => w.Title != title), $"window \"{title}\" to close", timeout);

    /// <summary>
    /// Waits for the app to be idle (so animations etc. finish) and saves a PNG of the window (see FindWindow) to the
    /// given path (relative to the output folder).
    /// </summary>
    public async Task<string> Capture(string path, string? window = null, bool waitForOperations = true)
    {
        await WaitIdle(waitForOperations: waitForOperations);
        var target = FindWindow(window);
        await WaitForActiveWindow(target);
        return CaptureWindow(target, path);
    }

    private async Task WaitForActiveWindow(TestScenarioWindow target)
    {
        if (!Ui(() => _platform.IsActive(target)))
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 1000 && !Ui(() => _platform.IsActive(target)))
            {
                await Task.Delay(50);
            }
            // Give it time to repaint
            await Task.Delay(200);
        }
    }

    internal void CaptureAll(string prefix)
    {
        var windows = GetWindows();
        for (int i = 0; i < windows.Count; i++)
        {
            CaptureWindow(windows[i], $"{prefix}{i}.png");
        }
    }

    private string CaptureWindow(TestScenarioWindow window, string path)
    {
        var fullPath = GetOutputPath(path);
        Ui(() => _platform.Capture(window, fullPath));
        AddOutput(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Describes the visible windows, their controls, and the desktop commands as JSON. If a path is specified, it's
    /// also saved to that path (relative to the output folder).
    /// </summary>
    public JsonObject DumpUi(string? path = null)
    {
        var dump = Ui(() =>
        {
            var windowsJson = new JsonArray();
            foreach (var window in _platform.GetWindows())
            {
                var obj = new JsonObject
                {
                    ["title"] = window.Title,
                    ["bounds"] = new JsonArray(window.Bounds.X, window.Bounds.Y, window.Bounds.Width,
                        window.Bounds.Height),
                    ["owner"] = _platform.GetOwner(window),
                    ["active"] = _platform.IsActive(window)
                };
                var etoWindow = GetEtoWindow(window);
                if (etoWindow != null)
                {
                    obj["type"] = etoWindow.GetType().Name;
                    var controls = TestScenarioUi.GetControls(etoWindow);
                    obj["controls"] = new JsonArray(controls
                        .Select((control, i) => (JsonNode) TestScenarioUi.Describe(control, i)).ToArray());
                }
                windowsJson.Add((JsonNode) obj);
            }
            var commands = new JsonObject();
            foreach (var (name, command) in GetDesktopCommands())
            {
                commands[name] = command.Enabled;
            }
            return new JsonObject
            {
                ["windows"] = windowsJson,
                ["commands"] = commands
            };
        });
        if (path != null)
        {
            var fullPath = GetOutputPath(path);
            TestScenarioModule.WriteJson(fullPath, dump);
            AddOutput(fullPath);
        }
        return dump;
    }

    /// <summary>
    /// Clicks a button, link, checkbox or radio button identified by its text, the text of the label before it, or
    /// its index (from DumpUi).
    /// </summary>
    public void Click(string? text = null, string? label = null, int? index = null, string? window = null)
    {
        var control = Ui(() => TestScenarioUi.FindControl(FindEtoWindow(window), text, label, index));
        Dispatch(() => TestScenarioUi.Click(control));
    }

    /// <summary>
    /// Sets the value of a text box, checkbox, dropdown (by item text or index), etc. identified by its text (or
    /// selected item), the text of the label before it, or its index (from DumpUi).
    /// </summary>
    public void SetValue(string value, string? text = null, string? label = null, int? index = null,
        string? window = null)
    {
        Ui(() => TestScenarioUi.SetValue(TestScenarioUi.FindControl(FindEtoWindow(window), text, label, index),
            value));
    }

    /// <summary>
    /// Runs a desktop command (a property name of DesktopCommands, e.g. "NewProfile" or "Settings").
    /// </summary>
    public void RunCommand(string name)
    {
        var command = Ui(() => GetDesktopCommands()
                                   .FirstOrDefault(x => string.Equals(x.Name, name,
                                       StringComparison.OrdinalIgnoreCase)).Command
                               ?? throw new TestScenarioException($"Unknown command: {name}"));
        RunCommand(command);
    }

    public void RunCommand(Func<DesktopCommands, Command> getCommand) =>
        RunCommand(Ui(() => getCommand(Get<DesktopCommands>())));

    private void RunCommand(Command command)
    {
        Dispatch(() =>
        {
            if (!command.Enabled) throw new TestScenarioException($"Command \"{command.MenuText}\" is disabled");
            command.Execute();
        });
    }

    private List<(string Name, Command Command)> GetDesktopCommands()
    {
        var commands = Get<DesktopCommands>();
        return typeof(DesktopCommands).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => typeof(Command).IsAssignableFrom(p.PropertyType))
            .Select(p => (p.Name, (Command) p.GetValue(commands)!))
            .ToList();
    }

    /// <summary>
    /// Shows a form (modal if it's a dialog), optionally configuring it first, and waits for its window (see
    /// WaitForNewWindow).
    /// </summary>
    public async Task<(T Form, TestScenarioWindow Window)> ShowForm<T>(Action<T>? setup = null) where T : IFormBase
    {
        T form = default!;
        var window = await WaitForNewWindow(() =>
        {
            form = Get<IFormFactory>().Create<T>();
            setup?.Invoke(form);
            Show(form);
        });
        return (form, window);
    }

    /// <summary>
    /// Shows a form by its type name (e.g. "SettingsForm"), modal if it's a dialog.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Form types are in NAPS2 assemblies, which aren't trimmed (TrimMode=partial)")]
    public void ShowForm(string typeName)
    {
        var type = typeof(IFormBase).Assembly.GetTypes()
                       .Concat(Desktop.GetType().Assembly.GetTypes())
                       .FirstOrDefault(t => typeof(IFormBase).IsAssignableFrom(t) && !t.IsAbstract &&
                                            (t.Name == typeName || t.FullName == typeName))
                   ?? throw new TestScenarioException($"Unknown form type: {typeName}");
        var create = typeof(IFormFactory).GetMethod(nameof(IFormFactory.Create))!.MakeGenericMethod(type);
        Dispatch(() => Show((IFormBase) create.Invoke(Get<IFormFactory>(), [])!));
    }

    private void Show(IFormBase form)
    {
        switch (form)
        {
            case Dialog dialog:
                dialog.ShowModal(Desktop);
                break;
            case Form etoForm:
                etoForm.Show();
                break;
        }
    }

    /// <summary>
    /// Closes the window with the given title (or the frontmost window).
    /// </summary>
    public void CloseWindow(string? window = null)
    {
        var etoWindow = Ui(() => FindEtoWindow(window));
        Dispatch(etoWindow.Close);
    }

    /// <summary>
    /// Resizes the window with the given title (or the frontmost window), in logical units.
    /// </summary>
    public void ResizeWindow(int width, int height, string? window = null, bool client = false)
    {
        Ui(() =>
        {
            var etoWindow = FindEtoWindow(window);
            if (client)
            {
                EtoPlatform.Current.SetClientSize(etoWindow, new Eto.Drawing.Size(width, height));
            }
            else
            {
                EtoPlatform.Current.SetFormSize(etoWindow, new Eto.Drawing.Size(width, height));
            }
        });
    }

    /// <summary>
    /// Sets a config value by its property path (e.g. "ThumbnailSize") for the rest of the test.
    /// </summary>
    public void SetConfig(string path, Func<Type, object?> getValue)
    {
        Ui(() => TestScenarioSetup.SetConfig(Get<Naps2Config>().Run, path, getValue));
    }

    public void Import(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!File.Exists(path)) throw new TestScenarioException($"File not found: {path}");
        }
        Ui(() => Get<DesktopController>().ImportFiles(paths));
    }

    public void SelectImages(params int[] indices)
    {
        Ui(() =>
        {
            var imageList = Get<UiImageList>();
            imageList.UpdateSelection(ListSelection.From(indices.Select(i => imageList.Images[i])));
        });
    }

    public void AddProfile(ScanProfile profile, bool isDefault = true)
    {
        Ui(() =>
        {
            var profileManager = Get<IProfileManager>();
            profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile), ListSelection.Of(profile));
            if (isDefault)
            {
                profileManager.DefaultProfile = profile;
            }
        });
    }

    /// <summary>
    /// Opens a dropdown's list, choosing the first enabled dropdown (in the window with the given title, or the
    /// frontmost window) that has an item matching the predicate.
    /// </summary>
    public void OpenDropDown(Func<string, bool> hasItem, string? window = null)
    {
        Ui(() =>
        {
            var etoWindow = FindEtoWindow(window);
            var dropDown = TestScenarioUi.GetControls(etoWindow).OfType<DropDown>()
                               .FirstOrDefault(d => d.Enabled && TestScenarioUi.GetItemTexts(d).Any(hasItem))
                           ?? throw new TestScenarioException($"No matching dropdown in window \"{etoWindow.Title}\"");
            dropDown.Focus();
            _platform.OpenDropDown(dropDown);
        });
    }

    /// <summary>
    /// Runs the action on the UI thread without waiting (as it may show a modal window), and waits for a new window
    /// to be visible and the app to be idle (see WaitIdle).
    /// </summary>
    public async Task<TestScenarioWindow> WaitForNewWindow(Action action, int timeout = 10000,
        bool waitForOperations = true)
    {
        var existing = GetWindows().Select(w => w.Handle).ToHashSet();
        Dispatch(action);
        var window = await WaitUntil(
            () => GetWindows().FirstOrDefault(w => w.Title != "" && !existing.Contains(w.Handle)),
            "a new window", timeout);
        await WaitIdle(waitForOperations: waitForOperations);
        return window;
    }

    /// <summary>
    /// Gets the path of a file in NAPS2.Sdk.Tests/Resources (requires running from a build in the repo).
    /// </summary>
    public string GetTestResource(string name)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var path = Path.Combine(dir.FullName, "NAPS2.Sdk.Tests", "Resources", name);
            if (File.Exists(path)) return path;
        }
        throw new TestScenarioException($"Couldn't find test resource {name} above {AppContext.BaseDirectory}");
    }

    public string GetOutputPath(string path)
    {
        var fullPath = Path.GetFullPath(Path.Combine(OutputFolder, path));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        return fullPath;
    }

    private void AddOutput(string path)
    {
        lock (_outputs)
        {
            _outputs.Add(path);
        }
        Log($"Saved {path}");
    }
}

public class TestScenarioException : Exception
{
    public TestScenarioException(string message) : base(message)
    {
    }

    public TestScenarioException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
#endif
