using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NAPS2.Testing;

/// <summary>
/// Provides facilities to detect unit testing and throw exceptions for bad testing practices.
/// </summary>
public static class TestingContext
{
    private static readonly Regex TestingAssemblyRegex = new Regex("NUnit|XUnit", RegexOptions.IgnoreCase);

    private static readonly Lazy<bool> Testing = new Lazy<bool>(() => AppDomain.CurrentDomain.GetAssemblies().Any(x => TestingAssemblyRegex.IsMatch(x.FullName)));

    public static bool IsTesting => Testing.Value;

    /// <summary>
    /// If testing, throws an exception.
    /// </summary>
    public static void NoStaticDefaults()
    {
        if (IsTesting)
        {
            throw new InvalidOperationException("Static defaults should not be used for unit tests. Use constructor injection instead.");
        }
    }
}