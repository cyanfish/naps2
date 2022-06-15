using NAPS2.Ocr;

namespace NAPS2.Scan;

public interface IOcrEngineProvider
{
    public IOcrEngine? ActiveEngine { get; }
}