namespace NAPS2.TestScenarios;

/// <summary>
/// Flags that test scenarios can set to change form behavior, e.g. to show UI that's otherwise platform-specific.
/// These are always false in builds without test scenarios.
/// </summary>
internal static class TestScenarioFlags
{
#if TEST_SCENARIOS
    /// <summary>
    /// Show the options for all drivers, not just the ones supported on the current platform.
    /// </summary>
    public static bool ShowAllDrivers { get; set; }

    /// <summary>
    /// Show the help link for missing devices in the Flatpak.
    /// </summary>
    public static bool ShowFlatpakLink { get; set; }

    /// <summary>
    /// Forms that start work on load (authorization, downloads) don't start it, so their initial state can be shown.
    /// </summary>
    public static bool SkipFormWork { get; set; }
#else
    public static bool ShowAllDrivers => false;
    public static bool ShowFlatpakLink => false;
    public static bool SkipFormWork => false;
#endif
}
