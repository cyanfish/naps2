using NAPS2.Platform.Windows;

namespace NAPS2.Platform;

public class Windows64SystemCompat : WindowsSystemCompat
{
    public override string TesseractExecutablePath => "_win64/tesseract.exe";
    
    public override string PdfiumLibraryPath => "_win64/pdfium.dll";
    
    public override IntPtr LoadSymbol(IntPtr libraryHandle, string symbol)
    {
        return Win32.GetProcAddress(libraryHandle, symbol);
    }
}