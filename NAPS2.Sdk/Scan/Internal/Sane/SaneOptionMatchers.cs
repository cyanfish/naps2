namespace NAPS2.Scan.Internal.Sane;

public static class SaneOptionMatchers
{
    private static readonly IEnumerable<string> FlatbedStrs = new[]
    {
        SaneOptionTranslations.Flatbed,
        SaneOptionTranslations.FB,
        SaneOptionTranslations.fb
    }.SelectMany(x => x);

    private static readonly IEnumerable<string> FeederStrs = new[]
    {
        SaneOptionTranslations.ADF,
        SaneOptionTranslations.adf,
        SaneOptionTranslations.Automatic_Document_Feeder,
        SaneOptionTranslations.ADF_Front
    }.SelectMany(x => x);

    private static readonly IEnumerable<string> DuplexStrs = new[]
    {
        SaneOptionTranslations.Duplex,
        SaneOptionTranslations.ADF_Duplex
    }.SelectMany(x => x);

    public static readonly SaneOptionMatcher Duplex =
        new SaneOptionMatcher(DuplexStrs, "duplex");

    public static readonly SaneOptionMatcher Feeder =
        new SaneOptionMatcher(FeederStrs, "feeder", "adf").Exclude(Duplex);

    public static readonly SaneOptionMatcher Flatbed =
        new SaneOptionMatcher(FlatbedStrs, "flatbed");

    public static readonly SaneOptionMatcher BlackAndWhite =
        new SaneOptionMatcher(SaneOptionTranslations.Lineart, "black and white", "black & white", "black/white");
    
    public static readonly SaneOptionMatcher Grayscale =
        new SaneOptionMatcher(SaneOptionTranslations.Gray, "gray", "grey");
    
    public static readonly SaneOptionMatcher Color =
        new SaneOptionMatcher(SaneOptionTranslations.Color, "color", "colour");
}