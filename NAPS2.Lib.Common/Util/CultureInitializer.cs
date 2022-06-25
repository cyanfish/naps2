using System.Globalization;

namespace NAPS2.Util;

/// <summary>
/// A helper to set the thread culture based on user and app configuration.
/// </summary>
public class CultureInitializer
{
    private readonly Naps2Config _config;

    public CultureInitializer(Naps2Config config)
    {
        _config = config;
    }

    public void InitCulture()
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
}