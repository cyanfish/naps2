namespace NAPS2.Escl;

public class EsclSettingProfile
{
    public string? Name { get; init; }
    public List<EsclColorMode> ColorModes { get; init; } = new();
}