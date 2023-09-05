namespace NAPS2.Escl;

public enum EsclAdfState
{
    Unknown,
    ScannerAdfProcessing,
    ScannerAdfEmpty,
    ScannerAdfJam,
    ScannedAdfLoaded,
    ScannerAdfMispick,
    ScannerAdfHatchOpen,
    ScannerAdfDuplexPageTooShort,
    ScannerAdfDuplexPageTooLong,
    ScannerAdfMultipickDetected,
    ScannerAdfInputTrayFailed,
    ScannerAdfInputTrayOverloaded
}