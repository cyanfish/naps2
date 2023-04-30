namespace NAPS2.Scan.Internal.Sane.Native;

internal class FlatpakSaneInstallation : ISaneInstallation
{
    public bool CanStreamDevices => true;

    public void Initialize()
    {
    }

    public string LibraryPath => "libsane.so.1";

    public string[]? LibraryDeps => null;
}