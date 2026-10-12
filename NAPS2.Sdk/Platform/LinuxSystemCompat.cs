#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
using NAPS2.Platform.Linux;

namespace NAPS2.Platform;

internal class LinuxSystemCompat : ISystemCompat
{
    private const int RTLD_LAZY = 1;
    private const int RTLD_GLOBAL = 8;

    public bool IsWiaDriverSupported => false;

    public bool IsTwainDriverSupported => false;

    public bool IsAppleDriverSupported => false;

    public bool IsSaneDriverSupported => true;

    public bool IsEsclDriverSupported => true;

    public bool SupportsTheme => true;

    public bool SupportsShowPageNumbers => false;

    public bool SupportsProfilesToolbar => true;

    public bool SupportsButtonActions => true;

    public bool SupportsKeyboardShortcuts => true;

    public bool SupportsSingleInstance => true;

    public bool CanUseWin32 => false;

    public bool CanEmail => true;

    public bool CanPrint => true;

    public bool CombinedPdfAndImageSaving => false;

    public bool ShouldRememberBackgroundOperations => true;

    public bool RenderInWorker => false;

    public bool SupportsWinX86Worker => false;

    public string? NativeWorkerAlias => null;

    public string? WinX86WorkerAlias => null;

    public string WorkerCrashMessage => SdkResources.WorkerCrash;

    public string[] ExeSearchPaths => LibrarySearchPaths;

    public string[] LibrarySearchPaths => new[]
    {
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "_linuxarm" : "_linux"
    };

    public string TesseractExecutableName => "tesseract";

    public string PdfiumLibraryName => "libpdfium.so";

    public string JpegLibraryName => "libnaps2_jpeg.so";

    public string[]? SaneLibraryDeps => null;

    public string SaneLibraryName => "libsane.so.1";

    public bool IsLibUsbReliable => true;

    // An empty locale name means the user's locale from the environment (LC_ALL/LC_PAPER/LANG)
    public PageSize? SystemDefaultPageSize => GetPaperSize("");

    /// <summary>
    /// Gets the paper size for the given locale (e.g. "en_US.UTF-8"), or null if the locale isn't available.
    /// </summary>
    internal static PageSize? GetPaperSize(string localeName)
    {
        const int LC_PAPER = 7;
        const int LC_PAPER_MASK = 1 << LC_PAPER;
        const int _NL_PAPER_HEIGHT = LC_PAPER << 16;
        const int _NL_PAPER_WIDTH = (LC_PAPER << 16) | 1;
        try
        {
            // Use a separate locale object rather than setlocale so we don't change the process-wide locale
            var locale = LinuxInterop.newlocale(LC_PAPER_MASK, localeName, IntPtr.Zero);
            if (locale == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                // For these items, glibc returns the value in mm in place of a string pointer. The value is a
                // 32-bit int in a union with the pointer, so the upper bits can be garbage and must be discarded.
                int height = unchecked((int) LinuxInterop.nl_langinfo_l(_NL_PAPER_HEIGHT, locale));
                int width = unchecked((int) LinuxInterop.nl_langinfo_l(_NL_PAPER_WIDTH, locale));
                if (width is > 0 and < 10000 && height is > 0 and < 10000)
                {
                    return PageSize.FindWellKnownSize(width, height, PageSizeUnit.Millimetre);
                }
            }
            finally
            {
                LinuxInterop.freelocale(locale);
            }
        }
        catch (Exception)
        {
            // Ignore
        }
        return null;
    }

    public IntPtr LoadLibrary(string path) => LinuxInterop.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);

    public IntPtr LoadSymbol(IntPtr libraryHandle, string symbol) => LinuxInterop.dlsym(libraryHandle, symbol);

    public string GetLoadError() => LinuxInterop.dlerror();

    public void SetEnv(string name, string value) => LinuxInterop.setenv(name, value, 1);

    public IDisposable FileReadLock(string path) => new FileStream(path, FileMode.Open, FileAccess.Read);

    public IDisposable FileWriteLock(string path) => new FileStream(path, FileMode.Open, FileAccess.Write);
}
#endif
