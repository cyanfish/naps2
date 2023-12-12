namespace NAPS2.Escl;

public class EsclInputCaps
{
    // Units of 1/300 inch (per ESCL spec); supports A3 in both orientations
    public const int DEFAULT_MAX_WIDTH = 5000;
    public const int DEFAULT_MAX_HEIGHT = 5000;

    public List<EsclSettingProfile> SettingProfiles { get; init; } = new();
    public int? MinWidth { get; set; }
    public int? MaxWidth { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxHeight { get; set; }
}