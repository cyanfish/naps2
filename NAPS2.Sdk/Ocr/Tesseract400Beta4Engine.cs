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
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.0.0b4/{0}"),
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-4.0.0b4/{0}/download")
        };

        public Tesseract400Beta4Engine(AppConfigManager appConfigManager, ComponentManager componentManager) : base(appConfigManager)
        {
            string exeFolder = Environment.Is64BitProcess ? "w64" : "w32";
            LanguageData = TesseractLanguageData.V400B4;
            TesseractBasePath = Path.Combine(componentManager.BasePath, "tesseract-4.0.0b4");
            TesseractExePath = Path.Combine(exeFolder, "tesseract.exe");
            PlatformSupport = PlatformSupport.ModernWindows;
            CanInstall = true;
            SupportedModes = new[] { OcrMode.Fast, OcrMode.Best, OcrMode.Legacy };

            var download = Environment.Is64BitProcess
                ? new DownloadInfo("tesseract.exe.w64.zip", Mirrors, 1.83, "4eba9aaf8800a100ef059c512be572e39ae72f4d", DownloadFormat.Zip)
                : new DownloadInfo("tesseract.exe.w32.zip", Mirrors, 1.56, "300ad281a5fa1c734dbb4a8a4dd49e3a8ab921a4", DownloadFormat.Zip);
            Component = new MultiFileExternalComponent("ocr", Path.Combine(TesseractBasePath, exeFolder), new[] { "tesseract.exe" }, download);

            LanguageComponents = LanguageData.Data.Select(x =>
                new MultiFileExternalComponent($"ocr-{x.Code}", TesseractBasePath, new[] { $"best/{x.Code}.traineddata", $"fast/{x.Code}.traineddata" },
                    new DownloadInfo(x.Filename, Mirrors, x.Size, x.Sha1, DownloadFormat.Zip)));
        }

        protected override RunInfo TesseractRunInfo(OcrParams ocrParams)
        {
            OcrMode mode = ocrParams.Mode;
            string folder = mode == OcrMode.Fast || mode == OcrMode.Default ? "fast" : "best";
            if (ocrParams.LanguageCode.Split('+').All(code => !File.Exists(Path.Combine(TesseractBasePath, folder, $"{code.ToLowerInvariant()}.traineddata"))))
            {
                // Use the other source if the selected one doesn't exist
                folder = folder == "fast" ? "best" : "fast";
                mode = folder == "fast" ? OcrMode.Fast : OcrMode.Best;
            }

            return new RunInfo
            {
                Arguments = mode == OcrMode.Best ? "--oem 1" : mode == OcrMode.Legacy ? "--oem 0" : "",
                DataPath = folder,
                PrefixPath = folder
            };
        }
    }
}
