using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class OcrManager
    {
        private readonly List<IOcrEngine> engines;

        public OcrManager(Tesseract302Engine t302, Tesseract304Engine t304, Tesseract304XpEngine t304Xp, Tesseract400Beta4Engine t400B4, TesseractSystemEngine tsys)
        {
            engines = new List<IOcrEngine>
            {
                t302,
                t304,
                t304Xp,
                t400B4,
                tsys
            };
        }

        public IEnumerable<IOcrEngine> Engines => engines;

        public bool IsReady => engines.Any(x => x.IsInstalled && x.IsSupported && x.InstalledLanguages.Any());

        public bool IsNewestReady => engines.Any(x => x.IsInstalled && x.IsSupported && !x.IsUpgradable && x.InstalledLanguages.Any());

        public bool CanUpgrade => !IsNewestReady && engines.Any(x => x.IsInstalled);

        public bool MustUpgrade => !IsReady && engines.Any(x => x.IsInstalled);

        public ExternalComponent UpgradeComponent => engines.Where(x => !x.IsInstalled && x.IsSupported && !x.IsUpgradable).Select(x => x.Component).FirstOrDefault();

        public bool MustInstallPackage => engines.All(x => !x.IsInstalled && (!x.IsSupported || !x.CanInstall));
    }
}
