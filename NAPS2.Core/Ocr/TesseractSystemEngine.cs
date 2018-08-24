using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;
using NAPS2.Util;

namespace NAPS2.Ocr
{
    public class TesseractSystemEngine : TesseractBaseEngine
    {
        private bool isInstalled;
        private DateTime? installCheckTime;
        private List<Language> installedLanguages;

        public TesseractSystemEngine(AppConfigManager appConfigManager) : base(appConfigManager)
        {
            // Use the most complete set of language mappings
            LanguageData = TesseractLanguageData.V400B4;
            TesseractBasePath = "";
            TesseractExePath = "tesseract";
            PlatformSupport = PlatformSupport.Linux;
            CanInstall = false;
        }

        protected override RunInfo TesseractRunInfo(OcrParams ocrParams) => new RunInfo
        {
            Arguments = "",
            DataPath = null,
            PrefixPath = null
        };

        public override bool IsInstalled
        {
            get
            {
                CheckIfInstalled();
                return isInstalled;
            }
        }

        public override IEnumerable<Language> InstalledLanguages
        {
            get
            {
                CheckIfInstalled();
                return installedLanguages;
            }
        }


        public override IEnumerable<Language> NotInstalledLanguages => Enumerable.Empty<Language>();

        private void CheckIfInstalled()
        {
            if (IsSupported && (installCheckTime == null || installCheckTime < DateTime.Now - TimeSpan.FromSeconds(2)))
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = TesseractExePath,
                        Arguments = "--list-langs",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                    if (process != null && process.Id != 0)
                    {
                        var codes = process.StandardError.ReadToEnd().Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length == 3);
                        installedLanguages = codes.Select(code => LanguageData.LanguageMap.Get($"ocr-{code}")).Where(lang => lang != null).ToList();
                        isInstalled = true;
                        process.Kill();
                    }
                }
                catch (Exception)
                {
                    // Component is not installed on the system path (or had an error)
                }
                installCheckTime = DateTime.Now;
            }
        }
    }
}
