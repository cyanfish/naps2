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
        public Tesseract302Engine(AppConfigManager appConfigManager, ComponentManager componentManager) : base(appConfigManager)
        {
            // Using the newer data since we just need the 302 engine for backwards compatibility
            LanguageData = TesseractLanguageData.V304;
            TesseractBasePath = Path.Combine(componentManager.BasePath, "tesseract-3.0.2");
            TesseractExePath = "tesseract.exe";
            TesseractHocrExtension = ".html";
            PlatformSupport = PlatformSupport.Windows;
            CanInstall = false;

            Component = new ExternalComponent("ocr", Path.Combine(TesseractBasePath, TesseractExePath), null);

            LanguageComponents = LanguageData.Data.Select(x =>
                new ExternalComponent($"ocr-{x.Code}", Path.Combine(TesseractBasePath, "tessdata", x.Filename.Replace(".gz", "")), null));
        }
    }
}
