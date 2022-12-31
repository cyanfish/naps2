namespace NAPS2.Platform;

public interface ISystemCompat
{
    bool IsWiaDriverSupported { get; }

    bool IsTwainDriverSupported { get; }

    bool IsAppleDriverSupported { get; }

    bool IsSaneDriverSupported { get; }

    bool CanUseWin32 { get; }

    // TODO: Implement Print/Email on Mac/Linux
    bool CanEmail { get; }

    bool CanPrint { get; }

    // TODO: Implement background progress notifications on Mac/Linux
    bool ShouldRememberBackgroundOperations { get; }

    bool UseSystemTesseract { get; }

    bool RenderInWorker { get; }

    bool SupportsWinX86Worker { get; }

    string[] ExeSearchPaths { get; }

    string[] LibrarySearchPaths { get; }

    string TesseractExecutableName { get; }

    string PdfiumLibraryName { get; }

    string[]? SaneLibraryDeps { get; }

    string SaneLibraryName { get; }

    IntPtr LoadLibrary(string path);

    IntPtr LoadSymbol(IntPtr libraryHandle, string symbol);

    string GetLoadError();

    IDisposable? FileReadLock(string path);

    IDisposable? FileWriteLock(string path);

    void SetEnv(string name, string value);
}