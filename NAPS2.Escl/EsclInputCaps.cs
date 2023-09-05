namespace NAPS2.Escl;

public class EsclInputCaps
{
    public List<EsclSettingProfile> SettingProfiles { get; init; } = new();
    public int? MinWidth { get; set; }
    public int? MaxWidth { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxHeight { get; set; }
}