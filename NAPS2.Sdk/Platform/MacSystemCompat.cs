using System.Runtime.InteropServices;
using NAPS2.Platform.Mac;

namespace NAPS2.Platform;

public class MacSystemCompat : ISystemCompat
{
    private const int RTLD_LAZY = 1;
    private const int RTLD_GLOBAL = 8;

    public bool IsWiaDriverSupported => false;

    public bool IsWia20Supported => false;

    public bool IsTwainDriverSupported => false;

    public bool IsAppleDriverSupported => true;

    public bool IsSaneDriverSupported => true;

    public bool CanUseWin32 => false;

    public bool UseSystemTesseract => false;

    public bool RenderInWorker => false;

    public string[] LibrarySearchPaths {
        get
        {
            var prefix = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "_macarm"
                : "_mac";
            return new[]
            {
                prefix,
                $"../Resources/{prefix}" // Path in .app bundle
            };
        }
    }

    public string TesseractExecutableName => "tesseract";

    public string PdfiumLibraryName => "libpdfium.dylib";

    public string SaneLibraryName => "libsane.dylib";

    public IntPtr LoadLibrary(string path) => MacInterop.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);

    public IntPtr LoadSymbol(IntPtr libraryHandle, string symbol) => MacInterop.dlsym(libraryHandle, symbol);

    public IDisposable FileReadLock(string path) => new FileStream(path, FileMode.Open, FileAccess.Read);

    public IDisposable FileWriteLock(string path) => new FileStream(path, FileMode.Open, FileAccess.Write);
}