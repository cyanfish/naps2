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

        protected override string TesseractExePath => Path.Combine("tess64", "tesseract.exe");

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

        public override IEnumerable<OcrMode> SupportedModes => new[] { OcrMode.Fast, OcrMode.Best, OcrMode.Legacy };

        protected override PlatformSupport PlatformSupport => PlatformSupport.ModernWindows64;

        public override bool CanInstall => true;
        
        public override IExternalComponent Component => new MultiFileExternalComponent("ocr", Path.Combine(TesseractBasePath, "tess64"), new[]
        {
            "pvt.cppan.demo.danbloomberg.leptonica-1.76.0.dll",
            "pvt.cppan.demo.jpeg-9.2.0.dll",
            "pvt.cppan.demo.madler.zlib-1.2.11.dll",
            "pvt.cppan.demo.openjpeg.openjp2-2.3.0.dll",
            "pvt.cppan.demo.png-1.6.35.dll",
            "pvt.cppan.demo.tiff-4.0.9.dll",
            "pvt.cppan.demo.webp-0.6.1.dll",
            "pvt.cppan.demo.xz_utils.lzma-5.2.4.dll",
            "tesseract40.dll",
            TesseractExePath
        }, DownloadInfo);

        protected override DownloadInfo DownloadInfo => new DownloadInfo("tesseract-4.0.0b4.zip", TesseractMirrors, 3.33, "03f4ae58312c1e2329323fe4b555e4f8c7ce8b0e", DownloadFormat.Zip);

        public override IEnumerable<IExternalComponent> LanguageComponents => TesseractLanguageData.Select(x =>
            new MultiFileExternalComponent($"ocr-{x.Code}", TesseractBasePath, new[] { $"best/{x.Code}.traineddata", $"fast/{x.Code}.traineddata" },
                CanInstall ? new DownloadInfo(x.Filename, TesseractMirrors, x.Size, x.Sha1, DownloadFormat.Zip) : null));
    }
}
