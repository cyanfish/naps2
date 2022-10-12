using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class ControlWithLayoutAttributes : LayoutElement
{
    private Size? _originalSize;

    public ControlWithLayoutAttributes(Control? control)
    {
        Control = control;
    }

    public ControlWithLayoutAttributes(
        ControlWithLayoutAttributes control, bool? center = null, bool? xScale = null, bool? yScale = null,
        bool? autoSize = null, Padding? padding = null, int? width = null, int? height = null)
    {
        Control = control.Control;
        Center = center ?? control.Center;
        XScale = xScale ?? control.XScale;
        YScale = yScale ?? control.YScale;
        AutoSize = autoSize ?? control.AutoSize;
        Padding = padding ?? control.Padding;
        Width = width ?? control.Width;
        Height = height ?? control.Height;
    }

    public static implicit operator ControlWithLayoutAttributes(Control control) =>
        new ControlWithLayoutAttributes(control);

    private Control? Control { get; }
    private bool Center { get; }
    private bool AutoSize { get; }
    private Padding Padding { get; }
    private int? Width { get; }
    private int? Height { get; }

    public override void AddTo(DynamicLayout layout)
    {
        if (Width != null)
        {
            Control!.Width = Width.Value;
        }
        if (Height != null)
        {
            Control!.Height = Height.Value;
        }
        if (AutoSize)
        {
            layout.AddAutoSized(Control, xscale: XScale, yscale: YScale, centered: Center, padding: Padding);
        }
        else if (Center)
        {
            layout.AddCentered(Control, xscale: XScale, yscale: YScale, padding: Padding);
        }
        else
        {
            // if (Padding != null)
            // {
            //     throw new InvalidOperationException(
            //         "Padding and Spacing aren't supported on controls except with AutoSize and/or Center.");
            // }
            layout.Add(Control, xscale: XScale, yscale: YScale);
        }
    }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        if (Control != null)
        {
            var location = new PointF(bounds.X + Padding.Left, bounds.Y + Padding.Right);
            var size = new SizeF(bounds.Width - Padding.Horizontal, bounds.Height - Padding.Vertical);
            if (context.IsFirstLayout)
            {
                EtoPlatform.Current.AddToContainer(context.Layout, Control);
            }
            EtoPlatform.Current.SetFrame(context.Layout, Control, Point.Round(location), Size.Round(size));
        }
    }

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        if (Control != null)
        {
            Control.Size = OriginalSize;
        }
        var size = Control?.GetPreferredSize(parentBounds.Size) ?? SizeF.Empty;
        return new SizeF(size.Width + Padding.Horizontal, size.Height + Padding.Vertical);
    }

    public Size OriginalSize
    {
        get
        {
            _originalSize ??= Control?.Size ?? Size.Empty;
            return _originalSize.Value;
        }
    }
}