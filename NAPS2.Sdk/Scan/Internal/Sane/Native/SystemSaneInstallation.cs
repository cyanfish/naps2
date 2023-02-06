namespace NAPS2.Scan.Internal.Sane.Native;

public class SystemSaneInstallation : ISaneInstallation
{
    private string? _libraryPath;

    public void Initialize()
    {
        _libraryPath = Environment.OSVersion.Platform == PlatformID.MacOSX
            ? "libsane.1.dylib"
            : "libsane.so.1";
    }

    public string LibraryPath => _libraryPath ?? throw new InvalidOperationException();
    public string[]? LibraryDeps => null;
}