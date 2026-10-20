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

    /// <summary>
    /// A minimal button template (content + rounded hover/pressed background using the Fluent "subtle" fills).
    /// </summary>
    public static ControlTemplate SubtleButtonTemplate { get; } =
        (ControlTemplate) Templates["SubtleButtonTemplate"]!;

    /// <summary>
    /// A focus rectangle drawn inside the control's bounds, for controls at the edge of a clipping container.
    /// </summary>
    public static Style InsetFocusVisualStyle { get; } = (Style) Templates["InsetFocusVisualStyle"]!;
}
