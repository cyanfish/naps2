using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public record LayoutContext(Control Layout)
{
    public int DefaultSpacing { get; init; }

    public List<float>? CellLengths { get; init; }

    public List<bool>? CellScaling { get; init; }

    public bool IsFirstLayout { get; set; }

    public bool IsNaturalSizeQuery { get; set; }

    public int Depth { get; set; }

    public Window? Window { get; set; }

    public bool InOverlay { get; set; }
}