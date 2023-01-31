namespace NAPS2.Scan.Internal.Sane.Native;

#if NET6_0_OR_GREATER
public class SystemSaneInstallation : ISaneInstallation
{
    private string? _libraryPath;

    public void Initialize()
    {
        _libraryPath = OperatingSystem.IsMacOS()
            ? "libsane.1.dylib"
            : "libsane.so.1";
    }

    public string LibraryPath => _libraryPath ?? throw new InvalidOperationException();
    public string[]? LibraryDeps => null;
}
#endif