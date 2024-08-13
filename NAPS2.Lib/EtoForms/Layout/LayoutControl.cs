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
    private static readonly MethodInfo TriggerPreLoadMethod =
        typeof(Control).GetMethod("TriggerPreLoad", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo TriggerLoadMethod =
        typeof(Control).GetMethod("TriggerLoad", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo TriggerLoadCompleteMethod =
        typeof(Control).GetMethod("TriggerLoadComplete", BindingFlags.NonPublic | BindingFlags.Instance)!;

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
        if (Control != null)
        {
            var location = new PointF(bounds.X + Padding.Left, bounds.Y + Padding.Top);
            var size = new SizeF(bounds.Width - Padding.Horizontal, bounds.Height - Padding.Vertical);
            size = SizeF.Max(SizeF.Empty, size);
            EnsureIsAdded(context);
            EtoPlatform.Current.SetFrame(
                context.Layout,
                Control,
                Point.Round(location),
                Size.Round(size),
                context.InOverlay);
        }
    }

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        var size = SizeF.Empty;
        if (!IsVisible)
        {
            EnsureIsAdded(context);
            return size;
        }
        if (Control != null)
        {
            EnsureIsAdded(context);
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
        return new SizeF(size.Width + Padding.Horizontal, size.Height + Padding.Vertical);
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
                EtoPlatform.Current.GetWrappedSize(Control, Math.Min((int) parentBounds.Width, wrapDefaultWidth)).Height);
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
        return size;
    }

    private void EnsureIsAdded(LayoutContext context)
    {
        if (Control == null) return;
        if (Visibility != null || context.ParentVisibility != null)
        {
            Control.Visible = IsVisible && (context.ParentVisibility?.IsVisible ?? true);
        }
        if (!_isAdded)
        {
            EtoPlatform.Current.AddToContainer(context.Layout, Control, context.InOverlay);
            _isAdded = true;
            if (Visibility != null)
            {
                Visibility.IsVisibleChanged += (_, _) => context.Invalidate();
            }
            if (context.ParentVisibility != null)
            {
                context.ParentVisibility.IsVisibleChanged += (_, _) => context.Invalidate();
            }
        }
        if (!_isWindowSet && context.Window != null)
        {
            Control.Properties.Set<Container>(VisualParentField.GetValue(null), context.Window);
            TriggerPreLoadMethod.Invoke(Control, new object[] { EventArgs.Empty });
            TriggerLoadMethod.Invoke(Control, new object[] { EventArgs.Empty });
            TriggerLoadCompleteMethod.Invoke(Control, new object[] { EventArgs.Empty });
            _isWindowSet = true;
        }
    }
}