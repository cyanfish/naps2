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
        }

        protected override string TesseractBasePath => Path.Combine(componentManager.BasePath, "tesseract-3.0.4");

        protected override string TesseractExePath => "tesseract.exe";

        protected override string TesseractHocrExtension => ".hocr";

        protected override PlatformSupport PlatformSupport => PlatformSupport.ModernWindows;

        public override bool IsUpgradable => true;

        public override bool CanInstall => false;
    }
}
