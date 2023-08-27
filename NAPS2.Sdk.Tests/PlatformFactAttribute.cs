using System.Runtime.InteropServices;
using Xunit;

namespace NAPS2.Sdk.Tests;

public sealed class PlatformFactAttribute : FactAttribute
{
    public PlatformFactAttribute(PlatformFlags include = PlatformFlags.None, PlatformFlags exclude = PlatformFlags.None)
    {
        if (include != PlatformFlags.None && (CurrentPlatformFlags.Get() & include) != include)
        {
            Skip = $"Only runs on platform(s): {include}";
        }
        if (exclude != PlatformFlags.None && (CurrentPlatformFlags.Get() & exclude) != PlatformFlags.None)
        {
            Skip = $"Doesn't run on platform(s): {exclude}";
        }
    }
}