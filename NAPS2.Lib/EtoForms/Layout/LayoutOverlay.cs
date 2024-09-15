using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutOverlay : LayoutContainer
{
    private bool _createdContainer;
    private Control? _container;

    public LayoutOverlay(IEnumerable<LayoutElement> children)
        : base(children)
    {
    }

    public LayoutOverlay(LayoutOverlay original, bool? scale = null, int? spacingAfter = null)
        : base(original.Children)
    {
        Scale = scale ?? original.Scale;
        SpacingAfter = spacingAfter ?? original.SpacingAfter;
        Width = original.Width;
        Height = original.Height;
    }

    public override void Materialize(LayoutContext context)
    {
        if (!_createdContainer)
        {
            _container = EtoPlatform.Current.MaybeCreateOverlayContainer();
            if (_container != null)
            {
                EtoPlatform.Current.AddToContainer(context.Container, _container, context.InOverlay);
            }
            _createdContainer = true;
        }
        bool inOverlay = false;
        foreach (var child in Children)
        {
            child.Materialize(GetChildContext(context, inOverlay));
            inOverlay = true;
        }
    }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        bool inOverlay = false;
        if (_container != null)
        {
            EtoPlatform.Current.SetFrame(context.Container, _container, Point.Round(bounds.Location),
                Size.Round(bounds.Size), false);
            bounds = new RectangleF(0, 0, bounds.Width, bounds.Height);
        }
        foreach (var child in Children)
        {
            child.DoLayout(GetChildContext(context, inOverlay), bounds);
            inOverlay = true;
        }
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        bool inOverlay = false;
        SizeF size = SizeF.Empty;
        foreach (var child in Children)
        {
            var childSize = child.GetPreferredSize(GetChildContext(context, inOverlay), parentBounds);
            size = SizeF.Max(size, childSize);
            inOverlay = true;
        }
        if (Width != null)
        {
            size.Width = Width.Value;
        }
        if (Height != null)
        {
            size.Height = Height.Value;
        }
        return size;
    }

    private LayoutContext GetChildContext(LayoutContext context, bool inOverlay)
    {
        return context with
        {
            InOverlay = context.InOverlay || inOverlay,
            Container = EtoPlatform.Current.GetOverlayContainer(_container, inOverlay) ?? context.Container
        };
    }
}