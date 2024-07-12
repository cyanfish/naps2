using System.Net.Http;
using System.Threading;
using NAPS2.Tools.Project.Targets;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Octokit;
using Octokit.Internal;
using Renci.SshNet;
using FileMode = System.IO.FileMode;
using Repository = NuGet.Protocol.Core.Types.Repository;

namespace NAPS2.Tools.Project.Releasing;

public class UploadCommand : ICommand<UploadOptions>
{
    private const int GITHUB_REPO_ID = 39348622;

    public int Run(UploadOptions opts)
    {
        string version = opts.Version ?? ProjectHelper.GetCurrentVersion();
        if (opts.Target != "sdk")
        {
            // Validate all package files
            foreach (var target in TargetsHelper.EnumeratePackageTargets())
            {
                var file = new FileInfo(target.PackagePath(version));
                if (!file.Exists)
                {
                    throw new Exception($"Expected package to exist: {file.FullName}");
                }
                if (opts.Version == null && !opts.AllowOld && file.LastWriteTime < DateTime.Now - TimeSpan.FromHours(2))
                {
                    throw new Exception($"Expected package to be recently modified: {file.FullName}");
                }
            }
        }

        bool didSomething = false;
        if (opts.Target is "all" or "github")
        {
            Output.Info("Uploading binaries to Github");
            UploadToGithub(version).Wait();
            didSomething = true;
        }
        if (opts.Target is "all" or "sourceforge")
        {
            Output.Info("Uploading binaries to SourceForge");
            UploadToSourceForge(version).Wait();
            didSomething = true;
        }
        if (opts.Target is "all" or "static")
        {
            Output.Info("Uploading binaries to static site downloads.naps2.com");
            UploadToStaticSite(version, opts.PackageType);
            didSomething = true;
        }
        if (opts.Target is "all" or "apt")
        {
            Output.Info("Updating Apt metadata on downloads.naps2.com");
            UpdateAptMetadata();
            didSomething = true;
        }
        if (opts.Target is "sdk")
        {
            UploadToNuget().Wait();
            didSomething = true;
        }

        if (didSomething)
        {
            Output.OperationEnd(opts.Target == "sdk" ? "Packages uploaded." : "Binaries uploaded.");
        }
        else
        {
            Output.Info("No upload target.");
        }
        return 0;
    }

    private async Task UploadToNuget()
    {
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = await repository.GetResourceAsync<PackageUpdateResource>();
        var v = ProjectHelper.GetSdkVersion();
        var packagePaths = ProjectHelper.GetSdkProjects().Select(x => $"{GetProjectFolder(x)}/bin/Release/{x}.{v}.nupkg").ToList();
        var key = await File.ReadAllTextAsync(Path.Combine(Paths.Naps2UserFolder, "nuget"));
        await resource.Push(
            packagePaths,
            symbolSource: null,
            timeoutInSecond: 5 * 60,
            disableBuffering: false,
            getApiKey: _ => key,
            getSymbolApiKey: _ => null,
            noServiceEndpoint: false,
            skipDuplicate: false,
            symbolPackageUpdateResource: null,
            NullLogger.Instance);
    }

    private string GetProjectFolder(string projectName)
    {
        if (projectName.StartsWith("NAPS2.Sdk.Worker"))
        {
            return "NAPS2.Sdk.Worker";
        }
        return projectName;
    }

    private async Task UploadToGithub(string version)
    {
        var token = await File.ReadAllTextAsync(Path.Combine(Paths.Naps2UserFolder, "github"));
        var client = new GitHubClient(new ProductHeaderValue("cyanfish"),
            new InMemoryCredentialStore(new Credentials(token)));

        var commits = await client.Repository.Commit.GetAll(GITHUB_REPO_ID, new CommitRequest
        {
            Since = DateTimeOffset.Now - TimeSpan.FromDays(1)
        });
        var publishCommit = commits.SingleOrDefault(x => x.Commit.Message == $"PUBLISH ({version})") ??
                            throw new Exception($"Could not find publish commit for {version}. Maybe push to github?");
        Output.Verbose($"Found publish commit {publishCommit.Sha}");

        Output.Verbose($"Creating draft release");
        var release = await client.Repository.Release.Create(GITHUB_REPO_ID,
            new NewRelease($"v{version}")
            {
                Name = version,
                Body = GetChangelog(),
                Draft = true,
                Prerelease = version.Contains("b"),
                TargetCommitish = publishCommit.Sha
            });
        foreach (var package in TargetsHelper.EnumeratePackageTargets())
        {
            var path = package.PackagePath(version);
            if (File.Exists(path))
            {
                Output.Verbose($"Uploading asset {path}");
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                await client.Repository.Release.UploadAsset(release,
                    new ReleaseAssetUpload(Path.GetFileName(path), "application/octet-stream", stream,
                        null));
            }
        }
        Output.Info($"Created draft Github release: {release.HtmlUrl}");
    }

