using System.Runtime.InteropServices;
using NAPS2.Platform.Mac;

namespace NAPS2.Platform;

internal class MacSystemCompat : ISystemCompat
{
    private const int RTLD_LAZY = 1;
    private const int RTLD_GLOBAL = 8;

    public bool IsWiaDriverSupported => false;

    public bool IsTwainDriverSupported => false;

    public bool IsAppleDriverSupported => true;

    public bool IsSaneDriverSupported => true;

    public bool IsEsclDriverSupported => true;

    public bool SupportsTheme => false;

    public bool SupportsShowPageNumbers => false;

    public bool SupportsProfilesToolbar => false;

    public bool SupportsButtonActions => false;

    public bool SupportsKeyboardShortcuts => true;

    public bool SupportsSingleInstance => false;

    public bool CanUseWin32 => false;

    public bool CanEmail => true;

    public bool CanPrint => true;

    public bool CombinedPdfAndImageSaving => true;

    public bool ShouldRememberBackgroundOperations => true;

    public bool RenderInWorker => false;

    public bool SupportsWinX86Worker => false;

    public string? NativeWorkerAlias => null;

    public string? WinX86WorkerAlias => null;

    public string WorkerCrashMessage => SdkResources.WorkerCrash;

    public string[] ExeSearchPaths => LibrarySearchPaths;

    public string[] LibrarySearchPaths
    {
        get
        {
            var prefix = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "_macarm"
                : "_mac";
            return new[]
            {
                "",
                prefix,
                $"../Resources/{prefix}" // Path in .app bundle
            };
        }
    }

    public string TesseractExecutableName => "tesseract";

    public string PdfiumLibraryName => "libpdfium.dylib";

    public string[] SaneLibraryDeps => new[] { "libusb-1.0.0.dylib", "libjpeg.62.dylib" };

    public string SaneLibraryName => "libsane.1.dylib";

    public bool IsLibUsbReliable => false;

    public IntPtr LoadLibrary(string path) => MacInterop.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);

    public IntPtr LoadSymbol(IntPtr libraryHandle, string symbol) => MacInterop.dlsym(libraryHandle, symbol);

    public string GetLoadError() => MacInterop.dlerror();

    public void SetEnv(string name, string value) => MacInterop.setenv(name, value, 1);

    public IDisposable FileReadLock(string path) => new FileStream(path, FileMode.Open, FileAccess.Read);

    public IDisposable FileWriteLock(string path) => new FileStream(path, FileMode.Open, FileAccess.Write);
}