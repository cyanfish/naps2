using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public class TesseractSystemEngine : TesseractBaseEngine
    {
        public TesseractSystemEngine(AppConfigManager appConfigManager) : base(appConfigManager)
        {
        }

        protected override string TesseractBasePath => "";

        protected override string TesseractExePath => "tesseract";

        protected override string TesseractDataPath => null;

        protected override string TesseractPrefixPath => null;

        protected override string TesseractHocrExtension => ".hocr";

        protected override PlatformSupport PlatformSupport => PlatformSupport.Linux;

        public override bool IsUpgradable => false;

        public override bool CanInstall => false;
    }
}
