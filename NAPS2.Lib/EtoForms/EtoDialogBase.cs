using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public abstract class EtoDialogBase : Dialog, IFormBase
{
    private IFormFactory? _formFactory;

    protected EtoDialogBase(Naps2Config config)
    {
        Config = config;
        FormStateController = new FormStateController(this, config);
        Resizable = true;
        ShowInTaskbar = true;
        LayoutController.Bind(this);
        LayoutController.Invalidated += (_, _) => FormStateController.UpdateLayoutSize(LayoutController);
        if (EtoPlatform.Current.IsMac)
        {
            // Always have a basic menu on Mac, otherwise system keyboard shortcuts like Copy/Paste don't work
            Menu = new MenuBar();
        }
        EtoPlatform.Current.InitForm(this);
    }

    protected abstract void BuildLayout();

    protected override void OnPreLoad(EventArgs e)
    {
        FormStateController.LoadState();
        BuildLayout();
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