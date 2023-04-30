using NAPS2.Platform.Windows;

namespace NAPS2.Platform;

internal class Windows64SystemCompat : WindowsSystemCompat
{
    // If we're running a 64-bit OS, we should prefer to run 64-bit exes and fall back to 32-bit if not found.
    public override string[] ExeSearchPaths => new[] { "_win64", "_win32" };

    public override string[] LibrarySearchPaths => new[] { "_win64" };
    
    public override IntPtr LoadSymbol(IntPtr libraryHandle, string symbol)
    {
        return Win32.GetProcAddress(libraryHandle, symbol);
    }
}