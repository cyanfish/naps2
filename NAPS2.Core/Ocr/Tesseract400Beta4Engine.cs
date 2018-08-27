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
        protected static readonly List<DownloadMirror> Mirrors = new List<DownloadMirror>
        {
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.00b4/{0}"),
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-4.00b4/{0}/download")
        };

        public Tesseract400Beta4Engine(AppConfigManager appConfigManager, ComponentManager componentManager) : base(appConfigManager)
        {
            string exeFolder = Environment.Is64BitProcess ? "tess64" : "tess32";
            LanguageData = TesseractLanguageData.V400B4;
            TesseractBasePath = Path.Combine(componentManager.BasePath, "tesseract-4.0.0b4");
            TesseractExePath = Path.Combine(exeFolder, "tesseract.exe");
            PlatformSupport = PlatformSupport.ModernWindows;
            CanInstall = true;
            SupportedModes = new[] { OcrMode.Fast, OcrMode.Best, OcrMode.Legacy };

            var download = Environment.Is64BitProcess
                ? new DownloadInfo("tesseract.exe-dlls-400b4-tess64.zip", Mirrors, 2.61, "03f4ae58312c1e2329323fe4b555e4f8c7ce8b0e", DownloadFormat.Zip)
                : new DownloadInfo("tesseract.exe-dlls-400b4-tess32.zip", Mirrors, 3.33, "7de12928ec6a7cdb28fb1895a41d637da968eb0c", DownloadFormat.Zip);
            Component = new MultiFileExternalComponent("ocr", Path.Combine(TesseractBasePath, exeFolder), new[] { "tesseract.exe" }, download);

            LanguageComponents = LanguageData.Data.Select(x =>
                new MultiFileExternalComponent($"ocr-{x.Code}", TesseractBasePath, new[] { $"best/{x.Code}.traineddata", $"fast/{x.Code}.traineddata" },
                    new DownloadInfo(x.Filename, Mirrors, x.Size, x.Sha1, DownloadFormat.Zip)));
        }

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
    }
}
