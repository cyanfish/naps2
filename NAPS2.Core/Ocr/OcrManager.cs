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

        public bool IsNewestReady => engines.Any(x => x.IsSupported && x.IsInstalled && !x.IsUpgradable && x.InstalledLanguages.Any());

        public bool CanUpgrade => !IsNewestReady && engines.Any(x => x.IsInstalled);

        public bool MustUpgrade => !IsReady && engines.Any(x => x.IsInstalled);

        public ExternalComponent UpgradeComponent => engines.Where(x => x.IsSupported && !x.IsInstalled && !x.IsUpgradable).Select(x => x.Component).FirstOrDefault();

        public bool MustInstallPackage => engines.All(x => (!x.IsSupported || !x.CanInstall) && !x.IsInstalled);

        public IOcrEngine ActiveEngine => engines.FirstOrDefault(x => x.IsSupported && x.IsInstalled && x.InstalledLanguages.Any());

        public OcrParams DefaultParams
        {
            get
            {
                // Prioritize app-level overrides
                if (appConfigManager.Config.OcrState == OcrState.Disabled)
                {
                    return null;
                }
                if (appConfigManager.Config.OcrState == OcrState.Enabled)
                {
                    // Prioritize the app-level language
                    if (!string.IsNullOrWhiteSpace(appConfigManager.Config.OcrDefaultLanguage))
                    {
                        return new OcrParams(appConfigManager.Config.OcrDefaultLanguage, appConfigManager.Config.OcrDefaultMode);
                    }
                    // Fall back to the user-selected language
                    if (!string.IsNullOrWhiteSpace(userConfigManager.Config.OcrLanguageCode))
                    {
                        return new OcrParams(userConfigManager.Config.OcrLanguageCode, userConfigManager.Config.OcrMode);
                    }
                    // Fall back to an arbitrary installed language (probably there is only one)
                    return new OcrParams(ActiveEngine?.InstalledLanguages.OrderBy(x => x.Name).Select(x => x.Code).FirstOrDefault(), OcrMode.Default);
                }
                // No overrides, so prioritize the user settings
                if (userConfigManager.Config.EnableOcr)
                {
                    // Prioritize the user-selected language
                    if (!string.IsNullOrWhiteSpace(userConfigManager.Config.OcrLanguageCode))
                    {
                        return new OcrParams(userConfigManager.Config.OcrLanguageCode, userConfigManager.Config.OcrMode);
                    }
                    // Fall back to the app-level language
                    if (!string.IsNullOrWhiteSpace(appConfigManager.Config.OcrDefaultLanguage))
                    {
                        return new OcrParams(appConfigManager.Config.OcrDefaultLanguage, appConfigManager.Config.OcrDefaultMode);
                    }
                    // Fall back to an arbitrary installed language (probably there is only one)
                    return new OcrParams(ActiveEngine?.InstalledLanguages.OrderBy(x => x.Name).Select(x => x.Code).FirstOrDefault(), OcrMode.Default);
                }
                return null;
            }
        }

        // TODO: Implement multi-file external components with SharpZipLib to package that as a single download
    }
}
