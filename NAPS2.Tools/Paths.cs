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
}