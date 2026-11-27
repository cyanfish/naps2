using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NAPS2.EtoForms.Gtk;

/// <summary>
/// Runs the static constructors of the GtkSharp classes used to show the main window on background threads.
/// <para>
/// Each GtkSharp class loads a delegate for every native function it binds in its static constructor, and each of
/// those needs a marshalling stub to be JIT-compiled. That's around 2000 stubs for the main window, which would
/// otherwise be compiled on the UI thread.
/// </para>
/// </summary>
internal static class GtkSharpPreloader
{
    private static readonly Type?[] Types =
    [
        // Ordered roughly by size so the threads finish around the same time
        typeof(Cairo.Context).Assembly.GetType("Cairo.NativeMethods"), typeof(global::Gtk.Widget),
        typeof(Gdk.Window), typeof(global::Gtk.Window), typeof(Gdk.Pixbuf), typeof(Pango.Layout),
        typeof(global::Gtk.StyleContext), typeof(global::Gtk.ComboBox), typeof(global::Gtk.Label),
        typeof(GLib.Application), typeof(GLib.Value), typeof(global::Gtk.Button), typeof(global::Gtk.Drag),
        typeof(global::Gtk.ScrolledWindow), typeof(global::Gtk.FlowBox), typeof(global::Gtk.ToolItem),
        typeof(global::Gtk.Container), typeof(global::Gtk.Menu), typeof(Gdk.Screen), typeof(GLib.Source),
        typeof(global::Gtk.MenuItem), typeof(global::Gtk.Application), typeof(global::Gtk.Image),
        typeof(global::Gtk.TreeModelAdapter), typeof(global::Gtk.Toolbar), typeof(Atk.Object),
        typeof(global::Gtk.IconSource), typeof(global::Gtk.ToolButton), typeof(GLib.ParamSpec),
        typeof(global::Gtk.TreePath), typeof(GLib.Object), typeof(global::Gtk.Box), typeof(global::Gtk.MenuShell),
        typeof(global::Gtk.Paned), typeof(GLib.Cancellable), typeof(global::Gtk.AccelGroup), typeof(Gdk.PixbufLoader),
        typeof(Gdk.CairoHelper), typeof(GLib.GType), typeof(global::Gtk.ImageMenuItem), typeof(global::Gtk.IconSet),
        typeof(global::Gtk.Settings), typeof(global::Gtk.CssProvider), typeof(GLib.Signal), typeof(GLib.ValueArray),
        typeof(global::Gtk.SizeGroup), typeof(global::Gtk.AccelLabel), typeof(GLib.Marshaller),
        typeof(global::Gtk.Frame), typeof(global::Gtk.MenuToolButton), typeof(Gdk.Threads), typeof(GLib.List),
        typeof(global::Gtk.EventBox), typeof(global::Gtk.Overlay), typeof(GLib.Global), typeof(global::Gtk.Alignment),
        typeof(global::Gtk.Fixed), typeof(global::Gtk.SeparatorToolItem), typeof(GLib.Timeout), typeof(global::Gtk.Bin),
        typeof(global::Gtk.CellRendererText), typeof(global::Gtk.CellRendererPixbuf), typeof(global::Gtk.DrawingArea),
        typeof(global::Gtk.SeparatorMenuItem), typeof(global::Gtk.TreeIter)
    ];

    private static readonly string[] Libraries =
        ["GLib", "GObject", "Gio", "Gtk", "Gdk", "GdkPixbuf", "Atk", "Pango", "PangoCairo", "Cairo"];

    private static Task? _task;

    /// <summary>
    /// Starts preloading. Nothing else should use GtkSharp until Wait is called, so the static constructors can't
    /// contend with the main thread.
    /// </summary>
    public static void Start()
    {
        _task = Task.Run(Preload);
    }

    public static void Wait()
    {
        _task?.Wait();
    }

    private static void Preload()
    {
        try
        {
            // Each GtkSharp assembly caches its loaded native libraries in a non-thread-safe dictionary, so populate
            // those caches before running the static constructors in parallel
            LoadLibraries();
            Parallel.ForEach(Partitioner.Create(Types.OfType<Type>(), EnumerablePartitionerOptions.NoBuffering),
                RunClassConstructor);
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error preloading GtkSharp", ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "GtkSharp assemblies aren't trimmed")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "GtkSharp assemblies aren't trimmed")]
    private static void LoadLibraries()
    {
        foreach (var assembly in Types.OfType<Type>().Select(type => type.Assembly).Distinct())
        {
            var libraryEnum = assembly.GetType("Library", throwOnError: true)!;
            var isSupported = assembly.GetType("GLibrary", throwOnError: true)!
                                  .GetMethod("IsSupported", BindingFlags.Public | BindingFlags.Static)
                              ?? throw new MissingMethodException("GLibrary", "IsSupported");
            foreach (var library in Libraries)
            {
                isSupported.Invoke(null, [Enum.Parse(libraryEnum, library)]);
            }
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2059", Justification = "GtkSharp assemblies aren't trimmed")]
    private static void RunClassConstructor(Type type)
    {
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);
    }
}
