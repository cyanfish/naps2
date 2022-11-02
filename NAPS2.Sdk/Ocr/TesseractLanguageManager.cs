using NAPS2.Dependencies;

namespace NAPS2.Ocr;

public class TesseractLanguageManager
{
    private static readonly List<DownloadMirror> Mirrors = new()
    {
        new(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.0.0b4/{0}"),
        new(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-4.0.0b4/{0}/download")
    };

    private readonly TesseractLanguageData _languageData = TesseractLanguageData.Latest;

    public TesseractLanguageManager(string basePath)
    {
        TessdataBasePath = GetTessdataBasePath(basePath);
        LanguageComponents = _languageData.Data.Select(x =>
            new MultiFileExternalComponent($"ocr-{x.Code}", TessdataBasePath, new[] { $"best/{x.Code}.traineddata", $"fast/{x.Code}.traineddata" },
                new DownloadInfo(x.Filename, Mirrors, x.Size, x.Sha1, DownloadFormat.Zip)));
    }

    private string GetTessdataBasePath(string basePath)
    {
        var legacyBasePath = Path.Combine(basePath, "tesseract-4.0.0b4");
        var newBasePath = Path.Combine(basePath, "tesseract4");
        if (Directory.Exists(legacyBasePath) && !Directory.Exists(newBasePath))
        {
            try
            {
                Directory.Move(legacyBasePath, newBasePath);
            }
            catch (Exception)
            {
                // Ignore errors and keep the legacy path, e.g. if the components folder is read-only
                return legacyBasePath;
            }
        }
        return newBasePath;
    }

    public string TessdataBasePath { get; }

    public virtual IEnumerable<Language> InstalledLanguages =>
        LanguageComponents.Where(x => x.IsInstalled).Select(x => _languageData.LanguageMap[x.Id]);

    public virtual IEnumerable<Language> NotInstalledLanguages =>
        LanguageComponents.Where(x => !x.IsInstalled).Select(x => _languageData.LanguageMap[x.Id]);

    public Language GetLanguage(string code) => _languageData.LanguageMap["ocr-" + code];

    public IEnumerable<IExternalComponent> LanguageComponents { get; }
}