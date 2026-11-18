using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

// Ignore unreachable code for DEBUG_LAYOUT
#pragma warning disable CS0162
public class LayoutControl : LayoutElement
{
    private static readonly FieldInfo VisualParentField =
        typeof(Control).GetField("VisualParent_Key", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly Action<Control, EventArgs> TriggerPreLoad = GetTriggerMethod("TriggerPreLoad");
    private static readonly Action<Control, EventArgs> TriggerLoad = GetTriggerMethod("TriggerLoad");
    private static readonly Action<Control, EventArgs> TriggerLoadComplete = GetTriggerMethod("TriggerLoadComplete");

    private static Action<Control, EventArgs> GetTriggerMethod(string name) =>
        typeof(Control).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<Control, EventArgs>>();

    private bool _isAdded;
    private bool _isWindowSet;

    public LayoutControl(Control? control)
    {
        Control = control;
    }

    public LayoutControl(
        LayoutControl control,
        bool? scale = null, Padding? padding = null, int? spacingAfter = null,
        int? width = null, int? minWidth = null, int? maxWidth = null, int? naturalWidth = null,
        int? height = null, int? minHeight = null, int? maxHeight = null, int? naturalHeight = null,
        int? wrapDefaultWidth = null, LayoutAlignment? alignment = null, LayoutVisibility? visibility = null)
    {
        Control = control.Control;
        Scale = scale ?? control.Scale;
        Padding = padding ?? control.Padding;
        SpacingBefore = control.SpacingBefore;
        SpacingAfter = spacingAfter ?? control.SpacingAfter;
        Width = width ?? control.Width;
        MinWidth = minWidth ?? control.MinWidth;
        MaxWidth = maxWidth ?? control.MaxWidth;
        NaturalWidth = naturalWidth ?? control.NaturalWidth;
        Height = height ?? control.Height;
        MinHeight = minHeight ?? control.MinHeight;
        MaxHeight = maxHeight ?? control.MaxHeight;
        NaturalHeight = naturalHeight ?? control.NaturalHeight;
        WrapDefaultWidth = wrapDefaultWidth ?? control.WrapDefaultWidth;
        Alignment = alignment ?? control.Alignment;
        Visibility = visibility ?? control.Visibility;
    }

    public static implicit operator LayoutControl(Control control) =>
        new LayoutControl(control);

    internal Control? Control { get; }
    private Padding Padding { get; }
    private int? MinWidth { get; }
    private int? MaxWidth { get; }
    private int? NaturalWidth { get; }
    private int? MinHeight { get; }
    private int? MaxHeight { get; }
    private int? NaturalHeight { get; }
    private int? WrapDefaultWidth { get; }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        if (DEBUG_LAYOUT)
        {
            var text = Control is TextControl txt ? $"\"{txt.Text}\" " : "";
            Debug.WriteLine(
                $"{new string(' ', context.Depth)}{text}{Control?.GetType().Name ?? "ZeroSpace"} layout with bounds {bounds}");
        }
        bounds.Size = UpdateFixedDimensions(context, bounds.Size);
        UpdateVisibility(context);
        UpdateFrame(context, bounds);
    }

    private void UpdateFrame(LayoutContext context, RectangleF bounds)
    {
        if (Control == null) return;
        var location = new PointF(bounds.X + Padding.Left, bounds.Y + Padding.Top);
        var size = new SizeF(
            bounds.Width - Padding.Horizontal,
            bounds.Height - Padding.Vertical);
        size = SizeF.Max(SizeF.Empty, size);
        EtoPlatform.Current.SetFrame(
            context.Container,
            Control,
            Point.Round(location),
            Size.Round(size),
            context.InOverlay);
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        var size = SizeF.Empty;
        if (!IsVisible || context.ParentVisibility?.IsVisible == false)
        {
            return size;
        }
        if (Control != null)
        {
            UpdateVisibility(context);
            if (WrapDefaultWidth is { } wrapDefaultWidth)
            {
                size = GetWrappedSize(context, parentBounds, wrapDefaultWidth);
            }
            else
            {
                size = EtoPlatform.Current.GetPreferredSize(Control, parentBounds.Size);
            }
        }
        size = UpdateFixedDimensions(context, size);
        size = new SizeF(
            size.Width + Padding.Horizontal,
            size.Height + Padding.Vertical);
        if (DEBUG_SIZE)
        {
            var text = Control is TextControl txt ? $"\"{txt.Text}\" " : "";
            Debug.WriteLine(
                $"{new string(' ', context.Depth)}{text}{Control?.GetType().Name ?? "ZeroSpace"} size {size}");
        }
        return size;
    }

