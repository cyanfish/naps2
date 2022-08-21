using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms;

public static class L
{
    public static Padding DefaultPadding { get; set; } = new Padding(10);
    public static Size DefaultSpacing { get; set; } = new Size(6, 6);
        
    public static LayoutColumn Column(params LayoutElement[] children) =>
        new LayoutColumn(children);
        
    public static LayoutRow Row(params LayoutElement[] children) =>
        new LayoutRow(children);

    public static LayoutRoot Root(params LayoutElement[] children) =>
        new LayoutRoot(children);

    public static DynamicLayout Create(params LayoutElement[] children)
    {
        var layout = new DynamicLayout
        {
            DefaultSpacing = DefaultSpacing,
            Padding = DefaultPadding
        };
        foreach (var child in children)
        {
            child.AddTo(layout);
        }
        return layout;
    }
}