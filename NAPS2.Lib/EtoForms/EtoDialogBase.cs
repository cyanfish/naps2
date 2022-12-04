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
        LayoutController.ContentSet += (_, _) => FormStateController.UpdateLayoutSize(LayoutController);
    }

    public void ShowIcon()
    {
        // TODO: PR for Eto to make Dialog.ShowIcon a property
        EtoPlatform.Current.ShowIcon(this);
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