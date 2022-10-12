using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public abstract class LayoutElement
{
    protected internal bool XScale { get; set; }
    protected internal bool YScale { get; set; }

    public abstract void AddTo(DynamicLayout layout);
        
    public static implicit operator LayoutElement(Control control) =>
        new ControlWithLayoutAttributes(control);
        
    public static implicit operator DynamicLayout(LayoutElement element)
    {
        return L.Create(element);
    }

    public abstract void DoLayout(LayoutContext context, RectangleF bounds);

    public abstract SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds);
}