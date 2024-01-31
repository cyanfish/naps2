namespace NAPS2.Scan.Internal.Sane.Native;

internal interface ISaneInstallation
{
    /// <summary>
    /// Whether the SANE installation has a patch to add the sane_stream_devices method.
    /// </summary>
    bool CanStreamDevices { get; }

    /// <summary>
    /// Performs any initialization work needed to use the SANE installation (e.g. generating config files).
    /// </summary>
    void Initialize();

    /// <summary>
    /// Sets the path to a custom sane.d config directory.
    /// </summary>
    void SetCustomConfigDir(string configDir);

    /// <summary>
    /// Gets the path to the default sane.d config directory.
    /// </summary>
    string DefaultConfigDir { get; }

    /// <summary>
    /// Gets the path to the libsane library.
    /// </summary>
    string LibraryPath { get; }

    /// <summary>
    /// Gets the paths to any libraries that need to be manually loaded before loading the libsane library.
    /// </summary>
    string[]? LibraryDeps { get; }
}