    private SizeF GetWrappedSize(LayoutContext context, RectangleF parentBounds, int wrapDefaultWidth)
    {
        if (Control == null) throw new InvalidOperationException();
        // Label wrapping is fairly complicated.
        if (!context.IsLayout)
        {
            // If we're not in a layout operation (i.e. we're getting a minimum or default form size), the
            // measured size should be based on the default width for wrapping.
            // This produces the label width (if small) or the default width (if long enough to wrap).
            return EtoPlatform.Current.GetWrappedSize(Control, wrapDefaultWidth);
        }
        if (context.IsCellLengthQuery)
        {
            // If we're measuring the size for a layout cell, we want the width to be the "real" width, and the
            // height to be the maximum height needed (i.e. the height when at the default wrapping width).
            // This ensures the cell height doesn't change as we changed the form width, which would otherwise
            // cause other controls to shift around vertically as we resize the form horizontally, which is not
            // usually what we want.
            return new SizeF(
                EtoPlatform.Current.GetWrappedSize(Control, (int) parentBounds.Width).Width,
                EtoPlatform.Current.GetWrappedSize(Control, Math.Min((int) parentBounds.Width, wrapDefaultWidth))
                    .Height);
        }
        // Now that we've handled the special cases, this measures the real dimensions of the label given
        // the parent bounds. In a layout cell, this ensures we align correctly (e.g. centered vertically).
        return EtoPlatform.Current.GetWrappedSize(Control, (int) parentBounds.Width);
    }

    private SizeF UpdateFixedDimensions(LayoutContext context, SizeF size)
    {
        if (MaxWidth != null)
        {
            size.Width = Math.Min(size.Width, MaxWidth.Value);
        }
        if (MinWidth != null)
        {
            size.Width = Math.Max(size.Width, MinWidth.Value);
        }
        if (Width != null)
        {
            size.Width = Width.Value;
        }
        if (!context.IsLayout && NaturalWidth != null)
        {
            size.Width = NaturalWidth.Value;
        }
        if (Height != null)
        {
            size.Height = Height.Value;
        }
        if (MaxHeight != null)
        {
            size.Height = Math.Min(size.Height, MaxHeight.Value);
        }
        if (MinHeight != null)
        {
            size.Height = Math.Max(size.Height, MinHeight.Value);
        }
        if (!context.IsLayout && NaturalHeight != null)
        {
            size.Height = NaturalHeight.Value;
        }
        // Fixed dimensions can't shrink a control below the platform's minimum size (e.g. Gtk always allocates widgets
        // at least their minimum size), so account for that or the control overflows its layout cell. This is per
        // dimension as e.g. a wrapping label's minimum height (at its minimum width) is much more than its real height.
        bool fixedWidth = (Width ?? MaxWidth ?? NaturalWidth) != null;
        bool fixedHeight = (Height ?? MaxHeight ?? NaturalHeight) != null;
        if (Control != null && (fixedWidth || fixedHeight))
        {
            var minSize = EtoPlatform.Current.GetMinimumSize(Control);
            if (fixedWidth)
            {
                size.Width = Math.Max(size.Width, minSize.Width);
            }
            if (fixedHeight)
            {
                size.Height = Math.Max(size.Height, minSize.Height);
            }
        }
        return size;
    }

    public override void Materialize(LayoutContext context)
    {
        if (Control == null) return;
        if (!_isAdded)
        {
            EtoPlatform.Current.AddToContainer(context.Container, Control, context.InOverlay);
            _isAdded = true;
            void OnVisibleChanged(object? sender, EventArgs args)
            {
                context.Invalidate();
                // TODO: This is suuuper hacky. But I don't have the time right now to figure out a better fix.
                // See https://github.com/cyanfish/naps2/issues/652
                if (EtoPlatform.Current.IsGtk)
                {
                    Task.Delay(100).ContinueWith(_ => context.Invalidate());
                }
            }
            if (Visibility != null)
            {
                Visibility.IsVisibleChanged += OnVisibleChanged;
            }
            if (context.ParentVisibility != null)
            {
                context.ParentVisibility.IsVisibleChanged += OnVisibleChanged;
            }
        }
        if (!_isWindowSet && context.Window != null)
        {
            Control.Properties.Set<Container>(VisualParentField.GetValue(null), context.Window);
            TriggerPreLoad(Control, EventArgs.Empty);
            TriggerLoad(Control, EventArgs.Empty);
            TriggerLoadComplete(Control, EventArgs.Empty);
            _isWindowSet = true;
        }
    }

    private void UpdateVisibility(LayoutContext context)
    {
        if (Control == null) return;
        if (Visibility != null || context.ParentVisibility != null)
        {
            Control.Visible = IsVisible && (context.ParentVisibility?.IsVisible ?? true);
        }
    }
}