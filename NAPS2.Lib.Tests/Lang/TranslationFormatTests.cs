using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using Xunit;

namespace NAPS2.Lib.Tests.Lang;

public class TranslationFormatTests
{
    private static readonly Regex FormatItemPattern = new(@"\{(\d+)(?:[,:][^}]*)?\}");

    public static IEnumerable<object[]> ResourceManagers =>
    [
        [nameof(MiscResources), MiscResources.ResourceManager],
        [nameof(SettingsResources), SettingsResources.ResourceManager],
        [nameof(UiStrings), UiStrings.ResourceManager],
        [nameof(SdkResources), SdkResources.ResourceManager]
    ];

    [Theory]
    [MemberData(nameof(ResourceManagers))]
    public void TranslatedFormatStringsAreValid(string name, ResourceManager resourceManager)
    {
        var baseSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var formatArgCounts = new Dictionary<string, int>();
        foreach (DictionaryEntry entry in baseSet)
        {
            if (entry.Value is string value)
            {
                var indices = FormatItemPattern.Matches(value).Select(m => int.Parse(m.Groups[1].Value)).ToList();
                if (indices.Count > 0)
                {
                    formatArgCounts[(string) entry.Key] = indices.Max() + 1;
                }
            }
        }
        Assert.NotEmpty(formatArgCounts);

        var errors = new List<string>();
        int cultureCount = 0;
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            if (Equals(culture, CultureInfo.InvariantCulture)) continue;
            var set = resourceManager.GetResourceSet(culture, true, false);
            if (set == null) continue;
            cultureCount++;
            foreach (var (key, argCount) in formatArgCounts)
            {
                var translated = set.GetString(key);
                if (translated == null) continue;
                try
                {
                    string.Format(translated, new object[argCount]);
                }
                catch (FormatException)
                {
                    errors.Add($"{name}.{culture.Name}.resx {key}: {translated}");
                }
            }
        }
        Assert.True(cultureCount > 0, $"No translations found for {name}");
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }
}
