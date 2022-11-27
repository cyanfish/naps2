using System.IO.Compression;
using System.Net.Http;
using Crowdin.Api;
using Crowdin.Api.SourceFiles;
using Crowdin.Api.Translations;
using NAPS2.Tools.Project;
using File = System.IO.File;

namespace NAPS2.Tools.Localization;

public class PullTranslationsCommand : ICommand<PullTranslationsOptions>
{
    public int Run(PullTranslationsOptions opts)
    {
        return Task.Run(async () =>
        {
            var client = CrowdinHelper.GetClient();

            await using var templatesFile =
                File.OpenRead(Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Lang", "po", "templates.pot"));
            var storage = await client.Storage.AddStorage(templatesFile, "templates.pot");

            await client.SourceFiles.UpdateOrRestoreFile(CrowdinHelper.PROJECT_ID, CrowdinHelper.TEMPLATES_FILE_ID,
                new ReplaceFileRequest
                {
                    StorageId = storage.Id
                });

            Output.Verbose("Building Crowdin project translations");
            var build = await client.Translations.BuildProjectTranslation(CrowdinHelper.PROJECT_ID,
                new BuildProjectTranslationRequest());
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                var status = await client.Translations.CheckProjectBuildStatus(CrowdinHelper.PROJECT_ID, build.Id);
                if (status.Status is BuildStatus.Failed or BuildStatus.Canceled)
                {
                    throw new Exception($"Crowdin build status: {status.Status}");
                }
                if (status.Status == BuildStatus.Finished)
                {
                    break;
                }
                if (stopwatch.ElapsedMilliseconds > 60_000)
                {
                    throw new Exception($"Crowdin build timeout");
                }
                await Task.Delay(1000);
            }
            Output.Verbose("Build succeeded, downloading translations");
            var translations =
                await client.Translations.DownloadProjectTranslations(CrowdinHelper.PROJECT_ID, build.Id);
            var response = await new HttpClient().GetAsync(translations.Link!.Url);
            Output.Verbose("Downloaded, writing to .po files");
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var zipFile = new ZipArchive(stream, ZipArchiveMode.Read);

            var localeMap = new List<(string locale, ZipArchiveEntry entry)>();
            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName.EndsWith("/")) continue;
                var locale = entry.FullName.Split("/")[0];
                localeMap.Add((locale, entry));
            }
            foreach (var (locale, entry) in localeMap)
            {
                var outputLocale = locale;
                // If we have a locale with a country (e.g. es-ES) but no other countries for the same code,
                // we only want to use the language code (es).
                if (locale.Contains("-"))
                {
                    var langCode = locale.Split("-")[0];
                    if (localeMap.Count(x => x.locale.StartsWith(langCode)) == 1)
                    {
                        outputLocale = langCode;
                    }
                }
                var outputPath = Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Lang", "po", $"{outputLocale}.po");
                await using var outputStream = File.OpenWrite(outputPath);
                await entry.Open().CopyToAsync(outputStream);
            }

            return 0;
        }).Result;
    }
}