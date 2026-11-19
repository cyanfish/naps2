using System.Collections;
using System.Globalization;
using System.Threading;
using NAPS2.Lang;

namespace NAPS2.Util;

/// <summary>
/// A helper to for culture-related functionality.
/// </summary>
public class CultureHelper
{
    private readonly Naps2Config _config;

    /// <summary>
    /// Loads globalization data on a background thread so it's ready by the time startup code needs it.
    /// </summary>
    public static void PreloadGlobalization()
    {
        new Thread(() =>
        {
            var culture = CultureInfo.CurrentCulture;
            _ = culture.CompareInfo.Compare("a", "b");
            _ = "a".ToUpper(culture);
            _ = DateTime.Now.ToString(culture);
        }) { IsBackground = true }.Start();
    }

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
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
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
        // For self-contained builds we don't have separate DLL files we can check for existence
        // TODO: Don't want to hard code this... it defeats the whole purpose of autodetection
        var exclude = new HashSet<string> { "bn", "ur" };
        return GetAllCultures().Where(x => !exclude.Contains(x.langCode));
    }
}