namespace NAPS2.Escl;

public class EsclCapabilities
{
    public const string DEFAULT_VERSION = "2.6";

    public string Version { get; init; } = DEFAULT_VERSION;
    public string? MakeAndModel { get; init; }
    public string? SerialNumber { get; init; }
    public string? Uuid { get; init; }
    public string? AdminUri { get; init; }
    public string? IconUri { get; init; }
    public byte[]? IconPng { get; init; }
    public string? Naps2Extensions { get; init; }
    public EsclInputCaps? PlatenCaps { get; init; }
    public EsclInputCaps? AdfSimplexCaps { get; init; }
    public EsclInputCaps? AdfDuplexCaps { get; init; }
}