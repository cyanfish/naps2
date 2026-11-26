#if LINUX
using System.Runtime.CompilerServices;
using System.Threading;

namespace NAPS2.Sdk.Tests;

internal static class GlibMainLoop
{
    // GtkSharp finalizers queue unrefs to the GLib main loop, so without one, undisposed images leak native memory.
    [ModuleInitializer]
    internal static void Start()
    {
        new Thread(() => new GLib.MainLoop().Run()) { IsBackground = true, Name = "GLib main loop" }.Start();
    }
}
#endif
