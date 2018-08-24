using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class Tesseract304Engine : TesseractBaseEngine
    {
        private readonly ComponentManager componentManager;

        public Tesseract304Engine(AppConfigManager appConfigManager, ComponentManager componentManager) : base(appConfigManager)
        {
            this.componentManager = componentManager;

            LanguageData = TesseractLanguageData.V304;
        }

        protected override string TesseractBasePath => Path.Combine(componentManager.BasePath, "tesseract-3.0.4");

        protected override string TesseractExePath => "tesseract.exe";

        protected override PlatformSupport PlatformSupport => PlatformSupport.ModernWindows;

        public override bool CanInstall => false;

        public override IEnumerable<IExternalComponent> LanguageComponents => LanguageData.Data.Select(x =>
            new ExternalComponent($"ocr-{x.Code}", Path.Combine(TesseractBasePath, "tessdata", x.Filename.Replace(".gz", "")),
                CanInstall ? new DownloadInfo(x.Filename, TesseractMirrors, x.Size, x.Sha1, DownloadFormat.Gzip) : null));
    }
}
