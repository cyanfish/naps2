using System.Net;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace NAPS2.Update;

public class UpdateChecker : IUpdateChecker
{
    private const string UPDATE_CHECK_ENDPOINT = "https://www.naps2.com/api/v1/update";
#if STANDALONE
        private const string UPDATE_FILE_EXT = "zip";
#elif INSTALLER_MSI
        private const string UPDATE_FILE_EXT = "msi";
#else
    private const string UPDATE_FILE_EXT = "exe";
#endif

    private readonly IOperationFactory _operationFactory;
    private readonly OperationProgress _operationProgress;

    public UpdateChecker(IOperationFactory operationFactory, OperationProgress operationProgress)
    {
        _operationFactory = operationFactory;
        _operationProgress = operationProgress;
    }

    public TimeSpan CheckInterval => TimeSpan.FromDays(7);

    public async Task<UpdateInfo> CheckForUpdates()
    {
        var json = await GetJson(UPDATE_CHECK_ENDPOINT);
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        foreach (var release in json.Value<JArray>("versions"))
        {
            var versionName = release.Value<string>("name");
            var version = ParseVersion(versionName);

            if (currentVersion >= version) continue;

            var gte = release["requires"].Value<string>("gte");
            var gteVersion = gte != null ? ParseVersion(gte) : null;
            if (gteVersion != null && currentVersion < gteVersion) continue;

            var updateFile = release["files"].Value<JToken>(UPDATE_FILE_EXT);
            if (updateFile == null) continue;

            var sha1 = updateFile.Value<string>("sha1");
            var sig = updateFile.Value<string>("sig");
            if (sha1 == null || sig == null) continue;

            return new UpdateInfo(versionName, updateFile.Value<string>("url"), Convert.FromBase64String(sha1), Convert.FromBase64String(sig));
        }
        return null;
    }

    public UpdateOperation StartUpdate(UpdateInfo update)
    {
        var op = _operationFactory.Create<UpdateOperation>();
        op.Start(update);
        _operationProgress.ShowModalProgress(op);
        return op;
    }

    private Version ParseVersion(string name)
    {
        return Version.Parse(name.Replace("b", "."));
    }

    private async Task<JObject> GetJson(string url)
    {
        return await Task.Run(() =>
        {
            using var client = new WebClient();
            var response = client.DownloadString(url);
            return JObject.Parse(response);
        });
    }
}