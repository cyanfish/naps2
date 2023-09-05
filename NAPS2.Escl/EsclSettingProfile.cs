using NAPS2.Escl.Client;

namespace NAPS2.Escl;

public class EsclSettingProfile
{
    public string? Name { get; init; }
    public List<EsclColorMode> ColorModes { get; init; } = new();
    public List<string> DocumentFormats { get; init; } = new();
    public List<string> DocumentFormatsExt { get; init; } = new();
    public List<DiscreteResolution> DiscreteResolutions { get; init; } = new();
    public ResolutionRange? XResolutionRange { get; init; }
    public ResolutionRange? YResolutionRange { get; init; }
}