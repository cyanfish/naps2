using System.Windows;
using System.Windows.Controls;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Arranges children in uniform cells across a fixed number of columns, filling each column from top to bottom (so the
/// visual order matches the logical order when read down the columns).
/// </summary>
internal class ColumnPanel : Panel
{
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        nameof(Columns), typeof(int), typeof(ColumnPanel),
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure));

    private Size _cellSize;

    public int Columns
    {
        get => (int) GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    private int Rows => (InternalChildren.Count + Columns - 1) / Columns;

    protected override Size MeasureOverride(Size availableSize)
    {
        _cellSize = new Size();
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _cellSize.Width = Math.Max(_cellSize.Width, child.DesiredSize.Width);
            _cellSize.Height = Math.Max(_cellSize.Height, child.DesiredSize.Height);
        }
        int columns = Math.Min(Columns, InternalChildren.Count);
        return new Size(_cellSize.Width * columns, _cellSize.Height * Rows);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int rows = Rows;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            // ReSharper disable once PossibleLossOfFraction
            InternalChildren[i].Arrange(new Rect(new Point(i / rows * _cellSize.Width, i % rows * _cellSize.Height),
                _cellSize));
        }
        return finalSize;
    }
}
