namespace NAPS2.Escl;

public class EsclScanSettings
{
    public int Width { get; init; }

    public int Height { get; init; }

    public int XOffset { get; init; }

    public int YOffset { get; init; }

    public string? DocumentFormat { get; init; }

    public EsclInputSource InputSource { get; init; }

    public int XResolution { get; init; }

    public int YResolution { get; init; }

    public EsclColorMode ColorMode { get; init; }

    public bool Duplex { get; init; }

    public int Brightness { get; init; }

    public int Contrast { get; init; }

    public int Threshold { get; init; }
}