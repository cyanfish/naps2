using System.Reflection;

namespace NAPS2.Util;

public class AssemblyHelper
{
    public static string GetFolder(Assembly? assembly)
    {
        return Path.GetDirectoryName(assembly?.Location) ?? LibFolder;
    }

    public static string LibFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

    public static string EntryFolder { get; } = GetFolder(Assembly.GetEntryAssembly());

    public static string EntryFile =>
        Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException("No entry file");

    private static string GetAssemblyAttributeValue<T>(Func<T, string> selector)
    {
        object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), false);
        if (attributes.Length == 0)
        {
            return "";
        }
        return selector((T) attributes[0]);
    }

    public static string Title
    {
        get
        {
            string title = GetAssemblyAttributeValue<AssemblyTitleAttribute>(x => x.Title);
            if (string.IsNullOrEmpty(title))
            {
                title = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
            return title;
        }
    }

    public static string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

    public static string Description => GetAssemblyAttributeValue<AssemblyDescriptionAttribute>(x => x.Description);

    public static string Product => GetAssemblyAttributeValue<AssemblyProductAttribute>(x => x.Product);

    public static string Copyright => GetAssemblyAttributeValue<AssemblyCopyrightAttribute>(x => x.Copyright);

    public static string Company => GetAssemblyAttributeValue<AssemblyCompanyAttribute>(x => x.Company);
}