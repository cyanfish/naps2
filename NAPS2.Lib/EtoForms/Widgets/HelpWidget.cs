using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Widgets;

public class HelpWidget
{
    private readonly Label _helpLabel = new();
    private readonly LayoutControl _helpButton;
    private readonly LayoutVisibility _helpVis = new(false);

    public HelpWidget()
    {
        _helpButton = C.IconButton("information_small", _helpVis.Toggle);
    }

    public string? Text
    {
        get => _helpLabel.Text;
        set => _helpLabel.Text = value ?? "";
    }
    
    public LayoutControl Button => _helpButton;
    
    public LayoutControl Label =>  _helpLabel.Visible(_helpVis).Padding(bottom: 6);

    public bool IsVisible
    {
        get => _helpVis.IsVisible;
        set => _helpVis.IsVisible = value;
    }
}
