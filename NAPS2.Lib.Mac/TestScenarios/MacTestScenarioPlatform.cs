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
            .Select(w => new TestScenarioWindow(w.Title ?? "",
                new Rectangle((int) w.Frame.X, (int) (screenHeight - w.Frame.Y - w.Frame.Height),
                    (int) w.Frame.Width, (int) w.Frame.Height), w))
            .ToList();
    }

    public void Capture(TestScenarioWindow window, string path)
    {
        var nsWindow = (NSWindow) window.Handle;
        var handle = CGWindowListCreateImage(CGRect.Null, kCGWindowListOptionIncludingWindow,
            (uint) nsWindow.WindowNumber, kCGWindowImageBoundsIgnoreFraming | kCGWindowImageBestResolution);
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Could not capture the window");
        }
        using var image = Runtime.GetINativeObject<CGImage>(handle, true)!;
        using var rep = new NSBitmapImageRep(image);
        using var data = rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png)!;
        if (!data.Save(path, true, out var error))
        {
            throw new InvalidOperationException($"Could not save the capture: {error}");
        }
    }

    // Obsolete in the macOS 15 SDK but still functional
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGWindowListCreateImage(CGRect screenBounds, uint listOption, uint windowId,
        uint imageOption);
}
#endif
