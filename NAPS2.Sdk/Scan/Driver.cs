namespace NAPS2.Scan;

/// <summary>
/// Specifies the driver type to be used for performing the actual scan. Available options depend on the platform.
/// <br/>
/// See https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk#drivers for more details.
/// </summary>
public enum Driver
{
    /// <summary>
    /// Use the default driver for the platform (Windows -> Wia, Mac -> Apple, Linux -> Sane).
    /// </summary>
    Default,

    /// <summary>
    /// Use a WIA driver (Windows-only).
    /// </summary>
    Wia,

    /// <summary>
    /// Use a TWAIN driver (Windows-only). Mac can use TWAIN indirectly through the Apple driver type.
    /// </summary>
    Twain,

    /// <summary>
    /// Use an Apple ImageCaptureCore driver (Mac-only). You will also need to compile against a macOS framework target
    /// (e.g net8-macos) to use this driver type.
    /// </summary>
    Apple,

    /// <summary>
    /// Use a SANE driver (Linux and Mac). To use on Mac you'll also need to reference the NAPS2.Sane.Binaries package.
    /// </summary>
    Sane,

    /// <summary>
    /// Use an ESCL network driver (all platforms).
    /// </summary>
    Escl
}