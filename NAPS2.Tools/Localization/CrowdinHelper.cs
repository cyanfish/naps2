using Crowdin.Api;

namespace NAPS2.Tools.Localization;

public static class CrowdinHelper
{
    public const int PROJECT_ID = 531762;
    public const int TEMPLATES_FILE_ID = 75;

    public static CrowdinApiClient GetClient()
    {
        var key = File.ReadAllText(Path.Combine(Paths.Naps2UserFolder, "crowdin"));
        var client = new CrowdinApiClient(new CrowdinCredentials { AccessToken = key });
        return client;
    }
}