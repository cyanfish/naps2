using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Config;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class OcrManager
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;

        private readonly List<IOcrEngine> engines;

        public OcrManager(Tesseract302Engine t302, Tesseract304Engine t304, Tesseract304XpEngine t304Xp, Tesseract400Beta4Engine t400B4, TesseractSystemEngine tsys, IUserConfigManager userConfigManager, AppConfigManager appConfigManager)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;

            // Order is important here. Newer/preferred first
            engines = new List<IOcrEngine>
            {
                t400B4,
                t304,
                t304Xp,
                t302,
                tsys
            };
        }

        public IEnumerable<IOcrEngine> Engines => engines;

        public bool IsReady => engines.Any(x => x.IsSupported && x.IsInstalled && x.InstalledLanguages.Any());

        public bool IsNewestReady
        {
            get
            {
                var latest = engines.FirstOrDefault(x => x.IsSupported);
                if (latest == null) return false;
                return latest.IsInstalled && latest.InstalledLanguages.Any();
            }
        }

        public bool CanUpgrade => !IsNewestReady && engines.Any(x => x.IsInstalled);

        public bool MustUpgrade => !IsReady && engines.Any(x => x.IsInstalled);

        public bool MustInstallPackage => engines.All(x => (!x.IsSupported || !x.CanInstall) && !x.IsInstalled);

        public IOcrEngine ActiveEngine => engines.FirstOrDefault(x => x.IsSupported && x.IsInstalled && x.InstalledLanguages.Any());

        public IOcrEngine InstalledEngine => engines.FirstOrDefault(x => x.IsInstalled && x.InstalledLanguages.Any());

        public IOcrEngine EngineToInstall => engines.FirstOrDefault(x => x.IsSupported && x.CanInstall);

        public OcrParams DefaultParams
        {
            get
            {
                OcrParams AppLevelParams()
                {
                    if (!string.IsNullOrWhiteSpace(appConfigManager.Config.OcrDefaultLanguage))
                    {
                        return new OcrParams(appConfigManager.Config.OcrDefaultLanguage, appConfigManager.Config.OcrDefaultMode);
                    }
                    return null;
                }

                OcrParams UserLevelParams()
                {
                    if (!string.IsNullOrWhiteSpace(userConfigManager.Config.OcrLanguageCode))
                    {
                        return new OcrParams(userConfigManager.Config.OcrLanguageCode, userConfigManager.Config.OcrMode);
                    }
                    return null;
                }

                OcrParams ArbitraryParams() => new OcrParams(ActiveEngine?.InstalledLanguages.OrderBy(x => x.Name).Select(x => x.Code).FirstOrDefault(), OcrMode.Default);

                // Prioritize app-level overrides
                if (appConfigManager.Config.OcrState == OcrState.Disabled)
                {
                    return null;
                }
                if (appConfigManager.Config.OcrState == OcrState.Enabled)
                {
                    return AppLevelParams() ?? UserLevelParams() ?? ArbitraryParams();
                }
                // No overrides, so prioritize the user settings
                if (userConfigManager.Config.EnableOcr)
                {
                    return UserLevelParams() ?? AppLevelParams() ?? ArbitraryParams();
                }
                return null;
            }
        }
    }
}
