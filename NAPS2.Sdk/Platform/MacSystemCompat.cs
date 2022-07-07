using NAPS2.Platform.Mac;

namespace NAPS2.Platform;

public class MacSystemCompat : ISystemCompat
{
    private const int RTLD_LAZY = 1;
    private const int RTLD_GLOBAL = 8;

    public bool IsWiaDriverSupported => false;

    public bool IsWia20Supported => false;

    public bool IsTwainDriverSupported => true;

    public bool IsSaneDriverSupported => false;

    public bool CanUseWin32 => false;

    public bool UseUnixFontResolver => true;

    public bool UseSystemTesseract => true;

    public string? TesseractExecutablePath => null;

    public string PdfiumLibraryPath => "_mac/libpdfium.dylib";

    public IntPtr LoadLibrary(string path) => MacInterop.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);

    public IntPtr LoadSymbol(IntPtr libraryHandle, string symbol) => MacInterop.dlsym(libraryHandle, symbol);
}