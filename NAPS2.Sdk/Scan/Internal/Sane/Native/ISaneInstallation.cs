namespace NAPS2.Scan.Internal.Sane.Native;

public interface ISaneInstallation
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
    /// Gets the path to the libsane library.
    /// </summary>
    string LibraryPath { get; }

    /// <summary>
    /// Gets the paths to any libraries that need to be manually loaded before loading the libsane library.
    /// </summary>
    string[]? LibraryDeps { get; }
}