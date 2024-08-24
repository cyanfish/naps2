using System.Reflection;

namespace NAPS2.Util;

internal class AssemblyHelper
{
    public const string COPYRIGHT_YEARS = "2009-2024";

    public static string GetFolder(Assembly? assembly)
    {
        return Path.GetDirectoryName(assembly?.Location) ?? LibFolder;
    }

    public static string LibFolder
    {
        get
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return string.IsNullOrEmpty(assemblyFolder) ? AppContext.BaseDirectory : assemblyFolder;
        }
    }

    public static string EntryFolder { get; }

    static AssemblyHelper()
    {
        // We can't use the assembly location, see
        // https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview?tabs=cli#api-incompatibility
        EntryFolder = AppContext.BaseDirectory;

        // If this is inside the "lib" subfolder, the base path needs to be the parent folder
        if (EntryFolder.EndsWith(@"\lib\"))
        {
            EntryFolder = EntryFolder.Substring(0, EntryFolder.Length - 4);
        }
    }

    public static string EntryFile =>
        Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException("No entry file");

    private static string GetAssemblyAttributeValue<T>(Func<T, string> selector)
    {
        object[] attributes = Assembly.GetEntryAssembly()!.GetCustomAttributes(typeof(T), false);
        if (attributes.Length == 0)
        {
            return "";
        }
        return selector((T) attributes[0]);
    }

    public static string Title => GetAssemblyAttributeValue<AssemblyTitleAttribute>(x => x.Title);

    public static Version Version => Assembly.GetEntryAssembly()!.GetName().Version!;

    public static string Description => GetAssemblyAttributeValue<AssemblyDescriptionAttribute>(x => x.Description);

    public static string Product => GetAssemblyAttributeValue<AssemblyProductAttribute>(x => x.Product);

    public static string Copyright => GetAssemblyAttributeValue<AssemblyCopyrightAttribute>(x => x.Copyright);

    public static string Company => GetAssemblyAttributeValue<AssemblyCompanyAttribute>(x => x.Company);
}