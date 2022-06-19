using NAPS2.Platform.Windows;

namespace NAPS2.Platform;

public class Windows32SystemCompat : WindowsSystemCompat
{
    public override string TesseractExecutablePath => "_win32/tesseract.exe";
    
    public override string PdfiumLibraryPath => "_win32/pdfium.dll";
    
    public override IntPtr LoadSymbol(IntPtr libraryHandle, string symbol)
    {
        // Names can be mangled in 32-bit
        for (int i = 0; i < 128; i += 4)
        {
            var address = Win32.GetProcAddress(libraryHandle, $"_{symbol}@{i}");
            if (address != IntPtr.Zero)
            {
                return address;
            }
        }
        return IntPtr.Zero;
    }
}