using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public abstract class EtoFormBase : Form, IFormBase
{
    private IFormFactory? _formFactory;

    protected EtoFormBase(Naps2Config config)
    {
        Config = config;
        FormStateController = new FormStateController(this, config);
        Resizable = true;
        LayoutController.Bind(this);
        LayoutController.ContentSet += (_, _) => FormStateController.UpdateLayoutSize(LayoutController);
    }

    public IFormStateController FormStateController { get; }

    public LayoutController LayoutController { get; } = new();

    public IFormFactory FormFactory
    {
        get => _formFactory ?? throw new InvalidOperationException();
        set => _formFactory = value;
    }
        
    public Naps2Config Config { get; set; }
}