namespace NAPS2.Scan.Internal.Sane.Native;

/// <summary>
/// Indicates the system-provided SANE build.
/// </summary>
internal class SystemSaneInstallation : ISaneInstallation
{
    private string? _libraryPath;

    public bool CanStreamDevices => false;

    public void Initialize()
    {
#if NET6_0_OR_GREATER
        _libraryPath = OperatingSystem.IsMacOS()
            ? "libsane.1.dylib"
            : "libsane.so.1";
#else
        _libraryPath = null;
#endif
    }

    public void SetCustomConfigDir(string? configDir)
    {
        // The trailing ":" means we'll search the default dirs if this dir doesn't exist
        PlatformCompat.System.SetEnv("SANE_CONFIG_DIR", $"{configDir}:");
    }

    // There may be some distros where this is incorrect, but if the path doesn't exist we just disable optimizations
    public string DefaultConfigDir => "/etc/sane.d";

    public string LibraryPath => _libraryPath ?? throw new InvalidOperationException();

    public string[]? LibraryDeps => null;
}