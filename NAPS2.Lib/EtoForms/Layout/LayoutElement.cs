using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public abstract class LayoutElement
{
    internal const bool DEBUG_LAYOUT = false;
    internal const bool DEBUG_SIZE = false;

    protected static List<LayoutElement> ExpandChildren(IEnumerable<LayoutElement> children)
    {
        return children.SelectMany(x => x is ExpandLayoutElement expand ? expand.Children : new[] { x })
            .Where(c => c is not SkipLayoutElement).ToList();
    }

    protected internal bool Scale { get; set; }
    protected internal LayoutAlignment Alignment { get; set; }
    protected internal LayoutVisibility? Visibility { get; set; }
    protected internal bool IsVisible => Visibility?.IsVisible ?? true;
    protected internal int? SpacingAfter { get; set; }
    protected internal int? Width { get; set; }
    protected internal int? Height { get; set; }

    public static implicit operator LayoutElement(Control control) =>
        new LayoutControl(control);

    /// <summary>
    /// Adds the contents of the layout element to the layout's container and window. This should be called before any
    /// call to GetPreferredSize or DoLayout.
    /// </summary>
    /// <param name="context">The layout context.</param>
    public abstract void Materialize(LayoutContext context);
    
    /// <summary>
    /// Performs the actual layout operation, updating the size and position of the layout element and its chidlren.
    /// </summary>
    /// <param name="context">The layout context.</param>
    /// <param name="bounds">The size and location of the layout element.</param>
    public abstract void DoLayout(LayoutContext context, RectangleF bounds);

    /// <summary>
    /// Gets the preferred size of the layout element. This is generally the minimum size required to fit the element's
    /// contents.
    /// </summary>
    /// <param name="context">The layout context.</param>
    /// <param name="parentBounds">The bounds constraining the size of the layout element.</param>
    /// <returns></returns>
    public SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        if (context.PreferredSizeCache.TryGetValue(this, out var size))
        {
            return size;
        }
        size = GetPreferredSizeCore(context, parentBounds);
        context.PreferredSizeCache[this] = size;
        return size;
    }

    protected abstract SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds);
}