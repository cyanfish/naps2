namespace NAPS2.Platform;

public interface ISystemCompat
{
    bool IsWiaDriverSupported { get; }

    bool IsTwainDriverSupported { get; }

    bool IsSaneDriverSupported { get; }

    bool CanUseWin32 { get; }

    bool IsWia20Supported { get; }

    bool UseSystemTesseract { get; }

    string? TesseractExecutablePath { get; }

    string PdfiumLibraryPath { get; }

    IntPtr LoadLibrary(string path);

    IntPtr LoadSymbol(IntPtr libraryHandle, string symbol);

    IDisposable? FileReadLock(string path);

    IDisposable? FileWriteLock(string path);
}