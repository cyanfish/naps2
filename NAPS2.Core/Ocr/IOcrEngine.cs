using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Dependencies;

namespace NAPS2.Ocr
{
    public interface IOcrEngine
    {
        bool CanProcess(string langCode);

        OcrResult ProcessImage(string imagePath, OcrParams ocrParams, CancellationToken cancelToken);

        bool IsSupported { get; }

        bool IsInstalled { get; }

        bool CanInstall { get; }

        IEnumerable<Language> InstalledLanguages { get; }

        IEnumerable<Language> NotInstalledLanguages { get; }

        IExternalComponent Component { get; }

        IEnumerable<IExternalComponent> LanguageComponents { get; }

        IEnumerable<OcrMode> SupportedModes { get; }
    }
}