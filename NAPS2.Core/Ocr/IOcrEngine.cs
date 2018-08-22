using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public interface IOcrEngine
    {
        bool CanProcess(string langCode);

        OcrResult ProcessImage(string imagePath, string langCode, Func<bool> cancelCallback);

        bool IsSupported { get; }

        bool IsInstalled { get; }

        bool IsUpgradable { get; }

        bool CanInstall { get; }

        IEnumerable<Language> InstalledLanguages { get; }

        ExternalComponent Component { get; }

        IEnumerable<ExternalComponent> LanguageComponents { get; }


    }
}