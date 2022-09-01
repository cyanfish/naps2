using NAPS2.Platform.Windows;

namespace NAPS2.Platform;

public class Windows64SystemCompat : WindowsSystemCompat
{
    public override string[] LibrarySearchPaths => new[] { "_win64" };
    
    public override IntPtr LoadSymbol(IntPtr libraryHandle, string symbol)
    {
        return Win32.GetProcAddress(libraryHandle, symbol);
    }
}