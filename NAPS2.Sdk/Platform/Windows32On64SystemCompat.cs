namespace NAPS2.Platform;

public class Windows32On64SystemCompat : Windows32SystemCompat
{
    // If we're running a 64-bit OS, we should prefer to run 64-bit exes and fall back to 32-bit if not found.
    public override string[] ExeSearchPaths => new[] { "_win64", "_win32" };
}