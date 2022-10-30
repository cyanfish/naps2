using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NAPS2.Tools;

public static class N2Config
{
    public static string ShareDir
    {
        get
        {
            if (!File.Exists(Paths.ConfigFile))
            {
                File.WriteAllText(Paths.ConfigFile, "{\n    \"share-dir\": \"\"\n}\n");
            }
            using var file = File.OpenText(Paths.ConfigFile);
            using var reader = new JsonTextReader(file);
            var obj = JToken.ReadFrom(reader);
            var dir = obj.Value<string>("share-dir");
            if (string.IsNullOrEmpty(dir))
            {
                throw new Exception("Expected share-dir to be specified in NAPS2.Tools/n2-config.json");
            }
            return dir;
        }
    }
}