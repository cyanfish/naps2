using NSubstitute;
using Xunit;

namespace NAPS2.Sdk.Tests;

public static class TestExtensions
{
    public static void ReceivedCallsCount<T>(this T substitute, int count) where T : class
    {
        Assert.Equal(count, substitute.ReceivedCalls().Count());
    }
}