using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public class SkipLayoutElement : LayoutElement
{
    public override void Materialize(LayoutContext context) => throw new NotSupportedException();
    public override void DoLayout(LayoutContext context, RectangleF bounds) => throw new NotSupportedException();
    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds) => SizeF.Empty;
}