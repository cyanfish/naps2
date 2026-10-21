using System.Windows;
using System.Windows.Controls;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// A container that measures and arranges each child at the frame given by our layout (see
/// WpfEtoPlatform.SetFrame). Unlike a Canvas, this doesn't need the child's Width/Height to be set, which Eto.Wpf also
/// manages.
/// </summary>
internal class FramePanel : Panel
{
    private static readonly DependencyProperty FrameProperty = DependencyProperty.RegisterAttached(
        "Frame", typeof(Rect?), typeof(FramePanel),
        new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.AffectsParentMeasure |
            FrameworkPropertyMetadataOptions.AffectsParentArrange));

    public static void SetFrame(UIElement element, Rect frame) => element.SetValue(FrameProperty, frame);

    private static Rect? GetFrame(UIElement element) => (Rect?) element.GetValue(FrameProperty);

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(GetFrame(child)?.Size ?? new Size(double.PositiveInfinity, double.PositiveInfinity));
        }
        return new Size();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            child.Arrange(GetFrame(child) ?? new Rect(child.DesiredSize));
        }
        return finalSize;
    }
}
