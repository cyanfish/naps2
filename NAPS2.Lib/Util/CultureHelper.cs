using System.Collections;
using System.Globalization;
using NAPS2.Lang;

namespace NAPS2.Util;

/// <summary>
/// A helper to for culture-related functionality.
/// </summary>
public class CultureHelper
{
    private readonly Naps2Config _config;

    public CultureHelper(Naps2Config config)
    {
        _config = config;
    }

    /// <summary>
    /// Sets thread and resource cultures based on the culture in the NAPS2 config (if present).
    /// </summary>
    public void SetCulturesFromConfig()
    {
        var cultureId = _config.Get(c => c.Culture);
        if (!string.IsNullOrWhiteSpace(cultureId))
        {
            try
            {
                var culture = new CultureInfo(cultureId);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                MiscResources.Culture = culture;
                SettingsResources.Culture = culture;
            }
            catch (CultureNotFoundException e)
            {
                Log.ErrorException("Invalid culture.", e);
            }
        }
    }

    public IEnumerable<(string langCode, string langName)> GetAllCultures()
    {
        // Read a list of languages from the Languages.resx file
        var resourceManager = LanguageNames.ResourceManager;
        var resourceSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;
        foreach (DictionaryEntry entry in resourceSet.Cast<DictionaryEntry>().OrderBy(x => x.Value))
        {
            var langCode = ((string) entry.Key).Replace("_", "-");
            var langName = (string) entry.Value!;
            yield return (langCode, langName);
        }
    }

    public IEnumerable<(string langCode, string langName)> GetAvailableCultures()
    {
        foreach (var (langCode, langName) in GetAllCultures())
        {
            // Only include those languages for which localized resources exist
            // TODO: Should we check for multiple project resource files? Or be less specific so this doesn't break if we rename the projects?
            string localizedResourcesPath =
                Path.Combine(AssemblyHelper.LibFolder, langCode, "NAPS2.Lib.resources.dll");
            if (langCode == "en" || File.Exists(localizedResourcesPath))
            {
                yield return (langCode, langName);
            }
        }
    }
}