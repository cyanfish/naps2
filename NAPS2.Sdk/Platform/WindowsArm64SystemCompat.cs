namespace NAPS2.Platform;

internal class WindowsArm64SystemCompat : WindowsSystemCompat
{
    public override string[] ExeSearchPaths => new[] { "_winarm" };

    public override string[] LibrarySearchPaths => new[] { "_winarm" };
}