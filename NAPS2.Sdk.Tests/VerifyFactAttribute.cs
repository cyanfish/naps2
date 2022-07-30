using Xunit;

namespace NAPS2.Sdk.Tests;

public sealed class PlatformFactAttribute : FactAttribute
{
    public PlatformFactAttribute(bool require64Bit = false)
    {
        if (require64Bit && !Environment.Is64BitProcess)
        {
            Skip = "This test only runs in 64 bit processes.";
        }
    }
}