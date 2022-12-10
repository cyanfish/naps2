using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public abstract class EtoDialogBase : Dialog, IFormBase
{
    private IFormFactory? _formFactory;
    
    protected EtoDialogBase(Naps2Config config)
    {
        EtoPlatform.Current.UpdateRtl(this);
        Config = config;
        FormStateController = new FormStateController(this, config);
        Resizable = true;
        ShowInTaskbar = true;
        LayoutController.Bind(this);
        LayoutController.Invalidated += (_, _) => FormStateController.UpdateLayoutSize(LayoutController);
    }

    protected abstract void BuildLayout();

    protected override void OnPreLoad(EventArgs e)
    {
        BuildLayout();
        base.OnPreLoad(e);
    }

    // TODO: PR for Eto to integrate this
    public new Icon Icon
    {
        get => base.Icon;
        set
        {
            base.Icon = value;
            EtoPlatform.Current.ShowIcon(this);
        }
    }

    public FormStateController FormStateController { get; }

    public LayoutController LayoutController { get; } = new();

    public IFormFactory FormFactory
    {
        get => _formFactory ?? throw new InvalidOperationException();
        set => _formFactory = value;
    }

    public Naps2Config Config { get; set; }
}