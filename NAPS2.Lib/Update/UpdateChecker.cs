using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace NAPS2.Update;

public class UpdateChecker : IUpdateChecker
{
    public static readonly TimeSpan CheckInterval = TimeSpan.FromDays(7);

    private const string UPDATE_CHECK_ENDPOINT = "https://www.naps2.com/api/v1/update";
#if ZIP
        private const string UPDATE_FILE_EXT = "zip";
#elif MSI
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

    public async Task<UpdateInfo?> CheckForUpdates()
    {
        var json = await GetJson(UPDATE_CHECK_ENDPOINT);
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        foreach (var release in json.Value<JArray>("versions")!)
        {
            var versionName = release.Value<string>("name")!;
            var version = ParseVersion(versionName);

            if (currentVersion >= version) continue;

            var gte = release["requires"]!.Value<string>("gte");
            var gteVersion = gte != null ? ParseVersion(gte) : null;
            if (gteVersion != null && currentVersion < gteVersion) continue;

            var updateFile = release["files"]!.Value<JToken>(UPDATE_FILE_EXT);
            if (updateFile == null) continue;

            var sha256 = updateFile.Value<string>("sha256");
            var sig = updateFile.Value<string>("sig");
            if (sha256 == null || sig == null) continue;

            return new UpdateInfo(versionName, updateFile.Value<string>("url")!, Convert.FromBase64String(sha256),
                Convert.FromBase64String(sig));
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
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return JObject.Parse(response);
    }
}