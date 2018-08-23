using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class Tesseract304XpEngine : Tesseract304Engine
    {
        public Tesseract304XpEngine(AppConfigManager appConfigManager) : base(appConfigManager)
        {
        }
        
        protected override string TesseractExePath => "tesseract_xp.exe";
        
        protected override PlatformSupport PlatformSupport => PlatformSupport.Windows;

        protected override DownloadInfo DownloadInfo => new DownloadInfo("tesseract_xp.exe.gz", TesseractMirrors, 1.32, "98d15e4765caae864f16fa2ab106e3fd6adbe8c3", DownloadFormat.Gzip);
    }
}
