namespace NAPS2.Scan.Internal.Sane.Native;

/// <summary>
/// Indicates a custom build of SANE inside the Flatpak sandbox.
/// </summary>
internal class FlatpakSaneInstallation : ISaneInstallation
{
    public bool CanStreamDevices => true;

    public void Initialize()
    {
    }

    public void SetCustomConfigDir(string configDir)
    {
        // The trailing ":" means we'll search the default dirs if this dir doesn't exist
        PlatformCompat.System.SetEnv("SANE_CONFIG_DIR", $"{configDir}:");
    }

    public string DefaultConfigDir => "/app/etc/sane.d";

    public string LibraryPath => "libsane.so.1";

    public string[]? LibraryDeps => null;
}