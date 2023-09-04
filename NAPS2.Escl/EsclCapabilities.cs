namespace NAPS2.Escl;

public class EsclCapabilities
{
    public string? Version { get; init; }
    public string? MakeAndModel { get; init; }
    public string? SerialNumber { get; init; }
    public string? Uuid { get; init; }
    public string? AdminUri { get; init; }
    public string? IconUri { get; init; }
    public EsclInputCaps? PlatenCaps { get; init; }
    public EsclInputCaps? AdfSimplexCaps { get; init; }
    public EsclInputCaps? AdfDuplexCaps { get; init; }
}