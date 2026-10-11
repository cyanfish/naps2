namespace NAPS2.Platform;

internal interface ISystemCompat
{
    bool IsWiaDriverSupported { get; }

    bool IsTwainDriverSupported { get; }

    bool IsAppleDriverSupported { get; }

    bool IsSaneDriverSupported { get; }

    bool IsEsclDriverSupported { get; }

    bool SupportsTheme { get; }
    
    bool SupportsShowPageNumbers { get; }
    
    bool SupportsProfilesToolbar { get; }
    
    bool SupportsButtonActions { get; }

    bool SupportsKeyboardShortcuts { get; }
    
    bool SupportsSingleInstance { get; }

    bool CanUseWin32 { get; }

    bool CanEmail { get; }

    bool CanPrint { get; }

    bool CombinedPdfAndImageSaving { get; }

    bool ShouldRememberBackgroundOperations { get; }

    bool RenderInWorker { get; }

    bool SupportsWinX86Worker { get; }

    string? NativeWorkerAlias { get; }

    string? WinX86WorkerAlias { get; }

    string WorkerCrashMessage { get; }

    string[] ExeSearchPaths { get; }

    string[] LibrarySearchPaths { get; }

    string TesseractExecutableName { get; }

    string PdfiumLibraryName { get; }

    string JpegLibraryName { get; }

    string[]? SaneLibraryDeps { get; }

    string SaneLibraryName { get; }

    bool IsLibUsbReliable { get; }

    /// <summary>
    /// The default paper size for the user's system (e.g. Letter in the US, A4 in most other places), or null if it
    /// can't be determined or isn't a well-known size.
    /// </summary>
    PageSize? SystemDefaultPageSize { get; }

    IntPtr LoadLibrary(string path);

    IntPtr LoadSymbol(IntPtr libraryHandle, string symbol);

    string GetLoadError();

    IDisposable? FileReadLock(string path);

    IDisposable? FileWriteLock(string path);

    void SetEnv(string name, string value);
}