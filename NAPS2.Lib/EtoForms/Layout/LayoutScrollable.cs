using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

// Ignore unreachable code for DEBUG_LAYOUT
#pragma warning disable CS0162
/// <summary>
/// A layout element that displays its content in a vertically-scrollable area. The content is laid out at the width of
/// the scrollable area (minus the scrollbar, if needed) and at least the height of the scrollable area.
///
/// The preferred size has the content's width but not its height (by default 0), so that the scrollable area doesn't
/// constrain the minimum form height.
/// </summary>
public class LayoutScrollable : LayoutContainer
{
    private readonly LayoutElement _content;
    private bool _createdContainer;
    private Control? _scrollContainer;
    private Control? _contentContainer;

    public LayoutScrollable(LayoutElement content) : base([content])
    {
        _content = content;
    }

    public LayoutScrollable(LayoutScrollable original, bool? scale = null, int? spacingAfter = null,
        int? height = null, LayoutVisibility? visibility = null)
        : base(original.Children)
    {
        _content = original._content;
        Scale = scale ?? original.Scale;
        SpacingBefore = original.SpacingBefore;
        SpacingAfter = spacingAfter ?? original.SpacingAfter;
        Width = original.Width;
        Height = height ?? original.Height;
        Alignment = original.Alignment;
        Visibility = visibility ?? original.Visibility;
    }

    public override void Materialize(LayoutContext context)
    {
        if (!_createdContainer)
        {
            _scrollContainer = EtoPlatform.Current.CreateScrollContainer();
            _contentContainer = EtoPlatform.Current.GetScrollContentContainer(_scrollContainer);
            EtoPlatform.Current.AddToContainer(context.Container, _scrollContainer, context.InOverlay);
            _createdContainer = true;
        }
        _content.Materialize(GetChildContext(context));
    }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        if (DEBUG_LAYOUT)
        {
            Debug.WriteLine($"{new string(' ', context.Depth)}{GetType().Name} layout with bounds {bounds}");
        }
        if (_scrollContainer == null) throw new InvalidOperationException();
        _scrollContainer.Visible = IsVisible && (context.ParentVisibility?.IsVisible ?? true);
        EtoPlatform.Current.SetFrame(context.Container, _scrollContainer, Point.Round(bounds.Location),
            Size.Round(bounds.Size), context.InOverlay);

        var childContext = GetChildContext(context);
        var width = bounds.Width;
        var height = MeasureContentHeight(childContext, width);
        if (height > bounds.Height)
        {
            width -= GetScrollBarSpace(context, _scrollContainer);
            height = MeasureContentHeight(childContext, width);
        }
        var contentSize = Size.Ceiling(new SizeF(Math.Max(width, 0), Math.Max(height, bounds.Height)));
        EtoPlatform.Current.SetScrollContentSize(_scrollContainer, contentSize);
        _content.DoLayout(childContext, new RectangleF(0, 0, contentSize.Width, contentSize.Height));
    }

    private static float GetScrollBarSpace(LayoutContext context, Control scrollContainer)
    {
        // The width is 0 for overlay scrollbars (e.g. on Mac), which don't need any spacing
        var scrollBarWidth = EtoPlatform.Current.GetVerticalScrollBarWidth(scrollContainer);
        return scrollBarWidth > 0 ? scrollBarWidth + context.DefaultSpacing * context.Scale : 0;
    }

    private float MeasureContentHeight(LayoutContext childContext, float width)
    {
        // Don't use the cache as we might measure multiple times with different widths
        var measureContext = childContext with { UseCache = false };
        return _content.GetPreferredSize(measureContext, new RectangleF(0, 0, width, LayoutController.MAX_SIZE))
            .Height;
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        if (!IsVisible || context.ParentVisibility?.IsVisible == false)
        {
            return SizeF.Empty;
        }
        var childContext = GetChildContext(context) with { UseCache = false };
        var contentSize = _content.GetPreferredSize(childContext, parentBounds);
        var height = (Height ?? 0) * context.Scale;
        var width = contentSize.Width;
        if (contentSize.Height > height && _scrollContainer != null)
        {
            // Leave room for the scrollbar so the content doesn't need to shrink horizontally
            width += GetScrollBarSpace(context, _scrollContainer);
        }
        if (Width != null)
        {
            width = Width.Value * context.Scale;
        }
        return new SizeF(width, height);
    }

    private LayoutContext GetChildContext(LayoutContext context)
    {
        return context with
        {
            Container = _contentContainer ?? throw new InvalidOperationException(),
            InOverlay = false,
            Depth = context.Depth + 1,
            ParentVisibility = LayoutVisibility.Combine(context.ParentVisibility, Visibility)
        };
    }
}