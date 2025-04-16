namespace NAPS2.Platform;

internal class WindowsArm64SystemCompat : WindowsSystemCompat
{
    // ARM64 Windows does have a 32-bit twain_32.dll, but that seems fairly nonsensical as 32-bit drivers won't work.
    // I think the point is to be a compatibility layer for x86 applications that use TWAIN instead of WIA.
    // But there's not really any point in supporting this in NAPS2, though I might reconsider upon request.
    public override bool IsTwainDriverSupported => false;

    public override string[] ExeSearchPaths => new[] { "_winarm" };

    public override string[] LibrarySearchPaths => new[] { "_winarm" };

    public override bool SupportsWinX86Worker => false;
}