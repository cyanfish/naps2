using NAPS2.Dependencies;

namespace NAPS2.Ocr;

public class TesseractLanguageManager
{
    private static readonly List<DownloadMirror> Mirrors = new()
    {
        new(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.0.0b4/{0}"),
        new(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-4.0.0b4/{0}/download")
    };

    public TesseractLanguageManager(string basePath)
    {
        LanguageData = TesseractLanguageData.Latest;
        TesseractBasePath = Path.Combine(basePath, "tesseract-4.0.0b4");

        LanguageComponents = LanguageData.Data.Select(x =>
            new MultiFileExternalComponent($"ocr-{x.Code}", TesseractBasePath, new[] { $"best/{x.Code}.traineddata", $"fast/{x.Code}.traineddata" },
                new DownloadInfo(x.Filename, Mirrors, x.Size, x.Sha1, DownloadFormat.Zip)));
    }

    public virtual IEnumerable<Language> InstalledLanguages =>
        LanguageComponents.Where(x => x.IsInstalled).Select(x => LanguageData.LanguageMap[x.Id]);

    public virtual IEnumerable<Language> NotInstalledLanguages =>
        LanguageComponents.Where(x => !x.IsInstalled).Select(x => LanguageData.LanguageMap[x.Id]);

    public string TesseractBasePath { get; protected init; }
    
    public TesseractLanguageData LanguageData { get; protected init; }
    
    public IEnumerable<IExternalComponent> LanguageComponents { get; protected init; }
}