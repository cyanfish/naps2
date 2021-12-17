using System;
using System.Diagnostics;
using System.Threading;

namespace NAPS2.Testing;

/// <summary>
/// A debug helper that randomly causes delays or errors.
/// </summary>
internal static class ChaosMonkey
{
    private static Lazy<Random> _random = new Lazy<Random>();

    [Conditional("DEBUG")] 
    public static void MaybeError(double chance, Exception? exception = null)
    {
        if (_random.Value.NextDouble() < chance)
        {
            throw exception ?? new Exception("Randomly generated exception for testing");
        }
    }

    [Conditional("DEBUG")] 
    public static void MaybeDelay(double chance, double durationInSeconds, double variationInSeconds = 0)
    {
        if (_random.Value.NextDouble() < chance)
        {
            double duration = durationInSeconds + variationInSeconds * (_random.Value.NextDouble() * 2 - 1);
            Thread.Sleep(TimeSpan.FromSeconds(duration));
        }
    }
}