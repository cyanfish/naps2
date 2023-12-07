namespace NAPS2.Tools.Localization;

public class TranslatableString
{
    public TranslatableString(string original, string? translation = null)
    {
        Original = original;
        Translation = translation;
    }
        
    public string Original { get; }

    public string? Translation { get; }

    public List<string> Context { get; } = [];
}