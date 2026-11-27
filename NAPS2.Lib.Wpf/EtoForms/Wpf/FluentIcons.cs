using System.Windows.Media;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Glyphs from the Windows icon font: Segoe Fluent Icons on Windows 11, falling back to Segoe MDL2 Assets on Windows 10
/// (which has mostly the same glyphs at the same code points).
/// </summary>
internal static class FluentIcons
{
    public static FontFamily FontFamily { get; } = new("Segoe Fluent Icons, Segoe MDL2 Assets");

    public const string Add = "\uE710";
    public const string ArrowExpand = "\uE9E3";
    public const string Cancel = "\uE711";
    public const string ChevronDown = "\uE70D";
    public const string ChevronLeft = "\uE76B";
    public const string ChevronRight = "\uE76C";
    public const string Delete = "\uE74D";
    public const string DockRight = "\uE90D";
    public const string Edit = "\uE70F";
    public const string OpenInNewWindow = "\uE8A7";
    public const string Play = "\uE768";
    public const string ZoomIn = "\uE8A3";
    public const string ZoomOut = "\uE71F";
    public const string StatusCircleOuter = "\uF136";
    public const string StatusCircleErrorX = "\uF13D";
    public const string StatusCircleInfo = "\uF13F";
    public const string StatusCircleQuestionMark = "\uF142";
    public const string StatusTriangleOuter = "\uF139";
    public const string StatusTriangleExclamation = "\uF13B";
}
