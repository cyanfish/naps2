using System.Runtime.InteropServices;
using System.Text;
using NAPS2.Platform.Windows;

namespace NAPS2.Platform;

internal abstract class WindowsSystemCompat : ISystemCompat
{
    public bool IsWiaDriverSupported => true;

    public bool IsTwainDriverSupported => true;

    public bool IsAppleDriverSupported => false;

    public bool IsSaneDriverSupported => false;

    public bool IsEsclDriverSupported => true;

    public bool SupportsShowPageNumbers => true;

    public bool SupportsProfilesToolbar => true;

    public bool SupportsButtonActions => true;

    public bool SupportsKeyboardShortcuts => true;

    public bool SupportsSingleInstance => true;

    public bool CanUseWin32 => true;

    public bool CanEmail => true;

    public bool CanPrint => true;

    public bool CombinedPdfAndImageSaving => false;

    public bool ShouldRememberBackgroundOperations => true;

    public bool RenderInWorker => true;

    public bool SupportsWinX86Worker => true;

    // Due to weird MSIX permission issues, we need to run worker processes using their aliases so that Windows sets
    // them up with "package identity" which will allow them to load their DLLs.
    public string? NativeWorkerAlias => IsRunningAsMsix ? "NAPS2_Alias.exe" : null;

    public string? WinX86WorkerAlias => IsRunningAsMsix ? "NAPS2.Worker_Alias.exe" : null;

    public string WorkerCrashMessage => SdkResources.WorkerCrashWindows;

    public abstract string[] ExeSearchPaths { get; }

    public abstract string[] LibrarySearchPaths { get; }

    public string TesseractExecutableName => "tesseract.exe";

    public string PdfiumLibraryName => "pdfium.dll";

    public string[]? SaneLibraryDeps => null;

    public string SaneLibraryName => "sane.dll";

    public bool IsLibUsbReliable => true;

    public IntPtr LoadLibrary(string path) => Win32.LoadLibrary(path);

    public string GetLoadError() => Marshal.GetLastWin32Error().ToString();

    public abstract IntPtr LoadSymbol(IntPtr libraryHandle, string symbol);

    public void SetEnv(string name, string value) => throw new NotSupportedException();

    public IDisposable? FileReadLock(string path) => null;

    public IDisposable? FileWriteLock(string path) => null;

    private const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

    private bool IsRunningAsMsix
    {
        get
        {
#if NET6_0_OR_GREATER
            if (OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                int length = 0;
                var sb = new StringBuilder(0);
                Win32.GetCurrentPackageFullName(ref length, sb);
                sb = new StringBuilder(length);
                int result = Win32.GetCurrentPackageFullName(ref length, sb);
                return result != APPMODEL_ERROR_NO_PACKAGE;
            }
#endif
            return false;
        }
    }
}