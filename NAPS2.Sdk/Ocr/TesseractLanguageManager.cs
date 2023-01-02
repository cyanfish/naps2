using NAPS2.Dependencies;

namespace NAPS2.Ocr;

public class TesseractLanguageManager
{
    private static readonly List<DownloadMirror> Mirrors = new()
    {
        new(@"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.0.0b4/{0}"),
        new(@"https://sourceforge.net/projects/naps2/files/components/tesseract-4.0.0b4/{0}/download")
    };

    private readonly TesseractLanguageData _languageData = TesseractLanguageData.Latest;

    public TesseractLanguageManager(string basePath)
    {
        TessdataBasePath = GetTessdataBasePath(basePath);
        LanguageComponents = _languageData.Data.Select(x =>
            new MultiFileExternalComponent($"ocr-{x.Code}", TessdataBasePath,
                new[] { $"best/{x.Code}.traineddata", $"fast/{x.Code}.traineddata" },
                new DownloadInfo(x.Filename, Mirrors, x.Size, x.Sha1, DownloadFormat.Zip)));
    }

    private string GetTessdataBasePath(string basePath)
    {
        var newBasePath = Path.Combine(basePath, "tesseract4");
        var legacyBasePath = Path.Combine(basePath, "tesseract-4.0.0b4");
        if (Directory.Exists(newBasePath))
        {
            return newBasePath;
        }
        if (Directory.Exists(legacyBasePath))
        {
            return legacyBasePath;
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