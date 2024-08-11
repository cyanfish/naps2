using System.Text;

namespace NAPS2.Util;

public class UriHelper
{
    // From https://stackoverflow.com/a/35734486/2112909
    public static string FilePathToFileUrl(string filePath)
    {
        StringBuilder uri = new StringBuilder();
        foreach (char v in filePath)
        {
            if ((v >= 'a' && v <= 'z') || (v >= 'A' && v <= 'Z') || (v >= '0' && v <= '9') ||
                v == '+' || v == '/' || v == ':' || v == '.' || v == '-' || v == '_' || v == '~' ||
                v > '\xFF')
            {
                uri.Append(v);
            }
            else if (v == Path.DirectorySeparatorChar || v == Path.AltDirectorySeparatorChar)
            {
                uri.Append('/');
            }
            else
            {
                uri.Append(String.Format("%{0:X2}", (int)v));
            }
        }
        if (uri.Length >= 2 && uri[0] == '/' && uri[1] == '/') // UNC path
            uri.Insert(0, "file:");
        else
            uri.Insert(0, "file:///");
        return uri.ToString();
    }

    public static Uri FilePathToFileUri(string filePath) => new(FilePathToFileUrl(filePath));
}