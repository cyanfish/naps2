using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NAPS2.Tools;

public static class N2Config
{
    public static string ShareDir
    {
        get
        {
            var dir = EnsureConfigFile().Value<string>("share-dir");
            if (string.IsNullOrEmpty(dir))
            {
                throw new Exception("Expected share-dir to be specified in NAPS2.Tools/n2-config.json");
            }
            return dir;
        }
    }

    public static string? MacApplicationIdentity => EnsureConfigFile().Value<string>("mac-application-identity") ?? "";

    public static string? MacInstallerIdentity => EnsureConfigFile().Value<string>("mac-installer-identity") ?? "";

    public static string? MacNotarizationArgs => EnsureConfigFile().Value<string>("mac-notarization-args") ?? "";

    private static JToken EnsureConfigFile()
    {
        if (!File.Exists(Paths.ConfigFile))
        {
            File.WriteAllText(Paths.ConfigFile, "{\n    \"share-dir\": \"\",\n    \"mac-application-identity\": \"\",    \"mac-installer-identity\": \"\",\n    \"mac-notarization-args\": \"\"\n}\n");
        }
        using var file = File.OpenText(Paths.ConfigFile);
        using var reader = new JsonTextReader(file);
        return JToken.ReadFrom(reader);
    }
}