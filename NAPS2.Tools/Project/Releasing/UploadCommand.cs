using NAPS2.Tools.Project.Targets;
using Octokit;
using Octokit.Internal;
using FileMode = System.IO.FileMode;

namespace NAPS2.Tools.Project.Releasing;

public class UploadCommand : ICommand<UploadOptions>
{
    private const int GITHUB_REPO_ID = 39348622;

    public int Run(UploadOptions opts)
    {
        // Validate all package files
        foreach (var target in TargetsHelper.EnumeratePackageTargets())
        {
            var file = new FileInfo(target.PackagePath);
            if (!file.Exists)
            {
                throw new Exception($"Expected package to exist: {file.FullName}");
            }
            if (file.LastWriteTime < DateTime.Now - TimeSpan.FromHours(2))
            {
                throw new Exception($"Expected package to be recently modified: {file.FullName}");
            }
        }

        bool didSomething = false;
        if (opts.Target is "all" or "github")
        {
            Output.Info("Uploading binaries to Github");
            UploadToGithub().Wait();
            didSomething = true;
        }

        if (didSomething)
        {
            Output.OperationEnd("Binaries uploaded.");
        }
        else
        {
            Output.Info("No upload target.");
        }
        return 0;
    }

    private async Task UploadToGithub()
    {

        var version = ProjectHelper.GetCurrentVersionName();
        var token = await File.ReadAllTextAsync(Path.Combine(Paths.Naps2UserFolder, "github"));
        var client = new GitHubClient(new ProductHeaderValue("cyanfish"), new InMemoryCredentialStore(new Credentials(token)));

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
            var path = package.PackagePath;
            Output.Verbose($"Uploading asset {path}");
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            await client.Repository.Release.UploadAsset(release,
                new ReleaseAssetUpload(Path.GetFileName(path), "application/octet-stream", stream,
                    null));
        }
        Output.Info($"Created draft Github release: {release.HtmlUrl}");
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
}