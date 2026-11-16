#if TEST_SCENARIOS
using System.Runtime.InteropServices;
using Eto.Drawing;

namespace NAPS2.TestScenarios;

/// <summary>
/// Window access for GTK. Captures render the window's widgets offscreen, so they work on X11 and Wayland but don't
/// include server-side window decorations.
/// </summary>
public class GtkTestScenarioPlatform : ITestScenarioPlatform
{
    public List<TestScenarioWindow> GetWindows()
    {
        return Gtk.Window.ListToplevels()
            .Where(w => w.Visible && w.IsMapped)
            // GTK doesn't expose the stacking order on all backends, so assume modal/transient windows (dialogs) are on
            // top. Focus alone isn't reliable as it changes asynchronously when windows open and close.
            .OrderByDescending(w => w.Modal)
            .ThenByDescending(GetTransientDepth)
            .ThenByDescending(w => w.IsActive)
            .Select(w =>
            {
                w.GetPosition(out var x, out var y);
                return new TestScenarioWindow(w.Title ?? "",
                    new Rectangle(x, y, w.Allocation.Width, w.Allocation.Height), w);
            })
            .ToList();
    }

    private static int GetTransientDepth(Gtk.Window window)
    {
        int depth = 0;
        for (var parent = window.TransientFor; parent != null; parent = parent.TransientFor)
        {
            depth++;
        }
        return depth;
    }

    public void Capture(TestScenarioWindow window, string path)
    {
        var gtkWindow = (Gtk.Window) window.Handle;
        int scale = gtkWindow.ScaleFactor;
        var allocation = gtkWindow.Allocation;
        using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, allocation.Width * scale,
            allocation.Height * scale);
        cairo_surface_set_device_scale(surface.Handle, scale, scale);
        using (var context = new Cairo.Context(surface))
        {
            gtkWindow.Draw(context);
        }
        surface.WriteToPng(path);
    }

    [DllImport("libcairo.so.2")]
    private static extern void cairo_surface_set_device_scale(IntPtr surface, double xScale, double yScale);
}
#endif