    private async Task UploadToSourceForge(string version)
    {
        var config = await File.ReadAllTextAsync(Path.Combine(Paths.Naps2UserFolder, "sourceforge"));
        var parts = config.Split("\n");
        var username = parts[0].Trim();
        var privateKeyFile = parts[1].Trim();
        var apiKey = parts[2].Trim();
        var connectionInfo = new ConnectionInfo("frs.sourceforge.net", username,
            new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(privateKeyFile)));
        using var client = new SftpClient(connectionInfo);
        await client.ConnectAsync(CancellationToken.None);
        Output.Verbose("Connected to SourceForge");
        try
        {
            client.CreateDirectory($"/home/frs/project/naps2/{version}");
        }
        catch (Exception)
        {
            // Maybe already created
        }
        Output.Verbose("Updating readme with changelog");
        await using var changelogStream = File.OpenRead(Path.Combine(Paths.SolutionRoot, "CHANGELOG.md"));
        client.UploadFile(changelogStream, "/home/frs/project/naps2/readme.txt");
        foreach (var package in TargetsHelper.EnumeratePackageTargets().Reverse())
        {
            var path = package.PackagePath(version);
            if (File.Exists(path))
            {
                Output.Verbose($"Uploading asset {path}");
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                client.UploadFile(stream, $"/home/frs/project/naps2/{version}/{Path.GetFileName(path)}");
            }
        }
        Output.Verbose($"Setting default downloads");
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        await httpClient.PutAsync(
            $"https://sourceforge.net/projects/naps2/files/{version}/naps2-{version}-win.exe",
            new FormUrlEncodedContent([
                new("default", "windows"),
                new("default", "android"),
                new("default", "bsd"),
                new("default", "solaris"),
                new("default", "others"),
                new("api_key", apiKey)
            ]));
        await httpClient.PutAsync(
            $"https://sourceforge.net/projects/naps2/files/{version}/naps2-{version}-mac-univ.pkg",
            new FormUrlEncodedContent([
                new("default", "mac"),
                new("api_key", apiKey)
            ]));
        await httpClient.PutAsync(
            $"https://sourceforge.net/projects/naps2/files/{version}/naps2-{version}-linux-x64.deb",
            new FormUrlEncodedContent([
                new("default", "linux"),
                new("api_key", apiKey)
            ]));
    }

    private string GetChangelog()
    {
        var lines = File.ReadAllLines(Path.Combine(Paths.SolutionRoot, "CHANGELOG.md"));
        var expected = $"Changes in {ProjectHelper.GetCurrentVersionName()}:";
        if (lines[0] == expected)
        {
            return string.Join('\n', lines.TakeWhile(x => x.Trim() != ""));
        }
        throw new Exception("Changelog needs updating (did not start with \"{expected}\")");
    }

    private void UploadToStaticSite(string version, string? packageType)
    {
        Cli.Run("ssh", $"user@downloads.naps2.com \"mkdir -p /var/www/html/{version}/\"");
        // Only upload packages that are needed (e.g. for Microsoft store, Apt repo)
        foreach (var package in TargetsHelper.EnumeratePackageTargets(packageType, null, false)
                     .Where(x => Path.GetExtension(x.PackagePath(version)) is ".msi" or ".deb"))
        {
            var path = package.PackagePath(version);
            var fileName = Path.GetFileName(path);
            Output.Verbose($"Uploading asset {path}");
            Cli.Run("scp", $"{path} user@downloads.naps2.com:/var/www/html/{version}/{fileName}");
        }
        Output.Info($"Uploaded files.");
    }

    private void UpdateAptMetadata()
    {
        var aptTemp = Path.Combine(Paths.SetupObj, "apt");
        if (Directory.Exists(aptTemp))
        {
            Directory.Delete(aptTemp, true);
        }
        Directory.CreateDirectory(aptTemp);

        Cli.Run("ssh", "user@downloads.naps2.com \"mkdir -p /home/user/apt-temp-packages/\"");
        Cli.Run("ssh", "user@downloads.naps2.com \"mkdir -p /home/user/apt-temp-release/\"");
        Cli.Run("ssh", "user@downloads.naps2.com \"cd /var/www/html/ ; apt-ftparchive packages . > /home/user/apt-temp-packages/Packages\" ; apt-ftparchive release /home/user/apt-temp-packages/ > /home/user/apt-temp-release/Release\"");
        Cli.Run("scp", $"user@downloads.naps2.com:/home/user/apt-temp-packages/Packages {Path.Combine(aptTemp, "Packages")}");
        Cli.Run("scp", $"user@downloads.naps2.com:/home/user/apt-temp-release/Release {Path.Combine(aptTemp, "Release")}");
        Cli.Run("gpg", $"--output {Path.Combine(aptTemp, "Release.gpg")} --sign {Path.Combine(aptTemp, "Release")}");
        Cli.Run("gpg", $"--output {Path.Combine(aptTemp, "InRelease")} --clearsign {Path.Combine(aptTemp, "Release")}");
        Cli.Run("scp", $"{Path.Combine(aptTemp, "Packages")} user@downloads.naps2.com:/var/www/html/Packages");
        Cli.Run("scp", $"{Path.Combine(aptTemp, "Release")} user@downloads.naps2.com:/var/www/html/Release");
        Cli.Run("scp", $"{Path.Combine(aptTemp, "Release.gpg")} user@downloads.naps2.com:/var/www/html/Release.gpg");
        Cli.Run("scp", $"{Path.Combine(aptTemp, "InRelease")} user@downloads.naps2.com:/var/www/html/InRelease");
    }
}