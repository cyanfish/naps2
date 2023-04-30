using NAPS2.Platform.Windows;

namespace NAPS2.Platform;

internal class Windows32SystemCompat : WindowsSystemCompat
{
    public override string[] ExeSearchPaths => new[] { "_win32" };

    public override string[] LibrarySearchPaths => new[] { "_win32" };
    
    public override IntPtr LoadSymbol(IntPtr libraryHandle, string symbol)
    {
        var address = Win32.GetProcAddress(libraryHandle, symbol);
        if (address != IntPtr.Zero)
        {
            return address;
        }
        // Names can be mangled in 32-bit
        for (int i = 0; i < 128; i += 4)
        {
            address = Win32.GetProcAddress(libraryHandle, $"_{symbol}@{i}");
            if (address != IntPtr.Zero)
            {
                return address;
            }
        }
        return IntPtr.Zero;
    }
}