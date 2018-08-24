using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class Tesseract400Beta4Engine : TesseractBaseEngine
    {
        private readonly ComponentManager componentManager;

        public Tesseract400Beta4Engine(AppConfigManager appConfigManager, ComponentManager componentManager) : base(appConfigManager)
        {
            this.componentManager = componentManager;
        }

        protected override string TesseractBasePath => Path.Combine(componentManager.BasePath, "tesseract-4.0.0b4");

        protected override string TesseractExePath => "tesseract.exe";

        protected override RunInfo TesseractRunInfo(OcrParams ocrParams)
        {
            string folder = ocrParams.Mode == OcrMode.Fast || ocrParams.Mode == OcrMode.Default ? "fast" : "best";
            if (!File.Exists(Path.Combine(TesseractBasePath, folder, $"{ocrParams.LanguageCode}.traineddata")))
            {
                // Use the other source if the selected one doesn't exist
                folder = folder == "fast" ? "best" : "fast";
            }

            return new RunInfo
            {
                Arguments = ocrParams.Mode == OcrMode.Best ? "--oem 1" : ocrParams.Mode == OcrMode.Legacy ? "--oem 0" : "",
                DataPath = folder,
                PrefixPath = folder
            };
        }

        protected override string TesseractHocrExtension => ".html";

        protected override PlatformSupport PlatformSupport => PlatformSupport.ModernWindows;

        public override bool IsUpgradable => false;

        public override bool CanInstall => true;

        protected override DownloadInfo DownloadInfo => new DownloadInfo("tesseract.exe.gz", TesseractMirrors, 1.32, "0b0fd21cd886c04c60ed5c3f38b9120b408139b3", DownloadFormat.Gzip);

        public override IEnumerable<OcrMode> SupportedModes => new[] { OcrMode.Fast, OcrMode.Best, OcrMode.Legacy };
    }
}
