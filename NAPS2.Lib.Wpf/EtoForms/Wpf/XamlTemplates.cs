using System.Windows;
using System.Windows.Controls;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Templates defined in XAML resource dictionaries.
/// </summary>
internal static class XamlTemplates
{
    private static readonly ResourceDictionary Templates = new()
    {
        Source = new Uri("/NAPS2.Lib.Wpf;component/EtoForms/Wpf/Templates.xaml", UriKind.Relative)
    };

    /// <summary>
    /// A plain list item template for thumbnails, without the Fluent hover/selection visuals.
    /// </summary>
    public static ControlTemplate ThumbnailItemTemplate { get; } =
        (ControlTemplate) Templates["ThumbnailItemTemplate"]!;
}
