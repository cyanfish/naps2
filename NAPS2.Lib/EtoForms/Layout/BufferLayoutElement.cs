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
        Element.DoLayout(context, new RectangleF(
            bounds.X + Left * context.Scale,
            bounds.Y + Top * context.Scale,
            bounds.Width - (Left + Right) * context.Scale,
            bounds.Height - (Top + Bottom) * context.Scale));
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        return Element.GetPreferredSize(context,
                   new RectangleF(
                       parentBounds.X + Left * context.Scale,
                       parentBounds.Y + Top * context.Scale,
                       parentBounds.Width - (Left + Right) * context.Scale,
                       parentBounds.Height - (Top + Bottom) * context.Scale))
               + new SizeF(Left + Right, Top + Bottom) * context.Scale;
    }
}