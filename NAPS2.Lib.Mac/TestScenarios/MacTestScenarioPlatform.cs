#if TEST_SCENARIOS
using Eto.Drawing;

namespace NAPS2.TestScenarios;

/// <summary>
/// Window access for macOS. Captures use the window server's image of the app's own window.
/// </summary>
public class MacTestScenarioPlatform : ITestScenarioPlatform
{
    private const uint kCGWindowListOptionIncludingWindow = 1 << 3;
    private const uint kCGWindowImageBoundsIgnoreFraming = 1 << 0;
    private const uint kCGWindowImageBestResolution = 1 << 3;

    public List<TestScenarioWindow> GetWindows()
    {
        var screenHeight = NSScreen.Screens[0].Frame.Height;
        var app = NSApplication.SharedApplication;
        return app.DangerousWindows.OfType<NSWindow>()
            .Where(w => w.IsVisible && w.Frame.Width > 0 && w.Frame.Height > 0)
            .OrderBy(w => w == app.ModalWindow ? 0 : w.IsKeyWindow ? 1 : w.IsMainWindow ? 2 : 3)
            .Select(w => new TestScenarioWindow(GetTitle(w),
                new Rectangle((int) w.Frame.X, (int) (screenHeight - w.Frame.Y - w.Frame.Height),
                    (int) w.Frame.Width, (int) w.Frame.Height), w))
            .ToList();
    }

    public string? GetOwner(TestScenarioWindow window)
    {
        var native = (NSWindow) window.Handle;
        if (native.SheetParent != null) return $"sheet of {GetTitle(native.SheetParent)}";
        return native.ParentWindow != null ? GetTitle(native.ParentWindow) : null;
    }

    private static string GetTitle(NSWindow window)
    {
        if (!string.IsNullOrEmpty(window.Title) || window.ContentView == null) return window.Title ?? "";
        // Message boxes (alerts) have no title, so they're identified by their message (the caption in Eto)
        return FindLabels(window.ContentView).FirstOrDefault() ?? "";
    }

    private static IEnumerable<string> FindLabels(NSView view)
    {
        if (view is NSTextField { Editable: false } label && !string.IsNullOrEmpty(label.StringValue))
        {
            yield return label.StringValue;
        }
        foreach (var text in view.Subviews.SelectMany(FindLabels))
        {
            yield return text;
        }
    }

    public void Capture(TestScenarioWindow window, string path)
    {
        var nsWindow = (NSWindow) window.Handle;
        var handle = CGWindowListCreateImage(CGRect.Null, kCGWindowListOptionIncludingWindow,
            (uint) nsWindow.WindowNumber, kCGWindowImageBoundsIgnoreFraming | kCGWindowImageBestResolution);
        using var rep = handle != IntPtr.Zero
            ? new NSBitmapImageRep(Runtime.GetINativeObject<CGImage>(handle, true)!)
            : RenderWindowContent(nsWindow);
        using var data = rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png)!;
        if (!data.Save(path, true, out var error))
        {
            throw new InvalidOperationException($"Could not save the capture: {error}");
        }
    }

    /// <summary>
    /// Renders the window's views ourselves, for windows the window server won't give us an image of (e.g. alerts).
    /// Some effects like vibrancy aren't rendered accurately.
    /// </summary>
    private static NSBitmapImageRep RenderWindowContent(NSWindow window)
    {
        var view = window.ContentView?.Superview ?? window.ContentView ??
                   throw new InvalidOperationException("Could not capture the window");
        var rep = view.BitmapImageRepForCachingDisplayInRect(view.Bounds) ??
                  throw new InvalidOperationException("Could not capture the window");
        view.CacheDisplay(view.Bounds, rep);
        return rep;
    }

    public bool IsActive(TestScenarioWindow window) =>
        NSApplication.SharedApplication.Active && ((NSWindow) window.Handle).IsKeyWindow;

    // Obsolete in the macOS 15 SDK but still functional
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, uint listOption, uint windowId,
        uint imageOption);
}
#endif
