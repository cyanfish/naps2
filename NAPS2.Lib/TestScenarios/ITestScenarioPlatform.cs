#if TEST_SCENARIOS
using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.TestScenarios;

/// <summary>
/// Platform-specific window access for test scenarios. All methods must be called on the UI thread.
/// </summary>
public interface ITestScenarioPlatform
{
    /// <summary>
    /// Gets the visible top-level windows of the current process (including native dialogs like message boxes),
    /// ordered front to back.
    /// </summary>
    List<TestScenarioWindow> GetWindows();

    /// <summary>
    /// Saves an image of the window's own rendered content (never a region of the screen) as a PNG file.
    /// </summary>
    void Capture(TestScenarioWindow window, string path);

    /// <summary>
    /// Gets whether the window is the active (foreground) window.
    /// </summary>
    bool IsActive(TestScenarioWindow window);

    void OpenDropDown(DropDown dropDown) =>
        throw new NotSupportedException("Opening dropdowns isn't supported on this platform");
}

/// <param name="Title">The window title.</param>
/// <param name="Bounds">The window bounds in the platform's native units.</param>
/// <param name="Handle">The platform's native window object (e.g. HWND, NSWindow, Gtk.Window).</param>
public record TestScenarioWindow(string Title, Rectangle Bounds, object Handle);
#endif
