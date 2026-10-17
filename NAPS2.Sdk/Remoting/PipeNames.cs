using System.Text;

namespace NAPS2.Remoting;

internal static class PipeNames
{
    // macOS limits Unix socket paths to 104 bytes (including the null terminator); Linux is 108
    private const int MAX_SOCKET_PATH_BYTES = 104;

    public static string Get(string name)
    {
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            return name;
        }
        // Duplicate the .NET-internal logic for constructing an absolute pipe path
        var defaultPath = Path.Combine(Path.GetTempPath(), "CoreFxPipe_") + name;
        if (Encoding.UTF8.GetByteCount(defaultPath) >= MAX_SOCKET_PATH_BYTES)
        {
            // If $TMPDIR is too long, use an absolute path in /tmp instead.
            // This is the same path .NET uses by default on Linux when $TMPDIR isn't set.
            return "/tmp/CoreFxPipe_" + name;
        }
#endif
        return name;
    }
}
