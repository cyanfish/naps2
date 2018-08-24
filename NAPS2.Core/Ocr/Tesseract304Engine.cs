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
        protected static readonly List<DownloadMirror> Mirrors = new List<DownloadMirror>
        {
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-3.04/{0}"),
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-3.04/{0}/download"),
            new DownloadMirror(PlatformSupport.WindowsXp, @"http://xp-mirror.naps2.com/tesseract-3.04/{0}")
        };

        public Tesseract304Engine(AppConfigManager appConfigManager, ComponentManager componentManager) : base(appConfigManager)
        {
            LanguageData = TesseractLanguageData.V304;
            TesseractBasePath = Path.Combine(componentManager.BasePath, "tesseract-3.0.4");
            TesseractExePath = "tesseract.exe";
            PlatformSupport = PlatformSupport.ModernWindows;
            CanInstall = true;

            Component = new ExternalComponent("ocr", Path.Combine(TesseractBasePath, TesseractExePath),
                new DownloadInfo("tesseract.exe.gz", Mirrors, 1.32, "0b0fd21cd886c04c60ed5c3f38b9120b408139b3", DownloadFormat.Gzip));

            LanguageComponents = LanguageData.Data.Select(x =>
                new ExternalComponent($"ocr-{x.Code}", Path.Combine(TesseractBasePath, "tessdata", x.Filename.Replace(".gz", "")),
                    new DownloadInfo(x.Filename, Mirrors, x.Size, x.Sha1, DownloadFormat.Gzip)));
        }
    }
}
