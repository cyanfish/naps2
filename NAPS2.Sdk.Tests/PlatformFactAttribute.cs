using System.Runtime.InteropServices;
using Xunit;

namespace NAPS2.Sdk.Tests;

public sealed class PlatformFactAttribute : FactAttribute
{
    public PlatformFactAttribute(Platform include = Platform.None, Platform exclude = Platform.None)
    {
        if (include != Platform.None && (GetPlatform() & include) != include)
        {
            Skip = $"Only runs on platform: {include}";
        }
        if (exclude != Platform.None && (GetPlatform() & exclude) != Platform.None)
        {
            Skip = $"Doesn't runs on platform: {exclude}";
        }
    }

    private Platform GetPlatform()
    {
        var p = Platform.None;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            p |= Platform.Windows;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            p |= Platform.Mac;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            p |= Platform.Linux;
        }
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            p |= Platform.X64;
        }
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            p |= Platform.Arm64;
        }
        return p;
    }
}