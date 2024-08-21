using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public class BufferLayoutElement : LayoutElement
{
    public BufferLayoutElement(LayoutElement element, int left, int top, int right, int bottom)
    {
        Element = element;
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public LayoutElement Element { get; }
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        Element.DoLayout(context,
            new RectangleF(bounds.X + Left, bounds.Top + Top, bounds.Width - Left - Right,
                bounds.Height - Top - Bottom));
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        return Element.GetPreferredSize(context,
                   new RectangleF(parentBounds.X + Left, parentBounds.Top + Top, parentBounds.Width - Left - Right,
                       parentBounds.Height - Top - Bottom))
               + new SizeF(Left + Right, Top + Bottom);
    }
}