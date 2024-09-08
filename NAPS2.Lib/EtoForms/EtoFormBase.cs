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
        LayoutController.Invalidated += (_, _) => FormStateController.UpdateLayoutSize(LayoutController);
        EtoPlatform.Current.InitForm(this);
    }

    protected virtual void BuildLayout()
    {
        FormStateController.LoadState();
    }

    protected override void OnPreLoad(EventArgs e)
    {
        BuildLayout();
        if (!FormStateController.Loaded)
        {
            throw new InvalidOperationException("Base BuildLayout method not called");
        }
        base.OnPreLoad(e);
    }

    public FormStateController FormStateController { get; }

    public LayoutController LayoutController { get; } = new();

    public IFormFactory FormFactory
    {
        get => _formFactory ?? throw new InvalidOperationException();
        set => _formFactory = value;
    }
        
    public Naps2Config Config { get; set; }

    public string IconName
    {
        set
        {
            EtoPlatform.Current.AttachDpiDependency(this,
                scale => Icon = EtoPlatform.Current.IconProvider.GetFormIcon(value, scale));
        }
    }
}