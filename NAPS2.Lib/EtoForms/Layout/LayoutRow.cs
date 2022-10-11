using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutRow : LayoutElement
{
    private readonly LayoutElement[] _children;

    public LayoutRow(LayoutElement[] children)
    {
        _children = children;
    }
        
    public LayoutRow(LayoutRow original, bool? yScale = null, bool? aligned = null)
    {
        _children = original._children;
        YScale = yScale ?? original.YScale;
        Aligned = aligned ?? original.Aligned;
    }

    private bool YScale { get; }
    private bool Aligned { get; }

    public override void AddTo(DynamicLayout layout)
    {
        if (!Aligned)
        {
            layout.BeginVertical();
        }
        layout.BeginHorizontal(yscale: YScale);
        foreach (var child in _children)
        {
            child.AddTo(layout);
        }
        layout.EndHorizontal();
        if (!Aligned)
        {
            layout.EndVertical();
        }
    }
}