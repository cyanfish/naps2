using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class Tesseract302Engine : TesseractBaseEngine
    {
        public Tesseract302Engine(AppConfigManager appConfigManager) : base(appConfigManager)
        {
        }

        protected override string TesseractExePath => Path.Combine(ExternalComponent.BasePath, "tesseract-3.0.2", "tesseract.exe");

        protected override string TesseractHocrExtension => ".html";

        protected override string TesseractDataPath => Path.Combine(ExternalComponent.BasePath, "tesseract-3.0.2");

        protected override string TesseractPrefixPath => Path.Combine(ExternalComponent.BasePath, "tesseract-3.0.2");

        protected override PlatformSupport PlatformSupport => PlatformSupport.Windows;

        public override bool IsInstalled => Component.IsInstalled;

        public override bool IsUpgradable => true;

        public override bool CanInstall => false;

        public override IEnumerable<Language> InstalledLanguages => LanguageComponents.Where(x => x.IsInstalled).Select(x => Languages[x.Id.Substring(4)]);

        public override ExternalComponent Component => new ExternalComponent("ocr", Path.Combine("tesseract-3.0.2", "tesseract.exe"), PlatformSupport.Windows);

        public override IEnumerable<ExternalComponent> LanguageComponents => LanguageData.Select(x => new ExternalComponent($"ocr-{x.Code}", Path.Combine("tesseract-3.0.2", "tessdata", x.Filename.Replace(".gz", ""))));

        public override IEnumerable<OcrMode> SupportedModes => null;

        // TODO:
        // Move common stuff to base
        // Extract redundant information
        // FIgure out how to handle laziness/repeated computation
        // Include download information (not for 302 of course)
    }
}
