using System.Runtime.CompilerServices;
using Xunit;

namespace NAPS2.Sdk.Tests;

public class NetworkFactAttribute : FactAttribute
{
    public NetworkFactAttribute(
        [CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (Environment.GetEnvironmentVariable("NAPS2_TEST_NONETWORK") == "1")
        {
            Skip = "Running without network access, skipping ESCL tests";
        }
    }
}