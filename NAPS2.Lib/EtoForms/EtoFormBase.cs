using Eto.Forms;

namespace NAPS2.EtoForms;

public abstract class EtoFormBase : Form, IFormBase
{
    private IFormFactory? _formFactory;

    protected EtoFormBase(Naps2Config config)
    {
        Config = config;
        FormStateController = new FormStateController(this, config);
    }

    public IFormStateController FormStateController { get; }

    public IFormFactory FormFactory
    {
        get => _formFactory ?? throw new InvalidOperationException();
        set => _formFactory = value;
    }
        
    public Naps2Config Config { get; set; }
}