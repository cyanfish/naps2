using System.Reflection;

namespace NAPS2.Tools;

internal static class Paths
{
    private static string? _root;

    public static string SolutionRoot
    {
        get
        {
            if (_root == null)
            {
                _root = Assembly.GetExecutingAssembly().Location;
                while (!File.Exists(Path.Combine(_root, "NAPS2.sln")))
                {
                    _root = Path.GetDirectoryName(_root);
                    if (_root == null)
                    {
                        throw new Exception("Couldn't find NAPS2 folder");
                    }
                }
            }

            return _root;
        }
    }

    public static string Setup => Path.Combine(SolutionRoot, "NAPS2.Setup");
    
    public static string SetupObj => Path.Combine(Setup, "obj");
    
    public static string Publish => Path.Combine(Setup, "publish");

    public static string Naps2UserFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".naps2");

    public static string ConfigFile => Path.Combine(SolutionRoot, "NAPS2.Tools", "n2-config.json");

    public static string PoFolder => Path.Combine(SolutionRoot, "NAPS2.Lib", "Lang", "po");

    public static string TemplatesFile => Path.Combine(PoFolder, "templates.pot");
}