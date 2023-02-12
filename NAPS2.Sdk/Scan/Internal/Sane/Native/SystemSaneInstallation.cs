namespace NAPS2.Scan.Internal.Sane.Native;

public class SystemSaneInstallation : ISaneInstallation
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

    public string LibraryPath => _libraryPath ?? throw new InvalidOperationException();
    public string[]? LibraryDeps => null;
}