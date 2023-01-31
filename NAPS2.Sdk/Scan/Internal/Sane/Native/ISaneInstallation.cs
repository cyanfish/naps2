namespace NAPS2.Scan.Internal.Sane.Native;

public interface ISaneInstallation
{
    void Initialize();

    string LibraryPath { get; }

    string[]? LibraryDeps { get; }
}