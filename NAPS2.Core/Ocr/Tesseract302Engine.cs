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

        protected override string TesseractBasePath => Path.Combine(ExternalComponent.BasePath, "tesseract-3.0.2");

        protected override string TesseractExePath => "tesseract.exe";

        protected override string TesseractHocrExtension => ".html";

        protected override PlatformSupport PlatformSupport => PlatformSupport.Windows;

        public override bool IsUpgradable => true;

        public override bool CanInstall => false;
    }
}
