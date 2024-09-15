using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public record LayoutContext(Control Layout)
{
    public int DefaultSpacing { get; init; }

    public int DefaultLabelSpacing { get; init; }

    public List<float>? CellLengths { get; init; }

    public List<bool>? CellScaling { get; init; }

    public bool IsLayout { get; init; }

    public bool IsNaturalSizeQuery { get; init; }

    public bool IsCellLengthQuery { get; set; }

    public int Depth { get; init; }

    public Window? Window { get; init; }

    public bool InOverlay { get; init; }

    public LayoutVisibility? ParentVisibility { get; init; }

    public required Action Invalidate { get; init; }

    public bool UseCache { get; init; } = true;
    
    public Dictionary<LayoutElement, Eto.Drawing.SizeF> PreferredSizeCache { get; } = new();

    public float Scale { get; init; }
}