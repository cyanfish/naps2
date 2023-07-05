using System.Runtime.InteropServices;
using Xunit;

namespace NAPS2.Sdk.Tests;

public sealed class PlatformFactAttribute : FactAttribute
{
    public PlatformFactAttribute(PlatformFlags include = PlatformFlags.None, PlatformFlags exclude = PlatformFlags.None)
    {
        if (include != PlatformFlags.None && (GetPlatform() & include) != include)
        {
            Skip = $"Only runs on platform(s): {include}";
        }
        if (exclude != PlatformFlags.None && (GetPlatform() & exclude) != PlatformFlags.None)
        {
            Skip = $"Doesn't run on platform(s): {exclude}";
        }
    }

    private PlatformFlags GetPlatform()
    {
        var p = PlatformFlags.None;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            p |= PlatformFlags.Windows;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            p |= PlatformFlags.Mac;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            p |= PlatformFlags.Linux;
        }
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            p |= PlatformFlags.X64;
        }
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            p |= PlatformFlags.Arm64;
        }
        return p;
    }
}