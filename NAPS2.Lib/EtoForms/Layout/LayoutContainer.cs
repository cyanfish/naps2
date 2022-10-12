using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public abstract class LayoutContainer : LayoutElement
{
    protected internal bool Aligned { get; set; }

    protected internal LayoutElement[] Children { get; set; }

    protected internal abstract bool DoesChildScale(LayoutElement child);

    protected abstract float GetBreadth(SizeF size);
    protected abstract float GetLength(SizeF size);
    protected abstract SizeF GetSize(float length, float breadth);
}