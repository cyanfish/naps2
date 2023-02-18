// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

/// <summary>
/// A common signature for progress reporting.
/// </summary>
/// <param name="current">The number of completed items.</param>
/// <param name="max">The total number of items.</param>
public delegate void ProgressCallback(int current, int max);