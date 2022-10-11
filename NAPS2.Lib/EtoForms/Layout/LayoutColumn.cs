using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutColumn : LayoutElement
{
    private readonly LayoutElement[] _children;

    public LayoutColumn(LayoutElement[] children)
    {
        _children = children;
    }
        
    public LayoutColumn(LayoutColumn original, Padding? padding = null, Size? spacing = null, bool? xScale = null, bool? yScale = null)
    {
        _children = original._children;
        Padding = padding ?? original.Padding;
        Spacing = spacing ?? original.Spacing;
        XScale = xScale ?? original.XScale;
        YScale = yScale ?? original.YScale;
    }

    private Padding? Padding { get; }
    private Size? Spacing { get; }
    private bool? XScale { get; }
    private bool? YScale { get; }

    public override void AddTo(DynamicLayout layout)
    {
        layout.BeginVertical(padding: Padding, spacing: Spacing, xscale: XScale, yscale: YScale);
        foreach (var child in _children)
        {
            child.AddTo(layout);
        }
        layout.EndVertical();
    }
}