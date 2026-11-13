using System.Runtime.CompilerServices;
using System.Threading;

file static class MacTestModuleInitializer
{
    /// <summary>
    /// On macOS, an AppKitSynchronizationContext is installed on the main thread. xUnit.net's generated entry point
    /// blocks the main thread waiting for the async test run without running the AppKit loop, so continuations posted to
    /// that context would deadlock.
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize() => SynchronizationContext.SetSynchronizationContext(null);
}
