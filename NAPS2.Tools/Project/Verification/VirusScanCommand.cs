using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public class VirusScanCommand : ICommand<VirusScanOptions>
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    public int Run(VirusScanOptions opts)
    {
        Output.Info("Checking for antivirus false positives");
        var version = ProjectHelper.GetCurrentVersionName();
        var key = File.ReadAllText(Path.Combine(Paths.Naps2UserFolder, "virustotal")).Trim();
        using var client = new HttpClient
        {
            BaseAddress = new Uri("https://www.virustotal.com/api/v3/"),
            Timeout = TimeSpan.FromMinutes(30),
            DefaultRequestHeaders = { { "x-apikey", key } }
        };

        var tasks = new List<Task>();
        foreach (var target in TargetsHelper.EnumeratePackageTargets(
                     new TargetParams { PackageType = opts.PackageType, Platform = opts.Platform }))
        {
            var ext = target.Type.ToString().ToLowerInvariant();
            var packageName = opts.Name == null
                ? target.Platform.PackageName()
                : $"{target.Platform.PackageName()}-{opts.Name}";
            tasks.Add(StartVirusScan(client, ProjectHelper.GetPackagePath(ext, target.Platform, version, packageName)));
        }
        Task.WaitAll(tasks.ToArray());
        Output.OperationEnd("No antivirus false positives.");
        return 0;
    }

    private static async Task StartVirusScan(HttpClient client, string packagePath)
    {
        string hash;
        await using (var stream = File.OpenRead(packagePath))
        {
            hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(stream));
        }
        var permalink = $"https://www.virustotal.com/gui/file/{hash}";
        Output.Info($"Report permalink: {permalink}");

        JsonNode? stats;
        var report = await Send(client, () => new HttpRequestMessage(HttpMethod.Get, $"files/{hash}"),
            allowNotFound: true);
        if (report != null)
        {
            Output.Verbose($"VirusTotal already has a report for: {packagePath}");
            stats = report["data"]?["attributes"]?["last_analysis_stats"];
        }
        else
        {
            Output.Verbose($"Uploading to VirusTotal: {packagePath}");
            var uploadUrlResponse =
                await Send(client, () => new HttpRequestMessage(HttpMethod.Get, "files/upload_url"));
            var uploadUrl = (string?) uploadUrlResponse?["data"] ??
                            throw new Exception("VirusTotal didn't return an upload URL");
            var upload = await Send(client, () =>
            {
                var file = new StreamContent(File.OpenRead(packagePath));
                file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                return new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = new MultipartFormDataContent { { file, "file", Path.GetFileName(packagePath) } }
                };
            });
            var analysisId = (string?) upload?["data"]?["id"] ??
                             throw new Exception("VirusTotal didn't return an analysis");

            while (true)
            {
                await Task.Delay(PollInterval);
                var analysis = await Send(client,
                    () => new HttpRequestMessage(HttpMethod.Get, $"analyses/{analysisId}"));
                var attributes = analysis?["data"]?["attributes"];
                if ((string?) attributes?["status"] == "completed")
                {
                    stats = attributes["stats"];
                    break;
                }
            }
        }

        if (stats == null)
        {
            throw new Exception($"VirusTotal didn't return results for {packagePath}");
        }
        if ((int?) stats["malicious"] > 0)
        {
            throw new Exception($"VirusTotal has {stats["malicious"]} engines with positive flags. {permalink}");
        }
        Output.Info(
            $"No false positives ({stats["undetected"]} undetected, {stats["suspicious"]} suspicious): {packagePath}");
    }

    private static async Task<JsonNode?> Send(HttpClient client, Func<HttpRequestMessage> createRequest,
        bool allowNotFound = false)
    {
        while (true)
        {
            using var request = createRequest();
            using var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(PollInterval);
                continue;
            }
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var body = await response.Content.ReadAsStringAsync();
            return !response.IsSuccessStatusCode
                ? throw new Exception($"VirusTotal request failed ({(int) response.StatusCode}): {body}")
                : JsonNode.Parse(body);
        }
    }
}