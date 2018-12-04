using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Ocr
{
    public class OcrManager
    {
        private readonly List<IOcrEngine> engines;

        private static OcrManager _default;

        public static bool HasDefault => _default != null;

        public static OcrManager Default
        {
            // TODO: Verify package info
            get => _default ?? throw new InvalidOperationException(
                       "OcrManager.Default must be initialized first. You can do one of the following: " +
                       "(a) Install the NAPS2.Sdk.Windows.Tesseract package. " +
                       "(b) Use OcrManager.SystemPathOnly. " +
                       "(c) Create an instance of OcrManager with your own configuration.");
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static OcrManager SystemPathOnly { get; } = new OcrManager(new[] { new TesseractSystemEngine() });

        /// <summary>
        /// Creates a new instance of OcrManager with the specified engines. The order of engines is important; preferred/newer first.
        /// </summary>
        /// <param name="orderedEngineList"></param>
        public OcrManager(IEnumerable<IOcrEngine> orderedEngineList)
        {
            engines = orderedEngineList.ToList();
        }

        /// <summary>
        /// Creates a new instance of OcrManager with the default set of engines.
        /// <param name="basePath">The base path for installed engines.</param>
        /// </summary>
        public OcrManager(string basePath)
        {
            engines = new List<IOcrEngine>
            {
                new Tesseract400Beta4Engine(basePath),
                new Tesseract304Engine(basePath),
                new Tesseract304XpEngine(basePath),
                new Tesseract302Engine(basePath),
                new TesseractSystemEngine()
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
                    if (!string.IsNullOrWhiteSpace(AppConfig.Current.OcrDefaultLanguage))
                    {
                        return new OcrParams(AppConfig.Current.OcrDefaultLanguage, AppConfig.Current.OcrDefaultMode);
                    }
                    return null;
                }

                OcrParams UserLevelParams()
                {
                    if (!string.IsNullOrWhiteSpace(UserConfig.Current.OcrLanguageCode))
                    {
                        return new OcrParams(UserConfig.Current.OcrLanguageCode, UserConfig.Current.OcrMode);
                    }
                    return null;
                }

                OcrParams ArbitraryParams() => new OcrParams(ActiveEngine?.InstalledLanguages.OrderBy(x => x.Name).Select(x => x.Code).FirstOrDefault(), OcrMode.Default);

                // Prioritize app-level overrides
                if (AppConfig.Current.OcrState == OcrState.Disabled)
                {
                    return null;
                }
                if (AppConfig.Current.OcrState == OcrState.Enabled)
                {
                    return AppLevelParams() ?? UserLevelParams() ?? ArbitraryParams();
                }
                // No overrides, so prioritize the user settings
                if (UserConfig.Current.EnableOcr)
                {
                    return UserLevelParams() ?? AppLevelParams() ?? ArbitraryParams();
                }
                return null;
            }
        }
    }
}
